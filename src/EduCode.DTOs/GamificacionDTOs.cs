namespace EduCode.DTOs;

public sealed class ResumenGamificacionDTO
{
    public bool VisibleEnRanking { get; set; }
    public int? PosicionRanking { get; set; }
    public decimal PuntajeRanking { get; set; }
    public decimal PuntajeMaximoDisponible { get; set; }
    public decimal PromedioMejoresPuntajes { get; set; }
    public int EjerciciosIntentados { get; set; }
    public int EjerciciosAprobados { get; set; }
    public RachaResolucionDTO RachaActual { get; set; } = new();
    public RachaResolucionDTO MejorRacha { get; set; } = new();
    public List<LogroUsuarioDTO> Insignias { get; set; } = [];
    public List<PosicionRankingDTO> Ranking { get; set; } = [];
}

public sealed class RachaResolucionDTO
{
    public int CantidadEjercicios { get; set; }
    public decimal Promedio { get; set; }
}

public sealed class PosicionRankingDTO
{
    public int Posicion { get; set; }
    public string NombreVisible { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string AvatarClave { get; set; } = "codigo";
    public string FotoPerfilRuta { get; set; } = string.Empty;
    public decimal PuntajeTotal { get; set; }
    public decimal Promedio { get; set; }
    public int EjerciciosAprobados { get; set; }
    public int EjerciciosIntentados { get; set; }
    public bool EsUsuarioActual { get; set; }
}

public sealed class LogroUsuarioDTO
{
    public int LogroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int ValorObjetivo { get; set; }
    public int ValorActual { get; set; }
    public int PorcentajeProgreso { get; set; }
    public bool Obtenido { get; set; }
    public DateTime? FechaObtencion { get; set; }
}

public sealed class LogroDesbloqueadoDTO
{
    public int LogroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
}

public sealed class CambiarVisibilidadRankingDTO
{
    public bool Mostrar { get; set; }
}
