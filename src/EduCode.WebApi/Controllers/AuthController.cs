using EduCode.Aplicacion.CasosUso;
using EduCode.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("registro")]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistroUsuarioDTO dto,
        [FromServices] CURegistrarUsuario casoUso,
        CancellationToken ct)
    {
        var resultado = await casoUso.EjecutarAsync(dto, ct);

        return StatusCode(
            StatusCodes.Status201Created,
            resultado);
    }

    [HttpPost("registro-docente")]
    public async Task<IActionResult> RegistrarDocente(
        [FromBody] RegistroDocenteInvitadoDTO dto,
        [FromServices] CURegistrarDocenteInvitado casoUso,
        CancellationToken ct)
    {
        var resultado = await casoUso.EjecutarAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, resultado);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> ActualizarToken(
        [FromBody] ActualizarTokenDTO dto,
        [FromServices] CUActualizarToken casoUso,
        CancellationToken ct)
    {
        return Ok(await casoUso.EjecutarAsync(dto, ct));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDTO dto,
        [FromServices] CUIniciarSesion casoUso,
        CancellationToken ct)
    {
        return Ok(await casoUso.EjecutarAsync(dto, ct));
    }
}
