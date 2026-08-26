using EduCode.Dominio.Enums;

namespace EduCode.DTOs;

public sealed class RegistroDocenteInvitadoDTO
{
    public string CodigoInvitacion { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmacionPassword { get; set; } = string.Empty;
    public string Biografia { get; set; } = string.Empty;
}

public sealed class CrearInvitacionDocenteDTO
{
    public string Especialidad { get; set; } = "Programación 1";
    public string? EmailPermitido { get; set; }
    public int DiasVigencia { get; set; } = 15;
}

public sealed class InvitacionDocenteDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public string EmailPermitido { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public string UtilizadaPor { get; set; } = string.Empty;
}

public sealed class CrearPropuestaEjercicioDTO
{
    public int TemaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Consigna { get; set; } = string.Empty;
    public NivelDificultad Dificultad { get; set; } = NivelDificultad.Basico;
    public string CodigoInicial { get; set; } = "function resolver() {\n    // Tu propuesta\n}";
    public string CriterioPrincipal { get; set; } = string.Empty;
    public string OrientacionPista { get; set; } = string.Empty;
    public string FundamentoPedagogico { get; set; } = string.Empty;
}

public sealed class RevisarPropuestaEjercicioDTO
{
    public string Decision { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
}

public sealed class PropuestaEjercicioDTO
{
    public int Id { get; set; }
    public int TemaId { get; set; }
    public string Tema { get; set; } = string.Empty;
    public int DocenteId { get; set; }
    public string Docente { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Consigna { get; set; } = string.Empty;
    public string Dificultad { get; set; } = string.Empty;
    public string CodigoInicial { get; set; } = string.Empty;
    public string CriterioPrincipal { get; set; } = string.Empty;
    public string OrientacionPista { get; set; } = string.Empty;
    public string FundamentoPedagogico { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaRevision { get; set; }
    public string ObservacionRevision { get; set; } = string.Empty;
    public int? EjercicioPublicadoId { get; set; }
}

public sealed class PanelDocenteDTO
{
    public int TotalGrupos { get; set; }
    public int TotalAlumnos { get; set; }
    public int AlumnosActivosUltimos7Dias { get; set; }
    public int AlumnosActivosUltimos30Dias { get; set; }
    public int IntentosUltimos30Dias { get; set; }
    public decimal TasaAprobacionUltimos30Dias { get; set; }
    public decimal PuntajePromedioUltimos30Dias { get; set; }
    public string LecturaGeneral { get; set; } = string.Empty;
    public List<GrupoObservacionDTO> Grupos { get; set; } = [];
    public List<TemaObservacionDTO> Temas { get; set; } = [];
    public List<AlumnoActividadDTO> ActividadAlumnos { get; set; } = [];
}

public sealed class GrupoObservacionDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Curso { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public int Alumnos { get; set; }
    public int ActivosUltimos7Dias { get; set; }
    public int IntentosUltimos30Dias { get; set; }
    public decimal TasaAprobacion { get; set; }
}

public sealed class TemaObservacionDTO
{
    public int TemaId { get; set; }
    public string Tema { get; set; } = string.Empty;
    public int Intentos { get; set; }
    public int AlumnosQuePracticaron { get; set; }
    public decimal TasaAprobacion { get; set; }
    public decimal PuntajePromedio { get; set; }
    public string Senal { get; set; } = string.Empty;
}

public sealed class AlumnoActividadDTO
{
    public int AlumnoId { get; set; }
    public string Alumno { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Grupo { get; set; } = string.Empty;
    public DateTime? UltimaActividad { get; set; }
    public int IntentosUltimos30Dias { get; set; }
    public int EjerciciosAprobados { get; set; }
    public string Ritmo { get; set; } = string.Empty;
}

public sealed class PanelAdministracionDTO
{
    public int TotalUsuarios { get; set; }
    public int TotalAlumnos { get; set; }
    public int TotalDocentes { get; set; }
    public int TotalGrupos { get; set; }
    public int UsuariosActivosUltimos30Dias { get; set; }
    public int IntentosUltimos30Dias { get; set; }
    public decimal TasaAprobacionUltimos30Dias { get; set; }
    public decimal PuntajePromedioUltimos30Dias { get; set; }
    public int SolicitudesIAUltimos30Dias { get; set; }
    public int FallosIAUltimos30Dias { get; set; }
    public int PropuestasPendientes { get; set; }
    public int InvitacionesDisponibles { get; set; }
    public List<TemaObservacionDTO> Temas { get; set; } = [];
    public List<AuditoriaResumenDTO> AuditoriaReciente { get; set; } = [];
}

public sealed class AuditoriaResumenDTO
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

public sealed class EstructuraAcademicaDTO
{
    public List<UsuarioAcademicoDTO> Docentes { get; set; } = [];
    public List<UsuarioAcademicoDTO> Alumnos { get; set; } = [];
    public List<CursoAcademicoDTO> Cursos { get; set; } = [];
    public List<PeriodoAcademicoDTO> Periodos { get; set; } = [];
    public List<GrupoAcademicoDTO> Grupos { get; set; } = [];
}

public sealed class UsuarioAcademicoDTO
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
}

public sealed class CursoAcademicoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public sealed class PeriodoAcademicoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public sealed class GrupoAcademicoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CursoId { get; set; }
    public string Curso { get; set; } = string.Empty;
    public int PeriodoId { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public int? DocenteId { get; set; }
    public string Docente { get; set; } = string.Empty;
    public int CantidadAlumnos { get; set; }
}

public sealed class CrearGrupoDTO
{
    public string Nombre { get; set; } = string.Empty;
    public int CursoId { get; set; }
    public int PeriodoAcademicoId { get; set; }
    public int DocenteId { get; set; }
}

public sealed class InscribirAlumnoDTO
{
    public int AlumnoId { get; set; }
}
