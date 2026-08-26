using EduCode.Aplicacion.Interfaces;
using EduCode.Aplicacion.Servicios;
using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using EduCode.Dominio.Excepciones;
using EduCode.DTOs;

namespace EduCode.Aplicacion.CasosUso;

public sealed class CUEnviarIntento
{
    private readonly IRepositorioEjercicios _ejercicios;
    private readonly IRepositorioIntentos _intentos;
    private readonly IRepositorioProgresos _progresos;
    private readonly IServicioUsuarioActual _usuario;
    private readonly IUnidadTrabajo _uow;
    private readonly ProcesadorCorreccionIntento _procesador;
    private readonly IServicioEvaluacionLocal _evaluadorLocal;
    private readonly MotorPuntajeEvaluacion _motor;

    public CUEnviarIntento(
        IRepositorioEjercicios ejercicios,
        IRepositorioIntentos intentos,
        IRepositorioProgresos progresos,
        IServicioUsuarioActual usuario,
        IUnidadTrabajo uow,
        ProcesadorCorreccionIntento procesador,
        IServicioEvaluacionLocal evaluadorLocal,
        MotorPuntajeEvaluacion motor)
    {
        _ejercicios = ejercicios;
        _intentos = intentos;
        _progresos = progresos;
        _usuario = usuario;
        _uow = uow;
        _procesador = procesador;
        _evaluadorLocal = evaluadorLocal;
        _motor = motor;
    }

    public async Task<ResultadoIntentoDTO> EjecutarAsync(
        EnviarIntentoDTO dto,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Codigo))
        {
            throw new ReglaNegocioException(
                "Debe ingresar una solución.");
        }

        Ejercicio ejercicio = await _ejercicios.ObtenerCompletoAsync(
            dto.EjercicioId,
            ct) ?? throw new EntidadNoEncontradaException(
                "el ejercicio",
                dto.EjercicioId);

        if (!ejercicio.Activo)
        {
            throw new ReglaNegocioException(
                "El ejercicio no está disponible.");
        }

        int usuarioId = _usuario.ObtenerUsuarioId();

        if (ejercicio.EjercicioPrerequisitoId.HasValue)
        {
            ProgresoEjercicio? previo = await _progresos.ObtenerAsync(
                usuarioId,
                ejercicio.EjercicioPrerequisitoId.Value,
                ct);

            if (previo?.Estado is not
                (EstadoProgreso.Aprobado or EstadoProgreso.Completado))
            {
                throw new ReglaNegocioException(
                    "El ejercicio está bloqueado.");
            }
        }

        IntentoEjercicio? intentoDuplicado =
            await _intentos.ObtenerIntentoRecienteDuplicadoAsync(
                usuarioId,
                ejercicio.Id,
                dto.Codigo,
                DateTime.UtcNow.AddSeconds(-8),
                ct);

        if (intentoDuplicado is not null)
        {
            return ConstruirResultadoDesdeIntentoExistente(
                intentoDuplicado,
                ejercicio);
        }

        int numero = await _intentos.ObtenerCantidadIntentosAsync(
            usuarioId,
            ejercicio.Id,
            ct) + 1;

        IntentoEjercicio intento = new(
            usuarioId,
            ejercicio.Id,
            numero,
            dto.Codigo,
            dto.TiempoResolucionSegundos);

        await _intentos.AgregarAsync(intento, ct);
        await _uow.GuardarCambiosAsync(ct);

        return await _procesador.ProcesarAsync(
            intento,
            ejercicio,
            ct);
    }

    private ResultadoIntentoDTO ConstruirResultadoDesdeIntentoExistente(
        IntentoEjercicio intento,
        Ejercicio ejercicio)
    {
        ResultadoEvaluacionLocalDTO local = _evaluadorLocal.Evaluar(
            ConstructorSolicitudCorreccion.Crear(intento, ejercicio));
        ResultadoMotorPuntaje evaluacion = _motor.Calcular(
            ejercicio,
            local.CorreccionBase.ResultadosCriterios);

        return ConstructorResultadoIntento.Crear(
            intento,
            ejercicio,
            local,
            evaluacion);
    }
}

public sealed class CUReintentarCorreccionIntento
{
    private readonly IRepositorioIntentos _intentos;
    private readonly IServicioUsuarioActual _usuario;
    private readonly IServicioEvaluacionLocal _evaluadorLocal;
    private readonly MotorPuntajeEvaluacion _motor;
    private readonly IColaFeedbackIA _colaFeedback;

    public CUReintentarCorreccionIntento(
        IRepositorioIntentos intentos,
        IServicioUsuarioActual usuario,
        IServicioEvaluacionLocal evaluadorLocal,
        MotorPuntajeEvaluacion motor,
        IColaFeedbackIA colaFeedback)
    {
        _intentos = intentos;
        _usuario = usuario;
        _evaluadorLocal = evaluadorLocal;
        _motor = motor;
        _colaFeedback = colaFeedback;
    }

    public async Task<ResultadoIntentoDTO> EjecutarAsync(
        int intentoId,
        CancellationToken ct = default)
    {
        IntentoEjercicio intento =
            await _intentos.ObtenerParaReintentoAsync(
                intentoId,
                _usuario.ObtenerUsuarioId(),
                ct) ?? throw new EntidadNoEncontradaException(
                    "el intento",
                    intentoId);

        await _colaFeedback.EncolarAsync(intento.Id, ct);
        return ConstruirResultado(intento);
    }

    private ResultadoIntentoDTO ConstruirResultado(IntentoEjercicio intento)
    {
        ResultadoEvaluacionLocalDTO local = _evaluadorLocal.Evaluar(
            ConstructorSolicitudCorreccion.Crear(intento, intento.Ejercicio));
        ResultadoMotorPuntaje evaluacion = _motor.Calcular(
            intento.Ejercicio,
            local.CorreccionBase.ResultadosCriterios);

        return ConstructorResultadoIntento.Crear(
            intento,
            intento.Ejercicio,
            local,
            evaluacion);
    }
}

public sealed class CUObtenerResultadoIntento
{
    private readonly IRepositorioIntentos _intentos;
    private readonly IServicioUsuarioActual _usuario;
    private readonly IServicioEvaluacionLocal _evaluadorLocal;
    private readonly MotorPuntajeEvaluacion _motor;

    public CUObtenerResultadoIntento(
        IRepositorioIntentos intentos,
        IServicioUsuarioActual usuario,
        IServicioEvaluacionLocal evaluadorLocal,
        MotorPuntajeEvaluacion motor)
    {
        _intentos = intentos;
        _usuario = usuario;
        _evaluadorLocal = evaluadorLocal;
        _motor = motor;
    }

    public async Task<ResultadoIntentoDTO> EjecutarAsync(
        int intentoId,
        CancellationToken ct = default)
    {
        IntentoEjercicio intento =
            await _intentos.ObtenerParaReintentoAsync(
                intentoId,
                _usuario.ObtenerUsuarioId(),
                ct) ?? throw new EntidadNoEncontradaException(
                    "el intento",
                    intentoId);

        ResultadoEvaluacionLocalDTO local = _evaluadorLocal.Evaluar(
            ConstructorSolicitudCorreccion.Crear(intento, intento.Ejercicio));
        ResultadoMotorPuntaje evaluacion = _motor.Calcular(
            intento.Ejercicio,
            local.CorreccionBase.ResultadosCriterios);

        return ConstructorResultadoIntento.Crear(
            intento,
            intento.Ejercicio,
            local,
            evaluacion);
    }
}

public sealed class CUObtenerHistorialIntentos
{
    private readonly IRepositorioIntentos _intentos;
    private readonly IServicioUsuarioActual _usuario;

    public CUObtenerHistorialIntentos(
        IRepositorioIntentos intentos,
        IServicioUsuarioActual usuario)
    {
        _intentos = intentos;
        _usuario = usuario;
    }

    public async Task<IReadOnlyCollection<IntentoHistorialDTO>> EjecutarAsync(
        int ejercicioId,
        CancellationToken ct = default)
    {
        var datos = await _intentos.ObtenerHistorialAsync(
            _usuario.ObtenerUsuarioId(),
            ejercicioId,
            ct);

        return datos
            .Select(i => new IntentoHistorialDTO
            {
                Id = i.Id,
                NumeroIntento = i.NumeroIntento,
                Fecha = i.FechaEnvio,
                Estado = i.Estado.ToString(),
                Puntaje = i.PuntajeObtenido,
                Codigo = i.CodigoEnviado,
                PuedeReintentar = i.Correccion is not null &&
                    i.Correccion.Origen == OrigenCorreccion.Automatica,
                EsCorreccionProvisoria = false,
                MensajeEstado = i.Correccion is not null &&
                    i.Correccion.Origen == OrigenCorreccion.Automatica
                    ? "La nota técnica es definitiva. La explicación personalizada de IA todavía está pendiente."
                    : "La explicación pedagógica personalizada está disponible."
            })
            .ToList();
    }
}

public sealed class CUSolicitarPista
{
    private readonly IRepositorioEjercicios _ejercicios;
    private readonly IRepositorioIntentos _intentos;
    private readonly IRepositorioPistas _pistas;
    private readonly IServicioUsuarioActual _usuario;

    public CUSolicitarPista(
        IRepositorioEjercicios ejercicios,
        IRepositorioIntentos intentos,
        IRepositorioPistas pistas,
        IServicioUsuarioActual usuario)
    {
        _ejercicios = ejercicios;
        _intentos = intentos;
        _pistas = pistas;
        _usuario = usuario;
    }

    public async Task<PistaDTO> EjecutarAsync(
        int ejercicioId,
        int nivelYaUsado = 0,
        CancellationToken ct = default)
    {
        var ejercicio = await _ejercicios.ObtenerPorIdAsync(ejercicioId, ct) ??
            throw new EntidadNoEncontradaException("el ejercicio", ejercicioId);

        if (!ejercicio.PermitePistas)
            throw new ReglaNegocioException(
                "Este ejercicio no permite solicitar pistas.");

        var cantidad = await _intentos.ObtenerCantidadIntentosAsync(
            _usuario.ObtenerUsuarioId(),
            ejercicioId,
            ct);

        var pista = await _pistas.ObtenerSiguienteDisponibleAsync(
            ejercicioId,
            cantidad,
            nivelYaUsado,
            ct) ?? throw new ReglaNegocioException(
                "No hay una nueva pista disponible todavía.");

        return new PistaDTO
        {
            Id = pista.Id,
            Nivel = pista.Nivel,
            Contenido = pista.Contenido,
            PenalizacionPuntaje = pista.PenalizacionPuntaje
        };
    }
}

public sealed class CUObtenerProgresoAlumno
{
    private readonly IRepositorioEjercicios _ejercicios;
    private readonly IRepositorioProgresos _progresos;
    private readonly IServicioUsuarioActual _usuario;

    public CUObtenerProgresoAlumno(
        IRepositorioEjercicios ejercicios,
        IRepositorioProgresos progresos,
        IServicioUsuarioActual usuario)
    {
        _ejercicios = ejercicios;
        _progresos = progresos;
        _usuario = usuario;
    }

    public async Task<ProgresoUsuarioDTO> EjecutarAsync(
        CancellationToken ct = default)
    {
        var ejercicios = await _ejercicios.ObtenerActivosAsync(ct);
        var progresos = await _progresos.ObtenerPorUsuarioAsync(
            _usuario.ObtenerUsuarioId(),
            ct);

        var aprobados = progresos.Count(p =>
            p.Estado is EstadoProgreso.Aprobado or EstadoProgreso.Completado);

        return new ProgresoUsuarioDTO
        {
            TotalEjercicios = ejercicios.Count,
            Aprobados = aprobados,
            EnProceso = progresos.Count(p =>
                p.Estado == EstadoProgreso.EnProceso),
            PorcentajeGeneral = ejercicios.Count == 0
                ? 0
                : Math.Round(aprobados * 100m / ejercicios.Count, 2),
            Ejercicios = progresos
                .Select(p => new ProgresoEjercicioDTO
                {
                    EjercicioId = p.EjercicioId,
                    Titulo = p.Ejercicio.Titulo,
                    Estado = p.Estado.ToString(),
                    CantidadIntentos = p.CantidadIntentos,
                    MejorPuntaje = p.MejorPuntaje,
                    UltimaActividad = p.FechaUltimaActividad
                })
                .ToList()
        };
    }
}
