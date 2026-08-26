using EduCode.Aplicacion.CasosUso;
using EduCode.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Route("api/ejercicios")]
[Authorize]
public sealed class EjerciciosController : ControllerBase
{
    [HttpGet("tema/{temaId:int}")]
    public async Task<IActionResult> ListarPorTema(
        int temaId,
        [FromServices] CUListarEjerciciosPorTema casoUso,
        CancellationToken ct)
    {
        return Ok(await casoUso.EjecutarAsync(temaId, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(
        int id,
        [FromServices] CUObtenerEjercicio casoUso,
        CancellationToken ct)
    {
        return Ok(await casoUso.EjecutarAsync(id, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear(
        [FromBody] AltaEjercicioDTO dto,
        [FromServices] CUAltaEjercicio casoUso,
        CancellationToken ct)
    {
        var id = await casoUso.EjecutarAsync(dto, ct);

        return CreatedAtAction(
            nameof(Obtener),
            new { id },
            new { id });
    }

    [HttpPost("{id:int}/intentos")]
    [Authorize(Roles = "Alumno")]
    public async Task<IActionResult> EnviarIntento(
        int id,
        [FromBody] EnviarIntentoDTO dto,
        [FromServices] CUEnviarIntento casoUso,
        CancellationToken ct)
    {
        dto.EjercicioId = id;
        ResultadoIntentoDTO resultado = await casoUso.EjecutarAsync(dto, ct);

        return Ok(resultado);
    }

    [HttpPost("intentos/{intentoId:int}/reintentar")]
    [Authorize(Roles = "Alumno")]
    public async Task<IActionResult> ReintentarCorreccion(
        int intentoId,
        [FromServices] CUReintentarCorreccionIntento casoUso,
        CancellationToken ct)
    {
        ResultadoIntentoDTO resultado = await casoUso.EjecutarAsync(
            intentoId,
            ct);

        return Ok(resultado);
    }


    [HttpGet("intentos/{intentoId:int}/resultado")]
    [Authorize(Roles = "Alumno")]
    public async Task<IActionResult> ObtenerResultado(
        int intentoId,
        [FromServices] CUObtenerResultadoIntento casoUso,
        CancellationToken ct)
    {
        return Ok(await casoUso.EjecutarAsync(intentoId, ct));
    }

    [HttpGet("{id:int}/intentos")]
    [Authorize(Roles = "Alumno")]
    public async Task<IActionResult> Historial(
        int id,
        [FromServices] CUObtenerHistorialIntentos casoUso,
        CancellationToken ct)
    {
        return Ok(await casoUso.EjecutarAsync(id, ct));
    }

    [HttpGet("{id:int}/pista")]
    [Authorize(Roles = "Alumno")]
    public async Task<IActionResult> ObtenerPista(
        int id,
        [FromQuery] int nivelActual,
        [FromServices] CUSolicitarPista casoUso,
        CancellationToken ct)
    {
        return Ok(await casoUso.EjecutarAsync(
            id,
            nivelActual,
            ct));
    }
}
