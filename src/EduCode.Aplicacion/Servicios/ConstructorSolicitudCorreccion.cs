using EduCode.Dominio.Entidades;
using EduCode.Dominio.Enums;
using EduCode.DTOs;

namespace EduCode.Aplicacion.Servicios;

public static class ConstructorSolicitudCorreccion
{
    public static SolicitudCorreccionIADTO Crear(
        IntentoEjercicio intento,
        Ejercicio ejercicio) =>
        new()
        {
            UsuarioId = intento.UsuarioId,
            IntentoId = intento.Id,
            TituloEjercicio = ejercicio.Titulo,
            Consigna = ejercicio.Consigna,
            CodigoAlumno = intento.CodigoEnviado,
            Lenguaje = ejercicio.Lenguaje,
            NivelDificultad = ejercicio.Dificultad.ToString(),
            PuntajeMinimoAprobacion = ejercicio.PuntajeMinimoAprobacion,
            VersionRubrica = ejercicio.VersionRubrica,
            NumeroIntento = intento.NumeroIntento,
            Criterios = ejercicio.CriteriosEvaluacion
                .OrderBy(criterio => criterio.Orden)
                .Select(criterio => new CriterioCorreccionIADTO
                {
                    Id = criterio.Id,
                    Descripcion = criterio.Descripcion,
                    Tipo = criterio.Tipo.ToString(),
                    Peso = criterio.Peso,
                    Obligatorio = criterio.Obligatorio,
                    PorcentajeMinimoObligatorio =
                        criterio.PorcentajeMinimoObligatorio
                })
                .ToList(),
            ConceptosPermitidos = ejercicio.ConceptosPermitidos
                .Select(concepto => concepto.Nombre)
                .ToList(),
            ConceptosNoPermitidos = ejercicio.ConceptosNoPermitidos
                .Select(concepto => concepto.Nombre)
                .ToList(),
            CasosPrueba = ejercicio.CasosPrueba
                .Where(caso => caso.Activo)
                .OrderBy(caso => caso.Orden)
                .Select(caso => new CasoPruebaCorreccionIADTO
                {
                    Nombre = caso.Nombre,
                    Entrada = caso.Entrada,
                    SalidaEsperada = caso.SalidaEsperada,
                    EsOculto = caso.Visibilidad ==
                        VisibilidadCasoPrueba.Oculto
                })
                .ToList()
        };
}
