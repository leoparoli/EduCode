using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using EduCode.Dominio.Excepciones;
using EduCode.DTOs;
using EduCode.Infraestructura.Configuracion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduCode.Infraestructura.Servicios;

public sealed class ServicioCorreccionGemini : IServicioCorreccionIA
{
    private readonly HttpClient _httpClient;
    private readonly ConfiguracionIA _configuracion;
    private readonly IRepositorioSolicitudesIA _solicitudes;
    private readonly IUnidadTrabajo _unidadTrabajo;
    private readonly ILogger<ServicioCorreccionGemini> _logger;

    private static readonly JsonSerializerOptions OpcionesJson =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

    public ServicioCorreccionGemini(
        HttpClient httpClient,
        IOptions<ConfiguracionIA> configuracion,
        IRepositorioSolicitudesIA solicitudes,
        IUnidadTrabajo unidadTrabajo,
        ILogger<ServicioCorreccionGemini> logger)
    {
        _httpClient = httpClient;
        _configuracion = configuracion.Value;
        _solicitudes = solicitudes;
        _unidadTrabajo = unidadTrabajo;
        _logger = logger;
    }

    public Task<RespuestaCorreccionIADTO> CorregirAsync(
        SolicitudCorreccionIADTO solicitud,
        CancellationToken ct = default) =>
        CorregirConModeloAsync(
            solicitud,
            _configuracion.Modelo,
            false,
            1,
            ct);

    public async Task<RespuestaCorreccionIADTO> CorregirConModeloAsync(
        SolicitudCorreccionIADTO solicitud,
        string modelo,
        bool esProvisoria,
        int numeroIntentoProveedor,
        CancellationToken ct = default)
    {
        ValidarConfiguracion(modelo);
        await ValidarLimiteDiarioAsync(solicitud.UsuarioId, ct);

        SolicitudIA registro = new(
            solicitud.UsuarioId,
            solicitud.IntentoId,
            "Google Gemini",
            modelo);

        await _solicitudes.AgregarAsync(registro, ct);
        await _unidadTrabajo.GuardarCambiosAsync(ct);

        IReadOnlyCollection<HallazgoCodigo> hallazgos =
            AnalizadorCodigoJavaScriptBasico.Analizar(
                solicitud.Lenguaje,
                solicitud.CodigoAlumno);

        try
        {
            using HttpRequestMessage request = CrearSolicitudHttp(
                solicitud,
                hallazgos,
                modelo);

            using HttpResponseMessage response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            string cuerpo = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                string mensaje = ConstruirMensajeError(
                    response.StatusCode,
                    cuerpo);

                registro.MarcarFallida(mensaje);
                await _unidadTrabajo.GuardarCambiosAsync(
                    CancellationToken.None);

                throw new ExcepcionProveedorIA(
                    mensaje,
                    (int)response.StatusCode,
                    EsEstadoTransitorio(response.StatusCode),
                    numeroIntentoProveedor);
            }

            RespuestaCorreccionIADTO resultado = InterpretarRespuesta(
                cuerpo,
                solicitud,
                hallazgos);

            resultado.Modelo = modelo;
            resultado.VersionPrompt = _configuracion.VersionPrompt;
            resultado.EsProvisoria = esProvisoria;

            registro.MarcarExitosa(
                resultado.TokensEntrada,
                resultado.TokensSalida,
                null);

            await _unidadTrabajo.GuardarCambiosAsync(ct);
            return resultado;
        }
        catch (LimiteSolicitudesIAException)
        {
            throw;
        }
        catch (ExcepcionProveedorIA)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            const string mensaje =
                "Gemini demoró demasiado en responder. " +
                "La solución quedó guardada y puede reintentarse.";

            registro.MarcarFallida(mensaje);
            await GuardarSinOcultarErrorAsync();

            throw new ExcepcionProveedorIA(
                mensaje,
                (int)HttpStatusCode.GatewayTimeout,
                true,
                numeroIntentoProveedor,
                ex);
        }
        catch (JsonException ex)
        {
            const string mensaje =
                "Gemini respondió, pero la corrección no tuvo el " +
                "formato esperado.";

            _logger.LogWarning(ex, "Respuesta JSON inválida de Gemini.");
            registro.MarcarFallida(mensaje);
            await GuardarSinOcultarErrorAsync();

            throw new ExcepcionProveedorIA(
                mensaje,
                null,
                true,
                numeroIntentoProveedor,
                ex);
        }
        catch (ServicioExternoException ex)
        {
            if (registro.Estado == EstadoSolicitudIA.Pendiente)
            {
                registro.MarcarFallida(ex.Message);
                await GuardarSinOcultarErrorAsync();
            }

            throw new ExcepcionProveedorIA(
                ex.Message,
                null,
                false,
                numeroIntentoProveedor,
                ex);
        }
        catch (HttpRequestException ex)
        {
            const string mensaje =
                "No fue posible establecer comunicación con Gemini.";

            _logger.LogWarning(ex, "Error de red invocando Gemini.");
            registro.MarcarFallida(mensaje);
            await GuardarSinOcultarErrorAsync();

            throw new ExcepcionProveedorIA(
                mensaje,
                null,
                true,
                numeroIntentoProveedor,
                ex);
        }
        catch (Exception ex)
        {
            const string mensaje =
                "Se produjo un error inesperado durante la corrección " +
                "con Gemini.";

            _logger.LogError(ex, "Error inesperado invocando Gemini.");
            registro.MarcarFallida(mensaje);
            await GuardarSinOcultarErrorAsync();

            throw new ExcepcionProveedorIA(
                mensaje,
                null,
                false,
                numeroIntentoProveedor,
                ex);
        }
    }

    private async Task ValidarLimiteDiarioAsync(
        int usuarioId,
        CancellationToken ct)
    {
        if (_configuracion.MaximoSolicitudesDiariasPorUsuario <= 0)
        {
            return;
        }

        DateTime desdeUtc = DateTime.UtcNow.Date;
        int cantidad = await _solicitudes.ContarPorUsuarioDesdeAsync(
            usuarioId,
            desdeUtc,
            ct);

        if (cantidad >= _configuracion.MaximoSolicitudesDiariasPorUsuario)
        {
            throw new LimiteSolicitudesIAException(
                $"Alcanzaste el límite diario de " +
                $"{_configuracion.MaximoSolicitudesDiariasPorUsuario} " +
                "correcciones completadas con inteligencia artificial.");
        }
    }

    private void ValidarConfiguracion(string modelo)
    {
        if (string.IsNullOrWhiteSpace(_configuracion.ApiKey))
        {
            throw new ServicioExternoException(
                "La API key de Gemini no está configurada. " +
                "Usá user-secrets o una variable de entorno.");
        }

        if (string.IsNullOrWhiteSpace(modelo))
        {
            throw new ServicioExternoException(
                "No se configuró el modelo de Gemini.");
        }
    }

    private static bool EsEstadoTransitorio(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout or
            HttpStatusCode.TooManyRequests ||
        (int)statusCode >= 500;

    private HttpRequestMessage CrearSolicitudHttp(
        SolicitudCorreccionIADTO solicitud,
        IReadOnlyCollection<HallazgoCodigo> hallazgos,
        string nombreModelo)
    {
        string modelo = Uri.EscapeDataString(nombreModelo.Trim());

        HttpRequestMessage request = new(
            HttpMethod.Post,
            $"models/{modelo}:generateContent");

        request.Headers.TryAddWithoutValidation(
            "x-goog-api-key",
            _configuracion.ApiKey);

        object cuerpo = new
        {
            systemInstruction = new
            {
                parts = new[]
                {
                    new { text = ObtenerInstruccionSistema() }
                }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new
                        {
                            text = ConstruirPrompt(solicitud, hallazgos)
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = _configuracion.Temperatura,
                maxOutputTokens = _configuracion.MaximoTokensSalida,
                thinkingConfig = new
                {
                    thinkingLevel = NormalizarNivelPensamiento(
                        _configuracion.NivelPensamiento)
                },

                // Este servicio utiliza models/{modelo}:generateContent.
                // En este endpoint, la salida JSON estructurada se configura
                // directamente con responseMimeType + responseJsonSchema.
                // responseFormat corresponde al esquema nuevo de Interactions
                // y no corresponde a esta integración basada en generateContent.
                responseMimeType = "application/json",
                responseJsonSchema = CrearEsquemaRespuesta()
            }
        };

        string json = JsonSerializer.Serialize(cuerpo, OpcionesJson);
        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        return request;
    }

    private RespuestaCorreccionIADTO InterpretarRespuesta(
        string cuerpo,
        SolicitudCorreccionIADTO solicitud,
        IReadOnlyCollection<HallazgoCodigo> hallazgos)
    {
        using JsonDocument document = JsonDocument.Parse(cuerpo);
        JsonElement raiz = document.RootElement;

        if (!raiz.TryGetProperty("candidates", out JsonElement candidates) ||
            candidates.GetArrayLength() == 0)
        {
            throw new ServicioExternoException(
                "Gemini no devolvió una corrección. " +
                "La respuesta pudo haber sido bloqueada por seguridad.");
        }

        JsonElement candidate = candidates[0];
        string? finishReason = candidate.TryGetProperty(
            "finishReason",
            out JsonElement reasonElement)
            ? reasonElement.GetString()
            : null;

        if (string.Equals(
                finishReason,
                "MAX_TOKENS",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ServicioExternoException(
                "Gemini interrumpió la corrección porque alcanzó el límite " +
                "de tokens de salida. Aumentá " +
                "InteligenciaArtificial:MaximoTokensSalida o reducí el " +
                "nivel de pensamiento.");
        }

        if (!string.IsNullOrWhiteSpace(finishReason) &&
            !string.Equals(
                finishReason,
                "STOP",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ServicioExternoException(
                $"Gemini no pudo completar la corrección. Motivo: " +
                $"{finishReason}.");
        }

        if (!candidate.TryGetProperty("content", out JsonElement content) ||
            !content.TryGetProperty("parts", out JsonElement parts))
        {
            throw new ServicioExternoException(
                $"Gemini no devolvió contenido utilizable" +
                (string.IsNullOrWhiteSpace(finishReason)
                    ? "."
                    : $". Motivo: {finishReason}."));
        }

        StringBuilder texto = new();

        foreach (JsonElement part in parts.EnumerateArray())
        {
            bool esPensamiento =
                part.TryGetProperty("thought", out JsonElement thought) &&
                thought.ValueKind == JsonValueKind.True;

            if (!esPensamiento &&
                part.TryGetProperty("text", out JsonElement textElement))
            {
                texto.Append(textElement.GetString());
            }
        }

        if (texto.Length == 0)
        {
            throw new ServicioExternoException(
                "Gemini no devolvió texto para la corrección.");
        }

        RespuestaCorreccionIADTO resultado;
        string respuestaTexto = texto.ToString();

        try
        {
            string jsonCorreccion = ExtraerJsonCorreccion(respuestaTexto);

            resultado =
                JsonSerializer.Deserialize<RespuestaCorreccionIADTO>(
                    jsonCorreccion,
                    OpcionesJson)
                ?? throw new JsonException(
                    "No fue posible deserializar la corrección de Gemini.");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Gemini devolvió JSON no interpretable. FinishReason: " +
                "{FinishReason}. Respuesta (máx. 2000 caracteres): {Texto}",
                finishReason,
                TruncarParaLog(respuestaTexto, 2000));

            throw new JsonException(
                "Gemini respondió, pero la corrección llegó incompleta o " +
                "con un JSON no interpretable. Se intentará nuevamente.",
                ex);
        }

        if (raiz.TryGetProperty("usageMetadata", out JsonElement usage))
        {
            resultado.TokensEntrada = ObtenerEntero(
                usage,
                "promptTokenCount");
            resultado.TokensSalida = ObtenerEntero(
                usage,
                "candidatesTokenCount");
        }

        resultado.Modelo = _configuracion.Modelo;
        resultado.VersionPrompt = _configuracion.VersionPrompt;

        NormalizarResultado(resultado, solicitud, hallazgos);
        return resultado;
    }

    private static void NormalizarResultado(
        RespuestaCorreccionIADTO resultado,
        SolicitudCorreccionIADTO solicitud,
        IReadOnlyCollection<HallazgoCodigo> hallazgos)
    {
        resultado.AspectosCorrectos = LimpiarLista(
            resultado.AspectosCorrectos);
        resultado.AspectosAMejorar = LimpiarLista(
            resultado.AspectosAMejorar);
        resultado.Resumen = resultado.Resumen?.Trim() ?? string.Empty;
        resultado.Pista = resultado.Pista?.Trim() ?? string.Empty;
        resultado.ResultadosCriterios = [];

        // La nota, la aprobación y los porcentajes por criterio pertenecen
        // exclusivamente al evaluador local de EduCode.
        resultado.Resultado = "FEEDBACK_PEDAGOGICO";

        if (string.IsNullOrWhiteSpace(resultado.Resumen))
        {
            resultado.Resumen = string.IsNullOrWhiteSpace(
                solicitud.ResumenTecnico)
                ? "EduCode completó la evaluación técnica. Revisá el detalle de casos y criterios."
                : solicitud.ResumenTecnico;
        }

        if (resultado.AspectosCorrectos.Count == 0)
        {
            resultado.AspectosCorrectos =
                solicitud.ResultadosCriteriosTecnicos
                    .Where(c => c.PorcentajeCumplimiento >= 75)
                    .OrderByDescending(c => c.PorcentajeCumplimiento)
                    .Take(4)
                    .Select(c => c.Descripcion)
                    .ToList();
        }

        if (resultado.AspectosAMejorar.Count == 0)
        {
            resultado.AspectosAMejorar =
                solicitud.HallazgosTecnicos
                    .Select(h => h.Descripcion)
                    .Concat(
                        solicitud.ResultadosCriteriosTecnicos
                            .Where(c => c.PorcentajeCumplimiento < 75)
                            .OrderBy(c => c.PorcentajeCumplimiento)
                            .Select(c => c.Observacion))
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(4)
                    .ToList();
        }
    }

    private static void AplicarImpactoHallazgo(
        HallazgoCodigo hallazgo,
        IReadOnlyCollection<CriterioCorreccionIADTO> criterios,
        List<ResultadoCriterioIADTO> resultados)
    {
        string[] palabrasPrincipales;
        int maximoPrincipal;
        string[] palabrasSecundarias = ["funcional", "resultado", "recorr", "cuenta"];
        int maximoSecundario;

        switch (hallazgo.Codigo)
        {
            case "JS_OFF_BY_ONE_LENGTH":
            case "JS_ACCESO_LENGTH":
                palabrasPrincipales = ["índice", "indice", "rango", "límite", "limite", "length"];
                maximoPrincipal = 22;
                maximoSecundario = 89;
                break;

            case "JS_ASIGNACION_CONDICIONAL":
                palabrasPrincipales = ["condición", "condicion", "compar", "operador"];
                maximoPrincipal = 25;
                maximoSecundario = 76;
                break;

            case "JS_LLAVES_DESEQUILIBRADAS":
            case "JS_PARENTESIS_DESEQUILIBRADOS":
                palabrasPrincipales = ["sintaxis", "estructura", "llaves", "paréntesis", "parentesis"];
                maximoPrincipal = 0;
                maximoSecundario = 42;
                break;

            default:
                palabrasPrincipales = ["correct", "funcional"];
                maximoPrincipal = hallazgo.Critico ? 45 : 80;
                maximoSecundario = hallazgo.Critico ? 70 : 90;
                break;
        }

        CriterioCorreccionIADTO? principal = BuscarCriterio(
            criterios,
            palabrasPrincipales)
            ?? criterios.FirstOrDefault(c => c.Obligatorio);

        if (principal is not null)
        {
            LimitarResultado(
                resultados,
                principal.Id,
                maximoPrincipal,
                hallazgo);
        }

        CriterioCorreccionIADTO? secundario = BuscarCriterio(
            criterios.Where(c => principal is null || c.Id != principal.Id),
            palabrasSecundarias);

        if (secundario is not null)
        {
            LimitarResultado(
                resultados,
                secundario.Id,
                maximoSecundario,
                hallazgo);
        }
    }

    private static CriterioCorreccionIADTO? BuscarCriterio(
        IEnumerable<CriterioCorreccionIADTO> criterios,
        IEnumerable<string> palabras) =>
        criterios.FirstOrDefault(c => palabras.Any(p =>
            c.Descripcion.Contains(p, StringComparison.OrdinalIgnoreCase) ||
            c.Tipo.Contains(p, StringComparison.OrdinalIgnoreCase)));

    private static void LimitarResultado(
        List<ResultadoCriterioIADTO> resultados,
        int criterioId,
        int maximo,
        HallazgoCodigo hallazgo)
    {
        ResultadoCriterioIADTO? resultado = resultados
            .FirstOrDefault(r => r.CriterioId == criterioId);

        if (resultado is null) return;

        resultado.PorcentajeCumplimiento = Math.Min(
            resultado.PorcentajeCumplimiento,
            maximo);

        string detalle = $"{hallazgo.Codigo}: {hallazgo.Descripcion}";

        if (string.IsNullOrWhiteSpace(resultado.Evidencia))
            resultado.Evidencia = detalle;
        else if (!resultado.Evidencia.Contains(
                     hallazgo.Codigo,
                     StringComparison.OrdinalIgnoreCase))
            resultado.Evidencia += " " + detalle;

        if (string.IsNullOrWhiteSpace(resultado.Observacion))
            resultado.Observacion = hallazgo.Descripcion;
        else if (!resultado.Observacion.Contains(
                     hallazgo.Codigo,
                     StringComparison.OrdinalIgnoreCase))
            resultado.Observacion += " " + detalle;
    }

    private static string ExtraerJsonCorreccion(string texto)
    {
        string limpio = texto.Trim();

        if (limpio.StartsWith("```", StringComparison.Ordinal))
        {
            int finPrimeraLinea = limpio.IndexOf('\n');
            if (finPrimeraLinea >= 0)
            {
                limpio = limpio[(finPrimeraLinea + 1)..];
            }

            int cierre = limpio.LastIndexOf("```", StringComparison.Ordinal);
            if (cierre >= 0)
            {
                limpio = limpio[..cierre];
            }

            limpio = limpio.Trim();
        }

        int inicioObjeto = limpio.IndexOf('{');
        int finObjeto = limpio.LastIndexOf('}');

        if (inicioObjeto < 0 || finObjeto <= inicioObjeto)
        {
            throw new JsonException(
                "La respuesta no contiene un objeto JSON completo.");
        }

        return limpio[inicioObjeto..(finObjeto + 1)];
    }

    private static string NormalizarNivelPensamiento(string? nivel)
    {
        string normalizado = nivel?.Trim().ToUpperInvariant() ?? "LOW";

        return normalizado is "MINIMAL" or "LOW" or "MEDIUM" or "HIGH"
            ? normalizado
            : "LOW";
    }

    private static string TruncarParaLog(string valor, int maximo) =>
        valor.Length <= maximo
            ? valor
            : valor[..maximo] + "…";

    private static List<string> LimpiarLista(List<string>? valores) =>
        (valores ?? [])
        .Where(v => !string.IsNullOrWhiteSpace(v))
        .Select(v => v.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Take(6)
        .ToList();

    private static int ObtenerEntero(
        JsonElement elemento,
        string propiedad) =>
        elemento.TryGetProperty(propiedad, out JsonElement valor) &&
        valor.TryGetInt32(out int numero)
            ? numero
            : 0;

    private static string ConstruirMensajeError(
        HttpStatusCode statusCode,
        string cuerpo)
    {
        string detalle = ExtraerMensajeError(cuerpo);

        return statusCode switch
        {
            HttpStatusCode.BadRequest =>
                "Gemini rechazó la solicitud. Revisá el modelo y la " +
                $"configuración de salida estructurada. {detalle}",
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                "La API key de Gemini no es válida o no tiene permisos " +
                $"para este proyecto. {detalle}",
            HttpStatusCode.NotFound =>
                "El modelo de Gemini configurado no está disponible. " +
                $"Revisá InteligenciaArtificial:Modelo. {detalle}",
            HttpStatusCode.TooManyRequests =>
                "Gemini alcanzó el límite de cuota o velocidad. " +
                $"Esperá unos minutos o revisá tu plan. {detalle}",
            _ when (int)statusCode >= 500 =>
                "Gemini se encuentra temporalmente indisponible. " + detalle,
            _ =>
                $"Gemini respondió con el estado {(int)statusCode}. {detalle}"
        };
    }

    private static string ExtraerMensajeError(string cuerpo)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(cuerpo);

            if (document.RootElement.TryGetProperty(
                    "error",
                    out JsonElement error) &&
                error.TryGetProperty(
                    "message",
                    out JsonElement message))
            {
                return message.GetString() ?? string.Empty;
            }
        }
        catch (JsonException)
        {
            // La respuesta de error no siempre contiene JSON.
        }

        return string.Empty;
    }

    private static string ObtenerInstruccionSistema() =>
        """
        Actuás como tutor pedagógico de Programación 1 para estudiantes
        principiantes que trabajan con JavaScript.

        EduCode ya realizó una evaluación técnica determinista mediante
        análisis sintáctico, ejecución limitada, casos de prueba, rúbricas
        ponderadas y un motor propio de puntuación.

        Tu tarea es únicamente explicar esa evidencia de forma clara,
        motivadora y didáctica.

        Reglas obligatorias:
        1. No recalcules, contradigas ni modifiques la nota, la aprobación,
           los porcentajes por criterio o los resultados de prueba.
        2. Fundamentá el feedback en la evidencia técnica proporcionada.
        3. Diferenciá decisiones correctas de ajustes pendientes.
        4. Explicá errores con lenguaje apropiado para principiantes.
        5. No reveles entradas ni salidas de casos de prueba ocultos.
        6. No inventes requisitos que no figuren en la consigna o rúbrica.
        7. No entregues una solución completa ni código listo para copiar.
        8. La pista debe orientar hacia el concepto a revisar.
        9. Respondé únicamente con el JSON solicitado.
        """;

    private static string ConstruirPrompt(
        SolicitudCorreccionIADTO solicitud,
        IReadOnlyCollection<HallazgoCodigo> hallazgos)
    {
        StringBuilder prompt = new();

        prompt.AppendLine("EJERCICIO");
        prompt.AppendLine($"Título: {solicitud.TituloEjercicio}");
        prompt.AppendLine($"Lenguaje: {solicitud.Lenguaje}");
        prompt.AppendLine($"Dificultad: {solicitud.NivelDificultad}");
        prompt.AppendLine($"Número de intento: {solicitud.NumeroIntento}");

        prompt.AppendLine();
        prompt.AppendLine("CONSIGNA");
        prompt.AppendLine(solicitud.Consigna);

        prompt.AppendLine();
        prompt.AppendLine("RESULTADO TÉCNICO DEFINITIVO DE EDUCCode");
        prompt.AppendLine(
            $"Puntaje exacto: {solicitud.PuntajeTecnicoExacto:0.00}/100");
        prompt.AppendLine(
            $"Aprobado: {(solicitud.AprobadoTecnico == true ? "sí" : "no")}");
        prompt.AppendLine($"Resumen: {solicitud.ResumenTecnico}");

        if (!string.IsNullOrWhiteSpace(solicitud.MotivoNoAprobacionTecnico))
        {
            prompt.AppendLine(
                "Motivo de no aprobación: " +
                solicitud.MotivoNoAprobacionTecnico);
        }

        prompt.AppendLine();
        prompt.AppendLine("RESULTADOS DE CASOS DE PRUEBA");
        foreach (ResultadoCasoPruebaDTO caso in
                 solicitud.ResultadosCasosTecnicos)
        {
            prompt.AppendLine(
                $"- {caso.Nombre} | " +
                $"Resultado: {(caso.Superado ? "superado" : "no superado")} | " +
                $"Oculto: {(caso.EsOculto ? "sí" : "no")} | " +
                caso.Mensaje);
        }

        if (solicitud.HallazgosTecnicos.Count > 0)
        {
            prompt.AppendLine();
            prompt.AppendLine("HALLAZGOS TÉCNICOS OBJETIVOS");
            foreach (HallazgoTecnicoDTO hallazgo in
                     solicitud.HallazgosTecnicos)
            {
                prompt.AppendLine(
                    $"- {hallazgo.Codigo} | " +
                    $"Crítico: {(hallazgo.Critico ? "sí" : "no")} | " +
                    hallazgo.Descripcion);
            }
        }

        prompt.AppendLine();
        prompt.AppendLine("EVALUACIÓN POR CRITERIOS");
        foreach (ResultadoCriterioTecnicoDTO criterio in
                 solicitud.ResultadosCriteriosTecnicos)
        {
            prompt.AppendLine(
                $"- {criterio.Descripcion} | " +
                $"Cumplimiento: {criterio.PorcentajeCumplimiento}% | " +
                $"Puntos: {criterio.PuntajeObtenido:0.00}/" +
                $"{criterio.PesoMaximo:0.00} | " +
                $"Obligatorio: {(criterio.Obligatorio ? "sí" : "no")} | " +
                $"Evidencia: {criterio.Evidencia} | " +
                $"Observación: {criterio.Observacion}");
        }

        prompt.AppendLine();
        prompt.AppendLine("CÓDIGO DEL ALUMNO");
        prompt.AppendLine("```javascript");
        prompt.AppendLine(solicitud.CodigoAlumno);
        prompt.AppendLine("```");

        prompt.AppendLine();
        prompt.AppendLine(
            "Redactá el feedback sin modificar la evaluación técnica. " +
            "No reveles datos de casos ocultos ni una solución completa.");

        return prompt.ToString();
    }

    private static object CrearEsquemaRespuesta() => new
    {
        type = "object",
        additionalProperties = false,
        properties = new Dictionary<string, object>
        {
            ["resultado"] = new
            {
                type = "string",
                description = "Usar el valor fijo FEEDBACK_PEDAGOGICO."
            },
            ["resumen"] = new
            {
                type = "string",
                description =
                    "Explicación pedagógica breve basada en la evidencia técnica."
            },
            ["aspectosCorrectos"] = new
            {
                type = "array",
                maxItems = 5,
                items = new { type = "string" }
            },
            ["aspectosAMejorar"] = new
            {
                type = "array",
                maxItems = 5,
                items = new { type = "string" }
            },
            ["pista"] = new
            {
                type = "string",
                description =
                    "Una orientación conceptual sin incluir la solución completa."
            },
            ["requiereRevisionDocente"] = new { type = "boolean" }
        },
        required = new[]
        {
            "resultado",
            "resumen",
            "aspectosCorrectos",
            "aspectosAMejorar",
            "pista",
            "requiereRevisionDocente"
        }
    };

    private async Task GuardarSinOcultarErrorAsync()
    {
        try
        {
            await _unidadTrabajo.GuardarCambiosAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No fue posible guardar el estado de la solicitud de IA.");
        }
    }
}
