using EduCode.Dominio.Enums;
using EduCode.Dominio.Excepciones;

namespace EduCode.Dominio.Entidades;

public static class CatalogoAvataresPerfil
{
    public const string Predeterminado = "codigo";

    private static readonly HashSet<string> Permitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "codigo", "terminal", "cohete", "robot",
        "cerebro", "laptop", "bug", "braces"
    };

    public static string Validar(string? clave)
    {
        string normalizada = string.IsNullOrWhiteSpace(clave)
            ? Predeterminado
            : clave.Trim().ToLowerInvariant();

        return Permitidos.Contains(normalizada)
            ? normalizada
            : throw new ReglaNegocioException("El avatar seleccionado no está disponible.");
    }

    public static IReadOnlyCollection<string> ObtenerTodos() =>
        Permitidos.OrderBy(x => x).ToArray();
}

public class OpinionUsuario : Entidad
{
    private OpinionUsuario() { }

    public OpinionUsuario(
        int usuarioId,
        CategoriaOpinion categoria,
        int calificacion,
        string comentario,
        bool autorizaPublicacion)
    {
        if (usuarioId <= 0)
            throw new ReglaNegocioException("No se pudo identificar al usuario de la opinión.");
        if (!Enum.IsDefined(typeof(CategoriaOpinion), categoria))
            throw new ReglaNegocioException("Seleccioná una categoría válida.");
        if (calificacion is < 1 or > 5)
            throw new ReglaNegocioException("La calificación debe estar entre 1 y 5 estrellas.");

        comentario = (comentario ?? string.Empty).Trim();
        if (comentario.Length < 20)
            throw new ReglaNegocioException("La opinión debe tener al menos 20 caracteres.");
        if (comentario.Length > 1200)
            throw new ReglaNegocioException("La opinión no puede superar 1200 caracteres.");

        UsuarioId = usuarioId;
        Categoria = categoria;
        Calificacion = calificacion;
        Comentario = comentario;
        AutorizaPublicacion = autorizaPublicacion;
        Estado = EstadoOpinion.Pendiente;
        FechaCreacion = DateTime.UtcNow;
    }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public CategoriaOpinion Categoria { get; private set; }
    public int Calificacion { get; private set; }
    public string Comentario { get; private set; } = string.Empty;
    public bool AutorizaPublicacion { get; private set; }
    public EstadoOpinion Estado { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaRevision { get; private set; }

    public void CambiarEstado(EstadoOpinion estado)
    {
        if (!Enum.IsDefined(typeof(EstadoOpinion), estado))
            throw new ReglaNegocioException("El estado indicado no es válido.");

        Estado = estado;
        FechaRevision = DateTime.UtcNow;
    }
}
