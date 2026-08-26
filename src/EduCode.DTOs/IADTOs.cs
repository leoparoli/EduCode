namespace EduCode.DTOs;

public sealed class SolicitudCorreccionIADTO
{
    public int UsuarioId { get; set; }
    public int IntentoId { get; set; }
    public string TituloEjercicio { get; set; } = string.Empty;
    public string Consigna { get; set; } = string.Empty;
    public string CodigoAlumno { get; set; } = string.Empty;
    public string Lenguaje { get; set; } = string.Empty;
    public string NivelDificultad { get; set; } = string.Empty;
    public int PuntajeMinimoAprobacion { get; set; } = 70;
    public string VersionRubrica { get; set; } = "rubrica-2.0";
    public List<CriterioCorreccionIADTO> Criterios { get; set; } = [];
    public List<string> ConceptosPermitidos { get; set; } = [];
    public List<string> ConceptosNoPermitidos { get; set; } = [];
    public List<CasoPruebaCorreccionIADTO> CasosPrueba { get; set; } = [];
    public int NumeroIntento { get; set; }

    // Contexto técnico ya calculado por EduCode. Los proveedores de IA
    // lo usan únicamente para redactar una explicación pedagógica.
    public decimal? PuntajeTecnicoExacto { get; set; }
    public bool? AprobadoTecnico { get; set; }
    public string ResumenTecnico { get; set; } = string.Empty;
    public string MotivoNoAprobacionTecnico { get; set; } = string.Empty;
    public List<ResultadoCasoPruebaDTO> ResultadosCasosTecnicos { get; set; } = [];
    public List<HallazgoTecnicoDTO> HallazgosTecnicos { get; set; } = [];
    public List<ResultadoCriterioTecnicoDTO> ResultadosCriteriosTecnicos { get; set; } = [];
}

public sealed class CriterioCorreccionIADTO
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Peso { get; set; }
    public bool Obligatorio { get; set; }
    public int PorcentajeMinimoObligatorio { get; set; }
}

public sealed class CasoPruebaCorreccionIADTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Entrada { get; set; } = string.Empty;
    public string SalidaEsperada { get; set; } = string.Empty;
    public bool EsOculto { get; set; }
}

public sealed class RespuestaCorreccionIADTO
{
    public string Resultado { get; set; } = string.Empty;
    public string Resumen { get; set; } = string.Empty;
    public List<string> AspectosCorrectos { get; set; } = [];
    public List<string> AspectosAMejorar { get; set; } = [];
    public string Pista { get; set; } = string.Empty;
    public bool RequiereRevisionDocente { get; set; }
    public List<ResultadoCriterioIADTO> ResultadosCriterios { get; set; } = [];
    public string Modelo { get; set; } = string.Empty;
    public string VersionPrompt { get; set; } = "1.0";
    public int TokensEntrada { get; set; }
    public int TokensSalida { get; set; }
    public bool EsProvisoria { get; set; }
}

public sealed class ResultadoCriterioIADTO
{
    public int CriterioId { get; set; }
    public int PorcentajeCumplimiento { get; set; }
    public string Evidencia { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
}


public sealed class ResultadoEvaluacionLocalDTO
{
    public RespuestaCorreccionIADTO CorreccionBase { get; set; } = new();
    public int CasosPruebaSuperados { get; set; }
    public int TotalCasosPrueba { get; set; }
    public List<ResultadoCasoPruebaDTO> ResultadosCasosPrueba { get; set; } = [];
    public List<HallazgoTecnicoDTO> HallazgosTecnicos { get; set; } = [];
}

public sealed class ResultadoCasoPruebaDTO
{
    public string Nombre { get; set; } = string.Empty;
    public bool Superado { get; set; }
    public bool EsOculto { get; set; }
    public string SalidaEsperada { get; set; } = string.Empty;
    public string SalidaObtenida { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

public sealed class HallazgoTecnicoDTO
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Critico { get; set; }
}


public sealed class ResultadoCriterioTecnicoDTO
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

// ======================================================
// TUTOR IA DE PROGRAMACIÓN 1 · VERSIÓN 2.9.4
// ======================================================

public sealed class SolicitudTutorIADTO
{
    public int UsuarioId { get; set; }
    public string Consulta { get; set; } = string.Empty;
    public List<MensajeTutorIADTO> Historial { get; set; } = [];
}

public sealed class MensajeTutorIADTO
{
    public string Rol { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
}

public sealed class RespuestaTutorIADTO
{
    public bool Aceptada { get; set; }
    public string Respuesta { get; set; } = string.Empty;
    public string MotivoRechazo { get; set; } = string.Empty;
    public int ConsultasRestantes { get; set; }
    public int LimiteDiario { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public bool UsoProveedorIA { get; set; }
    public bool EsFallbackLocal { get; set; }
    public bool RequiereMasContexto { get; set; }
    public string CodigoResultado { get; set; } = string.Empty;
    public string Clasificacion { get; set; } = string.Empty;
    public List<string> TemasDetectados { get; set; } = [];
    public string Advertencia { get; set; } = string.Empty;
    public List<string> Sugerencias { get; set; } = [];
}

public sealed class EstadoTutorIADTO
{
    public bool Habilitado { get; set; }
    public int LimiteDiario { get; set; }
    public int ConsultasUtilizadas { get; set; }
    public int ConsultasRestantes { get; set; }
    public int MaximoCaracteresConsulta { get; set; }
    public List<string> TemasHabilitados { get; set; } = [];
}

