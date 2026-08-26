using System.Text;
using EduCode.AccesoDatos;
using EduCode.AccesoDatos.Inicializacion;
using EduCode.Aplicacion;
using EduCode.Aplicacion.Interfaces;
using EduCode.Infraestructura;
using EduCode.WebApi.Configuracion;
using EduCode.WebApi.Middleware;
using EduCode.WebApi.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// CONFIGURACIÓN DE BASE DE DATOS
// ======================================================

string connectionString = builder.Configuration
    .GetConnectionString("EduCode")
    ?? throw new InvalidOperationException(
        "Falta la configuración ConnectionStrings:EduCode.");

// ======================================================
// CONTROLADORES Y SWAGGER
// ======================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "EduCode API",
            Version = "v1",
            Description =
                "API educativa para la práctica progresiva " +
                "de Programación 1."
        });

    options.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description =
                "Ingresá únicamente el access token JWT. " +
                "No es necesario escribir la palabra Bearer."
        });

    options.AddSecurityRequirement(
        document => new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(
                    "bearer",
                    document)
            ] = []
        });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IColaFeedbackIA, ColaFeedbackIA>();

builder.Services.AddHostedService<TrabajadorFeedbackIA>();

// ======================================================
// CAPAS DE LA APLICACIÓN
// ======================================================

builder.Services
    .AgregarAplicacion()
    .AgregarAccesoDatos(connectionString)
    .AgregarInfraestructura(builder.Configuration);

// ======================================================
// CONFIGURACIÓN JWT
// ======================================================

builder.Services.Configure<ConfiguracionJwt>(
    builder.Configuration.GetSection(
        ConfiguracionJwt.Seccion));

ConfiguracionJwt jwt = builder.Configuration
    .GetSection(ConfiguracionJwt.Seccion)
    .Get<ConfiguracionJwt>()
    ?? throw new InvalidOperationException(
        "Falta la configuración JWT.");

if (string.IsNullOrWhiteSpace(jwt.ClaveSecreta))
{
    throw new InvalidOperationException(
        "Jwt:ClaveSecreta no puede estar vacía.");
}

if (Encoding.UTF8.GetByteCount(jwt.ClaveSecreta) < 32)
{
    throw new InvalidOperationException(
        "Jwt:ClaveSecreta debe tener al menos 32 bytes.");
}

builder.Services.AddScoped<
    IServicioToken,
    ServicioTokenJwt>();

builder.Services.AddScoped<
    IServicioUsuarioActual,
    ServicioUsuarioActual>();

// ======================================================
// AUTENTICACIÓN Y AUTORIZACIÓN
// ======================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwt.Emisor,
                ValidAudience = jwt.Audiencia,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwt.ClaveSecreta)),

                ClockSkew = TimeSpan.FromMinutes(1)
            };
    });

builder.Services.AddAuthorization();

// ======================================================
// CONSTRUCCIÓN DE LA APLICACIÓN
// ======================================================

var app = builder.Build();

// ======================================================
// INICIALIZACIÓN DE DATOS
// ======================================================

// La inicialización de datos solamente se ejecuta
// durante el desarrollo local.
//
// NO se ejecuta automáticamente en Azure/Producción.

if (app.Environment.IsDevelopment())
{
    using IServiceScope scope =
        app.Services.CreateScope();

    EduCodeContext contexto =
        scope.ServiceProvider
            .GetRequiredService<EduCodeContext>();

    IServicioPassword servicioPassword =
        scope.ServiceProvider
            .GetRequiredService<IServicioPassword>();

    await InicializadorDatos.InicializarAsync(
        contexto,
        servicioPassword);
}

// ======================================================
// SWAGGER
// ======================================================

// Swagger queda disponible tanto en Development
// como en Production/Azure.

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "EduCode API v1");

    options.RoutePrefix = "swagger";

    options.DocumentTitle =
        "EduCode API - Documentación";

    options.DisplayRequestDuration();

    options.EnablePersistAuthorization();
});

// ======================================================
// MIDDLEWARES
// ======================================================

app.UseMiddleware<ManejadorExcepcionesMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

// ======================================================
// ENDPOINTS
// ======================================================

app.MapControllers();

app.Run();