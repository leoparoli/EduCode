using EduCode.Aplicacion.Interfaces;
using EduCode.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Route("api/tutor-ia")]
[Authorize(Roles = "Alumno")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class TutorIAController : ControllerBase
{
    [HttpGet("estado")]
    public async Task<IActionResult> ObtenerEstado(
        [FromServices] IServicioTutorIA tutor,
        [FromServices] IServicioUsuarioActual usuarioActual,
        CancellationToken ct)
    {
        int usuarioId = usuarioActual.ObtenerUsuarioId();
        return Ok(await tutor.ObtenerEstadoAsync(usuarioId, ct));
    }

    [HttpPost("consultar")]
    public async Task<IActionResult> Consultar(
        [FromBody] SolicitudTutorIADTO? solicitud,
        [FromServices] IServicioTutorIA tutor,
        [FromServices] IServicioUsuarioActual usuarioActual,
        CancellationToken ct)
    {
        if (solicitud is null)
            return BadRequest(new { error = "La consulta no tiene un formato válido." });

        // El identificador siempre se obtiene del JWT. Nunca se confía en el
        // valor enviado por el cliente.
        solicitud.UsuarioId = usuarioActual.ObtenerUsuarioId();
        RespuestaTutorIADTO respuesta = await tutor.ConsultarAsync(solicitud, ct);
        return Ok(respuesta);
    }
}
