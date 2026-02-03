using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LivelloHDServicePRO.Models
{
    public class FilterModel : INotifyPropertyChanged
    {
        private string _selectedProperty = string.Empty;
        private string _selectedValue = string.Empty;
        private string _filterOperator = "equals";

        public string SelectedProperty
        {
            get => _selectedProperty;
            set
            {
                _selectedProperty = value;
                OnPropertyChanged(nameof(SelectedProperty));
                AvailableValues.Clear();
            }
        }

        public string SelectedValue
        {
            get => _selectedValue;
            set
            {
                _selectedValue = value;
                OnPropertyChanged(nameof(SelectedValue));
            }
        }

        public string FilterOperator
        {
            get => _filterOperator;
            set
            {
                _filterOperator = value;
                OnPropertyChanged(nameof(FilterOperator));
            }
        }

        // Supporto per valori multipli separati da virgola
        public List<string> ParsedValues
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectedValue))
                    return new List<string>();

                return SelectedValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(v => v.Trim())
                                   .Where(v => !string.IsNullOrWhiteSpace(v))
                                   .ToList();
            }
        }

        public List<string> AvailableValues { get; set; } = new();
        public List<string> AvailableProperties { get; set; } = new();
        public List<FilterOperatorModel> AvailableOperators { get; set; } = new();

        public bool IsValid => !string.IsNullOrWhiteSpace(SelectedProperty) && 
                              (!string.IsNullOrWhiteSpace(SelectedValue) || 
                               FilterOperator == "empty" || FilterOperator == "not_empty");

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FilterOperatorModel
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public static List<FilterOperatorModel> GetOperators()
        {
            return new List<FilterOperatorModel>
            {
                new() { Id = "equals", DisplayName = "= (uguale)", Description = "Valore esatto" },
                new() { Id = "not_equals", DisplayName = "<> (diverso)", Description = "Diverso dal valore" },
                new() { Id = "contains", DisplayName = "Contiene", Description = "Contiene il testo" },
                new() { Id = "wildcard", DisplayName = "* (wildcard)", Description = "Usa * per pattern: *testo, testo*, *testo*" },
                new() { Id = "empty", DisplayName = "'' (vuoto)", Description = "Campo vuoto o nullo" },
                new() { Id = "not_empty", DisplayName = "Non vuoto", Description = "Campo non vuoto" },
                new() { Id = "greater", DisplayName = "> (maggiore)", Description = "Maggiore del valore (date/numeri)" },
                new() { Id = "less", DisplayName = "< (minore)", Description = "Minore del valore (date/numeri)" },
                new() { Id = "date_from", DisplayName = "Da data..", Description = "Data posteriore o uguale" },
                new() { Id = "date_to", DisplayName = "..A data", Description = "Data anteriore o uguale" },
                new() { Id = "multiple", DisplayName = "Multipli", Description = "Più valori separati da virgola" }
            };
        }
    }

    public class QuickFilterType
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public static List<QuickFilterType> GetQuickFilters()
        {
            return new List<QuickFilterType>
            {
                new() { Id = "none", DisplayName = "Nessun filtro" },
                new() { Id = "not_resolved", DisplayName = "Non risolti" },
                new() { Id = "not_taken", DisplayName = "Non presi in carico" },
                new() { Id = "out_of_sla", DisplayName = "Fuori SLA" },
                new() { Id = "tmc_out_of_sla", DisplayName = "TMC fuori SLA" },
                new() { Id = "teff_out_of_sla", DisplayName = "T-EFF fuori SLA" },
                new() { Id = "today", DisplayName = "Creati oggi" },
                new() { Id = "this_week", DisplayName = "Creati questa settimana" },
                new() { Id = "this_month", DisplayName = "Creati questo mese" },
                new() { Id = "suspended", DisplayName = "In sospensione" }
            };
        }
    }

    public static class FilterService
    {
        public static List<string> GetFilterableProperties()
        {
            return new List<string>
            {
                // Campi principali
                nameof(SlaRecord.Proprietario),
                nameof(SlaRecord.NumeroCaso),
                nameof(SlaRecord.Titolo),
                nameof(SlaRecord.Priorita),
                nameof(SlaRecord.TipoCaso),
                nameof(SlaRecord.Descrizione),
                nameof(SlaRecord.NoteChiusura),
                nameof(SlaRecord.MotivoStato),
                nameof(SlaRecord.Contatto),
                
                // Date
                nameof(SlaRecord.DataCreazione),
                nameof(SlaRecord.DataPresaInCarico),
                nameof(SlaRecord.DataInizioSospensione),
                nameof(SlaRecord.DataFineSospensione),
                nameof(SlaRecord.DataChiusura),
                nameof(SlaRecord.DataValidazione),
                nameof(SlaRecord.DataScadenzaAttivita),
                
                // Altri campi
                nameof(SlaRecord.RoadmapAssociata),
                nameof(SlaRecord.StatoImpegnoAttivita),
                nameof(SlaRecord.RilascioRoadmapInTest),
                nameof(SlaRecord.RilascioRoadmapInProduzione),
                
                // Campi SLA calcolati
                nameof(SlaRecord.TMC),
                nameof(SlaRecord.TMS),
                nameof(SlaRecord.TSOSP),
                nameof(SlaRecord.TEFF),
                nameof(SlaRecord.TMCFuoriSLA),
                nameof(SlaRecord.TEFFFuoriSLA)
            };
        }

        public static string GetPropertyDisplayName(string propertyName)
        {
            return propertyName switch
            {
                // Campi principali
                nameof(SlaRecord.Proprietario) => "Proprietario",
                nameof(SlaRecord.NumeroCaso) => "Numero Caso",
                nameof(SlaRecord.Titolo) => "Titolo",
                nameof(SlaRecord.Priorita) => "Priorità",
                nameof(SlaRecord.TipoCaso) => "Tipo Caso",
                nameof(SlaRecord.Descrizione) => "Descrizione",
                nameof(SlaRecord.NoteChiusura) => "Note di Chiusura",
                nameof(SlaRecord.MotivoStato) => "Motivo Stato",
                nameof(SlaRecord.Contatto) => "Contatto",
                
                // Date
                nameof(SlaRecord.DataCreazione) => "Data Creazione",
                nameof(SlaRecord.DataPresaInCarico) => "Data Presa in Carico",
                nameof(SlaRecord.DataInizioSospensione) => "Data Inizio Sospensione",
                nameof(SlaRecord.DataFineSospensione) => "Data Fine Sospensione",
                nameof(SlaRecord.DataChiusura) => "Data Chiusura",
                nameof(SlaRecord.DataValidazione) => "Data Validazione",
                nameof(SlaRecord.DataScadenzaAttivita) => "Data Scadenza Attività",
                
                // Altri campi
                nameof(SlaRecord.RoadmapAssociata) => "Roadmap Associata",
                nameof(SlaRecord.StatoImpegnoAttivita) => "Stato Impegno Attività",
                nameof(SlaRecord.RilascioRoadmapInTest) => "Rilascio Roadmap in Test",
                nameof(SlaRecord.RilascioRoadmapInProduzione) => "Rilascio Roadmap in Produzione",
                
                // Campi SLA
                nameof(SlaRecord.TMC) => "TMC",
                nameof(SlaRecord.TMS) => "TMS",
                nameof(SlaRecord.TSOSP) => "TSOSP",
                nameof(SlaRecord.TEFF) => "T-EFF",
                nameof(SlaRecord.TMCFuoriSLA) => "TMC Fuori SLA",
                nameof(SlaRecord.TEFFFuoriSLA) => "T-EFF Fuori SLA",
                
                _ => propertyName
            };
        }

        public static List<string> GetUniqueValues(List<SlaRecord> records, string propertyName)
        {
            var values = propertyName switch
            {
                // Campi principali
                nameof(SlaRecord.Proprietario) => records.Select(r => r.Proprietario),
                nameof(SlaRecord.NumeroCaso) => records.Select(r => r.NumeroCaso),
                nameof(SlaRecord.Titolo) => records.Select(r => r.Titolo),
                nameof(SlaRecord.Priorita) => records.Select(r => r.Priorita),
                nameof(SlaRecord.TipoCaso) => records.Select(r => r.TipoCaso),
                nameof(SlaRecord.Descrizione) => records.Select(r => r.Descrizione),
                nameof(SlaRecord.NoteChiusura) => records.Select(r => r.NoteChiusura),
                nameof(SlaRecord.MotivoStato) => records.Select(r => r.MotivoStato),
                nameof(SlaRecord.Contatto) => records.Select(r => r.Contatto),
                
                // Date (convertite in stringhe)
                nameof(SlaRecord.DataCreazione) => records.Select(r => r.DataCreazione?.ToString("dd/MM/yyyy HH:mm")),
                nameof(SlaRecord.DataPresaInCarico) => records.Select(r => r.DataPresaInCarico?.ToString("dd/MM/yyyy HH:mm")),
                nameof(SlaRecord.DataInizioSospensione) => records.Select(r => r.DataInizioSospensione?.ToString("dd/MM/yyyy HH:mm")),
                nameof(SlaRecord.DataFineSospensione) => records.Select(r => r.DataFineSospensione?.ToString("dd/MM/yyyy HH:mm")),
                nameof(SlaRecord.DataChiusura) => records.Select(r => r.DataChiusura?.ToString("dd/MM/yyyy HH:mm")),
                nameof(SlaRecord.DataValidazione) => records.Select(r => r.DataValidazione?.ToString("dd/MM/yyyy HH:mm")),
                nameof(SlaRecord.DataScadenzaAttivita) => records.Select(r => r.DataScadenzaAttivita?.ToString("dd/MM/yyyy HH:mm")),
                
                // Altri campi
                nameof(SlaRecord.RoadmapAssociata) => records.Select(r => r.RoadmapAssociata),
                nameof(SlaRecord.StatoImpegnoAttivita) => records.Select(r => r.StatoImpegnoAttivita),
                nameof(SlaRecord.RilascioRoadmapInTest) => records.Select(r => r.RilascioRoadmapInTest),
                nameof(SlaRecord.RilascioRoadmapInProduzione) => records.Select(r => r.RilascioRoadmapInProduzione),
                
                // Campi SLA
                nameof(SlaRecord.TMC) => records.Select(r => r.TMC),
                nameof(SlaRecord.TMS) => records.Select(r => r.TMS),
                nameof(SlaRecord.TSOSP) => records.Select(r => r.TSOSP),
                nameof(SlaRecord.TEFF) => records.Select(r => r.TEFF),
                nameof(SlaRecord.TMCFuoriSLA) => records.Select(r => r.TMCFuoriSLA),
                nameof(SlaRecord.TEFFFuoriSLA) => records.Select(r => r.TEFFFuoriSLA),
                
                _ => new List<string>()
            };

            return values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct()
                .OrderBy(v => v)
                .Take(50) // Limita a 50 valori per evitare liste troppo lunghe
                .ToList();
        }
    }
}