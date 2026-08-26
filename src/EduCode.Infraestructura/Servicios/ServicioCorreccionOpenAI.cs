using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EduCode.DTOs;
using EduCode.Infraestructura.Configuracion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduCode.Infraestructura.Servicios;

public sealed class ServicioCorreccionOpenAI
{
    private readonly HttpClient _httpClient;
    private readonly ConfiguracionIA _configuracion;
    private readonly ILogger<ServicioCorreccionOpenAI> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ServicioCorreccionOpenAI(
        HttpClient httpClient,
        IOptions<ConfiguracionIA> configuracion,
        ILogger<ServicioCorreccionOpenAI> logger)
    {
        _httpClient = httpClient;
        _configuracion = configuracion.Value;
        _logger = logger;
    }

    public async Task<RespuestaCorreccionIADTO> CorregirAsync(
        SolicitudCorreccionIADTO solicitud,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_configuracion.OpenAIApiKey))
        {
            throw new ExcepcionProveedorIA(
                "No se configuró InteligenciaArtificial:OpenAIApiKey.",
                401,
                false,
                1);
        }

        using HttpRequestMessage request = new(
            HttpMethod.Post,
            "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _configuracion.OpenAIApiKey);

        object body = new
        {
            model = _configuracion.OpenAIModelo,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = InstruccionSistema()
                },
                new
                {
                    role = "user",
                    content = ConstruirPrompt(solicitud)
                }
            },
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "feedback_educode",
                    strict = true,
                    schema = CrearEsquema()
                }
            }
        };

        request.Content = JsonContent.Create(body, options: JsonOptions);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, ct);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new ExcepcionProveedorIA(
                "OpenAI demoró demasiado en responder.",
                408,
                true,
                1,
                ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ExcepcionProveedorIA(
                "No fue posible conectar con OpenAI.",
                null,
                true,
                1,
                ex);
        }

        string content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            int status = (int)response.StatusCode;
            bool transitorio = status is 408 or 429 || status >= 500;
            throw new ExcepcionProveedorIA(
                "OpenAI rechazó la solicitud. " + ExtraerError(content),
                status,
                transitorio,
                1);
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(content);
            string? json = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            RespuestaCorreccionIADTO? resultado =
                JsonSerializer.Deserialize<RespuestaCorreccionIADTO>(
                    json ?? string.Empty,
                    JsonOptions);

            if (resultado is null)
                throw new JsonException("La respuesta no pudo deserializarse.");

            resultado.Resultado = "FEEDBACK_PEDAGOGICO";
            resultado.ResultadosCriterios = [];
            resultado.Modelo = "OpenAI · " + _configuracion.OpenAIModelo;
            resultado.VersionPrompt = _configuracion.VersionPrompt;
            resultado.EsProvisoria = false;
            return resultado;
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException)
        {
            _logger.LogWarning(
                ex,
                "OpenAI respondió con un formato inesperado: {Contenido}",
                content.Length <= 1200 ? content : content[..1200]);

            throw new ExcepcionProveedorIA(
                "OpenAI respondió, pero el feedback no tuvo el formato esperado.",
                502,
                true,
                1,
                ex);
        }
    }

    private static string InstruccionSistema() =>
        """
        Actuás como tutor de Programación 1. EduCode ya calculó la nota
        mediante casos de prueba y análisis local. Tu tarea es únicamente
        explicar la evidencia de forma clara y pedagógica. No cambies el
        puntaje, no declares aprobaciones y no entregues una solución completa.
        Respondé exclusivamente con el JSON solicitado.
        """;

    private static string ConstruirPrompt(SolicitudCorreccionIADTO solicitud)
    {
        StringBuilder sb = new();
        sb.AppendLine("EJERCICIO: " + solicitud.TituloEjercicio);
        sb.AppendLine("CONSIGNA: " + solicitud.Consigna);
        sb.AppendLine(
            $"NOTA TÉCNICA DEFINITIVA: " +
            $"{solicitud.PuntajeTecnicoExacto:0.00}/100");
        sb.AppendLine(
            $"APROBADO: {(solicitud.AprobadoTecnico == true ? "sí" : "no")}");
        sb.AppendLine("RESUMEN TÉCNICO: " + solicitud.ResumenTecnico);

        if (!string.IsNullOrWhiteSpace(solicitud.MotivoNoAprobacionTecnico))
        {
            sb.AppendLine(
                "MOTIVO DE NO APROBACIÓN: " +
                solicitud.MotivoNoAprobacionTecnico);
        }

        sb.AppendLine("CASOS DE PRUEBA:");
        foreach (ResultadoCasoPruebaDTO caso in
                 solicitud.ResultadosCasosTecnicos)
        {
            sb.AppendLine(
                $"- {caso.Nombre}: " +
                $"{(caso.Superado ? "superado" : "no superado")}; " +
                $"oculto={(caso.EsOculto ? "sí" : "no")}; {caso.Mensaje}");
        }

        sb.AppendLine("HALLAZGOS TÉCNICOS:");
        foreach (HallazgoTecnicoDTO hallazgo in solicitud.HallazgosTecnicos)
        {
            sb.AppendLine(
                $"- {hallazgo.Codigo}: {hallazgo.Descripcion}");
        }

        sb.AppendLine("CRITERIOS TÉCNICOS:");
        foreach (ResultadoCriterioTecnicoDTO criterio in
                 solicitud.ResultadosCriteriosTecnicos)
        {
            sb.AppendLine(
                $"- {criterio.Descripcion}: " +
                $"{criterio.PorcentajeCumplimiento}% y " +
                $"{criterio.PuntajeObtenido:0.00}/" +
                $"{criterio.PesoMaximo:0.00}. " +
                $"Evidencia: {criterio.Evidencia}. " +
                $"Observación: {criterio.Observacion}");
        }

        sb.AppendLine("CÓDIGO DEL ALUMNO:");
        sb.AppendLine("```javascript");
        sb.AppendLine(solicitud.CodigoAlumno);
        sb.AppendLine("```");
        sb.AppendLine(
            "Explicá la evidencia sin modificar la nota y sin revelar " +
            "casos ocultos ni una solución completa.");
        return sb.ToString();
    }

    private static object CrearEsquema() => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            resultado = new { type = "string" },
            resumen = new { type = "string" },
            aspectosCorrectos = new
            {
                type = "array",
                items = new { type = "string" }
            },
            aspectosAMejorar = new
            {
                type = "array",
                items = new { type = "string" }
            },
            pista = new { type = "string" },
            requiereRevisionDocente = new { type = "boolean" }
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

    private static string ExtraerError(string content)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(content);
            return doc.RootElement
                .GetProperty("error")
                .GetProperty("message")
                .GetString() ?? string.Empty;
        }
        catch
        {
            return content.Length <= 300 ? content : string.Empty;
        }
    }
}
