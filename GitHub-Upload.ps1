# Script PowerShell per inizializzare Git e caricare su GitHub
# Esegui questo script dalla cartella root del progetto

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Git Setup e Upload su GitHub      " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Verifica se git è installato
try {
    $gitVersion = git --version
    Write-Host "? Git trovato: $gitVersion" -ForegroundColor Green
} catch {
    Write-Host "? Git non trovato!" -ForegroundColor Red
    Write-Host "Installa Git da: https://git-scm.com/download/win" -ForegroundColor Yellow
    Read-Host "Premi ENTER per uscire"
    exit 1
}

Write-Host ""
Write-Host "PASSO 1: Inizializzazione Repository Git" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Yellow

# Inizializza repository se non esiste già
if (-not (Test-Path ".git")) {
    Write-Host "Inizializzo repository Git..." -ForegroundColor Gray
    git init
    Write-Host "? Repository inizializzato" -ForegroundColor Green
} else {
    Write-Host "? Repository Git già esistente" -ForegroundColor Green
}

Write-Host ""
Write-Host "PASSO 2: Configurazione Git" -ForegroundColor Yellow
Write-Host "============================" -ForegroundColor Yellow

# Chiedi nome utente se non configurato
$userName = git config user.name
if ([string]::IsNullOrEmpty($userName)) {
    Write-Host ""
    $userName = Read-Host "Inserisci il tuo nome (es. Mario Rossi)"
    git config user.name "$userName"
}
Write-Host "? Nome: $userName" -ForegroundColor Green

# Chiedi email se non configurata
$userEmail = git config user.email
if ([string]::IsNullOrEmpty($userEmail)) {
    Write-Host ""
    $userEmail = Read-Host "Inserisci la tua email GitHub"
    git config user.email "$userEmail"
}
Write-Host "? Email: $userEmail" -ForegroundColor Green

Write-Host ""
Write-Host "PASSO 3: Aggiungi File al Repository" -ForegroundColor Yellow
Write-Host "======================================" -ForegroundColor Yellow

Write-Host "Aggiungendo file..." -ForegroundColor Gray
git add .
Write-Host "? File aggiunti allo staging" -ForegroundColor Green

Write-Host ""
Write-Host "PASSO 4: Primo Commit" -ForegroundColor Yellow
Write-Host "=====================" -ForegroundColor Yellow

git commit -m "Initial commit - Livello HD Service PRO v1.0"
Write-Host "? Commit creato" -ForegroundColor Green

Write-Host ""
Write-Host "PASSO 5: Configurazione Branch Principale" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Yellow

git branch -M main
Write-Host "? Branch rinominato in 'main'" -ForegroundColor Green

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  PROSSIMI PASSI SU GITHUB          " -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Vai su GitHub.com e accedi" -ForegroundColor White
Write-Host "2. Clicca sul '+' in alto a destra e seleziona 'New repository'" -ForegroundColor White
Write-Host "3. Configura il repository:" -ForegroundColor White
Write-Host "   - Nome: LivelloHDServicePRO" -ForegroundColor Gray
Write-Host "   - Descrizione: Sistema Professionale di Calcolo SLA" -ForegroundColor Gray
Write-Host "   - Visibilità: Private (raccomandato) o Public" -ForegroundColor Gray
Write-Host "   - NON aggiungere README, .gitignore o license (già esistenti)" -ForegroundColor Gray
Write-Host "4. Clicca 'Create repository'" -ForegroundColor White
Write-Host ""
Write-Host "5. Copia l'URL del repository (es: https://github.com/username/LivelloHDServicePRO.git)" -ForegroundColor White
Write-Host ""

$repoUrl = Read-Host "Incolla qui l'URL del repository GitHub"

if ([string]::IsNullOrEmpty($repoUrl)) {
    Write-Host ""
    Write-Host "? URL non fornito" -ForegroundColor Red
    Write-Host ""
    Write-Host "Puoi eseguire manualmente questi comandi quando hai l'URL:" -ForegroundColor Yellow
    Write-Host "  git remote add origin <URL_REPOSITORY>" -ForegroundColor Gray
    Write-Host "  git push -u origin main" -ForegroundColor Gray
    Write-Host ""
    Read-Host "Premi ENTER per uscire"
    exit 0
}

Write-Host ""
Write-Host "PASSO 6: Collegamento Repository Remoto" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow

# Rimuovi remote origin se esiste già
$existingRemote = git remote get-url origin 2>$null
if ($existingRemote) {
    Write-Host "Rimuovo remote esistente..." -ForegroundColor Gray
    git remote remove origin
}

Write-Host "Collegando al repository GitHub..." -ForegroundColor Gray
git remote add origin $repoUrl
Write-Host "? Repository remoto collegato" -ForegroundColor Green

Write-Host ""
Write-Host "PASSO 7: Push su GitHub" -ForegroundColor Yellow
Write-Host "=======================" -ForegroundColor Yellow

Write-Host "Caricando file su GitHub..." -ForegroundColor Gray
Write-Host "(Potrebbe essere richiesto di autenticarti)" -ForegroundColor Yellow
Write-Host ""

git push -u origin main

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host "  ? CARICAMENTO COMPLETATO!         " -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Il tuo progetto è ora su GitHub!" -ForegroundColor Cyan
    Write-Host "URL: $repoUrl" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Prossimi passi suggeriti:" -ForegroundColor White
    Write-Host "1. Aggiungi una descrizione al repository" -ForegroundColor Gray
    Write-Host "2. Aggiungi topics/tags (es: wpf, dotnet, sla, helpdesk)" -ForegroundColor Gray
    Write-Host "3. Abilita GitHub Pages se necessario" -ForegroundColor Gray
    Write-Host "4. Configura branch protection per 'main'" -ForegroundColor Gray
} else {
    Write-Host ""
    Write-Host "? Errore durante il push" -ForegroundColor Red
    Write-Host "Potrebbe essere necessario autenticarsi con un token GitHub" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Per creare un token:" -ForegroundColor White
    Write-Host "1. Vai su GitHub.com ? Settings ? Developer settings" -ForegroundColor Gray
    Write-Host "2. Personal access tokens ? Tokens (classic)" -ForegroundColor Gray
    Write-Host "3. Generate new token ? Classic" -ForegroundColor Gray
    Write-Host "4. Seleziona scope: repo, workflow" -ForegroundColor Gray
    Write-Host "5. Copia il token e usalo al posto della password" -ForegroundColor Gray
}

Write-Host ""
Read-Host "Premi ENTER per uscire"
