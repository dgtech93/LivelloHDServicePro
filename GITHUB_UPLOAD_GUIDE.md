# ?? Guida Upload GitHub - Livello HD Service PRO v1.1.0

## ?? Checklist Pre-Upload

- ? Build completata con successo
- ? Versione aggiornata a 1.1.0
- ? File ISS aggiornato con nuova versione e icona
- ? CHANGELOG.md creato
- ? Tutti i file XAML modernizzati

---

## ?? Metodo 1: Upload Manuale (Raccomandato per Prima Volta)

### 1?? Inizializza Repository Git (se non già fatto)
```bash
git init
git add .
git commit -m "?? Release v1.1.0 - Modern UI Update"
```

### 2?? Crea Repository su GitHub
1. Vai su [GitHub.com](https://github.com)
2. Clicca su **"+"** ? **"New repository"**
3. Configura:
   - **Nome**: `LivelloHDServicePRO`
   - **Descrizione**: `Sistema Professionale di Calcolo SLA per Helpdesk - .NET 10 WPF`
   - **Visibilità**: Private o Public (tua scelta)
   - **NON** aggiungere README, .gitignore o license (già esistenti)
4. Clicca **"Create repository"**

### 3?? Collega e Pusha
Copia l'URL del repository (es: `https://github.com/USERNAME/LivelloHDServicePRO.git`)

```bash
git remote add origin https://github.com/USERNAME/LivelloHDServicePRO.git
git branch -M main
git push -u origin main
```

---

## ?? Metodo 2: Script Automatico

### Opzione A: Script PowerShell
```powershell
cd Setup
.\GitHub-Upload.ps1
```

### Opzione B: Comandi Git Diretti
```bash
# Aggiungi tutte le modifiche
git add .

# Commit con messaggio descrittivo
git commit -m "?? v1.1.0 - Modern UI redesign, new icons, improved UX"

# Push su GitHub (se già configurato)
git push
```

---

## ?? Dopo l'Upload

### Crea un Release su GitHub
1. Vai sul repository GitHub
2. Clicca su **"Releases"** ? **"Create a new release"**
3. Configura:
   - **Tag**: `v1.1.0`
   - **Release title**: `v1.1.0 - Modern UI Update`
   - **Description**: Copia dal CHANGELOG.md
   - **Assets**: Carica l'installer da `Releases/LivelloHDServicePRO_Setup_v1.1.0.exe`
4. Clicca **"Publish release"**

### Aggiungi Topics/Tags
Nel repository, aggiungi questi topics:
- `wpf`
- `dotnet`
- `csharp`
- `sla`
- `helpdesk`
- `ticketing`
- `service-level-agreement`
- `desktop-application`

### Proteggi il Branch Main
1. Settings ? Branches
2. Add rule per `main`
3. Abilita:
   - Require pull request reviews before merging
   - Require status checks to pass before merging

---

## ?? Autenticazione

Se richiesto durante il push, usa un **Personal Access Token**:

1. GitHub.com ? Settings ? Developer settings
2. Personal access tokens ? Tokens (classic) ? Generate new token
3. Seleziona scope: `repo`, `workflow`
4. Copia il token
5. Usalo al posto della password quando richiesto

---

## ?? Comandi Git Utili

```bash
# Verifica stato
git status

# Vedi commit history
git log --oneline

# Vedi remote configurato
git remote -v

# Tag per release
git tag -a v1.1.0 -m "Release v1.1.0 - Modern UI"
git push origin v1.1.0

# Vedi differenze
git diff

# Annulla modifiche non committed
git checkout -- <file>
```

---

## ?? Prossimi Passi

1. ? Carica su GitHub
2. ? Crea Release v1.1.0
3. ? Carica installer come asset
4. ?? Aggiorna README.md con screenshots
5. ?? Aggiungi documentazione API
6. ?? Setup CI/CD con GitHub Actions (opzionale)

---

## ?? Support

Per problemi durante l'upload:
- Verifica che Git sia installato: `git --version`
- Verifica autenticazione GitHub
- Controlla firewall/proxy
- Consulta [GitHub Docs](https://docs.github.com)

---

**Buon Upload! ??**
