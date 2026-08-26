using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using EduCode.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EduCode.AccesoDatos.Repositorios;

public sealed class RepositorioPortalAcademico : IRepositorioPortalAcademico
{
    private readonly EduCodeContext _contexto;

    public RepositorioPortalAcademico(EduCodeContext contexto) =>
        _contexto = contexto;

    public Task<InvitacionDocente?> ObtenerInvitacionPorCodigoAsync(
        string codigo,
        CancellationToken ct = default)
    {
        codigo = (codigo ?? string.Empty).Trim().ToUpperInvariant();
        return _contexto.InvitacionesDocentes
            .Include(i => i.CreadaPorUsuario)
            .Include(i => i.UtilizadaPorUsuario)
            .FirstOrDefaultAsync(i => i.Codigo == codigo, ct);
    }

    public async Task<IReadOnlyCollection<InvitacionDocente>> ObtenerInvitacionesAsync(
        CancellationToken ct = default) =>
        await _contexto.InvitacionesDocentes
            .AsNoTracking()
            .Include(i => i.CreadaPorUsuario)
            .Include(i => i.UtilizadaPorUsuario)
            .OrderBy(i => i.Estado)
            .ThenByDescending(i => i.FechaCreacion)
            .ToListAsync(ct);

    public Task<PropuestaEjercicio?> ObtenerPropuestaAsync(
        int id,
        CancellationToken ct = default) =>
        _contexto.PropuestasEjercicios
            .Include(p => p.Docente)
            .Include(p => p.Tema)
            .Include(p => p.RevisadaPorUsuario)
            .Include(p => p.EjercicioPublicado)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyCollection<PropuestaEjercicio>> ObtenerPropuestasDocenteAsync(
        int docenteId,
        CancellationToken ct = default) =>
        await _contexto.PropuestasEjercicios
            .AsNoTracking()
            .Include(p => p.Docente)
            .Include(p => p.Tema)
            .Include(p => p.RevisadaPorUsuario)
            .Where(p => p.DocenteId == docenteId)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<PropuestaEjercicio>> ObtenerPropuestasAdministracionAsync(
        CancellationToken ct = default) =>
        await _contexto.PropuestasEjercicios
            .AsNoTracking()
            .Include(p => p.Docente)
            .Include(p => p.Tema)
            .Include(p => p.RevisadaPorUsuario)
            .OrderBy(p => p.Estado == EstadoPropuestaEjercicio.Enviada ? 0 : 1)
            .ThenByDescending(p => p.FechaCreacion)
            .ToListAsync(ct);

    public async Task<PanelDocenteDTO> ObtenerPanelDocenteAsync(
        int docenteId,
        CancellationToken ct = default)
    {
        DateTime desde7 = DateTime.UtcNow.AddDays(-7);
        DateTime desde30 = DateTime.UtcNow.AddDays(-30);

        List<Grupo> grupos = await _contexto.Grupos
            .AsNoTracking()
            .Include(g => g.Curso)
            .Include(g => g.PeriodoAcademico)
            .Include(g => g.Inscripciones)
                .ThenInclude(i => i.Usuario)
            .Where(g => g.Activo && g.DocenteResponsableId == docenteId)
            .OrderBy(g => g.Nombre)
            .ToListAsync(ct);

        int[] alumnosIds = grupos
            .SelectMany(g => g.Inscripciones)
            .Where(i => i.Estado == EstadoInscripcion.Activa)
            .Select(i => i.UsuarioId)
            .Distinct()
            .ToArray();

        List<IntentoEjercicio> intentos;
        if (alumnosIds.Length == 0)
        {
            intentos = new List<IntentoEjercicio>();
        }
        else
        {
            intentos = await _contexto.IntentosEjercicios
                .AsNoTracking()
                .Include(i => i.Ejercicio)
                    .ThenInclude(e => e.Tema)
                .Where(i => alumnosIds.Contains(i.UsuarioId))
                .ToListAsync(ct);
        }

        List<IntentoEjercicio> intentos30 = intentos
            .Where(i => i.FechaEnvio >= desde30)
            .ToList();

        List<GrupoObservacionDTO> resumenGrupos = grupos.Select(grupo =>
        {
            int[] idsGrupo = grupo.Inscripciones
                .Where(i => i.Estado == EstadoInscripcion.Activa)
                .Select(i => i.UsuarioId)
                .Distinct()
                .ToArray();
            List<IntentoEjercicio> delGrupo = intentos30
                .Where(i => idsGrupo.Contains(i.UsuarioId))
                .ToList();
            int evaluados = delGrupo.Count(i => i.PuntajeObtenido.HasValue);
            return new GrupoObservacionDTO
            {
                Id = grupo.Id,
                Nombre = grupo.Nombre,
                Curso = grupo.Curso.Nombre,
                Periodo = $"{grupo.PeriodoAcademico.Semestre} {grupo.PeriodoAcademico.Anio}",
                Alumnos = idsGrupo.Length,
                ActivosUltimos7Dias = delGrupo
                    .Where(i => i.FechaEnvio >= desde7)
                    .Select(i => i.UsuarioId)
                    .Distinct()
                    .Count(),
                IntentosUltimos30Dias = delGrupo.Count,
                TasaAprobacion = evaluados == 0 ? 0 : Math.Round(
                    delGrupo.Count(i => i.Estado == EstadoIntento.Correcto) * 100m / evaluados,
                    1)
            };
        }).ToList();

        List<TemaObservacionDTO> temas = ConstruirMetricasTemas(intentos30);

        List<AlumnoActividadDTO> actividad = grupos
            .SelectMany(grupo => grupo.Inscripciones
                .Where(inscripcion => inscripcion.Estado == EstadoInscripcion.Activa)
                .Select(inscripcion => new { grupo.Nombre, inscripcion.Usuario }))
            .GroupBy(x => x.Usuario.Id)
            .Select(agrupado =>
            {
                Usuario alumno = agrupado.First().Usuario;
                List<IntentoEjercicio> delAlumno = intentos30
                    .Where(i => i.UsuarioId == alumno.Id)
                    .ToList();
                List<IntentoEjercicio> historicos = intentos
                    .Where(i => i.UsuarioId == alumno.Id)
                    .ToList();
                DateTime? ultima = historicos.Count == 0
                    ? null
                    : historicos.Max(i => i.FechaEnvio);
                return new AlumnoActividadDTO
                {
                    AlumnoId = alumno.Id,
                    Alumno = $"{alumno.Nombre} {alumno.Apellido}",
                    NombreUsuario = alumno.NombreUsuario,
                    Grupo = string.Join(", ", agrupado.Select(x => x.Nombre).Distinct()),
                    UltimaActividad = ultima,
                    IntentosUltimos30Dias = delAlumno.Count,
                    EjerciciosAprobados = historicos
                        .Where(i => i.Estado == EstadoIntento.Correcto)
                        .Select(i => i.EjercicioId)
                        .Distinct()
                        .Count(),
                    Ritmo = CalcularRitmo(ultima, delAlumno.Count)
                };
            })
            .OrderByDescending(a => a.UltimaActividad)
            .ThenBy(a => a.Alumno)
            .ToList();

        int evaluados30 = intentos30.Count(i => i.PuntajeObtenido.HasValue);
        int activos7 = intentos30.Where(i => i.FechaEnvio >= desde7)
            .Select(i => i.UsuarioId).Distinct().Count();
        int activos30 = intentos30.Select(i => i.UsuarioId).Distinct().Count();

        return new PanelDocenteDTO
        {
            TotalGrupos = grupos.Count,
            TotalAlumnos = alumnosIds.Length,
            AlumnosActivosUltimos7Dias = activos7,
            AlumnosActivosUltimos30Dias = activos30,
            IntentosUltimos30Dias = intentos30.Count,
            TasaAprobacionUltimos30Dias = evaluados30 == 0 ? 0 : Math.Round(
                intentos30.Count(i => i.Estado == EstadoIntento.Correcto) * 100m / evaluados30,
                1),
            PuntajePromedioUltimos30Dias = PromedioPuntaje(intentos30),
            LecturaGeneral = ConstruirLecturaGeneral(alumnosIds.Length, activos7, intentos30.Count, temas),
            Grupos = resumenGrupos,
            Temas = temas,
            ActividadAlumnos = actividad
        };
    }

    public async Task<PanelAdministracionDTO> ObtenerPanelAdministracionAsync(
        CancellationToken ct = default)
    {
        DateTime desde30 = DateTime.UtcNow.AddDays(-30);
        List<IntentoEjercicio> intentos30 = await _contexto.IntentosEjercicios
            .AsNoTracking()
            .Include(i => i.Ejercicio)
                .ThenInclude(e => e.Tema)
            .Where(i => i.FechaEnvio >= desde30)
            .ToListAsync(ct);

        int evaluados = intentos30.Count(i => i.PuntajeObtenido.HasValue);
        int usuariosActivos = intentos30.Select(i => i.UsuarioId).Distinct().Count();
        List<RegistroAuditoria> auditoria = await _contexto.RegistrosAuditoria
            .AsNoTracking()
            .Include(a => a.Usuario)
            .OrderByDescending(a => a.Fecha)
            .Take(20)
            .ToListAsync(ct);

        return new PanelAdministracionDTO
        {
            TotalUsuarios = await _contexto.Usuarios.CountAsync(u => u.Estado == EstadoUsuario.Activo, ct),
            TotalAlumnos = await _contexto.Usuarios.CountAsync(u => u.Estado == EstadoUsuario.Activo && u.Rol == RolUsuario.Alumno, ct),
            TotalDocentes = await _contexto.Usuarios.CountAsync(u => u.Estado == EstadoUsuario.Activo && u.Rol == RolUsuario.Docente, ct),
            TotalGrupos = await _contexto.Grupos.CountAsync(g => g.Activo, ct),
            UsuariosActivosUltimos30Dias = usuariosActivos,
            IntentosUltimos30Dias = intentos30.Count,
            TasaAprobacionUltimos30Dias = evaluados == 0 ? 0 : Math.Round(
                intentos30.Count(i => i.Estado == EstadoIntento.Correcto) * 100m / evaluados,
                1),
            PuntajePromedioUltimos30Dias = PromedioPuntaje(intentos30),
            SolicitudesIAUltimos30Dias = await _contexto.SolicitudesIA.CountAsync(
                s => s.FechaSolicitud >= desde30, ct),
            FallosIAUltimos30Dias = await _contexto.SolicitudesIA.CountAsync(
                s => s.FechaSolicitud >= desde30 && s.Estado == EstadoSolicitudIA.Fallida, ct),
            PropuestasPendientes = await _contexto.PropuestasEjercicios.CountAsync(
                p => p.Estado == EstadoPropuestaEjercicio.Enviada || p.Estado == EstadoPropuestaEjercicio.EnRevision,
                ct),
            InvitacionesDisponibles = await _contexto.InvitacionesDocentes.CountAsync(
                i => i.Estado == EstadoInvitacionDocente.Disponible && i.FechaExpiracion > DateTime.UtcNow,
                ct),
            Temas = ConstruirMetricasTemas(intentos30),
            AuditoriaReciente = auditoria.Select(a => new AuditoriaResumenDTO
            {
                Id = a.Id,
                Usuario = a.Usuario is null
                    ? "Sistema"
                    : $"{a.Usuario.Nombre} {a.Usuario.Apellido}",
                Accion = a.Accion.ToString(),
                Entidad = a.EntidadAfectada,
                EntidadId = a.EntidadId,
                Detalle = a.DatosNuevos,
                Fecha = a.Fecha
            }).ToList()
        };
    }

    public async Task<EstructuraAcademicaDTO> ObtenerEstructuraAcademicaAsync(
        CancellationToken ct = default)
    {
        List<Usuario> docentes = await _contexto.Usuarios
            .AsNoTracking()
            .Include(u => u.PerfilDocente)
            .Where(u => u.Estado == EstadoUsuario.Activo && u.Rol == RolUsuario.Docente)
            .OrderBy(u => u.Apellido)
            .ThenBy(u => u.Nombre)
            .ToListAsync(ct);

        List<Usuario> alumnos = await _contexto.Usuarios
            .AsNoTracking()
            .Where(u => u.Estado == EstadoUsuario.Activo && u.Rol == RolUsuario.Alumno)
            .OrderBy(u => u.Apellido)
            .ThenBy(u => u.Nombre)
            .ToListAsync(ct);

        List<Curso> cursos = await _contexto.Cursos
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync(ct);

        List<PeriodoAcademico> periodos = await _contexto.PeriodosAcademicos
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderByDescending(p => p.Anio)
            .ThenByDescending(p => p.Semestre)
            .ToListAsync(ct);

        List<Grupo> grupos = await _contexto.Grupos
            .AsNoTracking()
            .Include(g => g.Curso)
            .Include(g => g.PeriodoAcademico)
            .Include(g => g.DocenteResponsable)
            .Include(g => g.Inscripciones)
            .Where(g => g.Activo)
            .OrderBy(g => g.Nombre)
            .ToListAsync(ct);

        return new EstructuraAcademicaDTO
        {
            Docentes = docentes.Select(MapearUsuario).ToList(),
            Alumnos = alumnos.Select(MapearUsuario).ToList(),
            Cursos = cursos.Select(c => new CursoAcademicoDTO { Id = c.Id, Nombre = c.Nombre }).ToList(),
            Periodos = periodos.Select(p => new PeriodoAcademicoDTO
            {
                Id = p.Id,
                Nombre = $"{p.Semestre} {p.Anio}"
            }).ToList(),
            Grupos = grupos.Select(g => new GrupoAcademicoDTO
            {
                Id = g.Id,
                Nombre = g.Nombre,
                CursoId = g.CursoId,
                Curso = g.Curso.Nombre,
                PeriodoId = g.PeriodoAcademicoId,
                Periodo = $"{g.PeriodoAcademico.Semestre} {g.PeriodoAcademico.Anio}",
                DocenteId = g.DocenteResponsableId,
                Docente = g.DocenteResponsable is null
                    ? "Sin asignar"
                    : $"{g.DocenteResponsable.Nombre} {g.DocenteResponsable.Apellido}",
                CantidadAlumnos = g.Inscripciones.Count(i => i.Estado == EstadoInscripcion.Activa)
            }).ToList()
        };
    }

    public Task<Grupo?> ObtenerGrupoAsync(
        int grupoId,
        CancellationToken ct = default) =>
        _contexto.Grupos
            .Include(g => g.Inscripciones)
            .FirstOrDefaultAsync(g => g.Id == grupoId, ct);

    public Task<bool> ExisteInscripcionAsync(
        int alumnoId,
        int grupoId,
        CancellationToken ct = default) =>
        _contexto.InscripcionesCursos.AnyAsync(
            i => i.UsuarioId == alumnoId && i.GrupoId == grupoId,
            ct);

    private static UsuarioAcademicoDTO MapearUsuario(Usuario u) => new()
    {
        Id = u.Id,
        NombreCompleto = $"{u.Nombre} {u.Apellido}",
        NombreUsuario = u.NombreUsuario,
        Email = u.Email,
        Especialidad = u.PerfilDocente?.Especialidad ?? string.Empty
    };

    private static List<TemaObservacionDTO> ConstruirMetricasTemas(
        IReadOnlyCollection<IntentoEjercicio> intentos)
    {
        return intentos
            .Where(i => i.Ejercicio?.Tema is not null)
            .GroupBy(i => new { i.Ejercicio.TemaId, i.Ejercicio.Tema.Nombre })
            .Select(g =>
            {
                int evaluados = g.Count(i => i.PuntajeObtenido.HasValue);
                decimal tasa = evaluados == 0 ? 0 : Math.Round(
                    g.Count(i => i.Estado == EstadoIntento.Correcto) * 100m / evaluados,
                    1);
                return new TemaObservacionDTO
                {
                    TemaId = g.Key.TemaId,
                    Tema = g.Key.Nombre,
                    Intentos = g.Count(),
                    AlumnosQuePracticaron = g.Select(i => i.UsuarioId).Distinct().Count(),
                    TasaAprobacion = tasa,
                    PuntajePromedio = PromedioPuntaje(g),
                    Senal = evaluados == 0
                        ? "Aún sin datos suficientes"
                        : tasa switch
                        {
                            >= 75 => "Buen dominio general",
                            >= 50 => "Conviene reforzar algunos conceptos",
                            _ => "Tema con dificultad marcada"
                        }
                };
            })
            .OrderBy(t => t.Senal == "Aún sin datos suficientes" ? 1 : 0)
            .ThenBy(t => t.TasaAprobacion)
            .ThenByDescending(t => t.Intentos)
            .ToList();
    }

    private static decimal PromedioPuntaje(IEnumerable<IntentoEjercicio> intentos)
    {
        decimal[] valores = intentos
            .Where(i => i.PuntajeObtenido.HasValue)
            .Select(i => i.PuntajeObtenido!.Value)
            .ToArray();
        return valores.Length == 0 ? 0 : Math.Round(valores.Average(), 1);
    }

    private static string CalcularRitmo(DateTime? ultima, int intentos30)
    {
        if (!ultima.HasValue || ultima.Value < DateTime.UtcNow.AddDays(-30))
            return "Sin actividad reciente";
        if (ultima.Value >= DateTime.UtcNow.AddDays(-7) && intentos30 >= 5)
            return "Práctica frecuente";
        if (ultima.Value >= DateTime.UtcNow.AddDays(-7))
            return "Actividad reciente";
        return "Práctica intermitente";
    }

    private static string ConstruirLecturaGeneral(
        int alumnos,
        int activos7,
        int intentos30,
        IReadOnlyCollection<TemaObservacionDTO> temas)
    {
        if (alumnos == 0)
            return "Todavía no hay alumnos asociados a tus grupos.";
        if (intentos30 == 0)
            return "El grupo aún no registró práctica durante los últimos 30 días.";

        TemaObservacionDTO? mayorDificultad = temas
            .Where(t => t.Intentos >= 2 && t.Senal != "Aún sin datos suficientes")
            .OrderBy(t => t.TasaAprobacion)
            .FirstOrDefault();
        string foco = mayorDificultad is null
            ? "Todavía se necesitan más intentos para detectar patrones por tema."
            : $"{mayorDificultad.Tema} aparece como el principal punto a reforzar en clase.";
        return $"{activos7} de {alumnos} estudiantes practicaron durante la última semana. {foco}";
    }
}
