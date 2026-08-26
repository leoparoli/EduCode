using EduCode.Dominio.Entidades;
using EduCode.DTOs;

namespace EduCode.Aplicacion.Interfaces;

public interface IRepositorio<T> where T : Entidad
{
    Task<T?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyCollection<T>> ObtenerTodosAsync(CancellationToken ct = default);
    Task AgregarAsync(T entidad, CancellationToken ct = default);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
    Task<bool> ExisteAsync(int id, CancellationToken ct = default);
}

public interface IRepositorioUsuarios : IRepositorio<Usuario>
{
    Task<Usuario?> ObtenerPorEmailONombreUsuarioAsync(string valor, CancellationToken ct = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken ct = default);
    Task<bool> ExisteNombreUsuarioDeOtroAsync(
        string nombreUsuario,
        int usuarioId,
        CancellationToken ct = default);
}

public interface IRepositorioCambiosNombreUsuario : IRepositorio<CambioNombreUsuario>
{
    Task<IReadOnlyCollection<CambioNombreUsuario>> ObtenerDesdeAsync(
        int usuarioId,
        DateTime desdeUtc,
        CancellationToken ct = default);
}

public interface IRepositorioOpiniones : IRepositorio<OpinionUsuario>
{
    Task<IReadOnlyCollection<OpinionUsuario>> ObtenerPorUsuarioAsync(
        int usuarioId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<OpinionUsuario>> ObtenerTodasConUsuarioAsync(
        CancellationToken ct = default);

    Task<OpinionUsuario?> ObtenerConUsuarioAsync(
        int id,
        CancellationToken ct = default);

    Task<int> ContarPorUsuarioDesdeAsync(
        int usuarioId,
        DateTime desdeUtc,
        CancellationToken ct = default);

    Task<bool> ExisteDuplicadaRecienteAsync(
        int usuarioId,
        string comentario,
        DateTime desdeUtc,
        CancellationToken ct = default);
}

public interface IRepositorioTokens : IRepositorio<TokenActualizacion>
{
    Task<TokenActualizacion?> ObtenerValidoAsync(
        string token,
        CancellationToken ct = default);
}

public interface IRepositorioCursos : IRepositorio<Curso>
{
    Task<Curso?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyCollection<Curso>> ObtenerActivosConTemasAsync(CancellationToken ct = default);
}

public interface IRepositorioTemas : IRepositorio<Tema>
{
    Task<IReadOnlyCollection<Tema>> ObtenerPorCursoAsync(int cursoId, CancellationToken ct = default);
}

public interface IRepositorioEjercicios : IRepositorio<Ejercicio>
{
    Task<Ejercicio?> ObtenerCompletoAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Ejercicio>> ObtenerPorTemaAsync(int temaId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Ejercicio>> ObtenerActivosAsync(CancellationToken ct = default);
}

public interface IRepositorioIntentos : IRepositorio<IntentoEjercicio>
{
    Task<IntentoEjercicio?> ObtenerCompletoPorIdAsync(
        int intentoId,
        CancellationToken ct = default);

    Task<int> ObtenerCantidadIntentosAsync(
        int usuarioId,
        int ejercicioId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<IntentoEjercicio>> ObtenerHistorialAsync(
        int usuarioId,
        int ejercicioId,
        CancellationToken ct = default);

    Task<IntentoEjercicio?> ObtenerIntentoRecienteDuplicadoAsync(
        int usuarioId,
        int ejercicioId,
        string codigo,
        DateTime desdeUtc,
        CancellationToken ct = default);

    Task<IntentoEjercicio?> ObtenerParaReintentoAsync(
        int intentoId,
        int usuarioId,
        CancellationToken ct = default);
}

public interface IRepositorioProgresos : IRepositorio<ProgresoEjercicio>
{
    Task<ProgresoEjercicio?> ObtenerAsync(int usuarioId, int ejercicioId, CancellationToken ct = default);
    Task<IReadOnlyCollection<ProgresoEjercicio>> ObtenerPorUsuarioAsync(
        int usuarioId, CancellationToken ct = default);
}

public interface IRepositorioPistas : IRepositorio<PistaEjercicio>
{
    Task<PistaEjercicio?> ObtenerSiguienteDisponibleAsync(
        int ejercicioId, int cantidadIntentos, int nivelYaUsado, CancellationToken ct = default);
}

public interface IRepositorioSolicitudesIA : IRepositorio<SolicitudIA>
{
    Task<int> ContarPorUsuarioDesdeAsync(
        int usuarioId,
        DateTime desdeUtc,
        CancellationToken ct = default);

    Task<int> ContarTutorPorUsuarioDesdeAsync(
        int usuarioId,
        DateTime desdeUtc,
        CancellationToken ct = default);
}

public interface IRepositorioGamificacion
{
    Task<Usuario?> ObtenerUsuarioAsync(
        int usuarioId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<Usuario>> ObtenerUsuariosConProgresosAsync(
        CancellationToken ct = default);

    Task<IReadOnlyCollection<IntentoEjercicio>> ObtenerIntentosUsuarioAsync(
        int usuarioId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<Logro>> ObtenerLogrosActivosAsync(
        CancellationToken ct = default);

    Task<IReadOnlyCollection<UsuarioLogro>> ObtenerLogrosUsuarioAsync(
        int usuarioId,
        CancellationToken ct = default);

    Task AgregarUsuarioLogroAsync(
        UsuarioLogro usuarioLogro,
        CancellationToken ct = default);
}


public interface IRepositorioPortalAcademico
{
    Task<InvitacionDocente?> ObtenerInvitacionPorCodigoAsync(
        string codigo, CancellationToken ct = default);
    Task<IReadOnlyCollection<InvitacionDocente>> ObtenerInvitacionesAsync(
        CancellationToken ct = default);
    Task<PropuestaEjercicio?> ObtenerPropuestaAsync(
        int id, CancellationToken ct = default);
    Task<IReadOnlyCollection<PropuestaEjercicio>> ObtenerPropuestasDocenteAsync(
        int docenteId, CancellationToken ct = default);
    Task<IReadOnlyCollection<PropuestaEjercicio>> ObtenerPropuestasAdministracionAsync(
        CancellationToken ct = default);
    Task<PanelDocenteDTO> ObtenerPanelDocenteAsync(
        int docenteId, CancellationToken ct = default);
    Task<PanelAdministracionDTO> ObtenerPanelAdministracionAsync(
        CancellationToken ct = default);
    Task<EstructuraAcademicaDTO> ObtenerEstructuraAcademicaAsync(
        CancellationToken ct = default);
    Task<Grupo?> ObtenerGrupoAsync(int grupoId, CancellationToken ct = default);
    Task<bool> ExisteInscripcionAsync(
        int alumnoId, int grupoId, CancellationToken ct = default);
}

public interface IUnidadTrabajo
{
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}

public interface IServicioPassword
{
    string GenerarHash(string password);
    bool Verificar(string password, string passwordHash);
}

public interface IServicioToken
{
    TokenDTO GenerarToken(Usuario usuario);
}

public interface IServicioCorreccionIA
{
    Task<RespuestaCorreccionIADTO> CorregirAsync(
        SolicitudCorreccionIADTO solicitud, CancellationToken ct = default);
}


public interface IServicioTutorIA
{
    Task<RespuestaTutorIADTO> ConsultarAsync(
        SolicitudTutorIADTO solicitud,
        CancellationToken ct = default);

    Task<EstadoTutorIADTO> ObtenerEstadoAsync(
        int usuarioId,
        CancellationToken ct = default);
}

public interface IServicioEvaluacionLocal
{
    ResultadoEvaluacionLocalDTO Evaluar(
        SolicitudCorreccionIADTO solicitud);
}

public interface IColaFeedbackIA
{
    ValueTask EncolarAsync(
        int intentoId,
        CancellationToken ct = default);

    ValueTask<int> DesencolarAsync(
        CancellationToken ct);
}

public interface IServicioUsuarioActual
{
    int ObtenerUsuarioId();
    string ObtenerRol();
}

public interface IServicioArchivos
{
    Task<string> GuardarAsync(Stream archivo, string nombre, string tipoContenido,
        CancellationToken ct = default);
    Task<Stream> ObtenerAsync(string ruta, CancellationToken ct = default);
    Task EliminarAsync(string ruta, CancellationToken ct = default);
}
