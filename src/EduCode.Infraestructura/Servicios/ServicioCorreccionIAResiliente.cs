using EduCode.Aplicacion.Interfaces;
using EduCode.Dominio.Excepciones;
using EduCode.DTOs;
using EduCode.Infraestructura.Configuracion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduCode.Infraestructura.Servicios;

public sealed class ServicioCorreccionIAResiliente : IServicioCorreccionIA
{
    private readonly ServicioCorreccionGemini _gemini;
    private readonly ServicioCorreccionOpenAI _openAI;
    private readonly EstadoCircuitoProveedorIA _circuitoGemini;
    private readonly ConfiguracionIA _configuracion;
    private readonly ILogger<ServicioCorreccionIAResiliente> _logger;

    public ServicioCorreccionIAResiliente(
        ServicioCorreccionGemini gemini,
        ServicioCorreccionOpenAI openAI,
        EstadoCircuitoProveedorIA circuitoGemini,
        IOptions<ConfiguracionIA> configuracion,
        ILogger<ServicioCorreccionIAResiliente> logger)
    {
        _gemini = gemini;
        _openAI = openAI;
        _circuitoGemini = circuitoGemini;
        _configuracion = configuracion.Value;
        _logger = logger;
    }

    public async Task<RespuestaCorreccionIADTO> CorregirAsync(
        SolicitudCorreccionIADTO solicitud,
        CancellationToken ct = default)
    {
        ExcepcionProveedorIA? ultimoError = null;

        if (!_circuitoGemini.EstaAbierto(out TimeSpan esperaCircuito))
        {
            try
            {
                RespuestaCorreccionIADTO respuesta =
                    await EjecutarGeminiAsync(solicitud, ct);

                _circuitoGemini.RegistrarExito();
                return respuesta;
            }
            catch (LimiteSolicitudesIAException)
            {
                throw;
            }
            catch (ExcepcionProveedorIA ex)
            {
                ultimoError = ex;

                if (ex.EsTransitoria)
                {
                    _circuitoGemini.RegistrarFalloTransitorio(
                        _configuracion.FallasParaAbrirCircuito,
                        TimeSpan.FromSeconds(
                            Math.Max(
                                10,
                                _configuracion.SegundosCircuitoAbierto)));
                }

                _logger.LogWarning(
                    ex,
                    "Gemini no pudo generar el feedback pedagógico. " +
                    "Se evaluará el proveedor alternativo.");
            }
            catch (ServicioExternoException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Gemini no pudo generar el feedback pedagógico.");
            }
        }
        else
        {
            _logger.LogInformation(
                "Se omite Gemini porque el circuito permanece abierto " +
                "durante {Segundos:0} segundos.",
                esperaCircuito.TotalSeconds);
        }

        if (PuedeUsarOpenAI())
        {
            try
            {
                return await _openAI.CorregirAsync(solicitud, ct);
            }
            catch (ExcepcionProveedorIA ex)
            {
                ultimoError = ex;
                _logger.LogWarning(
                    ex,
                    "El proveedor alternativo OpenAI tampoco pudo " +
                    "generar el feedback pedagógico.");
            }
        }

        throw new ServicioExternoTemporalException(
            "Los proveedores de tutoría con IA no están disponibles. " +
            "La nota técnica, el progreso y el feedback local de EduCode " +
            "permanecen disponibles.",
            ultimoError?.CodigoEstadoHttp,
            ultimoError?.NumeroIntentoProveedor ?? 1);
    }

    private bool PuedeUsarOpenAI() =>
        _configuracion.UsarProveedorAlternativo &&
        string.Equals(
            _configuracion.ProveedorAlternativo,
            "OpenAI",
            StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(_configuracion.OpenAIApiKey);

    private async Task<RespuestaCorreccionIADTO> EjecutarGeminiAsync(
        SolicitudCorreccionIADTO solicitud,
        CancellationToken ct)
    {
        ExcepcionProveedorIA? ultimoError = null;
        int total = Math.Max(0, _configuracion.CantidadReintentos) + 1;

        for (int intento = 1; intento <= total; intento++)
        {
            try
            {
                RespuestaCorreccionIADTO respuesta =
                    await _gemini.CorregirConModeloAsync(
                        solicitud,
                        _configuracion.Modelo,
                        false,
                        intento,
                        ct);

                respuesta.EsProvisoria = false;
                return respuesta;
            }
            catch (ExcepcionProveedorIA ex) when (ex.EsTransitoria)
            {
                ultimoError = ex;

                if (intento >= total || ct.IsCancellationRequested)
                    break;

                await Task.Delay(CalcularEspera(intento), ct);
            }
            catch (ExcepcionProveedorIA)
            {
                throw;
            }
        }

        throw ultimoError ?? new ExcepcionProveedorIA(
            "Gemini no pudo generar el feedback.",
            503,
            true,
            total);
    }

    private TimeSpan CalcularEspera(int numeroFallo)
    {
        int inicial = Math.Max(250, _configuracion.RetrasoInicialMilisegundos);
        int maximo = Math.Max(inicial, _configuracion.RetrasoMaximoMilisegundos);
        double exponencial = inicial * Math.Pow(2, numeroFallo - 1);
        int jitter = Random.Shared.Next(150, 650);

        return TimeSpan.FromMilliseconds(
            Math.Min(maximo, exponencial + jitter));
    }
}
