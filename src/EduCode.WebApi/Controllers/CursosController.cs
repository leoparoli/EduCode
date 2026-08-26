using EduCode.Aplicacion.CasosUso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Route("api/cursos")]
[Authorize]
public sealed class CursosController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromServices] CUListarCursos casoUso,
        CancellationToken ct)
    {
        return Ok(await casoUso.EjecutarAsync(ct));
    }
}
