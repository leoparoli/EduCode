using EduCode.Aplicacion.CasosUso;
using EduCode.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/administracion/opiniones")]
public sealed class OpinionesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromServices] CUOpinionesUsuario casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.ObtenerTodasAsync(ct));

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(
        int id,
        [FromBody] ActualizarEstadoOpinionDTO dto,
        [FromServices] CUOpinionesUsuario casoUso,
        CancellationToken ct) =>
        Ok(await casoUso.CambiarEstadoAsync(id, dto, ct));
}
