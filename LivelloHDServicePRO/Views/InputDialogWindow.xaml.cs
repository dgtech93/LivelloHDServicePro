using System.Windows;

namespace LivelloHDServicePRO.Views
{
    /// <summary>
    /// Dialog WPF nativo per sostituire InputBox di Visual Basic
    /// Compatibile con .NET 10 senza dipendenze da Windows.Forms
    /// </summary>
    public partial class InputDialogWindow : Window
    {
        public string ResponseText { get; private set; } = string.Empty;

        public InputDialogWindow(string prompt, string title = "Input", string defaultResponse = "")
        {
            InitializeComponent();

            Title = title;
            PromptTextBlock.Text = prompt;
            InputTextBox.Text = defaultResponse;
            
            // Focus sul textbox
            Loaded += (s, e) =>
            {
                InputTextBox.Focus();
                InputTextBox.SelectAll();
            };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = InputTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = string.Empty;
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Metodo statico helper per semplificare l'uso (simile a InputBox)
        /// </summary>
        public static string Show(string prompt, string title = "Input", string defaultResponse = "", Window? owner = null)
        {
            var dialog = new InputDialogWindow(prompt, title, defaultResponse);
            
            if (owner != null)
            {
                dialog.Owner = owner;
            }
            
            var result = dialog.ShowDialog();
            
            return result == true ? dialog.ResponseText : string.Empty;
        }
    }
}
