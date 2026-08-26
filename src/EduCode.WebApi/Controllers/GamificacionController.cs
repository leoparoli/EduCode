using EduCode.Aplicacion.CasosUso;
using EduCode.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Route("api/gamificacion")]
[Authorize(Roles = "Alumno")]
public sealed class GamificacionController : ControllerBase
{
    private readonly CUGamificacion _casoUso;

    public GamificacionController(CUGamificacion casoUso) =>
        _casoUso = casoUso;

    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ResumenGamificacionDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenGamificacionDTO>> ObtenerResumen(
        [FromQuery] int limiteRanking = 20,
        CancellationToken ct = default) =>
        Ok(await _casoUso.ObtenerResumenAsync(limiteRanking, ct));

    [HttpPut("visibilidad")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CambiarVisibilidad(
        [FromBody] CambiarVisibilidadRankingDTO dto,
        CancellationToken ct = default)
    {
        await _casoUso.CambiarVisibilidadRankingAsync(dto.Mostrar, ct);
        return NoContent();
    }
}
