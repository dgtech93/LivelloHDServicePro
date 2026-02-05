# Changelog

Tutte le modifiche importanti a questo progetto saranno documentate in questo file.

Il formato è basato su [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [1.1.3] - 2026-02-05

### ?? HOTFIX Critico
- **BUGFIX CRITICO**: Risolto crash all'avvio su altri PC
  - **Problema**: `System.PlatformNotSupportedException: Method requires System.Windows.Forms`
  - **Causa**: `Microsoft.VisualBasic.Interaction.InputBox` non supportato in .NET 10 WPF
  - **Soluzione**: Sostituito con `InputDialogWindow` WPF nativo
  - Rimosso pacchetto `Microsoft.VisualBasic` (non più necessario)
  - Crash risolto nei seguenti scenari:
    - Creazione nuovo cliente da "Setup SLA"
    - Creazione nuovo cliente da "Orari Lavorativi"

### ? Miglioramenti UI
- **InputDialogWindow**: Dialog WPF moderno e responsive
  - ? **Layout adattivo**: Finestra si adatta automaticamente al contenuto (`SizeToContent="WidthAndHeight"`)
  - ? **Dimensioni ottimizzate**: 
    - `MinWidth="450px"` - Garantisce leggibilità
    - `MaxWidth="700px"` - Previene finestre troppo larghe
  - ? **TextBox migliorata**:
    - Altezza: 40px (più spaziosa)
    - Font size: 14 (più leggibile)
    - Padding: 10px (più comoda)
    - Bordo visibile per migliore identificazione
  - ? **Pulsanti sempre visibili**: Layout a 3 righe (Auto) senza spazi vuoti
  - ? **Stile moderno**: Coerente con il design dell'applicazione
  - ? **Focus automatico**: TextBox selezionata all'apertura

### ?? File Modificati
- **Nuovo**: `InputDialogWindow.xaml` - Dialog WPF nativo
- **Nuovo**: `InputDialogWindow.xaml.cs` - Logica e API semplificata
- **Aggiornato**: `SlaSetupWindow.xaml.cs` - Sostituito InputBox
- **Aggiornato**: `OrariLavorativiWindow.xaml.cs` - Sostituito InputBox
- **Aggiornato**: `LivelloHDServicePRO.csproj` - Rimosso Microsoft.VisualBasic
- **Aggiornato**: `MainWindow.xaml` - Versione aggiornata a v1.1.3

### ? Benefici
- ? **Stabilità**: Applicazione funziona su tutti i PC con .NET 10
- ? **Nessuna dipendenza**: Eliminata dipendenza da Windows.Forms
- ? **UX migliorata**: Dialog più grande, leggibile e responsive
- ? **Dimensioni ridotte**: Installer più leggero (dipendenza rimossa)
- ? **Manutenibilità**: Codice WPF nativo più pulito

---

## [1.1.2] - 2026-02-04

### ?? Corretto
- **BUGFIX CRITICO**: "Analisi Risorse per Priorità" ora calcola correttamente le medie
  - Fix: I tempi medi ora vengono calcolati SOLO su valori validi (> TimeSpan.Zero)
  - Fix: Filtro corretto per evitare dati non veritieri
  - Aggiunto **Indice di Gravità** per identificare situazioni critiche
  - **Ordinamento**: Prima i casi più gravi (alto volume + tempi alti + % fuori SLA)
  - **Colorazione righe**:
    - ?? **Rosso** (Critico): IndiceGravità > 10000 O T-EFF > 500h + >50% fuori SLA
    - ?? **Arancione** (Alto): IndiceGravità > 5000 O T-EFF > 200h O >70% fuori SLA
    - ?? **Giallo** (Medio): IndiceGravità > 1000 O T-EFF > 50h O >30% fuori SLA
    - ?? **Verde** (Basso): Sotto controllo
  - Nuova colonna "Livello" per immediata identificazione criticità

- **REFACTOR LIVELLO GRAVITÀ**: Calcolo semplificato e basato su metriche oggettive
  - **Eliminato**: Sistema complesso con indice multi-fattoriale
  - **Nuovo**: Livello basato SOLO sul **ritardo medio SLA** (molto più chiaro!)
  - **Soglie oggettive**:
    - ?? **Critico**: Ritardo medio > 100 ore (>12 giorni lavorativi)
    - ?? **Alto**: Ritardo medio > 40 ore (>5 giorni lavorativi)
    - ?? **Medio**: Ritardo medio > 16 ore (>2 giorni lavorativi)
    - ?? **Basso**: Ritardo medio ? 16 ore
  - Più facile da capire e spiegare ai manager!

- **REFACTOR RIEPILOGO ESECUTIVO**: Dati oggettivi invece di valutazioni soggettive
  - **Eliminato**: "Proprietario Migliore/Critico" (valutazione generica confusa)
  - **Nuovo**: Metriche oggettive e chiare divise in:
    - **MIGLIORI PERFORMANCE**:
      - Media TMC più bassa + valore
      - Media T-EFF più bassa + valore
      - Maggior volume ticket gestiti + numero
    - **DA MIGLIORARE**:
      - Media TMC più alta + valore
      - Media T-EFF più alta + valore
      - Minor volume ticket gestiti + numero
  - Ogni metrica mostra NOME + VALORE numerico (trasparenza totale!)
  - Aggiornato in: Report UI, Excel export, PDF export
  
- **BUGFIX PDF**: Esportazione PDF Report completo ora aggiornata
- **"Analisi Proprietari" ? "Analisi Risorse"**
  - Rimossa colonna "Valutazione"
  - Nuove colonne: # Non Risolti, # TMC Out, # TEFF Out, # In SLA
  - **TUTTE le risorse** incluse nel PDF (non più solo TOP 15)
  - Contatore totale risorse nel titolo
- **"Analisi Proprietari per Priorità" ? "Analisi Risorse per Priorità"**
  - Rimossa colonna "Valutazione"
  - Nuova colonna "Livello" (Critico/Alto/Medio/Basso)
  - Colorazione righe per gravità (rosso ? verde)
  - Bold per righe "Critico"
  - Nuove colonne: # TMC Out, # TEFF Out
  - **TUTTE le combinazioni** incluse nel PDF (non più solo TOP 20)
  - Contatore totale combinazioni nel titolo
- Sottotitolo "Ordinate per gravità (Critico ? Basso)"
  
- **BUGFIX PDF**: Esportazione PDF AnalisiRisorse ora aggiornata con nuovi dati
  - Aggiunto dettaglio punteggi (5 categorie × 20 punti)
  - Aggiunta sezione "TICKET FUORI SLA" con tabella dettagliata (max 20)
  - Formattazione migliorata e colori aggiornati

### ? Aggiunto
- **AnalisiRisorseWindow**: Aggiunta sezione "TICKET FUORI SLA" con lista dettagliata
  - Mostra Numero Caso, TMC, T-EFF e Priorità per ogni ticket fuori SLA
  - Contatore totale ticket fuori SLA per risorsa
- **AnalisiRisorseWindow**: Sezione "DETTAGLIO PUNTEGGI" con breakdown per categoria
  - Visualizza i 5 punteggi parziali (TMC, T-EFF, Tempi Medi, Risoluzione, Volume)
  - Card colorate per ogni categoria con punteggio X/20

### ?? Modificato
- **AnalisiRisorseWindow**: Finestra ora si apre a **tutto schermo** (WindowState="Maximized")
- **Sistema di Valutazione Risorse**: Completamente rivisto con punteggio **su 100 punti**
  
  **Formula di Valutazione (5 Categorie × 20 punti)**:
  
  1. **TMC rispetto a SLA** (20 punti)
  2. **T-EFF rispetto a SLA** (20 punti)
  3. **Valutazione Tempi Medi** (20 punti)
  4. **Tasso Risoluzione** (20 punti)
  5. **Volume Ticket Gestiti** (20 punti)

- **ReportAnalisiWindow**: Semplificato e focalizzato sui dati concreti
  - ? **Rimosso**: Tutte le colonne di valutazione/punteggi
  - ? **Mantenuto**: Tempi Medi, Performance SLA, Distribuzione, Top Proprietari, Riepilogo
  - ?? **"Analisi Proprietari" ? "Analisi Risorse"**:
    - # Ticket totali
    - # Non Risolti (stato ? Risolto)
    - # Fuori SLA TMC
    - # Fuori SLA T-EFF  
    - # In SLA (entrambi TMC e T-EFF entro SLA)
  - ?? **"Analisi Proprietari per Priorità" ? "Analisi Risorse per Priorità"**:
    - Risorsa + Priorità
    - TMC Medio / T-EFF Medio
    - # TMC Fuori SLA
    - # T-EFF Fuori SLA

### ?? Migliorato
- Report più focalizzato su metriche concrete e quantificabili
- Rimozione di valutazioni soggettive in favore di dati oggettivi
- Migliore chiarezza con conteggi separati per TMC e T-EFF fuori SLA
- AnalisiRisorse mantiene sistema dettagliato di valutazione per analisi approfondita

---

## [1.1.1] - 2024-12-20

### ?? Corretto
- **BUGFIX CRITICO**: Risolto errore "Access Denied" durante il salvataggio delle configurazioni
  - I file di configurazione ora vengono salvati in `%LOCALAPPDATA%\LivelloHDServicePRO\Config\` invece di `Program Files`
  - Risolve il problema di permessi quando si salvano:
    - Orari Lavorativi (`OrariLavorativi.xml`)
    - Setup SLA (`SlaSetup.xml`)
    - Festivita (`Festivita.xml`)
  - L'applicazione ora funziona correttamente senza privilegi di amministratore

### ?? Percorsi Aggiornati
- **Prima**: `C:\Program Files\Livello HD Service PRO\Config\` (richiede admin) ?
- **Dopo**: `C:\Users\[USERNAME]\AppData\Local\LivelloHDServicePRO\Config\` (utente standard) ?

---

## [1.1.0] - 2024-12-XX

### ?? Aggiunto
- **Restyling completo interfaccia moderna**: Tutte le finestre ora hanno un design moderno e professionale coerente
- **Nuove emoji e icone**: Aggiunte emoji intuitive per migliorare l'UX (?? Festività, ?? SLA, ?? Orari, ?? Dettaglio)
- **Icona applicazione**: Nuova icona `sla.ico` sostituisce la vecchia `app-icon.ico`

### ?? Modificato
- **FestivitaWindow**: Design moderno con card, badge e colori coerenti
- **SlaSetupWindow**: Layout compatto (650x800px) con campi input ottimizzati
- **OrariLavorativiWindow**: Finestra auto-ridimensionabile (`SizeToContent`) con limiti min/max
- **SlaDetailWindow**: Restyling con card moderne e badge colorati per le date
- **Style System**: Palette colori unificata (#2C3E50, #7F8C8D, #2196F3, etc.)

### ?? Migliorato
- **UX consistente**: Tutti i bottoni usano lo stesso stile moderno con hover e animazioni
- **Layout ottimizzato**: Margini, padding e spaziature uniformi in tutte le finestre
- **Leggibilità**: Font Segoe UI, dimensioni e pesi ottimizzati
- **Tab modernizzati**: Tab con emoji, colori e stili coerenti

### ?? Corretto
- Layout SlaDetailWindow preservato per evitare perdita di informazioni
- Emoji nei tab e header ora visualizzate correttamente

### ?? Tecnico
- Versione incrementata: 1.0.0 ? 1.1.0
- Build ottimizzata per .NET 10
- Setup Inno aggiornato con nuova icona

---

## [1.0.0] - 2024-XX-XX

### ?? Release Iniziale
- Sistema completo di calcolo SLA per helpdesk
- Import da Excel con supporto multi-formato
- Calcolo TMC, TMS, TSOSP
- Gestione clienti con orari lavorativi personalizzati
- Gestione festività nazionali
- Setup priorità e tempi SLA per cliente
- Report analisi con esportazione Excel/PDF
- Filtri avanzati e salvataggio configurazioni
