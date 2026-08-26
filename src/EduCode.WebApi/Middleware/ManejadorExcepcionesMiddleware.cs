using System.Net;
using System.Text.Json;
using EduCode.Dominio.Excepciones;

namespace EduCode.WebApi.Middleware;

public sealed class ManejadorExcepcionesMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ManejadorExcepcionesMiddleware> _logger;

    public ManejadorExcepcionesMiddleware(
        RequestDelegate next,
        ILogger<ManejadorExcepcionesMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error procesando {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            HttpStatusCode status = ex switch
            {
                EntidadNoEncontradaException =>
                    HttpStatusCode.NotFound,
                ConflictoDatosException =>
                    HttpStatusCode.Conflict,
                ReglaNegocioException =>
                    HttpStatusCode.BadRequest,
                CredencialesInvalidasException =>
                    HttpStatusCode.Unauthorized,
                UnauthorizedAccessException =>
                    HttpStatusCode.Unauthorized,
                LimiteSolicitudesIAException =>
                    HttpStatusCode.TooManyRequests,
                ServicioExternoTemporalException =>
                    HttpStatusCode.ServiceUnavailable,
                ServicioExternoException =>
                    HttpStatusCode.BadGateway,
                _ =>
                    HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    error = ObtenerMensajePublico(ex, status),
                    status = (int)status
                }));
        }
    }

    private static string ObtenerMensajePublico(
        Exception ex,
        HttpStatusCode status)
    {
        return ex switch
        {
            LimiteSolicitudesIAException => ex.Message,
            EntidadNoEncontradaException or
            ConflictoDatosException or
            ReglaNegocioException or
            CredencialesInvalidasException => ex.Message,
            UnauthorizedAccessException =>
                "Tu sesión venció o no tenés permisos para realizar esta operación.",
            ServicioExternoTemporalException =>
                "El servicio está tardando más de lo esperado. Intentá nuevamente en unos segundos.",
            ServicioExternoException =>
                "No pudimos completar la operación en este momento. Intentá nuevamente en unos segundos.",
            _ when status == HttpStatusCode.InternalServerError =>
                "Ocurrió un error inesperado. Intentá nuevamente.",
            _ => "No pudimos completar la operación. Intentá nuevamente."
        };
    }
}
