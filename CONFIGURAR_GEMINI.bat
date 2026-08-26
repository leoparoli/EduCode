@echo off
setlocal
cd /d "%~dp0"

echo ======================================================
echo CONFIGURACION LOCAL DE GEMINI PARA EDUCODE
echo ======================================================
echo.
echo La clave se guardara mediante .NET user-secrets y no se
echo escribira dentro del proyecto ni en appsettings.json.
echo.
set /p GEMINI_KEY=Pegue su API key de Gemini y presione ENTER: 

if "%GEMINI_KEY%"=="" (
    echo.
    echo No se ingreso ninguna clave.
    exit /b 1
)

dotnet user-secrets set "InteligenciaArtificial:ApiKey" "%GEMINI_KEY%" --project ".\src\EduCode.WebApi\EduCode.WebApi.csproj"

if errorlevel 1 (
    echo.
    echo No fue posible guardar la clave.
    exit /b 1
)

set GEMINI_KEY=
echo.
echo Clave guardada correctamente.
echo Puede iniciar la API mediante INICIAR_API.bat.
endlocal
