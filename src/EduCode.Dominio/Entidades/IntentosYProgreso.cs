using EduCode.Dominio.Enums;

namespace EduCode.Dominio.Entidades;

public class IntentoEjercicio : Entidad
{
    private IntentoEjercicio() { }

    public IntentoEjercicio(
        int usuarioId,
        int ejercicioId,
        int numeroIntento,
        string codigoEnviado,
        int? tiempoResolucionSegundos)
    {
        UsuarioId = usuarioId;
        EjercicioId = ejercicioId;
        NumeroIntento = numeroIntento;
        CodigoEnviado = codigoEnviado;
        TiempoResolucionSegundos = tiempoResolucionSegundos;
        FechaEnvio = DateTime.UtcNow;
        Estado = EstadoIntento.Enviado;
    }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public int EjercicioId { get; private set; }
    public Ejercicio Ejercicio { get; private set; } = null!;
    public int NumeroIntento { get; private set; }
    public string CodigoEnviado { get; private set; } = string.Empty;
    public DateTime FechaEnvio { get; private set; }
    public EstadoIntento Estado { get; private set; }
    public decimal? PuntajeObtenido { get; private set; }
    public int? TiempoResolucionSegundos { get; private set; }
    public bool UtilizoPista { get; private set; }
    public bool SolicitoSolucion { get; private set; }
    public CorreccionIntento? Correccion { get; private set; }

    public void MarcarEnCorreccion() => Estado = EstadoIntento.EnCorreccion;

    public void MarcarPendienteCorreccion() =>
        Estado = EstadoIntento.PendienteCorreccion;

    public void RegistrarResultado(
        decimal puntajeExacto,
        bool aprobado,
        bool completamenteIncorrecto = false)
    {
        PuntajeObtenido = decimal.Round(puntajeExacto, 2);
        Estado = aprobado
            ? EstadoIntento.Correcto
            : completamenteIncorrecto || puntajeExacto <= 0
                ? EstadoIntento.Incorrecto
                : EstadoIntento.ParcialmenteCorrecto;
    }

    public void RegistrarResultadoProvisorio(decimal puntajeExacto)
    {
        PuntajeObtenido = decimal.Round(puntajeExacto, 2);
        Estado = EstadoIntento.CorreccionProvisoria;
    }

    public void RegistrarErrorCorreccion() =>
        Estado = EstadoIntento.ErrorCorreccion;

    public bool PuedeReintentarCorreccion() => Estado is
        EstadoIntento.PendienteCorreccion or
        EstadoIntento.ErrorCorreccion or
        EstadoIntento.CorreccionProvisoria;

    public void AsociarCorreccion(CorreccionIntento correccion) =>
        Correccion = correccion;
    public void MarcarUsoPista() => UtilizoPista = true;
}

public class CorreccionIntento : Entidad
{
    private CorreccionIntento() { }

    public CorreccionIntento(
        int intentoEjercicioId,
        OrigenCorreccion origen,
        string resumen,
        string pistaSugerida,
        decimal puntajeExacto,
        int puntajeMostrado,
        bool aprobado,
        bool cumpleCriteriosObligatorios,
        bool requiereRevisionDocente,
        string modeloIAUtilizado,
        string versionPrompt,
        string versionRubrica,
        string versionMotorPuntaje)
    {
        IntentoEjercicioId = intentoEjercicioId;
        Origen = origen;
        Resumen = resumen;
        PistaSugerida = pistaSugerida;
        PuntajeExacto = decimal.Round(puntajeExacto, 2);
        PuntajeMostrado = puntajeMostrado;
        Aprobado = aprobado;
        CumpleCriteriosObligatorios = cumpleCriteriosObligatorios;
        RequiereRevisionDocente = requiereRevisionDocente;
        ModeloIAUtilizado = modeloIAUtilizado;
        VersionPrompt = versionPrompt;
        VersionRubrica = versionRubrica;
        VersionMotorPuntaje = versionMotorPuntaje;
        FechaCorreccion = DateTime.UtcNow;
    }

    public void Actualizar(
        OrigenCorreccion origen,
        string resumen,
        string pistaSugerida,
        decimal puntajeExacto,
        int puntajeMostrado,
        bool aprobado,
        bool cumpleCriteriosObligatorios,
        bool requiereRevisionDocente,
        string modeloIAUtilizado,
        string versionPrompt,
        string versionRubrica,
        string versionMotorPuntaje)
    {
        Origen = origen;
        Resumen = resumen;
        PistaSugerida = pistaSugerida;
        PuntajeExacto = decimal.Round(puntajeExacto, 2);
        PuntajeMostrado = puntajeMostrado;
        Aprobado = aprobado;
        CumpleCriteriosObligatorios = cumpleCriteriosObligatorios;
        RequiereRevisionDocente = requiereRevisionDocente;
        ModeloIAUtilizado = modeloIAUtilizado;
        VersionPrompt = versionPrompt;
        VersionRubrica = versionRubrica;
        VersionMotorPuntaje = versionMotorPuntaje;
        FechaCorreccion = DateTime.UtcNow;

        ResultadosCriterios.Clear();
        ItemsFeedback.Clear();
    }

    public void ActualizarFeedbackPedagogico(
        string resumen,
        string pistaSugerida,
        bool requiereRevisionDocente,
        string modeloIAUtilizado,
        string versionPrompt)
    {
        Origen = OrigenCorreccion.Mixta;
        Resumen = resumen?.Trim() ?? string.Empty;
        PistaSugerida = pistaSugerida?.Trim() ?? string.Empty;
        RequiereRevisionDocente = requiereRevisionDocente;
        ModeloIAUtilizado = modeloIAUtilizado?.Trim() ?? string.Empty;
        VersionPrompt = versionPrompt?.Trim() ?? string.Empty;
        FechaCorreccion = DateTime.UtcNow;
        ItemsFeedback.Clear();
    }

    public int IntentoEjercicioId { get; private set; }
    public IntentoEjercicio IntentoEjercicio { get; private set; } = null!;
    public OrigenCorreccion Origen { get; private set; }
    public string Resumen { get; private set; } = string.Empty;
    public string PistaSugerida { get; private set; } = string.Empty;
    public decimal PuntajeExacto { get; private set; }
    public int PuntajeMostrado { get; private set; }
    public bool Aprobado { get; private set; }
    public bool CumpleCriteriosObligatorios { get; private set; }
    public bool RequiereRevisionDocente { get; private set; }
    public string ModeloIAUtilizado { get; private set; } = string.Empty;
    public string VersionPrompt { get; private set; } = string.Empty;
    public string VersionRubrica { get; private set; } = string.Empty;
    public string VersionMotorPuntaje { get; private set; } = string.Empty;
    public DateTime FechaCorreccion { get; private set; }

    public ICollection<ResultadoCriterio> ResultadosCriterios { get; private set; } =
        new List<ResultadoCriterio>();
    public ICollection<ItemFeedback> ItemsFeedback { get; private set; } =
        new List<ItemFeedback>();

    public void AgregarResultadoCriterio(ResultadoCriterio resultado) =>
        ResultadosCriterios.Add(resultado);

    public void AgregarItemFeedback(ItemFeedback item) =>
        ItemsFeedback.Add(item);
}

public class ResultadoCriterio : Entidad
{
    private ResultadoCriterio() { }

    public ResultadoCriterio(
        int correccionIntentoId,
        int criterioEvaluacionId,
        int porcentajeCumplimiento,
        decimal pesoAplicado,
        decimal puntajeObtenido,
        string evidencia,
        string observacion,
        bool cumplimientoObligatorioAlcanzado)
    {
        CorreccionIntentoId = correccionIntentoId;
        CriterioEvaluacionId = criterioEvaluacionId;
        PorcentajeCumplimiento = Math.Clamp(porcentajeCumplimiento, 0, 100);
        PesoAplicado = pesoAplicado;
        PuntajeObtenido = decimal.Round(puntajeObtenido, 2);
        Evidencia = evidencia?.Trim() ?? string.Empty;
        Observacion = observacion?.Trim() ?? string.Empty;
        CumplimientoObligatorioAlcanzado = cumplimientoObligatorioAlcanzado;
    }

    public int CorreccionIntentoId { get; private set; }
    public CorreccionIntento CorreccionIntento { get; private set; } = null!;
    public int CriterioEvaluacionId { get; private set; }
    public CriterioEvaluacion CriterioEvaluacion { get; private set; } = null!;
    public int PorcentajeCumplimiento { get; private set; }
    public decimal PesoAplicado { get; private set; }
    public decimal PuntajeObtenido { get; private set; }
    public string Evidencia { get; private set; } = string.Empty;
    public string Observacion { get; private set; } = string.Empty;
    public bool CumplimientoObligatorioAlcanzado { get; private set; }
}

public class ItemFeedback : Entidad
{
    private ItemFeedback() { }

    public ItemFeedback(
        int correccionIntentoId,
        TipoItemFeedback tipo,
        string descripcion,
        int orden)
    {
        CorreccionIntentoId = correccionIntentoId;
        Tipo = tipo;
        Descripcion = descripcion;
        Orden = orden;
    }

    public int CorreccionIntentoId { get; private set; }
    public CorreccionIntento CorreccionIntento { get; private set; } = null!;
    public TipoItemFeedback Tipo { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public int Orden { get; private set; }
}

public class ProgresoEjercicio : Entidad
{
    private ProgresoEjercicio() { }

    public ProgresoEjercicio(
        int usuarioId,
        int ejercicioId,
        EstadoProgreso estadoInicial = EstadoProgreso.Disponible)
    {
        UsuarioId = usuarioId;
        EjercicioId = ejercicioId;
        Estado = estadoInicial;
    }

    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public int EjercicioId { get; private set; }
    public Ejercicio Ejercicio { get; private set; } = null!;
    public EstadoProgreso Estado { get; private set; }
    public int CantidadIntentos { get; private set; }
    public decimal MejorPuntaje { get; private set; }
    public DateTime? FechaInicio { get; private set; }
    public DateTime? FechaAprobacion { get; private set; }
    public DateTime? FechaUltimaActividad { get; private set; }

    public void RegistrarIntento(decimal puntajeExacto, bool aprobado)
    {
        CantidadIntentos++;
        FechaInicio ??= DateTime.UtcNow;
        FechaUltimaActividad = DateTime.UtcNow;

        if (puntajeExacto > MejorPuntaje)
            MejorPuntaje = decimal.Round(puntajeExacto, 2);

        if (aprobado)
        {
            Estado = EstadoProgreso.Aprobado;
            FechaAprobacion ??= DateTime.UtcNow;
        }
        else
        {
            Estado = EstadoProgreso.EnProceso;
        }
    }
}
