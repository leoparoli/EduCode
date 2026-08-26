namespace EduCode.Infraestructura.Configuracion;

public sealed class ConfiguracionIA
{
    public const string Seccion = "InteligenciaArtificial";

    public bool UsarSimulador { get; set; } = true;
    public bool UsarFallbackSimulado { get; set; }
    public string BaseUrl { get; set; } =
        "https://generativelanguage.googleapis.com/v1beta/";
    public string ApiKey { get; set; } = string.Empty;

    public string Modelo { get; set; } = "gemini-3.5-flash";
    public bool UsarModeloRespaldo { get; set; }
    public string ModeloRespaldo { get; set; } = "gemini-3.1-flash-lite";

    public bool UsarProveedorAlternativo { get; set; } = true;
    public string ProveedorAlternativo { get; set; } = "OpenAI";
    public string OpenAIBaseUrl { get; set; } = "https://api.openai.com/v1/";
    public string OpenAIApiKey { get; set; } = string.Empty;
    public string OpenAIModelo { get; set; } = "gpt-5-mini";

    public int MaximoTokensSalida { get; set; } = 2400;
    public double Temperatura { get; set; } = 0.1;
    public int TimeoutSegundos { get; set; } = 45;
    public int MaximoSolicitudesDiariasPorUsuario { get; set; } = 25;
    public string NivelPensamiento { get; set; } = "LOW";

    public int CantidadReintentos { get; set; } = 2;
    public int RetrasoInicialMilisegundos { get; set; } = 2000;
    public int RetrasoMaximoMilisegundos { get; set; } = 8000;
    public int FallasParaAbrirCircuito { get; set; } = 3;
    public int SegundosCircuitoAbierto { get; set; } = 90;

    public string VersionPrompt { get; set; } = "tutor-p1-4.2";

    // Configuración independiente del Tutor IA conversacional.
    public bool TutorHabilitado { get; set; } = true;
    public int MaximoConsultasTutorDiariasPorUsuario { get; set; } = 12;
    public int MaximoCaracteresConsultaTutor { get; set; } = 800;
    public int MaximoTokensSalidaTutor { get; set; } = 1200;
    public bool TutorUsarFallbackLocal { get; set; } = true;
    public string VersionPromptTutor { get; set; } = "tutor-consultas-p1-2.9.2";

    // Ajustes propios de los proveedores del tutor conversacional.
    // Se mantienen separados del evaluador para no alterar puntajes ni ranking.
    public string TutorModeloGeminiRespaldo { get; set; } = "gemini-3.1-flash-lite";
    public string TutorNivelPensamientoGemini { get; set; } = "LOW";
    public string TutorModeloOpenAIRespaldo { get; set; } = "gpt-4o-mini";
    public string TutorRazonamientoOpenAI { get; set; } = "low";
    public int TutorMaximoReintentosProveedor { get; set; } = 1;
    public bool TutorReintentarSinEsquemaEstricto { get; set; } = true;

    // Defensa de salida: valida que el proveedor no revele instrucciones,
    // inserte HTML ejecutable ni se desvíe a tecnologías ajenas al curso.
    public bool TutorValidarSalidaProveedor { get; set; } = true;
    public int TutorMaximoCaracteresRespuesta { get; set; } = 6000;
}
