using EduCode.Dominio.Excepciones;

namespace EduCode.Dominio.Entidades;

public sealed class CambioNombreUsuario : Entidad
{
    private CambioNombreUsuario() { }

    public CambioNombreUsuario(
        int usuarioId,
        string nombreAnterior,
        string nombreNuevo,
        DateTime fechaCambioUtc)
    {
        if (usuarioId <= 0)
            throw new ReglaNegocioException("El usuario asociado al cambio es obligatorio.");

        UsuarioId = usuarioId;
        NombreAnterior = ValidarNombre(nombreAnterior, "El nombre de usuario anterior");
        NombreNuevo = ValidarNombre(nombreNuevo, "El nuevo nombre de usuario");
        FechaCambio = fechaCambioUtc.Kind == DateTimeKind.Utc
            ? fechaCambioUtc
            : fechaCambioUtc.ToUniversalTime();
    }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string NombreAnterior { get; private set; } = string.Empty;
    public string NombreNuevo { get; private set; } = string.Empty;
    public DateTime FechaCambio { get; private set; }

    private static string ValidarNombre(string valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ReglaNegocioException($"{campo} es obligatorio.");

        valor = valor.Trim().ToLowerInvariant();
        if (valor.Length > 50)
            throw new ReglaNegocioException($"{campo} no puede superar 50 caracteres.");

        return valor;
    }
}
