using System.Text.RegularExpressions;

namespace EduCode.Infraestructura.Servicios;

internal sealed record HallazgoCodigo(
    string Codigo,
    string Descripcion,
    bool Critico);

internal static partial class AnalizadorCodigoJavaScriptBasico
{
    [GeneratedRegex(
        @"\b(?:for|while)\s*\([^)]*(?:<=\s*[A-Za-z_$][\w$\.]*\.length|[A-Za-z_$][\w$\.]*\.length\s*>=)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RecorridoLengthInclusivoRegex();

    [GeneratedRegex(
        @"\[\s*[A-Za-z_$][\w$\.]*\.length\s*\]",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AccesoLengthRegex();

    [GeneratedRegex(
        @"\bif\s*\([^)]*(?<![=!<>])=(?!=)[^)]*\)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AsignacionEnIfRegex();

    public static IReadOnlyCollection<HallazgoCodigo> Analizar(
        string lenguaje,
        string codigo)
    {
        if (!lenguaje.Contains(
                "javascript",
                StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        List<HallazgoCodigo> hallazgos = [];

        if (RecorridoLengthInclusivoRegex().IsMatch(codigo))
        {
            hallazgos.Add(new HallazgoCodigo(
                "JS_OFF_BY_ONE_LENGTH",
                "Se detectó un recorrido que admite que el índice alcance length. " +
                "Como el último índice válido es length - 1, debe revisarse el uso " +
                "de <= o de una comparación inclusiva equivalente.",
                true));
        }

        if (AccesoLengthRegex().IsMatch(codigo))
        {
            hallazgos.Add(new HallazgoCodigo(
                "JS_ACCESO_LENGTH",
                "Se accede directamente a la posición length, que se encuentra " +
                "fuera del rango válido de un string o array.",
                true));
        }

        if (AsignacionEnIfRegex().IsMatch(codigo))
        {
            hallazgos.Add(new HallazgoCodigo(
                "JS_ASIGNACION_CONDICIONAL",
                "Hay una asignación simple dentro de una condición. Revisá si " +
                "realmente se pretendía comparar valores.",
                true));
        }

        int aperturasLlaves = codigo.Count(c => c == '{');
        int cierresLlaves = codigo.Count(c => c == '}');

        if (aperturasLlaves != cierresLlaves)
        {
            hallazgos.Add(new HallazgoCodigo(
                "JS_LLAVES_DESEQUILIBRADAS",
                "La cantidad de llaves de apertura y cierre no coincide.",
                true));
        }

        int aperturasParentesis = codigo.Count(c => c == '(');
        int cierresParentesis = codigo.Count(c => c == ')');

        if (aperturasParentesis != cierresParentesis)
        {
            hallazgos.Add(new HallazgoCodigo(
                "JS_PARENTESIS_DESEQUILIBRADOS",
                "La cantidad de paréntesis de apertura y cierre no coincide.",
                true));
        }

        return hallazgos;
    }
}
