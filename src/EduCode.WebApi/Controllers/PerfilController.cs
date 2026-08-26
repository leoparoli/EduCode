using EduCode.Aplicacion.CasosUso;
using EduCode.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/perfil")]
public sealed class PerfilController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obtener(
        [FromServices] CUPerfilUsuario casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.ObtenerAsync(ct));

    [HttpPut]
    public async Task<IActionResult> Actualizar(
        [FromBody] ActualizarPerfilDTO dto,
        [FromServices] CUPerfilUsuario casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.ActualizarAsync(dto, ct));

    [HttpGet("opiniones")]
    [Authorize(Roles = "Alumno,Docente")]
    public async Task<IActionResult> ObtenerMisOpiniones(
        [FromServices] CUOpinionesUsuario casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.ObtenerMiasAsync(ct));

    [HttpPost("opiniones")]
    [Authorize(Roles = "Alumno,Docente")]
    public async Task<IActionResult> CrearOpinion(
        [FromBody] CrearOpinionDTO dto,
        [FromServices] CUOpinionesUsuario casoUso,
        CancellationToken ct) =>
        StatusCode(
            StatusCodes.Status201Created,
            await casoUso.CrearAsync(dto, ct));
}
