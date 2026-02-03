using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LivelloHDServicePRO.Models;
using LivelloHDServicePRO.Services;

namespace LivelloHDServicePRO.Views
{
    public partial class SlaSetupWindow : Window
    {
        private readonly SlaSetupService _slaSetupService;
        private readonly OrariLavorativiService _orariService;
        private List<SlaSetup> _setupEsistenti;
        private SlaSetup _setupCorrente;

        public List<int> GiorniDisponibili { get; private set; }
        public List<int> OreDisponibili { get; private set; }

        public event EventHandler? SlaSetupSalvato;

        public SlaSetupWindow() : this(null)
        {
        }

        public SlaSetupWindow(string? clientePreselezionato)
        {
            InitializeComponent();
            
            _slaSetupService = new SlaSetupService();
            _orariService = new OrariLavorativiService();
            _setupEsistenti = new List<SlaSetup>();
            _setupCorrente = new SlaSetup();

            GiorniDisponibili = _slaSetupService.GetGiorniDisponibili();
            OreDisponibili = _slaSetupService.GetOreDisponibili();

            DataContext = this;
            
            InitializeWindow();

            // Preseleziona il cliente se fornito
            if (!string.IsNullOrWhiteSpace(clientePreselezionato))
            {
                PreselezionaCliente(clientePreselezionato);
            }
        }

        private void InitializeWindow()
        {
            try
            {
                CaricaClientiDisponibili();
                CaricaSlaSetup();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'inizializzazione: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CaricaClientiDisponibili()
        {
            try
            {
                var setupClienti = _orariService.CaricaOrariLavorativi();
                
                ClienteComboBox.Items.Clear();
                foreach (var setup in setupClienti.OrderBy(s => s.NomeCliente))
                {
                    ClienteComboBox.Items.Add(setup.NomeCliente);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei clienti: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CaricaSlaSetup()
        {
            try
            {
                _setupEsistenti = _slaSetupService.CaricaSlaSetup();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei setup SLA: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PreselezionaCliente(string nomeCliente)
        {
            try
            {
                // Aggiorna il titolo della finestra per mostrare il cliente preselezionato
                this.Title = $"Configurazione SLA per Priorita - Cliente: {nomeCliente}";

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
                
                // Carica il setup SLA se esiste
                var setup = _setupEsistenti.FirstOrDefault(s => s.NomeCliente.Equals(nomeCliente, StringComparison.OrdinalIgnoreCase));
                if (setup != null)
                {
                    _setupCorrente = setup;
                }
                else
                {
                    // Crea setup SLA default per il cliente
                    _setupCorrente = _slaSetupService.CreaSetupDefault(nomeCliente);
                }

                AggiornaInterfaccia();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la preselezione del cliente SLA: {ex.Message}", 
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
                }
                else
                {
                    _setupCorrente = _slaSetupService.CreaSetupDefault(nomeCliente);
                }

                AggiornaInterfaccia();
            }
        }

        private void NuovoClienteButton_Click(object sender, RoutedEventArgs e)
        {
            var nomeCliente = Microsoft.VisualBasic.Interaction.InputBox(
                "Inserisci il nome del nuovo cliente:\n\n" +
                "Esempio: 'Azienda S.p.A.', 'Cliente Test', ecc.\n\n" +
                "Il setup SLA verra creato con priorita standard.", 
                "Nuovo Cliente SLA", 
                "");

            if (!string.IsNullOrWhiteSpace(nomeCliente))
            {
                nomeCliente = nomeCliente.Trim();
                
                // Verifica se esiste gia
                if (_setupEsistenti.Any(s => s.NomeCliente.Equals(nomeCliente, StringComparison.OrdinalIgnoreCase)))
                {
                    var result = MessageBox.Show(
                        $"?? Esiste gia un setup SLA per il cliente '{nomeCliente}'.\n\n" +
                        $"Vuoi caricarlo per modificarlo?", 
                        "Cliente Esistente", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        ClienteComboBox.Text = nomeCliente;
                        return;
                    }
                    else
                    {
                        return;
                    }
                }

                _setupCorrente = _slaSetupService.CreaSetupDefault(nomeCliente);
                ClienteComboBox.Text = nomeCliente;
                AggiornaInterfaccia();

                MessageBox.Show($"? Setup SLA creato per '{nomeCliente}'!\n\n" +
                              $"?? Priorita standard configurate: {_setupCorrente.Regole.Count}\n\n" +
                              $"Puoi modificare le priorita e i tempi SLA,\n" +
                              $"poi salva con il pulsante '?? Salva Setup SLA'.", 
                              "Nuovo Cliente Creato", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
        }

        private void AggiornaInterfaccia()
        {
            SlaRulesItemsControl.ItemsSource = null;
            SlaRulesItemsControl.ItemsSource = _setupCorrente.Regole;
            
            EliminaPrioritaButton.IsEnabled = _setupCorrente.Regole.Count > 1;
        }

        private void AggiungiPrioritaButton_Click(object sender, RoutedEventArgs e)
        {
            var nuovaPriorita = NuovaPrioritaTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(nuovaPriorita))
            {
                MessageBox.Show("?? Inserisci il nome della nuova priorita.\n\n" +
                              "Esempio: 'Critica', 'Alta', 'Media', 'Bassa'", 
                              "Campo Obbligatorio", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                NuovaPrioritaTextBox.Focus();
                return;
            }

            if (_setupCorrente.Regole.Any(r => r.Priorita.Equals(nuovaPriorita, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"? Esiste gia una priorita con il nome '{nuovaPriorita}'.\n\n" +
                              "Scegli un nome diverso o modifica la priorita esistente.", 
                              "Priorita Duplicata", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                NuovaPrioritaTextBox.SelectAll();
                NuovaPrioritaTextBox.Focus();
                return;
            }

            var nuovaRegola = new SlaRegola
            {
                Priorita = nuovaPriorita,
                GiorniPresaInCarico = 0,
                OrePresaInCarico = 4,
                GiorniRisoluzione = 1,
                OreRisoluzione = 0
            };

            _setupCorrente.Regole.Add(nuovaRegola);
            AggiornaInterfaccia();
            NuovaPrioritaTextBox.Clear();
            NuovaPrioritaTextBox.Focus();

            MessageBox.Show($"? Priorita '{nuovaPriorita}' aggiunta con successo!\n\n" +
                          $"?? Valori di default impostati:\n" +
                          $"  • Presa in carico: 4 ore\n" +
                          $"  • Risoluzione: 1 giorno\n\n" +
                          $"Puoi modificare questi valori dalla tabella sottostante.", 
                          "Priorita Aggiunta", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Information);
        }

        private void EliminaPrioritaButton_Click(object sender, RoutedEventArgs e)
        {
            if (_setupCorrente.Regole.Count <= 1)
            {
                MessageBox.Show("?? Impossibile eliminare l'ultima priorita.\n\n" +
                              "Deve rimanere almeno una priorita configurata per il calcolo SLA.", 
                              "Vincolo Configurazione", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                return;
            }

            var ultimaRegola = _setupCorrente.Regole.Last();
            var result = MessageBox.Show(
                $"??? Sei sicuro di voler eliminare la priorita '{ultimaRegola.Priorita}'?\n\n" +
                $"Configurazione attuale:\n" +
                $"  • Presa in carico: {ultimaRegola.TempoPresaInCaricoFormatted}\n" +
                $"  • Risoluzione: {ultimaRegola.TempoRisoluzioneFormatted}\n\n" +
                $"?? Questa azione non puo essere annullata.", 
                "Conferma Eliminazione", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var nomePriorita = ultimaRegola.Priorita;
                _setupCorrente.Regole.Remove(ultimaRegola);
                AggiornaInterfaccia();

                MessageBox.Show($"? Priorita '{nomePriorita}' eliminata con successo!\n\n" +
                              $"Priorita rimanenti: {_setupCorrente.Regole.Count}", 
                              "Priorita Eliminata", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
        }

        private void RipristinaDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            var prioritaAttuali = string.Join(", ", _setupCorrente.Regole.Select(r => r.Priorita));
            
            var result = MessageBox.Show(
                $"?? Sei sicuro di voler ripristinare le priorita e tempi default?\n\n" +
                $"?? Configurazione attuale ({_setupCorrente.Regole.Count} priorita):\n" +
                $"{prioritaAttuali}\n\n" +
                $"? Verranno ripristinate le priorita standard:\n" +
                $"  • Critica (4h / 1gg)\n" +
                $"  • Alta (8h / 2gg)\n" +
                $"  • Media (1gg / 5gg)\n" +
                $"  • Bassa (3gg / 10gg)\n\n" +
                $"Questa operazione sostituira TUTTE le configurazioni attuali.", 
                "Conferma Ripristino Default", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var nomeCliente = _setupCorrente.NomeCliente;
                _setupCorrente = _slaSetupService.CreaSetupDefault(nomeCliente);
                AggiornaInterfaccia();

                MessageBox.Show($"? Ripristino completato con successo!\n\n" +
                              $"?? Priorita standard configurate: {_setupCorrente.Regole.Count}\n" +
                              $"?? Cliente: {nomeCliente}", 
                              "Ripristino Default Completato", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
        }

        private void SalvaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validazione 1: Nome cliente
                if (string.IsNullOrWhiteSpace(ClienteComboBox.Text))
                {
                    MessageBox.Show("?? Seleziona o inserisci il nome del cliente!\n\n" +
                                  "Il nome del cliente e obbligatorio per salvare la configurazione SLA.", 
                                  "Campo Obbligatorio", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                    ClienteComboBox.Focus();
                    return;
                }

                _setupCorrente.NomeCliente = ClienteComboBox.Text.Trim();

                // Validazione 2: Almeno una regola
                if (_setupCorrente.Regole.Count == 0)
                {
                    MessageBox.Show("?? Nessuna priorita configurata!\n\n" +
                                  "Deve essere configurata almeno una priorita per il calcolo SLA.\n\n" +
                                  "Usa il pulsante '? Aggiungi' per creare nuove priorita.", 
                                  "Configurazione Incompleta", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                    return;
                }

                // Validazione 3: Tempi validi
                var regoleInvalide = _setupCorrente.Regole.Where(r => 
                    (r.GiorniPresaInCarico == 0 && r.OrePresaInCarico == 0) ||
                    (r.GiorniRisoluzione == 0 && r.OreRisoluzione == 0)).ToList();

                if (regoleInvalide.Any())
                {
                    var nomiPriorita = string.Join(", ", regoleInvalide.Select(r => $"'{r.Priorita}'"));
                    MessageBox.Show($"?? Tempi SLA non validi per: {nomiPriorita}\n\n" +
                                  "Ogni priorita deve avere almeno:\n" +
                                  "  • Tempo presa in carico > 0\n" +
                                  "  • Tempo risoluzione > 0", 
                                  "Validazione Tempi SLA", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                    return;
                }

                // Salvataggio
                _slaSetupService.SalvaSetupCliente(_setupCorrente, _setupEsistenti);

                // Feedback dettagliato
                var prioritaConfigurate = string.Join("\n  • ", _setupCorrente.Regole.Select(r => 
                    $"{r.Priorita}: {r.TempoPresaInCaricoFormatted} / {r.TempoRisoluzioneFormatted}"));

                MessageBox.Show($"? Setup SLA salvato con successo!\n\n" +
                              $"?? Cliente: {_setupCorrente.NomeCliente}\n" +
                              $"?? Priorita configurate: {_setupCorrente.Regole.Count}\n\n" +
                              $"Dettagli configurazione:\n  • {prioritaConfigurate}\n\n" +
                              $"Il setup e stato salvato e sara utilizzato per tutti i calcoli SLA futuri.", 
                              "Salvataggio Completato", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);

                // Notifica che il setup è stato salvato
                SlaSetupSalvato?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"? Errore durante il salvataggio:\n\n{ex.Message}\n\n" +
                              $"Verifica che:\n" +
                              $"  • Il file di configurazione sia accessibile\n" +
                              $"  • Non ci siano altri processi che bloccano il file\n" +
                              $"  • Hai i permessi di scrittura nella cartella", 
                              "Errore Salvataggio", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        private void TestMatchingButton_Click(object sender, RoutedEventArgs e)
        {
            var testPriorita = TestPrioritaTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(testPriorita))
            {
                TestResultTextBlock.Text = "?? Inserisci testo da testare";
                TestResultTextBlock.Foreground = System.Windows.Media.Brushes.Orange;
                return;
            }

            if (string.IsNullOrWhiteSpace(_setupCorrente.NomeCliente))
            {
                TestResultTextBlock.Text = "? Seleziona un cliente";
                TestResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            var regolaTrovata = _slaSetupService.FindRegolaByPriorita(_setupCorrente, testPriorita);
            
            if (regolaTrovata != null)
            {
                TestResultTextBlock.Text = $"? Match: '{regolaTrovata.Priorita}'";
                TestResultTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                TestResultTextBlock.FontWeight = FontWeights.Bold;
                
                // Mostra anche i tempi SLA associati
                var tempiInfo = $"\n?? Presa in carico: {regolaTrovata.TempoPresaInCaricoFormatted}\n" +
                               $"?? Risoluzione: {regolaTrovata.TempoRisoluzioneFormatted}";
                
                MessageBox.Show($"Priorita riconosciuta: '{regolaTrovata.Priorita}'\n" +
                              $"{tempiInfo}", 
                              "Test Matching - Successo", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);
            }
            else
            {
                TestResultTextBlock.Text = "? Nessun match trovato";
                TestResultTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                TestResultTextBlock.FontWeight = FontWeights.Bold;
                
                MessageBox.Show($"La priorita '{testPriorita}' non corrisponde a nessuna regola configurata.\n\n" +
                              "Suggerimento: Verifica che il testo inserito contenga una delle priorita configurate.",
                              "Test Matching - Nessuna Corrispondenza",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
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