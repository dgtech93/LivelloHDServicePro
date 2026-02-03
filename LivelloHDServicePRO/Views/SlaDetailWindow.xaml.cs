using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LivelloHDServicePRO.Models;
using LivelloHDServicePRO.Services;

namespace LivelloHDServicePRO.Views
{
    public partial class SlaDetailWindow : Window
    {
        private readonly SlaDetailInfo _detailInfo;

        public SlaDetailWindow(SlaRecord record, OrariLavorativiSetup? clienteSetup)
        {
            InitializeComponent();
            
            var calculationService = new SlaCalculationService();
            _detailInfo = calculationService.CalculateSlaDetails(record, clienteSetup);
            
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            // Update header information
            NumeroCasoTextBlock.Text = _detailInfo.Record.NumeroCaso;
            TitoloTextBlock.Text = _detailInfo.Record.Titolo;
            ClienteTextBlock.Text = _detailInfo.ClienteSetup?.NomeCliente ?? "Nessun cliente selezionato";

            // Update date information
            DataAperturaTextBlock.Text = FormatDateTime(_detailInfo.DataApertura);
            DataPresaInCaricoTextBlock.Text = FormatDateTime(_detailInfo.DataPresaInCarico);
            DataChiusuraTextBlock.Text = FormatDateTime(_detailInfo.DataChiusura);

            // Create calculation panels
            CreateTmcPanel();
            CreateTmsPanel();
            CreateTsospPanel();
            CreateSlaComparisonPanel();
        }

        private void CreateTmcPanel()
        {
            TmcPanel.Children.Clear();

            if (!_detailInfo.HasValidTmc)
            {
                var noDataPanel = CreateNoDataPanel("TMC", 
                    _detailInfo.TmcResult?.ErrorMessage ?? 
                    "Dati insufficienti per calcolare il TMC (necessarie Data Apertura e Data Presa in Carico)");
                TmcPanel.Children.Add(noDataPanel);
                return;
            }

            var tmcResult = _detailInfo.TmcResult!;
            
            // Period info
            var periodPanel = CreatePeriodPanel(tmcResult.DataInizio, tmcResult.DataFine);
            TmcPanel.Children.Add(periodPanel);

            // Calculation details
            var detailsPanel = CreateCalculationDetailsPanel(tmcResult);
            TmcPanel.Children.Add(detailsPanel);

            // Exclusions panel
            var exclusionsPanel = CreateExclusionsPanel(tmcResult);
            TmcPanel.Children.Add(exclusionsPanel);

            // Total result
            var totalPanel = CreateTotalPanel(tmcResult.TotaleOreFormatted, "#FF4CAF50");
            TmcPanel.Children.Add(totalPanel);

            // Detailed breakdown
            var breakdownPanel = CreateBreakdownPanel(tmcResult.DettaglioCalcolo);
            TmcPanel.Children.Add(breakdownPanel);
        }

        private void CreateTmsPanel()
        {
            TmsPanel.Children.Clear();

            if (!_detailInfo.HasValidTms)
            {
                var noDataPanel = CreateNoDataPanel("TMS", 
                    _detailInfo.TmsResult?.ErrorMessage ?? 
                    "Dati insufficienti per calcolare il TMS (necessarie Data Apertura e Data Chiusura)");
                TmsPanel.Children.Add(noDataPanel);
                return;
            }

            var tmsResult = _detailInfo.TmsResult!;
            
            // Period info
            var periodPanel = CreatePeriodPanel(tmsResult.DataInizio, tmsResult.DataFine);
            TmsPanel.Children.Add(periodPanel);

            // Calculation details
            var detailsPanel = CreateCalculationDetailsPanel(tmsResult);
            TmsPanel.Children.Add(detailsPanel);

            // Exclusions panel
            var exclusionsPanel = CreateExclusionsPanel(tmsResult);
            TmsPanel.Children.Add(exclusionsPanel);

            // Total result
            var totalPanel = CreateTotalPanel(tmsResult.TotaleOreFormatted, "#FF2196F3");
            TmsPanel.Children.Add(totalPanel);

            // Detailed breakdown
            var breakdownPanel = CreateBreakdownPanel(tmsResult.DettaglioCalcolo);
            TmsPanel.Children.Add(breakdownPanel);
        }

        private void CreateTsospPanel()
        {
            TsospPanel.Children.Clear();

            if (!_detailInfo.HasValidTsosp)
            {
                var noDataPanel = CreateNoDataPanel("TSOSP", 
                    _detailInfo.TsospResult?.ErrorMessage ?? 
                    "Nessuna sospensione registrata per questo caso");
                TsospPanel.Children.Add(noDataPanel);
                return;
            }

            var tsospResult = _detailInfo.TsospResult!;
            
            // Period info
            var periodPanel = CreatePeriodPanel(tsospResult.DataInizio, tsospResult.DataFine, "Sospensione");
            TsospPanel.Children.Add(periodPanel);

            // Calculation details
            var detailsPanel = CreateCalculationDetailsPanel(tsospResult);
            TsospPanel.Children.Add(detailsPanel);

            // Exclusions panel
            var exclusionsPanel = CreateExclusionsPanel(tsospResult);
            TsospPanel.Children.Add(exclusionsPanel);

            // Total result
            var totalPanel = CreateTotalPanel(tsospResult.TotaleOreFormatted, "#FFFF9800");
            TsospPanel.Children.Add(totalPanel);

            // Detailed breakdown
            var breakdownPanel = CreateBreakdownPanel(tsospResult.DettaglioCalcolo);
            TsospPanel.Children.Add(breakdownPanel);
        }

        private GroupBox CreateNoDataPanel(string title, string message)
        {
            var groupBox = new GroupBox
            {
                Header = $"Calcolo {title}",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                FontWeight = FontWeights.Bold
            };

            var textBlock = new TextBlock
            {
                Text = message,
                FontWeight = FontWeights.Normal,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray,
                TextWrapping = TextWrapping.Wrap
            };

            groupBox.Content = textBlock;
            return groupBox;
        }

        private GroupBox CreatePeriodPanel(DateTime startDate, DateTime endDate, string title = "Periodo")
        {
            var groupBox = new GroupBox
            {
                Header = title,
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                FontWeight = FontWeights.Bold
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var startLabel = new TextBlock
            {
                Text = "Da:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 2, 5, 2)
            };
            Grid.SetColumn(startLabel, 0);

            var startValue = new TextBlock
            {
                Text = FormatDateTime(startDate),
                Margin = new Thickness(5, 2, 5, 2),
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Normal
            };
            Grid.SetColumn(startValue, 1);

            var endLabel = new TextBlock
            {
                Text = "A:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(15, 2, 5, 2)
            };
            Grid.SetColumn(endLabel, 2);

            var endValue = new TextBlock
            {
                Text = FormatDateTime(endDate),
                Margin = new Thickness(5, 2, 5, 2),
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Normal
            };
            Grid.SetColumn(endValue, 3);

            grid.Children.Add(startLabel);
            grid.Children.Add(startValue);
            grid.Children.Add(endLabel);
            grid.Children.Add(endValue);

            groupBox.Content = grid;
            return groupBox;
        }

        private GroupBox CreateCalculationDetailsPanel(SlaCalculationResult result)
        {
            var groupBox = new GroupBox
            {
                Header = "Dettaglio Calcolo",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                FontWeight = FontWeights.Bold
            };

            var grid = new Grid();
            // Aumentiamo le colonne per ospitare più informazioni
            for (int i = 0; i < 6; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            }
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            var row = 0;
            var col = 0;

            // Prima riga: Ore del calcolo
            AddDetailItem(grid, "Primo giorno:", result.OrePrimoGiornoFormatted, row, col);
            col += 2;

            AddDetailItem(grid, "Ultimo giorno:", result.OreUltimoGiornoFormatted, row, col);
            col += 2;

            AddDetailItem(grid, "Giorni intermedi:", $"{result.GiorniIntermedi}", row, col);
            col += 2;

            var oreIntermedieTesto = result.GiorniIntermedi > 0 
                ? $"{result.OreGiorniIntermediFormatted} ({result.GiorniIntermedi} giorni)" 
                : "00:00:00";
            AddDetailItem(grid, "Ore intermedie:", oreIntermedieTesto, row, col);

            // Seconda riga: Conteggi giorni
            row = 1;
            col = 0;

            AddDetailItem(grid, "Giorni totali:", $"{result.TotaleGiorniPeriodo}", row, col);
            col += 2;

            AddDetailItem(grid, "Giorni lavorativi:", $"{result.TotaleGiorniConsiderati}", row, col);
            col += 2;

            AddDetailItem(grid, "Non lavorativi:", $"{result.GiorniNonLavorativiEsclusi}", row, col);
            col += 2;

            AddDetailItem(grid, "Festività:", $"{result.FestivitaEscluse}", row, col);

            groupBox.Content = grid;
            return groupBox;
        }

        private void AddDetailItem(Grid grid, string label, string value, int row, int col)
        {
            var labelBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 2, 5, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(labelBlock, row);
            Grid.SetColumn(labelBlock, col);

            var valueBlock = new TextBlock
            {
                Text = value,
                Margin = new Thickness(5, 2, 5, 2),
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(valueBlock, row);
            Grid.SetColumn(valueBlock, col + 1);

            grid.Children.Add(labelBlock);
            grid.Children.Add(valueBlock);
        }

        private GroupBox CreateTotalPanel(string totalTime, string colorHex)
        {
            var groupBox = new GroupBox
            {
                Header = "Totale Ore Lavorative",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                FontWeight = FontWeights.Bold
            };

            var textBlock = new TextBlock
            {
                Text = totalTime,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Consolas")
            };

            groupBox.Content = textBlock;
            return groupBox;
        }

        private GroupBox CreateBreakdownPanel(string detailText)
        {
            var groupBox = new GroupBox
            {
                Header = "Dettaglio Completo",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                FontWeight = FontWeights.Bold
            };

            var textBlock = new TextBlock
            {
                Text = detailText,
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Normal,
                TextWrapping = TextWrapping.Wrap,
                Background = new SolidColorBrush(Colors.LightGray),
                Padding = new Thickness(10),
                Margin = new Thickness(5)
            };

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 150
            };
            scrollViewer.Content = textBlock;

            groupBox.Content = scrollViewer;
            return groupBox;
        }

        private GroupBox CreateExclusionsPanel(SlaCalculationResult result)
        {
            var groupBox = new GroupBox
            {
                Header = "Giorni Esclusi dal Calcolo",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                FontWeight = FontWeights.Bold
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.RowDefinitions.Add(new RowDefinition());

            var col = 0;

            // Totale giorni nel periodo
            AddDetailItem(grid, "Totale giorni:", $"{result.TotaleGiorniPeriodo}", 0, col);
            col += 2;

            // Giorni non lavorativi esclusi (sabato/domenica o giorni configurati come non lavorativi)
            var nonLavorativiText = result.GiorniNonLavorativiEsclusi > 0 
                ? $"{result.GiorniNonLavorativiEsclusi}" 
                : "0";
            AddDetailItem(grid, "Non lavorativi:", nonLavorativiText, 0, col);
            col += 2;

            // Festività escluse
            var festivitaText = result.FestivitaEscluse > 0 
                ? $"{result.FestivitaEscluse}" 
                : "0";
            AddDetailItem(grid, "Festività:", festivitaText, 0, col);

            groupBox.Content = grid;
            return groupBox;
        }

        private void CreateSlaComparisonPanel()
        {
            SlaComparisonPanel.Children.Clear();

            if (!_detailInfo.HasSlaComparison)
            {
                var noSlaPanel = CreateNoDataPanel("Confronto SLA", 
                    "Nessun setup SLA configurato per questo cliente o priorità non riconosciuta");
                SlaComparisonPanel.Children.Add(noSlaPanel);
                return;
            }

            // Info setup SLA
            var setupInfoPanel = CreateSlaSetupInfoPanel();
            SlaComparisonPanel.Children.Add(setupInfoPanel);

            // Confronto TMC
            if (_detailInfo.TmcComparison != null)
            {
                var tmcComparisonPanel = CreateComparisonPanel(_detailInfo.TmcComparison, "#FF4CAF50");
                SlaComparisonPanel.Children.Add(tmcComparisonPanel);
            }

            // Confronto T-EFF
            if (_detailInfo.TeffComparison != null)
            {
                var teffComparisonPanel = CreateComparisonPanel(_detailInfo.TeffComparison, "#FF2196F3");
                SlaComparisonPanel.Children.Add(teffComparisonPanel);
            }
        }

        private GroupBox CreateSlaSetupInfoPanel()
        {
            var groupBox = new GroupBox
            {
                Header = "Configurazione SLA",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                FontWeight = FontWeights.Bold
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            var prioritaLabel = new TextBlock
            {
                Text = "Priorità:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 2, 5, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(prioritaLabel, 0);
            Grid.SetColumn(prioritaLabel, 0);

            var prioritaValue = new TextBlock
            {
                Text = _detailInfo.Record.Priorita,
                Margin = new Thickness(5, 2, 5, 2),
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(prioritaValue, 0);
            Grid.SetColumn(prioritaValue, 1);

            var tmcSlaLabel = new TextBlock
            {
                Text = "SLA Presa in Carico:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 2, 5, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(tmcSlaLabel, 1);
            Grid.SetColumn(tmcSlaLabel, 0);

            var tmcSlaValue = new TextBlock
            {
                Text = _detailInfo.SlaRegola?.TempoPresaInCaricoFormatted ?? "N/D",
                Margin = new Thickness(5, 2, 5, 2),
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(tmcSlaValue, 1);
            Grid.SetColumn(tmcSlaValue, 1);

            var teffSlaLabel = new TextBlock
            {
                Text = "SLA Risoluzione:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 2, 5, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(teffSlaLabel, 2);
            Grid.SetColumn(teffSlaLabel, 0);

            var teffSlaValue = new TextBlock
            {
                Text = _detailInfo.SlaRegola?.TempoRisoluzioneFormatted ?? "N/D",
                Margin = new Thickness(5, 2, 5, 2),
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(teffSlaValue, 2);
            Grid.SetColumn(teffSlaValue, 1);

            grid.Children.Add(prioritaLabel);
            grid.Children.Add(prioritaValue);
            grid.Children.Add(tmcSlaLabel);
            grid.Children.Add(tmcSlaValue);
            grid.Children.Add(teffSlaLabel);
            grid.Children.Add(teffSlaValue);

            groupBox.Content = grid;
            return groupBox;
        }

        private GroupBox CreateComparisonPanel(SlaComparisonResult comparison, string colorHex)
        {
            var groupBox = new GroupBox
            {
                Header = $"Confronto {comparison.TipoSla}",
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                FontWeight = FontWeights.Bold
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Riga 1: Tempo Effettivo vs SLA
            var comparisonGrid = new Grid();
            comparisonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            comparisonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            comparisonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            comparisonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            var effettivoLabel = new TextBlock
            {
                Text = "Tempo Effettivo:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 2, 5, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(effettivoLabel, 0);

            var effettivoValue = new TextBlock
            {
                Text = comparison.TempoEffettivoFormatted,
                Margin = new Thickness(5, 2, 5, 2),
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(effettivoValue, 1);

            var slaLabel = new TextBlock
            {
                Text = "Limite SLA:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(15, 2, 5, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(slaLabel, 2);

            var slaValue = new TextBlock
            {
                Text = comparison.TempoSlaFormatted,
                Margin = new Thickness(5, 2, 5, 2),
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(slaValue, 3);

            comparisonGrid.Children.Add(effettivoLabel);
            comparisonGrid.Children.Add(effettivoValue);
            comparisonGrid.Children.Add(slaLabel);
            comparisonGrid.Children.Add(slaValue);

            Grid.SetRow(comparisonGrid, 0);

            // Riga 2: Status
            var statusTextBlock = new TextBlock
            {
                Text = comparison.StatusMessage,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 10, 5, 5),
                FontFamily = new FontFamily("Consolas")
            };

            // Colore basato su stato SLA
            if (comparison.EntroSla)
            {
                statusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                statusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }

            Grid.SetRow(statusTextBlock, 1);

            mainGrid.Children.Add(comparisonGrid);
            mainGrid.Children.Add(statusTextBlock);

            groupBox.Content = mainGrid;
            return groupBox;
        }

        private string FormatDateTime(DateTime? dateTime)
        {
            return dateTime?.ToString("dd/MM/yyyy HH:mm") ?? "Non disponibile";
        }

        private void ChiudiButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}