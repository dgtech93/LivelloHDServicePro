using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LivelloHDServicePRO.Models;
using LivelloHDServicePRO.Services;
using Microsoft.Win32;

namespace LivelloHDServicePRO.Views
{
    /// <summary>
    /// Finestra per l'analisi dettagliata delle risorse/proprietari
    /// </summary>
    public partial class AnalisiRisorseWindow : Window
    {
        private readonly List<AnalisiDettagliataRisorsa> _risorse;

        public AnalisiRisorseWindow(List<AnalisiDettagliataRisorsa> risorse)
        {
            InitializeComponent();
            _risorse = risorse ?? new List<AnalisiDettagliataRisorsa>();
            
            CaricaRisorse();
        }

        private void CaricaRisorse()
        {
            RisorseCountText.Text = $"Risorse analizzate: {_risorse.Count}";
            RisorseListBox.ItemsSource = _risorse;
            
            if (_risorse.Any())
            {
                RisorseListBox.SelectedIndex = 0;
            }
        }

        private void RisorseListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RisorseListBox.SelectedItem is AnalisiDettagliataRisorsa risorsa)
            {
                MostraDettaglio(risorsa);
            }
        }

        private void MostraDettaglio(AnalisiDettagliataRisorsa risorsa)
        {
            // Statistiche principali
            VotazioneText.Text = risorsa.DescrizioneValutazione;
            VotazioneText.Foreground = GetColoreValutazione(risorsa.ValutazioneComplessiva);
            
            TicketTotaliText.Text = risorsa.TicketTotali.ToString();
            PercentualeRisoluzioneText.Text = $"{risorsa.PercentualeRisoluzione:F1}%";
            
            // Punteggi dettagliati per categoria
            PunteggioTMCText.Text = $"{risorsa.PunteggioTMC:F1}/20";
            PunteggioTEFFText.Text = $"{risorsa.PunteggioTEFF:F1}/20";
            PunteggioTempiMediText.Text = $"{risorsa.PunteggioTempiMedi:F1}/20";
            PunteggioRisoluzioneText.Text = $"{risorsa.PunteggioRisoluzione:F1}/20";
            PunteggioVolumeText.Text = $"{risorsa.PunteggioVolume:F1}/20";
            PunteggioTotaleRiepilogoText.Text = $"{risorsa.PunteggioTotale:F1}/100";
            
            // Sintesi analitica
            SintesiText.Text = !string.IsNullOrEmpty(risorsa.SintesiAnalitica) 
                ? risorsa.SintesiAnalitica 
                : "Nessuna sintesi disponibile.";
            
            // Tempi medi
            TmcText.Text = risorsa.TempoMedioTMCFormatted;
            TeffText.Text = risorsa.TempoMedioTEFFFormatted;
            
            // Deviazioni dalla media
            DeviazioneTmcText.Text = FormatDeviazione(risorsa.DeviazioneDallaMediaTMC);
            DeviazioneTmcText.Foreground = risorsa.DeviazioneDallaMediaTMC < 0 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
            
            DeviazioneTeffText.Text = FormatDeviazione(risorsa.DeviazioneDallaMediaTEFF);
            DeviazioneTeffText.Foreground = risorsa.DeviazioneDallaMediaTEFF < 0 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
            
            // Performance SLA
            EntroSlaText.Text = $"{risorsa.TicketsEntroSLA} ({risorsa.PercentualeEntroSLA:F1}%)";
            FuoriSlaText.Text = $"{risorsa.TicketsFuoriSLA} ({risorsa.PercentualeFuoriSLA:F1}%)";
            PosizioneText.Text = risorsa.PosizioneRelativa;
            PosizioneText.Foreground = risorsa.PosizioneRelativa.Contains("Top") 
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"))
                : risorsa.PosizioneRelativa.Contains("Bottom")
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
            
            // Punti di forza
            if (risorsa.PuntiDiForza.Any())
            {
                PuntiDiForzaList.ItemsSource = risorsa.PuntiDiForza;
                PuntiDiForzaList.Visibility = Visibility.Visible;
                NoPuntiDiForzaText.Visibility = Visibility.Collapsed;
            }
            else
            {
                PuntiDiForzaList.ItemsSource = null;
                PuntiDiForzaList.Visibility = Visibility.Collapsed;
                NoPuntiDiForzaText.Visibility = Visibility.Visible;
            }
            
            // Aree di miglioramento
            if (risorsa.AreeDiMiglioramento.Any())
            {
                AreeDiMiglioramentoList.ItemsSource = risorsa.AreeDiMiglioramento;
                AreeDiMiglioramentoList.Visibility = Visibility.Visible;
                NoAreeMiglioramentoText.Visibility = Visibility.Collapsed;
            }
            else
            {
                AreeDiMiglioramentoList.ItemsSource = null;
                AreeDiMiglioramentoList.Visibility = Visibility.Collapsed;
                NoAreeMiglioramentoText.Visibility = Visibility.Visible;
            }
            
            // Suggerimenti per azioni
            if (risorsa.SuggerimentiAzioni.Any())
            {
                SuggerimentiAzioniList.ItemsSource = risorsa.SuggerimentiAzioni;
                SuggerimentiAzioniList.Visibility = Visibility.Visible;
                NoSuggerimentiText.Visibility = Visibility.Collapsed;
            }
            else
            {
                SuggerimentiAzioniList.ItemsSource = null;
                SuggerimentiAzioniList.Visibility = Visibility.Collapsed;
                NoSuggerimentiText.Visibility = Visibility.Visible;
            }
            
            // Distribuzione priorità
            PrioritaDataGrid.ItemsSource = risorsa.TicketsPerPriorita.ToList();
            
            // Ticket Fuori SLA
            if (risorsa.ListaTicketFuoriSLA.Any())
            {
                TicketFuoriSlaCount.Text = $"Totale: {risorsa.ListaTicketFuoriSLA.Count} ticket fuori SLA";
                TicketFuoriSlaDataGrid.ItemsSource = risorsa.ListaTicketFuoriSLA;
            }
            else
            {
                TicketFuoriSlaCount.Text = "Nessun ticket fuori SLA";
                TicketFuoriSlaDataGrid.ItemsSource = null;
            }
            
            // Tendenze mensili
            TendenzeDataGrid.ItemsSource = risorsa.TendenzeMensili;
            
            // Status bar
            StatusText.Text = $"Visualizzando dettaglio per: {risorsa.NomeProprietario}";
        }

        private SolidColorBrush GetColoreValutazione(ValutazioneQualita valutazione)
        {
            return valutazione switch
            {
                ValutazioneQualita.Ottimo => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")),
                ValutazioneQualita.Discreto => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")),
                ValutazioneQualita.Migliorabile => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5722")),
                ValutazioneQualita.Critico => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9E9E9E"))
            };
        }

        private string FormatDeviazione(double deviazione)
        {
            var segno = deviazione >= 0 ? "+" : "";
            return $"{segno}{deviazione:F1}% dalla media";
        }

        private void ChiudiButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void EsportaPdfButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RisorseListBox.SelectedItem is not AnalisiDettagliataRisorsa risorsaSelezionata)
                {
                    MessageBox.Show("Seleziona una risorsa per esportare l'analisi in PDF.", 
                                    "Nessuna risorsa selezionata", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Information);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Analisi_{risorsaSelezionata.NomeProprietario.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    Title = "Salva analisi risorsa in PDF"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var pdfService = new PdfExportService();
                    pdfService.EsportaAnalisiRisorsaPdf(risorsaSelezionata, saveFileDialog.FileName);
                    
                    var result = MessageBox.Show(
                        "PDF generato con successo!\n\nVuoi aprire il file?",
                        "Esportazione completata",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }

                    StatusText.Text = $"PDF esportato: {System.IO.Path.GetFileName(saveFileDialog.FileName)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'esportazione del PDF:\n{ex.Message}", 
                                "Errore", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                StatusText.Text = "Errore durante l'esportazione PDF";
            }
        }
    }
}
