using EduCode.Dominio.Enums;

namespace EduCode.DTOs;

public sealed class CursoListadoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int CantidadTemas { get; set; }
}

public sealed class AltaCursoDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public sealed class AltaTemaDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int CursoId { get; set; }
    public int? TemaPrerequisitoId { get; set; }
}

public sealed class TemaListadoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Orden { get; set; }
    public int CantidadEjercicios { get; set; }
    public int EjerciciosCompletados { get; set; }
    public decimal PorcentajeProgreso { get; set; }
    public bool Bloqueado { get; set; }
}

public sealed class AltaEjercicioDTO
{
    public string Titulo { get; set; } = string.Empty;
    public string Consigna { get; set; } = string.Empty;
    public TipoEjercicio Tipo { get; set; } = TipoEjercicio.Codigo;
    public NivelDificultad Dificultad { get; set; }
    public int PuntajeMaximo { get; set; }
    public int PuntajeMinimoAprobacion { get; set; }
    public int Orden { get; set; }
    public string Lenguaje { get; set; } = "JavaScript";
    public string CodigoInicial { get; set; } = string.Empty;
    public int TemaId { get; set; }
    public int? EjercicioPrerequisitoId { get; set; }
    public bool PermitePistas { get; set; } = true;
    public bool PermiteVerSolucion { get; set; }
    public int CantidadIntentosRecomendada { get; set; } = 4;
    public string VersionRubrica { get; set; } = "rubrica-2.0";
    public List<AltaCriterioEvaluacionDTO> Criterios { get; set; } = [];
    public List<AltaPistaDTO> Pistas { get; set; } = [];
    public List<AltaConceptoDTO> ConceptosPermitidos { get; set; } = [];
    public List<AltaConceptoDTO> ConceptosNoPermitidos { get; set; } = [];
    public List<AltaCasoPruebaDTO> CasosPrueba { get; set; } = [];
}

public sealed class AltaCriterioEvaluacionDTO
{
    public string Descripcion { get; set; } = string.Empty;
    public TipoCriterio Tipo { get; set; }
    public decimal Peso { get; set; }
    public bool Obligatorio { get; set; }
    public int PorcentajeMinimoObligatorio { get; set; } = 60;
    public int Orden { get; set; }
}

public sealed class AltaPistaDTO
{
    public int Nivel { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public int IntentosMinimosRequeridos { get; set; }
    public decimal PenalizacionPuntaje { get; set; }
}

public sealed class AltaConceptoDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string DescripcionOMotivo { get; set; } = string.Empty;
}

public sealed class AltaCasoPruebaDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Entrada { get; set; } = string.Empty;
    public string SalidaEsperada { get; set; } = string.Empty;
    public VisibilidadCasoPrueba Visibilidad { get; set; }
    public decimal Peso { get; set; }
    public int Orden { get; set; }
}

public sealed class EjercicioListadoDTO
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public int PuntajeMaximo { get; set; }
    public int Orden { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal MejorPuntaje { get; set; }
    public int CantidadIntentos { get; set; }
    public bool Bloqueado { get; set; }
}

public sealed class EjercicioDetalleDTO
{
    public int Id { get; set; }
    public int TemaId { get; set; }
    public string TemaNombre { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Consigna { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public string Lenguaje { get; set; } = string.Empty;
    public string CodigoInicial { get; set; } = string.Empty;
    public int PuntajeMaximo { get; set; }
    public int PuntajeMinimoAprobacion { get; set; }
    public int CantidadIntentosRealizados { get; set; }
    public bool PermiteSolicitarPista { get; set; }
    public int? SiguienteEjercicioId { get; set; }
    public string SiguienteEjercicioTitulo { get; set; } = string.Empty;
    public bool HaySiguienteEjercicioEnTema { get; set; }
    public List<CriterioPublicoDTO> Criterios { get; set; } = [];
}

public sealed class CriterioPublicoDTO
{
    public string Descripcion { get; set; } = string.Empty;
    public decimal Peso { get; set; }
    public bool Obligatorio { get; set; }
    public int PorcentajeMinimoObligatorio { get; set; }
}
