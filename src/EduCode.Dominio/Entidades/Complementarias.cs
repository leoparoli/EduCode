using EduCode.Dominio.Enums;

namespace EduCode.Dominio.Entidades;

public class EstadisticaUsuario : Entidad
{
    private EstadisticaUsuario() { }
    public EstadisticaUsuario(int usuarioId) => UsuarioId = usuarioId;

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public int EjerciciosCompletados { get; private set; }
    public int EjerciciosAprobados { get; private set; }
    public decimal PuntajeTotal { get; private set; }
    public decimal PuntajePromedio { get; private set; }
    public int RachaActualDias { get; private set; }
    public int MejorRachaDias { get; private set; }
    public DateTime? FechaUltimaActividad { get; private set; }
}

public class Logro : Entidad
{
    private Logro() { }

    public Logro(string nombre, string descripcion, string icono,
        TipoLogro tipo, int valorObjetivo)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        Icono = icono;
        Tipo = tipo;
        ValorObjetivo = valorObjetivo;
        Activo = true;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string Icono { get; private set; } = string.Empty;
    public TipoLogro Tipo { get; private set; }
    public int ValorObjetivo { get; private set; }
    public bool Activo { get; private set; }

    public ICollection<UsuarioLogro> Usuarios { get; private set; } = new List<UsuarioLogro>();
}

public class UsuarioLogro : Entidad
{
    private UsuarioLogro() { }

    public UsuarioLogro(int usuarioId, int logroId)
    {
        UsuarioId = usuarioId;
        LogroId = logroId;
        FechaObtencion = DateTime.UtcNow;
    }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public int LogroId { get; private set; }
    public Logro Logro { get; private set; } = null!;
    public DateTime FechaObtencion { get; private set; }
}

public class NotaDocente : Entidad
{
    private NotaDocente() { }

    public NotaDocente(int docenteId, int alumnoId, string contenido,
        int? grupoId, bool privada = true)
    {
        DocenteId = docenteId;
        AlumnoId = alumnoId;
        Contenido = contenido;
        GrupoId = grupoId;
        Privada = privada;
        FechaCreacion = DateTime.UtcNow;
    }

    public int DocenteId { get; private set; }
    public Usuario Docente { get; private set; } = null!;
    public int AlumnoId { get; private set; }
    public Usuario Alumno { get; private set; } = null!;
    public int? GrupoId { get; private set; }
    public Grupo? Grupo { get; private set; }
    public string Contenido { get; private set; } = string.Empty;
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaModificacion { get; private set; }
    public bool Privada { get; private set; }
}

public class RevisionDocente : Entidad
{
    private RevisionDocente() { }

    public RevisionDocente(int docenteId, int intentoEjercicioId,
        string observacion, decimal? puntajeCorregido)
    {
        DocenteId = docenteId;
        IntentoEjercicioId = intentoEjercicioId;
        Observacion = observacion;
        PuntajeCorregido = puntajeCorregido;
        FechaRevision = DateTime.UtcNow;
    }

    public int DocenteId { get; private set; }
    public Usuario Docente { get; private set; } = null!;
    public int IntentoEjercicioId { get; private set; }
    public IntentoEjercicio IntentoEjercicio { get; private set; } = null!;
    public string Observacion { get; private set; } = string.Empty;
    public decimal? PuntajeCorregido { get; private set; }
    public DateTime FechaRevision { get; private set; }
}

public class RegistroAuditoria : Entidad
{
    private RegistroAuditoria() { }

    public RegistroAuditoria(int? usuarioId, TipoAccionAuditoria accion,
        string entidadAfectada, string entidadId, string datosAnteriores,
        string datosNuevos, string direccionIp)
    {
        UsuarioId = usuarioId;
        Accion = accion;
        EntidadAfectada = entidadAfectada;
        EntidadId = entidadId;
        DatosAnteriores = datosAnteriores;
        DatosNuevos = datosNuevos;
        DireccionIp = direccionIp;
        Fecha = DateTime.UtcNow;
    }

    public int? UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public TipoAccionAuditoria Accion { get; private set; }
    public string EntidadAfectada { get; private set; } = string.Empty;
    public string EntidadId { get; private set; } = string.Empty;
    public string DatosAnteriores { get; private set; } = string.Empty;
    public string DatosNuevos { get; private set; } = string.Empty;
    public string DireccionIp { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
}

public class SolicitudIA : Entidad
{
    private SolicitudIA() { }

    public SolicitudIA(int usuarioId, int? intentoEjercicioId, string proveedor, string modelo)
    {
        UsuarioId = usuarioId;
        IntentoEjercicioId = intentoEjercicioId;
        Proveedor = proveedor;
        Modelo = modelo;
        Estado = EstadoSolicitudIA.Pendiente;
        FechaSolicitud = DateTime.UtcNow;
    }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public int? IntentoEjercicioId { get; private set; }
    public IntentoEjercicio? IntentoEjercicio { get; private set; }
    public string Proveedor { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public int TokensEntrada { get; private set; }
    public int TokensSalida { get; private set; }
    public decimal? CostoEstimado { get; private set; }
    public EstadoSolicitudIA Estado { get; private set; }
    public string MensajeError { get; private set; } = string.Empty;
    public DateTime FechaSolicitud { get; private set; }
    public DateTime? FechaRespuesta { get; private set; }

    public void MarcarExitosa(int entrada, int salida, decimal? costo)
    {
        TokensEntrada = entrada;
        TokensSalida = salida;
        CostoEstimado = costo;
        Estado = EstadoSolicitudIA.Exitosa;
        FechaRespuesta = DateTime.UtcNow;
    }

    public void MarcarFallida(string mensaje)
    {
        MensajeError = mensaje;
        Estado = EstadoSolicitudIA.Fallida;
        FechaRespuesta = DateTime.UtcNow;
    }
}
