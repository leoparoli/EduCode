using EduCode.Aplicacion.CasosUso;
using EduCode.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Route("api/docente")]
[Authorize(Roles = "Docente")]
public sealed class DocenteController : ControllerBase
{
    [HttpGet("panel")]
    public async Task<IActionResult> ObtenerPanel(
        [FromServices] CUPortalDocente casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.ObtenerPanelAsync(ct));

    [HttpGet("propuestas")]
    public async Task<IActionResult> ObtenerPropuestas(
        [FromServices] CUPortalDocente casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.ListarPropuestasAsync(ct));

    [HttpPut("propuestas/{propuestaId:int}")]
    public async Task<IActionResult> ActualizarPropuesta(
        int propuestaId,
        [FromBody] CrearPropuestaEjercicioDTO dto,
        [FromServices] CUPortalDocente casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.ActualizarPropuestaAsync(propuestaId, dto, ct));

    [HttpPost("propuestas")]
    public async Task<IActionResult> CrearPropuesta(
        [FromBody] CrearPropuestaEjercicioDTO dto,
        [FromServices] CUPortalDocente casoUso,
        CancellationToken ct)
    {
        PropuestaEjercicioDTO propuesta = await casoUso.CrearPropuestaAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, propuesta);
    }
}
