using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LivelloHDServicePRO.Models;
using LivelloHDServicePRO.Services;

namespace LivelloHDServicePRO.Views
{
    public partial class FestivitaWindow : Window
    {
        private readonly FestivitaService _festivitaService;
        private List<Festivita> _festivita;
        private Festivita? _festivitaInModifica;
        private bool _isModifying = false;

        public FestivitaWindow()
        {
            InitializeComponent();
            
            _festivitaService = new FestivitaService();
            _festivita = new List<Festivita>();

            InitializeComponents();
            CaricaFestivita();
        }

        private void InitializeComponents()
        {
            // Popola i mesi
            var mesi = _festivitaService.GetMesiDisponibili();
            foreach (var mese in mesi)
            {
                MeseComboBox.Items.Add(mese);
            }
            
            // Imposta mese default (gennaio)
            if (MeseComboBox.Items.Count > 0)
            {
                MeseComboBox.SelectedIndex = 0;
            }
        }

        private void CaricaFestivita()
        {
            try
            {
                _festivita = _festivitaService.CaricaFestivita();
                AggiornaListaFestivita();
                AggiornaContatore();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento delle festivita: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AggiornaListaFestivita()
        {
            FestivitaListBox.ItemsSource = null;
            FestivitaListBox.ItemsSource = _festivita.OrderBy(f => f.Mese).ThenBy(f => f.Giorno);
        }

        private void AggiornaContatore()
        {
            ContatoreFestivitaTextBlock.Text = _festivita.Count.ToString();
        }

        private void MeseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MeseComboBox.SelectedItem != null)
            {
                var meseString = MeseComboBox.SelectedItem.ToString()!;
                var numeroMese = int.Parse(meseString.Split(' ')[0]);
                
                // Popola i giorni in base al mese selezionato
                var giorni = _festivitaService.GetGiorniDisponibili(numeroMese);
                
                GiornoComboBox.Items.Clear();
                foreach (var giorno in giorni)
                {
                    GiornoComboBox.Items.Add(giorno);
                }
                
                // Seleziona il primo giorno
                if (GiornoComboBox.Items.Count > 0)
                {
                    GiornoComboBox.SelectedIndex = 0;
                }
            }
        }

        private void GiornoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Validate date when day changes
            ValidateCurrentInput();
        }

        private void ValidateCurrentInput()
        {
            var canAdd = !string.IsNullOrWhiteSpace(NomeFestivitaTextBox.Text) &&
                        GiornoComboBox.SelectedItem != null &&
                        MeseComboBox.SelectedItem != null;

            AggiungiButton.IsEnabled = canAdd && !_isModifying;
            ModificaButton.IsEnabled = canAdd && _isModifying;
        }

        private void AggiungiButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput(out int giorno, out int mese, out string nome))
                    return;

                // Check if holiday already exists
                if (_festivitaService.EsisteFestivita(_festivita, giorno, mese))
                {
                    MessageBox.Show($"Esiste gia una festivita il {giorno}/{mese}.", 
                                  "Festivita Esistente", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nuovaFestivita = new Festivita
                {
                    Giorno = giorno,
                    Mese = mese,
                    Nome = nome
                };

                _festivita.Add(nuovaFestivita);
                AggiornaListaFestivita();
                AggiornaContatore();
                PulisciCampiInput();

                MessageBox.Show($"Festivita '{nome}' aggiunta con successo!", 
                              "Aggiunta Completata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'aggiunta: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ModificaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_festivitaInModifica == null || !ValidateInput(out int giorno, out int mese, out string nome))
                    return;

                // Check if another holiday exists for this date (excluding current one)
                var esisteAltra = _festivita.Any(f => f != _festivitaInModifica && 
                                                     f.Giorno == giorno && f.Mese == mese);
                if (esisteAltra)
                {
                    MessageBox.Show($"Esiste gia un'altra festivita il {giorno}/{mese}.", 
                                  "Festivita Esistente", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _festivitaInModifica.Giorno = giorno;
                _festivitaInModifica.Mese = mese;
                _festivitaInModifica.Nome = nome;

                AggiornaListaFestivita();
                AnnullaModifica();

                MessageBox.Show($"Festivita '{nome}' modificata con successo!", 
                              "Modifica Completata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la modifica: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AnnullaButton_Click(object sender, RoutedEventArgs e)
        {
            AnnullaModifica();
        }

        private void FestivitaListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EliminaButton.IsEnabled = FestivitaListBox.SelectedItem != null;
            
            if (FestivitaListBox.SelectedItem is Festivita festivitaSelezionata && !_isModifying)
            {
                // Populate fields for potential editing
                NomeFestivitaTextBox.Text = festivitaSelezionata.Nome;
                
                // Set month
                var meseIndex = festivitaSelezionata.Mese - 1;
                if (meseIndex >= 0 && meseIndex < MeseComboBox.Items.Count)
                {
                    MeseComboBox.SelectedIndex = meseIndex;
                }
                
                // Set day (after month is set to populate days)
                for (int i = 0; i < GiornoComboBox.Items.Count; i++)
                {
                    if ((int)GiornoComboBox.Items[i] == festivitaSelezionata.Giorno)
                    {
                        GiornoComboBox.SelectedIndex = i;
                        break;
                    }
                }

                // Enable modify mode
                _festivitaInModifica = festivitaSelezionata;
                _isModifying = true;
                AggiungiButton.IsEnabled = false;
                ModificaButton.IsEnabled = true;
                AnnullaButton.IsEnabled = true;
                ValidateCurrentInput();
            }
        }

        private void EliminaButton_Click(object sender, RoutedEventArgs e)
        {
            if (FestivitaListBox.SelectedItem is Festivita festivitaSelezionata)
            {
                var result = MessageBox.Show($"Sei sicuro di voler eliminare la festivita '{festivitaSelezionata.DisplayText}'?", 
                                           "Conferma Eliminazione", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _festivita.Remove(festivitaSelezionata);
                    AggiornaListaFestivita();
                    AggiornaContatore();
                    AnnullaModifica();

                    MessageBox.Show("Festivita eliminata con successo!", 
                                  "Eliminazione Completata", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void RipristinaDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Sei sicuro di voler ripristinare le festivita nazionali italiane default?\n" +
                                        "Questa operazione sostituira tutte le festivita attuali.", 
                                        "Conferma Ripristino", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _festivita.Clear();
                
                // Create default holidays
                var defaultService = new FestivitaService();
                var defaultFestivita = new List<Festivita>
                {
                    new() { Giorno = 1, Mese = 1, Nome = "Capodanno" },
                    new() { Giorno = 6, Mese = 1, Nome = "Epifania" },
                    new() { Giorno = 25, Mese = 4, Nome = "Festa della Liberazione" },
                    new() { Giorno = 1, Mese = 5, Nome = "Festa del Lavoro" },
                    new() { Giorno = 2, Mese = 6, Nome = "Festa della Repubblica" },
                    new() { Giorno = 15, Mese = 8, Nome = "Ferragosto" },
                    new() { Giorno = 1, Mese = 11, Nome = "Ognissanti" },
                    new() { Giorno = 8, Mese = 12, Nome = "Immacolata Concezione" },
                    new() { Giorno = 25, Mese = 12, Nome = "Natale" },
                    new() { Giorno = 26, Mese = 12, Nome = "Santo Stefano" }
                };

                _festivita.AddRange(defaultFestivita);
                AggiornaListaFestivita();
                AggiornaContatore();
                AnnullaModifica();

                MessageBox.Show("Festivita nazionali italiane ripristinate con successo!", 
                              "Ripristino Completato", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SalvaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _festivitaService.SalvaFestivita(_festivita);
                MessageBox.Show($"Configurazione salvata con successo!\nFestivita totali: {_festivita.Count}", 
                              "Salvataggio Completato", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput(out int giorno, out int mese, out string nome)
        {
            giorno = 0;
            mese = 0;
            nome = string.Empty;

            if (string.IsNullOrWhiteSpace(NomeFestivitaTextBox.Text))
            {
                MessageBox.Show("Inserisci il nome della festivita.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (GiornoComboBox.SelectedItem == null)
            {
                MessageBox.Show("Seleziona il giorno.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (MeseComboBox.SelectedItem == null)
            {
                MessageBox.Show("Seleziona il mese.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            giorno = (int)GiornoComboBox.SelectedItem;
            var meseString = MeseComboBox.SelectedItem.ToString()!;
            mese = int.Parse(meseString.Split(' ')[0]);
            nome = NomeFestivitaTextBox.Text.Trim();

            // Validate date
            try
            {
                var testDate = new DateTime(DateTime.Now.Year, mese, giorno);
            }
            catch
            {
                MessageBox.Show("La data inserita non e valida.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void PulisciCampiInput()
        {
            NomeFestivitaTextBox.Clear();
            if (MeseComboBox.Items.Count > 0)
                MeseComboBox.SelectedIndex = 0;
        }

        private void AnnullaModifica()
        {
            _festivitaInModifica = null;
            _isModifying = false;
            AggiungiButton.IsEnabled = true;
            ModificaButton.IsEnabled = false;
            AnnullaButton.IsEnabled = false;
            PulisciCampiInput();
            FestivitaListBox.SelectedItem = null;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void NomeFestivitaTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateCurrentInput();
        }
    }
}