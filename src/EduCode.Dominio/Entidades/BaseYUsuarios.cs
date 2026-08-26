using EduCode.Dominio.Enums;
using EduCode.Dominio.Excepciones;

namespace EduCode.Dominio.Entidades;

public abstract class Entidad
{
    public int Id { get; protected set; }
}

public class Usuario : Entidad
{
    private Usuario() { }

    public Usuario(string nombre, string apellido, string nombreUsuario, string email, string passwordHash,
        RolUsuario rol = RolUsuario.Alumno)
    {
        CambiarNombre(nombre, apellido);
        CambiarNombreUsuario(nombreUsuario);
        CambiarEmail(email);
        PasswordHash = string.IsNullOrWhiteSpace(passwordHash)
            ? throw new ReglaNegocioException("El hash de contraseña es obligatorio.")
            : passwordHash;
        Rol = rol;
        Estado = EstadoUsuario.Activo;
        FechaRegistro = DateTime.UtcNow;
        MostrarEnRanking = true;
        AvatarClave = CatalogoAvataresPerfil.Predeterminado;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Apellido { get; private set; } = string.Empty;
    public string NombreUsuario { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public RolUsuario Rol { get; private set; }
    public EstadoUsuario Estado { get; private set; }
    public DateTime FechaRegistro { get; private set; }
    public DateTime? FechaUltimoAcceso { get; private set; }
    public bool MostrarEnRanking { get; private set; }
    public string AvatarClave { get; private set; } = CatalogoAvataresPerfil.Predeterminado;
    public string? FotoPerfilRuta { get; private set; }

    public PerfilDocente? PerfilDocente { get; private set; }
    public ICollection<IntentoEjercicio> IntentosEjercicios { get; private set; } = new List<IntentoEjercicio>();
    public ICollection<ProgresoEjercicio> ProgresosEjercicios { get; private set; } = new List<ProgresoEjercicio>();
    public ICollection<InscripcionCurso> Inscripciones { get; private set; } = new List<InscripcionCurso>();
    public ICollection<EntregaEvaluacion> EntregasEvaluaciones { get; private set; } = new List<EntregaEvaluacion>();
    public ICollection<ProgresoObligatorio> ProgresosObligatorios { get; private set; } = new List<ProgresoObligatorio>();
    public ICollection<OpinionUsuario> Opiniones { get; private set; } = new List<OpinionUsuario>();
    public ICollection<CambioNombreUsuario> CambiosNombreUsuario { get; private set; } = new List<CambioNombreUsuario>();

    public void CambiarNombre(string nombre, string apellido)
    {
        Nombre = ValidarTexto(nombre, "El nombre", 80);
        Apellido = ValidarTexto(apellido, "El apellido", 80);
    }

    public void CambiarNombreUsuario(string valor) =>
        NombreUsuario = NormalizarNombreUsuario(valor);

    public static string NormalizarNombreUsuario(string valor)
    {
        string normalizado = ValidarTexto(valor, "El nombre de usuario", 50).ToLowerInvariant();
        if (normalizado.Length < 3)
            throw new ReglaNegocioException("El nombre de usuario debe tener al menos 3 caracteres.");

        bool caracteresValidos = normalizado.All(c =>
            char.IsLetterOrDigit(c) || c is '.' or '_' or '-');
        if (!caracteresValidos)
            throw new ReglaNegocioException(
                "El nombre de usuario solo puede contener letras, números, punto, guion o guion bajo.");

        if (!char.IsLetterOrDigit(normalizado[0]) ||
            !char.IsLetterOrDigit(normalizado[^1]))
            throw new ReglaNegocioException(
                "El nombre de usuario debe comenzar y terminar con una letra o un número.");

        return normalizado;
    }

    public void CambiarEmail(string valor)
    {
        var email = ValidarTexto(valor, "El correo electrónico", 150).ToLowerInvariant();
        if (!email.Contains('@')) throw new ReglaNegocioException("El correo electrónico no tiene un formato válido.");
        Email = email;
    }

    public void CambiarPasswordHash(string valor) => PasswordHash = ValidarTexto(valor, "La contraseña", 500);

    public void ActualizarPersonalizacion(
        string nombreUsuario,
        string avatarClave)
    {
        CambiarNombreUsuario(nombreUsuario);
        SeleccionarAvatar(avatarClave);
    }

    public void SeleccionarAvatar(string avatarClave) =>
        AvatarClave = CatalogoAvataresPerfil.Validar(avatarClave);

    public void CambiarFotoPerfil(string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            throw new ReglaNegocioException("La ruta de la foto de perfil es obligatoria.");

        ruta = ruta.Trim();
        string rutaNormalizada = ruta.Replace('\\', '/');
        string[] extensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];

        if (rutaNormalizada.Length > 260 ||
            !rutaNormalizada.StartsWith("/uploads/perfiles/", StringComparison.Ordinal) ||
            rutaNormalizada.Contains("..", StringComparison.Ordinal) ||
            !extensionesPermitidas.Any(ext =>
                rutaNormalizada.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ReglaNegocioException("La foto de perfil enviada no es válida.");
        }

        FotoPerfilRuta = rutaNormalizada;
    }

    public void QuitarFotoPerfil() => FotoPerfilRuta = null;
    public void RegistrarAcceso() => FechaUltimoAcceso = DateTime.UtcNow;
    public void CambiarVisibilidadRanking(bool mostrar) => MostrarEnRanking = mostrar;
    public void Bloquear() => Estado = EstadoUsuario.Bloqueado;
    public void Activar() => Estado = EstadoUsuario.Activo;

    private static string ValidarTexto(string valor, string campo, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor)) throw new ReglaNegocioException($"{campo} es obligatorio.");
        valor = valor.Trim();
        if (valor.Length > maximo) throw new ReglaNegocioException($"{campo} no puede superar {maximo} caracteres.");
        return valor;
    }
}

public class PerfilDocente : Entidad
{
    private PerfilDocente() { }
    public PerfilDocente(int usuarioId, string especialidad)
    {
        UsuarioId = usuarioId;
        Especialidad = especialidad.Trim();
        FechaAlta = DateTime.UtcNow;
    }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string Biografia { get; private set; } = string.Empty;
    public string Especialidad { get; private set; } = string.Empty;
    public bool Verificado { get; private set; }
    public DateTime FechaAlta { get; private set; }

    public void Verificar() => Verificado = true;

    public void ActualizarPresentacion(string especialidad, string biografia)
    {
        Especialidad = (especialidad ?? string.Empty).Trim();
        Biografia = (biografia ?? string.Empty).Trim();
    }
}

public class TokenActualizacion : Entidad
{
    private TokenActualizacion() { }
    public TokenActualizacion(int usuarioId, string token, DateTime fechaExpiracion)
    {
        UsuarioId = usuarioId;
        Token = token;
        FechaCreacion = DateTime.UtcNow;
        FechaExpiracion = fechaExpiracion;
    }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string Token { get; private set; } = string.Empty;
    public DateTime FechaCreacion { get; private set; }
    public DateTime FechaExpiracion { get; private set; }
    public bool Revocado { get; private set; }
    public DateTime? FechaRevocacion { get; private set; }

    public void Revocar()
    {
        Revocado = true;
        FechaRevocacion = DateTime.UtcNow;
    }
}
