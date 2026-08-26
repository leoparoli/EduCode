using EduCode.Aplicacion.CasosUso;
using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using EduCode.DTOs;

namespace EduCode.Aplicacion.Servicios;

public sealed class ProcesadorCorreccionIntento
{
    private readonly IRepositorioProgresos _progresos;
    private readonly IServicioEvaluacionLocal _evaluadorLocal;
    private readonly IColaFeedbackIA _colaFeedback;
    private readonly IUnidadTrabajo _uow;
    private readonly MotorPuntajeEvaluacion _motorPuntaje;
    private readonly CUGamificacion _gamificacion;

    public ProcesadorCorreccionIntento(
        IRepositorioProgresos progresos,
        IServicioEvaluacionLocal evaluadorLocal,
        IColaFeedbackIA colaFeedback,
        IUnidadTrabajo uow,
        MotorPuntajeEvaluacion motorPuntaje,
        CUGamificacion gamificacion)
    {
        _progresos = progresos;
        _evaluadorLocal = evaluadorLocal;
        _colaFeedback = colaFeedback;
        _uow = uow;
        _motorPuntaje = motorPuntaje;
        _gamificacion = gamificacion;
    }

    public async Task<ResultadoIntentoDTO> ProcesarAsync(
        IntentoEjercicio intento,
        Ejercicio ejercicio,
        CancellationToken ct = default)
    {
        intento.MarcarEnCorreccion();

        SolicitudCorreccionIADTO solicitud =
            ConstructorSolicitudCorreccion.Crear(intento, ejercicio);
        ResultadoEvaluacionLocalDTO local =
            _evaluadorLocal.Evaluar(solicitud);
        ResultadoMotorPuntaje evaluacion = _motorPuntaje.Calcular(
            ejercicio,
            local.CorreccionBase.ResultadosCriterios);

        intento.RegistrarResultado(
            evaluacion.PuntajeExacto,
            evaluacion.Aprobado,
            string.Equals(
                local.CorreccionBase.Resultado,
                "INCORRECTO",
                StringComparison.OrdinalIgnoreCase));

        CorreccionIntento correccion = CrearCorreccionLocal(
            intento,
            ejercicio,
            local.CorreccionBase,
            evaluacion);

        CargarDetalleCorreccion(
            correccion,
            local.CorreccionBase,
            evaluacion);

        await ActualizarProgresoAsync(
            intento.UsuarioId,
            ejercicio.Id,
            evaluacion.PuntajeExacto,
            evaluacion.Aprobado,
            ct);

        await _uow.GuardarCambiosAsync(ct);

        List<LogroDesbloqueadoDTO> nuevosLogros =
            await _gamificacion.EvaluarYAsignarLogrosAsync(
                intento.UsuarioId,
                ct);

        await _colaFeedback.EncolarAsync(intento.Id, ct);

        ResultadoIntentoDTO resultado = ConstructorResultadoIntento.Crear(
            intento,
            ejercicio,
            local,
            evaluacion);
        resultado.NuevosLogros = nuevosLogros;
        return resultado;
    }

    private static CorreccionIntento CrearCorreccionLocal(
        IntentoEjercicio intento,
        Ejercicio ejercicio,
        RespuestaCorreccionIADTO respuesta,
        ResultadoMotorPuntaje evaluacion)
    {
        CorreccionIntento correccion = new(
            intento.Id,
            OrigenCorreccion.Automatica,
            respuesta.Resumen,
            respuesta.Pista,
            evaluacion.PuntajeExacto,
            evaluacion.PuntajeMostrado,
            evaluacion.Aprobado,
            evaluacion.CumpleCriteriosObligatorios,
            respuesta.RequiereRevisionDocente,
            respuesta.Modelo,
            respuesta.VersionPrompt,
            ejercicio.VersionRubrica,
            MotorPuntajeEvaluacion.Version);

        intento.AsociarCorreccion(correccion);
        return correccion;
    }

    private static void CargarDetalleCorreccion(
        CorreccionIntento correccion,
        RespuestaCorreccionIADTO respuesta,
        ResultadoMotorPuntaje evaluacion)
    {
        foreach (ResultadoCriterioCalculado resultado in evaluacion.Resultados)
        {
            correccion.AgregarResultadoCriterio(new ResultadoCriterio(
                0,
                resultado.Criterio.Id,
                resultado.PorcentajeCumplimiento,
                resultado.Criterio.Peso,
                resultado.PuntajeObtenido,
                resultado.Evidencia,
                resultado.Observacion,
                resultado.CumplimientoObligatorioAlcanzado));
        }

        AgregarItemsFeedback(correccion, respuesta);
    }

    public static void AgregarItemsFeedback(
        CorreccionIntento correccion,
        RespuestaCorreccionIADTO respuesta)
    {
        int orden = 1;

        foreach (string aspecto in respuesta.AspectosCorrectos)
        {
            correccion.AgregarItemFeedback(new ItemFeedback(
                0,
                TipoItemFeedback.AspectoCorrecto,
                aspecto,
                orden++));
        }

        foreach (string aspecto in respuesta.AspectosAMejorar)
        {
            correccion.AgregarItemFeedback(new ItemFeedback(
                0,
                TipoItemFeedback.AspectoAMejorar,
                aspecto,
                orden++));
        }
    }

    private async Task ActualizarProgresoAsync(
        int usuarioId,
        int ejercicioId,
        decimal puntajeExacto,
        bool aprobado,
        CancellationToken ct)
    {
        ProgresoEjercicio? progreso = await _progresos.ObtenerAsync(
            usuarioId,
            ejercicioId,
            ct);

        if (progreso is null)
        {
            progreso = new ProgresoEjercicio(usuarioId, ejercicioId);
            await _progresos.AgregarAsync(progreso, ct);
        }

        progreso.RegistrarIntento(puntajeExacto, aprobado);
    }
}
