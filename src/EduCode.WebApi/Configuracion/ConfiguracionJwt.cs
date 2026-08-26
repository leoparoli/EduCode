namespace EduCode.WebApi.Configuracion;

public sealed class ConfiguracionJwt
{
    public const string Seccion = "Jwt";

    public string ClaveSecreta { get; set; } = string.Empty;
    public string Emisor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public int DuracionMinutos { get; set; } = 60;
    public int DuracionRefreshTokenDias { get; set; } = 7;
}
