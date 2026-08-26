using EduCode.Dominio.Enums;
using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace EduCode.AccesoDatos.Repositorios;

public class Repositorio<T> : IRepositorio<T> where T : Entidad
{
    protected readonly EduCodeContext Contexto;
    protected readonly DbSet<T> DbSet;

    public Repositorio(EduCodeContext contexto)
    {
        Contexto = contexto;
        DbSet = contexto.Set<T>();
    }

    public virtual async Task<T?> ObtenerPorIdAsync(
        int id, CancellationToken ct = default) =>
        await DbSet.FindAsync(new object?[] { id }, ct);

    public virtual async Task<IReadOnlyCollection<T>> ObtenerTodosAsync(
        CancellationToken ct = default) =>
        await DbSet.AsNoTracking().ToListAsync(ct);

    public virtual async Task AgregarAsync(
        T entidad, CancellationToken ct = default) =>
        await DbSet.AddAsync(entidad, ct);

    public void Actualizar(T entidad) => DbSet.Update(entidad);
    public void Eliminar(T entidad) => DbSet.Remove(entidad);

    public Task<bool> ExisteAsync(int id, CancellationToken ct = default) =>
        DbSet.AnyAsync(x => x.Id == id, ct);
}

public sealed class RepositorioUsuarios :
    Repositorio<Usuario>, IRepositorioUsuarios
{
    public RepositorioUsuarios(EduCodeContext contexto) : base(contexto) { }

    public Task<Usuario?> ObtenerPorEmailONombreUsuarioAsync(
        string valor, CancellationToken ct = default)
    {
        valor = valor.Trim().ToLowerInvariant();

        return DbSet.FirstOrDefaultAsync(
            usuario =>
                usuario.Email == valor ||
                usuario.NombreUsuario == valor,
            ct);
    }

    public Task<bool> ExisteEmailAsync(
        string email, CancellationToken ct = default)
    {
        email = email.Trim().ToLowerInvariant();
        return DbSet.AnyAsync(usuario => usuario.Email == email, ct);
    }

    public Task<bool> ExisteNombreUsuarioAsync(
        string nombreUsuario, CancellationToken ct = default)
    {
        nombreUsuario = nombreUsuario.Trim().ToLowerInvariant();
        return DbSet.AnyAsync(
            usuario => usuario.NombreUsuario == nombreUsuario,
            ct);
    }

    public Task<bool> ExisteNombreUsuarioDeOtroAsync(
        string nombreUsuario,
        int usuarioId,
        CancellationToken ct = default)
    {
        nombreUsuario = nombreUsuario.Trim().ToLowerInvariant();
        return DbSet.AnyAsync(
            usuario =>
                usuario.Id != usuarioId &&
                usuario.NombreUsuario == nombreUsuario,
            ct);
    }
}

public sealed class RepositorioCambiosNombreUsuario :
    Repositorio<CambioNombreUsuario>, IRepositorioCambiosNombreUsuario
{
    public RepositorioCambiosNombreUsuario(EduCodeContext contexto) : base(contexto) { }

    public async Task<IReadOnlyCollection<CambioNombreUsuario>> ObtenerDesdeAsync(
        int usuarioId,
        DateTime desdeUtc,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(cambio =>
                cambio.UsuarioId == usuarioId &&
                cambio.FechaCambio >= desdeUtc)
            .OrderBy(cambio => cambio.FechaCambio)
            .ThenBy(cambio => cambio.Id)
            .ToListAsync(ct);
}

public sealed class RepositorioOpiniones :
    Repositorio<OpinionUsuario>, IRepositorioOpiniones
{
    public RepositorioOpiniones(EduCodeContext contexto) : base(contexto) { }

    public async Task<IReadOnlyCollection<OpinionUsuario>> ObtenerPorUsuarioAsync(
        int usuarioId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Include(opinion => opinion.Usuario)
            .Where(opinion => opinion.UsuarioId == usuarioId)
            .OrderByDescending(opinion => opinion.FechaCreacion)
            .ThenByDescending(opinion => opinion.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<OpinionUsuario>> ObtenerTodasConUsuarioAsync(
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Include(opinion => opinion.Usuario)
            .OrderBy(opinion => opinion.Estado)
            .ThenByDescending(opinion => opinion.FechaCreacion)
            .ThenByDescending(opinion => opinion.Id)
            .ToListAsync(ct);

    public Task<OpinionUsuario?> ObtenerConUsuarioAsync(
        int id,
        CancellationToken ct = default) =>
        DbSet
            .Include(opinion => opinion.Usuario)
            .FirstOrDefaultAsync(opinion => opinion.Id == id, ct);

    public Task<int> ContarPorUsuarioDesdeAsync(
        int usuarioId,
        DateTime desdeUtc,
        CancellationToken ct = default) =>
        DbSet.CountAsync(
            opinion =>
                opinion.UsuarioId == usuarioId &&
                opinion.FechaCreacion >= desdeUtc,
            ct);

    public Task<bool> ExisteDuplicadaRecienteAsync(
        int usuarioId,
        string comentario,
        DateTime desdeUtc,
        CancellationToken ct = default)
    {
        comentario = comentario.Trim();
        return DbSet.AnyAsync(
            opinion =>
                opinion.UsuarioId == usuarioId &&
                opinion.Comentario == comentario &&
                opinion.FechaCreacion >= desdeUtc,
            ct);
    }
}

public sealed class RepositorioTokens :
    Repositorio<TokenActualizacion>, IRepositorioTokens
{
    public RepositorioTokens(EduCodeContext contexto) : base(contexto) { }

    public Task<TokenActualizacion?> ObtenerValidoAsync(
        string token,
        CancellationToken ct = default) =>
        DbSet
            .Include(actualizacion => actualizacion.Usuario)
            .FirstOrDefaultAsync(
                actualizacion =>
                    actualizacion.Token == token &&
                    !actualizacion.Revocado &&
                    actualizacion.FechaExpiracion > DateTime.UtcNow,
                ct);
}

public sealed class RepositorioCursos :
    Repositorio<Curso>, IRepositorioCursos
{
    public RepositorioCursos(EduCodeContext contexto) : base(contexto) { }

    public Task<Curso?> ObtenerPorCodigoAsync(
        string codigo, CancellationToken ct = default)
    {
        codigo = codigo.Trim().ToUpperInvariant();
        return DbSet
            .Include(curso => curso.Temas)
            .FirstOrDefaultAsync(curso => curso.Codigo == codigo, ct);
    }

    public async Task<IReadOnlyCollection<Curso>> ObtenerActivosConTemasAsync(
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Include(curso => curso.Temas)
            .Where(curso => curso.Activo)
            .OrderBy(curso => curso.Nombre)
            .ToListAsync(ct);
}

public sealed class RepositorioTemas :
    Repositorio<Tema>, IRepositorioTemas
{
    public RepositorioTemas(EduCodeContext contexto) : base(contexto) { }

    public async Task<IReadOnlyCollection<Tema>> ObtenerPorCursoAsync(
        int cursoId, CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Include(tema => tema.Ejercicios)
            .Where(tema => tema.CursoId == cursoId && tema.Activo)
            .OrderBy(tema => tema.Orden)
            .ToListAsync(ct);
}

public sealed class RepositorioEjercicios :
    Repositorio<Ejercicio>, IRepositorioEjercicios
{
    public RepositorioEjercicios(EduCodeContext contexto) : base(contexto) { }

    public Task<Ejercicio?> ObtenerCompletoAsync(
        int id, CancellationToken ct = default) =>
        DbSet
            .Include(ejercicio => ejercicio.Tema)
            .Include(ejercicio => ejercicio.CriteriosEvaluacion)
            .Include(ejercicio => ejercicio.Pistas)
            .Include(ejercicio => ejercicio.CasosPrueba)
            .Include(ejercicio => ejercicio.ConceptosPermitidos)
            .Include(ejercicio => ejercicio.ConceptosNoPermitidos)
            .FirstOrDefaultAsync(ejercicio => ejercicio.Id == id, ct);

    public async Task<IReadOnlyCollection<Ejercicio>> ObtenerPorTemaAsync(
        int temaId, CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(ejercicio =>
                ejercicio.TemaId == temaId &&
                ejercicio.Activo)
            .OrderBy(ejercicio => ejercicio.Orden)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<Ejercicio>> ObtenerActivosAsync(
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Include(ejercicio => ejercicio.Tema)
            .Where(ejercicio => ejercicio.Activo)
            .ToListAsync(ct);
}

public sealed class RepositorioIntentos :
    Repositorio<IntentoEjercicio>, IRepositorioIntentos
{
    public RepositorioIntentos(EduCodeContext contexto) : base(contexto) { }

    public Task<IntentoEjercicio?> ObtenerCompletoPorIdAsync(
        int intentoId,
        CancellationToken ct = default) =>
        DbSet
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.CriteriosEvaluacion)
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.ConceptosPermitidos)
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.ConceptosNoPermitidos)
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.CasosPrueba)
            .Include(intento => intento.Correccion)
                .ThenInclude(correccion => correccion!.ResultadosCriterios)
            .Include(intento => intento.Correccion)
                .ThenInclude(correccion => correccion!.ItemsFeedback)
            .FirstOrDefaultAsync(
                intento => intento.Id == intentoId,
                ct);

    public Task<int> ObtenerCantidadIntentosAsync(
        int usuarioId, int ejercicioId, CancellationToken ct = default) =>
        DbSet.CountAsync(
            intento =>
                intento.UsuarioId == usuarioId &&
                intento.EjercicioId == ejercicioId,
            ct);

    public async Task<IReadOnlyCollection<IntentoEjercicio>> ObtenerHistorialAsync(
        int usuarioId, int ejercicioId, CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Include(intento => intento.Correccion)
            .Where(intento =>
                intento.UsuarioId == usuarioId &&
                intento.EjercicioId == ejercicioId)
            .OrderByDescending(intento => intento.NumeroIntento)
            .ToListAsync(ct);

    public Task<IntentoEjercicio?> ObtenerIntentoRecienteDuplicadoAsync(
        int usuarioId,
        int ejercicioId,
        string codigo,
        DateTime desdeUtc,
        CancellationToken ct = default) =>
        DbSet
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.CriteriosEvaluacion)
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.ConceptosPermitidos)
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.ConceptosNoPermitidos)
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.CasosPrueba)
            .Include(intento => intento.Correccion)
                .ThenInclude(correccion => correccion!.ResultadosCriterios)
            .Include(intento => intento.Correccion)
                .ThenInclude(correccion => correccion!.ItemsFeedback)
            .FirstOrDefaultAsync(
                intento =>
                    intento.UsuarioId == usuarioId &&
                    intento.EjercicioId == ejercicioId &&
                    intento.CodigoEnviado == codigo &&
                    intento.FechaEnvio >= desdeUtc,
                ct);

    public Task<IntentoEjercicio?> ObtenerParaReintentoAsync(
        int intentoId,
        int usuarioId,
        CancellationToken ct = default) =>
        DbSet
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.CriteriosEvaluacion)
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.ConceptosPermitidos)
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.ConceptosNoPermitidos)
            .Include(intento => intento.Ejercicio)
                .ThenInclude(ejercicio => ejercicio.CasosPrueba)
            .Include(intento => intento.Correccion)
                .ThenInclude(correccion => correccion!.ResultadosCriterios)
            .Include(intento => intento.Correccion)
                .ThenInclude(correccion => correccion!.ItemsFeedback)
            .FirstOrDefaultAsync(
                intento =>
                    intento.Id == intentoId &&
                    intento.UsuarioId == usuarioId,
                ct);
}

public sealed class RepositorioProgresos :
    Repositorio<ProgresoEjercicio>, IRepositorioProgresos
{
    public RepositorioProgresos(EduCodeContext contexto) : base(contexto) { }

    public Task<ProgresoEjercicio?> ObtenerAsync(
        int usuarioId, int ejercicioId, CancellationToken ct = default) =>
        DbSet
            .Include(progreso => progreso.Ejercicio)
            .FirstOrDefaultAsync(
                progreso =>
                    progreso.UsuarioId == usuarioId &&
                    progreso.EjercicioId == ejercicioId,
                ct);

    public async Task<IReadOnlyCollection<ProgresoEjercicio>>
        ObtenerPorUsuarioAsync(
            int usuarioId,
            CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Include(progreso => progreso.Ejercicio)
            .Where(progreso => progreso.UsuarioId == usuarioId)
            .ToListAsync(ct);
}

public sealed class RepositorioPistas :
    Repositorio<PistaEjercicio>, IRepositorioPistas
{
    public RepositorioPistas(EduCodeContext contexto) : base(contexto) { }

    public Task<PistaEjercicio?> ObtenerSiguienteDisponibleAsync(
        int ejercicioId,
        int cantidadIntentos,
        int nivelYaUsado,
        CancellationToken ct = default) =>
        DbSet
            .AsNoTracking()
            .Where(pista =>
                pista.EjercicioId == ejercicioId &&
                pista.Activa &&
                pista.IntentosMinimosRequeridos <= cantidadIntentos &&
                pista.Nivel > nivelYaUsado)
            .OrderBy(pista => pista.Nivel)
            .FirstOrDefaultAsync(ct);
}


public sealed class RepositorioSolicitudesIA :
    Repositorio<SolicitudIA>, IRepositorioSolicitudesIA
{
    public RepositorioSolicitudesIA(EduCodeContext contexto) : base(contexto) { }

    public Task<int> ContarPorUsuarioDesdeAsync(
        int usuarioId,
        DateTime desdeUtc,
        CancellationToken ct = default) =>
        DbSet.CountAsync(
            solicitud =>
                solicitud.UsuarioId == usuarioId &&
                solicitud.IntentoEjercicioId != null &&
                solicitud.FechaSolicitud >= desdeUtc &&
                solicitud.Estado == EstadoSolicitudIA.Exitosa,
            ct);

    public Task<int> ContarTutorPorUsuarioDesdeAsync(
        int usuarioId,
        DateTime desdeUtc,
        CancellationToken ct = default) =>
        DbSet.CountAsync(
            solicitud =>
                solicitud.UsuarioId == usuarioId &&
                solicitud.IntentoEjercicioId == null &&
                solicitud.Proveedor.StartsWith("Tutor IA") &&
                solicitud.FechaSolicitud >= desdeUtc &&
                solicitud.Estado == EstadoSolicitudIA.Exitosa,
            ct);
}


public sealed class RepositorioGamificacion : IRepositorioGamificacion
{
    private readonly EduCodeContext _contexto;

    public RepositorioGamificacion(EduCodeContext contexto) =>
        _contexto = contexto;

    public Task<Usuario?> ObtenerUsuarioAsync(
        int usuarioId,
        CancellationToken ct = default) =>
        _contexto.Usuarios.FirstOrDefaultAsync(
            usuario => usuario.Id == usuarioId,
            ct);

    public async Task<IReadOnlyCollection<Usuario>> ObtenerUsuariosConProgresosAsync(
        CancellationToken ct = default) =>
        await _contexto.Usuarios
            .AsNoTracking()
            .Include(usuario => usuario.ProgresosEjercicios)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<IntentoEjercicio>> ObtenerIntentosUsuarioAsync(
        int usuarioId,
        CancellationToken ct = default) =>
        await _contexto.IntentosEjercicios
            .AsNoTracking()
            .Where(intento => intento.UsuarioId == usuarioId)
            .OrderBy(intento => intento.FechaEnvio)
            .ThenBy(intento => intento.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<Logro>> ObtenerLogrosActivosAsync(
        CancellationToken ct = default) =>
        await _contexto.Logros
            .AsNoTracking()
            .Where(logro => logro.Activo)
            .OrderBy(logro => logro.Tipo)
            .ThenBy(logro => logro.ValorObjetivo)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<UsuarioLogro>> ObtenerLogrosUsuarioAsync(
        int usuarioId,
        CancellationToken ct = default) =>
        await _contexto.UsuariosLogros
            .AsNoTracking()
            .Include(usuarioLogro => usuarioLogro.Logro)
            .Where(usuarioLogro => usuarioLogro.UsuarioId == usuarioId)
            .ToListAsync(ct);

    public async Task AgregarUsuarioLogroAsync(
        UsuarioLogro usuarioLogro,
        CancellationToken ct = default) =>
        await _contexto.UsuariosLogros.AddAsync(usuarioLogro, ct);
}
