using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LivelloHDServicePRO.Models
{
    public class SlaSetup : INotifyPropertyChanged
    {
        private string _nomeCliente = string.Empty;

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

        [XmlElement("SlaRegola")]
        public List<SlaRegola> Regole { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SlaRegola : INotifyPropertyChanged
    {
        private string _priorita = string.Empty;
        private int _giorniPresaInCarico = 0;
        private int _orePresaInCarico = 4;
        private int _giorniRisoluzione = 1;
        private int _oreRisoluzione = 0;

        [XmlAttribute]
        public string Priorita
        {
            get => _priorita;
            set
            {
                _priorita = value;
                OnPropertyChanged(nameof(Priorita));
            }
        }

        [XmlAttribute]
        public int GiorniPresaInCarico
        {
            get => _giorniPresaInCarico;
            set
            {
                _giorniPresaInCarico = value;
                OnPropertyChanged(nameof(GiorniPresaInCarico));
                OnPropertyChanged(nameof(TempoPresaInCaricoFormatted));
                OnPropertyChanged(nameof(TempoPresaInCaricoTotaleOre));
            }
        }

        [XmlAttribute]
        public int OrePresaInCarico
        {
            get => _orePresaInCarico;
            set
            {
                _orePresaInCarico = value;
                OnPropertyChanged(nameof(OrePresaInCarico));
                OnPropertyChanged(nameof(TempoPresaInCaricoFormatted));
                OnPropertyChanged(nameof(TempoPresaInCaricoTotaleOre));
            }
        }

        [XmlAttribute]
        public int GiorniRisoluzione
        {
            get => _giorniRisoluzione;
            set
            {
                _giorniRisoluzione = value;
                OnPropertyChanged(nameof(GiorniRisoluzione));
                OnPropertyChanged(nameof(TempoRisoluzioneFormatted));
                OnPropertyChanged(nameof(TempoRisoluzioneTotaleOre));
            }
        }

        [XmlAttribute]
        public int OreRisoluzione
        {
            get => _oreRisoluzione;
            set
            {
                _oreRisoluzione = value;
                OnPropertyChanged(nameof(OreRisoluzione));
                OnPropertyChanged(nameof(TempoRisoluzioneFormatted));
                OnPropertyChanged(nameof(TempoRisoluzioneTotaleOre));
            }
        }

        [XmlIgnore]
        public string TempoPresaInCaricoFormatted
        {
            get
            {
                if (GiorniPresaInCarico == 0 && OrePresaInCarico == 0)
                    return "0h";
                if (GiorniPresaInCarico == 0)
                    return $"{OrePresaInCarico}h";
                if (OrePresaInCarico == 0)
                    return $"{GiorniPresaInCarico}gg";
                return $"{GiorniPresaInCarico}gg {OrePresaInCarico}h";
            }
        }

        [XmlIgnore]
        public string TempoRisoluzioneFormatted
        {
            get
            {
                if (GiorniRisoluzione == 0 && OreRisoluzione == 0)
                    return "0h";
                if (GiorniRisoluzione == 0)
                    return $"{OreRisoluzione}h";
                if (OreRisoluzione == 0)
                    return $"{GiorniRisoluzione}gg";
                return $"{GiorniRisoluzione}gg {OreRisoluzione}h";
            }
        }

        [XmlIgnore]
        public int TempoPresaInCaricoTotaleOre => (GiorniPresaInCarico * 24) + OrePresaInCarico;

        [XmlIgnore]
        public int TempoRisoluzioneTotaleOre => (GiorniRisoluzione * 24) + OreRisoluzione;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SlaViolation
    {
        public string TipoViolazione { get; set; } = string.Empty; // "TMC" o "T-EFF"
        public TimeSpan TempoEffettivo { get; set; }
        public TimeSpan TempoSla { get; set; }
        public TimeSpan Differenza { get; set; }
        public bool FuoriSla { get; set; }

        public string DifferenzaFormatted
        {
            get
            {
                if (!FuoriSla) return "Entro SLA";
                
                var totalHours = (int)Differenza.TotalHours;
                var minutes = Differenza.Minutes;
                return $"+{totalHours:D2}:{minutes:D2}:00";
            }
        }
    }
}