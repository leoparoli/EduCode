namespace EduCode.DTOs;

public sealed class PerfilUsuarioDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string AvatarClave { get; set; } = "codigo";
    public string FotoPerfilRuta { get; set; } = string.Empty;
    public int MaximoCambiosNombreUsuario { get; set; }
    public int DiasVentanaCambiosNombreUsuario { get; set; }
    public int CambiosNombreUsuarioUsados { get; set; }
    public int CambiosNombreUsuarioDisponibles { get; set; }
    public bool PuedeCambiarNombreUsuario { get; set; }
    public DateTime? ProximoCambioNombreUsuarioDisponible { get; set; }
    public List<AvatarDisponibleDTO> AvataresDisponibles { get; set; } = [];
}

public sealed class AvatarDisponibleDTO
{
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
}

public sealed class ActualizarPerfilDTO
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string AvatarClave { get; set; } = "codigo";
    public string? NuevaFotoPerfilRuta { get; set; }
    public bool EliminarFotoPerfil { get; set; }
}

public sealed class CrearOpinionDTO
{
    public int Categoria { get; set; }
    public int Calificacion { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public bool AutorizaPublicacion { get; set; }
}

public sealed class OpinionUsuarioDTO
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreVisible { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string AvatarClave { get; set; } = "codigo";
    public string FotoPerfilRuta { get; set; } = string.Empty;
    public int Categoria { get; set; }
    public string CategoriaTexto { get; set; } = string.Empty;
    public int Calificacion { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public bool AutorizaPublicacion { get; set; }
    public int Estado { get; set; }
    public string EstadoTexto { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaRevision { get; set; }
}

public sealed class ActualizarEstadoOpinionDTO
{
    public int Estado { get; set; }
}
