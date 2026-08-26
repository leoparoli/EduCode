# =========================
# Etapa de compilación
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copiar todos los archivos .csproj
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

# Publicar
RUN dotnet publish src/EduCode.WebApi/EduCode.WebApi.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# =========================
# Etapa de ejecución
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Azure Container Apps escucha en el puerto 8080
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "EduCode.WebApi.dll"]