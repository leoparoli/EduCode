using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using EduCode.DTOs;

namespace EduCode.Aplicacion.Servicios;

public static class ConstructorResultadoIntento
{
    public static ResultadoIntentoDTO Crear(
        IntentoEjercicio intento,
        Ejercicio ejercicio,
        ResultadoEvaluacionLocalDTO local,
        ResultadoMotorPuntaje evaluacion)
    {
        CorreccionIntento? correccion = intento.Correccion;
        bool feedbackDisponible = correccion is not null &&
            correccion.Origen == OrigenCorreccion.Mixta;

        List<string> correctos = feedbackDisponible
            ? correccion!.ItemsFeedback
                .Where(item => item.Tipo == TipoItemFeedback.AspectoCorrecto)
                .OrderBy(item => item.Orden)
                .Select(item => item.Descripcion)
                .ToList()
            : local.CorreccionBase.AspectosCorrectos;

        List<string> mejorar = feedbackDisponible
            ? correccion!.ItemsFeedback
                .Where(item => item.Tipo == TipoItemFeedback.AspectoAMejorar)
                .OrderBy(item => item.Orden)
                .Select(item => item.Descripcion)
                .ToList()
            : local.CorreccionBase.AspectosAMejorar;

        return new ResultadoIntentoDTO
        {
            IntentoId = intento.Id,
            NumeroIntento = intento.NumeroIntento,
            Estado = intento.Estado.ToString(),
            PuntajeExacto = evaluacion.PuntajeExacto,
            PuntajeMostrado = evaluacion.PuntajeMostrado,
            EjercicioAprobado = evaluacion.Aprobado,
            CumpleCriteriosObligatorios =
                evaluacion.CumpleCriteriosObligatorios,
            MotivoNoAprobacion = evaluacion.MotivoNoAprobacion,
            Resumen = feedbackDisponible
                ? correccion!.Resumen
                : local.CorreccionBase.Resumen,
            AspectosCorrectos = correctos,
            AspectosAMejorar = mejorar,
            Pista = feedbackDisponible
                ? correccion!.PistaSugerida
                : local.CorreccionBase.Pista,
            VersionRubrica = ejercicio.VersionRubrica,
            VersionMotorPuntaje = MotorPuntajeEvaluacion.Version,
            Criterios = evaluacion.Resultados
                .Select(resultado => new ResultadoCriterioDTO
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
                .ToList(),
            CorreccionPendiente = false,
            EsCorreccionProvisoria = false,
            PuedeReintentar = !feedbackDisponible,
            AfectaProgreso = true,
            MensajeCorreccion = feedbackDisponible
                ? "La explicación pedagógica personalizada ya está disponible."
                : "La nota técnica ya es definitiva. EduCode está generando una explicación pedagógica adicional en segundo plano.",
            FeedbackPersonalizadoPendiente = !feedbackDisponible,
            FeedbackPersonalizadoDisponible = feedbackDisponible,
            ProveedorFeedback = correccion?.ModeloIAUtilizado ??
                local.CorreccionBase.Modelo,
            CasosPruebaSuperados = local.CasosPruebaSuperados,
            TotalCasosPrueba = local.TotalCasosPrueba,
            ResultadosCasosPrueba = local.ResultadosCasosPrueba,
            HallazgosTecnicos = local.HallazgosTecnicos
        };
    }
}
