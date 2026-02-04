# Changelog

Tutte le modifiche importanti a questo progetto saranno documentate in questo file.

Il formato è basato su [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [1.1.2] - 2024-12-XX

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

## [1.1.1] - 2024-12-XX

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
