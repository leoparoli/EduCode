using EduCode.Aplicacion.CasosUso;
using EduCode.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Route("api/administracion")]
[Authorize(Roles = "Administrador")]
public sealed class PortalAdministracionController : ControllerBase
{
    [HttpGet("panel")]
    public async Task<IActionResult> ObtenerPanel(
        [FromServices] CUPortalAdministracion casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.ObtenerPanelAsync(ct));

    [HttpGet("estructura-academica")]
    public async Task<IActionResult> ObtenerEstructura(
        [FromServices] CUPortalAdministracion casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.ObtenerEstructuraAsync(ct));

    [HttpPost("grupos")]
    public async Task<IActionResult> CrearGrupo(
        [FromBody] CrearGrupoDTO dto,
        [FromServices] CUPortalAdministracion casoUso,
        CancellationToken ct)
    {
        int id = await casoUso.CrearGrupoAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, new { id });
    }

    [HttpPost("grupos/{grupoId:int}/inscripciones")]
    public async Task<IActionResult> InscribirAlumno(
        int grupoId,
        [FromBody] InscribirAlumnoDTO dto,
        [FromServices] CUPortalAdministracion casoUso,
        CancellationToken ct)
    {
        await casoUso.InscribirAlumnoAsync(grupoId, dto, ct);
        return NoContent();
    }

    [HttpGet("invitaciones-docentes")]
    public async Task<IActionResult> ObtenerInvitaciones(
        [FromServices] CUInvitacionesDocentes casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.ListarAsync(ct));

    [HttpPut("invitaciones-docentes/{invitacionId:int}/revocar")]
    public async Task<IActionResult> RevocarInvitacion(
        int invitacionId,
        [FromServices] CUInvitacionesDocentes casoUso,
        CancellationToken ct)
    {
        await casoUso.RevocarAsync(invitacionId, ct);
        return NoContent();
    }

    [HttpPost("invitaciones-docentes")]
    public async Task<IActionResult> CrearInvitacion(
        [FromBody] CrearInvitacionDocenteDTO dto,
        [FromServices] CUInvitacionesDocentes casoUso,
        CancellationToken ct)
    {
        InvitacionDocenteDTO invitacion = await casoUso.CrearAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, invitacion);
    }

    [HttpGet("propuestas-ejercicios")]
    public async Task<IActionResult> ObtenerPropuestas(
        [FromServices] CUPortalAdministracion casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.ListarPropuestasAsync(ct));

    [HttpPut("propuestas-ejercicios/{propuestaId:int}")]
    public async Task<IActionResult> RevisarPropuesta(
        int propuestaId,
        [FromBody] RevisarPropuestaEjercicioDTO dto,
        [FromServices] CUPortalAdministracion casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.RevisarPropuestaAsync(propuestaId, dto, ct));
}
