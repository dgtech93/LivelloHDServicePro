using System;

namespace LivelloHDServicePRO.Models
{
    public class SlaRecord
    {
        public string Proprietario { get; set; } = string.Empty;
        public string NumeroCaso { get; set; } = string.Empty;
        public string Titolo { get; set; } = string.Empty;
        public DateTime? DataCreazione { get; set; }
        public string Priorita { get; set; } = string.Empty;
        public string TipoCaso { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public string NoteChiusura { get; set; } = string.Empty;
        public string MotivoStato { get; set; } = string.Empty;
        public string Contatto { get; set; } = string.Empty;
        public DateTime? DataPresaInCarico { get; set; }
        public DateTime? DataInizioSospensione { get; set; }
        public DateTime? DataFineSospensione { get; set; }
        public DateTime? DataChiusura { get; set; }
        public DateTime? DataValidazione { get; set; }
        public string RoadmapAssociata { get; set; } = string.Empty;
        public string StatoImpegnoAttivita { get; set; } = string.Empty;
        public DateTime? DataScadenzaAttivita { get; set; }
        public string RilascioRoadmapInTest { get; set; } = string.Empty;
        public string RilascioRoadmapInProduzione { get; set; } = string.Empty;

        // Proprietà calcolate per SLA (verranno popolate dopo il calcolo)
        public string TMC { get; set; } = string.Empty; // Tempo Medio di Carico
        public string TMS { get; set; } = string.Empty; // Tempo Medio di Soluzione
        public string TSOSP { get; set; } = string.Empty; // Tempo Sospensione
        public string TEFF { get; set; } = string.Empty; // Tempo Effettivo (TMS - TSOSP)
        
        // Proprietà per SLA violations
        public string TMCFuoriSLA { get; set; } = string.Empty; // TMC Fuori SLA
        public string TEFFFuoriSLA { get; set; } = string.Empty; // T-EFF Fuori SLA
        
        // Proprietà interne per i calcoli (non visibili nella griglia)
        internal TimeSpan? TmcTimeSpan { get; set; }
        internal TimeSpan? TmsTimeSpan { get; set; }
        internal TimeSpan? TsospTimeSpan { get; set; }
        internal TimeSpan? TeffTimeSpan { get; set; }
    }
}