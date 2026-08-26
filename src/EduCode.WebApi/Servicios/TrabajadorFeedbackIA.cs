using EduCode.Aplicacion.Interfaces;
using EduCode.Aplicacion.Servicios;

namespace EduCode.WebApi.Servicios;

public sealed class TrabajadorFeedbackIA : BackgroundService
{
    private readonly IColaFeedbackIA _cola;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TrabajadorFeedbackIA> _logger;

    public TrabajadorFeedbackIA(
        IColaFeedbackIA cola,
        IServiceScopeFactory scopeFactory,
        ILogger<TrabajadorFeedbackIA> logger)
    {
        _cola = cola;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int intentoId;

            try
            {
                intentoId = await _cola.DesencolarAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await using AsyncServiceScope scope =
                    _scopeFactory.CreateAsyncScope();
                ProcesadorFeedbackPedagogico procesador = scope
                    .ServiceProvider
                    .GetRequiredService<ProcesadorFeedbackPedagogico>();

                await procesador.ProcesarAsync(intentoId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "No se pudo enriquecer con IA el intento {IntentoId}. " +
                    "El resultado técnico local permanece disponible.",
                    intentoId);
            }
        }
    }
}
