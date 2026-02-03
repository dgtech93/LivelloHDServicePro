# ?? Guida Rapida - Creare l'Installer

## ? Metodo Veloce (3 Passi)

### 1?? Installa Inno Setup
```
Scarica: https://jrsoftware.org/isdl.php
Installa nella posizione predefinita
```

### 2?? Esegui lo Script
**Windows PowerShell (come Amministratore):**
```powershell
cd Setup
.\Build-Installer.ps1
```

**Oppure usa il Batch:**
```cmd
cd Setup
Build-Installer.bat
```

### 3?? Trova l'Installer
L'installer sarà in: `Releases\LivelloHDServicePRO_Setup_v1.0.0.exe`

---

## ?? Checklist Pre-Build

- [ ] .NET 10 SDK installato
- [ ] Inno Setup 6.x installato
- [ ] Progetto compila senza errori in Visual Studio
- [ ] Tutti i file necessari sono nel progetto
- [ ] Numero versione aggiornato (se necessario)

---

## ?? Personalizzazioni Comuni

### Cambiare Versione
Modifica in `LivelloHDServicePRO_Setup.iss`:
```pascal
#define MyAppVersion "1.0.0"  ? Cambia qui
```

### Cambiare Icona
Sostituisci il file: `LivelloHDServicePRO\Assets\app-icon.ico`

### Includere File Extra
Modifica in `LivelloHDServicePRO_Setup.iss`:
```pascal
[Files]
Source: "MioFile.txt"; DestDir: "{app}"; Flags: ignoreversion
```

---

## ?? Problemi Comuni

| Errore | Soluzione |
|--------|-----------|
| "Inno Setup non trovato" | Installa da jrsoftware.org |
| "dotnet non trovato" | Installa .NET 10 SDK |
| "Build fallita" | Controlla errori in Visual Studio |
| "File mancanti" | Verifica cartella publish |

---

## ?? Output Finale

```
Releases/
??? LivelloHDServicePRO_Setup_v1.0.0.exe
    ??? Dimensione: ~50-80 MB
    ??? Tipo: Windows Installer
    ??? Richiede: .NET 10 Runtime
```

---

## ? Test dell'Installer

1. **Installa** su una VM o PC pulito
2. **Verifica** avvio applicazione
3. **Testa** funzioni principali
4. **Disinstalla** e controlla pulizia

---

## ?? Documentazione Completa

Vedi `README.md` per dettagli completi su:
- Configurazioni avanzate
- Opzioni di pubblicazione
- Firma digitale
- Troubleshooting

---

**Pronto? Esegui lo script e in pochi minuti avrai il tuo installer! ??**
