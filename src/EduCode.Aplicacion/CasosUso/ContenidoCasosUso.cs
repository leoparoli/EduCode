using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using EduCode.Dominio.Excepciones;
using EduCode.DTOs;

namespace EduCode.Aplicacion.CasosUso;

public sealed class CUListarCursos
{
    private readonly IRepositorioCursos _cursos;

    public CUListarCursos(IRepositorioCursos cursos)
    {
        _cursos = cursos;
    }

    public async Task<IReadOnlyCollection<CursoListadoDTO>> EjecutarAsync(
        CancellationToken ct = default)
    {
        var cursos = await _cursos.ObtenerActivosConTemasAsync(ct);

        return cursos
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CursoListadoDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Codigo = c.Codigo,
                Descripcion = c.Descripcion,
                CantidadTemas = c.Temas.Count(t => t.Activo)
            })
            .ToList();
    }
}

public sealed class CUAltaTema
{
    private readonly IRepositorioTemas _temas;
    private readonly IRepositorioCursos _cursos;
    private readonly IUnidadTrabajo _uow;

    public CUAltaTema(IRepositorioTemas temas, IRepositorioCursos cursos, IUnidadTrabajo uow)
    {
        _temas = temas;
        _cursos = cursos;
        _uow = uow;
    }

    public async Task<int> EjecutarAsync(AltaTemaDTO dto, CancellationToken ct = default)
    {
        if (!await _cursos.ExisteAsync(dto.CursoId, ct))
            throw new EntidadNoEncontradaException("el curso", dto.CursoId);

        if (dto.TemaPrerequisitoId.HasValue &&
            !await _temas.ExisteAsync(dto.TemaPrerequisitoId.Value, ct))
            throw new EntidadNoEncontradaException(
                "el tema prerequisito", dto.TemaPrerequisitoId.Value);

        var tema = new Tema(
            dto.Nombre,
            dto.Descripcion,
            dto.Orden,
            dto.CursoId,
            dto.TemaPrerequisitoId);

        await _temas.AgregarAsync(tema, ct);
        await _uow.GuardarCambiosAsync(ct);
        return tema.Id;
    }
}

public sealed class CUAltaEjercicio
{
    private readonly IRepositorioEjercicios _ejercicios;
    private readonly IRepositorioTemas _temas;
    private readonly IServicioUsuarioActual _usuarioActual;
    private readonly IUnidadTrabajo _uow;

    public CUAltaEjercicio(IRepositorioEjercicios ejercicios, IRepositorioTemas temas,
        IServicioUsuarioActual usuarioActual, IUnidadTrabajo uow)
    {
        _ejercicios = ejercicios;
        _temas = temas;
        _usuarioActual = usuarioActual;
        _uow = uow;
    }

    public async Task<int> EjecutarAsync(AltaEjercicioDTO dto, CancellationToken ct = default)
    {
        if (!await _temas.ExisteAsync(dto.TemaId, ct))
            throw new EntidadNoEncontradaException("el tema", dto.TemaId);

        if (dto.Criterios.Count == 0)
            throw new ReglaNegocioException(
                "El ejercicio debe tener al menos un criterio de evaluación.");

        decimal sumaPesos = dto.Criterios.Sum(c => c.Peso);
        if (decimal.Round(sumaPesos, 2) != 100m)
            throw new ReglaNegocioException(
                $"La suma de los pesos de los criterios debe ser exactamente 100. Actualmente suma {sumaPesos:0.##}.");

        var ejercicio = new Ejercicio(
            dto.Titulo,
            dto.Consigna,
            dto.Tipo,
            dto.Dificultad,
            dto.PuntajeMaximo,
            dto.PuntajeMinimoAprobacion,
            dto.Orden,
            dto.Lenguaje,
            dto.CodigoInicial,
            dto.TemaId,
            _usuarioActual.ObtenerUsuarioId(),
            dto.EjercicioPrerequisitoId);

        ejercicio.ConfigurarAyudas(
            dto.CantidadIntentosRecomendada,
            dto.PermitePistas,
            dto.PermiteVerSolucion);
        ejercicio.ConfigurarVersionRubrica(dto.VersionRubrica);

        foreach (var criterio in dto.Criterios)
        {
            ejercicio.AgregarCriterio(new CriterioEvaluacion(
                0,
                criterio.Descripcion,
                criterio.Tipo,
                criterio.Peso,
                criterio.Obligatorio,
                criterio.Orden,
                criterio.PorcentajeMinimoObligatorio));
        }

        foreach (var pista in dto.Pistas)
        {
            ejercicio.AgregarPista(new PistaEjercicio(
                0,
                pista.Nivel,
                pista.Contenido,
                pista.IntentosMinimosRequeridos,
                pista.PenalizacionPuntaje));
        }

        foreach (var concepto in dto.ConceptosPermitidos)
        {
            ejercicio.AgregarConceptoPermitido(new ConceptoPermitido(
                0, concepto.Nombre, concepto.DescripcionOMotivo));
        }

        foreach (var concepto in dto.ConceptosNoPermitidos)
        {
            ejercicio.AgregarConceptoNoPermitido(new ConceptoNoPermitido(
                0, concepto.Nombre, concepto.DescripcionOMotivo));
        }

        foreach (var caso in dto.CasosPrueba)
        {
            ejercicio.AgregarCasoPrueba(new CasoPrueba(
                0,
                caso.Nombre,
                caso.Entrada,
                caso.SalidaEsperada,
                caso.Visibilidad,
                caso.Peso,
                caso.Orden));
        }

        await _ejercicios.AgregarAsync(ejercicio, ct);
        await _uow.GuardarCambiosAsync(ct);
        return ejercicio.Id;
    }
}

public sealed class CUListarTemasAlumno
{
    private readonly IRepositorioTemas _temas;
    private readonly IRepositorioProgresos _progresos;
    private readonly IServicioUsuarioActual _usuario;

    public CUListarTemasAlumno(IRepositorioTemas temas, IRepositorioProgresos progresos,
        IServicioUsuarioActual usuario)
    {
        _temas = temas;
        _progresos = progresos;
        _usuario = usuario;
    }

    public async Task<IReadOnlyCollection<TemaListadoDTO>> EjecutarAsync(
        int cursoId, CancellationToken ct = default)
    {
        var temas = await _temas.ObtenerPorCursoAsync(cursoId, ct);
        var progresos = await _progresos.ObtenerPorUsuarioAsync(
            _usuario.ObtenerUsuarioId(), ct);

        return temas
            .OrderBy(t => t.Orden)
            .Select(t =>
            {
                var progresosTema = progresos
                    .Where(p => p.Ejercicio.TemaId == t.Id)
                    .ToList();

                var total = t.Ejercicios.Count(e => e.Activo);
                var aprobados = progresosTema.Count(p =>
                    p.Estado is EstadoProgreso.Aprobado or EstadoProgreso.Completado);

                return new TemaListadoDTO
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    Orden = t.Orden,
                    CantidadEjercicios = total,
                    EjerciciosCompletados = aprobados,
                    PorcentajeProgreso = total == 0
                        ? 0
                        : Math.Round(aprobados * 100m / total, 2),
                    Bloqueado = false
                };
            })
            .ToList();
    }
}

public sealed class CUListarEjerciciosPorTema
{
    private readonly IRepositorioEjercicios _ejercicios;
    private readonly IRepositorioProgresos _progresos;
    private readonly IServicioUsuarioActual _usuario;

    public CUListarEjerciciosPorTema(IRepositorioEjercicios ejercicios,
        IRepositorioProgresos progresos, IServicioUsuarioActual usuario)
    {
        _ejercicios = ejercicios;
        _progresos = progresos;
        _usuario = usuario;
    }

    public async Task<IReadOnlyCollection<EjercicioListadoDTO>> EjecutarAsync(
        int temaId, CancellationToken ct = default)
    {
        var ejercicios = await _ejercicios.ObtenerPorTemaAsync(temaId, ct);
        var progresos = await _progresos.ObtenerPorUsuarioAsync(
            _usuario.ObtenerUsuarioId(), ct);

        return ejercicios
            .OrderBy(e => e.Orden)
            .Select(e =>
            {
                var progreso = progresos.FirstOrDefault(p => p.EjercicioId == e.Id);

                bool usuarioAcademico = !_usuario.ObtenerRol().Equals(
                    RolUsuario.Alumno.ToString(),
                    StringComparison.OrdinalIgnoreCase);
                var bloqueado = !usuarioAcademico && e.EjercicioPrerequisitoId.HasValue &&
                    !progresos.Any(p =>
                        p.EjercicioId == e.EjercicioPrerequisitoId &&
                        p.Estado is EstadoProgreso.Aprobado or EstadoProgreso.Completado);

                return new EjercicioListadoDTO
                {
                    Id = e.Id,
                    Titulo = e.Titulo,
                    Dificultad = e.Dificultad.ToString(),
                    PuntajeMaximo = e.PuntajeMaximo,
                    Orden = e.Orden,
                    Estado = progreso?.Estado.ToString() ??
                        (bloqueado
                            ? EstadoProgreso.Bloqueado.ToString()
                            : EstadoProgreso.Disponible.ToString()),
                    MejorPuntaje = progreso?.MejorPuntaje ?? 0,
                    CantidadIntentos = progreso?.CantidadIntentos ?? 0,
                    Bloqueado = bloqueado
                };
            })
            .ToList();
    }
}

public sealed class CUObtenerEjercicio
{
    private readonly IRepositorioEjercicios _ejercicios;
    private readonly IRepositorioIntentos _intentos;
    private readonly IRepositorioProgresos _progresos;
    private readonly IServicioUsuarioActual _usuario;

    public CUObtenerEjercicio(IRepositorioEjercicios ejercicios,
        IRepositorioIntentos intentos, IRepositorioProgresos progresos,
        IServicioUsuarioActual usuario)
    {
        _ejercicios = ejercicios;
        _intentos = intentos;
        _progresos = progresos;
        _usuario = usuario;
    }

    public async Task<EjercicioDetalleDTO> EjecutarAsync(
        int id, CancellationToken ct = default)
    {
        var ejercicio = await _ejercicios.ObtenerCompletoAsync(id, ct) ??
            throw new EntidadNoEncontradaException("el ejercicio", id);

        if (!ejercicio.Activo)
            throw new ReglaNegocioException("El ejercicio no está disponible.");

        var usuarioId = _usuario.ObtenerUsuarioId();

        bool usuarioAcademico = !_usuario.ObtenerRol().Equals(
            RolUsuario.Alumno.ToString(),
            StringComparison.OrdinalIgnoreCase);

        if (!usuarioAcademico && ejercicio.EjercicioPrerequisitoId.HasValue)
        {
            var previo = await _progresos.ObtenerAsync(
                usuarioId, ejercicio.EjercicioPrerequisitoId.Value, ct);

            if (previo?.Estado is not
                (EstadoProgreso.Aprobado or EstadoProgreso.Completado))
                throw new ReglaNegocioException(
                    "Debe aprobar el ejercicio anterior antes de continuar.");
        }

        var cantidad = await _intentos.ObtenerCantidadIntentosAsync(
            usuarioId, id, ct);
        var ejerciciosTema = await _ejercicios.ObtenerPorTemaAsync(
            ejercicio.TemaId,
            ct);
        var siguiente = ejerciciosTema
            .Where(e => e.Orden > ejercicio.Orden)
            .OrderBy(e => e.Orden)
            .FirstOrDefault();

        return new EjercicioDetalleDTO
        {
            Id = ejercicio.Id,
            TemaId = ejercicio.TemaId,
            TemaNombre = ejercicio.Tema?.Nombre ?? string.Empty,
            Titulo = ejercicio.Titulo,
            Consigna = ejercicio.Consigna,
            Tipo = ejercicio.Tipo.ToString(),
            Dificultad = ejercicio.Dificultad.ToString(),
            Lenguaje = ejercicio.Lenguaje,
            CodigoInicial = ejercicio.CodigoInicial,
            PuntajeMaximo = ejercicio.PuntajeMaximo,
            PuntajeMinimoAprobacion = ejercicio.PuntajeMinimoAprobacion,
            CantidadIntentosRealizados = cantidad,
            PermiteSolicitarPista = ejercicio.PermitePistas &&
                ejercicio.Pistas.Any(p =>
                    p.Activa && p.IntentosMinimosRequeridos <= cantidad),
            SiguienteEjercicioId = siguiente?.Id,
            SiguienteEjercicioTitulo = siguiente?.Titulo ?? string.Empty,
            HaySiguienteEjercicioEnTema = siguiente is not null,
            Criterios = ejercicio.CriteriosEvaluacion
                .OrderBy(c => c.Orden)
                .Select(c => new CriterioPublicoDTO
                {
                    Descripcion = c.Descripcion,
                    Peso = c.Peso,
                    Obligatorio = c.Obligatorio,
                    PorcentajeMinimoObligatorio =
                        c.PorcentajeMinimoObligatorio
                })
                .ToList()
        };
    }
}
