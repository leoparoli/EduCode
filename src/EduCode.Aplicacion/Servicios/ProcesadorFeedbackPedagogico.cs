using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using EduCode.DTOs;

namespace EduCode.Aplicacion.Servicios;

public sealed class ProcesadorFeedbackPedagogico
{
    private readonly IRepositorioIntentos _intentos;
    private readonly IServicioCorreccionIA _tutorIA;
    private readonly IServicioEvaluacionLocal _evaluadorLocal;
    private readonly MotorPuntajeEvaluacion _motorPuntaje;
    private readonly IUnidadTrabajo _uow;

    public ProcesadorFeedbackPedagogico(
        IRepositorioIntentos intentos,
        IServicioCorreccionIA tutorIA,
        IServicioEvaluacionLocal evaluadorLocal,
        MotorPuntajeEvaluacion motorPuntaje,
        IUnidadTrabajo uow)
    {
        _intentos = intentos;
        _tutorIA = tutorIA;
        _evaluadorLocal = evaluadorLocal;
        _motorPuntaje = motorPuntaje;
        _uow = uow;
    }

    public async Task ProcesarAsync(
        int intentoId,
        CancellationToken ct = default)
    {
        IntentoEjercicio? intento = await _intentos.ObtenerCompletoPorIdAsync(
            intentoId,
            ct);

        if (intento?.Correccion is null) return;

        SolicitudCorreccionIADTO solicitud =
            ConstructorSolicitudCorreccion.Crear(intento, intento.Ejercicio);

        ResultadoEvaluacionLocalDTO local = _evaluadorLocal.Evaluar(solicitud);
        ResultadoMotorPuntaje evaluacion = _motorPuntaje.Calcular(
            intento.Ejercicio,
            local.CorreccionBase.ResultadosCriterios);

        CargarContextoTecnico(solicitud, local, evaluacion);

        RespuestaCorreccionIADTO feedback = await _tutorIA.CorregirAsync(
            solicitud,
            ct);

        intento.Correccion.ActualizarFeedbackPedagogico(
            feedback.Resumen,
            feedback.Pista,
            feedback.RequiereRevisionDocente,
            feedback.Modelo,
            feedback.VersionPrompt);

        ProcesadorCorreccionIntento.AgregarItemsFeedback(
            intento.Correccion,
            feedback);

        await _uow.GuardarCambiosAsync(ct);
    }

    private static void CargarContextoTecnico(
        SolicitudCorreccionIADTO solicitud,
        ResultadoEvaluacionLocalDTO local,
        ResultadoMotorPuntaje evaluacion)
    {
        solicitud.PuntajeTecnicoExacto = evaluacion.PuntajeExacto;
        solicitud.AprobadoTecnico = evaluacion.Aprobado;
        solicitud.ResumenTecnico = local.CorreccionBase.Resumen;
        solicitud.MotivoNoAprobacionTecnico = evaluacion.MotivoNoAprobacion;
        solicitud.ResultadosCasosTecnicos = local.ResultadosCasosPrueba;
        solicitud.HallazgosTecnicos = local.HallazgosTecnicos;
        solicitud.ResultadosCriteriosTecnicos = evaluacion.Resultados
            .Select(resultado => new ResultadoCriterioTecnicoDTO
            {
                CriterioId = resultado.Criterio.Id,
                Descripcion = resultado.Criterio.Descripcion,
                PorcentajeCumplimiento =
                    resultado.PorcentajeCumplimiento,
                PesoMaximo = resultado.Criterio.Peso,
                PuntajeObtenido = resultado.PuntajeObtenido,
                Obligatorio = resultado.Criterio.Obligatorio,
                PorcentajeMinimoObligatorio =
                    resultado.Criterio.PorcentajeMinimoObligatorio,
                CumplimientoObligatorioAlcanzado =
                    resultado.CumplimientoObligatorioAlcanzado,
                Evidencia = resultado.Evidencia,
                Observacion = resultado.Observacion
            })
            .ToList();
    }
}
