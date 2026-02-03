using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LivelloHDServicePRO.Models
{
    public class OrariLavorativiSetup : INotifyPropertyChanged
    {
        private string _nomeCliente = string.Empty;
        private bool _consideraFestivita = true;

        [XmlAttribute]
        public string NomeCliente
        {
            get => _nomeCliente;
            set
            {
                _nomeCliente = value;
                OnPropertyChanged(nameof(NomeCliente));
            }
        }

        [XmlAttribute]
        public bool ConsideraFestivita
        {
            get => _consideraFestivita;
            set
            {
                _consideraFestivita = value;
                OnPropertyChanged(nameof(ConsideraFestivita));
            }
        }

        [XmlElement]
        public GiornoLavorativo Lunedi { get; set; } = new() { NomeGiorno = "Lunedì" };

        [XmlElement]
        public GiornoLavorativo Martedi { get; set; } = new() { NomeGiorno = "Martedì" };

        [XmlElement]
        public GiornoLavorativo Mercoledi { get; set; } = new() { NomeGiorno = "Mercoledì" };

        [XmlElement]
        public GiornoLavorativo Giovedi { get; set; } = new() { NomeGiorno = "Giovedì" };

        [XmlElement]
        public GiornoLavorativo Venerdi { get; set; } = new() { NomeGiorno = "Venerdì" };

        [XmlElement]
        public GiornoLavorativo Sabato { get; set; } = new() { NomeGiorno = "Sabato" };

        [XmlElement]
        public GiornoLavorativo Domenica { get; set; } = new() { NomeGiorno = "Domenica" };

        [XmlIgnore]
        public GiornoLavorativo[] Giorni => new[] { Lunedi, Martedi, Mercoledi, Giovedi, Venerdi, Sabato, Domenica };

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class GiornoLavorativo : INotifyPropertyChanged
    {
        private bool _isLavorativo = true;
        private bool _isContinuativo = true;
        private TimeSpan _oraInizioGiornata = new(9, 0, 0);
        private TimeSpan _oraFineGiornata = new(18, 0, 0);
        private TimeSpan _oraInizioMattina = new(9, 0, 0);
        private TimeSpan _oraFineMattina = new(13, 0, 0);
        private TimeSpan _oraInizioPomeriggio = new(14, 0, 0);
        private TimeSpan _oraFinePomeriggio = new(18, 0, 0);

        [XmlAttribute]
        public string NomeGiorno { get; set; } = string.Empty;

        [XmlAttribute]
        public bool IsLavorativo
        {
            get => _isLavorativo;
            set
            {
                _isLavorativo = value;
                OnPropertyChanged(nameof(IsLavorativo));
            }
        }

        [XmlAttribute]
        public bool IsContinuativo
        {
            get => _isContinuativo;
            set
            {
                _isContinuativo = value;
                OnPropertyChanged(nameof(IsContinuativo));
            }
        }

        [XmlElement]
        public TimeSpan OraInizioGiornata
        {
            get => _oraInizioGiornata;
            set
            {
                _oraInizioGiornata = value;
                OnPropertyChanged(nameof(OraInizioGiornata));
            }
        }

        [XmlElement]
        public TimeSpan OraFineGiornata
        {
            get => _oraFineGiornata;
            set
            {
                _oraFineGiornata = value;
                OnPropertyChanged(nameof(OraFineGiornata));
            }
        }

        [XmlElement]
        public TimeSpan OraInizioMattina
        {
            get => _oraInizioMattina;
            set
            {
                _oraInizioMattina = value;
                OnPropertyChanged(nameof(OraInizioMattina));
            }
        }

        [XmlElement]
        public TimeSpan OraFineMattina
        {
            get => _oraFineMattina;
            set
            {
                _oraFineMattina = value;
                OnPropertyChanged(nameof(OraFineMattina));
            }
        }

        [XmlElement]
        public TimeSpan OraInizioPomeriggio
        {
            get => _oraInizioPomeriggio;
            set
            {
                _oraInizioPomeriggio = value;
                OnPropertyChanged(nameof(OraInizioPomeriggio));
            }
        }

        [XmlElement]
        public TimeSpan OraFinePomeriggio
        {
            get => _oraFinePomeriggio;
            set
            {
                _oraFinePomeriggio = value;
                OnPropertyChanged(nameof(OraFinePomeriggio));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}