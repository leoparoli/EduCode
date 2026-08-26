# Integración de Google Gemini en EduCode

La API utiliza Google Gemini como tutor opcional de Programación 1. La evaluación y la nota se calculan localmente antes de invocar al proveedor. Gemini solo redacta una explicación pedagógica y su clave no se incluye en el código ni en el ZIP.

## 1. Guardar la API key

Ejecutá `CONFIGURAR_GEMINI.bat` o, desde la raíz de `EduCode.API`:

```cmd
dotnet user-secrets set "InteligenciaArtificial:ApiKey" "TU_API_KEY" --project ".\src\EduCode.WebApi\EduCode.WebApi.csproj"
```

El proyecto ya posee `UserSecretsId`, por lo que no hace falta ejecutar
`dotnet user-secrets init`.

Para comprobar la configuración:

```cmd
dotnet user-secrets list --project ".\src\EduCode.WebApi\EduCode.WebApi.csproj"
```

## 2. Ejecutar

```cmd
dotnet restore
dotnet run --project ".\src\EduCode.WebApi\EduCode.WebApi.csproj"
```

Swagger:

```text
https://localhost:7168/swagger
```

## 3. Confirmar el proveedor

Iniciá sesión como administrador, autorizá Swagger con el JWT y ejecutá:

```http
GET /api/inteligencia-artificial/estado
```

Debe informar:

- `proveedor`: Google Gemini
- `usarSimulador`: false
- `apiKeyConfigurada`: true
- `modelo`: gemini-3.5-flash

## 4. Prueba del error de límite

Solución con error:

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

La corrección debe detectar que `i <= texto.length` permite que el índice
alcance una posición inexistente. Aunque Gemini no lo detectara, el analizador
local incorporado aplica una regla de seguridad y evita que el intento sea
marcado como correcto.

## 5. Configuración

En `src/EduCode.WebApi/appsettings.json`:

- `UsarSimulador`: activa o desactiva Gemini.
- `UsarFallbackSimulado`: si está activo, usa una corrección provisoria cuando
  Gemini falla. Por defecto está desactivado para no ocultar problemas de cuota.
- `Modelo`: modelo de Gemini.
- `MaximoTokensSalida`: limita la extensión del feedback.
- `Temperatura`: se recomienda mantenerla baja para obtener evaluaciones
  consistentes.
- `TimeoutSegundos`: tiempo máximo de espera.
- `MaximoSolicitudesDiariasPorUsuario`: protección de cuota.

## 6. Seguridad

Nunca coloques la clave en:

- `appsettings.json`
- el proyecto MVC
- JavaScript
- GitHub
- documentación pública

Para producción, utilizá la variable de entorno:

```text
InteligenciaArtificial__ApiKey
```

## 7. Qué analiza la corrección

Gemini recibe:

- consigna;
- lenguaje y dificultad;
- rúbrica con pesos y obligatoriedad;
- conceptos permitidos y no permitidos;
- casos de prueba registrados;
- código del alumno;
- hallazgos del analizador local.

La respuesta se exige mediante un esquema JSON y luego se valida nuevamente
en el servidor. Gemini devuelve porcentajes por criterio. El motor score-2.0 calcula el
puntaje exacto y determina la aprobación sin aceptar una nota general de la IA.
