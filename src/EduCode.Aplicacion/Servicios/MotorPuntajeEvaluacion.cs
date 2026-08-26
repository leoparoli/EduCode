using EduCode.Dominio.Entidades;
using EduCode.Dominio.Excepciones;
using EduCode.DTOs;

namespace EduCode.Aplicacion.Servicios;

public sealed class MotorPuntajeEvaluacion
{
    public const string Version = "score-3.1-local";

    public ResultadoMotorPuntaje Calcular(
        Ejercicio ejercicio,
        IReadOnlyCollection<ResultadoCriterioIADTO> evaluaciones)
    {
        List<CriterioEvaluacion> criterios = ejercicio.CriteriosEvaluacion
            .OrderBy(c => c.Orden)
            .ToList();

        if (criterios.Count == 0)
            throw new ReglaNegocioException(
                "El ejercicio no tiene criterios de evaluación configurados.");

        decimal pesoTotal = criterios.Sum(c => c.Peso);

        if (decimal.Round(pesoTotal, 2) != 100m)
            throw new ReglaNegocioException(
                $"Los pesos de la rúbrica deben sumar 100. Actualmente suman {pesoTotal:0.##}.");

        Dictionary<int, ResultadoCriterioIADTO> porCriterio = evaluaciones
            .Where(e => criterios.Any(c => c.Id == e.CriterioId))
            .GroupBy(e => e.CriterioId)
            .ToDictionary(g => g.Key, g => g.First());

        List<ResultadoCriterioCalculado> resultados = [];

        foreach (CriterioEvaluacion criterio in criterios)
        {
            porCriterio.TryGetValue(criterio.Id, out ResultadoCriterioIADTO? evaluacion);

            int porcentaje = Math.Clamp(
                evaluacion?.PorcentajeCumplimiento ?? 0,
                0,
                100);

            decimal puntos = decimal.Round(
                criterio.Peso * porcentaje / 100m,
                2,
                MidpointRounding.AwayFromZero);

            bool obligatorioAlcanzado = !criterio.Obligatorio ||
                porcentaje >= criterio.PorcentajeMinimoObligatorio;

            resultados.Add(new ResultadoCriterioCalculado
            {
                Criterio = criterio,
                PorcentajeCumplimiento = porcentaje,
                PuntajeObtenido = puntos,
                Evidencia = evaluacion?.Evidencia?.Trim() ?? string.Empty,
                Observacion = evaluacion?.Observacion?.Trim() ??
                    "No se recibió una evaluación específica para este criterio.",
                CumplimientoObligatorioAlcanzado = obligatorioAlcanzado
            });
        }

        decimal puntajeExacto = decimal.Round(
            resultados.Sum(r => r.PuntajeObtenido),
            2,
            MidpointRounding.AwayFromZero);

        int puntajeMostrado = (int)Math.Round(
            puntajeExacto,
            MidpointRounding.AwayFromZero);

        bool cumpleObligatorios = resultados
            .Where(r => r.Criterio.Obligatorio)
            .All(r => r.CumplimientoObligatorioAlcanzado);

        bool aprobado =
            puntajeExacto >= ejercicio.PuntajeMinimoAprobacion &&
            cumpleObligatorios;

        string motivo = aprobado
            ? string.Empty
            : ConstruirMotivoNoAprobacion(
                ejercicio,
                puntajeExacto,
                resultados,
                cumpleObligatorios);

        return new ResultadoMotorPuntaje
        {
            PuntajeExacto = puntajeExacto,
            PuntajeMostrado = puntajeMostrado,
            Aprobado = aprobado,
            CumpleCriteriosObligatorios = cumpleObligatorios,
            MotivoNoAprobacion = motivo,
            Resultados = resultados
        };
    }

    private static string ConstruirMotivoNoAprobacion(
        Ejercicio ejercicio,
        decimal puntajeExacto,
        IReadOnlyCollection<ResultadoCriterioCalculado> resultados,
        bool cumpleObligatorios)
    {
        List<string> motivos = [];

        if (puntajeExacto < ejercicio.PuntajeMinimoAprobacion)
        {
            motivos.Add(
                $"El puntaje exacto {puntajeExacto:0.00} no alcanza el mínimo " +
                $"de {ejercicio.PuntajeMinimoAprobacion}.");
        }

        if (!cumpleObligatorios)
        {
            string criterios = string.Join(
                "; ",
                resultados
                    .Where(r => r.Criterio.Obligatorio &&
                                !r.CumplimientoObligatorioAlcanzado)
                    .Select(r =>
                        $"{r.Criterio.Descripcion} " +
                        $"({r.PorcentajeCumplimiento}% de " +
                        $"{r.Criterio.PorcentajeMinimoObligatorio}% requerido)"));

            motivos.Add(
                "No se alcanzó el mínimo de uno o más criterios obligatorios: " +
                criterios + ".");
        }

        return string.Join(" ", motivos);
    }
}

public sealed class ResultadoMotorPuntaje
{
    public decimal PuntajeExacto { get; init; }
    public int PuntajeMostrado { get; init; }
    public bool Aprobado { get; init; }
    public bool CumpleCriteriosObligatorios { get; init; }
    public string MotivoNoAprobacion { get; init; } = string.Empty;
    public List<ResultadoCriterioCalculado> Resultados { get; init; } = [];
}

public sealed class ResultadoCriterioCalculado
{
    public CriterioEvaluacion Criterio { get; init; } = null!;
    public int PorcentajeCumplimiento { get; init; }
    public decimal PuntajeObtenido { get; init; }
    public string Evidencia { get; init; } = string.Empty;
    public string Observacion { get; init; } = string.Empty;
    public bool CumplimientoObligatorioAlcanzado { get; init; }
}
