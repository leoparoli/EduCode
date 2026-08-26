using EduCode.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace EduCode.AccesoDatos;

public class EduCodeContext : DbContext
{
    public EduCodeContext(DbContextOptions<EduCodeContext> options)
        : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<PerfilDocente> PerfilesDocentes => Set<PerfilDocente>();
    public DbSet<TokenActualizacion> TokensActualizacion => Set<TokenActualizacion>();
    public DbSet<OpinionUsuario> OpinionesUsuarios => Set<OpinionUsuario>();
    public DbSet<CambioNombreUsuario> CambiosNombreUsuario => Set<CambioNombreUsuario>();

    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<PeriodoAcademico> PeriodosAcademicos => Set<PeriodoAcademico>();
    public DbSet<Grupo> Grupos => Set<Grupo>();
    public DbSet<InscripcionCurso> InscripcionesCursos => Set<InscripcionCurso>();

    public DbSet<Tema> Temas => Set<Tema>();
    public DbSet<Ejercicio> Ejercicios => Set<Ejercicio>();
    public DbSet<CriterioEvaluacion> CriteriosEvaluacion => Set<CriterioEvaluacion>();
    public DbSet<ConceptoPermitido> ConceptosPermitidos => Set<ConceptoPermitido>();
    public DbSet<ConceptoNoPermitido> ConceptosNoPermitidos => Set<ConceptoNoPermitido>();
    public DbSet<CasoPrueba> CasosPrueba => Set<CasoPrueba>();
    public DbSet<PistaEjercicio> PistasEjercicios => Set<PistaEjercicio>();
    public DbSet<PistaUtilizada> PistasUtilizadas => Set<PistaUtilizada>();

    public DbSet<IntentoEjercicio> IntentosEjercicios => Set<IntentoEjercicio>();
    public DbSet<CorreccionIntento> CorreccionesIntentos => Set<CorreccionIntento>();
    public DbSet<ResultadoCriterio> ResultadosCriterios => Set<ResultadoCriterio>();
    public DbSet<ItemFeedback> ItemsFeedback => Set<ItemFeedback>();
    public DbSet<ProgresoEjercicio> ProgresosEjercicios => Set<ProgresoEjercicio>();

    public DbSet<Evaluacion> Evaluaciones => Set<Evaluacion>();
    public DbSet<EvaluacionEjercicio> EvaluacionesEjercicios => Set<EvaluacionEjercicio>();
    public DbSet<EntregaEvaluacion> EntregasEvaluaciones => Set<EntregaEvaluacion>();
    public DbSet<RespuestaEvaluacion> RespuestasEvaluaciones => Set<RespuestaEvaluacion>();

    public DbSet<Obligatorio> Obligatorios => Set<Obligatorio>();
    public DbSet<EtapaObligatorio> EtapasObligatorios => Set<EtapaObligatorio>();
    public DbSet<TareaEtapa> TareasEtapas => Set<TareaEtapa>();
    public DbSet<ProgresoObligatorio> ProgresosObligatorios => Set<ProgresoObligatorio>();
    public DbSet<EntregaEtapaObligatorio> EntregasEtapasObligatorios =>
        Set<EntregaEtapaObligatorio>();

    public DbSet<EstadisticaUsuario> EstadisticasUsuarios => Set<EstadisticaUsuario>();
    public DbSet<Logro> Logros => Set<Logro>();
    public DbSet<UsuarioLogro> UsuariosLogros => Set<UsuarioLogro>();
    public DbSet<NotaDocente> NotasDocentes => Set<NotaDocente>();
    public DbSet<RevisionDocente> RevisionesDocentes => Set<RevisionDocente>();
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();
    public DbSet<SolicitudIA> SolicitudesIA => Set<SolicitudIA>();
    public DbSet<InvitacionDocente> InvitacionesDocentes => Set<InvitacionDocente>();
    public DbSet<PropuestaEjercicio> PropuestasEjercicios => Set<PropuestaEjercicio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AplicarConfiguracionesEduCode();
    }
}
