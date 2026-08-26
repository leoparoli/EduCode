using EduCode.Aplicacion.CasosUso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Route("api/progreso")]
[Authorize(Roles = "Alumno")]
public sealed class ProgresoController : ControllerBase
{
    [HttpGet("mi-progreso")]
    public async Task<IActionResult> Obtener(
        [FromServices] CUObtenerProgresoAlumno casoUso,
        CancellationToken ct)
    {
        return Ok(await casoUso.EjecutarAsync(ct));
    }
}
