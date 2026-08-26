# Evaluador local e IA asíncrona

## Responsabilidades

### EduCode local

Es responsable de la evidencia académica definitiva:

- parseo sintáctico;
- ejecución de casos de prueba;
- detección de patrones técnicos;
- porcentaje de cada criterio;
- puntaje exacto;
- aprobación;
- progreso.

### Proveedores de IA

Son tutores de redacción. Reciben la evidencia ya calculada y generan:

- un resumen comprensible;
- aspectos correctos;
- próximos ajustes;
- una pista pedagógica.

Nunca deciden la nota.

## Configuración de proveedores

La sección `InteligenciaArtificial` de `appsettings.json` incluye:

```json
{
  "Modelo": "gemini-3.5-flash",
  "UsarProveedorAlternativo": true,
  "ProveedorAlternativo": "OpenAI",
  "OpenAIModelo": "gpt-5-mini",
  "FallasParaAbrirCircuito": 3,
  "SegundosCircuitoAbierto": 90
}
```

Las claves deben permanecer fuera del proyecto:

```text
InteligenciaArtificial:ApiKey
InteligenciaArtificial:OpenAIApiKey
```

## Prueba recomendada

En `Contar vocales`, probá:

```javascript
function contarVocales(texto) {
    let cantidad = 0;
    texto = texto.toLowerCase();

    for (let i = 0; i <= texto.length; i++) {
        if ("aeiou".includes(texto[i])) {
            cantidad++;
        }
    }

    return cantidad;
}
```

La API debe devolver inmediatamente:

- nota técnica;
- casos superados;
- hallazgo `JS_OFF_BY_ONE_LENGTH`;
- criterio de índices afectado;
- progreso actualizado.

La explicación de IA puede aparecer unos segundos después, pero su disponibilidad no altera el resultado.

## Limitaciones actuales

- El evaluador local está preparado para JavaScript.
- La ejecución de código no confiable está restringida dentro de Jint; para una publicación con alta concurrencia se recomienda aislarla adicionalmente en un proceso o contenedor dedicado.
- Las reglas de calidad son heurísticas y deben seguir calibrándose con soluciones de referencia.
