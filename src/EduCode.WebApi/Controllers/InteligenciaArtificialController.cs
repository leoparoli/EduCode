using EduCode.Aplicacion.Servicios;
using EduCode.Infraestructura.Configuracion;
using EduCode.Infraestructura.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EduCode.WebApi.Controllers;

[ApiController]
[Route("api/inteligencia-artificial")]
[Authorize(Roles = "Administrador")]
public sealed class InteligenciaArtificialController : ControllerBase
{
    [HttpGet("estado")]
    public IActionResult ObtenerEstado(
        [FromServices] IOptions<ConfiguracionIA> opciones,
        [FromServices] EstadoCircuitoProveedorIA circuitoGemini,
        [FromServices] EstadoProveedoresTutorIA estadoProveedoresTutor)
    {
        ConfiguracionIA configuracion = opciones.Value;
        EstadoCircuitoSnapshot circuito = circuitoGemini.ObtenerEstado();
        EstadoProveedoresTutorSnapshot proveedoresTutor =
            estadoProveedoresTutor.ObtenerEstado();

        return Ok(new
        {
            evaluacionAcademica = new
            {
                proveedor = "EduCode Local",
                motorPuntaje = MotorPuntajeEvaluacion.Version,
                catalogoAcademico = "programacion-1-2.5",
                solucionMinimaValida = true,
                analizadorSintactico = "Esprima .NET",
                ejecutorCasos = "Jint",
                bloqueaAlUsuarioSiFallaLaIA = false,
                afectaRanking = true
            },
            tutoriaPedagogica = new
            {
                proveedorPrincipal = configuracion.UsarSimulador
                    ? "Simulador local"
                    : "Google Gemini",
                configuracion.Modelo,
                configuracion.UsarSimulador,
                apiKeyGeminiConfigurada =
                    !string.IsNullOrWhiteSpace(configuracion.ApiKey),
                configuracion.CantidadReintentos,
                circuitoGemini = new
                {
                    circuito.Abierto,
                    circuito.FallasConsecutivas,
                    circuito.AbiertoHastaUtc
                },
                configuracion.UsarProveedorAlternativo,
                configuracion.ProveedorAlternativo,
                configuracion.OpenAIModelo,
                apiKeyAlternativaConfigurada =
                    !string.IsNullOrWhiteSpace(
                        configuracion.OpenAIApiKey),
                seEjecutaEnSegundoPlano = true,
                afectaNota = false,
                afectaRanking = false,
                configuracion.VersionPrompt
            },
            tutorConversacional = new
            {
                configuracion.TutorHabilitado,
                filtroTematicoPrevio = true,
                consultasFueraDeAlcanceConsumenTokens = false,
                limiteIndependienteDelEvaluador = true,
                configuracion.MaximoConsultasTutorDiariasPorUsuario,
                configuracion.MaximoCaracteresConsultaTutor,
                configuracion.MaximoTokensSalidaTutor,
                configuracion.TutorUsarFallbackLocal,
                configuracion.VersionPromptTutor,
                versionValidadorTematico = "filtro-tematico-2.8.0",
                validacionContextual = true,
                toleranciaErroresOrtograficos = true,
                arrayComoTemaIndependiente = true,
                listaComoTemaIndependiente = true,
                validacionSalidaProveedor = configuracion.TutorValidarSalidaProveedor,
                maximoCaracteresRespuesta = configuracion.TutorMaximoCaracteresRespuesta,
                historialClienteTratadoComoNoConfiable = true,
                bloqueoManipulacionPrompt = true,
                bloqueoEntradaEjecutable = true,
                gemini = new
                {
                    modeloPrincipal = configuracion.Modelo,
                    modeloRespaldo = configuracion.TutorModeloGeminiRespaldo,
                    nivelPensamiento = configuracion.TutorNivelPensamientoGemini
                },
                openAI = new
                {
                    modeloPrincipal = configuracion.OpenAIModelo,
                    modeloRespaldo = configuracion.TutorModeloOpenAIRespaldo,
                    razonamiento = configuracion.TutorRazonamientoOpenAI
                },
                trazabilidadEnEjecucion = proveedoresTutor,
                historialMaximoEnviado = 6,
                afectaNota = false,
                afectaRanking = false
            }
        });
    }
}
