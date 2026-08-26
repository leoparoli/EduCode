namespace EduCode.Dominio.Enums;

public enum RolUsuario { Alumno = 1, Docente = 2, Administrador = 3 }
public enum EstadoUsuario { PendienteActivacion = 1, Activo = 2, Bloqueado = 3, Eliminado = 4 }
public enum Semestre { Primero = 1, Segundo = 2 }
public enum EstadoInscripcion { Activa = 1, Finalizada = 2, Abandonada = 3, Suspendida = 4 }
public enum NivelDificultad { Inicial = 1, Basico = 2, Intermedio = 3, Avanzado = 4, Desafio = 5 }
public enum TipoEjercicio { Codigo = 1, RespuestaAbierta = 2, OpcionMultiple = 3, VerdaderoFalso = 4, AnalisisCodigo = 5, CompletarCodigo = 6 }
public enum TipoCriterio { Funcional = 1, Sintactico = 2, Conceptual = 3, RestriccionAcademica = 4, CalidadCodigo = 5, CasoPrueba = 6 }
public enum VisibilidadCasoPrueba { Publico = 1, Oculto = 2 }
public enum EstadoIntento
{
    Enviado = 1,
    EnCorreccion = 2,
    Correcto = 3,
    ParcialmenteCorrecto = 4,
    Incorrecto = 5,
    ErrorCorreccion = 6,
    PendienteCorreccion = 7,
    CorreccionProvisoria = 8
}
public enum OrigenCorreccion { InteligenciaArtificial = 1, Docente = 2, Automatica = 3, Mixta = 4 }
public enum TipoItemFeedback { AspectoCorrecto = 1, AspectoAMejorar = 2, Advertencia = 3, Recomendacion = 4 }
public enum EstadoProgreso { Bloqueado = 1, Disponible = 2, EnProceso = 3, Aprobado = 4, Completado = 5 }
public enum TipoEvaluacion { PrimerParcial = 1, SegundoParcial = 2, Examen = 3, Simulacro = 4 }
public enum ModalidadEvaluacion { Practica = 1, Simulacro = 2 }
public enum EstadoEntregaEvaluacion { EnCurso = 1, Finalizada = 2, Vencida = 3, Corregida = 4, Abandonada = 5 }
public enum TipoEtapaObligatorio { AnalisisConsigna = 1, IdentificacionClases = 2, ModeladoDominio = 3, Implementacion = 4, Pruebas = 5, Documentacion = 6, RevisionFinal = 7 }
public enum EstadoProgresoObligatorio { NoIniciado = 1, EnProceso = 2, Completado = 3, Abandonado = 4 }
public enum TipoLogro
{
    EjerciciosCompletados = 1,
    RachaEjercicios = 2,
    TemaCompletado = 3,
    PuntajeObtenido = 4,
    EvaluacionAprobada = 5,
    MejoraPersonal = 6
}
public enum TipoAccionAuditoria { Alta = 1, Modificacion = 2, Baja = 3, InicioSesion = 4, CorreccionIA = 5, PublicacionContenido = 6, BloqueoUsuario = 7 }
public enum EstadoSolicitudIA { Pendiente = 1, Exitosa = 2, Fallida = 3, Rechazada = 4 }

public enum EstadoInvitacionDocente
{
    Disponible = 1,
    Utilizada = 2,
    Revocada = 3,
    Vencida = 4
}

public enum EstadoPropuestaEjercicio
{
    Enviada = 1,
    EnRevision = 2,
    Aprobada = 3,
    RequiereAjustes = 4,
    Rechazada = 5
}

public enum CategoriaOpinion
{
    ExperienciaGeneral = 1,
    TutorIA = 2,
    Ejercicios = 3,
    DisenoYAccesibilidad = 4,
    ReporteDeError = 5,
    Sugerencia = 6
}

public enum EstadoOpinion
{
    Pendiente = 1,
    Revisada = 2,
    Considerada = 3,
    Archivada = 4
}
