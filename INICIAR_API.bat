@echo off
setlocal
chcp 65001 >nul
cd /d "%~dp0"
title EduCode API 3.1.2

echo ================================================
echo              EDUCODE API 3.1.2
echo ================================================
echo Swagger: http://localhost:5168/swagger
echo Estado:  http://localhost:5168/api/estado
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
    echo ERROR: No se encontro el SDK de .NET en el PATH.
    pause
    exit /b 1
)

dotnet restore EduCode.sln
if errorlevel 1 goto ERROR_API

dotnet run --project ".\src\EduCode.WebApi\EduCode.WebApi.csproj" --launch-profile http
if errorlevel 1 goto ERROR_API

exit /b 0

:ERROR_API
echo.
echo ERROR: EduCode API no pudo iniciarse.
echo Revisa el mensaje anterior para identificar la causa concreta.
pause
exit /b 1
