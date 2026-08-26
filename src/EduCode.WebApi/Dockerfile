# =========================
# Etapa de compilación
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copiar los archivos .csproj
COPY src/EduCode.WebApi/EduCode.WebApi.csproj src/EduCode.WebApi/
COPY src/EduCode.Aplicacion/EduCode.Aplicacion.csproj src/EduCode.Aplicacion/
COPY src/EduCode.AccesoDatos/EduCode.AccesoDatos.csproj src/EduCode.AccesoDatos/
COPY src/EduCode.Infraestructura/EduCode.Infraestructura.csproj src/EduCode.Infraestructura/

# Restaurar dependencias
RUN dotnet restore src/EduCode.WebApi/EduCode.WebApi.csproj

# Copiar el código fuente
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

# Puerto utilizado por Azure Container Apps
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "EduCode.WebApi.dll"]