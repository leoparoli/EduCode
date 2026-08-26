using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCode.AccesoDatos.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Logros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Icono = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    ValorObjetivo = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosAcademicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Semestre = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosAcademicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MostrarEnRanking = table.Column<bool>(type: "bit", nullable: false),
                    AvatarClave = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "codigo"),
                    FotoPerfilRuta = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Temas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    TemaPrerequisitoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Temas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Temas_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Temas_Temas_TemaPrerequisitoId",
                        column: x => x.TemaPrerequisitoId,
                        principalTable: "Temas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CambiosNombreUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    NombreAnterior = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreNuevo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CambiosNombreUsuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CambiosNombreUsuario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstadisticasUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    EjerciciosCompletados = table.Column<int>(type: "int", nullable: false),
                    EjerciciosAprobados = table.Column<int>(type: "int", nullable: false),
                    PuntajeTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PuntajePromedio = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    RachaActualDias = table.Column<int>(type: "int", nullable: false),
                    MejorRachaDias = table.Column<int>(type: "int", nullable: false),
                    FechaUltimaActividad = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadisticasUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstadisticasUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Evaluaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Semestre = table.Column<int>(type: "int", nullable: false),
                    Letra = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaOriginal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    PuntajeTotal = table.Column<int>(type: "int", nullable: false),
                    PuntajeMinimoAprobacion = table.Column<int>(type: "int", nullable: false),
                    RutaDocumentoPdf = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Publicada = table.Column<bool>(type: "bit", nullable: false),
                    PerteneceCursoActual = table.Column<bool>(type: "bit", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioCreadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluaciones_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evaluaciones_Usuarios_UsuarioCreadorId",
                        column: x => x.UsuarioCreadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    PeriodoAcademicoId = table.Column<int>(type: "int", nullable: false),
                    DocenteResponsableId = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grupos_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Grupos_PeriodosAcademicos_PeriodoAcademicoId",
                        column: x => x.PeriodoAcademicoId,
                        principalTable: "PeriodosAcademicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Grupos_Usuarios_DocenteResponsableId",
                        column: x => x.DocenteResponsableId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvitacionesDocentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreadaPorUsuarioId = table.Column<int>(type: "int", nullable: false),
                    Especialidad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EmailPermitido = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    UtilizadaPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaUtilizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitacionesDocentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvitacionesDocentes_Usuarios_CreadaPorUsuarioId",
                        column: x => x.CreadaPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvitacionesDocentes_Usuarios_UtilizadaPorUsuarioId",
                        column: x => x.UtilizadaPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Obligatorios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Semestre = table.Column<int>(type: "int", nullable: false),
                    RutaDocumentoPdf = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Publicado = table.Column<bool>(type: "bit", nullable: false),
                    PerteneceCursoActual = table.Column<bool>(type: "bit", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioCreadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obligatorios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Obligatorios_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Obligatorios_Usuarios_UsuarioCreadorId",
                        column: x => x.UsuarioCreadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpinionesUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    Calificacion = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(1200)", maxLength: 1200, nullable: false),
                    AutorizaPublicacion = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpinionesUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpinionesUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfilesDocentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Biografia = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Especialidad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Verificado = table.Column<bool>(type: "bit", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilesDocentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilesDocentes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    Accion = table.Column<int>(type: "int", nullable: false),
                    EntidadAfectada = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EntidadId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DatosAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatosNuevos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DireccionIp = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAuditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosAuditoria_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TokensActualizacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revocado = table.Column<bool>(type: "bit", nullable: false),
                    FechaRevocacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokensActualizacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokensActualizacion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosLogros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    LogroId = table.Column<int>(type: "int", nullable: false),
                    FechaObtencion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosLogros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosLogros_Logros_LogroId",
                        column: x => x.LogroId,
                        principalTable: "Logros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosLogros_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ejercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Consigna = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Dificultad = table.Column<int>(type: "int", nullable: false),
                    PuntajeMaximo = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Lenguaje = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CodigoInicial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CantidadIntentosRecomendada = table.Column<int>(type: "int", nullable: false),
                    PuntajeMinimoAprobacion = table.Column<int>(type: "int", nullable: false),
                    PermitePistas = table.Column<bool>(type: "bit", nullable: false),
                    PermiteVerSolucion = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VersionRubrica = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TemaId = table.Column<int>(type: "int", nullable: false),
                    EjercicioPrerequisitoId = table.Column<int>(type: "int", nullable: true),
                    UsuarioCreadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ejercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ejercicios_Ejercicios_EjercicioPrerequisitoId",
                        column: x => x.EjercicioPrerequisitoId,
                        principalTable: "Ejercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ejercicios_Temas_TemaId",
                        column: x => x.TemaId,
                        principalTable: "Temas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ejercicios_Usuarios_UsuarioCreadorId",
                        column: x => x.UsuarioCreadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntregasEvaluaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluacionId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Modalidad = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PuntajeObtenido = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    Aprobada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntregasEvaluaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntregasEvaluaciones_Evaluaciones_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "Evaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntregasEvaluaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InscripcionesCursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    PorcentajeProgreso = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    PuntajeAcumulado = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InscripcionesCursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InscripcionesCursos_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InscripcionesCursos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotasDocentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocenteId = table.Column<int>(type: "int", nullable: false),
                    AlumnoId = table.Column<int>(type: "int", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: true),
                    Contenido = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Privada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasDocentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasDocentes_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasDocentes_Usuarios_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasDocentes_Usuarios_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EtapasObligatorios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObligatorioId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Instruccion = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Puntaje = table.Column<int>(type: "int", nullable: false),
                    RequiereAprobacion = table.Column<bool>(type: "bit", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtapasObligatorios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtapasObligatorios_Obligatorios_ObligatorioId",
                        column: x => x.ObligatorioId,
                        principalTable: "Obligatorios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CasosPrueba",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EjercicioId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Entrada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalidaEsperada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Visibilidad = table.Column<int>(type: "int", nullable: false),
                    Peso = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CasosPrueba", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CasosPrueba_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConceptosNoPermitidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EjercicioId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptosNoPermitidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConceptosNoPermitidos_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConceptosPermitidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EjercicioId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptosPermitidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConceptosPermitidos_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CriteriosEvaluacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EjercicioId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Peso = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Obligatorio = table.Column<bool>(type: "bit", nullable: false),
                    PorcentajeMinimoObligatorio = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriosEvaluacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriteriosEvaluacion_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluacionesEjercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluacionId = table.Column<int>(type: "int", nullable: false),
                    EjercicioId = table.Column<int>(type: "int", nullable: false),
                    NumeroConsigna = table.Column<int>(type: "int", nullable: false),
                    Puntaje = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluacionesEjercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvaluacionesEjercicios_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluacionesEjercicios_Evaluaciones_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "Evaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntentosEjercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    EjercicioId = table.Column<int>(type: "int", nullable: false),
                    NumeroIntento = table.Column<int>(type: "int", nullable: false),
                    CodigoEnviado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    PuntajeObtenido = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    TiempoResolucionSegundos = table.Column<int>(type: "int", nullable: true),
                    UtilizoPista = table.Column<bool>(type: "bit", nullable: false),
                    SolicitoSolucion = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentosEjercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntentosEjercicios_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntentosEjercicios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PistasEjercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EjercicioId = table.Column<int>(type: "int", nullable: false),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IntentosMinimosRequeridos = table.Column<int>(type: "int", nullable: false),
                    PenalizacionPuntaje = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PistasEjercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PistasEjercicios_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgresosEjercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    EjercicioId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    CantidadIntentos = table.Column<int>(type: "int", nullable: false),
                    MejorPuntaje = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaUltimaActividad = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgresosEjercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgresosEjercicios_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgresosEjercicios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PropuestasEjercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocenteId = table.Column<int>(type: "int", nullable: false),
                    TemaId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Consigna = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Dificultad = table.Column<int>(type: "int", nullable: false),
                    CodigoInicial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CriterioPrincipal = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OrientacionPista = table.Column<string>(type: "nvarchar(1200)", maxLength: 1200, nullable: false),
                    FundamentoPedagogico = table.Column<string>(type: "nvarchar(1600)", maxLength: 1600, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevisadaPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObservacionRevision = table.Column<string>(type: "nvarchar(1600)", maxLength: 1600, nullable: false),
                    EjercicioPublicadoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropuestasEjercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropuestasEjercicios_Ejercicios_EjercicioPublicadoId",
                        column: x => x.EjercicioPublicadoId,
                        principalTable: "Ejercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropuestasEjercicios_Temas_TemaId",
                        column: x => x.TemaId,
                        principalTable: "Temas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropuestasEjercicios_Usuarios_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropuestasEjercicios_Usuarios_RevisadaPorUsuarioId",
                        column: x => x.RevisadaPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgresosObligatorios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ObligatorioId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaUltimaActividad = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EtapaActualId = table.Column<int>(type: "int", nullable: false),
                    PorcentajeAvance = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgresosObligatorios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgresosObligatorios_EtapasObligatorios_EtapaActualId",
                        column: x => x.EtapaActualId,
                        principalTable: "EtapasObligatorios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgresosObligatorios_Obligatorios_ObligatorioId",
                        column: x => x.ObligatorioId,
                        principalTable: "Obligatorios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgresosObligatorios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TareasEtapas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtapaObligatorioId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Obligatoria = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TareasEtapas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TareasEtapas_EtapasObligatorios_EtapaObligatorioId",
                        column: x => x.EtapaObligatorioId,
                        principalTable: "EtapasObligatorios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RespuestasEvaluaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntregaEvaluacionId = table.Column<int>(type: "int", nullable: false),
                    EvaluacionEjercicioId = table.Column<int>(type: "int", nullable: false),
                    Respuesta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaUltimaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PuntajeObtenido = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasEvaluaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespuestasEvaluaciones_EntregasEvaluaciones_EntregaEvaluacionId",
                        column: x => x.EntregaEvaluacionId,
                        principalTable: "EntregasEvaluaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RespuestasEvaluaciones_EvaluacionesEjercicios_EvaluacionEjercicioId",
                        column: x => x.EvaluacionEjercicioId,
                        principalTable: "EvaluacionesEjercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CorreccionesIntentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IntentoEjercicioId = table.Column<int>(type: "int", nullable: false),
                    Origen = table.Column<int>(type: "int", nullable: false),
                    Resumen = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    PistaSugerida = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PuntajeExacto = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PuntajeMostrado = table.Column<int>(type: "int", nullable: false),
                    Aprobado = table.Column<bool>(type: "bit", nullable: false),
                    CumpleCriteriosObligatorios = table.Column<bool>(type: "bit", nullable: false),
                    RequiereRevisionDocente = table.Column<bool>(type: "bit", nullable: false),
                    ModeloIAUtilizado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VersionPrompt = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VersionRubrica = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VersionMotorPuntaje = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaCorreccion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorreccionesIntentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorreccionesIntentos_IntentosEjercicios_IntentoEjercicioId",
                        column: x => x.IntentoEjercicioId,
                        principalTable: "IntentosEjercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevisionesDocentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocenteId = table.Column<int>(type: "int", nullable: false),
                    IntentoEjercicioId = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    PuntajeCorregido = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionesDocentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevisionesDocentes_IntentosEjercicios_IntentoEjercicioId",
                        column: x => x.IntentoEjercicioId,
                        principalTable: "IntentosEjercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionesDocentes_Usuarios_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesIA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    IntentoEjercicioId = table.Column<int>(type: "int", nullable: true),
                    Proveedor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TokensEntrada = table.Column<int>(type: "int", nullable: false),
                    TokensSalida = table.Column<int>(type: "int", nullable: false),
                    CostoEstimado = table.Column<decimal>(type: "decimal(12,6)", precision: 12, scale: 6, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    MensajeError = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRespuesta = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesIA_IntentosEjercicios_IntentoEjercicioId",
                        column: x => x.IntentoEjercicioId,
                        principalTable: "IntentosEjercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SolicitudesIA_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PistasUtilizadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PistaEjercicioId = table.Column<int>(type: "int", nullable: false),
                    IntentoEjercicioId = table.Column<int>(type: "int", nullable: true),
                    FechaUtilizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PistasUtilizadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PistasUtilizadas_IntentosEjercicios_IntentoEjercicioId",
                        column: x => x.IntentoEjercicioId,
                        principalTable: "IntentosEjercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PistasUtilizadas_PistasEjercicios_PistaEjercicioId",
                        column: x => x.PistaEjercicioId,
                        principalTable: "PistasEjercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PistasUtilizadas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntregasEtapasObligatorios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgresoObligatorioId = table.Column<int>(type: "int", nullable: false),
                    EtapaObligatorioId = table.Column<int>(type: "int", nullable: false),
                    Respuesta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aprobada = table.Column<bool>(type: "bit", nullable: false),
                    PuntajeObtenido = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntregasEtapasObligatorios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntregasEtapasObligatorios_EtapasObligatorios_EtapaObligatorioId",
                        column: x => x.EtapaObligatorioId,
                        principalTable: "EtapasObligatorios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntregasEtapasObligatorios_ProgresosObligatorios_ProgresoObligatorioId",
                        column: x => x.ProgresoObligatorioId,
                        principalTable: "ProgresosObligatorios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemsFeedback",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorreccionIntentoId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemsFeedback_CorreccionesIntentos_CorreccionIntentoId",
                        column: x => x.CorreccionIntentoId,
                        principalTable: "CorreccionesIntentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResultadosCriterios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorreccionIntentoId = table.Column<int>(type: "int", nullable: false),
                    CriterioEvaluacionId = table.Column<int>(type: "int", nullable: false),
                    PorcentajeCumplimiento = table.Column<int>(type: "int", nullable: false),
                    PesoAplicado = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PuntajeObtenido = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Evidencia = table.Column<string>(type: "nvarchar(2500)", maxLength: 2500, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(2500)", maxLength: 2500, nullable: false),
                    CumplimientoObligatorioAlcanzado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadosCriterios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultadosCriterios_CorreccionesIntentos_CorreccionIntentoId",
                        column: x => x.CorreccionIntentoId,
                        principalTable: "CorreccionesIntentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResultadosCriterios_CriteriosEvaluacion_CriterioEvaluacionId",
                        column: x => x.CriterioEvaluacionId,
                        principalTable: "CriteriosEvaluacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CambiosNombreUsuario_UsuarioId_FechaCambio",
                table: "CambiosNombreUsuario",
                columns: new[] { "UsuarioId", "FechaCambio" });

            migrationBuilder.CreateIndex(
                name: "IX_CasosPrueba_EjercicioId",
                table: "CasosPrueba",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosNoPermitidos_EjercicioId",
                table: "ConceptosNoPermitidos",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosPermitidos_EjercicioId",
                table: "ConceptosPermitidos",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_CorreccionesIntentos_IntentoEjercicioId",
                table: "CorreccionesIntentos",
                column: "IntentoEjercicioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriteriosEvaluacion_EjercicioId",
                table: "CriteriosEvaluacion",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_Codigo",
                table: "Cursos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_EjercicioPrerequisitoId",
                table: "Ejercicios",
                column: "EjercicioPrerequisitoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_TemaId",
                table: "Ejercicios",
                column: "TemaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_UsuarioCreadorId",
                table: "Ejercicios",
                column: "UsuarioCreadorId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasEtapasObligatorios_EtapaObligatorioId",
                table: "EntregasEtapasObligatorios",
                column: "EtapaObligatorioId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasEtapasObligatorios_ProgresoObligatorioId",
                table: "EntregasEtapasObligatorios",
                column: "ProgresoObligatorioId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasEvaluaciones_EvaluacionId",
                table: "EntregasEvaluaciones",
                column: "EvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasEvaluaciones_UsuarioId",
                table: "EntregasEvaluaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_EstadisticasUsuarios_UsuarioId",
                table: "EstadisticasUsuarios",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtapasObligatorios_ObligatorioId",
                table: "EtapasObligatorios",
                column: "ObligatorioId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluaciones_CursoId",
                table: "Evaluaciones",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluaciones_UsuarioCreadorId",
                table: "Evaluaciones",
                column: "UsuarioCreadorId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionesEjercicios_EjercicioId",
                table: "EvaluacionesEjercicios",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluacionesEjercicios_EvaluacionId_EjercicioId",
                table: "EvaluacionesEjercicios",
                columns: new[] { "EvaluacionId", "EjercicioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_CursoId",
                table: "Grupos",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_DocenteResponsableId",
                table: "Grupos",
                column: "DocenteResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_PeriodoAcademicoId",
                table: "Grupos",
                column: "PeriodoAcademicoId");

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesCursos_GrupoId",
                table: "InscripcionesCursos",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesCursos_UsuarioId_GrupoId",
                table: "InscripcionesCursos",
                columns: new[] { "UsuarioId", "GrupoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntentosEjercicios_EjercicioId",
                table: "IntentosEjercicios",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_IntentosEjercicios_UsuarioId_EjercicioId_NumeroIntento",
                table: "IntentosEjercicios",
                columns: new[] { "UsuarioId", "EjercicioId", "NumeroIntento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvitacionesDocentes_Codigo",
                table: "InvitacionesDocentes",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvitacionesDocentes_CreadaPorUsuarioId",
                table: "InvitacionesDocentes",
                column: "CreadaPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_InvitacionesDocentes_Estado_FechaExpiracion",
                table: "InvitacionesDocentes",
                columns: new[] { "Estado", "FechaExpiracion" });

            migrationBuilder.CreateIndex(
                name: "IX_InvitacionesDocentes_UtilizadaPorUsuarioId",
                table: "InvitacionesDocentes",
                column: "UtilizadaPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsFeedback_CorreccionIntentoId",
                table: "ItemsFeedback",
                column: "CorreccionIntentoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasDocentes_AlumnoId",
                table: "NotasDocentes",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasDocentes_DocenteId",
                table: "NotasDocentes",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasDocentes_GrupoId",
                table: "NotasDocentes",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Obligatorios_CursoId",
                table: "Obligatorios",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Obligatorios_UsuarioCreadorId",
                table: "Obligatorios",
                column: "UsuarioCreadorId");

            migrationBuilder.CreateIndex(
                name: "IX_OpinionesUsuarios_Estado_FechaCreacion",
                table: "OpinionesUsuarios",
                columns: new[] { "Estado", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_OpinionesUsuarios_UsuarioId_FechaCreacion",
                table: "OpinionesUsuarios",
                columns: new[] { "UsuarioId", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesDocentes_UsuarioId",
                table: "PerfilesDocentes",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosAcademicos_Anio_Semestre",
                table: "PeriodosAcademicos",
                columns: new[] { "Anio", "Semestre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PistasEjercicios_EjercicioId_Nivel",
                table: "PistasEjercicios",
                columns: new[] { "EjercicioId", "Nivel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PistasUtilizadas_IntentoEjercicioId",
                table: "PistasUtilizadas",
                column: "IntentoEjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_PistasUtilizadas_PistaEjercicioId",
                table: "PistasUtilizadas",
                column: "PistaEjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_PistasUtilizadas_UsuarioId",
                table: "PistasUtilizadas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosEjercicios_EjercicioId",
                table: "ProgresosEjercicios",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosEjercicios_UsuarioId_EjercicioId",
                table: "ProgresosEjercicios",
                columns: new[] { "UsuarioId", "EjercicioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosObligatorios_EtapaActualId",
                table: "ProgresosObligatorios",
                column: "EtapaActualId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosObligatorios_ObligatorioId",
                table: "ProgresosObligatorios",
                column: "ObligatorioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgresosObligatorios_UsuarioId_ObligatorioId",
                table: "ProgresosObligatorios",
                columns: new[] { "UsuarioId", "ObligatorioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasEjercicios_DocenteId_FechaCreacion",
                table: "PropuestasEjercicios",
                columns: new[] { "DocenteId", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasEjercicios_EjercicioPublicadoId",
                table: "PropuestasEjercicios",
                column: "EjercicioPublicadoId");

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasEjercicios_Estado_FechaCreacion",
                table: "PropuestasEjercicios",
                columns: new[] { "Estado", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasEjercicios_RevisadaPorUsuarioId",
                table: "PropuestasEjercicios",
                column: "RevisadaPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasEjercicios_TemaId",
                table: "PropuestasEjercicios",
                column: "TemaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAuditoria_UsuarioId",
                table: "RegistrosAuditoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasEvaluaciones_EntregaEvaluacionId",
                table: "RespuestasEvaluaciones",
                column: "EntregaEvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasEvaluaciones_EvaluacionEjercicioId",
                table: "RespuestasEvaluaciones",
                column: "EvaluacionEjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosCriterios_CorreccionIntentoId",
                table: "ResultadosCriterios",
                column: "CorreccionIntentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosCriterios_CriterioEvaluacionId",
                table: "ResultadosCriterios",
                column: "CriterioEvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionesDocentes_DocenteId",
                table: "RevisionesDocentes",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionesDocentes_IntentoEjercicioId",
                table: "RevisionesDocentes",
                column: "IntentoEjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesIA_IntentoEjercicioId",
                table: "SolicitudesIA",
                column: "IntentoEjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesIA_UsuarioId",
                table: "SolicitudesIA",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TareasEtapas_EtapaObligatorioId",
                table: "TareasEtapas",
                column: "EtapaObligatorioId");

            migrationBuilder.CreateIndex(
                name: "IX_Temas_CursoId_Orden",
                table: "Temas",
                columns: new[] { "CursoId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Temas_TemaPrerequisitoId",
                table: "Temas",
                column: "TemaPrerequisitoId");

            migrationBuilder.CreateIndex(
                name: "IX_TokensActualizacion_Token",
                table: "TokensActualizacion",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokensActualizacion_UsuarioId",
                table: "TokensActualizacion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosLogros_LogroId",
                table: "UsuariosLogros",
                column: "LogroId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosLogros_UsuarioId_LogroId",
                table: "UsuariosLogros",
                columns: new[] { "UsuarioId", "LogroId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CambiosNombreUsuario");

            migrationBuilder.DropTable(
                name: "CasosPrueba");

            migrationBuilder.DropTable(
                name: "ConceptosNoPermitidos");

            migrationBuilder.DropTable(
                name: "ConceptosPermitidos");

            migrationBuilder.DropTable(
                name: "EntregasEtapasObligatorios");

            migrationBuilder.DropTable(
                name: "EstadisticasUsuarios");

            migrationBuilder.DropTable(
                name: "InscripcionesCursos");

            migrationBuilder.DropTable(
                name: "InvitacionesDocentes");

            migrationBuilder.DropTable(
                name: "ItemsFeedback");

            migrationBuilder.DropTable(
                name: "NotasDocentes");

            migrationBuilder.DropTable(
                name: "OpinionesUsuarios");

            migrationBuilder.DropTable(
                name: "PerfilesDocentes");

            migrationBuilder.DropTable(
                name: "PistasUtilizadas");

            migrationBuilder.DropTable(
                name: "ProgresosEjercicios");

            migrationBuilder.DropTable(
                name: "PropuestasEjercicios");

            migrationBuilder.DropTable(
                name: "RegistrosAuditoria");

            migrationBuilder.DropTable(
                name: "RespuestasEvaluaciones");

            migrationBuilder.DropTable(
                name: "ResultadosCriterios");

            migrationBuilder.DropTable(
                name: "RevisionesDocentes");

            migrationBuilder.DropTable(
                name: "SolicitudesIA");

            migrationBuilder.DropTable(
                name: "TareasEtapas");

            migrationBuilder.DropTable(
                name: "TokensActualizacion");

            migrationBuilder.DropTable(
                name: "UsuariosLogros");

            migrationBuilder.DropTable(
                name: "ProgresosObligatorios");

            migrationBuilder.DropTable(
                name: "Grupos");

            migrationBuilder.DropTable(
                name: "PistasEjercicios");

            migrationBuilder.DropTable(
                name: "EntregasEvaluaciones");

            migrationBuilder.DropTable(
                name: "EvaluacionesEjercicios");

            migrationBuilder.DropTable(
                name: "CorreccionesIntentos");

            migrationBuilder.DropTable(
                name: "CriteriosEvaluacion");

            migrationBuilder.DropTable(
                name: "Logros");

            migrationBuilder.DropTable(
                name: "EtapasObligatorios");

            migrationBuilder.DropTable(
                name: "PeriodosAcademicos");

            migrationBuilder.DropTable(
                name: "Evaluaciones");

            migrationBuilder.DropTable(
                name: "IntentosEjercicios");

            migrationBuilder.DropTable(
                name: "Obligatorios");

            migrationBuilder.DropTable(
                name: "Ejercicios");

            migrationBuilder.DropTable(
                name: "Temas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Cursos");
        }
    }
}
