# Livello HD Service PRO - Setup e Distribuzione

## Prerequisiti

### Software Richiesto
1. **.NET 10 SDK** - Per compilare l'applicazione
   - Download: https://dotnet.microsoft.com/download/dotnet/10.0

2. **Inno Setup 6.x** - Per creare l'installer
   - Download: https://jrsoftware.org/isdl.php
   - Installare nella posizione predefinita: `C:\Program Files (x86)\Inno Setup 6\`

### Opzionale
- **Visual Studio 2022** (o versione superiore) per modifiche al codice

## Struttura File

```
Setup/
??? LivelloHDServicePRO_Setup.iss    # Script Inno Setup
??? Build-Installer.ps1               # Script PowerShell per build
??? Build-Installer.bat               # Script Batch per build
??? README.md                         # Questo file

LivelloHDServicePRO/
??? bin/Release/net10.0-windows/publish/   # Output della pubblicazione
??? ...                                     # File sorgenti

Releases/                             # Cartella output installer (creata automaticamente)
??? LivelloHDServicePRO_Setup_v1.0.0.exe
```

## Come Creare l'Installer

### Metodo 1: Script PowerShell (Raccomandato)
1. Apri PowerShell come **Amministratore**
2. Naviga nella cartella Setup: `cd Setup`
3. Esegui: `.\Build-Installer.ps1`
4. Attendi il completamento (circa 2-5 minuti)
5. L'installer sarà in `..\Releases\`

### Metodo 2: Script Batch
1. Apri il Prompt dei Comandi come **Amministratore**
2. Naviga nella cartella Setup: `cd Setup`
3. Esegui: `Build-Installer.bat`
4. Attendi il completamento
5. L'installer sarà in `..\Releases\`

### Metodo 3: Manuale
1. Pubblica l'applicazione:
   ```bash
   cd LivelloHDServicePRO
   dotnet publish -c Release -f net10.0-windows -r win-x64 --self-contained false
   ```

2. Compila l'installer:
   ```bash
   cd Setup
   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" LivelloHDServicePRO_Setup.iss
   ```

## Configurazione dello Script Inno Setup

### Personalizzazioni Principali

Nel file `LivelloHDServicePRO_Setup.iss`, puoi modificare:

```pascal
#define MyAppVersion "1.0.0"          ; Versione dell'app
#define MyAppPublisher "Livello HD"   ; Nome publisher
```

### Opzioni di Installazione
- **DefaultDirName**: Cartella di installazione predefinita
- **Compression**: Tipo di compressione (lzma2/max è il migliore)
- **PrivilegesRequired**: Se serve admin o no
- **ArchitecturesAllowed**: x64 per sistemi a 64-bit

## Opzioni di Pubblicazione .NET

Lo script utilizza queste opzioni per ottimizzare l'output:

- **PublishSingleFile=false**: File multipli (più veloce all'avvio)
- **PublishReadyToRun=true**: Pre-compilato per performance migliori
- **self-contained=false**: Richiede .NET 10 Runtime installato (file più piccoli)

### Per Installer Autonomo (Più Grande)
Modifica lo script sostituendo:
```
--self-contained false
```
con:
```
--self-contained true
```

Questo include il runtime .NET (circa 150-200 MB in più).

## Verifica dell'Installer

Prima di distribuire, testa l'installer:

1. **Installa** su una macchina pulita (o VM)
2. **Verifica** che l'app si avvii correttamente
3. **Testa** tutte le funzionalità principali
4. **Disinstalla** e verifica la pulizia completa

## Distribuzione

L'installer creato (`LivelloHDServicePRO_Setup_v1.0.0.exe`) può essere:
- Distribuito via email
- Caricato su server/cloud
- Distribuito su rete aziendale
- Pubblicato su sito web

### Requisiti per l'Utente Finale
- Windows 10/11 (64-bit)
- .NET 10 Desktop Runtime (l'installer lo rileva e guida l'installazione)
- Privilegi di amministratore per l'installazione

## Firmare l'Installer (Opzionale ma Raccomandato)

Per evitare avvisi di sicurezza di Windows:

1. Ottieni un **certificato di code signing**
2. Usa **SignTool** per firmare l'exe:
   ```bash
   signtool sign /f certificato.pfx /p password /t http://timestamp.digicert.com LivelloHDServicePRO_Setup_v1.0.0.exe
   ```

## Troubleshooting

### Errore: "Inno Setup non trovato"
- Verifica il percorso di installazione
- Modifica la variabile `InnoSetupCompiler` negli script se necessario

### Errore: ".NET SDK non trovato"
- Installa .NET 10 SDK da Microsoft
- Riavvia il terminale dopo l'installazione

### Errore: "dotnet publish fallito"
- Verifica che il progetto compili in Visual Studio
- Controlla che tutti i pacchetti NuGet siano ripristinati

### File mancanti nell'installer
- Verifica che tutti i file necessari siano in `bin\Release\net10.0-windows\publish\`
- Controlla la sezione `[Files]` dello script Inno Setup

## Aggiornamenti Versione

Quando rilasci una nuova versione:

1. Aggiorna il numero versione in:
   - `LivelloHDServicePRO_Setup.iss` ? `#define MyAppVersion`
   - `AssemblyInfo.cs` o `.csproj` del progetto

2. Ricompila l'installer

3. Testa l'aggiornamento da versione precedente

## Supporto

Per problemi o domande:
- Email: support@livellohd.com
- Documentazione: [link documentazione]
- Issue Tracker: [link repository]

---

**Nota**: Assicurati sempre di testare l'installer su una macchina pulita prima della distribuzione finale!
