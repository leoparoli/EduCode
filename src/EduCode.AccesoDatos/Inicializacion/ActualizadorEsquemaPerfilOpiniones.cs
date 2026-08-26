using Microsoft.EntityFrameworkCore;

namespace EduCode.AccesoDatos.Inicializacion;

public static class ActualizadorEsquemaPerfilOpiniones
{
    public static async Task AplicarAsync(
        EduCodeContext context,
        CancellationToken ct = default)
    {
        const string sql = """
IF COL_LENGTH('Usuarios', 'AvatarClave') IS NULL
BEGIN
    ALTER TABLE Usuarios ADD AvatarClave nvarchar(40) NOT NULL
        CONSTRAINT DF_Usuarios_AvatarClave DEFAULT 'codigo';
END;

IF COL_LENGTH('Usuarios', 'FotoPerfilRuta') IS NULL
BEGIN
    ALTER TABLE Usuarios ADD FotoPerfilRuta nvarchar(260) NULL;
END;

IF OBJECT_ID('OpinionesUsuarios', 'U') IS NULL
BEGIN
    CREATE TABLE OpinionesUsuarios
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OpinionesUsuarios PRIMARY KEY,
        UsuarioId int NOT NULL,
        Categoria int NOT NULL,
        Calificacion int NOT NULL,
        Comentario nvarchar(1200) NOT NULL,
        AutorizaPublicacion bit NOT NULL,
        Estado int NOT NULL,
        FechaCreacion datetime2 NOT NULL,
        FechaRevision datetime2 NULL,
        CONSTRAINT FK_OpinionesUsuarios_Usuarios_UsuarioId
            FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_OpinionesUsuarios_UsuarioId_FechaCreacion
        ON OpinionesUsuarios(UsuarioId, FechaCreacion);
    CREATE INDEX IX_OpinionesUsuarios_Estado_FechaCreacion
        ON OpinionesUsuarios(Estado, FechaCreacion);
END;


IF OBJECT_ID('CambiosNombreUsuario', 'U') IS NULL
BEGIN
    CREATE TABLE CambiosNombreUsuario
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CambiosNombreUsuario PRIMARY KEY,
        UsuarioId int NOT NULL,
        NombreAnterior nvarchar(50) NOT NULL,
        NombreNuevo nvarchar(50) NOT NULL,
        FechaCambio datetime2 NOT NULL,
        CONSTRAINT FK_CambiosNombreUsuario_Usuarios_UsuarioId
            FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_CambiosNombreUsuario_UsuarioId_FechaCambio
        ON CambiosNombreUsuario(UsuarioId, FechaCambio);
END;
""";

        await context.Database.ExecuteSqlRawAsync(sql, ct);
    }
}
