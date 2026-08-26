using EduCode.AccesoDatos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Route("api/estado")]
public sealed class EstadoController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obtener(
        [FromServices] EduCodeContext contexto,
        CancellationToken ct)
    {
        bool baseDatosOperativa;
        string detalle;
        try
        {
            baseDatosOperativa = await contexto.Database.CanConnectAsync(ct);
            detalle = baseDatosOperativa
                ? "Conexión verificada"
                : "No fue posible establecer conexión";
        }
        catch
        {
            baseDatosOperativa = false;
            detalle = "No fue posible verificar la conexión";
        }

        return Ok(new
        {
            servicio = "EduCode API",
            estado = baseDatosOperativa ? "Operativa" : "Degradada",
            fechaUtc = DateTime.UtcNow,
            version = "3.0.4",
            componentes = new
            {
                api = new { estado = "Operativa", detalle = "Solicitud procesada" },
                baseDatos = new
                {
                    estado = baseDatosOperativa ? "Operativa" : "No disponible",
                    detalle
                }
            }
        });
    }
}
