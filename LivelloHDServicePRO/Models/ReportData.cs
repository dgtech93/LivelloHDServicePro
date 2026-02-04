using System;
using System.Collections.Generic;

namespace LivelloHDServicePRO.Models
{
    /// <summary>
    /// Modello per i dati del report di analisi
    /// </summary>
    public class ReportData
    {
        public DateTime DataGenerazione { get; set; } = DateTime.Now;
        public int TotalTickets { get; set; }
        public int TicketsRisolti { get; set; }
        public int TicketsInCorso { get; set; }
        public string PeriodoAnalisi { get; set; } = string.Empty;

        // Tempi medi generali
        public List<TempoMedioPriorita> TempiMediPerPriorita { get; set; } = new();
        
        // Analisi per proprietario
        public List<AnalisiProprietario> AnalisiProprietari { get; set; } = new();
        
        // ?? Analisi dettagliata proprietario-priorità
        public List<AnalisiProprietarioPriorita> AnalisiProprietariPerPriorita { get; set; } = new();
        
        // ?? Analisi dettagliata delle risorse
        public List<AnalisiDettagliataRisorsa> AnalisiDettagliataRisorse { get; set; } = new();
        
        // Distribuzione ticket
        public List<DistribuzionePriorita> DistribuzionePriorita { get; set; } = new();
        public List<DistribuzioneProprietario> DistribuzioneProprietario { get; set; } = new();
        
        // SLA Performance
        public List<SlaPerformance> SlaPerformanceData { get; set; } = new();
        
        // Statistiche di riepilogo
        public RiepilogoStatistiche RiepilogoGenerale { get; set; } = new();
    }

    /// <summary>
    /// Tempi medi divisi per priorità
    /// </summary>
    public class TempoMedioPriorita
    {
        public string Priorita { get; set; } = string.Empty;
        public int NumeroTickets { get; set; }
        public TimeSpan TempoMedioTMC { get; set; }
        public TimeSpan TempoMedioTMS { get; set; }
        public TimeSpan TempoMedioTEFF { get; set; }
        public TimeSpan TempoMedioTSOSP { get; set; }
        
        // Formattazione per visualizzazione
        public string TempoMedioTMCFormatted => FormatTimeSpan(TempoMedioTMC);
        public string TempoMedioTMSFormatted => FormatTimeSpan(TempoMedioTMS);
        public string TempoMedioTEFFFormatted => FormatTimeSpan(TempoMedioTEFF);
        public string TempoMedioTSOSPFormatted => FormatTimeSpan(TempoMedioTSOSP);

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts == TimeSpan.Zero) return "N/A";
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }

    /// <summary>
    /// Analisi dettagliata per ogni proprietario
    /// </summary>
    public class AnalisiProprietario
    {
        public string NomeProprietario { get; set; } = string.Empty;
        public int TotalTickets { get; set; }
        public int TicketsRisolti { get; set; }
        public int TicketsNonRisolti => TotalTickets - TicketsRisolti;
        public double PercentualeRisoluzione => TotalTickets > 0 ? (double)TicketsRisolti / TotalTickets * 100 : 0;
        
        // Nuovi campi per SLA
        public int TicketsFuoriSLATMC { get; set; }
        public int TicketsFuoriSLATEFF { get; set; }
        public int TicketsInSLA { get; set; }
        
        // Tempi medi per priorità
        public Dictionary<string, TempiProprietario> TempiPerPriorita { get; set; } = new();
        
        // Valutazione complessiva
        public ValutazioneQualita ValutazioneComplessiva { get; set; }
        public string DescrizioneValutazione => GetDescrizioneValutazione();
        public int ValutazioneNumerica => GetValutazioneNumerica();
        public string ColorValutazione => GetColorValutazione();
        
        // Punteggio su 100
        public double Punteggio { get; set; } = 0;

        private string GetDescrizioneValutazione()
        {
            return $"{(int)Math.Round(Punteggio)}/100";
        }

        private int GetValutazioneNumerica()
        {
            return (int)Math.Round(Punteggio);
        }

        private string GetColorValutazione()
        {
            return ValutazioneComplessiva switch
            {
                ValutazioneQualita.Ottimo => "#4CAF50",     // Verde
                ValutazioneQualita.Discreto => "#FF9800",   // Arancione
                ValutazioneQualita.Migliorabile => "#FF5722", // Rosso arancione
                ValutazioneQualita.Critico => "#F44336",    // Rosso
                _ => "#9E9E9E"                              // Grigio
            };
        }
    }

    /// <summary>
    /// Analisi dettagliata per proprietario divisa per priorità
    /// </summary>
    public class AnalisiProprietarioPriorita
    {
        public string NomeProprietario { get; set; } = string.Empty;
        public string Priorita { get; set; } = string.Empty;
        public int NumeroTickets { get; set; }
        public int TicketsRisolti { get; set; }
        public double PercentualeRisoluzione => NumeroTickets > 0 ? (double)TicketsRisolti / NumeroTickets * 100 : 0;
        
        // Tempi medi per questa combinazione proprietario-priorità
        public TimeSpan TempoMedioTMC { get; set; }
        public TimeSpan TempoMedioTMS { get; set; }
        public TimeSpan TempoMedioTEFF { get; set; }
        public TimeSpan TempoMedioTSOSP { get; set; }
        
        // Formattazione per visualizzazione
        public string TempoMedioTMCFormatted => FormatTimeSpan(TempoMedioTMC);
        public string TempoMedioTMSFormatted => FormatTimeSpan(TempoMedioTMS);
        public string TempoMedioTEFFFormatted => FormatTimeSpan(TempoMedioTEFF);
        public string TempoMedioTSOSPFormatted => FormatTimeSpan(TempoMedioTSOSP);
        
        // Nuovi campi per conteggio fuori SLA
        public int NumeroTMCFuoriSLA { get; set; }
        public int NumeroTEFFSuoriSLA { get; set; }
        
        // Indice di gravità per ordinamento (alto = critico)
        public double IndiceGravita { get; set; }
        public string LivelloGravita { get; set; } = string.Empty; // "Critico", "Alto", "Medio", "Basso"
        
        // Valutazione specifica per questa priorità
        public ValutazioneQualita ValutazionePriorita { get; set; }
        public string DescrizioneValutazionePriorita => GetDescrizioneValutazionePriorita();
        public int ValutazioneNumerica => GetValutazioneNumerica();
        public string ColorValutazionePriorita => GetColorValutazionePriorita();
        
        // Punteggio su 100
        public double Punteggio { get; set; } = 0;
        
        // SLA Performance per questa combinazione
        public int TicketsFuoriSLA { get; set; }
        public double PercentualeFuoriSLA => NumeroTickets > 0 ? (double)TicketsFuoriSLA / NumeroTickets * 100 : 0;

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts == TimeSpan.Zero) return "N/A";
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        private string GetDescrizioneValutazionePriorita()
        {
            return $"{(int)Math.Round(Punteggio)}/100";
        }

        private int GetValutazioneNumerica()
        {
            return (int)Math.Round(Punteggio);
        }

        private string GetColorValutazionePriorita()
        {
            return ValutazionePriorita switch
            {
                ValutazioneQualita.Ottimo => "#4CAF50",     // Verde
                ValutazioneQualita.Discreto => "#FF9800",   // Arancione
                ValutazioneQualita.Migliorabile => "#FF5722", // Rosso arancione
                ValutazioneQualita.Critico => "#F44336",    // Rosso
                _ => "#9E9E9E"                              // Grigio
            };
        }
    }

    /// <summary>
    /// Analisi dettagliata e completa di una risorsa/proprietario
    /// </summary>
    public class AnalisiDettagliataRisorsa
    {
        public string NomeProprietario { get; set; } = string.Empty;
        
        // Statistiche generali
        public int TicketTotali { get; set; }
        public int TicketChiusi { get; set; }
        public int TicketInCorso { get; set; }
        public double PercentualeRisoluzione => TicketTotali > 0 ? (double)TicketChiusi / TicketTotali * 100 : 0;
        
        // Tempi medi generali
        public TimeSpan TempoMedioTMC { get; set; }
        public TimeSpan TempoMedioTMS { get; set; }
        public TimeSpan TempoMedioTEFF { get; set; }
        public TimeSpan TempoMedioTSOSP { get; set; }
        
        // Performance SLA
        public int TicketsEntroSLA { get; set; }
        public int TicketsFuoriSLA { get; set; }
        public double PercentualeEntroSLA => TicketTotali > 0 ? (double)TicketsEntroSLA / TicketTotali * 100 : 0;
        public double PercentualeFuoriSLA => TicketTotali > 0 ? (double)TicketsFuoriSLA / TicketTotali * 100 : 0;
        
        // Lista ticket fuori SLA
        public List<SlaRecord> ListaTicketFuoriSLA { get; set; } = new();
        
        // Distribuzione per priorità
        public Dictionary<string, int> TicketsPerPriorita { get; set; } = new();
        public string PrioritaPiuGestita => GetPrioritaPiuGestita();
        
        // Valutazioni
        public ValutazioneQualita ValutazioneComplessiva { get; set; }
        public int VotazioneNumerica => GetVotazioneNumerica();
        public string DescrizioneValutazione => GetDescrizioneValutazione();
        
        // Nuovo sistema di valutazione su 100 punti
        public double PunteggioTotale { get; set; } // Su 100
        public double PunteggioTMC { get; set; } // Su 20
        public double PunteggioTEFF { get; set; } // Su 20
        public double PunteggioTempiMedi { get; set; } // Su 20
        public double PunteggioRisoluzione { get; set; } // Su 20
        public double PunteggioVolume { get; set; } // Su 20
        
        // Tendenze mensili (ultimi 3 mesi se disponibili)
        public List<TendenzaMensile> TendenzeMensili { get; set; } = new();
        
        // Analisi e suggerimenti
        public List<string> PuntiDiForza { get; set; } = new();
        public List<string> AreeDiMiglioramento { get; set; } = new();
        public List<string> SuggerimentiAzioni { get; set; } = new();
        public string SintesiAnalitica { get; set; } = string.Empty;
        
        // Comparazione con media team
        public double DeviazioneDallaMediaTMC { get; set; } // in percentuale
        public double DeviazioneDallaMediaTEFF { get; set; } // in percentuale
        public string PosizioneRelativa { get; set; } = string.Empty; // "Top 20%", "Nella media", "Bottom 20%"
        
        // Formattazione per display
        public string TempoMedioTMCFormatted => FormatTimeSpan(TempoMedioTMC);
        public string TempoMedioTMSFormatted => FormatTimeSpan(TempoMedioTMS);
        public string TempoMedioTEFFFormatted => FormatTimeSpan(TempoMedioTEFF);
        public string TempoMedioTSOSPFormatted => FormatTimeSpan(TempoMedioTSOSP);
        
        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts == TimeSpan.Zero) return "N/A";
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        private string GetPrioritaPiuGestita()
        {
            if (!TicketsPerPriorita.Any()) return "N/A";
            return TicketsPerPriorita.OrderByDescending(x => x.Value).First().Key;
        }

        private int GetVotazioneNumerica()
        {
            // Ora basato su punteggio su 100
            return (int)Math.Round(PunteggioTotale);
        }

        private string GetDescrizioneValutazione()
        {
            var punteggio = (int)Math.Round(PunteggioTotale);
            return $"{punteggio}/100";
        }
    }

    /// <summary>
    /// Tendenza mensile di una risorsa
    /// </summary>
    public class TendenzaMensile
    {
        public string Mese { get; set; } = string.Empty; // "2024-01", "2024-02", etc.
        public string MeseDisplay { get; set; } = string.Empty; // "Gennaio 2024"
        public int TicketsGestiti { get; set; }
        public double PercentualeRisoluzione { get; set; }
        public TimeSpan TempoMedioTEFF { get; set; }
        public string TendenzaIcon { get; set; } = "?"; // ? ? ?
        public string TendenzaDescrizione { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tempi specifici di un proprietario
    /// </summary>
    public class TempiProprietario
    {
        public TimeSpan TempoMedioTMC { get; set; }
        public TimeSpan TempoMedioTMS { get; set; }
        public TimeSpan TempoMedioTEFF { get; set; }
        public int NumeroTickets { get; set; }
    }

    /// <summary>
    /// Valutazione qualitativa delle prestazioni
    /// </summary>
    public enum ValutazioneQualita
    {
        NonValutabile,
        Critico,      // >= 90% del limite SLA
        Migliorabile, // 70-89% del limite SLA
        Discreto,     // 50-69% del limite SLA  
        Ottimo        // < 50% del limite SLA
    }

    /// <summary>
    /// Distribuzione ticket per priorità
    /// </summary>
    public class DistribuzionePriorita
    {
        public string Priorita { get; set; } = string.Empty;
        public int NumeroTickets { get; set; }
        public double Percentuale { get; set; }
        public int TicketsFuoriSLA { get; set; }
        public double PercentualeFuoriSLA => NumeroTickets > 0 ? (double)TicketsFuoriSLA / NumeroTickets * 100 : 0;
    }

    /// <summary>
    /// Distribuzione ticket per proprietario
    /// </summary>
    public class DistribuzioneProprietario
    {
        public string Proprietario { get; set; } = string.Empty;
        public int NumeroTickets { get; set; }
        public double Percentuale { get; set; }
        public int TicketsRisolti { get; set; }
        public double PercentualeRisoluzione => NumeroTickets > 0 ? (double)TicketsRisolti / NumeroTickets * 100 : 0;
    }

    /// <summary>
    /// Performance SLA generale
    /// </summary>
    public class SlaPerformance
    {
        public string Categoria { get; set; } = string.Empty; // TMC, TMS, T-EFF
        public int TicketsTotali { get; set; }
        public int TicketsEntroSLA { get; set; }
        public int TicketsFuoriSLA { get; set; }
        public double PercentualeEntroSLA => TicketsTotali > 0 ? (double)TicketsEntroSLA / TicketsTotali * 100 : 0;
        public double PercentualeFuoriSLA => TicketsTotali > 0 ? (double)TicketsFuoriSLA / TicketsTotali * 100 : 0;
    }

    /// <summary>
    /// Statistiche di riepilogo generale
    /// </summary>
    public class RiepilogoStatistiche
    {
        public TimeSpan TempoMedioGlobaleTMC { get; set; }
        public TimeSpan TempoMedioGlobaleTMS { get; set; }
        public TimeSpan TempoMedioGlobaleTEFF { get; set; }
        
        public double PercentualeComplessivaEntroSLA { get; set; }
        public double PercentualeComplessivaFuoriSLA { get; set; }
        
        public string ProprietarioMigliore { get; set; } = string.Empty;
        public string ProprietarioCritico { get; set; } = string.Empty;
        
        public string PrioritaPiuProblematica { get; set; } = string.Empty;
        public string PrioritaMenoProblematica { get; set; } = string.Empty;

        // Formattazione
        public string TempoMedioGlobaleTMCFormatted => FormatTimeSpan(TempoMedioGlobaleTMC);
        public string TempoMedioGlobaleTMSFormatted => FormatTimeSpan(TempoMedioGlobaleTMS);
        public string TempoMedioGlobaleTEFFFormatted => FormatTimeSpan(TempoMedioGlobaleTEFF);

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts == TimeSpan.Zero) return "N/A";
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }
}