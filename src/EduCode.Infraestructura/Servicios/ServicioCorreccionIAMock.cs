using EduCode.Aplicacion.Interfaces;
using EduCode.DTOs;

namespace EduCode.Infraestructura.Servicios;

public sealed class ServicioCorreccionIAMock : IServicioCorreccionIA
{
    public Task<RespuestaCorreccionIADTO> CorregirAsync(
        SolicitudCorreccionIADTO solicitud,
        CancellationToken ct = default)
    {
        string codigo = solicitud.CodigoAlumno.Trim();
        bool tieneContenido = codigo.Length >= 15;
        bool tieneEstructura =
            codigo.Contains("function", StringComparison.OrdinalIgnoreCase) ||
            codigo.Contains("if", StringComparison.OrdinalIgnoreCase) ||
            codigo.Contains("for", StringComparison.OrdinalIgnoreCase) ||
            codigo.Contains("while", StringComparison.OrdinalIgnoreCase) ||
            codigo.Contains("class", StringComparison.OrdinalIgnoreCase);
        bool tieneRetorno = codigo.Contains(
            "return",
            StringComparison.OrdinalIgnoreCase);

        List<ResultadoCriterioIADTO> resultados = solicitud.Criterios
            .Select((criterio, indice) =>
            {
                int baseCumplimiento = tieneContenido ? 56 : 12;
                if (tieneEstructura) baseCumplimiento += 19;
                if (tieneRetorno) baseCumplimiento += 11;

                int variacionDeterminista =
                    (HashCode.Combine(codigo, criterio.Id, indice) & int.MaxValue) % 9;

                int porcentaje = Math.Clamp(
                    baseCumplimiento + variacionDeterminista,
                    0,
                    96);

                return new ResultadoCriterioIADTO
                {
                    CriterioId = criterio.Id,
                    PorcentajeCumplimiento = porcentaje,
                    Evidencia = tieneContenido
                        ? "La simulación detectó una propuesta de código relacionada con la consigna."
                        : "La respuesta contiene muy poco código para demostrar el criterio.",
                    Observacion =
                        "Esta evaluación fue generada por el simulador local y requiere validación con Gemini."
                };
            })
            .ToList();

        decimal promedio = resultados.Count == 0
            ? 0
            : resultados.Average(r => (decimal)r.PorcentajeCumplimiento);

        return Task.FromResult(new RespuestaCorreccionIADTO
        {
            Resultado = promedio >= 88
                ? "CORRECTO"
                : promedio > 0
                    ? "PARCIALMENTE_CORRECTO"
                    : "INCORRECTO",
            Resumen = tieneContenido
                ? "La solución presenta una estructura reconocible, evaluada en modo de simulación."
                : "La solución requiere mayor desarrollo antes de poder evaluarse.",
            AspectosCorrectos = tieneContenido
                ? ["Se presentó una propuesta de solución."]
                : [],
            AspectosAMejorar =
            [
                "Revisá cada condición solicitada por la consigna.",
                "Probá la solución con distintos valores de entrada."
            ],
            Pista =
                "Dividí la consigna en decisiones pequeñas y verificá una por vez.",
            RequiereRevisionDocente = true,
            ResultadosCriterios = resultados,
            Modelo = "simulador-local",
            VersionPrompt = "mock-2.0",
            TokensEntrada = 0,
            TokensSalida = 0
        });
    }
}
