using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LivelloHDServicePRO.Models;
using LivelloHDServicePRO.Services;

namespace LivelloHDServicePRO.Views
{
    public partial class OrariLavorativiWindow : Window
    {
        private readonly OrariLavorativiService _orariService;
        private List<OrariLavorativiSetup> _setupEsistenti;
        private OrariLavorativiSetup _setupCorrente;
        private List<string> _orariDisponibili;

        public event EventHandler? ClienteSalvato;

        public OrariLavorativiWindow() : this(null)
        {
        }

        public OrariLavorativiWindow(string? clientePreselezionato)
        {
            InitializeComponent();
            
            _orariService = new OrariLavorativiService();
            _orariDisponibili = _orariService.GetOrariDisponibili();
            _setupEsistenti = _orariService.CaricaOrariLavorativi();
            _setupCorrente = new OrariLavorativiSetup();

            InitializeComponent();
            CaricaClientiEsistenti();
            CreaInterfacciaGiorni();

            // Preseleziona il cliente se fornito
            if (!string.IsNullOrWhiteSpace(clientePreselezionato))
            {
                PreselezionaCliente(clientePreselezionato);
            }
        }

        private void CaricaClientiEsistenti()
        {
            ClienteComboBox.Items.Clear();
            foreach (var setup in _setupEsistenti)
            {
                ClienteComboBox.Items.Add(setup.NomeCliente);
            }
        }

        private void CreaInterfacciaGiorni()
        {
            GiorniStackPanel.Children.Clear();

            foreach (var giorno in _setupCorrente.Giorni)
            {
                var groupBox = CreaGroupBoxPerGiorno(giorno);
                GiorniStackPanel.Children.Add(groupBox);
            }
        }

        private GroupBox CreaGroupBoxPerGiorno(GiornoLavorativo giorno)
        {
            var groupBox = new GroupBox
            {
                Header = giorno.NomeGiorno,
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                FontWeight = FontWeights.Bold
            };

            var mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Checkbox Lavorativo
            var lavorativoCheck = new CheckBox
            {
                Content = "Lavorativo",
                IsChecked = giorno.IsLavorativo,
                Margin = new Thickness(5, 2, 5, 2),
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Normal
            };
            lavorativoCheck.Checked += (s, e) => {
                giorno.IsLavorativo = true;
                AggiornaPannelloOrari(mainGrid, giorno);
            };
            lavorativoCheck.Unchecked += (s, e) => {
                giorno.IsLavorativo = false;
                AggiornaPannelloOrari(mainGrid, giorno);
            };

            // Checkbox Continuativo
            var continuativoCheck = new CheckBox
            {
                Content = "Continuativo",
                IsChecked = giorno.IsContinuativo,
                Margin = new Thickness(5, 2, 5, 2),
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Normal
            };
            continuativoCheck.Checked += (s, e) => {
                giorno.IsContinuativo = true;
                AggiornaPannelloOrari(mainGrid, giorno);
            };
            continuativoCheck.Unchecked += (s, e) => {
                giorno.IsContinuativo = false;
                AggiornaPannelloOrari(mainGrid, giorno);
            };

            Grid.SetColumn(lavorativoCheck, 0);
            Grid.SetColumn(continuativoCheck, 1);

            mainGrid.Children.Add(lavorativoCheck);
            mainGrid.Children.Add(continuativoCheck);

            // Panel for time controls
            var orariPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(orariPanel, 3);
            mainGrid.Children.Add(orariPanel);

            // Store reference to update later
            mainGrid.Tag = new { Giorno = giorno, OrariPanel = orariPanel };

            AggiornaPannelloOrari(mainGrid, giorno);
            groupBox.Content = mainGrid;

            return groupBox;
        }

        private void AggiornaPannelloOrari(Grid mainGrid, GiornoLavorativo giorno)
        {
            var tagData = (dynamic)mainGrid.Tag;
            var orariPanel = (StackPanel)tagData.OrariPanel;
            orariPanel.Children.Clear();

            if (!giorno.IsLavorativo)
            {
                var nonLavorativoLabel = new TextBlock
                {
                    Text = "Giorno non lavorativo",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontStyle = FontStyles.Italic,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontWeight = FontWeights.Normal
                };
                orariPanel.Children.Add(nonLavorativoLabel);
                return;
            }

            if (giorno.IsContinuativo)
            {
                // Orario continuativo
                orariPanel.Children.Add(new TextBlock { Text = "Inizio:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 5, 0), FontWeight = FontWeights.Normal });
                
                var inizioCombo = CreaComboBoxOrario(giorno.OraInizioGiornata);
                inizioCombo.SelectionChanged += (s, e) => {
                    giorno.OraInizioGiornata = _orariService.ParseOrario(inizioCombo.SelectedItem?.ToString() ?? "09:00");
                };
                orariPanel.Children.Add(inizioCombo);

                orariPanel.Children.Add(new TextBlock { Text = "Fine:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 5, 0), FontWeight = FontWeights.Normal });
                
                var fineCombo = CreaComboBoxOrario(giorno.OraFineGiornata);
                fineCombo.SelectionChanged += (s, e) => {
                    giorno.OraFineGiornata = _orariService.ParseOrario(fineCombo.SelectedItem?.ToString() ?? "18:00");
                };
                orariPanel.Children.Add(fineCombo);
            }
            else
            {
                // Orario spezzato
                orariPanel.Children.Add(new TextBlock { Text = "Mattina:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 5, 0), FontWeight = FontWeights.Normal });
                
                var mattinaInizioCombo = CreaComboBoxOrario(giorno.OraInizioMattina);
                mattinaInizioCombo.SelectionChanged += (s, e) => {
                    giorno.OraInizioMattina = _orariService.ParseOrario(mattinaInizioCombo.SelectedItem?.ToString() ?? "09:00");
                };
                orariPanel.Children.Add(mattinaInizioCombo);

                orariPanel.Children.Add(new TextBlock { Text = "-", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 5, 0), FontWeight = FontWeights.Normal });
                
                var mattinaFineCombo = CreaComboBoxOrario(giorno.OraFineMattina);
                mattinaFineCombo.SelectionChanged += (s, e) => {
                    giorno.OraFineMattina = _orariService.ParseOrario(mattinaFineCombo.SelectedItem?.ToString() ?? "13:00");
                };
                orariPanel.Children.Add(mattinaFineCombo);

                orariPanel.Children.Add(new TextBlock { Text = " | Pomeriggio:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 5, 0), FontWeight = FontWeights.Normal });
                
                var pomeriggioInizioCombo = CreaComboBoxOrario(giorno.OraInizioPomeriggio);
                pomeriggioInizioCombo.SelectionChanged += (s, e) => {
                    giorno.OraInizioPomeriggio = _orariService.ParseOrario(pomeriggioInizioCombo.SelectedItem?.ToString() ?? "14:00");
                };
                orariPanel.Children.Add(pomeriggioInizioCombo);

                orariPanel.Children.Add(new TextBlock { Text = "-", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 5, 0), FontWeight = FontWeights.Normal });
                
                var pomeriggioFineCombo = CreaComboBoxOrario(giorno.OraFinePomeriggio);
                pomeriggioFineCombo.SelectionChanged += (s, e) => {
                    giorno.OraFinePomeriggio = _orariService.ParseOrario(pomeriggioFineCombo.SelectedItem?.ToString() ?? "18:00");
                };
                orariPanel.Children.Add(pomeriggioFineCombo);
            }
        }

        private ComboBox CreaComboBoxOrario(TimeSpan orarioCorrente)
        {
            var combo = new ComboBox
            {
                Width = 80,
                Margin = new Thickness(5, 2, 5, 2),
                VerticalAlignment = VerticalAlignment.Center
            };

            foreach (var orario in _orariDisponibili)
            {
                combo.Items.Add(orario);
            }

            // Set selected value
            var orarioString = $"{orarioCorrente.Hours:D2}:{orarioCorrente.Minutes:D2}";
            combo.SelectedItem = orarioString;

            return combo;
        }

        private void PreselezionaCliente(string nomeCliente)
        {
            try
            {
                // Aggiorna il titolo della finestra per mostrare il cliente preselezionato
                this.Title = $"Configurazione Ore e Giorni Lavorativi - Cliente: {nomeCliente}";

                // Cerca il cliente nella ComboBox
                for (int i = 0; i < ClienteComboBox.Items.Count; i++)
                {
                    if (ClienteComboBox.Items[i].ToString()?.Equals(nomeCliente, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        ClienteComboBox.SelectedIndex = i;
                        return;
                    }
                }

                // Se non trovato, imposta il testo direttamente (per permettere editing)
                ClienteComboBox.Text = nomeCliente;
                
                // Carica il setup se esiste
                var setup = _setupEsistenti.FirstOrDefault(s => s.NomeCliente.Equals(nomeCliente, StringComparison.OrdinalIgnoreCase));
                if (setup != null)
                {
                    _setupCorrente = setup;
                    ConsideraFestivitaCheckBox.IsChecked = _setupCorrente.ConsideraFestivita;
                    CreaInterfacciaGiorni();
                }
                else
                {
                    // Crea setup default per il cliente
                    _setupCorrente = _orariService.CreaSetupDefault(nomeCliente);
                    ConsideraFestivitaCheckBox.IsChecked = _setupCorrente.ConsideraFestivita;
                    CreaInterfacciaGiorni();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la preselezione del cliente: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClienteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClienteComboBox.SelectedItem != null)
            {
                var nomeCliente = ClienteComboBox.SelectedItem.ToString()!;
                var setup = _setupEsistenti.FirstOrDefault(s => s.NomeCliente == nomeCliente);
                
                if (setup != null)
                {
                    _setupCorrente = setup;
                    ConsideraFestivitaCheckBox.IsChecked = _setupCorrente.ConsideraFestivita;
                    CreaInterfacciaGiorni();
                }
            }
        }

        private void NuovoClienteButton_Click(object sender, RoutedEventArgs e)
        {
            var nomeCliente = InputDialogWindow.Show(
                "Inserisci il nome del nuovo cliente:", 
                "Nuovo Cliente", 
                "",
                this);

            if (!string.IsNullOrWhiteSpace(nomeCliente))
            {
                _setupCorrente = _orariService.CreaSetupDefault(nomeCliente);
                ClienteComboBox.Text = nomeCliente;
                ConsideraFestivitaCheckBox.IsChecked = _setupCorrente.ConsideraFestivita;
                CreaInterfacciaGiorni();
            }
        }

        private void CopiaLunediButton_Click(object sender, RoutedEventArgs e)
        {
            var lunedi = _setupCorrente.Lunedi;
            
            foreach (var giorno in _setupCorrente.Giorni)
            {
                if (giorno.IsLavorativo && giorno != lunedi)
                {
                    giorno.IsContinuativo = lunedi.IsContinuativo;
                    giorno.OraInizioGiornata = lunedi.OraInizioGiornata;
                    giorno.OraFineGiornata = lunedi.OraFineGiornata;
                    giorno.OraInizioMattina = lunedi.OraInizioMattina;
                    giorno.OraFineMattina = lunedi.OraFineMattina;
                    giorno.OraInizioPomeriggio = lunedi.OraInizioPomeriggio;
                    giorno.OraFinePomeriggio = lunedi.OraFinePomeriggio;
                }
            }

            CreaInterfacciaGiorni();
            MessageBox.Show("Orari del Lunedì copiati a tutti i giorni lavorativi!", 
                          "Copia Completata", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            // Set Monday to Friday as working days (9:00-18:00)
            for (int i = 0; i < 5; i++) // Monday to Friday
            {
                var giorno = _setupCorrente.Giorni[i];
                giorno.IsLavorativo = true;
                giorno.IsContinuativo = true;
                giorno.OraInizioGiornata = new TimeSpan(9, 0, 0);
                giorno.OraFineGiornata = new TimeSpan(18, 0, 0);
            }

            // Set weekend as non-working
            _setupCorrente.Sabato.IsLavorativo = false;
            _setupCorrente.Domenica.IsLavorativo = false;

            CreaInterfacciaGiorni();
            MessageBox.Show("Impostati orari default (9:00-18:00, Lunedì-Venerdì)!", 
                          "Default Applicato", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SalvaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ClienteComboBox.Text))
                {
                    MessageBox.Show("Inserisci il nome del cliente!", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _setupCorrente.NomeCliente = ClienteComboBox.Text;
                _setupCorrente.ConsideraFestivita = ConsideraFestivitaCheckBox.IsChecked == true;

                _orariService.SalvaSetupCliente(_setupCorrente, _setupEsistenti);

                MessageBox.Show($"Setup salvato per il cliente '{_setupCorrente.NomeCliente}'!", 
                              "Salvataggio Completato", MessageBoxButton.OK, MessageBoxImage.Information);

                CaricaClientiEsistenti();
                
                // Notifica al MainWindow che è stato salvato un cliente
                ClienteSalvato?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    }
}