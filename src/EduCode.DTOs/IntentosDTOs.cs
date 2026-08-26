namespace EduCode.DTOs;

public sealed class EnviarIntentoDTO
{
    public int EjercicioId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int? TiempoResolucionSegundos { get; set; }
}

public sealed class ResultadoIntentoDTO
{
    public int IntentoId { get; set; }
    public int NumeroIntento { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal PuntajeExacto { get; set; }
    public int PuntajeMostrado { get; set; }
    public bool EjercicioAprobado { get; set; }
    public bool CumpleCriteriosObligatorios { get; set; }
    public string MotivoNoAprobacion { get; set; } = string.Empty;
    public string Resumen { get; set; } = string.Empty;
    public List<string> AspectosCorrectos { get; set; } = [];
    public List<string> AspectosAMejorar { get; set; } = [];
    public string Pista { get; set; } = string.Empty;
    public string VersionRubrica { get; set; } = string.Empty;
    public string VersionMotorPuntaje { get; set; } = string.Empty;
    public List<ResultadoCriterioDTO> Criterios { get; set; } = [];
    public bool CorreccionPendiente { get; set; }
    public bool EsCorreccionProvisoria { get; set; }
    public bool PuedeReintentar { get; set; }
    public bool AfectaProgreso { get; set; } = true;
    public string MensajeCorreccion { get; set; } = string.Empty;
    public bool FeedbackPersonalizadoPendiente { get; set; }
    public bool FeedbackPersonalizadoDisponible { get; set; }
    public string ProveedorFeedback { get; set; } = string.Empty;
    public int CasosPruebaSuperados { get; set; }
    public int TotalCasosPrueba { get; set; }
    public List<ResultadoCasoPruebaDTO> ResultadosCasosPrueba { get; set; } = [];
    public List<HallazgoTecnicoDTO> HallazgosTecnicos { get; set; } = [];
    public List<LogroDesbloqueadoDTO> NuevosLogros { get; set; } = [];
}

public sealed class ResultadoCriterioDTO
{
    public int CriterioId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int PorcentajeCumplimiento { get; set; }
    public decimal PesoMaximo { get; set; }
    public decimal PuntajeObtenido { get; set; }
    public bool Obligatorio { get; set; }
    public int PorcentajeMinimoObligatorio { get; set; }
    public bool CumplimientoObligatorioAlcanzado { get; set; }
    public string Evidencia { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
}

public sealed class IntentoHistorialDTO
{
    public int Id { get; set; }
    public int NumeroIntento { get; set; }
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal? Puntaje { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public bool PuedeReintentar { get; set; }
    public bool EsCorreccionProvisoria { get; set; }
    public string MensajeEstado { get; set; } = string.Empty;
}

public sealed class PistaDTO
{
    public int Id { get; set; }
    public int Nivel { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public decimal PenalizacionPuntaje { get; set; }
}

public sealed class ProgresoUsuarioDTO
{
    public int TotalEjercicios { get; set; }
    public int Aprobados { get; set; }
    public int EnProceso { get; set; }
    public decimal PorcentajeGeneral { get; set; }
    public List<ProgresoEjercicioDTO> Ejercicios { get; set; } = [];
}

public sealed class ProgresoEjercicioDTO
{
    public int EjercicioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int CantidadIntentos { get; set; }
    public decimal MejorPuntaje { get; set; }
    public DateTime? UltimaActividad { get; set; }
}
