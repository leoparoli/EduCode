using EduCode.AccesoDatos.Repositorios;
using EduCode.Aplicacion.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduCode.AccesoDatos;

public static class DependencyInjection
{
    public static IServiceCollection AgregarAccesoDatos(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<EduCodeContext>(
            options => options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IRepositorio<>), typeof(Repositorio<>));
        services.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();
        services.AddScoped<IRepositorioCambiosNombreUsuario, RepositorioCambiosNombreUsuario>();
        services.AddScoped<IRepositorioTokens, RepositorioTokens>();
        services.AddScoped<IRepositorioOpiniones, RepositorioOpiniones>();
        services.AddScoped<IRepositorioCursos, RepositorioCursos>();
        services.AddScoped<IRepositorioTemas, RepositorioTemas>();
        services.AddScoped<IRepositorioEjercicios, RepositorioEjercicios>();
        services.AddScoped<IRepositorioIntentos, RepositorioIntentos>();
        services.AddScoped<IRepositorioProgresos, RepositorioProgresos>();
        services.AddScoped<IRepositorioPistas, RepositorioPistas>();
        services.AddScoped<IRepositorioSolicitudesIA, RepositorioSolicitudesIA>();
        services.AddScoped<IRepositorioGamificacion, RepositorioGamificacion>();
        services.AddScoped<IRepositorioPortalAcademico, RepositorioPortalAcademico>();
        services.AddScoped<IUnidadTrabajo, UnidadTrabajo>();

        return services;
    }
}
