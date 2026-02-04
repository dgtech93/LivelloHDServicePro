@echo off
REM Script Batch per creare l'installer di Livello HD Service PRO
REM Eseguire come amministratore

title Livello HD Service PRO - Build Setup
color 0B

echo ========================================
echo   Livello HD Service PRO - Build Setup
echo ========================================
echo.

REM Configurazione
set ProjectPath=..\LivelloHDServicePRO\LivelloHDServicePRO.csproj
set Configuration=Release
set PublishPath=..\LivelloHDServicePRO\bin\Release\net10.0-windows\publish
set InnoSetupCompiler=C:\Program Files (x86)\Inno Setup 6\ISCC.exe
set SetupScriptPath=.\LivelloHDServicePRO_Setup.iss

REM Verifica Inno Setup
if not exist "%InnoSetupCompiler%" (
    echo ERRORE: Inno Setup non trovato!
    echo Scarica e installa da: https://jrsoftware.org/isdl.php
    echo.
    pause
    exit /b 1
)

echo [1/5] Pulizia build precedenti...
if exist "%PublishPath%" (
    rmdir /s /q "%PublishPath%"
)

echo [2/5] Restore pacchetti NuGet...
dotnet restore "%ProjectPath%"
if errorlevel 1 (
    echo ERRORE durante il restore!
    pause
    exit /b 1
)

echo [3/5] Build del progetto...
dotnet build "%ProjectPath%" -c %Configuration% --no-restore
if errorlevel 1 (
    echo ERRORE durante la build!
    pause
    exit /b 1
)

echo [4/5] Pubblicazione applicazione...
dotnet publish "%ProjectPath%" -c %Configuration% -f net10.0-windows -r win-x64 --self-contained false -p:PublishSingleFile=false -p:PublishReadyToRun=true
if errorlevel 1 (
    echo ERRORE durante la pubblicazione!
    pause
    exit /b 1
)

echo [5/5] Creazione installer...
"%InnoSetupCompiler%" "%SetupScriptPath%"
if errorlevel 1 (
    echo ERRORE durante la creazione installer!
    pause
    exit /b 1
)

echo.
echo ========================================
echo   BUILD COMPLETATO CON SUCCESSO!
echo ========================================
echo.
echo L'installer si trova in: ..\Releases
echo.

REM Apri la cartella releases
explorer.exe ..\Releases

pause
