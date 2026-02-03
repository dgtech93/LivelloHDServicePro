using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Services
{
    public class FestivitaService
    {
        private const string ConfigDirectory = "Config";
        private const string FestivitaFileName = "Festivita.xml";
        private string ConfigFilePath => Path.Combine(ConfigDirectory, FestivitaFileName);

        public FestivitaService()
        {
            // Ensure config directory exists
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }

        public List<Festivita> CaricaFestivita()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    return CreaFestivitaDefault();
                }

                var serializer = new XmlSerializer(typeof(FestivitaCollection));
                using var reader = new FileStream(ConfigFilePath, FileMode.Open, FileAccess.Read);
                var collection = (FestivitaCollection?)serializer.Deserialize(reader);
                
                var festivita = collection?.Festivita ?? new List<Festivita>();
                
                // If no holidays loaded, create default ones
                if (festivita.Count == 0)
                {
                    festivita = CreaFestivitaDefault();
                }

                return festivita.OrderBy(f => f.Mese).ThenBy(f => f.Giorno).ToList();
            }
            catch (Exception)
            {
                // In case of error, return default holidays
                return CreaFestivitaDefault();
            }
        }

        public void SalvaFestivita(List<Festivita> festivita)
        {
            try
            {
                var collection = new FestivitaCollection { Festivita = festivita };
                var serializer = new XmlSerializer(typeof(FestivitaCollection));
                
                using var writer = new FileStream(ConfigFilePath, FileMode.Create, FileAccess.Write);
                serializer.Serialize(writer, collection);
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore durante il salvataggio delle festivita: {ex.Message}", ex);
            }
        }

        private List<Festivita> CreaFestivitaDefault()
        {
            return new List<Festivita>
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
        }

        public List<int> GetGiorniDisponibili(int mese)
        {
            var giorni = new List<int>();
            try
            {
                var giorniInMese = DateTime.DaysInMonth(DateTime.Now.Year, mese);
                for (int i = 1; i <= giorniInMese; i++)
                {
                    giorni.Add(i);
                }
            }
            catch
            {
                // Default to 31 days
                for (int i = 1; i <= 31; i++)
                {
                    giorni.Add(i);
                }
            }
            return giorni;
        }

        public List<string> GetMesiDisponibili()
        {
            var mesi = new List<string>();
            for (int i = 1; i <= 12; i++)
            {
                try
                {
                    var nomeMese = System.Globalization.CultureInfo.GetCultureInfo("it-IT")
                                    .DateTimeFormat.GetMonthName(i);
                    mesi.Add($"{i} - {nomeMese}");
                }
                catch
                {
                    mesi.Add($"Mese {i}");
                }
            }
            return mesi;
        }

        public bool EsisteFestivita(List<Festivita> festivita, int giorno, int mese, string nome = "")
        {
            return festivita.Any(f => f.Giorno == giorno && f.Mese == mese && 
                                    (string.IsNullOrEmpty(nome) || f.Nome != nome));
        }

        public bool IsFestivita(List<Festivita> festivita, DateTime data)
        {
            return festivita.Any(f => f.Giorno == data.Day && f.Mese == data.Month);
        }
    }

    [XmlRoot("FestivitaCollection")]
    public class FestivitaCollection
    {
        [XmlElement("Festivita")]
        public List<Festivita> Festivita { get; set; } = new();
    }
}