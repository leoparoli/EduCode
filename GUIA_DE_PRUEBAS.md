# Guía de prueba rápida

## 1. Ejecutar

```cmd
INICIAR_API.bat
```

Swagger: `https://localhost:7168/swagger`

## 2. Verificar estado

`GET /api/estado`

## 3. Autenticarse

`POST /api/auth/login`

```json
{
  "emailONombreUsuario": "alumno@educode.local",
  "password": "Alumno123!"
}
```

Copiar el `accessToken` en **Authorize**.

## 4. Probar el recorrido

1. `GET /api/cursos`
2. `GET /api/temas/curso/1`
3. `GET /api/ejercicios/tema/{temaId}`
4. `GET /api/ejercicios/{id}`
5. `POST /api/ejercicios/{id}/intentos`
6. `GET /api/progreso/mi-progreso`
7. `GET /api/ejercicios/{id}/intentos`

## 5. Probar administración

Ingresar con `admin@educode.local / Admin123!` y utilizar:

- `POST /api/temas`
- `POST /api/ejercicios`
