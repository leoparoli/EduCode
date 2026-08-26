using EduCode.Aplicacion.Interfaces;
using EduCode.Infraestructura.Configuracion;
using EduCode.Infraestructura.Servicios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EduCode.Infraestructura;

public static class DependencyInjection
{
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ConfiguracionIA>(
            configuration.GetSection(ConfiguracionIA.Seccion));

        services.AddSingleton<IServicioPassword, ServicioPassword>();
        services.AddScoped<IServicioEvaluacionLocal,
            ServicioEvaluacionLocalJavaScript>();
        services.AddScoped<ServicioCorreccionIAMock>();
        services.AddSingleton<EstadoCircuitoProveedorIA>();
        services.AddSingleton<EstadoProveedoresTutorIA>();

        // Tutor IA conversacional: independiente del evaluador de ejercicios.
        services.AddScoped<ValidadorTematicaTutorP1>();
        services.AddScoped<IServicioTutorIA, ServicioTutorIA>();

        services.AddHttpClient("TutorGemini", (provider, client) =>
        {
            ConfiguracionIA configuracion = provider
                .GetRequiredService<IOptions<ConfiguracionIA>>()
                .Value;

            client.BaseAddress = new Uri(configuracion.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(
                Math.Max(10, configuracion.TimeoutSegundos));
        });

        services.AddHttpClient("TutorOpenAI", (provider, client) =>
        {
            ConfiguracionIA configuracion = provider
                .GetRequiredService<IOptions<ConfiguracionIA>>()
                .Value;

            client.BaseAddress = new Uri(configuracion.OpenAIBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(
                Math.Max(10, configuracion.TimeoutSegundos));
        });

        bool usarSimulador = configuration.GetValue<bool>(
            $"{ConfiguracionIA.Seccion}:UsarSimulador",
            true);

        if (usarSimulador)
        {
            services.AddScoped<IServicioCorreccionIA>(provider =>
                provider.GetRequiredService<ServicioCorreccionIAMock>());
            return services;
        }

        services.AddHttpClient<ServicioCorreccionGemini>(
            (provider, client) =>
            {
                ConfiguracionIA configuracion = provider
                    .GetRequiredService<IOptions<ConfiguracionIA>>()
                    .Value;

                client.BaseAddress = new Uri(configuracion.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(
                    Math.Max(10, configuracion.TimeoutSegundos));
            });

        services.AddHttpClient<ServicioCorreccionOpenAI>(
            (provider, client) =>
            {
                ConfiguracionIA configuracion = provider
                    .GetRequiredService<IOptions<ConfiguracionIA>>()
                    .Value;

                client.BaseAddress = new Uri(configuracion.OpenAIBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(
                    Math.Max(10, configuracion.TimeoutSegundos));
            });

        services.AddScoped<ServicioCorreccionIAResiliente>();
        services.AddScoped<IServicioCorreccionIA>(provider =>
            provider.GetRequiredService<ServicioCorreccionIAResiliente>());

        return services;
    }
}
