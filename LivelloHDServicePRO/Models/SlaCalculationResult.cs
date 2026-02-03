using System;

namespace LivelloHDServicePRO.Models
{
    public class SlaCalculationResult
    {
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public string TipoCalcolo { get; set; } = string.Empty; // TMC, TMS, TSOSP
        
        // Dettagli calcolo
        public TimeSpan OrePrimoGiorno { get; set; }
        public TimeSpan OreUltimoGiorno { get; set; }
        public int GiorniIntermedi { get; set; }
        public TimeSpan OreGiorniIntermedi { get; set; }
        public int TotaleGiorniConsiderati { get; set; }
        public TimeSpan TotaleOre { get; set; }
        
        // Giorni esclusi dal calcolo
        public int GiorniNonLavorativiEsclusi { get; set; }
        public int FestivitaEscluse { get; set; }
        public int TotaleGiorniPeriodo { get; set; }
        
        // Informazioni aggiuntive
        public bool HasSospensione { get; set; }
        public string DettaglioCalcolo { get; set; } = string.Empty;
        public bool CalcoloValido { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        
        // Formattazione per display
        public string TotaleOreFormatted => FormatTimeSpanHHMMSS(TotaleOre);
        public string OrePrimoGiornoFormatted => FormatTimeSpanHHMMSS(OrePrimoGiorno);
        public string OreUltimoGiornoFormatted => FormatTimeSpanHHMMSS(OreUltimoGiorno);
        public string OreGiorniIntermediFormatted => FormatTimeSpanHHMMSS(OreGiorniIntermedi);
        
        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
            {
                return $"{(int)timeSpan.TotalDays}gg {timeSpan.Hours}h {timeSpan.Minutes}m";
            }
            return $"{timeSpan.Hours}h {timeSpan.Minutes}m";
        }

        private string FormatTimeSpanHHMMSS(TimeSpan timeSpan)
        {
            // Arrotonda i secondi: se > 30 secondi, arrotonda il minuto per eccesso
            var totalSeconds = (int)timeSpan.TotalSeconds;
            var seconds = totalSeconds % 60;
            var totalMinutes = totalSeconds / 60;
            
            if (seconds > 30)
            {
                totalMinutes++; // Arrotonda per eccesso
            }
            
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            
            return $"{hours:D2}:{minutes:D2}:00";
        }

        // Verifica la coerenza del calcolo
        public bool IsCalculationConsistent()
        {
            var calcolateSeparatamente = OrePrimoGiorno + OreGiorniIntermedi + OreUltimoGiorno;
            var differenza = Math.Abs((TotaleOre - calcolateSeparatamente).TotalMinutes);
            
            // Tolleriamo una differenza di max 1 minuto (per arrotondamenti)
            return differenza <= 1;
        }

        public string GetCalculationSummary()
        {
            var sommaComponenti = OrePrimoGiorno + OreGiorniIntermedi + OreUltimoGiorno;
            var isConsistent = IsCalculationConsistent();
            
            var summary = $"Primo: {OrePrimoGiornoFormatted} + ";
            summary += $"Intermedi: {OreGiorniIntermediFormatted} + ";
            summary += $"Ultimo: {OreUltimoGiornoFormatted} = ";
            summary += $"Somma: {FormatTimeSpanHHMMSS(sommaComponenti)}";
            
            if (!isConsistent)
            {
                summary += $"\n?? ATTENZIONE: Differenza con totale calcolato: {TotaleOreFormatted}";
            }
            
            return summary;
        }
    }

    public class SlaDetailInfo
    {
        public SlaRecord Record { get; set; } = new();
        public OrariLavorativiSetup? ClienteSetup { get; set; }
        public SlaSetup? SlaSetup { get; set; }
        public SlaRegola? SlaRegola { get; set; }
        
        // Date principali
        public DateTime? DataApertura { get; set; }
        public DateTime? DataPresaInCarico { get; set; }
        public DateTime? DataInizioSospensione { get; set; }
        public DateTime? DataFineSospensione { get; set; }
        public DateTime? DataChiusura { get; set; }
        
        // Risultati calcoli
        public SlaCalculationResult? TmcResult { get; set; }
        public SlaCalculationResult? TmsResult { get; set; }
        public SlaCalculationResult? TsospResult { get; set; }
        
        // Risultati confronto SLA
        public SlaComparisonResult? TmcComparison { get; set; }
        public SlaComparisonResult? TeffComparison { get; set; }
        
        public bool HasValidTmc => TmcResult?.CalcoloValido == true;
        public bool HasValidTms => TmsResult?.CalcoloValido == true;
        public bool HasValidTsosp => TsospResult?.CalcoloValido == true;
        public bool HasSlaComparison => SlaRegola != null && ClienteSetup != null;
    }

    public class SlaComparisonResult
    {
        public string TipoSla { get; set; } = string.Empty; // "TMC" o "T-EFF"
        public TimeSpan TempoEffettivo { get; set; }
        public TimeSpan TempoSla { get; set; }
        public bool EntroSla { get; set; }
        public TimeSpan Differenza { get; set; }
        
        public string StatusMessage
        {
            get
            {
                if (EntroSla)
                {
                    var ore = (int)Differenza.TotalHours;
                    var minuti = Differenza.Minutes;
                    return $"Entro SLA di {ore:D2}:{minuti:D2}:00";
                }
                else
                {
                    var ore = (int)Differenza.TotalHours;
                    var minuti = Differenza.Minutes;
                    return $"Superato il tempo di {ore:D2}:{minuti:D2}:00";
                }
            }
        }
        
        public string TempoEffettivoFormatted
        {
            get
            {
                var ore = (int)TempoEffettivo.TotalHours;
                var minuti = TempoEffettivo.Minutes;
                return $"{ore:D2}:{minuti:D2}:00";
            }
        }
        
        public string TempoSlaFormatted
        {
            get
            {
                var ore = (int)TempoSla.TotalHours;
                var minuti = TempoSla.Minutes;
                return $"{ore:D2}:{minuti:D2}:00";
            }
        }
    }
}