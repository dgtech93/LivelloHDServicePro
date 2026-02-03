using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.Win32;
using LivelloHDServicePRO.Models;
using LivelloHDServicePRO.Services;
using LivelloHDServicePRO.Views;

namespace LivelloHDServicePRO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<SlaRecord> _originalData = new();
        private List<SlaRecord> _originalDataBackup = new(); // 🆕 BACKUP INTATTO dei dati originali
        private ObservableCollection<SlaRecord> _filteredData = new();
        private List<ColumnVisibilityModel> _columnVisibilities = new();
        private ExcelImportService _excelImportService = new();
        private ExcelExportService _excelExportService = new();
        private AutoMappingService _autoMappingService = new();
        private OrariLavorativiService _orariService = new();
        private OrariLavorativiSetup? _clienteSelezionato;
        private SlaBulkCalculationService _slaBulkCalculationService = new();
        private ReportAnalisiService _reportService = new(); // 🆕 Servizio per i report
        
        // Filtri avanzati
        private List<FilterModel> _activeFilters = new();
        private Dictionary<string, List<string>> _availableFilterValues = new();
        
        // Data odierna per calcoli SLA
        private bool _useTodayForMissingDates = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeColumns();
            SlaDataGrid.ItemsSource = _filteredData;
            CaricaClientiDisponibili();
            
            // Inizializza i filtri dopo che la finestra è stata caricata
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeFilters();
        }

        private void InitializeColumns()
        {
            _columnVisibilities = new List<ColumnVisibilityModel>
            {
                new() { PropertyName = nameof(SlaRecord.Proprietario), DisplayName = "Proprietario" },
                new() { PropertyName = nameof(SlaRecord.NumeroCaso), DisplayName = "Numero Caso" },
                new() { PropertyName = nameof(SlaRecord.Titolo), DisplayName = "Titolo" },
                new() { PropertyName = nameof(SlaRecord.DataCreazione), DisplayName = "Data Creazione" },
                new() { PropertyName = nameof(SlaRecord.Priorita), DisplayName = "Priorità" },
                new() { PropertyName = nameof(SlaRecord.TipoCaso), DisplayName = "Tipo Caso" },
                new() { PropertyName = nameof(SlaRecord.Descrizione), DisplayName = "Descrizione" },
                new() { PropertyName = nameof(SlaRecord.NoteChiusura), DisplayName = "Note di Chiusura" },
                new() { PropertyName = nameof(SlaRecord.MotivoStato), DisplayName = "Motivo Stato" },
                new() { PropertyName = nameof(SlaRecord.Contatto), DisplayName = "Contatto" },
                new() { PropertyName = nameof(SlaRecord.DataPresaInCarico), DisplayName = "Data Presa in Carico" },
                new() { PropertyName = nameof(SlaRecord.DataInizioSospensione), DisplayName = "Data Inizio Sospensione" },
                new() { PropertyName = nameof(SlaRecord.DataFineSospensione), DisplayName = "Data Fine Sospensione" },
                new() { PropertyName = nameof(SlaRecord.DataChiusura), DisplayName = "Data Chiusura" },
                new() { PropertyName = nameof(SlaRecord.DataValidazione), DisplayName = "Data Validazione" },
                new() { PropertyName = nameof(SlaRecord.RoadmapAssociata), DisplayName = "Roadmap Associata" },
                new() { PropertyName = nameof(SlaRecord.StatoImpegnoAttivita), DisplayName = "Stato Impegno Attività" },
                new() { PropertyName = nameof(SlaRecord.DataScadenzaAttivita), DisplayName = "Data Scadenza Attività" },
                new() { PropertyName = nameof(SlaRecord.RilascioRoadmapInTest), DisplayName = "Rilascio Roadmap in Test" },
                new() { PropertyName = nameof(SlaRecord.RilascioRoadmapInProduzione), DisplayName = "Rilascio Roadmap in Produzione" }
            };

            CreateDataGridColumns();
        }

        private void CreateDataGridColumns()
        {
            SlaDataGrid.Columns.Clear();

            // Aggiungi prima le colonne SLA fisse
            CreateSlaColumns();

            // Poi aggiungi le altre colonne configurabili
            foreach (var columnVisibility in _columnVisibilities)
            {
                DataGridColumn column;

                // Create appropriate column type based on property type
                if (columnVisibility.PropertyName.Contains("Data"))
                {
                    column = new DataGridTextColumn
                    {
                        Header = columnVisibility.DisplayName,
                        Binding = new Binding(columnVisibility.PropertyName) 
                        { 
                            StringFormat = "dd/MM/yyyy HH:mm" 
                        },
                        Width = DataGridLength.SizeToCells,
                        MinWidth = 120,
                        MaxWidth = 180
                    };
                }
                else
                {
                    // Create text column with wrapping
                    var textColumn = new DataGridTextColumn
                    {
                        Header = columnVisibility.DisplayName,
                        Binding = new Binding(columnVisibility.PropertyName),
                        Width = DataGridLength.SizeToCells,
                        MinWidth = GetMinWidth(columnVisibility.PropertyName),
                        MaxWidth = GetMaxWidth(columnVisibility.PropertyName)
                    };

                    // Configure text wrapping style
                    var textBlockStyle = new Style(typeof(TextBlock));
                    textBlockStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                    textBlockStyle.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Top));
                    textBlockStyle.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(4, 2, 4, 2)));
                    textBlockStyle.Setters.Add(new Setter(TextBlock.LineHeightProperty, 16.0));
                    
                    textColumn.ElementStyle = textBlockStyle;
                    column = textColumn;
                }

                column.Visibility = columnVisibility.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                SlaDataGrid.Columns.Add(column);

                // Subscribe to visibility changes
                columnVisibility.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ColumnVisibilityModel.IsVisible))
                    {
                        var col = SlaDataGrid.Columns.FirstOrDefault(c => c.Header.ToString() == columnVisibility.DisplayName);
                        if (col != null)
                        {
                            col.Visibility = columnVisibility.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                };
            }
        }

        private void CreateSlaColumns()
        {
            var slaHeaderStyle = (Style)FindResource("SlaHeaderStyle");
            var slaCellStyle = (Style)FindResource("SlaCellStyle");

            System.Diagnostics.Debug.WriteLine("CreateSlaColumns - Nessuna conversione, sempre formato hh:mm:ss");

            // TMC Column (sempre hh:mm:ss)
            var tmcColumn = new DataGridTextColumn
            {
                Header = "TMC",
                Binding = new Binding(nameof(SlaRecord.TMC)),
                Width = new DataGridLength(110),
                HeaderStyle = slaHeaderStyle,
                ElementStyle = slaCellStyle,
                CanUserSort = true
            };
            SlaDataGrid.Columns.Add(tmcColumn);

            // TMS Column (sempre hh:mm:ss)
            var tmsColumn = new DataGridTextColumn
            {
                Header = "TMS",
                Binding = new Binding(nameof(SlaRecord.TMS)),
                Width = new DataGridLength(110),
                HeaderStyle = slaHeaderStyle,
                ElementStyle = slaCellStyle,
                CanUserSort = true
            };
            SlaDataGrid.Columns.Add(tmsColumn);

            // TSOSP Column (sempre hh:mm:ss)
            var tsospColumn = new DataGridTextColumn
            {
                Header = "TSOSP",
                Binding = new Binding(nameof(SlaRecord.TSOSP)),
                Width = new DataGridLength(110),
                HeaderStyle = slaHeaderStyle,
                ElementStyle = slaCellStyle,
                CanUserSort = true
            };
            SlaDataGrid.Columns.Add(tsospColumn);

            // TEFF Column (sempre hh:mm:ss)
            var teffColumn = new DataGridTextColumn
            {
                Header = "T-EFF",
                Binding = new Binding(nameof(SlaRecord.TEFF)),
                Width = new DataGridLength(110),
                HeaderStyle = slaHeaderStyle,
                ElementStyle = slaCellStyle,
                CanUserSort = true
            };
            SlaDataGrid.Columns.Add(teffColumn);

            // TMC Fuori SLA Column con converter per colori
            var tmcFuoriSlaColumn = new DataGridTemplateColumn
            {
                Header = "TMC Fuori SLA",
                Width = new DataGridLength(120),
                HeaderStyle = slaHeaderStyle,
                CanUserSort = true,
                SortMemberPath = nameof(SlaRecord.TMCFuoriSLA)
            };
            
            // Crea il template per TMC Fuori SLA
            var tmcFuoriSlaTemplate = new DataTemplate();
            var tmcFuoriSlaFactory = new FrameworkElementFactory(typeof(TextBlock));
            tmcFuoriSlaFactory.SetBinding(TextBlock.TextProperty, new Binding(nameof(SlaRecord.TMCFuoriSLA)));
            tmcFuoriSlaFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
            tmcFuoriSlaFactory.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Consolas"));
            tmcFuoriSlaFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            tmcFuoriSlaFactory.SetValue(TextBlock.PaddingProperty, new Thickness(4, 2, 4, 2));
            tmcFuoriSlaFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            
            // Applica i converter per background e foreground
            var tmcBgBinding = new Binding(nameof(SlaRecord.TMCFuoriSLA))
            {
                Converter = (IValueConverter)FindResource("SlaViolationBackgroundConverter"),
                ConverterParameter = "TMC"
            };
            tmcFuoriSlaFactory.SetBinding(TextBlock.BackgroundProperty, tmcBgBinding);
            
            var tmcFgBinding = new Binding(nameof(SlaRecord.TMCFuoriSLA))
            {
                Converter = (IValueConverter)FindResource("SlaViolationForegroundConverter"),
                ConverterParameter = "TMC"
            };
            tmcFuoriSlaFactory.SetBinding(TextBlock.ForegroundProperty, tmcFgBinding);
            
            tmcFuoriSlaTemplate.VisualTree = tmcFuoriSlaFactory;
            tmcFuoriSlaColumn.CellTemplate = tmcFuoriSlaTemplate;
            SlaDataGrid.Columns.Add(tmcFuoriSlaColumn);

            // T-EFF Fuori SLA Column con converter per colori
            var teffFuoriSlaColumn = new DataGridTemplateColumn
            {
                Header = "T-EFF Fuori SLA",
                Width = new DataGridLength(120),
                HeaderStyle = slaHeaderStyle,
                CanUserSort = true,
                SortMemberPath = nameof(SlaRecord.TEFFFuoriSLA)
            };
            
            // Crea il template per T-EFF Fuori SLA
            var teffFuoriSlaTemplate = new DataTemplate();
            var teffFuoriSlaFactory = new FrameworkElementFactory(typeof(TextBlock));
            teffFuoriSlaFactory.SetBinding(TextBlock.TextProperty, new Binding(nameof(SlaRecord.TEFFFuoriSLA)));
            teffFuoriSlaFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
            teffFuoriSlaFactory.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Consolas"));
            teffFuoriSlaFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            teffFuoriSlaFactory.SetValue(TextBlock.PaddingProperty, new Thickness(4, 2, 4, 2));
            teffFuoriSlaFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            
            // Applica i converter per background e foreground
            var teffBgBinding = new Binding(nameof(SlaRecord.TEFFFuoriSLA))
            {
                Converter = (IValueConverter)FindResource("SlaViolationBackgroundConverter"),
                ConverterParameter = "TEFF"
            };
            teffFuoriSlaFactory.SetBinding(TextBlock.BackgroundProperty, teffBgBinding);
            
            var teffFgBinding = new Binding(nameof(SlaRecord.TEFFFuoriSLA))
            {
                Converter = (IValueConverter)FindResource("SlaViolationForegroundConverter"),
                ConverterParameter = "TEFF"
            };
            teffFuoriSlaFactory.SetBinding(TextBlock.ForegroundProperty, teffFgBinding);
            
            teffFuoriSlaTemplate.VisualTree = teffFuoriSlaFactory;
            teffFuoriSlaColumn.CellTemplate = teffFuoriSlaTemplate;
            SlaDataGrid.Columns.Add(teffFuoriSlaColumn);

            System.Diagnostics.Debug.WriteLine("Tutte le colonne SLA create senza converter - sempre hh:mm:ss");
        }

        private double GetMinWidth(string propertyName)
        {
            return propertyName switch
            {
                nameof(SlaRecord.NumeroCaso) => 100,
                nameof(SlaRecord.Priorita) => 80,
                nameof(SlaRecord.TipoCaso) => 120,
                nameof(SlaRecord.Proprietario) => 120,
                nameof(SlaRecord.Contatto) => 120,
                nameof(SlaRecord.StatoImpegnoAttivita) => 140,
                nameof(SlaRecord.MotivoStato) => 100,
                _ => 150
            };
        }

        private double GetMaxWidth(string propertyName)
        {
            return propertyName switch
            {
                nameof(SlaRecord.NumeroCaso) => 150,
                nameof(SlaRecord.Priorita) => 100,
                nameof(SlaRecord.TipoCaso) => 180,
                nameof(SlaRecord.Proprietario) => 200,
                nameof(SlaRecord.Contatto) => 200,
                nameof(SlaRecord.Titolo) => 350,
                nameof(SlaRecord.Descrizione) => 400,
                nameof(SlaRecord.NoteChiusura) => 350,
                nameof(SlaRecord.StatoImpegnoAttivita) => 200,
                nameof(SlaRecord.MotivoStato) => 180,
                nameof(SlaRecord.RoadmapAssociata) => 250,
                nameof(SlaRecord.RilascioRoadmapInTest) => 200,
                nameof(SlaRecord.RilascioRoadmapInProduzione) => 200,
                _ => 300
            };
        }

        private async void ImportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "File Excel (*.xlsx;*.xls)|*.xlsx;*.xls",
                Title = "Seleziona file Excel da importare"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    StatusTextBlock.Text = "Analisi file Excel in corso...";
                    ImportExcelButton.IsEnabled = false;

                    // Get Excel columns and perform auto-mapping
                    List<ExcelColumnInfo> excelColumns = new();
                    List<ColumnMappingModel> columnMappings = new();

                    await Task.Run(() =>
                    {
                        excelColumns = _excelImportService.GetExcelColumns(openFileDialog.FileName);
                        columnMappings = _autoMappingService.AutoMapColumns(excelColumns);
                    });

                    // Show mapping results
                    var mappedCount = columnMappings.Count(m => m.ExcelColumnIndex > 0);
                    var requiredMapped = columnMappings.Where(m => m.IsRequired && m.ExcelColumnIndex > 0).Count();
                    var requiredTotal = columnMappings.Count(m => m.IsRequired);

                    StatusTextBlock.Text = $"Mappati {mappedCount} campi automaticamente. Campi richiesti: {requiredMapped}/{requiredTotal}";

                    // Check if required fields are missing
                    var missingRequired = columnMappings.Where(m => m.IsRequired && m.ExcelColumnIndex <= 0).ToList();
                    if (missingRequired.Any())
                    {
                        var missingFields = string.Join(", ", missingRequired.Select(m => m.DisplayName));
                        var result = MessageBox.Show(
                            $"I seguenti campi obbligatori non sono stati riconosciuti automaticamente:\n{missingFields}\n\n" +
                            "Vuoi aprire la finestra di mappatura manuale?",
                            "Mappatura Automatica Incompleta",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            var mappingWindow = new ColumnMappingWindow(columnMappings, excelColumns)
                            {
                                Owner = this
                            };

                            if (mappingWindow.ShowDialog() != true)
                            {
                                StatusTextBlock.Text = "Importazione annullata";
                                return;
                            }
                        }
                        else
                        {
                            MessageBox.Show("L'importazione continuerà con i campi mappati automaticamente.\n" +
                                          "I campi mancanti risulteranno vuoti.", 
                                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    StatusTextBlock.Text = "Importazione dati in corso...";

                    await Task.Run(() =>
                    {
                        var importedData = _excelImportService.ImportFromExcel(openFileDialog.FileName, columnMappings);
                        
                        // 🆕 SALVA BACKUP INTATTO (deep copy)
                        _originalDataBackup = CreateDeepCopy(importedData);
                        _originalData = importedData;
                        
                        System.Diagnostics.Debug.WriteLine($"✅ Backup salvato: {_originalDataBackup.Count} record");
                        System.Diagnostics.Debug.WriteLine($"📊 Dati di lavoro: {_originalData.Count} record");
                    });

                    StatusTextBlock.Text = "Calcolo SLA in corso...";

                    // Calcola i valori SLA per tutti i record
                    await Task.Run(() =>
                    {
                        _slaBulkCalculationService.CalculateSlaBulk(_originalData, _clienteSelezionato, _useTodayForMissingDates);
                    });

                    _filteredData.Clear();
                    foreach (var record in _originalData)
                    {
                        _filteredData.Add(record);
                    }

                    RecordCountTextBlock.Text = _originalData.Count.ToString();

                    // Aggiorna i valori disponibili per i filtri
                    UpdateAvailableFilterValues();

                    SelectColumnsButton.IsEnabled = true;
                    ExportExcelButton.IsEnabled = true; // Abilita l'esportazione
                    ReportAnalisiButton.IsEnabled = true; // 🆕 Abilita il report di analisi

                    // Show detailed import results
                    StatusTextBlock.Text = $"✅ Importati {_originalData.Count} record da {System.IO.Path.GetFileName(openFileDialog.FileName)} " +
                                          $"({mappedCount} campi mappati automaticamente)";

                    // Show mapping details in a message
                    var mappingDetails = string.Join("\n", 
                        columnMappings.Where(m => m.ExcelColumnIndex > 0)
                                    .Select(m => $"• {m.DisplayName} ← {m.ExcelColumnName}"));

                    if (!string.IsNullOrEmpty(mappingDetails))
                    {
                        MessageBox.Show($"Mappatura automatica completata:\n\n{mappingDetails}", 
                                      "Mappatura Completata", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante l'importazione del file Excel:\n{ex.Message}", 
                                  "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusTextBlock.Text = "❌ Errore durante l'importazione";
                }
                finally
                {
                    ImportExcelButton.IsEnabled = true;
                }
            }
        }

        private async void ExportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_filteredData.Count == 0)
                {
                    MessageBox.Show("Nessun dato da esportare. Importa prima un file Excel.", 
                                  "Nessun Dato", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Configura il dialogo di salvataggio
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Esporta dati SLA in Excel",
                    DefaultExt = ".xlsx",
                    Filter = "File Excel (*.xlsx)|*.xlsx",
                    FileName = _excelExportService.GetSuggestedFileName()
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExportExcelButton.IsEnabled = false;
                    StatusTextBlock.Text = "Esportazione in corso...";

                    await Task.Run(() =>
                    {
                        _excelExportService.ExportToExcel(
                            _filteredData.ToList(),
                            _columnVisibilities,
                            saveFileDialog.FileName);
                    });

                    var recordsCount = _filteredData.Count;
                    var visibleColumnsCount = _columnVisibilities.Count(c => c.IsVisible) + 6; // +6 per le colonne SLA

                    MessageBox.Show($"Esportazione completata con successo!\n\n" +
                                  $"File: {System.IO.Path.GetFileName(saveFileDialog.FileName)}\n" +
                                  $"Record esportati: {recordsCount}\n" +
                                  $"Colonne esportate: {visibleColumnsCount}\n\n" +
                                  $"Posizione: {saveFileDialog.FileName}",
                                  "Esportazione Completata",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    StatusTextBlock.Text = $"✅ Esportazione completata: {recordsCount} record salvati";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'esportazione:\n{ex.Message}",
                              "Errore Esportazione",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                StatusTextBlock.Text = "❌ Errore durante l'esportazione";
            }
            finally
            {
                ExportExcelButton.IsEnabled = true;
            }
        }

        private void SelectColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            var columnSelectionWindow = new ColumnSelectionWindow(_columnVisibilities)
            {
                Owner = this
            };

            columnSelectionWindow.ShowDialog();
        }

        private async void ReportAnalisiButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_originalData.Count == 0)
                {
                    MessageBox.Show("Nessun dato disponibile per generare il report.\n\nImporta prima un file Excel con i dati SLA.", 
                                  "Dati mancanti", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                StatusTextBlock.Text = "Generazione report di analisi in corso...";
                ReportAnalisiButton.IsEnabled = false;

                ReportData reportData = null!;

                await Task.Run(() =>
                {
                    reportData = _reportService.GeneraReport(_originalData, _clienteSelezionato);
                });

                var reportWindow = new Views.ReportAnalisiWindow(reportData)
                {
                    Owner = this
                };

                StatusTextBlock.Text = $"✅ Report generato per {_originalData.Count} record";
                reportWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la generazione del report:\n{ex.Message}", 
                              "Errore Report", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "❌ Errore nella generazione del report";
            }
            finally
            {
                ReportAnalisiButton.IsEnabled = true;
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyAllFilters();
        }

        #region Data Grid Events

        private void SlaDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SlaDataGrid.SelectedItem is SlaRecord selectedRecord)
            {
                try
                {
                    var detailWindow = new SlaDetailWindow(selectedRecord, _clienteSelezionato)
                    {
                        Owner = this
                    };

                    detailWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nell'apertura del dettaglio SLA: {ex.Message}", 
                                  "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Filters Management

        private void InitializeFilters()
        {
            try
            {
                // Verifica che i controlli siano inizializzati
                if (QuickFilterComboBox == null || AddFilterComboBox == null)
                {
                    return;
                }

                // Inizializza filtro veloce
                QuickFilterComboBox.ItemsSource = QuickFilterType.GetQuickFilters();
                QuickFilterComboBox.DisplayMemberPath = nameof(QuickFilterType.DisplayName);
                QuickFilterComboBox.SelectedValuePath = nameof(QuickFilterType.Id);
                QuickFilterComboBox.SelectedValue = "none";

                // Inizializza combobox per aggiungere filtri con ordine logico
                var filterableProperties = FilterService.GetFilterableProperties()
                    .Select(p => new { Value = p, Display = FilterService.GetPropertyDisplayName(p) })
                    .OrderBy(x => GetCategoryOrder(x.Value)) // Prima per categoria
                    .ThenBy(x => x.Display) // Poi alfabeticamente
                    .ToList();

                // Aggiungi il placeholder all'inizio
                var allItems = new List<object>
                {
                    new { Value = "", Display = "-- Aggiungi filtro per colonna --" }
                };
                allItems.AddRange(filterableProperties);

                AddFilterComboBox.ItemsSource = allItems;
                AddFilterComboBox.DisplayMemberPath = "Display";
                AddFilterComboBox.SelectedValuePath = "Value";
                AddFilterComboBox.SelectedIndex = 0;
                
                System.Diagnostics.Debug.WriteLine($"✅ AddFilterComboBox inizializzato con {filterableProperties.Count} colonne disponibili:");
                foreach (var prop in filterableProperties.Take(10)) // Mostra le prime 10
                {
                    System.Diagnostics.Debug.WriteLine($"   • {prop.Display}");
                }
                if (filterableProperties.Count > 10)
                {
                    System.Diagnostics.Debug.WriteLine($"   ... e altre {filterableProperties.Count - 10} colonne");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'inizializzazione dei filtri: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Determina l'ordine di categoria per organizzare le colonne nel filtro
        /// </summary>
        private int GetCategoryOrder(string propertyName)
        {
            return propertyName switch
            {
                // 1. Campi principali identificativi
                nameof(SlaRecord.NumeroCaso) => 1,
                nameof(SlaRecord.Proprietario) => 1,
                nameof(SlaRecord.Titolo) => 1,
                nameof(SlaRecord.Priorita) => 1,
                nameof(SlaRecord.TipoCaso) => 1,
                
                // 2. Date (molto utilizzate per filtrare)  
                nameof(SlaRecord.DataCreazione) => 2,
                nameof(SlaRecord.DataPresaInCarico) => 2,
                nameof(SlaRecord.DataChiusura) => 2,
                nameof(SlaRecord.DataInizioSospensione) => 2,
                nameof(SlaRecord.DataFineSospensione) => 2,
                nameof(SlaRecord.DataValidazione) => 2,
                nameof(SlaRecord.DataScadenzaAttivita) => 2,
                
                // 3. Campi SLA (molto importanti)
                nameof(SlaRecord.TMC) => 3,
                nameof(SlaRecord.TMS) => 3,
                nameof(SlaRecord.TSOSP) => 3,
                nameof(SlaRecord.TEFF) => 3,
                nameof(SlaRecord.TMCFuoriSLA) => 3,
                nameof(SlaRecord.TEFFFuoriSLA) => 3,
                
                // 4. Altri campi operativi
                nameof(SlaRecord.Contatto) => 4,
                nameof(SlaRecord.MotivoStato) => 4,
                nameof(SlaRecord.StatoImpegnoAttivita) => 4,
                
                // 5. Campi descrittivi (meno utilizzati per filtri)
                nameof(SlaRecord.Descrizione) => 5,
                nameof(SlaRecord.NoteChiusura) => 5,
                nameof(SlaRecord.RoadmapAssociata) => 5,
                nameof(SlaRecord.RilascioRoadmapInTest) => 5,
                nameof(SlaRecord.RilascioRoadmapInProduzione) => 5,
                
                _ => 9 // Altri campi alla fine
            };
        }

        private void QuickFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyAllFilters();
        }

        private void AddFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AddFilterComboBox.SelectedValue != null && 
                !string.IsNullOrEmpty(AddFilterComboBox.SelectedValue.ToString()))
            {
                var propertyName = AddFilterComboBox.SelectedValue.ToString()!;
                AddColumnFilter(propertyName);
                
                // Reset alla voce placeholder
                AddFilterComboBox.SelectedIndex = 0;
            }
        }

        private void AddColumnFilter(string propertyName)
        {
            // Permettiamo più filtri per la stessa proprietà
            var existingCount = _activeFilters.Count(f => f.SelectedProperty == propertyName);
            
            var filter = new FilterModel
            {
                SelectedProperty = propertyName,
                AvailableProperties = FilterService.GetFilterableProperties()
            };

            // Popola i valori disponibili per questa proprietà
            if (_availableFilterValues.TryGetValue(propertyName, out var availableValues))
            {
                filter.AvailableValues = availableValues;
            }

            _activeFilters.Add(filter);
            CreateFilterControl(filter);
            
            // Mostra messaggio informativo per filtri multipli sulla stessa proprietà
            if (existingCount >= 1)
            {
                var filterCount = existingCount + 1;
                StatusTextBlock.Text = $"✨ Aggiunto {GetOrdinalNumber(filterCount)} filtro per '{FilterService.GetPropertyDisplayName(propertyName)}' - logica OR applicata";
                System.Diagnostics.Debug.WriteLine($"🔗 Filtro multiplo #{filterCount} per {propertyName} - logica OR");
            }
        }

        /// <summary>
        /// Converte un numero in ordinale italiano
        /// </summary>
        private string GetOrdinalNumber(int number)
        {
            return number switch
            {
                2 => "secondo",
                3 => "terzo", 
                4 => "quarto",
                5 => "quinto",
                _ => $"{number}°"
            };
        }

        private void CreateFilterControl(FilterModel filter)
        {
            // Conta quanti filtri esistono già per questa proprietà
            var existingFiltersCount = _activeFilters.Count(f => f.SelectedProperty == filter.SelectedProperty);
            var isMultipleFilter = existingFiltersCount > 1;

            var filterPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5),
                Background = isMultipleFilter ? 
                    new SolidColorBrush(Color.FromRgb(173, 216, 230)) : // Light Blue per filtri multipli
                    new SolidColorBrush(Color.FromRgb(144, 238, 144)),   // Light Green per filtri singoli
                Tag = filter // Associa il filtro al pannello
            };

            // Label della proprietà con indicazione filtro multiplo
            var displayText = FilterService.GetPropertyDisplayName(filter.SelectedProperty);
            if (isMultipleFilter)
            {
                displayText += $" #{existingFiltersCount}";
            }

            var propertyLabel = new TextBlock
            {
                Text = displayText + ":",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5),
                FontWeight = FontWeights.Bold,
                ToolTip = isMultipleFilter ? 
                    $"Filtro multiplo per {FilterService.GetPropertyDisplayName(filter.SelectedProperty)}\n• I record che soddisfano QUALSIASI di questi filtri verranno mostrati (logica OR)" :
                    $"Filtro per {FilterService.GetPropertyDisplayName(filter.SelectedProperty)}"
            };

            // ComboBox per i valori (dichiarato prima)
            var valueComboBox = new ComboBox
            {
                Width = 180,
                Margin = new Thickness(5),
                IsEditable = true,
                ItemsSource = filter.AvailableValues,
                ToolTip = GetValueTooltipForOperator(filter.FilterOperator)
            };

            // ComboBox per gli operatori
            var operatorComboBox = new ComboBox
            {
                Width = 120,
                Margin = new Thickness(5),
                ItemsSource = FilterOperatorModel.GetOperators(),
                DisplayMemberPath = nameof(FilterOperatorModel.DisplayName),
                SelectedValuePath = nameof(FilterOperatorModel.Id),
                SelectedValue = filter.FilterOperator,
                ToolTip = "Seleziona il tipo di confronto"
            };

            operatorComboBox.SelectionChanged += (s, e) =>
            {
                if (operatorComboBox.SelectedValue != null)
                {
                    filter.FilterOperator = operatorComboBox.SelectedValue.ToString()!;
                    UpdateValueControlForOperator(filter, valueComboBox, operatorComboBox.SelectedValue.ToString()!);
                    if (!string.IsNullOrWhiteSpace(filter.SelectedValue))
                    {
                        ApplyAllFilters();
                    }
                }
            };

            // Inizializza il controllo valore in base all'operatore corrente
            UpdateValueControlForOperator(filter, valueComboBox, filter.FilterOperator);

            valueComboBox.SelectionChanged += (s, e) =>
            {
                if (valueComboBox.SelectedItem != null)
                {
                    filter.SelectedValue = valueComboBox.SelectedItem.ToString()!;
                    ApplyAllFilters();
                }
            };

            // Per ComboBox editabile, monitora i cambiamenti di testo
            valueComboBox.Loaded += (s, e) =>
            {
                var textBox = valueComboBox.Template?.FindName("PART_EditableTextBox", valueComboBox) as TextBox;
                if (textBox != null)
                {
                    textBox.TextChanged += (sender, args) =>
                    {
                        filter.SelectedValue = textBox.Text;
                        // Debounce: applica filtro dopo una breve pausa
                        Task.Delay(300).ContinueWith(_ =>
                        {
                            Dispatcher.Invoke(ApplyAllFilters);
                        });
                    };
                }
            };

            // Bottone per rimuovere il filtro
            var removeButton = new Button
            {
                Content = "✖",
                Width = 25,
                Height = 25,
                Margin = new Thickness(2),
                Background = new SolidColorBrush(Colors.Red),
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                ToolTip = "Rimuovi questo filtro"
            };

            removeButton.Click += (s, e) =>
            {
                RemoveFilter(filter, filterPanel);
            };

            filterPanel.Children.Add(propertyLabel);
            filterPanel.Children.Add(operatorComboBox);
            filterPanel.Children.Add(valueComboBox);
            filterPanel.Children.Add(removeButton);

            ActiveFiltersPanel?.Children.Add(filterPanel);
        }

        private void UpdateValueControlForOperator(FilterModel filter, ComboBox valueComboBox, string operatorType)
        {
            valueComboBox.ToolTip = GetValueTooltipForOperator(operatorType);

            switch (operatorType)
            {
                case "empty":
                case "not_empty":
                    valueComboBox.IsEnabled = false;
                    valueComboBox.Text = "";
                    filter.SelectedValue = ""; // Valore dummy per validazione
                    break;
                
                case "wildcard":
                    valueComboBox.IsEnabled = true;
                    valueComboBox.IsEditable = true;
                    if (string.IsNullOrWhiteSpace(filter.SelectedValue))
                    {
                        valueComboBox.Text = "*esempio*"; // Placeholder
                    }
                    break;
                
                case "multiple":
                    valueComboBox.IsEnabled = true;
                    valueComboBox.IsEditable = true;
                    if (string.IsNullOrWhiteSpace(filter.SelectedValue))
                    {
                        valueComboBox.Text = "valore1, *valore2*, valore3*"; // Placeholder con esempi wildcard
                    }
                    break;
                
                case "date_from":
                case "date_to":
                    valueComboBox.IsEnabled = true;
                    valueComboBox.IsEditable = true;
                    if (string.IsNullOrWhiteSpace(filter.SelectedValue))
                    {
                        valueComboBox.Text = DateTime.Today.ToString("dd/MM/yyyy");
                    }
                    break;
                
                default:
                    valueComboBox.IsEnabled = true;
                    valueComboBox.IsEditable = true;
                    break;
            }
        }

        private string GetValueTooltipForOperator(string operatorType)
        {
            return operatorType switch
            {
                "equals" => "Inserisci il valore esatto da cercare",
                "not_equals" => "Inserisci il valore da escludere",
                "contains" => "Inserisci il testo che deve essere contenuto",
                "wildcard" => "Usa * per pattern:\n• *testo = finisce con 'testo'\n• testo* = inizia con 'testo'\n• *testo* = contiene 'testo'\n• te*to = pattern complesso",
                "empty" => "Trova campi vuoti o nulli (non serve valore)",
                "not_empty" => "Trova campi non vuoti (non serve valore)",
                "greater" => "Inserisci valore numerico o testo (maggiore di)",
                "less" => "Inserisci valore numerico o testo (minore di)",
                "date_from" => "Inserisci data nel formato dd/MM/yyyy",
                "date_to" => "Inserisci data nel formato dd/MM/yyyy", 
                "multiple" => "Inserisci più valori separati da virgola:\n• Valore1, Valore2, Valore3\n• Supporta anche wildcard: *inizio, fine*, *mezzo*",
                _ => "Seleziona o inserisci un valore"
            };
        }

        private void RemoveFilter(FilterModel filter, StackPanel panel)
        {
            _activeFilters.Remove(filter);
            ActiveFiltersPanel?.Children.Remove(panel);
            ApplyAllFilters();
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Pulisci tutti i filtri
                _activeFilters.Clear();
                ActiveFiltersPanel?.Children.Clear();
                
                // Reset dei controlli
                if (QuickFilterComboBox != null)
                    QuickFilterComboBox.SelectedValue = "none";
                
                if (FilterTextBox != null)
                    FilterTextBox.Clear();
                
                // Riapplica senza filtri
                ApplyAllFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la pulizia dei filtri: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyAllFilters()
        {
            if (_originalData.Count == 0) return;

            try
            {
                var quickFilterId = QuickFilterComboBox?.SelectedValue?.ToString() ?? "none";
                var textFilter = FilterTextBox?.Text ?? string.Empty;

                var filteredRecords = FilteringService.ApplyFilters(
                    _originalData, 
                    _activeFilters, 
                    quickFilterId, 
                    textFilter);

                _filteredData.Clear();
                foreach (var record in filteredRecords)
                {
                    _filteredData.Add(record);
                }

                RecordCountTextBlock.Text = _filteredData.Count.ToString();
                
                // Aggiorna status bar con informazioni dettagliate sui filtri
                var totalRecords = _originalData.Count;
                var filteredCount = _filteredData.Count;
                var activeFiltersCount = _activeFilters.Count(f => f.IsValid);
                
                if (filteredCount == totalRecords && activeFiltersCount == 0 && quickFilterId == "none" && string.IsNullOrWhiteSpace(textFilter))
                {
                    StatusTextBlock.Text = "Tutti i record visualizzati";
                }
                else
                {
                    var filterInfo = "";
                    
                    if (activeFiltersCount > 0) 
                    {
                        var propertiesWithFilters = _activeFilters.Where(f => f.IsValid)
                                                                 .GroupBy(f => f.SelectedProperty)
                                                                 .Count();
                        var multipleFilters = activeFiltersCount - propertiesWithFilters;
                        
                        if (multipleFilters > 0)
                        {
                            filterInfo += $"{propertiesWithFilters} colonne ({multipleFilters} multipli), ";
                        }
                        else
                        {
                            filterInfo += $"{activeFiltersCount} filtri colonna, ";
                        }
                    }
                    
                    if (quickFilterId != "none") filterInfo += "filtro veloce, ";
                    if (!string.IsNullOrWhiteSpace(textFilter)) filterInfo += "filtro testo, ";
                    
                    filterInfo = filterInfo.TrimEnd(' ', ',');
                    StatusTextBlock.Text = $"Mostra {filteredCount} di {totalRecords} record ({filterInfo})";
                }

                AggiornaStatusBarCliente();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'applicazione dei filtri: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateAvailableFilterValues()
        {
            if (_originalData.Count > 0)
            {
                _availableFilterValues = FilteringService.GetAvailableValuesForAllProperties(_originalData);
                
                // Aggiorna i valori disponibili per i filtri attivi
                foreach (var filter in _activeFilters)
                {
                    if (_availableFilterValues.TryGetValue(filter.SelectedProperty, out var values))
                    {
                        filter.AvailableValues = values;
                    }
                }
            }
        }

        #endregion

        #region Configuration Menu Event Handlers

        private void OreLavorativeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var nomeClienteSelezionato = _clienteSelezionato?.NomeCliente;
            var orariWindow = new OrariLavorativiWindow(nomeClienteSelezionato)
            {
                Owner = this
            };

            // Sottoscrive l'evento per aggiornare la lista clienti quando viene salvato un nuovo setup
            orariWindow.ClienteSalvato += (s, args) => AggiornaListaClienti();

            orariWindow.ShowDialog();
        }

        private void FestivitaMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var festivitaWindow = new FestivitaWindow()
            {
                Owner = this
            };

            festivitaWindow.ShowDialog();
        }

        private void SlaMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var nomeClienteSelezionato = _clienteSelezionato?.NomeCliente;
            var slaWindow = new SlaSetupWindow(nomeClienteSelezionato)
            {
                Owner = this
            };

            // Sottoscrive l'evento per aggiornare i calcoli SLA quando viene salvato un nuovo setup
            slaWindow.SlaSetupSalvato += async (s, args) => 
            {
                if (_originalData.Count > 0)
                {
                    StatusTextBlock.Text = "Ricalcolo SLA in corso...";
                    
                    await Task.Run(() =>
                    {
                        _slaBulkCalculationService.CalculateSlaBulk(_originalData, _clienteSelezionato);
                    });

                    // Aggiorna la visualizzazione
                    _filteredData.Clear();
                    foreach (var record in _originalData)
                    {
                        _filteredData.Add(record);
                    }

                    var messaggioBase = "SLA ricalcolato dopo aggiornamento configurazione";
                    AggiornaStatusBarCliente();
                    var statusParts = StatusTextBlock.Text.Split('|');
                    StatusTextBlock.Text = messaggioBase + (statusParts.Length > 1 ? $" | {statusParts[1].Trim()}" : "");
                }
            };

            slaWindow.ShowDialog();
        }

        #endregion

        #region Client Management

        private void CaricaClientiDisponibili()
        {
            try
            {
                var setupClienti = _orariService.CaricaOrariLavorativi();
                
                ClienteComboBox.Items.Clear();
                ClienteComboBox.Items.Add("-- Nessun cliente selezionato --");
                
                foreach (var setup in setupClienti.OrderBy(s => s.NomeCliente))
                {
                    ClienteComboBox.Items.Add(setup.NomeCliente);
                }

                // Seleziona il primo elemento (nessun cliente)
                if (ClienteComboBox.Items.Count > 0)
                {
                    ClienteComboBox.SelectedIndex = 0;
                }

                AggiornaStatusBarCliente();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei clienti: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void ClienteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _clienteSelezionato = null;

                if (ClienteComboBox.SelectedItem != null && ClienteComboBox.SelectedIndex > 0)
                {
                    var nomeCliente = ClienteComboBox.SelectedItem.ToString()!;
                    var setupClienti = _orariService.CaricaOrariLavorativi();
                    _clienteSelezionato = setupClienti.FirstOrDefault(s => s.NomeCliente == nomeCliente);
                }

                AggiornaStatusBarCliente();

                // Ricalcola i valori SLA se ci sono dati caricati
                if (_originalData.Count > 0)
                {
                    StatusTextBlock.Text = "Ricalcolo SLA in corso...";
                    
                    await Task.Run(() =>
                    {
                        _slaBulkCalculationService.CalculateSlaBulk(_originalData, _clienteSelezionato, _useTodayForMissingDates);
                    });

                    // Aggiorna la visualizzazione
                    _filteredData.Clear();
                    foreach (var record in _originalData)
                    {
                        _filteredData.Add(record);
                    }

                    var messaggioBase = _clienteSelezionato != null 
                        ? $"SLA ricalcolato per cliente {_clienteSelezionato.NomeCliente}"
                        : "SLA impostato come non disponibile (nessun cliente)";
                    
                    AggiornaStatusBarCliente();
                    var statusParts = StatusTextBlock.Text.Split('|');
                    StatusTextBlock.Text = messaggioBase + (statusParts.Length > 1 ? $" | {statusParts[1].Trim()}" : "");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la selezione del cliente: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AggiornaStatusBarCliente()
        {
            var statusCliente = _clienteSelezionato != null 
                ? $" | Cliente: {_clienteSelezionato.NomeCliente}" 
                : " | Nessun cliente selezionato";

            // Aggiorna la status bar mantenendo il messaggio principale
            var messaggioBase = StatusTextBlock.Text.Split('|')[0].Trim();
            StatusTextBlock.Text = messaggioBase + statusCliente;
        }

        public OrariLavorativiSetup? GetClienteSelezionato()
        {
            return _clienteSelezionato;
        }

        public void AggiornaListaClienti()
        {
            // Metodo per ricaricare la lista clienti (utile dopo aver aggiunto nuovi setup)
            var clienteCorrenteSelezionato = ClienteComboBox.SelectedItem?.ToString();
            CaricaClientiDisponibili();
            
            // Ripristina la selezione precedente se esiste ancora
            if (!string.IsNullOrEmpty(clienteCorrenteSelezionato))
            {
                for (int i = 0; i < ClienteComboBox.Items.Count; i++)
                {
                    if (ClienteComboBox.Items[i].ToString() == clienteCorrenteSelezionato)
                    {
                        ClienteComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Event Handlers

        private void UseTodayDateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _useTodayForMissingDates = true;
            System.Diagnostics.Debug.WriteLine("🟢 CHECKBOX ABILITATO: _useTodayForMissingDates = true");
            System.Diagnostics.Debug.WriteLine("   → Le date mancanti verranno sostituite con data odierna");
            RecalculateSlaWithCurrentSettings();
        }

        private void UseTodayDateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _useTodayForMissingDates = false;
            System.Diagnostics.Debug.WriteLine("🔴 CHECKBOX DISABILITATO: _useTodayForMissingDates = false");
            System.Diagnostics.Debug.WriteLine("   → Ripristino le date originali dal backup");
            RecalculateSlaWithCurrentSettings();
        }

        private async void RecalculateSlaWithCurrentSettings()
        {
            if (_originalData.Count > 0)
            {
                var actionMessage = _useTodayForMissingDates 
                    ? "Ricalcolo SLA con data odierna per date mancanti..." 
                    : "Ripristino dati originali e ricalcolo SLA...";
                
                StatusTextBlock.Text = actionMessage;
                System.Diagnostics.Debug.WriteLine($"RecalculateSlaWithCurrentSettings: {actionMessage}");

                await Task.Run(() =>
                {
                    // 🎯 LOGICA MIGLIORATA: 
                    if (!_useTodayForMissingDates)
                    {
                        // DISABILITATO: Ripristina dai dati originali intatti
                        RestoreFromBackup();
                        System.Diagnostics.Debug.WriteLine("🔄 Ripristinati dati originali dal backup");
                    }
                    // Se abilitato, usa i dati correnti (potrebbero già avere date sostituite)
                    
                    // Calcola SLA con il flag corretto
                    _slaBulkCalculationService.CalculateSlaBulk(_originalData, _clienteSelezionato, _useTodayForMissingDates);
                });

                // Aggiorna la visualizzazione
                ApplyAllFilters();

                var message = _useTodayForMissingDates ? 
                    "✅ SLA ricalcolato CON data odierna per date mancanti" : 
                    "✅ SLA ricalcolato con date ORIGINALI ripristinate";
                
                AggiornaStatusBarCliente();
                var statusParts = StatusTextBlock.Text.Split('|');
                StatusTextBlock.Text = message + (statusParts.Length > 1 ? $" | {statusParts[1].Trim()}" : "");
                
                System.Diagnostics.Debug.WriteLine($"RecalculateSlaWithCurrentSettings completato: {message}");
            }
        }

        #region Data Management

        /// <summary>
        /// Crea una copia profonda della lista di SlaRecord per preservare lo stato originale
        /// </summary>
        private List<SlaRecord> CreateDeepCopy(List<SlaRecord> sourceList)
        {
            var copy = new List<SlaRecord>();
            
            foreach (var original in sourceList)
            {
                var recordCopy = new SlaRecord
                {
                    // Copia TUTTI i campi dal record originale
                    Proprietario = original.Proprietario,
                    NumeroCaso = original.NumeroCaso,
                    Titolo = original.Titolo,
                    Priorita = original.Priorita,
                    TipoCaso = original.TipoCaso,
                    Descrizione = original.Descrizione,
                    NoteChiusura = original.NoteChiusura,
                    MotivoStato = original.MotivoStato,
                    Contatto = original.Contatto,
                    RoadmapAssociata = original.RoadmapAssociata,
                    StatoImpegnoAttivita = original.StatoImpegnoAttivita,
                    RilascioRoadmapInTest = original.RilascioRoadmapInTest,
                    RilascioRoadmapInProduzione = original.RilascioRoadmapInProduzione,
                    
                    // 🎯 COPIA LE DATE ORIGINALI (potrebbero essere null)
                    DataCreazione = original.DataCreazione,
                    DataPresaInCarico = original.DataPresaInCarico,
                    DataInizioSospensione = original.DataInizioSospensione,
                    DataFineSospensione = original.DataFineSospensione,
                    DataChiusura = original.DataChiusura,
                    DataValidazione = original.DataValidazione,
                    DataScadenzaAttivita = original.DataScadenzaAttivita,
                    
                    // Campi SLA (inizialmente vuoti, verranno calcolati)
                    TMC = original.TMC,
                    TMS = original.TMS,
                    TSOSP = original.TSOSP,
                    TEFF = original.TEFF,
                    TMCFuoriSLA = original.TMCFuoriSLA,
                    TEFFFuoriSLA = original.TEFFFuoriSLA,
                    
                    // TimeSpan fields
                    TmcTimeSpan = original.TmcTimeSpan,
                    TmsTimeSpan = original.TmsTimeSpan,
                    TsospTimeSpan = original.TsospTimeSpan,
                    TeffTimeSpan = original.TeffTimeSpan
                };
                
                copy.Add(recordCopy);
            }
            
            System.Diagnostics.Debug.WriteLine($"CreateDeepCopy: Creata copia di {copy.Count} record");
            return copy;
        }

        /// <summary>
        /// Ripristina i dati originali dal backup intatto
        /// </summary>
        private void RestoreFromBackup()
        {
            if (_originalDataBackup.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Nessun backup disponibile per il ripristino");
                return;
            }

            // Ripristina i dati originali dal backup
            _originalData = CreateDeepCopy(_originalDataBackup);
            
            System.Diagnostics.Debug.WriteLine($"🔄 Ripristinati {_originalData.Count} record dal backup");
            System.Diagnostics.Debug.WriteLine($"📋 Date originali preservate (eventuali null mantenuti)");
        }

        #endregion

        #endregion
    }
}
