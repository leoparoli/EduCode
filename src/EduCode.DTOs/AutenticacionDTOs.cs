namespace EduCode.DTOs;

public sealed class RegistroUsuarioDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmacionPassword { get; set; } = string.Empty;
}

public sealed class LoginDTO
{
    public string EmailONombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class ActualizarTokenDTO
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class UsuarioAutenticadoDTO
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string AvatarClave { get; set; } = "codigo";
    public string FotoPerfilRuta { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime FechaExpiracion { get; set; }
    public DateTime FechaExpiracionRefreshToken { get; set; }
}

public sealed class TokenDTO
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime FechaExpiracion { get; set; }
    public DateTime FechaExpiracionRefreshToken { get; set; }
}
