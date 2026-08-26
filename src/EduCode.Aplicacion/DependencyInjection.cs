using EduCode.Aplicacion.Servicios;
using EduCode.Aplicacion.CasosUso;
using Microsoft.Extensions.DependencyInjection;

namespace EduCode.Aplicacion;

public static class DependencyInjection
{
    public static IServiceCollection AgregarAplicacion(
        this IServiceCollection services)
    {
        services.AddScoped<CURegistrarUsuario>();
        services.AddScoped<CUListarCursos>();
        services.AddScoped<CUIniciarSesion>();
        services.AddScoped<CUActualizarToken>();
        services.AddScoped<CUPerfilUsuario>();
        services.AddScoped<CUOpinionesUsuario>();
        services.AddScoped<CURegistrarDocenteInvitado>();
        services.AddScoped<CUInvitacionesDocentes>();
        services.AddScoped<CUPortalDocente>();
        services.AddScoped<CUPortalAdministracion>();
        services.AddScoped<CUAltaTema>();
        services.AddScoped<CUAltaEjercicio>();
        services.AddScoped<CUListarTemasAlumno>();
        services.AddScoped<CUListarEjerciciosPorTema>();
        services.AddScoped<CUObtenerEjercicio>();
        services.AddScoped<CUEnviarIntento>();
        services.AddScoped<CUReintentarCorreccionIntento>();
        services.AddScoped<CUObtenerResultadoIntento>();
        services.AddScoped<CUObtenerHistorialIntentos>();
        services.AddScoped<CUSolicitarPista>();
        services.AddScoped<CUObtenerProgresoAlumno>();
        services.AddScoped<CUGamificacion>();
        services.AddScoped<MotorPuntajeEvaluacion>();
        services.AddScoped<ProcesadorCorreccionIntento>();
        services.AddScoped<ProcesadorFeedbackPedagogico>();

        return services;
    }
}
