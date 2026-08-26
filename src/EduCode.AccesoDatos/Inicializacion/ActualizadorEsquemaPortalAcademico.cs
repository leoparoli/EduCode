using Microsoft.EntityFrameworkCore;

namespace EduCode.AccesoDatos.Inicializacion;

public static class ActualizadorEsquemaPortalAcademico
{
    public static async Task AplicarAsync(
        EduCodeContext context,
        CancellationToken ct = default)
    {
        const string sql = """
IF OBJECT_ID('InvitacionesDocentes', 'U') IS NULL
BEGIN
    CREATE TABLE InvitacionesDocentes
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvitacionesDocentes PRIMARY KEY,
        Codigo nvarchar(40) NOT NULL,
        CreadaPorUsuarioId int NOT NULL,
        Especialidad nvarchar(150) NOT NULL,
        EmailPermitido nvarchar(150) NULL,
        FechaCreacion datetime2 NOT NULL,
        FechaExpiracion datetime2 NOT NULL,
        Estado int NOT NULL,
        UtilizadaPorUsuarioId int NULL,
        FechaUtilizacion datetime2 NULL,
        CONSTRAINT FK_InvitacionesDocentes_Usuarios_CreadaPorUsuarioId
            FOREIGN KEY (CreadaPorUsuarioId) REFERENCES Usuarios(Id),
        CONSTRAINT FK_InvitacionesDocentes_Usuarios_UtilizadaPorUsuarioId
            FOREIGN KEY (UtilizadaPorUsuarioId) REFERENCES Usuarios(Id)
    );
    CREATE UNIQUE INDEX IX_InvitacionesDocentes_Codigo
        ON InvitacionesDocentes(Codigo);
    CREATE INDEX IX_InvitacionesDocentes_Estado_FechaExpiracion
        ON InvitacionesDocentes(Estado, FechaExpiracion);
END;

IF OBJECT_ID('PropuestasEjercicios', 'U') IS NULL
BEGIN
    CREATE TABLE PropuestasEjercicios
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PropuestasEjercicios PRIMARY KEY,
        DocenteId int NOT NULL,
        TemaId int NOT NULL,
        Titulo nvarchar(150) NOT NULL,
        Consigna nvarchar(max) NOT NULL,
        Dificultad int NOT NULL,
        CodigoInicial nvarchar(max) NOT NULL,
        CriterioPrincipal nvarchar(1000) NOT NULL,
        OrientacionPista nvarchar(1200) NOT NULL,
        FundamentoPedagogico nvarchar(1600) NOT NULL,
        Estado int NOT NULL,
        FechaCreacion datetime2 NOT NULL,
        RevisadaPorUsuarioId int NULL,
        FechaRevision datetime2 NULL,
        ObservacionRevision nvarchar(1600) NOT NULL,
        EjercicioPublicadoId int NULL,
        CONSTRAINT FK_PropuestasEjercicios_Usuarios_DocenteId
            FOREIGN KEY (DocenteId) REFERENCES Usuarios(Id),
        CONSTRAINT FK_PropuestasEjercicios_Temas_TemaId
            FOREIGN KEY (TemaId) REFERENCES Temas(Id),
        CONSTRAINT FK_PropuestasEjercicios_Usuarios_RevisadaPorUsuarioId
            FOREIGN KEY (RevisadaPorUsuarioId) REFERENCES Usuarios(Id),
        CONSTRAINT FK_PropuestasEjercicios_Ejercicios_EjercicioPublicadoId
            FOREIGN KEY (EjercicioPublicadoId) REFERENCES Ejercicios(Id)
    );
    CREATE INDEX IX_PropuestasEjercicios_DocenteId_FechaCreacion
        ON PropuestasEjercicios(DocenteId, FechaCreacion);
    CREATE INDEX IX_PropuestasEjercicios_Estado_FechaCreacion
        ON PropuestasEjercicios(Estado, FechaCreacion);
END;
ELSE IF EXISTS
(
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('PropuestasEjercicios')
      AND c.name = 'Consigna'
      AND t.name = 'nvarchar'
      AND c.max_length <> -1
)
BEGIN
    ALTER TABLE PropuestasEjercicios
        ALTER COLUMN Consigna nvarchar(max) NOT NULL;
END;
""";

        await context.Database.ExecuteSqlRawAsync(sql, ct);
    }
}
