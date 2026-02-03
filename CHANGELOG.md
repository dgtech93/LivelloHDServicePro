# Changelog

Tutte le modifiche importanti a questo progetto saranno documentate in questo file.

Il formato è basato su [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

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
