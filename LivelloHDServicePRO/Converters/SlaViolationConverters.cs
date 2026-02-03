using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LivelloHDServicePRO.Converters
{
    public class SlaViolationBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Transparent;
            
            var slaValue = value.ToString();
            
            if (string.IsNullOrWhiteSpace(slaValue))
                return Brushes.Transparent;

            // Se il valore inizia con '+', è fuori SLA
            if (slaValue.StartsWith("+"))
            {
                // Determina il colore in base al parametro (TMC o T-EFF)
                var colorType = parameter?.ToString()?.ToUpperInvariant();
                
                return colorType switch
                {
                    "TMC" => new SolidColorBrush(Color.FromRgb(255, 205, 210)),  // Rosso chiaro
                    "TEFF" => new SolidColorBrush(Color.FromRgb(255, 224, 178)), // Arancione chiaro
                    _ => new SolidColorBrush(Color.FromRgb(255, 235, 238))       // Rosa molto chiaro (default)
                };
            }

            // Se contiene "Entro SLA", usa un verde molto chiaro
            if (slaValue.Contains("Entro SLA", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(200, 230, 201)); // Verde molto chiaro
            }

            // Per tutti gli altri casi (N/D, Errore, ecc.)
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SlaViolationForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Brushes.Black;
            
            var slaValue = value.ToString();
            
            if (string.IsNullOrWhiteSpace(slaValue))
                return Brushes.Black;

            // Se il valore inizia con '+', è fuori SLA
            if (slaValue.StartsWith("+"))
            {
                // Determina il colore del testo in base al parametro (TMC o T-EFF)
                var colorType = parameter?.ToString()?.ToUpperInvariant();
                
                return colorType switch
                {
                    "TMC" => new SolidColorBrush(Color.FromRgb(183, 28, 28)),    // Rosso scuro
                    "TEFF" => new SolidColorBrush(Color.FromRgb(230, 81, 0)),    // Arancione scuro
                    _ => new SolidColorBrush(Color.FromRgb(136, 14, 79))         // Viola scuro (default)
                };
            }

            // Se contiene "Entro SLA", usa un verde scuro
            if (slaValue.Contains("Entro SLA", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(27, 94, 32)); // Verde scuro
            }

            // Per tutti gli altri casi, usa il colore standard
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}