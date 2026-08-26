using EduCode.Dominio.Enums;

namespace EduCode.Dominio.Entidades;

public class Obligatorio : Entidad
{
    private Obligatorio() { }

    public Obligatorio(string titulo, string descripcion, int anio,
        Semestre semestre, int cursoId, int usuarioCreadorId)
    {
        Titulo = titulo;
        Descripcion = descripcion;
        Anio = anio;
        Semestre = semestre;
        CursoId = cursoId;
        UsuarioCreadorId = usuarioCreadorId;
    }

    public string Titulo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public int Anio { get; private set; }
    public Semestre Semestre { get; private set; }
    public string RutaDocumentoPdf { get; private set; } = string.Empty;
    public bool Publicado { get; private set; }
    public bool PerteneceCursoActual { get; private set; }
    public int CursoId { get; private set; }
    public Curso Curso { get; private set; } = null!;
    public int UsuarioCreadorId { get; private set; }
    public Usuario UsuarioCreador { get; private set; } = null!;

    public ICollection<EtapaObligatorio> Etapas { get; private set; } = new List<EtapaObligatorio>();
    public ICollection<ProgresoObligatorio> Progresos { get; private set; } = new List<ProgresoObligatorio>();
}

public class EtapaObligatorio : Entidad
{
    private EtapaObligatorio() { }

    public EtapaObligatorio(int obligatorioId, string titulo, string descripcion,
        string instruccion, TipoEtapaObligatorio tipo, int orden, int puntaje,
        bool requiereAprobacion)
    {
        ObligatorioId = obligatorioId;
        Titulo = titulo;
        Descripcion = descripcion;
        Instruccion = instruccion;
        Tipo = tipo;
        Orden = orden;
        Puntaje = puntaje;
        RequiereAprobacion = requiereAprobacion;
        Activa = true;
    }

    public int ObligatorioId { get; private set; }
    public Obligatorio Obligatorio { get; private set; } = null!;
    public string Titulo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Instruccion { get; private set; } = string.Empty;
    public TipoEtapaObligatorio Tipo { get; private set; }
    public int Orden { get; private set; }
    public int Puntaje { get; private set; }
    public bool RequiereAprobacion { get; private set; }
    public bool Activa { get; private set; }

    public ICollection<TareaEtapa> Tareas { get; private set; } = new List<TareaEtapa>();
}

public class TareaEtapa : Entidad
{
    private TareaEtapa() { }

    public TareaEtapa(int etapaObligatorioId, string titulo, string descripcion,
        int orden, bool obligatoria)
    {
        EtapaObligatorioId = etapaObligatorioId;
        Titulo = titulo;
        Descripcion = descripcion;
        Orden = orden;
        Obligatoria = obligatoria;
    }

    public int EtapaObligatorioId { get; private set; }
    public EtapaObligatorio EtapaObligatorio { get; private set; } = null!;
    public string Titulo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public int Orden { get; private set; }
    public bool Obligatoria { get; private set; }
}

public class ProgresoObligatorio : Entidad
{
    private ProgresoObligatorio() { }

    public ProgresoObligatorio(int usuarioId, int obligatorioId, int etapaActualId)
    {
        UsuarioId = usuarioId;
        ObligatorioId = obligatorioId;
        EtapaActualId = etapaActualId;
        Estado = EstadoProgresoObligatorio.NoIniciado;
    }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public int ObligatorioId { get; private set; }
    public Obligatorio Obligatorio { get; private set; } = null!;
    public EstadoProgresoObligatorio Estado { get; private set; }
    public DateTime? FechaInicio { get; private set; }
    public DateTime? FechaFinalizacion { get; private set; }
    public DateTime? FechaUltimaActividad { get; private set; }
    public int EtapaActualId { get; private set; }
    public EtapaObligatorio EtapaActual { get; private set; } = null!;
    public decimal PorcentajeAvance { get; private set; }

    public ICollection<EntregaEtapaObligatorio> EntregasEtapas { get; private set; } =
        new List<EntregaEtapaObligatorio>();
}

public class EntregaEtapaObligatorio : Entidad
{
    private EntregaEtapaObligatorio() { }

    public EntregaEtapaObligatorio(int progresoObligatorioId, int etapaObligatorioId,
        string respuesta, string codigo)
    {
        ProgresoObligatorioId = progresoObligatorioId;
        EtapaObligatorioId = etapaObligatorioId;
        Respuesta = respuesta;
        Codigo = codigo;
        FechaEntrega = DateTime.UtcNow;
    }

    public int ProgresoObligatorioId { get; private set; }
    public ProgresoObligatorio ProgresoObligatorio { get; private set; } = null!;
    public int EtapaObligatorioId { get; private set; }
    public EtapaObligatorio EtapaObligatorio { get; private set; } = null!;
    public string Respuesta { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;
    public DateTime FechaEntrega { get; private set; }
    public bool Aprobada { get; private set; }
    public decimal PuntajeObtenido { get; private set; }
    public string Feedback { get; private set; } = string.Empty;
}
