using EduCode.Dominio.Enums;
using EduCode.Dominio.Excepciones;

namespace EduCode.Dominio.Entidades;

public sealed class InvitacionDocente : Entidad
{
    private InvitacionDocente() { }

    public InvitacionDocente(
        string codigo,
        int creadaPorUsuarioId,
        string especialidad,
        DateTime fechaExpiracion,
        string? emailPermitido = null)
    {
        codigo = (codigo ?? string.Empty).Trim().ToUpperInvariant();
        if (codigo.Length < 8 || codigo.Length > 40)
            throw new ReglaNegocioException("El código de invitación no es válido.");
        if (fechaExpiracion <= DateTime.UtcNow)
            throw new ReglaNegocioException("La invitación debe tener una fecha de expiración futura.");

        Codigo = codigo;
        CreadaPorUsuarioId = creadaPorUsuarioId;
        Especialidad = (especialidad ?? string.Empty).Trim();
        EmailPermitido = string.IsNullOrWhiteSpace(emailPermitido)
            ? null
            : emailPermitido.Trim().ToLowerInvariant();
        FechaCreacion = DateTime.UtcNow;
        FechaExpiracion = fechaExpiracion;
        Estado = EstadoInvitacionDocente.Disponible;
    }

    public string Codigo { get; private set; } = string.Empty;
    public int CreadaPorUsuarioId { get; private set; }
    public Usuario CreadaPorUsuario { get; private set; } = null!;
    public string Especialidad { get; private set; } = string.Empty;
    public string? EmailPermitido { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime FechaExpiracion { get; private set; }
    public EstadoInvitacionDocente Estado { get; private set; }
    public int? UtilizadaPorUsuarioId { get; private set; }
    public Usuario? UtilizadaPorUsuario { get; private set; }
    public DateTime? FechaUtilizacion { get; private set; }

    public bool PuedeUtilizarse(string email)
    {
        if (Estado != EstadoInvitacionDocente.Disponible)
            return false;
        if (FechaExpiracion <= DateTime.UtcNow)
            return false;
        if (string.IsNullOrWhiteSpace(email))
            return false;
        return string.IsNullOrWhiteSpace(EmailPermitido) ||
            EmailPermitido.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public void MarcarUtilizada(int usuarioId)
    {
        if (Estado != EstadoInvitacionDocente.Disponible || FechaExpiracion <= DateTime.UtcNow)
            throw new ReglaNegocioException("La invitación ya no está disponible.");
        UtilizadaPorUsuarioId = usuarioId;
        FechaUtilizacion = DateTime.UtcNow;
        Estado = EstadoInvitacionDocente.Utilizada;
    }

    public void Revocar()
    {
        if (Estado == EstadoInvitacionDocente.Utilizada)
            throw new ReglaNegocioException("Una invitación utilizada no puede revocarse.");
        Estado = EstadoInvitacionDocente.Revocada;
    }

    public void MarcarVencida()
    {
        if (Estado == EstadoInvitacionDocente.Disponible && FechaExpiracion <= DateTime.UtcNow)
            Estado = EstadoInvitacionDocente.Vencida;
    }
}

public sealed class PropuestaEjercicio : Entidad
{
    private PropuestaEjercicio() { }

    public PropuestaEjercicio(
        int docenteId,
        int temaId,
        string titulo,
        string consigna,
        NivelDificultad dificultad,
        string codigoInicial,
        string criterioPrincipal,
        string orientacionPista,
        string fundamentoPedagogico)
    {
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(consigna))
            throw new ReglaNegocioException("El título y la consigna son obligatorios.");
        if (string.IsNullOrWhiteSpace(criterioPrincipal))
            throw new ReglaNegocioException("Indicá qué debería lograr una solución correcta.");

        DocenteId = docenteId;
        TemaId = temaId;
        Titulo = titulo.Trim();
        Consigna = consigna.Trim();
        Dificultad = dificultad;
        CodigoInicial = (codigoInicial ?? string.Empty).Trim();
        CriterioPrincipal = criterioPrincipal.Trim();
        OrientacionPista = (orientacionPista ?? string.Empty).Trim();
        FundamentoPedagogico = (fundamentoPedagogico ?? string.Empty).Trim();
        Estado = EstadoPropuestaEjercicio.Enviada;
        FechaCreacion = DateTime.UtcNow;
    }

    public int DocenteId { get; private set; }
    public Usuario Docente { get; private set; } = null!;
    public int TemaId { get; private set; }
    public Tema Tema { get; private set; } = null!;
    public string Titulo { get; private set; } = string.Empty;
    public string Consigna { get; private set; } = string.Empty;
    public NivelDificultad Dificultad { get; private set; }
    public string CodigoInicial { get; private set; } = string.Empty;
    public string CriterioPrincipal { get; private set; } = string.Empty;
    public string OrientacionPista { get; private set; } = string.Empty;
    public string FundamentoPedagogico { get; private set; } = string.Empty;
    public EstadoPropuestaEjercicio Estado { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public int? RevisadaPorUsuarioId { get; private set; }
    public Usuario? RevisadaPorUsuario { get; private set; }
    public DateTime? FechaRevision { get; private set; }
    public string ObservacionRevision { get; private set; } = string.Empty;
    public int? EjercicioPublicadoId { get; private set; }
    public Ejercicio? EjercicioPublicado { get; private set; }

    public void MarcarEnRevision()
    {
        if (Estado == EstadoPropuestaEjercicio.Enviada)
            Estado = EstadoPropuestaEjercicio.EnRevision;
    }

    public void ActualizarParaReenvio(
        int docenteId,
        int temaId,
        string titulo,
        string consigna,
        NivelDificultad dificultad,
        string codigoInicial,
        string criterioPrincipal,
        string orientacionPista,
        string fundamentoPedagogico)
    {
        if (DocenteId != docenteId)
            throw new ReglaNegocioException("La propuesta pertenece a otro docente.");
        if (Estado != EstadoPropuestaEjercicio.RequiereAjustes)
            throw new ReglaNegocioException("Solo pueden editarse propuestas que requieren ajustes.");
        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(consigna) ||
            string.IsNullOrWhiteSpace(criterioPrincipal))
            throw new ReglaNegocioException("Completá el título, la consigna y el criterio principal.");

        TemaId = temaId;
        Titulo = titulo.Trim();
        Consigna = consigna.Trim();
        Dificultad = dificultad;
        CodigoInicial = (codigoInicial ?? string.Empty).Trim();
        CriterioPrincipal = criterioPrincipal.Trim();
        OrientacionPista = (orientacionPista ?? string.Empty).Trim();
        FundamentoPedagogico = (fundamentoPedagogico ?? string.Empty).Trim();
        Estado = EstadoPropuestaEjercicio.Enviada;
        FechaRevision = null;
        RevisadaPorUsuarioId = null;
    }

    public void Aprobar(int administradorId, int ejercicioPublicadoId, string observacion)
    {
        RevisadaPorUsuarioId = administradorId;
        EjercicioPublicadoId = ejercicioPublicadoId;
        ObservacionRevision = (observacion ?? string.Empty).Trim();
        FechaRevision = DateTime.UtcNow;
        Estado = EstadoPropuestaEjercicio.Aprobada;
    }

    public void SolicitarAjustes(int administradorId, string observacion)
    {
        if (string.IsNullOrWhiteSpace(observacion))
            throw new ReglaNegocioException("Indicá qué debería ajustarse en la propuesta.");
        RevisadaPorUsuarioId = administradorId;
        ObservacionRevision = observacion.Trim();
        FechaRevision = DateTime.UtcNow;
        Estado = EstadoPropuestaEjercicio.RequiereAjustes;
    }

    public void Rechazar(int administradorId, string observacion)
    {
        if (string.IsNullOrWhiteSpace(observacion))
            throw new ReglaNegocioException("Indicá brevemente el motivo del rechazo.");
        RevisadaPorUsuarioId = administradorId;
        ObservacionRevision = observacion.Trim();
        FechaRevision = DateTime.UtcNow;
        Estado = EstadoPropuestaEjercicio.Rechazada;
    }
}
