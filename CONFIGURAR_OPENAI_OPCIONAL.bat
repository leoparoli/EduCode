@echo off
setlocal
cd /d "%~dp0"

echo ========================================
echo CONFIGURACION OPCIONAL DE OPENAI
ECHO ========================================
echo.
echo Esta clave se guarda con user-secrets y no dentro del proyecto.
echo OpenAI solo redacta feedback pedagogico: no modifica la nota.
echo.
set /p OPENAI_KEY=Pegue su API key de OpenAI: 

if "%OPENAI_KEY%"=="" (
    echo No se ingreso ninguna clave.
    pause
    exit /b 1
)

dotnet user-secrets set "InteligenciaArtificial:OpenAIApiKey" "%OPENAI_KEY%" --project ".\src\EduCode.WebApi\EduCode.WebApi.csproj"
if errorlevel 1 goto :error

echo.
echo Clave opcional configurada correctamente.
echo Puede verificarla con:
echo dotnet user-secrets list --project ".\src\EduCode.WebApi\EduCode.WebApi.csproj"
pause
exit /b 0

:error
echo.
echo No fue posible configurar la clave.
pause
exit /b 1
