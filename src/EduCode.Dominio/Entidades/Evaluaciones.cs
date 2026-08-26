using EduCode.Dominio.Enums;

namespace EduCode.Dominio.Entidades;

public class Evaluacion : Entidad
{
    private Evaluacion() { }

    public Evaluacion(string titulo, string descripcion, TipoEvaluacion tipo,
        int anio, Semestre semestre, string letra, int duracionMinutos,
        int puntajeTotal, int puntajeMinimoAprobacion, int cursoId, int usuarioCreadorId)
    {
        Titulo = titulo;
        Descripcion = descripcion;
        Tipo = tipo;
        Anio = anio;
        Semestre = semestre;
        Letra = letra;
        DuracionMinutos = duracionMinutos;
        PuntajeTotal = puntajeTotal;
        PuntajeMinimoAprobacion = puntajeMinimoAprobacion;
        CursoId = cursoId;
        UsuarioCreadorId = usuarioCreadorId;
    }

    public string Titulo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public TipoEvaluacion Tipo { get; private set; }
    public int Anio { get; private set; }
    public Semestre Semestre { get; private set; }
    public string Letra { get; private set; } = string.Empty;
    public DateTime? FechaOriginal { get; private set; }
    public int DuracionMinutos { get; private set; }
    public int PuntajeTotal { get; private set; }
    public int PuntajeMinimoAprobacion { get; private set; }
    public string RutaDocumentoPdf { get; private set; } = string.Empty;
    public bool Publicada { get; private set; }
    public bool PerteneceCursoActual { get; private set; }
    public int CursoId { get; private set; }
    public Curso Curso { get; private set; } = null!;
    public int UsuarioCreadorId { get; private set; }
    public Usuario UsuarioCreador { get; private set; } = null!;

    public ICollection<EvaluacionEjercicio> Ejercicios { get; private set; } = new List<EvaluacionEjercicio>();
    public ICollection<EntregaEvaluacion> Entregas { get; private set; } = new List<EntregaEvaluacion>();
}

public class EvaluacionEjercicio : Entidad
{
    private EvaluacionEjercicio() { }

    public EvaluacionEjercicio(int evaluacionId, int ejercicioId, int numeroConsigna, int puntaje, int orden)
    {
        EvaluacionId = evaluacionId;
        EjercicioId = ejercicioId;
        NumeroConsigna = numeroConsigna;
        Puntaje = puntaje;
        Orden = orden;
    }

    public int EvaluacionId { get; private set; }
    public Evaluacion Evaluacion { get; private set; } = null!;
    public int EjercicioId { get; private set; }
    public Ejercicio Ejercicio { get; private set; } = null!;
    public int NumeroConsigna { get; private set; }
    public int Puntaje { get; private set; }
    public int Orden { get; private set; }
}

public class EntregaEvaluacion : Entidad
{
    private EntregaEvaluacion() { }

    public EntregaEvaluacion(int evaluacionId, int usuarioId, ModalidadEvaluacion modalidad)
    {
        EvaluacionId = evaluacionId;
        UsuarioId = usuarioId;
        Modalidad = modalidad;
        Estado = EstadoEntregaEvaluacion.EnCurso;
        FechaInicio = DateTime.UtcNow;
    }

    public int EvaluacionId { get; private set; }
    public Evaluacion Evaluacion { get; private set; } = null!;
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public ModalidadEvaluacion Modalidad { get; private set; }
    public EstadoEntregaEvaluacion Estado { get; private set; }
    public DateTime FechaInicio { get; private set; }
    public DateTime? FechaFinalizacion { get; private set; }
    public decimal? PuntajeObtenido { get; private set; }
    public bool Aprobada { get; private set; }

    public ICollection<RespuestaEvaluacion> Respuestas { get; private set; } = new List<RespuestaEvaluacion>();
}

public class RespuestaEvaluacion : Entidad
{
    private RespuestaEvaluacion() { }

    public RespuestaEvaluacion(int entregaEvaluacionId, int evaluacionEjercicioId, string respuesta)
    {
        EntregaEvaluacionId = entregaEvaluacionId;
        EvaluacionEjercicioId = evaluacionEjercicioId;
        Respuesta = respuesta;
        FechaUltimaModificacion = DateTime.UtcNow;
    }

    public int EntregaEvaluacionId { get; private set; }
    public EntregaEvaluacion EntregaEvaluacion { get; private set; } = null!;
    public int EvaluacionEjercicioId { get; private set; }
    public EvaluacionEjercicio EvaluacionEjercicio { get; private set; } = null!;
    public string Respuesta { get; private set; } = string.Empty;
    public DateTime? FechaUltimaModificacion { get; private set; }
    public decimal? PuntajeObtenido { get; private set; }
    public string Feedback { get; private set; } = string.Empty;
}
