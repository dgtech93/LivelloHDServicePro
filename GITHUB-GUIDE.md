# ?? Guida Rapida: Carica su GitHub

## Metodo Automatico (Raccomandato)

### 1. Esegui lo Script
```powershell
cd Setup
.\GitHub-Upload.ps1
```

Lo script farà automaticamente:
- ? Inizializza repository Git
- ? Configura nome e email
- ? Aggiunge tutti i file
- ? Crea il primo commit
- ? Collega a GitHub
- ? Carica i file

### 2. Crea il Repository su GitHub
1. Vai su [github.com](https://github.com) e accedi
2. Click sul **+** in alto a destra ? **New repository**
3. Compila:
   - **Repository name**: `LivelloHDServicePRO`
   - **Description**: `Sistema Professionale di Calcolo SLA`
   - **Visibility**: Private (raccomandato)
   - ?? **NON** selezionare README, .gitignore, o license
4. Click **Create repository**
5. Copia l'URL (es: `https://github.com/username/LivelloHDServicePRO.git`)
6. Incollalo quando richiesto dallo script

---

## Metodo Manuale

### 1. Inizializza Git (se non fatto)
```bash
git init
git config user.name "Tuo Nome"
git config user.email "tua.email@example.com"
```

### 2. Aggiungi File e Commit
```bash
git add .
git commit -m "Initial commit - Livello HD Service PRO v1.0"
git branch -M main
```

### 3. Crea Repository su GitHub
- Vai su github.com e crea nuovo repository
- **Nome**: LivelloHDServicePRO
- **Visibilità**: Private o Public
- NON aggiungere file predefiniti

### 4. Collega e Carica
```bash
git remote add origin https://github.com/username/LivelloHDServicePRO.git
git push -u origin main
```

---

## ?? Autenticazione

### Token GitHub (se richiesto)
1. GitHub.com ? Settings ? Developer settings
2. Personal access tokens ? Tokens (classic)
3. Generate new token (classic)
4. Seleziona scope: **repo**, **workflow**
5. Copia il token
6. Usalo come password quando richiesto

### GitHub CLI (alternativa)
```bash
# Installa GitHub CLI
winget install GitHub.cli

# Autentica
gh auth login

# Crea e carica
gh repo create LivelloHDServicePRO --private --source=. --remote=origin --push
```

---

## ?? Checklist Pre-Upload

Prima di caricare, verifica:

- [x] `.gitignore` creato (esclude file temporanei)
- [x] `README.md` presente (documentazione)
- [x] File sensibili rimossi (password, token, ecc.)
- [x] File build esclusi (bin/, obj/, Release/)
- [x] Installer esclusi (Releases/)

---

## ?? Dopo il Caricamento

### Configurazione Repository
1. **About**: Aggiungi descrizione e topics
2. **Settings ? General**: Configura default branch (main)
3. **Settings ? Branches**: Branch protection (opzionale)
4. **Settings ? Collaborators**: Aggiungi team (se necessario)

### Tags e Releases
```bash
# Crea tag versione
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0

# Crea release su GitHub con installer
```

### Prossimi Commit
```bash
# Modifica file...
git add .
git commit -m "Descrizione modifiche"
git push
```

---

## ?? Troubleshooting

### "Failed to push"
- Verifica autenticazione (token)
- Verifica URL repository
- Prova: `git push -u origin main --force` (?? usa con cautela)

### "Permission denied"
- Genera nuovo token con permessi corretti
- Verifica di essere owner/collaborator del repo

### "Large files detected"
- Usa Git LFS per file > 50MB
- O escludili dal repository

---

## ?? Risorse Utili

- [GitHub Docs](https://docs.github.com)
- [Git Cheat Sheet](https://education.github.com/git-cheat-sheet-education.pdf)
- [GitHub Desktop](https://desktop.github.com) - GUI alternativa

---

**Buon coding! ??**
