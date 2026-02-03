# Script PowerShell per pubblicare e creare l'installer di Livello HD Service PRO
# Eseguire come amministratore

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Livello HD Service PRO - Build Setup  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configurazione
$ProjectPath = "..\LivelloHDServicePRO\LivelloHDServicePRO.csproj"
$Configuration = "Release"
$Framework = "net10.0-windows"
$PublishPath = "..\LivelloHDServicePRO\bin\Release\net10.0-windows\publish"
$InnoSetupCompiler = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$SetupScriptPath = ".\LivelloHDServicePRO_Setup.iss"

# Verifica se Inno Setup è installato
if (-not (Test-Path $InnoSetupCompiler)) {
    Write-Host "ERRORE: Inno Setup non trovato!" -ForegroundColor Red
    Write-Host "Scarica e installa Inno Setup da: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    Write-Host "Percorso atteso: $InnoSetupCompiler" -ForegroundColor Yellow
    Read-Host "Premi ENTER per uscire"
    exit 1
}

# Step 1: Pulizia build precedenti
Write-Host "[1/5] Pulizia build precedenti..." -ForegroundColor Green
if (Test-Path $PublishPath) {
    Remove-Item -Path $PublishPath -Recurse -Force
    Write-Host "  Cartella publish pulita" -ForegroundColor Gray
}

# Step 2: Restore dei pacchetti NuGet
Write-Host "[2/5] Restore pacchetti NuGet..." -ForegroundColor Green
dotnet restore $ProjectPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRORE durante il restore dei pacchetti!" -ForegroundColor Red
    Read-Host "Premi ENTER per uscire"
    exit 1
}

# Step 3: Build del progetto
Write-Host "[3/5] Build del progetto..." -ForegroundColor Green
dotnet build $ProjectPath -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRORE durante la build!" -ForegroundColor Red
    Read-Host "Premi ENTER per uscire"
    exit 1
}

# Step 4: Publish dell'applicazione
Write-Host "[4/5] Pubblicazione dell'applicazione..." -ForegroundColor Green
dotnet publish $ProjectPath `
    -c $Configuration `
    -f $Framework `
    -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRORE durante la pubblicazione!" -ForegroundColor Red
    Read-Host "Premi ENTER per uscire"
    exit 1
}

Write-Host "  Pubblicazione completata in: $PublishPath" -ForegroundColor Gray

# Step 5: Creazione dell'installer con Inno Setup
Write-Host "[5/5] Creazione installer con Inno Setup..." -ForegroundColor Green

# Verifica che il file .iss esista
if (-not (Test-Path $SetupScriptPath)) {
    Write-Host "ERRORE: File setup script non trovato: $SetupScriptPath" -ForegroundColor Red
    Read-Host "Premi ENTER per uscire"
    exit 1
}

# Compila l'installer
& $InnoSetupCompiler $SetupScriptPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRORE durante la creazione dell'installer!" -ForegroundColor Red
    Read-Host "Premi ENTER per uscire"
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  BUILD COMPLETATO CON SUCCESSO!        " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "L'installer è stato creato nella cartella: ..\Releases" -ForegroundColor Cyan
Write-Host ""

# Apri la cartella dei releases
$ReleasesPath = "..\Releases"
if (Test-Path $ReleasesPath) {
    Write-Host "Aprendo la cartella dei releases..." -ForegroundColor Gray
    Start-Process explorer.exe $ReleasesPath
}

Write-Host ""
Read-Host "Premi ENTER per uscire"
