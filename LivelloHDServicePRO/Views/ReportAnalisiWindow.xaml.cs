using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using LivelloHDServicePRO.Models;
using LivelloHDServicePRO.Services;

namespace LivelloHDServicePRO.Views
{
    public partial class ReportAnalisiWindow : Window
    {
        private ReportData _reportData;
        private readonly ExcelExportService _excelExportService;
        private readonly PdfExportService _pdfExportService; // ?? Servizio PDF

        public ReportAnalisiWindow(ReportData reportData)
        {
            InitializeComponent();
            _reportData = reportData;
            _excelExportService = new ExcelExportService();
            _pdfExportService = new PdfExportService(); // ?? Inizializza servizio PDF
            
            CaricaDati();
        }

        private void CaricaDati()
        {
            try
            {
                // Aggiorna header
                PeriodoTextBlock.Text = $"Periodo di analisi: {_reportData.PeriodoAnalisi}";
                GeneratoTextBlock.Text = $"Generato il: {_reportData.DataGenerazione:dd/MM/yyyy HH:mm}";

                // Statistiche generali
                TotalTicketsText.Text = _reportData.TotalTickets.ToString();
                TicketsRisoltiText.Text = _reportData.TicketsRisolti.ToString();
                PercentualeEntroSlaText.Text = $"{_reportData.RiepilogoGenerale.PercentualeComplessivaEntroSLA:F1}%";
                PercentualeFuoriSlaText.Text = $"{_reportData.RiepilogoGenerale.PercentualeComplessivaFuoriSLA:F1}%";

                // Tempi medi per priorità
                TempiMediDataGrid.ItemsSource = _reportData.TempiMediPerPriorita;

                // Performance SLA
                SlaPerformanceDataGrid.ItemsSource = _reportData.SlaPerformanceData;

                // Analisi proprietari
                ProprietariDataGrid.ItemsSource = _reportData.AnalisiProprietari;

                // ?? Analisi proprietari per priorità
                ProprietariPrioritaDataGrid.ItemsSource = _reportData.AnalisiProprietariPerPriorita;

                // Distribuzione priorità
                DistribuzionePrioritaDataGrid.ItemsSource = _reportData.DistribuzionePriorita;

                // Top proprietari (primi 10)
                TopProprietariDataGrid.ItemsSource = _reportData.DistribuzioneProprietario.Take(10);

                // Riepilogo esecutivo
                TempoMedioGlobaleTMCText.Text = _reportData.RiepilogoGenerale.TempoMedioGlobaleTMCFormatted;
                TempoMedioGlobaleTEFFText.Text = _reportData.RiepilogoGenerale.TempoMedioGlobaleTEFFFormatted;
                
                // MIGLIORI (metriche oggettive)
                ProprietarioMiglioreText.Text = $"• TMC più basso: {_reportData.RiepilogoGenerale.MiglioreMediaTMC} ({_reportData.RiepilogoGenerale.MiglioreMediaTMCValoreFormatted})\n" +
                                               $"• T-EFF più basso: {_reportData.RiepilogoGenerale.MiglioreMediaTEFF} ({_reportData.RiepilogoGenerale.MiglioreMediaTEFFValoreFormatted})\n" +
                                               $"• Volume più alto: {_reportData.RiepilogoGenerale.MaggioreVolumeTK} ({_reportData.RiepilogoGenerale.MaggioreVolumeTKValore} TK)";
                
                // PEGGIORI (metriche oggettive)
                ProprietarioCriticoText.Text = $"• TMC più alto: {_reportData.RiepilogoGenerale.PeggioreMediaTMC} ({_reportData.RiepilogoGenerale.PeggioreMediaTMCValoreFormatted})\n" +
                                              $"• T-EFF più alto: {_reportData.RiepilogoGenerale.PeggioreMediaTEFF} ({_reportData.RiepilogoGenerale.PeggioreMediaTEFFValoreFormatted})\n" +
                                              $"• Volume più basso: {_reportData.RiepilogoGenerale.MinoreVolumeTK} ({_reportData.RiepilogoGenerale.MinoreVolumeTKValore} TK)";
                
                PrioritaProblematicaText.Text = _reportData.RiepilogoGenerale.PrioritaPiuProblematica;
                PrioritaMenoProblematicaText.Text = _reportData.RiepilogoGenerale.PrioritaMenoProblematica;

                StatusTextBlock.Text = $"Report caricato: {_reportData.TotalTickets} ticket analizzati";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nel caricamento dei dati del report: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EsportaReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Esporta Report di Analisi",
                    DefaultExt = ".xlsx",
                    Filter = "File Excel (*.xlsx)|*.xlsx|File CSV (*.csv)|*.csv",
                    FileName = $"Report_Analisi_{DateTime.Now:yyyyMMdd_HHmm}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    StatusTextBlock.Text = "Esportazione report in corso...";
                    EsportaReportButton.IsEnabled = false;

                    if (Path.GetExtension(saveFileDialog.FileName).ToLower() == ".xlsx")
                    {
                        EsportaExcel(saveFileDialog.FileName);
                    }
                    else
                    {
                        EsportaCsv(saveFileDialog.FileName);
                    }

                    MessageBox.Show($"Report esportato con successo!\n\nFile salvato in:\n{saveFileDialog.FileName}", 
                                  "Esportazione Completata", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    StatusTextBlock.Text = "Report esportato con successo";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'esportazione: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Errore durante l'esportazione";
            }
            finally
            {
                EsportaReportButton.IsEnabled = true;
            }
        }

        private void EsportaPdfButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Esporta Report di Analisi in PDF",
                    DefaultExt = ".pdf",
                    Filter = "File PDF (*.pdf)|*.pdf",
                    FileName = $"Report_Analisi_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var originalFilePath = saveFileDialog.FileName;
                    var finalFilePath = GetAvailableFileName(originalFilePath);

                    if (finalFilePath != originalFilePath)
                    {
                        var result = MessageBox.Show($"Il file '{Path.GetFileName(originalFilePath)}' è già in uso.\n\n" +
                                                   $"Vuoi salvare come '{Path.GetFileName(finalFilePath)}' invece?",
                                                   "File in uso", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.No)
                        {
                            MessageBox.Show("Chiudi il file PDF esistente e riprova, oppure scegli un nome diverso.", 
                                          "Salvataggio annullato", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }

                    StatusTextBlock.Text = "Generazione PDF in corso...";
                    EsportaPdfButton.IsEnabled = false;
                    EsportaReportButton.IsEnabled = false;

                    // Genera PDF in background per evitare blocchi UI
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            _pdfExportService.EsportaReportPdf(_reportData, finalFilePath);
                            
                            // Torna al thread UI per aggiornare l'interfaccia
                            Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show($"Report PDF esportato con successo!\n\nFile salvato come:\n{Path.GetFileName(finalFilePath)}\n\nPercorso completo:\n{finalFilePath}", 
                                              "Esportazione PDF Completata", MessageBoxButton.OK, MessageBoxImage.Information);
                                
                                StatusTextBlock.Text = "Report PDF esportato con successo";
                                
                                // Chiedi se aprire il file
                                var result = MessageBox.Show("Vuoi aprire il file PDF generato?", 
                                                            "Apri PDF", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (result == MessageBoxResult.Yes)
                                {
                                    try
                                    {
                                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                                        {
                                            FileName = finalFilePath,
                                            UseShellExecute = true
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show($"Impossibile aprire il PDF automaticamente: {ex.Message}\n\nPercorso file: {finalFilePath}", 
                                                      "Avviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                System.Diagnostics.Debug.WriteLine($"? Dettaglio errore PDF: {ex}");
                                
                                var errorMessage = ex.Message;
                                if (ex.InnerException != null)
                                {
                                    errorMessage += $"\n\nDettagli: {ex.InnerException.Message}";
                                }
                                
                                // Suggerimenti per errori comuni
                                var suggestions = GetErrorSuggestions(ex);
                                
                                MessageBox.Show($"Errore durante la generazione del PDF:\n\n{errorMessage}{suggestions}", 
                                              "Errore PDF", MessageBoxButton.OK, MessageBoxImage.Error);
                                StatusTextBlock.Text = "Errore durante la generazione PDF";
                            });
                        }
                        finally
                        {
                            Dispatcher.Invoke(() =>
                            {
                                EsportaPdfButton.IsEnabled = true;
                                EsportaReportButton.IsEnabled = true;
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'esportazione PDF: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Errore durante l'esportazione PDF";
                EsportaPdfButton.IsEnabled = true;
                EsportaReportButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Trova un nome file disponibile se quello richiesto è in uso
        /// </summary>
        private string GetAvailableFileName(string originalPath)
        {
            if (!IsFileInUse(originalPath))
                return originalPath;

            var directory = Path.GetDirectoryName(originalPath);
            var fileName = Path.GetFileNameWithoutExtension(originalPath);
            var extension = Path.GetExtension(originalPath);

            for (int i = 1; i <= 99; i++)
            {
                var newPath = Path.Combine(directory, $"{fileName}_{i:D2}{extension}");
                if (!IsFileInUse(newPath))
                    return newPath;
            }

            // Se tutti i tentativi falliscono, usa timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return Path.Combine(directory, $"{fileName}_{timestamp}{extension}");
        }

        /// <summary>
        /// Controlla se il file è in uso da un altro processo
        /// </summary>
        private bool IsFileInUse(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                using var fs = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Fornisce suggerimenti basati sul tipo di errore
        /// </summary>
        private string GetErrorSuggestions(Exception ex)
        {
            var message = ex.Message.ToLower();
            
            if (message.Contains("being used") || message.Contains("in uso"))
            {
                return "\n\n?? Suggerimenti:\n" +
                       "• Chiudi tutti i PDF viewer aperti (Adobe Reader, Browser, etc.)\n" +
                       "• Chiudi il file se è aperto in un'altra applicazione\n" +
                       "• Prova a salvare con un nome diverso\n" +
                       "• Riavvia l'applicazione se il problema persiste";
            }
            else if (message.Contains("access") || message.Contains("unauthorized"))
            {
                return "\n\n?? Suggerimenti:\n" +
                       "• Verifica i permessi sulla cartella di destinazione\n" +
                       "• Prova a salvare in un'altra cartella (es. Desktop)\n" +
                       "• Esegui l'applicazione come amministratore se necessario";
            }
            else if (message.Contains("pdf") || message.Contains("itext"))
            {
                return "\n\n?? Suggerimenti:\n" +
                       "• I dati potrebbero contenere caratteri speciali problematici\n" +
                       "• Verifica che tutti i campi del report siano compilati correttamente\n" +
                       "• Prova a generare il report con meno dati per test";
            }
            else
            {
                return "\n\n?? Suggerimenti generali:\n" +
                       "• Riprova tra qualche secondo\n" +
                       "• Verifica lo spazio disponibile sul disco\n" +
                       "• Contatta il supporto se il problema persiste";
            }
        }

        private void EsportaExcel(string fileName)
        {
            // Implementazione export Excel (semplificata)
            var reportSummary = new List<Dictionary<string, object>>
            {
                new() {
                    {"Metrica", "Ticket Totali"}, {"Valore", _reportData.TotalTickets}
                },
                new() {
                    {"Metrica", "Ticket Risolti"}, {"Valore", _reportData.TicketsRisolti}
                },
                new() {
                    {"Metrica", "% Entro SLA"}, {"Valore", $"{_reportData.RiepilogoGenerale.PercentualeComplessivaEntroSLA:F1}%"}
                },
                new() {
                    {"Metrica", "% Fuori SLA"}, {"Valore", $"{_reportData.RiepilogoGenerale.PercentualeComplessivaFuoriSLA:F1}%"}
                },
                new() {
                    {"Metrica", "Tempo Medio TMC Globale"}, {"Valore", _reportData.RiepilogoGenerale.TempoMedioGlobaleTMCFormatted}
                },
                new() {
                    {"Metrica", "Tempo Medio T-EFF Globale"}, {"Valore", _reportData.RiepilogoGenerale.TempoMedioGlobaleTEFFFormatted}
                },
                new() {
                    {"Metrica", "Migliore Media TMC"}, {"Valore", $"{_reportData.RiepilogoGenerale.MiglioreMediaTMC} ({_reportData.RiepilogoGenerale.MiglioreMediaTMCValoreFormatted})"}
                },
                new() {
                    {"Metrica", "Migliore Media T-EFF"}, {"Valore", $"{_reportData.RiepilogoGenerale.MiglioreMediaTEFF} ({_reportData.RiepilogoGenerale.MiglioreMediaTEFFValoreFormatted})"}
                },
                new() {
                    {"Metrica", "Maggior Volume TK"}, {"Valore", $"{_reportData.RiepilogoGenerale.MaggioreVolumeTK} ({_reportData.RiepilogoGenerale.MaggioreVolumeTKValore})"}
                },
                new() {
                    {"Metrica", "Peggiore Media TMC"}, {"Valore", $"{_reportData.RiepilogoGenerale.PeggioreMediaTMC} ({_reportData.RiepilogoGenerale.PeggioreMediaTMCValoreFormatted})"}
                },
                new() {
                    {"Metrica", "Peggiore Media T-EFF"}, {"Valore", $"{_reportData.RiepilogoGenerale.PeggioreMediaTEFF} ({_reportData.RiepilogoGenerale.PeggioreMediaTEFFValoreFormatted})"}
                },
                new() {
                    {"Metrica", "Minor Volume TK"}, {"Valore", $"{_reportData.RiepilogoGenerale.MinoreVolumeTK} ({_reportData.RiepilogoGenerale.MinoreVolumeTKValore})"}
                }
            };

            // Per ora salva un CSV strutturato - implementazione Excel completa richiede librerie aggiuntive
            EsportaCsv(fileName.Replace(".xlsx", ".csv"));
        }

        private void EsportaCsv(string fileName)
        {
            using var writer = new StreamWriter(fileName, false, System.Text.Encoding.UTF8);
            
            // Header generale
            writer.WriteLine($"REPORT DI ANALISI - QUALITÀ DEL SERVIZIO");
            writer.WriteLine($"Generato il: {_reportData.DataGenerazione:dd/MM/yyyy HH:mm}");
            writer.WriteLine($"Periodo: {_reportData.PeriodoAnalisi}");
            writer.WriteLine();

            // Statistiche generali
            writer.WriteLine("STATISTICHE GENERALI");
            writer.WriteLine("Metrica,Valore");
            writer.WriteLine($"Ticket Totali,{_reportData.TotalTickets}");
            writer.WriteLine($"Ticket Risolti,{_reportData.TicketsRisolti}");
            writer.WriteLine($"Percentuale Entro SLA,{_reportData.RiepilogoGenerale.PercentualeComplessivaEntroSLA:F1}%");
            writer.WriteLine($"Percentuale Fuori SLA,{_reportData.RiepilogoGenerale.PercentualeComplessivaFuoriSLA:F1}%");
            writer.WriteLine();

            // Tempi medi per priorità
            writer.WriteLine("TEMPI MEDI PER PRIORITÀ");
            writer.WriteLine("Priorità,Numero Tickets,TMC Medio,T-EFF Medio");
            foreach (var tempo in _reportData.TempiMediPerPriorita)
            {
                writer.WriteLine($"{tempo.Priorita},{tempo.NumeroTickets},{tempo.TempoMedioTMCFormatted},{tempo.TempoMedioTEFFFormatted}");
            }
            writer.WriteLine();

            // Analisi proprietari
            writer.WriteLine("ANALISI PROPRIETARI");
            writer.WriteLine("Proprietario,Ticket Totali,Ticket Risolti,% Risoluzione,Valutazione Numerica,Valutazione Descrittiva");
            foreach (var prop in _reportData.AnalisiProprietari)
            {
                writer.WriteLine($"{prop.NomeProprietario},{prop.TotalTickets},{prop.TicketsRisolti},{prop.PercentualeRisoluzione:F1}%,{prop.ValutazioneNumerica}/10,{prop.DescrizioneValutazione}");
            }
            writer.WriteLine();

            // ?? Analisi proprietari per priorità
            writer.WriteLine("ANALISI PROPRIETARI PER PRIORITA");
            writer.WriteLine("Proprietario,Priorita,Numero Tickets,Ticket Risolti,% Risoluzione,TMC Medio,T-EFF Medio,% Fuori SLA,Valutazione Numerica,Valutazione Descrittiva");
            foreach (var prop in _reportData.AnalisiProprietariPerPriorita)
            {
                writer.WriteLine($"{prop.NomeProprietario},{prop.Priorita},{prop.NumeroTickets},{prop.TicketsRisolti},{prop.PercentualeRisoluzione:F1}%,{prop.TempoMedioTMCFormatted},{prop.TempoMedioTEFFFormatted},{prop.PercentualeFuoriSLA:F1}%,{prop.ValutazioneNumerica}/10,{prop.DescrizioneValutazionePriorita}");
            }
            writer.WriteLine();

            // Performance SLA
            writer.WriteLine("PERFORMANCE SLA");
            writer.WriteLine("Categoria,Ticket Totali,% Entro SLA,% Fuori SLA");
            foreach (var sla in _reportData.SlaPerformanceData)
            {
                writer.WriteLine($"{sla.Categoria},{sla.TicketsTotali},{sla.PercentualeEntroSLA:F1}%,{sla.PercentualeFuoriSLA:F1}%");
            }
        }

        private void RisorseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_reportData?.AnalisiDettagliataRisorse == null || !_reportData.AnalisiDettagliataRisorse.Any())
            {
                MessageBox.Show("Nessun dato di analisi dettagliata delle risorse disponibile.\n\n" +
                              "Potrebbero non esserci abbastanza dati per generare l'analisi (minimo 3 ticket per proprietario).", 
                              "Analisi Risorse", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var analisiRisorseWindow = new AnalisiRisorseWindow(_reportData.AnalisiDettagliataRisorse);
                analisiRisorseWindow.Owner = this;
                analisiRisorseWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore nell'apertura della finestra Analisi Risorse:\n{ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StampaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Implementazione stampa - per ora mostra un messaggio
                    MessageBox.Show("Funzionalità di stampa in sviluppo.\n\nPer ora è possibile esportare il report e stamparlo da Excel.", 
                                  "Stampa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la stampa: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
            }
        }
    }
}