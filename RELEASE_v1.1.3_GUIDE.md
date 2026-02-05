# ?? Release v1.1.3 - Guida Rapida

## ? Stato Attuale
- ? Codice aggiornato e testato
- ? CHANGELOG.md aggiornato
- ? Release notes create (GITHUB_RELEASE_v1.1.3.md)
- ? Versione aggiornata a 1.1.3 in:
  - LivelloHDServicePRO.csproj
  - Setup/LivelloHDServicePRO_Setup.iss
  - MainWindow.xaml (status bar)
- ? Compilazione: OK

---

## ?? Passi per Completare la Release

### 1?? Build e Publish

```powershell
# Clean del progetto
dotnet clean LivelloHDServicePRO\LivelloHDServicePRO.csproj

# Publish in Release mode
dotnet publish LivelloHDServicePRO\LivelloHDServicePRO.csproj -c Release -r win-x64 --self-contained true -o ".\Publish"
```

### 2?? Crea Installer con Inno Setup

```powershell
# Genera installer
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "Setup\LivelloHDServicePRO_Setup.iss"

# Verifica file creato
Get-Item "Releases\LivelloHDServicePRO_Setup_v1.1.3.exe" | Select-Object Name, Length, LastWriteTime
```

### 3?? Git Commit e Tag

```powershell
# Stage tutti i file modificati
git add -A

# Commit
git commit -m "HOTFIX v1.1.3 - Risolto crash InputBox, migliorato UI dialog"

# Push su GitHub
git push origin main

# Crea tag
git tag -a v1.1.3 -m "HOTFIX v1.1.3 - InputDialogWindow WPF nativo, UI migliorata"

# Push tag
git push origin v1.1.3
```

### 4?? Crea Release su GitHub

#### Opzione A: Via Web (Consigliata)

1. Vai su: https://github.com/dgtech93/LivelloHDServicePro/releases/new

2. Compila:
   - **Tag**: `v1.1.3`
   - **Title**: `v1.1.3 - HOTFIX Crash InputBox + UI Migliorata`
   - **Description**: Copia da `Releases\GITHUB_RELEASE_v1.1.3.md`

3. Upload Assets:
   - Trascina `Releases\LivelloHDServicePRO_Setup_v1.1.3.exe`

4. Pubblica:
   - ? Spunta "Set as the latest release"
   - Clicca "Publish release"

#### Opzione B: Via GitHub CLI

```powershell
gh release create v1.1.3 `
  --title "v1.1.3 - HOTFIX Crash InputBox + UI Migliorata" `
  --notes-file "Releases\GITHUB_RELEASE_v1.1.3.md" `
  "Releases\LivelloHDServicePRO_Setup_v1.1.3.exe"
```

---

## ?? Cosa Include Questa Release

### ?? Bug Fix Critico
- ? Risolto crash `PlatformNotSupportedException` su altri PC
- ? Sostituito `Microsoft.VisualBasic.InputBox` con dialog WPF nativo
- ? Rimosso pacchetto `Microsoft.VisualBasic`

### ? Miglioramenti UI
- ? Dialog adattivo (SizeToContent)
- ? TextBox più grande (40px) e leggibile (font 14)
- ? Layout responsive (MinWidth 450px, MaxWidth 700px)
- ? Pulsanti sempre visibili
- ? Focus automatico sulla textbox
- ? Stile moderno WPF

### ?? File Modificati
- **Nuovo**: InputDialogWindow.xaml
- **Nuovo**: InputDialogWindow.xaml.cs
- **Aggiornato**: SlaSetupWindow.xaml.cs
- **Aggiornato**: OrariLavorativiWindow.xaml.cs
- **Aggiornato**: LivelloHDServicePRO.csproj (rimosso VisualBasic)
- **Aggiornato**: MainWindow.xaml (versione v1.1.3)

---

## ?? Dimensioni Previste

- **Installer**: ~51 MB (leggermente ridotto senza Microsoft.VisualBasic)
- **Publish folder**: ~180 MB

---

## ? Checklist Finale

Prima di pubblicare, verifica:

- [ ] Compilazione: OK
- [ ] Tutti i file committati
- [ ] Tag v1.1.3 creato e pushato
- [ ] Installer generato in `Releases\LivelloHDServicePRO_Setup_v1.1.3.exe`
- [ ] Installer testato su un PC pulito
- [ ] Release notes pronte in `Releases\GITHUB_RELEASE_v1.1.3.md`
- [ ] CHANGELOG.md aggiornato
- [ ] Versione v1.1.3 visibile nella status bar dell'app

---

## ?? Rollback (Se Necessario)

Se qualcosa va male:

```powershell
# Torna all'ultimo commit funzionante
git reset --hard HEAD~1

# Elimina tag locale
git tag -d v1.1.3

# Elimina tag remoto (se già pushato)
git push origin :refs/tags/v1.1.3
```

---

## ?? Note Aggiuntive

- **Priorità**: HOTFIX critico - risolvere crash bloccante
- **Compatibilità**: .NET 10 Desktop Runtime richiesto
- **Breaking Changes**: Nessuno
- **Migration Required**: No (aggiornamento trasparente)

---

Buona release! ??
