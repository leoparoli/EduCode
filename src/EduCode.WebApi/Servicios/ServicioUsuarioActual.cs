using System.Security.Claims;
using EduCode.Aplicacion.Interfaces;

namespace EduCode.WebApi.Servicios;

public sealed class ServicioUsuarioActual : IServicioUsuarioActual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServicioUsuarioActual(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int ObtenerUsuarioId()
    {
        var valor = _httpContextAccessor
            .HttpContext?
            .User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(valor, out var usuarioId)
            ? usuarioId
            : throw new UnauthorizedAccessException(
                "No se pudo identificar al usuario autenticado.");
    }

    public string ObtenerRol() =>
        _httpContextAccessor
            .HttpContext?
            .User
            .FindFirstValue(ClaimTypes.Role) ??
        string.Empty;
}
