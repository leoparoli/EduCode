using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using EduCode.Dominio.Excepciones;
using EduCode.DTOs;

namespace EduCode.Aplicacion.CasosUso;

public sealed class CURegistrarUsuario
{
    private readonly IRepositorioUsuarios _usuarios;
    private readonly IRepositorioTokens _tokensRepositorio;
    private readonly IServicioPassword _password;
    private readonly IServicioToken _tokens;
    private readonly IUnidadTrabajo _uow;

    public CURegistrarUsuario(
        IRepositorioUsuarios usuarios,
        IRepositorioTokens tokensRepositorio,
        IServicioPassword password,
        IServicioToken tokens,
        IUnidadTrabajo uow)
    {
        _usuarios = usuarios;
        _tokensRepositorio = tokensRepositorio;
        _password = password;
        _tokens = tokens;
        _uow = uow;
    }

    public async Task<UsuarioAutenticadoDTO> EjecutarAsync(
        RegistroUsuarioDTO dto,
        CancellationToken ct = default)
    {
        if (dto.Password != dto.ConfirmacionPassword)
            throw new ReglaNegocioException("Las contraseñas no coinciden.");

        if (dto.Password.Length < 8)
            throw new ReglaNegocioException(
                "La contraseña debe tener al menos 8 caracteres.");

        if (await _usuarios.ExisteEmailAsync(dto.Email, ct))
            throw new ConflictoDatosException(
                "El correo electrónico ya está registrado.");

        if (await _usuarios.ExisteNombreUsuarioAsync(dto.NombreUsuario, ct))
            throw new ConflictoDatosException(
                "El nombre de usuario ya está registrado.");

        var usuario = new Usuario(
            dto.Nombre,
            dto.Apellido,
            dto.NombreUsuario,
            dto.Email,
            _password.GenerarHash(dto.Password));

        await _usuarios.AgregarAsync(usuario, ct);
        await _uow.GuardarCambiosAsync(ct);

        var token = _tokens.GenerarToken(usuario);

        await _tokensRepositorio.AgregarAsync(
            new TokenActualizacion(
                usuario.Id,
                token.RefreshToken,
                token.FechaExpiracionRefreshToken),
            ct);

        await _uow.GuardarCambiosAsync(ct);

        return Mapear(usuario, token);
    }

    private static UsuarioAutenticadoDTO Mapear(
        Usuario usuario,
        TokenDTO token) =>
        new()
        {
            Id = usuario.Id,
            NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email,
            Rol = usuario.Rol.ToString(),
            AvatarClave = usuario.AvatarClave,
            FotoPerfilRuta = usuario.FotoPerfilRuta ?? string.Empty,
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            FechaExpiracion = token.FechaExpiracion,
            FechaExpiracionRefreshToken =
                token.FechaExpiracionRefreshToken
        };
}

public sealed class CUIniciarSesion
{
    private readonly IRepositorioUsuarios _usuarios;
    private readonly IRepositorioTokens _tokensRepositorio;
    private readonly IServicioPassword _password;
    private readonly IServicioToken _tokens;
    private readonly IUnidadTrabajo _uow;

    public CUIniciarSesion(
        IRepositorioUsuarios usuarios,
        IRepositorioTokens tokensRepositorio,
        IServicioPassword password,
        IServicioToken tokens,
        IUnidadTrabajo uow)
    {
        _usuarios = usuarios;
        _tokensRepositorio = tokensRepositorio;
        _password = password;
        _tokens = tokens;
        _uow = uow;
    }

    public async Task<UsuarioAutenticadoDTO> EjecutarAsync(
        LoginDTO dto,
        CancellationToken ct = default)
    {
        var usuario =
            await _usuarios.ObtenerPorEmailONombreUsuarioAsync(
                dto.EmailONombreUsuario,
                ct) ??
            throw new CredencialesInvalidasException();

        if (usuario.Estado != EstadoUsuario.Activo ||
            !_password.Verificar(
                dto.Password,
                usuario.PasswordHash))
            throw new CredencialesInvalidasException();

        usuario.RegistrarAcceso();

        var token = _tokens.GenerarToken(usuario);

        await _tokensRepositorio.AgregarAsync(
            new TokenActualizacion(
                usuario.Id,
                token.RefreshToken,
                token.FechaExpiracionRefreshToken),
            ct);

        await _uow.GuardarCambiosAsync(ct);

        return new UsuarioAutenticadoDTO
        {
            Id = usuario.Id,
            NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email,
            Rol = usuario.Rol.ToString(),
            AvatarClave = usuario.AvatarClave,
            FotoPerfilRuta = usuario.FotoPerfilRuta ?? string.Empty,
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            FechaExpiracion = token.FechaExpiracion,
            FechaExpiracionRefreshToken =
                token.FechaExpiracionRefreshToken
        };
    }
}

public sealed class CUActualizarToken
{
    private readonly IRepositorioTokens _tokensRepositorio;
    private readonly IServicioToken _tokens;
    private readonly IUnidadTrabajo _uow;

    public CUActualizarToken(
        IRepositorioTokens tokensRepositorio,
        IServicioToken tokens,
        IUnidadTrabajo uow)
    {
        _tokensRepositorio = tokensRepositorio;
        _tokens = tokens;
        _uow = uow;
    }

    public async Task<UsuarioAutenticadoDTO> EjecutarAsync(
        ActualizarTokenDTO dto,
        CancellationToken ct = default)
    {
        var tokenAnterior =
            await _tokensRepositorio.ObtenerValidoAsync(
                dto.RefreshToken,
                ct) ??
            throw new CredencialesInvalidasException();

        if (tokenAnterior.Usuario.Estado != EstadoUsuario.Activo)
            throw new CredencialesInvalidasException();

        tokenAnterior.Revocar();

        var nuevoToken = _tokens.GenerarToken(
            tokenAnterior.Usuario);

        await _tokensRepositorio.AgregarAsync(
            new TokenActualizacion(
                tokenAnterior.UsuarioId,
                nuevoToken.RefreshToken,
                nuevoToken.FechaExpiracionRefreshToken),
            ct);

        await _uow.GuardarCambiosAsync(ct);

        var usuario = tokenAnterior.Usuario;

        return new UsuarioAutenticadoDTO
        {
            Id = usuario.Id,
            NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email,
            Rol = usuario.Rol.ToString(),
            AvatarClave = usuario.AvatarClave,
            FotoPerfilRuta = usuario.FotoPerfilRuta ?? string.Empty,
            AccessToken = nuevoToken.AccessToken,
            RefreshToken = nuevoToken.RefreshToken,
            FechaExpiracion = nuevoToken.FechaExpiracion,
            FechaExpiracionRefreshToken =
                nuevoToken.FechaExpiracionRefreshToken
        };
    }
}
