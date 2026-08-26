using EduCode.Aplicacion.CasosUso;
using EduCode.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Route("api/temas")]
[Authorize]
public sealed class TemasController : ControllerBase
{
    [HttpGet("curso/{cursoId:int}")]
    public async Task<IActionResult> Listar(
        int cursoId,
        [FromServices] CUListarTemasAlumno casoUso,
        CancellationToken ct)
    {
        return Ok(await casoUso.EjecutarAsync(cursoId, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear(
        [FromBody] AltaTemaDTO dto,
        [FromServices] CUAltaTema casoUso,
        CancellationToken ct)
    {
        var id = await casoUso.EjecutarAsync(dto, ct);

        return CreatedAtAction(
            nameof(Listar),
            new { cursoId = dto.CursoId },
            new { id });
    }
}
