<<<<<<< HEAD
# ?? Livello HD Service PRO

**Sistema Professionale di Calcolo SLA per Help Desk**

Un'applicazione WPF moderna e completa per il calcolo e l'analisi degli SLA (Service Level Agreement) dei ticket di supporto, con funzionalità avanzate di reporting e analisi delle performance.

![.NET](https://img.shields.io/badge/.NET-10.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![License](https://img.shields.io/badge/license-Proprietary-red)

## ? Caratteristiche Principali

### ?? Import/Export
- **Importazione Excel**: Supporto completo per file Excel con mapping automatico delle colonne
- **Esportazione Excel**: Export dei dati filtrati con formattazione professionale
- **Report PDF**: Generazione di report professionali in PDF con grafici e statistiche

### ?? Calcolo SLA
- **TMC (Tempo Medio di Chiusura)**: Calcolo accurato considerando orari lavorativi
- **TMS (Tempo Medio di Sospensione)**: Gestione automatica delle sospensioni
- **T-EFF (Tempo Effettivo)**: Calcolo del tempo effettivo di lavoro
- **Verifica SLA**: Controllo automatico del rispetto degli SLA con evidenziazione visiva

### ?? Analisi e Report
- **Dashboard Risorse**: Analisi dettagliata per singola risorsa/proprietario
- **Statistiche Avanzate**: Tendenze, percentuali di risoluzione, performance SLA
- **Report Completo**: Report PDF con grafici, statistiche e suggerimenti
- **Analisi Temporale**: Trend mensili e stagionali

### ?? Configurazione
- **Orari Lavorativi**: Setup personalizzato per cliente con orari flessibili
- **Festività**: Gestione calendario festività e giorni non lavorativi
- **Regole SLA**: Configurazione parametri SLA per priorità
- **Mapping Colonne**: Sistema intelligente di mapping colonne Excel

### ?? Filtri Avanzati
- **Filtri Veloci**: Predefiniti per scenari comuni
- **Filtro Testo**: Ricerca globale su tutti i campi
- **Filtri Colonna**: Filtri multipli per singola colonna
- **Combinazioni**: Possibilità di combinare più filtri

## ??? Requisiti di Sistema

- **Sistema Operativo**: Windows 10 (64-bit) o superiore
- **Framework**: .NET 10 Desktop Runtime
- **RAM**: Minimo 4 GB (8 GB raccomandati)
- **Spazio Disco**: 500 MB disponibili
- **Risoluzione**: Minimo 1280x800 (1920x1080 raccomandato)

## ?? Installazione

### Metodo 1: Installer (Raccomandato)
1. Scarica `LivelloHDServicePRO_Setup_v1.0.0.exe` dalla sezione [Releases](../../releases)
2. Esegui l'installer e segui le istruzioni
3. L'applicazione sarà disponibile nel menu Start

### Metodo 2: Build da Sorgente
```bash
# Clona il repository
git clone https://github.com/tuo-username/LivelloHDServicePRO.git
cd LivelloHDServicePRO

# Restore pacchetti
dotnet restore

# Build
dotnet build -c Release

# Esegui
dotnet run --project LivelloHDServicePRO
```

## ?? Come Usare

### Primo Avvio
1. **Configura Orari Lavorativi**: Menu Configurazione ? Ore e Giorni Lavorativi
2. **Aggiungi Festività**: Menu Configurazione ? Festività
3. **Imposta Regole SLA**: Menu Configurazione ? SLA

### Import ed Elaborazione
1. **Importa Excel**: Click su "?? Importa Excel"
2. **Seleziona Cliente**: Scegli il cliente dal menu a tendina
3. **Visualizza Dati**: I dati verranno elaborati e visualizzati nella griglia

### Analisi e Report
1. **Filtra Dati**: Usa i filtri per selezionare i ticket desiderati
2. **Report Analisi**: Click su "?? Report Analisi" per il report completo
3. **Esporta**: Salva in Excel o PDF secondo necessità

## ??? Architettura

```
LivelloHDServicePRO/
??? Models/              # Modelli dati
?   ??? SlaRecord.cs
?   ??? OrariLavorativiSetup.cs
?   ??? Festivita.cs
?   ??? ...
??? Services/            # Logica di business
?   ??? SlaCalculationService.cs
?   ??? ExcelImportService.cs
?   ??? ReportAnalisiService.cs
?   ??? ...
??? Views/               # Interfacce utente
?   ??? MainWindow.xaml
?   ??? ReportAnalisiWindow.xaml
?   ??? AnalisiRisorseWindow.xaml
?   ??? ...
??? Converters/          # Value Converters WPF
??? Setup/               # Script di installazione
```

## ??? Tecnologie Utilizzate

- **.NET 10**: Framework applicativo
- **WPF**: Framework UI
- **EPPlus**: Gestione file Excel
- **QuestPDF**: Generazione PDF
- **Inno Setup**: Creazione installer

## ?? Funzionalità Dettagliate

### Calcolo SLA Intelligente
- Considera solo gli orari lavorativi configurati
- Esclude automaticamente festività e weekend
- Gestisce le sospensioni del ticket
- Supporta configurazioni multiple per cliente

### Report Avanzati
- **Report Globale**: Panoramica completa di tutte le risorse
- **Report Singola Risorsa**: Analisi dettagliata per proprietario
- **Grafici**: Visualizzazioni intuitive delle performance
- **Suggerimenti**: AI-powered suggestions per miglioramenti

### Interfaccia Moderna
- Design Material Design inspired
- Tema chiaro e leggibile
- Responsive e intuitiva
- Keyboard shortcuts

## ?? Privacy e Sicurezza

- **Dati Locali**: Tutti i dati rimangono sul computer dell'utente
- **No Cloud**: Nessun dato viene inviato a server esterni
- **No Internet**: L'applicazione funziona completamente offline
- **Configurazioni Locali**: Salvate in `%LOCALAPPDATA%\LivelloHDServicePRO\`

## ?? Contribuire

Questo è un progetto proprietario. Per suggerimenti o segnalazioni bug:
- Email: support@livellohd.com
- Issue Tracker: [GitHub Issues](../../issues)

## ?? Licenza

Copyright © 2024 Livello HD. Tutti i diritti riservati.

Questo software è proprietario e protetto da copyright. L'uso è soggetto ai termini della licenza fornita con l'applicazione.

## ?? Autori

- **Livello HD** - *Sviluppo iniziale e manutenzione*

## ?? Supporto

- **Email**: support@livellohd.com
- **Web**: www.livellohd.com
- **Telefono**: +39 XXX XXXXXXX
- **Orari**: Lun-Ven 9:00-18:00

## ?? Changelog

### Version 1.0.0 (2024)
- ?? Release iniziale
- ? Calcolo SLA completo (TMC, TMS, T-EFF)
- ? Import/Export Excel
- ? Report PDF professionali
- ? Analisi per risorsa
- ? Filtri avanzati
- ? Interfaccia moderna e intuitiva

## ?? Ringraziamenti

Grazie a tutti coloro che hanno contribuito al testing e al feedback durante lo sviluppo.

---

**Made with ?? by Livello HD**
=======
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
>>>>>>> f6f7e5d8943a87529d8da5fdd8998d35dfa83ef8
