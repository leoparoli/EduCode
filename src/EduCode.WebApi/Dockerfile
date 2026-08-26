# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copiar configuración global de .NET
COPY Directory.Build.props ./

# Copiar archivos de proyecto
COPY src/EduCode.WebApi/EduCode.WebApi.csproj src/EduCode.WebApi/
COPY src/EduCode.Aplicacion/EduCode.Aplicacion.csproj src/EduCode.Aplicacion/
COPY src/EduCode.AccesoDatos/EduCode.AccesoDatos.csproj src/EduCode.AccesoDatos/
COPY src/EduCode.Infraestructura/EduCode.Infraestructura.csproj src/EduCode.Infraestructura/
COPY src/EduCode.Dominio/EduCode.Dominio.csproj src/EduCode.Dominio/
COPY src/EduCode.DTOs/EduCode.DTOs.csproj src/EduCode.DTOs/

# Restaurar dependencias
RUN dotnet restore src/EduCode.WebApi/EduCode.WebApi.csproj

# Copiar todo el código fuente
COPY src/ src/

# Compilar
RUN dotnet build src/EduCode.WebApi/EduCode.WebApi.csproj \
    -c Release \
    --no-restore

# Publicar
RUN dotnet publish src/EduCode.WebApi/EduCode.WebApi.csproj \
    -c Release \
    --no-restore \
    -o /app/publish

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Copiar publicación
COPY --from=build /app/publish .

# Puerto utilizado por Azure Container Apps
EXPOSE 8080

# Iniciar API
ENTRYPOINT ["dotnet", "EduCode.WebApi.dll"]