using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Services
{
    public class OrariLavorativiService
    {
        private const string ConfigDirectory = "Config";
        private const string OrariFileName = "OrariLavorativi.xml";
        private string ConfigFilePath => Path.Combine(ConfigDirectory, OrariFileName);

        public OrariLavorativiService()
        {
            // Ensure config directory exists
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }

        public List<OrariLavorativiSetup> CaricaOrariLavorativi()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    return new List<OrariLavorativiSetup>();
                }

                var serializer = new XmlSerializer(typeof(OrariLavorativiCollection));
                using var reader = new FileStream(ConfigFilePath, FileMode.Open, FileAccess.Read);
                var collection = (OrariLavorativiCollection?)serializer.Deserialize(reader);
                return collection?.Setup ?? new List<OrariLavorativiSetup>();
            }
            catch (Exception)
            {
                // In case of error, return empty list
                return new List<OrariLavorativiSetup>();
            }
        }

        public void SalvaOrariLavorativi(List<OrariLavorativiSetup> orariLavorativi)
        {
            try
            {
                var collection = new OrariLavorativiCollection { Setup = orariLavorativi };
                var serializer = new XmlSerializer(typeof(OrariLavorativiCollection));
                
                using var writer = new FileStream(ConfigFilePath, FileMode.Create, FileAccess.Write);
                serializer.Serialize(writer, collection);
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore durante il salvataggio della configurazione: {ex.Message}", ex);
            }
        }

        public void SalvaSetupCliente(OrariLavorativiSetup setup, List<OrariLavorativiSetup> setupEsistenti)
        {
            // Remove existing setup for this client
            var existing = setupEsistenti.FirstOrDefault(s => s.NomeCliente.Equals(setup.NomeCliente, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                setupEsistenti.Remove(existing);
            }

            // Add new setup
            setupEsistenti.Add(setup);

            // Save to XML
            SalvaOrariLavorativi(setupEsistenti);
        }

        public OrariLavorativiSetup CreaSetupDefault(string nomeCliente)
        {
            var setup = new OrariLavorativiSetup
            {
                NomeCliente = nomeCliente,
                ConsideraFestivita = true
            };

            // Set default working hours for Monday to Friday
            foreach (var giorno in setup.Giorni.Take(5)) // Monday to Friday
            {
                giorno.IsLavorativo = true;
                giorno.IsContinuativo = true;
                giorno.OraInizioGiornata = new TimeSpan(9, 0, 0);
                giorno.OraFineGiornata = new TimeSpan(18, 0, 0);
            }

            // Weekend as non-working days
            setup.Sabato.IsLavorativo = false;
            setup.Domenica.IsLavorativo = false;

            return setup;
        }

        public List<string> GetOrariDisponibili()
        {
            var orari = new List<string>();
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    orari.Add($"{hour:D2}:{minute:D2}");
                }
            }
            return orari;
        }

        public TimeSpan ParseOrario(string orario)
        {
            if (TimeSpan.TryParse(orario, out TimeSpan result))
            {
                return result;
            }
            return new TimeSpan(9, 0, 0); // Default to 9:00 AM
        }
    }

    [XmlRoot("OrariLavorativiCollection")]
    public class OrariLavorativiCollection
    {
        [XmlElement("Setup")]
        public List<OrariLavorativiSetup> Setup { get; set; } = new();
    }
}