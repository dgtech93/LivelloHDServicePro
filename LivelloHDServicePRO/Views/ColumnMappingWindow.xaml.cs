using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Views
{
    public class ExcelColumnDisplayItem
    {
        public int Index { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }

    public partial class ColumnMappingWindow : Window
    {
        public List<ColumnMappingModel> ColumnMappings { get; set; }
        public List<ExcelColumnInfo> ExcelColumns { get; set; }
        public List<ExcelColumnDisplayItem> ExcelColumnsWithNone { get; set; }

        public ColumnMappingWindow(List<ColumnMappingModel> columnMappings, List<ExcelColumnInfo> excelColumns)
        {
            InitializeComponent();
            
            ColumnMappings = columnMappings;
            ExcelColumns = excelColumns;
            
            // Create display list with "None" option
            ExcelColumnsWithNone = new List<ExcelColumnDisplayItem>
            {
                new() { Index = -1, DisplayText = "-- Non mappare --" }
            };
            
            foreach (var col in ExcelColumns)
            {
                ExcelColumnsWithNone.Add(new ExcelColumnDisplayItem
                {
                    Index = col.Index,
                    DisplayText = $"{col.Name} (es: {col.SampleValue})"
                });
            }

            MappingDataGrid.ItemsSource = ColumnMappings;
        }

        private void AutoMappingButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mapping in ColumnMappings)
            {
                // Try to find a matching column by name
                var matchingColumn = ExcelColumns.FirstOrDefault(col => 
                    col.Name.ToLower().Contains(GetKeywords(mapping.FieldName).FirstOrDefault()?.ToLower() ?? ""));

                if (matchingColumn != null)
                {
                    mapping.ExcelColumnIndex = matchingColumn.Index;
                    mapping.ExcelColumnName = matchingColumn.Name;
                }
                else
                {
                    // Try partial matching
                    var keywords = GetKeywords(mapping.FieldName);
                    foreach (var keyword in keywords)
                    {
                        matchingColumn = ExcelColumns.FirstOrDefault(col => 
                            col.Name.ToLower().Contains(keyword.ToLower()));
                        
                        if (matchingColumn != null)
                        {
                            mapping.ExcelColumnIndex = matchingColumn.Index;
                            mapping.ExcelColumnName = matchingColumn.Name;
                            break;
                        }
                    }
                }
            }

            MappingDataGrid.Items.Refresh();
        }

        private void ResetMappingButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mapping in ColumnMappings)
            {
                mapping.ExcelColumnIndex = -1;
                mapping.ExcelColumnName = string.Empty;
            }

            MappingDataGrid.Items.Refresh();
        }

        private List<string> GetKeywords(string fieldName)
        {
            return fieldName switch
            {
                nameof(SlaRecord.Proprietario) => new List<string> { "proprietario", "owner", "assegnato" },
                nameof(SlaRecord.NumeroCaso) => new List<string> { "numero", "caso", "ticket", "id", "case" },
                nameof(SlaRecord.Titolo) => new List<string> { "titolo", "title", "oggetto", "subject" },
                nameof(SlaRecord.DataCreazione) => new List<string> { "creazione", "created", "apertura", "data" },
                nameof(SlaRecord.Priorita) => new List<string> { "priorità", "priority", "urgenza" },
                nameof(SlaRecord.TipoCaso) => new List<string> { "tipo", "type", "categoria", "category" },
                nameof(SlaRecord.Descrizione) => new List<string> { "descrizione", "description", "dettaglio" },
                nameof(SlaRecord.NoteChiusura) => new List<string> { "note", "chiusura", "closure", "risoluzione" },
                nameof(SlaRecord.MotivoStato) => new List<string> { "motivo", "stato", "status", "reason" },
                nameof(SlaRecord.Contatto) => new List<string> { "contatto", "contact", "richiedente", "utente" },
                nameof(SlaRecord.DataPresaInCarico) => new List<string> { "presa", "carico", "assigned", "assegnazione" },
                nameof(SlaRecord.DataInizioSospensione) => new List<string> { "inizio", "sospensione", "suspend", "start" },
                nameof(SlaRecord.DataFineSospensione) => new List<string> { "fine", "sospensione", "suspend", "end" },
                nameof(SlaRecord.DataChiusura) => new List<string> { "chiusura", "closed", "risolto", "resolved" },
                nameof(SlaRecord.DataValidazione) => new List<string> { "validazione", "validation", "approvazione" },
                nameof(SlaRecord.RoadmapAssociata) => new List<string> { "roadmap", "associata", "piano" },
                nameof(SlaRecord.StatoImpegnoAttivita) => new List<string> { "stato", "impegno", "attività", "activity" },
                nameof(SlaRecord.DataScadenzaAttivita) => new List<string> { "scadenza", "attività", "deadline", "due" },
                nameof(SlaRecord.RilascioRoadmapInTest) => new List<string> { "test", "rilascio", "roadmap" },
                nameof(SlaRecord.RilascioRoadmapInProduzione) => new List<string> { "produzione", "rilascio", "roadmap", "production" },
                _ => new List<string>()
            };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate required mappings
            var missingRequired = ColumnMappings.Where(m => m.IsRequired && m.ExcelColumnIndex <= 0).ToList();
            
            if (missingRequired.Any())
            {
                var missingFields = string.Join(", ", missingRequired.Select(m => m.DisplayName));
                MessageBox.Show($"I seguenti campi richiesti non sono stati mappati:\n{missingFields}", 
                              "Mappatura incompleta", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}