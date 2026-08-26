# EduCode API — Versión 3.0.4

Backend de EduCode en .NET 10 con arquitectura en capas, SQL Server, JWT, Entity Framework Core, evaluación local de JavaScript y servicios de IA pedagógica.

## Proyectos

- `EduCode.Dominio`: entidades y reglas de negocio.
- `EduCode.DTOs`: contratos externos, incluidos los DTO del Tutor IA.
- `EduCode.Aplicacion`: casos de uso, interfaces y `score-3.1-local`.
- `EduCode.AccesoDatos`: EF Core, repositorios y catálogo académico.
- `EduCode.Infraestructura`: Esprima, Jint, validación temática y proveedores de IA.
- `EduCode.WebApi`: controladores, JWT, Swagger y middleware.

## Portal académico 3.0

La API separa los permisos de Alumno, Docente y Administrador. El docente dispone de métricas observacionales sobre sus grupos y de un flujo para proponer ejercicios, pero no puede enviar intentos, corregir soluciones, asignar calificaciones ni modificar puntajes o rankings.

Endpoints principales:

```http
POST /api/auth/registro-docente
GET  /api/docente/panel
GET  /api/docente/propuestas
POST /api/docente/propuestas
PUT  /api/docente/propuestas/{id}
GET  /api/administracion/panel
GET  /api/administracion/estructura-academica
POST /api/administracion/grupos
POST /api/administracion/grupos/{id}/inscripciones
GET  /api/administracion/invitaciones-docentes
POST /api/administracion/invitaciones-docentes
PUT  /api/administracion/invitaciones-docentes/{id}/revocar
GET  /api/administracion/propuestas-ejercicios
PUT  /api/administracion/propuestas-ejercicios/{id}
GET  /api/estado
```

Las tablas `InvitacionesDocentes` y `PropuestasEjercicios` se agregan mediante un actualizador idempotente para conservar `EduCodeDbV2`.

## Evaluador de ejercicios

La nota se calcula sin depender de Gemini ni OpenAI:

1. Esprima valida la sintaxis.
2. Jint ejecuta casos públicos y ocultos.
3. Se comprueban restricciones académicas.
4. El motor aplica la rúbrica ponderada.

La IA de resultados redacta orientación pedagógica, pero no altera la nota.


## Perfil e identidad 2.9.6

Endpoints autenticados del usuario:

```http
GET  /api/perfil
PUT  /api/perfil
GET  /api/perfil/opiniones
POST /api/perfil/opiniones
```

Endpoints exclusivos para `Administrador`:

```http
GET /api/administracion/opiniones
PUT /api/administracion/opiniones/{id}/estado
```

Reglas principales:

- El nombre y el apellido se conservan como datos de registro y no forman parte del contrato de edición del perfil.
- El nombre de usuario es único, tiene entre 3 y 50 caracteres y solo admite letras, números, punto, guion y guion bajo.
- Alumnos y docentes pueden realizar como máximo dos cambios efectivos de nombre de usuario dentro de una ventana móvil de 15 días; guardar el mismo nombre, cambiar el avatar o actualizar la foto no consume el límite. La identidad de una cuenta administradora queda fija para preservar la trazabilidad.
- El avatar debe pertenecer al catálogo interno.
- La ruta de una foto solo puede apuntar al directorio de perfiles y al identificador del usuario autenticado.
- Las opiniones pueden ser enviadas por alumnos y docentes; las cuentas administradoras únicamente las supervisan y clasifican. Cada opinión requiere categoría válida, entre 1 y 5 estrellas y un comentario de 20 a 1200 caracteres.
- Se aceptan hasta cinco opiniones diarias y se bloquean duplicados exactos durante diez minutos.
- El estado administrativo puede ser pendiente, revisada, considerada o archivada.

## Gamificación 2.9.0

- `GET api/gamificacion/resumen`: posición, puntos, promedio, rachas, insignias y Top 20.
- `PUT api/gamificacion/visibilidad`: participación voluntaria en el ranking.
- Los puntos suman una sola vez la mejor nota de cada ejercicio.
- Las insignias se evalúan después de cada corrección definitiva.
- El cálculo de rachas utiliza el historial real de intentos y no días de acceso.


## Tutor IA 2.9.2

Endpoints autenticados:

```http
GET  /api/tutor-ia/estado
POST /api/tutor-ia/consultar
```

Temas habilitados:

- Variables y operadores.
- Condicionales.
- Bucles.
- Strings.
- Array.
- Lista.
- Funciones.
- Clases y objetos.

El flujo aplica defensa por capas:

1. Limpieza de caracteres de control e invisibles.
2. Validación exacta de vacío y longitud.
3. Revalidación del historial enviado por el navegador.
4. Bloqueo de manipulación, contenido peligroso y entradas ejecutables.
5. Clasificación contextual y detección de temas.
6. Aclaración local cuando falta información.
7. Control de cupo y serialización por usuario.
8. Gemini y OpenAI con reintentos/modelos alternativos.
9. Validación de la respuesta externa antes de entregarla.
10. Respaldo local específico cuando los proveedores no responden.
11. Filtrado temático del historial para evitar arrastrar temas antiguos sin relación.
12. Detección local de pedidos de solución completa, reemplazados por pistas progresivas.
13. Redirección a un ejercicio real del recorrido cuando el alumno pide práctica.

Las consultas rechazadas, las aclaraciones, las redirecciones al recorrido y los respaldos locales no consumen el cupo diario. Solo se contabilizan respuestas externas registradas como exitosas.

## Contenido precargado

- 8 unidades académicas.
- 56 ejercicios.
- 8 ejercicios en las seis unidades originales y 4 ejercicios iniciales en cada nueva unidad: Array y Lista.
- Cinco criterios por ejercicio.
- Tres pistas progresivas.
- Casos públicos y ocultos.

Array y Lista se publican como unidades independientes en el catálogo, la carga inicial, las propuestas docentes, el estado del Tutor y la validación temática. Al iniciar una base anterior, la etiqueta combinada se normaliza sin eliminar usuarios, intentos ni progreso.

## Base de datos

```text
Server=(localdb)\MSSQLLocalDB
Database=EduCodeDbV2
```

Las versiones 2.9.5 y 2.9.6 agregaron perfil, opiniones e historial de cambios de usuario. La versión 3.0 suma `InvitacionesDocentes` y `PropuestasEjercicios`. El inicializador de desarrollo ejecuta actualizaciones idempotentes después de `EnsureCreated`, de modo que una base local existente se conserva sin necesidad de borrarla.

## Configuración de IA

Las claves se guardan mediante User Secrets:

```cmd
dotnet user-secrets set "InteligenciaArtificial:ApiKey" "TU_API_KEY" --project ".\src\EduCode.WebApi\EduCode.WebApi.csproj"
```

También pueden utilizarse `CONFIGURAR_GEMINI.bat` y `CONFIGURAR_OPENAI_OPCIONAL.bat`.

## Ejecución

```cmd
dotnet restore
dotnet run --project ".\src\EduCode.WebApi\EduCode.WebApi.csproj"
```

- Swagger local recomendado: `http://localhost:5168/swagger`
- Estado administrativo: `GET /api/inteligencia-artificial/estado`


## Nota de versión 2.9.3

La cuota del Tutor ya estaba aislada por usuario mediante la identidad JWT. La versión 2.9.3 corrige en Web MVC el almacenamiento visual del historial para que utilice el `UsuarioId` de la sesión y no una clave global del navegador. La API y su esquema de base de datos no requieren migraciones adicionales.


## Nota de versión 2.9.4

El middleware conserva los errores técnicos completos en logs, pero devuelve mensajes públicos seguros y comprensibles. El endpoint del Tutor ya no expone nombres de proveedores ni modelos: responde con la identidad genérica `Tutor EduCode` y mantiene los detalles internos únicamente para diagnóstico y auditoría.
