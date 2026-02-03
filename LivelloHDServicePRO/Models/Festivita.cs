using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace LivelloHDServicePRO.Models
{
    public class Festivita : INotifyPropertyChanged
    {
        private int _giorno = 1;
        private int _mese = 1;
        private string _nome = string.Empty;

        [XmlAttribute]
        public int Giorno
        {
            get => _giorno;
            set
            {
                _giorno = value;
                OnPropertyChanged(nameof(Giorno));
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        [XmlAttribute]
        public int Mese
        {
            get => _mese;
            set
            {
                _mese = value;
                OnPropertyChanged(nameof(Mese));
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        [XmlAttribute]
        public string Nome
        {
            get => _nome;
            set
            {
                _nome = value ?? string.Empty;
                OnPropertyChanged(nameof(Nome));
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        [XmlIgnore]
        public string DisplayText
        {
            get
            {
                try
                {
                    var date = new DateTime(DateTime.Now.Year, Mese, Giorno);
                    var nomeMesseCompleto = CultureInfo.GetCultureInfo("it-IT").DateTimeFormat.GetMonthName(Mese);
                    return $"{Giorno} {nomeMesseCompleto} - {Nome}";
                }
                catch
                {
                    return $"{Giorno}/{Mese} - {Nome}";
                }
            }
        }

        [XmlIgnore]
        public string NomeMese
        {
            get
            {
                try
                {
                    return CultureInfo.GetCultureInfo("it-IT").DateTimeFormat.GetMonthName(Mese);
                }
                catch
                {
                    return Mese.ToString();
                }
            }
        }

        public bool IsValidDate()
        {
            try
            {
                var date = new DateTime(DateTime.Now.Year, Mese, Giorno);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DateTime GetDateForYear(int year)
        {
            return new DateTime(year, Mese, Giorno);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}