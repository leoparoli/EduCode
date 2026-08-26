using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using EduCode.Aplicacion.Interfaces;
using EduCode.DTOs;
using Esprima;
using Jint;

namespace EduCode.Infraestructura.Servicios;

public sealed class ServicioEvaluacionLocalJavaScript : IServicioEvaluacionLocal
{
    private static readonly TimeSpan TiempoMaximo = TimeSpan.FromSeconds(2);

    public ResultadoEvaluacionLocalDTO Evaluar(SolicitudCorreccionIADTO solicitud)
    {
        bool esJavaScript = solicitud.Lenguaje.Contains(
            "javascript",
            StringComparison.OrdinalIgnoreCase);

        if (!esJavaScript)
        {
            return CrearResultadoNoSoportado(solicitud);
        }

        List<HallazgoTecnicoDTO> hallazgos = [];
        bool sintaxisValida = ValidarSintaxis(solicitud.CodigoAlumno, hallazgos);

        foreach (HallazgoCodigo hallazgo in
                 AnalizadorCodigoJavaScriptBasico.Analizar(
                     solicitud.Lenguaje,
                     solicitud.CodigoAlumno))
        {
            hallazgos.Add(new HallazgoTecnicoDTO
            {
                Codigo = hallazgo.Codigo,
                Descripcion = hallazgo.Descripcion,
                Critico = hallazgo.Critico
            });
        }

        DetectarRestriccionesAcademicas(solicitud, hallazgos);
        DetectarSolucionDesconectadaDeParametros(
            solicitud.CodigoAlumno,
            hallazgos);

        List<ResultadoCasoPruebaDTO> casos = sintaxisValida
            ? EjecutarCasos(solicitud, hallazgos)
            : solicitud.CasosPrueba.Select(caso => new ResultadoCasoPruebaDTO
            {
                Nombre = caso.Nombre,
                Superado = false,
                EsOculto = caso.EsOculto,
                SalidaEsperada = caso.SalidaEsperada,
                Mensaje = "No se ejecutó porque el código contiene un error de sintaxis."
            }).ToList();

        int superados = casos.Count(caso => caso.Superado);
        int total = casos.Count;
        decimal proporcionCasos = total == 0
            ? 0m
            : casos.Sum(caso => caso.Superado ? PesoCaso(caso) : 0m) /
              casos.Sum(PesoCaso);

        bool tieneReturn = Regex.IsMatch(
            solicitud.CodigoAlumno,
            @"\breturn\b",
            RegexOptions.CultureInvariant);
        bool tieneEstructuraPrincipal = Regex.IsMatch(
            solicitud.CodigoAlumno,
            @"\b(function|class)\b",
            RegexOptions.CultureInvariant);
        bool codigoSinImplementacion = EsCodigoSinImplementacion(
            solicitud.CodigoAlumno);
        bool errorEjecucion = hallazgos.Any(h =>
            h.Codigo == "JS_ERROR_EJECUCION");
        int criticos = hallazgos.Count(h => h.Critico);
        int advertencias = hallazgos.Count - criticos;
        int calidad = CalcularCalidad(solicitud.CodigoAlumno, sintaxisValida);
        bool solucionMinimaValida =
            sintaxisValida &&
            total > 0 &&
            superados == total &&
            criticos == 0;

        List<ResultadoCriterioIADTO> evaluaciones = [];

        foreach (CriterioCorreccionIADTO criterio in solicitud.Criterios)
        {
            int porcentajeBase = solucionMinimaValida
                ? 100
                : criterio.Tipo.ToLowerInvariant() switch
            {
                "funcional" => CalcularFuncional(
                    proporcionCasos,
                    sintaxisValida,
                    tieneReturn,
                    criticos),
                "casoprueba" => RedondearPorcentaje(proporcionCasos * 100m),
                "sintactico" => CalcularSintactico(
                    sintaxisValida,
                    tieneReturn,
                    tieneEstructuraPrincipal,
                    criticos),
                "calidadcodigo" => calidad,
                "conceptual" or "restriccionacademica" =>
                    CalcularTecnico(
                        proporcionCasos,
                        sintaxisValida,
                        criticos,
                        advertencias),
                _ => CalcularTecnico(
                    proporcionCasos,
                    sintaxisValida,
                    criticos,
                    advertencias)
            };

            int porcentaje = AjustarPorEstadoGlobal(
                criterio.Tipo,
                porcentajeBase,
                sintaxisValida,
                codigoSinImplementacion,
                superados,
                total,
                errorEjecucion,
                criticos);

            evaluaciones.Add(new ResultadoCriterioIADTO
            {
                CriterioId = criterio.Id,
                PorcentajeCumplimiento = porcentaje,
                Evidencia = ConstruirEvidencia(
                    criterio.Tipo,
                    superados,
                    total,
                    sintaxisValida,
                    hallazgos),
                Observacion = ConstruirObservacion(
                    porcentaje,
                    criterio,
                    hallazgos)
            });
        }

        List<string> correctos = ConstruirAspectosCorrectos(
            sintaxisValida,
            superados,
            total,
            tieneReturn,
            calidad);
        List<string> mejorar = ConstruirAspectosAMejorar(
            hallazgos,
            superados,
            total,
            sintaxisValida);

        string pista = hallazgos.FirstOrDefault(h => h.Critico)?.Descripcion
            ?? (superados < total
                ? "Revisá los casos que no coincidieron y compará la salida obtenida con la esperada."
                : "La solución supera los casos configurados. Revisá si podés simplificarla sin perder claridad.");

        bool completamenteIncorrecto =
            !sintaxisValida ||
            codigoSinImplementacion ||
            (total > 0 && superados == 0 && criticos > 0);

        string resultado = superados == total && total > 0 && criticos == 0
            ? "CORRECTO"
            : completamenteIncorrecto
                ? "INCORRECTO"
                : "PARCIALMENTE_CORRECTO";

        return new ResultadoEvaluacionLocalDTO
        {
            CasosPruebaSuperados = superados,
            TotalCasosPrueba = total,
            ResultadosCasosPrueba = casos,
            HallazgosTecnicos = hallazgos,
            CorreccionBase = new RespuestaCorreccionIADTO
            {
                Resultado = resultado,
                Resumen = ConstruirResumenLocal(
                    sintaxisValida,
                    superados,
                    total,
                    criticos),
                AspectosCorrectos = correctos,
                AspectosAMejorar = mejorar,
                Pista = pista,
                RequiereRevisionDocente = false,
                ResultadosCriterios = evaluaciones,
                Modelo = "EduCode Local · Jint + Esprima",
                VersionPrompt = "local-evaluator-2.9",
                EsProvisoria = false
            }
        };
    }

    private static bool ValidarSintaxis(
        string codigo,
        ICollection<HallazgoTecnicoDTO> hallazgos)
    {
        try
        {
            JavaScriptParser parser = new();
            _ = parser.ParseScript(codigo);
            return true;
        }
        catch (Exception ex)
        {
            hallazgos.Add(new HallazgoTecnicoDTO
            {
                Codigo = "JS_SINTAXIS_INVALIDA",
                Descripcion = "El código no pudo interpretarse: " + ex.Message,
                Critico = true
            });
            return false;
        }
    }

    private static List<ResultadoCasoPruebaDTO> EjecutarCasos(
        SolicitudCorreccionIADTO solicitud,
        ICollection<HallazgoTecnicoDTO> hallazgos)
    {
        List<ResultadoCasoPruebaDTO> resultados = [];

        foreach (CasoPruebaCorreccionIADTO caso in solicitud.CasosPrueba)
        {
            try
            {
                Engine engine = new(options => options
                    .Strict()
                    .TimeoutInterval(TiempoMaximo)
                    .MaxStatements(10_000)
                    .LimitMemory(12_000_000));

                engine.Execute(solicitud.CodigoAlumno);
                string salidaObtenida = ObtenerJsonResultado(engine, caso.Entrada);
                string salidaEsperada = NormalizarSalidaEsperada(caso.SalidaEsperada);
                bool coincide = SonEquivalentes(salidaEsperada, salidaObtenida);

                resultados.Add(new ResultadoCasoPruebaDTO
                {
                    Nombre = caso.Nombre,
                    Superado = coincide,
                    EsOculto = caso.EsOculto,
                    SalidaEsperada = caso.EsOculto
                        ? "Caso oculto"
                        : caso.SalidaEsperada,
                    SalidaObtenida = caso.EsOculto
                        ? (coincide ? "Correcta" : "No coincide")
                        : FormatearJson(salidaObtenida),
                    Mensaje = coincide
                        ? "La salida coincide con la esperada."
                        : "La salida obtenida no coincide con la esperada."
                });
            }
            catch (Exception ex)
            {
                resultados.Add(new ResultadoCasoPruebaDTO
                {
                    Nombre = caso.Nombre,
                    Superado = false,
                    EsOculto = caso.EsOculto,
                    SalidaEsperada = caso.EsOculto
                        ? "Caso oculto"
                        : caso.SalidaEsperada,
                    SalidaObtenida = "Error de ejecución",
                    Mensaje = "La ejecución falló: " + Limitar(ex.Message, 240)
                });

                if (!hallazgos.Any(h => h.Codigo == "JS_ERROR_EJECUCION"))
                {
                    hallazgos.Add(new HallazgoTecnicoDTO
                    {
                        Codigo = "JS_ERROR_EJECUCION",
                        Descripcion = "Al menos un caso de prueba produjo un error durante la ejecución.",
                        Critico = true
                    });
                }
            }
        }

        return resultados;
    }

    private static string ObtenerJsonResultado(Engine engine, string entrada)
    {
        string expresionEvaluable = ConstruirExpresionEvaluable(entrada);
        var valor = engine.Evaluate(
            $"JSON.stringify({expresionEvaluable})");

        if (valor.IsString())
        {
            return valor.AsString();
        }

        // JSON.stringify devuelve undefined para algunos valores especiales.
        // En ese caso se evalúa directamente y se normaliza desde .NET.
        var directo = engine.Evaluate(expresionEvaluable);
        return JsonSerializer.Serialize(directo.ToObject());
    }

    private static string ConstruirExpresionEvaluable(string entrada)
    {
        string codigo = entrada.Trim().TrimEnd(';').Trim();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            return "undefined";
        }

        int ultimoSeparador = codigo.LastIndexOf(';');

        if (ultimoSeparador < 0)
        {
            return $"({codigo})";
        }

        string instrucciones = codigo[..(ultimoSeparador + 1)];
        string expresionFinal = codigo[(ultimoSeparador + 1)..].Trim();

        if (string.IsNullOrWhiteSpace(expresionFinal))
        {
            return $"(() => {{ {instrucciones} return undefined; }})()";
        }

        return
            $"(() => {{ {instrucciones} return ({expresionFinal}); }})()";
    }

    private static string NormalizarSalidaEsperada(string esperada)
    {
        string valor = esperada.Trim();

        if (string.IsNullOrEmpty(valor))
        {
            return JsonSerializer.Serialize(string.Empty);
        }

        if (valor.StartsWith('{') || valor.StartsWith('['))
        {
            try
            {
                Engine engine = new(options => options
                    .Strict()
                    .TimeoutInterval(TimeSpan.FromMilliseconds(500))
                    .MaxStatements(500));
                var json = engine.Evaluate($"JSON.stringify(({valor}))");
                if (json.IsString()) return json.AsString();
            }
            catch
            {
                // Se continúa con las alternativas de normalización.
            }
        }

        if (decimal.TryParse(
                valor,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out decimal numero))
        {
            return numero.ToString(CultureInfo.InvariantCulture);
        }

        if (bool.TryParse(valor, out bool booleano))
        {
            return booleano ? "true" : "false";
        }

        if (string.Equals(valor, "null", StringComparison.OrdinalIgnoreCase))
        {
            return "null";
        }

        return JsonSerializer.Serialize(valor);
    }

    private static bool SonEquivalentes(string esperado, string obtenido)
    {
        try
        {
            using JsonDocument esperadoDoc = JsonDocument.Parse(esperado);
            using JsonDocument obtenidoDoc = JsonDocument.Parse(obtenido);
            return CompararElementos(esperadoDoc.RootElement, obtenidoDoc.RootElement);
        }
        catch (JsonException)
        {
            return string.Equals(
                esperado.Trim(),
                obtenido.Trim(),
                StringComparison.Ordinal);
        }
    }

    private static bool CompararElementos(JsonElement esperado, JsonElement obtenido)
    {
        if (esperado.ValueKind == JsonValueKind.Number &&
            obtenido.ValueKind == JsonValueKind.Number)
        {
            double diferencia = Math.Abs(
                esperado.GetDouble() - obtenido.GetDouble());
            return diferencia <= 0.000001d;
        }

        if (esperado.ValueKind != obtenido.ValueKind) return false;

        return esperado.ValueKind switch
        {
            JsonValueKind.Object => CompararObjetos(esperado, obtenido),
            JsonValueKind.Array => CompararArrays(esperado, obtenido),
            JsonValueKind.String => esperado.GetString() == obtenido.GetString(),
            JsonValueKind.True or JsonValueKind.False =>
                esperado.GetBoolean() == obtenido.GetBoolean(),
            JsonValueKind.Null => true,
            _ => esperado.GetRawText() == obtenido.GetRawText()
        };
    }

    private static bool CompararObjetos(JsonElement esperado, JsonElement obtenido)
    {
        Dictionary<string, JsonElement> propiedadesObtenidas = obtenido
            .EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value);

        List<JsonProperty> propiedadesEsperadas = esperado
            .EnumerateObject()
            .ToList();

        return propiedadesEsperadas.Count == propiedadesObtenidas.Count &&
               propiedadesEsperadas.All(p =>
                   propiedadesObtenidas.TryGetValue(p.Name, out JsonElement valor) &&
                   CompararElementos(p.Value, valor));
    }

    private static bool CompararArrays(JsonElement esperado, JsonElement obtenido)
    {
        JsonElement.ArrayEnumerator esperadoItems = esperado.EnumerateArray();
        JsonElement.ArrayEnumerator obtenidoItems = obtenido.EnumerateArray();
        List<JsonElement> a = esperadoItems.ToList();
        List<JsonElement> b = obtenidoItems.ToList();

        return a.Count == b.Count &&
               a.Zip(b).All(par => CompararElementos(par.First, par.Second));
    }

    private static int AjustarPorEstadoGlobal(
        string tipoCriterio,
        int porcentajeBase,
        bool sintaxisValida,
        bool codigoSinImplementacion,
        int casosSuperados,
        int totalCasos,
        bool errorEjecucion,
        int hallazgosCriticos)
    {
        string tipo = tipoCriterio.ToLowerInvariant();

        if (!sintaxisValida)
        {
            return tipo == "calidadcodigo"
                ? Math.Min(porcentajeBase, 5)
                : 0;
        }

        if (totalCasos > 0 && casosSuperados == 0 && codigoSinImplementacion)
        {
            int tope = tipo switch
            {
                "funcional" or "casoprueba" => 0,
                "conceptual" or "restriccionacademica" => 8,
                "sintactico" => 55,
                "calidadcodigo" => 12,
                _ => 8
            };

            return Math.Min(porcentajeBase, tope);
        }

        if (totalCasos > 0 && casosSuperados == 0)
        {
            bool fallaCritica = errorEjecucion || hallazgosCriticos > 0;
            int tope = (tipo, fallaCritica) switch
            {
                ("funcional", true) => 8,
                ("funcional", false) => 22,
                ("casoprueba", _) => 0,
                ("conceptual", true) or ("restriccionacademica", true) => 18,
                ("conceptual", false) or ("restriccionacademica", false) => 35,
                ("sintactico", true) => 65,
                ("sintactico", false) => 92,
                ("calidadcodigo", true) => 45,
                ("calidadcodigo", false) => 70,
                _ => fallaCritica ? 18 : 35
            };

            return Math.Min(porcentajeBase, tope);
        }

        return porcentajeBase;
    }

    private static bool EsCodigoSinImplementacion(string codigo)
    {
        string sinComentarios = Regex.Replace(
            codigo,
            @"//.*?$|/\*.*?\*/",
            string.Empty,
            RegexOptions.Multiline | RegexOptions.Singleline);

        Match funcion = Regex.Match(
            sinComentarios,
            @"\bfunction\s+[A-Za-z_$][\w$]*\s*\([^)]*\)\s*\{(?<cuerpo>[\s\S]*)\}\s*$",
            RegexOptions.CultureInvariant);

        if (!funcion.Success)
        {
            return string.IsNullOrWhiteSpace(sinComentarios);
        }

        string cuerpo = funcion.Groups["cuerpo"].Value.Trim();
        if (string.IsNullOrWhiteSpace(cuerpo))
        {
            return true;
        }

        return !Regex.IsMatch(
            cuerpo,
            @"\b(return|if|else|for|while|switch|class|new|throw)\b|(?:\+\+|--|=>)|(?<![=!<>])=(?!=)",
            RegexOptions.CultureInvariant);
    }

    private static void DetectarSolucionDesconectadaDeParametros(
        string codigo,
        ICollection<HallazgoTecnicoDTO> hallazgos)
    {
        string sinComentarios = Regex.Replace(
            codigo,
            @"//.*?$|/\*.*?\*/",
            string.Empty,
            RegexOptions.Multiline | RegexOptions.Singleline);

        Match funcion = Regex.Match(
            sinComentarios,
            @"\bfunction\s+[A-Za-z_$][\w$]*\s*\((?<parametros>[^)]*)\)\s*\{(?<cuerpo>[\s\S]*)\}\s*$",
            RegexOptions.CultureInvariant);

        if (!funcion.Success)
        {
            return;
        }

        string[] parametros = funcion.Groups["parametros"].Value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => p.Split('=')[0].Trim())
            .Where(p => Regex.IsMatch(p, @"^[A-Za-z_$][\w$]*$"))
            .ToArray();

        if (parametros.Length == 0)
        {
            return;
        }

        string cuerpo = funcion.Groups["cuerpo"].Value;
        bool usaAlgunParametro = parametros.Any(parametro =>
            Regex.IsMatch(
                cuerpo,
                $@"\b{Regex.Escape(parametro)}\b",
                RegexOptions.CultureInvariant));

        if (!usaAlgunParametro)
        {
            hallazgos.Add(new HallazgoTecnicoDTO
            {
                Codigo = "JS_PARAMETROS_IGNORADOS",
                Descripcion =
                    "La función no utiliza ninguno de los parámetros recibidos. " +
                    "La solución parece devolver un resultado fijo o desconectado de la entrada.",
                Critico = true
            });
        }
    }

    private static void DetectarRestriccionesAcademicas(
        SolicitudCorreccionIADTO solicitud,
        ICollection<HallazgoTecnicoDTO> hallazgos)
    {
        string codigo = Regex.Replace(
            solicitud.CodigoAlumno,
            @"//.*?$|/\*.*?\*/",
            string.Empty,
            RegexOptions.Multiline | RegexOptions.Singleline);

        foreach (string restriccionOriginal in
                 solicitud.ConceptosNoPermitidos
                     .Where(r => !string.IsNullOrWhiteSpace(r))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string restriccion = restriccionOriginal.Trim();
            bool utilizada = restriccion.ToLowerInvariant() switch
            {
                "math.max" => Regex.IsMatch(
                    codigo,
                    @"\bMath\s*\.\s*max\s*\(",
                    RegexOptions.IgnoreCase),
                "math.min" => Regex.IsMatch(
                    codigo,
                    @"\bMath\s*\.\s*min\s*\(",
                    RegexOptions.IgnoreCase),
                "reverse" or "reverse()" => Regex.IsMatch(
                    codigo,
                    @"\.\s*reverse\s*\(",
                    RegexOptions.IgnoreCase),
                "arrays" or "array" => Regex.IsMatch(
                    codigo,
                    @"\bArray\s*\(|\[[^\]]*\]",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline),
                _ => Regex.IsMatch(
                    codigo,
                    $@"\b{Regex.Escape(restriccion)}\b",
                    RegexOptions.IgnoreCase)
            };

            if (!utilizada) continue;

            string codigoHallazgo = "P1_RECURSO_NO_PERMITIDO_" +
                Regex.Replace(
                    restriccion.ToUpperInvariant(),
                    @"[^A-Z0-9]+",
                    "_")
                .Trim('_');

            hallazgos.Add(new HallazgoTecnicoDTO
            {
                Codigo = codigoHallazgo,
                Descripcion =
                    $"La solución utiliza {restriccion}, pero la consigna " +
                    "solicita resolver el ejercicio sin ese recurso.",
                Critico = true
            });
        }
    }

    private static decimal PesoCaso(ResultadoCasoPruebaDTO caso) =>
        caso.EsOculto ? 1.17m : 1m;

    private static int CalcularFuncional(
        decimal proporcion,
        bool sintaxisValida,
        bool tieneReturn,
        int criticos)
    {
        decimal valor = proporcion * 82m;
        if (sintaxisValida) valor += 8.5m;
        if (tieneReturn) valor += 9.5m;
        valor -= Math.Min(18m, criticos * 6m);
        return RedondearPorcentaje(valor);
    }

    private static int CalcularTecnico(
        decimal proporcion,
        bool sintaxisValida,
        int criticos,
        int advertencias)
    {
        decimal valor = sintaxisValida ? 68m : 8m;
        valor += proporcion * 24m;
        valor -= criticos * 31m;
        valor -= advertencias * 8m;
        return RedondearPorcentaje(valor);
    }

    private static int CalcularSintactico(
        bool sintaxisValida,
        bool tieneReturn,
        bool tieneEstructuraPrincipal,
        int criticos)
    {
        if (!sintaxisValida) return 0;

        decimal valor = 59m;
        if (tieneReturn) valor += 19m;
        if (tieneEstructuraPrincipal) valor += 13m;
        if (criticos == 0) valor += 9m;
        else valor -= Math.Min(22m, criticos * 7m);
        return RedondearPorcentaje(valor);
    }

    private static int CalcularCalidad(string codigo, bool sintaxisValida)
    {
        if (!sintaxisValida) return 10;

        // La brevedad no es un defecto. Una expresión directa y una solución
        // con variables auxiliares reciben el mismo reconocimiento cuando ambas
        // son correctas. Esta heurística solo detecta problemas evidentes.
        decimal valor = 76m;
        int lineasUtiles = codigo
            .Split('\n')
            .Count(linea =>
                !string.IsNullOrWhiteSpace(linea) &&
                !linea.TrimStart().StartsWith("//"));

        if (lineasUtiles <= 45) valor += 8m;
        if (!Regex.IsMatch(codigo, @"\bconsole\.log\s*\(")) valor += 8m;
        if (!Regex.IsMatch(codigo, @"\b(?:a|b|x|y|n)\s*=")) valor += 4m;
        if (!Regex.IsMatch(codigo, @"(?:TODO|Tu código|completar)", RegexOptions.IgnoreCase))
            valor += 4m;

        return RedondearPorcentaje(valor);
    }

    private static int RedondearPorcentaje(decimal valor) =>
        Math.Clamp(
            (int)Math.Round(valor, MidpointRounding.AwayFromZero),
            0,
            100);

    private static string ConstruirEvidencia(
        string tipo,
        int superados,
        int total,
        bool sintaxisValida,
        IReadOnlyCollection<HallazgoTecnicoDTO> hallazgos) =>
        tipo.ToLowerInvariant() switch
        {
            "casoprueba" or "funcional" =>
                $"Se superaron {superados} de {total} casos de prueba configurados.",
            "sintactico" => sintaxisValida
                ? "Esprima pudo analizar el programa sin errores de sintaxis."
                : "Esprima detectó un error que impide interpretar el programa.",
            "calidadcodigo" =>
                "La evaluación considera legibilidad, estructura y adecuación al nivel.",
            _ => hallazgos.Count == 0
                ? "No se detectaron patrones técnicos problemáticos entre las reglas locales configuradas."
                : string.Join(" ", hallazgos.Take(2).Select(h => h.Descripcion))
        };

    private static string ConstruirObservacion(
        int porcentaje,
        CriterioCorreccionIADTO criterio,
        IReadOnlyCollection<HallazgoTecnicoDTO> hallazgos)
    {
        if (porcentaje >= 90)
            return "El criterio se cumple de forma sólida según la evidencia local.";
        if (porcentaje >= criterio.PorcentajeMinimoObligatorio)
            return "El criterio alcanza el mínimo, aunque todavía admite mejoras.";
        if (hallazgos.Count > 0)
            return "El criterio se ve afectado por: " + hallazgos.First().Descripcion;
        return "El resultado observado no alcanza todavía el nivel esperado para este criterio.";
    }

    private static List<string> ConstruirAspectosCorrectos(
        bool sintaxisValida,
        int superados,
        int total,
        bool tieneReturn,
        int calidad)
    {
        List<string> items = [];
        if (sintaxisValida) items.Add("El código tiene una sintaxis interpretable.");
        if (superados > 0) items.Add($"Supera {superados} de {total} casos de prueba.");
        if (tieneReturn) items.Add("La solución utiliza return para entregar un resultado.");
        if (calidad >= 75) items.Add("La estructura general es legible y adecuada al nivel.");
        return items.Count == 0 ? ["La solución fue recibida y analizada."] : items;
    }

    private static List<string> ConstruirAspectosAMejorar(
        IReadOnlyCollection<HallazgoTecnicoDTO> hallazgos,
        int superados,
        int total,
        bool sintaxisValida)
    {
        List<string> items = hallazgos
            .Select(h => h.Descripcion)
            .Distinct()
            .Take(4)
            .ToList();

        if (sintaxisValida && superados < total)
        {
            items.Add($"Revisá los {total - superados} casos de prueba que no producen la salida esperada.");
        }

        return items.Count == 0
            ? ["No se detectaron ajustes técnicos obligatorios en los casos configurados."]
            : items;
    }

    private static string ConstruirResumenLocal(
        bool sintaxisValida,
        int superados,
        int total,
        int criticos)
    {
        if (!sintaxisValida)
            return "EduCode no pudo ejecutar la solución porque contiene un error de sintaxis.";
        if (total == 0)
            return "La solución fue analizada localmente, pero este ejercicio todavía no tiene casos de prueba configurados.";
        if (superados == total && criticos == 0)
            return $"La solución supera los {total} casos de prueba y no presenta hallazgos técnicos críticos.";
        return $"La solución supera {superados} de {total} casos de prueba y presenta {criticos} hallazgo(s) técnico(s) crítico(s).";
    }

    private static string FormatearJson(string json)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement);
        }
        catch
        {
            return json;
        }
    }

    private static string Limitar(string valor, int maximo) =>
        valor.Length <= maximo ? valor : valor[..maximo] + "…";

    private static ResultadoEvaluacionLocalDTO CrearResultadoNoSoportado(
        SolicitudCorreccionIADTO solicitud) =>
        new()
        {
            CorreccionBase = new RespuestaCorreccionIADTO
            {
                Resultado = "PENDIENTE_REVISION",
                Resumen = "El evaluador local automático está preparado actualmente para JavaScript.",
                AspectosCorrectos = ["La solución quedó guardada."],
                AspectosAMejorar = ["Este lenguaje requiere revisión docente."],
                Pista = "Consultá al docente para revisar esta solución.",
                RequiereRevisionDocente = true,
                ResultadosCriterios = solicitud.Criterios.Select(c =>
                    new ResultadoCriterioIADTO
                    {
                        CriterioId = c.Id,
                        PorcentajeCumplimiento = 0,
                        Evidencia = "Lenguaje no soportado por el evaluador local.",
                        Observacion = "Requiere revisión docente."
                    }).ToList(),
                Modelo = "EduCode Local",
                VersionPrompt = "local-evaluator-2.5"
            }
        };
}
