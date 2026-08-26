using EduCode.Dominio.Enums;
using EduCode.Dominio.Excepciones;

namespace EduCode.Dominio.Entidades;

public class Curso : Entidad
{
    private Curso() { }

    public Curso(string nombre, string codigo, string descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(codigo))
            throw new ReglaNegocioException("El nombre y el código del curso son obligatorios.");

        Nombre = nombre.Trim();
        Codigo = codigo.Trim().ToUpperInvariant();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public bool Activo { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    public ICollection<Tema> Temas { get; private set; } = new List<Tema>();
    public ICollection<Grupo> Grupos { get; private set; } = new List<Grupo>();
    public ICollection<Evaluacion> Evaluaciones { get; private set; } = new List<Evaluacion>();
    public ICollection<Obligatorio> Obligatorios { get; private set; } = new List<Obligatorio>();
}

public class PeriodoAcademico : Entidad
{
    private PeriodoAcademico() { }

    public PeriodoAcademico(int anio, Semestre semestre, DateTime fechaInicio, DateTime fechaFin)
    {
        if (fechaFin <= fechaInicio) throw new ReglaNegocioException("La fecha final debe ser posterior a la inicial.");
        Anio = anio;
        Semestre = semestre;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Activo = true;
    }

    public int Anio { get; private set; }
    public Semestre Semestre { get; private set; }
    public DateTime FechaInicio { get; private set; }
    public DateTime FechaFin { get; private set; }
    public bool Activo { get; private set; }

    public ICollection<Grupo> Grupos { get; private set; } = new List<Grupo>();
}

public class Grupo : Entidad
{
    private Grupo() { }

    public Grupo(string nombre, int cursoId, int periodoAcademicoId, int? docenteResponsableId = null)
    {
        Nombre = nombre.Trim();
        CursoId = cursoId;
        PeriodoAcademicoId = periodoAcademicoId;
        DocenteResponsableId = docenteResponsableId;
        Activo = true;
    }

    public string Nombre { get; private set; } = string.Empty;
    public int CursoId { get; private set; }
    public Curso Curso { get; private set; } = null!;
    public int PeriodoAcademicoId { get; private set; }
    public PeriodoAcademico PeriodoAcademico { get; private set; } = null!;
    public int? DocenteResponsableId { get; private set; }
    public Usuario? DocenteResponsable { get; private set; }
    public bool Activo { get; private set; }

    public ICollection<InscripcionCurso> Inscripciones { get; private set; } = new List<InscripcionCurso>();
}

public class InscripcionCurso : Entidad
{
    private InscripcionCurso() { }

    public InscripcionCurso(int usuarioId, int grupoId)
    {
        UsuarioId = usuarioId;
        GrupoId = grupoId;
        FechaInscripcion = DateTime.UtcNow;
        Estado = EstadoInscripcion.Activa;
    }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public int GrupoId { get; private set; }
    public Grupo Grupo { get; private set; } = null!;
    public DateTime FechaInscripcion { get; private set; }
    public EstadoInscripcion Estado { get; private set; }
    public decimal PorcentajeProgreso { get; private set; }
    public decimal PuntajeAcumulado { get; private set; }
}

public class Tema : Entidad
{
    private Tema() { }

    public Tema(string nombre, string descripcion, int orden, int cursoId, int? temaPrerequisitoId = null)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaNegocioException("El nombre del tema es obligatorio.");
        if (orden <= 0) throw new ReglaNegocioException("El orden del tema debe ser mayor que cero.");

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        Orden = orden;
        CursoId = cursoId;
        TemaPrerequisitoId = temaPrerequisitoId;
        Activo = true;
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public int Orden { get; private set; }
    public bool Activo { get; private set; }
    public int CursoId { get; private set; }
    public Curso Curso { get; private set; } = null!;
    public int? TemaPrerequisitoId { get; private set; }
    public Tema? TemaPrerequisito { get; private set; }
    public ICollection<Tema> TemasDependientes { get; private set; } = new List<Tema>();
    public ICollection<Ejercicio> Ejercicios { get; private set; } = new List<Ejercicio>();

    public void Editar(string nombre, string descripcion, int orden)
    {
        Nombre = nombre.Trim();
        Descripcion = descripcion.Trim();
        Orden = orden;
    }

    public void Desactivar() => Activo = false;
}

public class Ejercicio : Entidad
{
    private Ejercicio() { }

    public Ejercicio(
        string titulo,
        string consigna,
        TipoEjercicio tipo,
        NivelDificultad dificultad,
        int puntajeMaximo,
        int puntajeMinimoAprobacion,
        int orden,
        string lenguaje,
        string codigoInicial,
        int temaId,
        int usuarioCreadorId,
        int? ejercicioPrerequisitoId = null)
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(consigna))
            throw new ReglaNegocioException("El título y la consigna son obligatorios.");

        if (puntajeMaximo <= 0 || puntajeMinimoAprobacion < 0 || puntajeMinimoAprobacion > puntajeMaximo)
            throw new ReglaNegocioException("Los puntajes del ejercicio no son válidos.");

        Titulo = titulo.Trim();
        Consigna = consigna.Trim();
        Tipo = tipo;
        Dificultad = dificultad;
        PuntajeMaximo = puntajeMaximo;
        PuntajeMinimoAprobacion = puntajeMinimoAprobacion;
        Orden = orden;
        Lenguaje = lenguaje.Trim();
        CodigoInicial = codigoInicial ?? string.Empty;
        TemaId = temaId;
        UsuarioCreadorId = usuarioCreadorId;
        EjercicioPrerequisitoId = ejercicioPrerequisitoId;
        CantidadIntentosRecomendada = 4;
        PermitePistas = true;
        PermiteVerSolucion = false;
        Activo = true;
        VersionRubrica = "rubrica-2.0";
        FechaCreacion = DateTime.UtcNow;
    }

    public string Titulo { get; private set; } = string.Empty;
    public string Consigna { get; private set; } = string.Empty;
    public TipoEjercicio Tipo { get; private set; }
    public NivelDificultad Dificultad { get; private set; }
    public int PuntajeMaximo { get; private set; }
    public int Orden { get; private set; }
    public string Lenguaje { get; private set; } = string.Empty;
    public string CodigoInicial { get; private set; } = string.Empty;
    public int CantidadIntentosRecomendada { get; private set; }
    public int PuntajeMinimoAprobacion { get; private set; }
    public bool PermitePistas { get; private set; }
    public bool PermiteVerSolucion { get; private set; }
    public bool Activo { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaModificacion { get; private set; }
    public string VersionRubrica { get; private set; } = "rubrica-2.0";

    public int TemaId { get; private set; }
    public Tema Tema { get; private set; } = null!;
    public int? EjercicioPrerequisitoId { get; private set; }
    public Ejercicio? EjercicioPrerequisito { get; private set; }
    public ICollection<Ejercicio> EjerciciosDependientes { get; private set; } = new List<Ejercicio>();
    public int UsuarioCreadorId { get; private set; }
    public Usuario UsuarioCreador { get; private set; } = null!;

    public ICollection<CriterioEvaluacion> CriteriosEvaluacion { get; private set; } = new List<CriterioEvaluacion>();
    public ICollection<ConceptoPermitido> ConceptosPermitidos { get; private set; } = new List<ConceptoPermitido>();
    public ICollection<ConceptoNoPermitido> ConceptosNoPermitidos { get; private set; } = new List<ConceptoNoPermitido>();
    public ICollection<PistaEjercicio> Pistas { get; private set; } = new List<PistaEjercicio>();
    public ICollection<CasoPrueba> CasosPrueba { get; private set; } = new List<CasoPrueba>();
    public ICollection<IntentoEjercicio> Intentos { get; private set; } = new List<IntentoEjercicio>();

    public void ConfigurarAyudas(int intentos, bool permitePistas, bool permiteVerSolucion)
    {
        if (intentos <= 0) throw new ReglaNegocioException("La cantidad recomendada de intentos debe ser mayor que cero.");
        CantidadIntentosRecomendada = intentos;
        PermitePistas = permitePistas;
        PermiteVerSolucion = permiteVerSolucion;
        FechaModificacion = DateTime.UtcNow;
    }

    public void ConfigurarVersionRubrica(string versionRubrica)
    {
        if (string.IsNullOrWhiteSpace(versionRubrica))
            throw new ReglaNegocioException("La versión de la rúbrica es obligatoria.");

        VersionRubrica = versionRubrica.Trim();
        FechaModificacion = DateTime.UtcNow;
    }

    public void ActualizarContenidoAcademico(
        string titulo,
        string consigna,
        NivelDificultad dificultad,
        int orden,
        string codigoInicial,
        int? ejercicioPrerequisitoId)
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(consigna))
            throw new ReglaNegocioException(
                "El título y la consigna del ejercicio son obligatorios.");
        if (orden <= 0)
            throw new ReglaNegocioException(
                "El orden del ejercicio debe ser mayor que cero.");

        Titulo = titulo.Trim();
        Consigna = consigna.Trim();
        Dificultad = dificultad;
        Orden = orden;
        CodigoInicial = codigoInicial ?? string.Empty;
        EjercicioPrerequisitoId = ejercicioPrerequisitoId;
        Activo = true;
        FechaModificacion = DateTime.UtcNow;
    }

    public void AgregarCriterio(CriterioEvaluacion criterio) => CriteriosEvaluacion.Add(criterio);
    public void AgregarPista(PistaEjercicio pista) => Pistas.Add(pista);
    public void AgregarConceptoPermitido(ConceptoPermitido concepto) => ConceptosPermitidos.Add(concepto);
    public void AgregarConceptoNoPermitido(ConceptoNoPermitido concepto) => ConceptosNoPermitidos.Add(concepto);
    public void AgregarCasoPrueba(CasoPrueba caso) => CasosPrueba.Add(caso);

    public void Desactivar()
    {
        Activo = false;
        FechaModificacion = DateTime.UtcNow;
    }
}

public class CriterioEvaluacion : Entidad
{
    private CriterioEvaluacion() { }

    public CriterioEvaluacion(
        int ejercicioId,
        string descripcion,
        TipoCriterio tipo,
        decimal peso,
        bool obligatorio,
        int orden,
        int porcentajeMinimoObligatorio = 60)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ReglaNegocioException("La descripción del criterio es obligatoria.");
        if (peso < 0 || peso > 100)
            throw new ReglaNegocioException("El peso del criterio debe estar entre 0 y 100.");
        if (porcentajeMinimoObligatorio < 0 || porcentajeMinimoObligatorio > 100)
            throw new ReglaNegocioException(
                "El porcentaje mínimo obligatorio debe estar entre 0 y 100.");

        EjercicioId = ejercicioId;
        Descripcion = descripcion.Trim();
        Tipo = tipo;
        Peso = peso;
        Obligatorio = obligatorio;
        Orden = orden;
        PorcentajeMinimoObligatorio = obligatorio
            ? porcentajeMinimoObligatorio
            : 0;
    }

    public int EjercicioId { get; private set; }
    public Ejercicio Ejercicio { get; private set; } = null!;
    public string Descripcion { get; private set; } = string.Empty;
    public TipoCriterio Tipo { get; private set; }
    public decimal Peso { get; private set; }
    public bool Obligatorio { get; private set; }
    public int PorcentajeMinimoObligatorio { get; private set; }
    public int Orden { get; private set; }

    public void ActualizarContenidoAcademico(
        string descripcion,
        TipoCriterio tipo,
        decimal peso,
        bool obligatorio,
        int orden,
        int porcentajeMinimoObligatorio = 60)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ReglaNegocioException(
                "La descripción del criterio es obligatoria.");

        Descripcion = descripcion.Trim();
        Tipo = tipo;
        ActualizarConfiguracion(
            peso,
            obligatorio,
            orden,
            porcentajeMinimoObligatorio);
    }

    public void ActualizarConfiguracion(
        decimal peso,
        bool obligatorio,
        int orden,
        int porcentajeMinimoObligatorio = 60)
    {
        if (peso < 0 || peso > 100)
            throw new ReglaNegocioException(
                "El peso del criterio debe estar entre 0 y 100.");
        if (porcentajeMinimoObligatorio < 0 || porcentajeMinimoObligatorio > 100)
            throw new ReglaNegocioException(
                "El porcentaje mínimo obligatorio debe estar entre 0 y 100.");

        Peso = peso;
        Obligatorio = obligatorio;
        Orden = orden;
        PorcentajeMinimoObligatorio = obligatorio
            ? porcentajeMinimoObligatorio
            : 0;
    }
}

public class ConceptoPermitido : Entidad
{
    private ConceptoPermitido() { }
    public ConceptoPermitido(int ejercicioId, string nombre, string descripcion)
    {
        EjercicioId = ejercicioId;
        Nombre = nombre.Trim();
        Descripcion = descripcion.Trim();
    }

    public int EjercicioId { get; private set; }
    public Ejercicio Ejercicio { get; private set; } = null!;
    public string Nombre { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;

    public void ActualizarContenidoAcademico(string nombre, string descripcion)
    {
        Nombre = nombre.Trim();
        Descripcion = descripcion.Trim();
    }
}

public class ConceptoNoPermitido : Entidad
{
    private ConceptoNoPermitido() { }
    public ConceptoNoPermitido(int ejercicioId, string nombre, string motivo)
    {
        EjercicioId = ejercicioId;
        Nombre = nombre.Trim();
        Motivo = motivo.Trim();
    }

    public int EjercicioId { get; private set; }
    public Ejercicio Ejercicio { get; private set; } = null!;
    public string Nombre { get; private set; } = string.Empty;
    public string Motivo { get; private set; } = string.Empty;

    public void ActualizarContenidoAcademico(string nombre, string motivo)
    {
        Nombre = nombre.Trim();
        Motivo = motivo.Trim();
    }
}

public class CasoPrueba : Entidad
{
    private CasoPrueba() { }
    public CasoPrueba(int ejercicioId, string nombre, string entrada, string salidaEsperada,
        VisibilidadCasoPrueba visibilidad, decimal peso, int orden)
    {
        EjercicioId = ejercicioId;
        Nombre = nombre.Trim();
        Entrada = entrada;
        SalidaEsperada = salidaEsperada;
        Visibilidad = visibilidad;
        Peso = peso;
        Orden = orden;
        Activo = true;
    }

    public int EjercicioId { get; private set; }
    public Ejercicio Ejercicio { get; private set; } = null!;
    public string Nombre { get; private set; } = string.Empty;
    public string Entrada { get; private set; } = string.Empty;
    public string SalidaEsperada { get; private set; } = string.Empty;
    public VisibilidadCasoPrueba Visibilidad { get; private set; }
    public decimal Peso { get; private set; }
    public int Orden { get; private set; }
    public bool Activo { get; private set; }

    public void ActualizarContenidoAcademico(
        string nombre,
        string entrada,
        string salidaEsperada,
        VisibilidadCasoPrueba visibilidad,
        decimal peso,
        int orden)
    {
        Nombre = nombre.Trim();
        Entrada = entrada;
        SalidaEsperada = salidaEsperada;
        Visibilidad = visibilidad;
        Peso = peso;
        Orden = orden;
        Activo = true;
    }
}

public class PistaEjercicio : Entidad
{
    private PistaEjercicio() { }
    public PistaEjercicio(int ejercicioId, int nivel, string contenido,
        int intentosMinimosRequeridos, decimal penalizacionPuntaje)
    {
        EjercicioId = ejercicioId;
        Nivel = nivel;
        Contenido = contenido.Trim();
        IntentosMinimosRequeridos = intentosMinimosRequeridos;
        PenalizacionPuntaje = penalizacionPuntaje;
        Activa = true;
    }

    public int EjercicioId { get; private set; }
    public Ejercicio Ejercicio { get; private set; } = null!;
    public int Nivel { get; private set; }
    public string Contenido { get; private set; } = string.Empty;
    public int IntentosMinimosRequeridos { get; private set; }
    public decimal PenalizacionPuntaje { get; private set; }
    public bool Activa { get; private set; }

    public void ActualizarContenidoAcademico(
        int nivel,
        string contenido,
        int intentosMinimosRequeridos,
        decimal penalizacionPuntaje)
    {
        Nivel = nivel;
        Contenido = contenido.Trim();
        IntentosMinimosRequeridos = intentosMinimosRequeridos;
        PenalizacionPuntaje = penalizacionPuntaje;
        Activa = true;
    }
}

public class PistaUtilizada : Entidad
{
    private PistaUtilizada() { }
    public PistaUtilizada(int usuarioId, int pistaEjercicioId, int? intentoEjercicioId)
    {
        UsuarioId = usuarioId;
        PistaEjercicioId = pistaEjercicioId;
        IntentoEjercicioId = intentoEjercicioId;
        FechaUtilizacion = DateTime.UtcNow;
    }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public int PistaEjercicioId { get; private set; }
    public PistaEjercicio PistaEjercicio { get; private set; } = null!;
    public int? IntentoEjercicioId { get; private set; }
    public IntentoEjercicio? IntentoEjercicio { get; private set; }
    public DateTime FechaUtilizacion { get; private set; }
}
