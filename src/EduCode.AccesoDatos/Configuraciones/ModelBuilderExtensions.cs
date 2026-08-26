using EduCode.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace EduCode.AccesoDatos;

public static class ModelBuilderExtensions
{
    public static void AplicarConfiguracionesEduCode(this ModelBuilder b)
    {
        ConfigurarUsuarios(b);
        ConfigurarCursos(b);
        ConfigurarContenido(b);
        ConfigurarIntentos(b);
        ConfigurarEvaluaciones(b);
        ConfigurarObligatorios(b);
        ConfigurarComplementarias(b);
        ConfigurarPortalAcademico(b);
    }

    private static void ConfigurarUsuarios(ModelBuilder b)
    {
        b.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
            e.Property(x => x.Apellido).HasMaxLength(80).IsRequired();
            e.Property(x => x.NombreUsuario).HasMaxLength(50).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.AvatarClave)
                .HasMaxLength(40)
                .HasDefaultValue(CatalogoAvataresPerfil.Predeterminado)
                .IsRequired();
            e.Property(x => x.FotoPerfilRuta).HasMaxLength(260);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.NombreUsuario).IsUnique();
        });

        b.Entity<PerfilDocente>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Especialidad).HasMaxLength(150);
            e.Property(x => x.Biografia).HasMaxLength(2000);
            e.HasOne(x => x.Usuario)
                .WithOne(x => x.PerfilDocente)
                .HasForeignKey<PerfilDocente>(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<TokenActualizacion>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Token).IsUnique();
            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<OpinionUsuario>(e =>
        {
            e.ToTable("OpinionesUsuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Comentario).HasMaxLength(1200).IsRequired();
            e.HasIndex(x => new { x.UsuarioId, x.FechaCreacion });
            e.HasIndex(x => new { x.Estado, x.FechaCreacion });
            e.HasOne(x => x.Usuario)
                .WithMany(x => x.Opiniones)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<CambioNombreUsuario>(e =>
        {
            e.ToTable("CambiosNombreUsuario");
            e.HasKey(x => x.Id);
            e.Property(x => x.NombreAnterior).HasMaxLength(50).IsRequired();
            e.Property(x => x.NombreNuevo).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.UsuarioId, x.FechaCambio });
            e.HasOne(x => x.Usuario)
                .WithMany(x => x.CambiosNombreUsuario)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurarCursos(ModelBuilder b)
    {
        b.Entity<Curso>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            e.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(2000);
            e.HasIndex(x => x.Codigo).IsUnique();
        });

        b.Entity<PeriodoAcademico>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.Anio, x.Semestre }).IsUnique();
        });

        b.Entity<Grupo>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(80).IsRequired();

            e.HasOne(x => x.Curso)
                .WithMany(x => x.Grupos)
                .HasForeignKey(x => x.CursoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.PeriodoAcademico)
                .WithMany(x => x.Grupos)
                .HasForeignKey(x => x.PeriodoAcademicoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.DocenteResponsable)
                .WithMany()
                .HasForeignKey(x => x.DocenteResponsableId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<InscripcionCurso>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PorcentajeProgreso).HasPrecision(8, 2);
            e.Property(x => x.PuntajeAcumulado).HasPrecision(10, 2);
            e.HasIndex(x => new { x.UsuarioId, x.GrupoId }).IsUnique();

            e.HasOne(x => x.Usuario)
                .WithMany(x => x.Inscripciones)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Grupo)
                .WithMany(x => x.Inscripciones)
                .HasForeignKey(x => x.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarContenido(ModelBuilder b)
    {
        b.Entity<Tema>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(2000);
            e.HasIndex(x => new { x.CursoId, x.Orden }).IsUnique();

            e.HasOne(x => x.Curso)
                .WithMany(x => x.Temas)
                .HasForeignKey(x => x.CursoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.TemaPrerequisito)
                .WithMany(x => x.TemasDependientes)
                .HasForeignKey(x => x.TemaPrerequisitoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Ejercicio>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(150).IsRequired();
            e.Property(x => x.Consigna).HasMaxLength(5000).IsRequired();
            e.Property(x => x.Lenguaje).HasMaxLength(30).IsRequired();
            e.Property(x => x.CodigoInicial).HasColumnType("nvarchar(max)");
            e.Property(x => x.VersionRubrica).HasMaxLength(50).IsRequired();

            e.HasOne(x => x.Tema)
                .WithMany(x => x.Ejercicios)
                .HasForeignKey(x => x.TemaId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.EjercicioPrerequisito)
                .WithMany(x => x.EjerciciosDependientes)
                .HasForeignKey(x => x.EjercicioPrerequisitoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.UsuarioCreador)
                .WithMany()
                .HasForeignKey(x => x.UsuarioCreadorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CriterioEvaluacion>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Descripcion).HasMaxLength(600).IsRequired();
            e.Property(x => x.Peso).HasPrecision(8, 2);

            e.HasOne(x => x.Ejercicio)
                .WithMany(x => x.CriteriosEvaluacion)
                .HasForeignKey(x => x.EjercicioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ConceptoPermitido>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(1000);

            e.HasOne(x => x.Ejercicio)
                .WithMany(x => x.ConceptosPermitidos)
                .HasForeignKey(x => x.EjercicioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ConceptoNoPermitido>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            e.Property(x => x.Motivo).HasMaxLength(1000);

            e.HasOne(x => x.Ejercicio)
                .WithMany(x => x.ConceptosNoPermitidos)
                .HasForeignKey(x => x.EjercicioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<CasoPrueba>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.Entrada).HasColumnType("nvarchar(max)");
            e.Property(x => x.SalidaEsperada).HasColumnType("nvarchar(max)");
            e.Property(x => x.Peso).HasPrecision(8, 2);

            e.HasOne(x => x.Ejercicio)
                .WithMany(x => x.CasosPrueba)
                .HasForeignKey(x => x.EjercicioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<PistaEjercicio>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Contenido).HasMaxLength(2000).IsRequired();
            e.Property(x => x.PenalizacionPuntaje).HasPrecision(8, 2);
            e.HasIndex(x => new { x.EjercicioId, x.Nivel }).IsUnique();

            e.HasOne(x => x.Ejercicio)
                .WithMany(x => x.Pistas)
                .HasForeignKey(x => x.EjercicioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<PistaUtilizada>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.PistaEjercicio)
                .WithMany()
                .HasForeignKey(x => x.PistaEjercicioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.IntentoEjercicio)
                .WithMany()
                .HasForeignKey(x => x.IntentoEjercicioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarIntentos(ModelBuilder b)
    {
        b.Entity<IntentoEjercicio>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CodigoEnviado).HasColumnType("nvarchar(max)").IsRequired();
            e.Property(x => x.PuntajeObtenido).HasPrecision(8, 2);
            e.HasIndex(x => new
            {
                x.UsuarioId,
                x.EjercicioId,
                x.NumeroIntento
            }).IsUnique();

            e.HasOne(x => x.Usuario)
                .WithMany(x => x.IntentosEjercicios)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Ejercicio)
                .WithMany(x => x.Intentos)
                .HasForeignKey(x => x.EjercicioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Correccion)
                .WithOne(x => x.IntentoEjercicio)
                .HasForeignKey<CorreccionIntento>(x => x.IntentoEjercicioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<CorreccionIntento>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PuntajeExacto).HasPrecision(5, 2);
            e.Property(x => x.Resumen).HasMaxLength(4000);
            e.Property(x => x.PistaSugerida).HasMaxLength(2000);
            e.Property(x => x.ModeloIAUtilizado).HasMaxLength(100);
            e.Property(x => x.VersionPrompt).HasMaxLength(50);
            e.Property(x => x.VersionRubrica).HasMaxLength(50);
            e.Property(x => x.VersionMotorPuntaje).HasMaxLength(50);
        });

        b.Entity<ResultadoCriterio>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PesoAplicado).HasPrecision(5, 2);
            e.Property(x => x.PuntajeObtenido).HasPrecision(5, 2);
            e.Property(x => x.Evidencia).HasMaxLength(2500);
            e.Property(x => x.Observacion).HasMaxLength(2500);

            e.HasOne(x => x.CorreccionIntento)
                .WithMany(x => x.ResultadosCriterios)
                .HasForeignKey(x => x.CorreccionIntentoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.CriterioEvaluacion)
                .WithMany()
                .HasForeignKey(x => x.CriterioEvaluacionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ItemFeedback>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Descripcion).HasMaxLength(2000).IsRequired();

            e.HasOne(x => x.CorreccionIntento)
                .WithMany(x => x.ItemsFeedback)
                .HasForeignKey(x => x.CorreccionIntentoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ProgresoEjercicio>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.MejorPuntaje).HasPrecision(8, 2);
            e.HasIndex(x => new { x.UsuarioId, x.EjercicioId }).IsUnique();

            e.HasOne(x => x.Usuario)
                .WithMany(x => x.ProgresosEjercicios)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Ejercicio)
                .WithMany()
                .HasForeignKey(x => x.EjercicioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarEvaluaciones(ModelBuilder b)
    {
        b.Entity<Evaluacion>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(180).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(3000);
            e.Property(x => x.Letra).HasMaxLength(20);
            e.Property(x => x.RutaDocumentoPdf).HasMaxLength(500);

            e.HasOne(x => x.Curso)
                .WithMany(x => x.Evaluaciones)
                .HasForeignKey(x => x.CursoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.UsuarioCreador)
                .WithMany()
                .HasForeignKey(x => x.UsuarioCreadorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<EvaluacionEjercicio>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.EvaluacionId, x.EjercicioId }).IsUnique();

            e.HasOne(x => x.Evaluacion)
                .WithMany(x => x.Ejercicios)
                .HasForeignKey(x => x.EvaluacionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Ejercicio)
                .WithMany()
                .HasForeignKey(x => x.EjercicioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<EntregaEvaluacion>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PuntajeObtenido).HasPrecision(8, 2);

            e.HasOne(x => x.Evaluacion)
                .WithMany(x => x.Entregas)
                .HasForeignKey(x => x.EvaluacionId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Usuario)
                .WithMany(x => x.EntregasEvaluaciones)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<RespuestaEvaluacion>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Respuesta).HasColumnType("nvarchar(max)");
            e.Property(x => x.Feedback).HasColumnType("nvarchar(max)");
            e.Property(x => x.PuntajeObtenido).HasPrecision(8, 2);

            e.HasOne(x => x.EntregaEvaluacion)
                .WithMany(x => x.Respuestas)
                .HasForeignKey(x => x.EntregaEvaluacionId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.EvaluacionEjercicio)
                .WithMany()
                .HasForeignKey(x => x.EvaluacionEjercicioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarObligatorios(ModelBuilder b)
    {
        b.Entity<Obligatorio>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(180).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(4000);
            e.Property(x => x.RutaDocumentoPdf).HasMaxLength(500);

            e.HasOne(x => x.Curso)
                .WithMany(x => x.Obligatorios)
                .HasForeignKey(x => x.CursoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.UsuarioCreador)
                .WithMany()
                .HasForeignKey(x => x.UsuarioCreadorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<EtapaObligatorio>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(180).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(3000);
            e.Property(x => x.Instruccion).HasMaxLength(5000);

            e.HasOne(x => x.Obligatorio)
                .WithMany(x => x.Etapas)
                .HasForeignKey(x => x.ObligatorioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<TareaEtapa>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(180).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(2000);

            e.HasOne(x => x.EtapaObligatorio)
                .WithMany(x => x.Tareas)
                .HasForeignKey(x => x.EtapaObligatorioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ProgresoObligatorio>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PorcentajeAvance).HasPrecision(8, 2);
            e.HasIndex(x => new { x.UsuarioId, x.ObligatorioId }).IsUnique();

            e.HasOne(x => x.Usuario)
                .WithMany(x => x.ProgresosObligatorios)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Obligatorio)
                .WithMany(x => x.Progresos)
                .HasForeignKey(x => x.ObligatorioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.EtapaActual)
                .WithMany()
                .HasForeignKey(x => x.EtapaActualId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<EntregaEtapaObligatorio>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Respuesta).HasColumnType("nvarchar(max)");
            e.Property(x => x.Codigo).HasColumnType("nvarchar(max)");
            e.Property(x => x.Feedback).HasColumnType("nvarchar(max)");
            e.Property(x => x.PuntajeObtenido).HasPrecision(8, 2);

            e.HasOne(x => x.ProgresoObligatorio)
                .WithMany(x => x.EntregasEtapas)
                .HasForeignKey(x => x.ProgresoObligatorioId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.EtapaObligatorio)
                .WithMany()
                .HasForeignKey(x => x.EtapaObligatorioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarComplementarias(ModelBuilder b)
    {
        b.Entity<EstadisticaUsuario>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UsuarioId).IsUnique();
            e.Property(x => x.PuntajeTotal).HasPrecision(10, 2);
            e.Property(x => x.PuntajePromedio).HasPrecision(8, 2);

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Logro>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(1000);
            e.Property(x => x.Icono).HasMaxLength(300);
        });

        b.Entity<UsuarioLogro>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UsuarioId, x.LogroId }).IsUnique();

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Logro)
                .WithMany(x => x.Usuarios)
                .HasForeignKey(x => x.LogroId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<NotaDocente>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Contenido).HasMaxLength(4000).IsRequired();

            e.HasOne(x => x.Docente)
                .WithMany()
                .HasForeignKey(x => x.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Alumno)
                .WithMany()
                .HasForeignKey(x => x.AlumnoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Grupo)
                .WithMany()
                .HasForeignKey(x => x.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<RevisionDocente>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Observacion).HasMaxLength(4000);
            e.Property(x => x.PuntajeCorregido).HasPrecision(8, 2);

            e.HasOne(x => x.Docente)
                .WithMany()
                .HasForeignKey(x => x.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.IntentoEjercicio)
                .WithMany()
                .HasForeignKey(x => x.IntentoEjercicioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<RegistroAuditoria>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EntidadAfectada).HasMaxLength(150);
            e.Property(x => x.EntidadId).HasMaxLength(100);
            e.Property(x => x.DireccionIp).HasMaxLength(80);
            e.Property(x => x.DatosAnteriores).HasColumnType("nvarchar(max)");
            e.Property(x => x.DatosNuevos).HasColumnType("nvarchar(max)");

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<SolicitudIA>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Proveedor).HasMaxLength(100);
            e.Property(x => x.Modelo).HasMaxLength(100);
            e.Property(x => x.CostoEstimado).HasPrecision(12, 6);
            e.Property(x => x.MensajeError).HasMaxLength(4000);

            e.HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.IntentoEjercicio)
                .WithMany()
                .HasForeignKey(x => x.IntentoEjercicioId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigurarPortalAcademico(ModelBuilder b)
    {
        b.Entity<InvitacionDocente>(e =>
        {
            e.ToTable("InvitacionesDocentes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Codigo).HasMaxLength(40).IsRequired();
            e.Property(x => x.Especialidad).HasMaxLength(150);
            e.Property(x => x.EmailPermitido).HasMaxLength(150);
            e.HasIndex(x => x.Codigo).IsUnique();
            e.HasIndex(x => new { x.Estado, x.FechaExpiracion });
            e.HasOne(x => x.CreadaPorUsuario)
                .WithMany()
                .HasForeignKey(x => x.CreadaPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.UtilizadaPorUsuario)
                .WithMany()
                .HasForeignKey(x => x.UtilizadaPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<PropuestaEjercicio>(e =>
        {
            e.ToTable("PropuestasEjercicios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(150).IsRequired();
            e.Property(x => x.Consigna).HasColumnType("nvarchar(max)").HasMaxLength(5000).IsRequired();
            e.Property(x => x.CodigoInicial).HasColumnType("nvarchar(max)");
            e.Property(x => x.CriterioPrincipal).HasMaxLength(1000).IsRequired();
            e.Property(x => x.OrientacionPista).HasMaxLength(1200);
            e.Property(x => x.FundamentoPedagogico).HasMaxLength(1600);
            e.Property(x => x.ObservacionRevision).HasMaxLength(1600);
            e.HasIndex(x => new { x.DocenteId, x.FechaCreacion });
            e.HasIndex(x => new { x.Estado, x.FechaCreacion });
            e.HasOne(x => x.Docente)
                .WithMany()
                .HasForeignKey(x => x.DocenteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Tema)
                .WithMany()
                .HasForeignKey(x => x.TemaId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RevisadaPorUsuario)
                .WithMany()
                .HasForeignKey(x => x.RevisadaPorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.EjercicioPublicado)
                .WithMany()
                .HasForeignKey(x => x.EjercicioPublicadoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

}
