using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Services
{
    public class SlaSetupService
    {
        private const string ConfigDirectory = "Config";
        private const string SlaFileName = "SlaSetup.xml";
        private string ConfigFilePath => Path.Combine(ConfigDirectory, SlaFileName);

        public SlaSetupService()
        {
            // Ensure config directory exists
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }
        }

        public List<SlaSetup> CaricaSlaSetup()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    return new List<SlaSetup>();
                }

                var serializer = new XmlSerializer(typeof(SlaSetupCollection));
                using var reader = new FileStream(ConfigFilePath, FileMode.Open, FileAccess.Read);
                var collection = (SlaSetupCollection?)serializer.Deserialize(reader);
                return collection?.Setup ?? new List<SlaSetup>();
            }
            catch (Exception)
            {
                // In case of error, return empty list
                return new List<SlaSetup>();
            }
        }

        public void SalvaSlaSetup(List<SlaSetup> slaSetup)
        {
            try
            {
                var collection = new SlaSetupCollection { Setup = slaSetup };
                var serializer = new XmlSerializer(typeof(SlaSetupCollection));
                
                using var writer = new FileStream(ConfigFilePath, FileMode.Create, FileAccess.Write);
                serializer.Serialize(writer, collection);
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore durante il salvataggio della configurazione SLA: {ex.Message}", ex);
            }
        }

        public void SalvaSetupCliente(SlaSetup setup, List<SlaSetup> setupEsistenti)
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
            SalvaSlaSetup(setupEsistenti);
        }

        public SlaSetup CreaSetupDefault(string nomeCliente)
        {
            var setup = new SlaSetup
            {
                NomeCliente = nomeCliente,
                Regole = new List<SlaRegola>
                {
                    new() { Priorita = "Bloccante", GiorniPresaInCarico = 0, OrePresaInCarico = 1, GiorniRisoluzione = 0, OreRisoluzione = 4 },
                    new() { Priorita = "Alta", GiorniPresaInCarico = 0, OrePresaInCarico = 2, GiorniRisoluzione = 0, OreRisoluzione = 8 },
                    new() { Priorita = "Media", GiorniPresaInCarico = 0, OrePresaInCarico = 4, GiorniRisoluzione = 1, OreRisoluzione = 0 },
                    new() { Priorita = "Bassa", GiorniPresaInCarico = 0, OrePresaInCarico = 8, GiorniRisoluzione = 2, OreRisoluzione = 0 }
                }
            };

            return setup;
        }

        public List<string> GetPrioritaDefault()
        {
            return new List<string> { "Bloccante", "Alta", "Media", "Bassa" };
        }

        public List<int> GetGiorniDisponibili()
        {
            var giorni = new List<int>();
            for (int i = 0; i <= 30; i++)
            {
                giorni.Add(i);
            }
            return giorni;
        }

        public List<int> GetOreDisponibili()
        {
            var ore = new List<int>();
            for (int i = 0; i <= 23; i++)
            {
                ore.Add(i);
            }
            return ore;
        }

        public SlaViolation CalculateSlaViolation(string tipoViolazione, TimeSpan tempoEffettivo, SlaRegola regola, OrariLavorativiSetup clienteSetup)
        {
            TimeSpan tempoSla;
            
            if (tipoViolazione == "TMC")
            {
                // Calcola le ore lavorative per la presa in carico
                tempoSla = CalculateWorkingHoursFromSla(regola.GiorniPresaInCarico, regola.OrePresaInCarico, clienteSetup);
            }
            else // T-EFF
            {
                // Calcola le ore lavorative per la risoluzione
                tempoSla = CalculateWorkingHoursFromSla(regola.GiorniRisoluzione, regola.OreRisoluzione, clienteSetup);
            }

            var fuoriSla = tempoEffettivo > tempoSla;
            var differenza = fuoriSla ? tempoEffettivo - tempoSla : TimeSpan.Zero;

            return new SlaViolation
            {
                TipoViolazione = tipoViolazione,
                TempoEffettivo = tempoEffettivo,
                TempoSla = tempoSla,
                Differenza = differenza,
                FuoriSla = fuoriSla
            };
        }

        public TimeSpan CalculateWorkingHoursFromSla(int giorni, int ore, OrariLavorativiSetup clienteSetup)
        {
            if (clienteSetup == null)
            {
                // Se non c'è setup cliente, usa 8 ore per giorno lavorativo standard
                return TimeSpan.FromHours((giorni * 8) + ore);
            }

            // Calcola le ore lavorative giornaliere per questo cliente
            var oreLavorativeGiornaliere = GetDailyWorkingHours(clienteSetup);
            
            // Calcola il tempo SLA totale in ore lavorative
            var oreTotaliSla = (giorni * oreLavorativeGiornaliere) + ore;
            
            return TimeSpan.FromHours(oreTotaliSla);
        }

        private double GetDailyWorkingHours(OrariLavorativiSetup clienteSetup)
        {
            // Usa il lunedì come riferimento per calcolare le ore lavorative giornaliere
            var lunedi = clienteSetup.Lunedi;
            
            if (!lunedi.IsLavorativo)
            {
                // Se il lunedì non è lavorativo, usa il primo giorno lavorativo
                var primoGiornoLavorativo = clienteSetup.Giorni.FirstOrDefault(g => g.IsLavorativo);
                if (primoGiornoLavorativo != null)
                {
                    lunedi = primoGiornoLavorativo;
                }
                else
                {
                    // Nessun giorno lavorativo, usa 8 ore standard
                    return 8.0;
                }
            }

            if (lunedi.IsContinuativo)
            {
                return (lunedi.OraFineGiornata - lunedi.OraInizioGiornata).TotalHours;
            }
            else
            {
                var oreMattina = (lunedi.OraFineMattina - lunedi.OraInizioMattina).TotalHours;
                var orePomeriggio = (lunedi.OraFinePomeriggio - lunedi.OraInizioPomeriggio).TotalHours;
                return oreMattina + orePomeriggio;
            }
        }

        public SlaRegola? FindRegolaByPriorita(SlaSetup slaSetup, string priorita)
        {
            if (string.IsNullOrWhiteSpace(priorita))
                return null;

            // Prima prova una ricerca esatta (per retrocompatibilità)
            var regolaEsatta = slaSetup.Regole.FirstOrDefault(r => r.Priorita.Equals(priorita, StringComparison.OrdinalIgnoreCase));
            if (regolaEsatta != null)
                return regolaEsatta;

            // Se non trova match esatto, prova una ricerca intelligente
            var prioritaPulita = PulisciPriorita(priorita);
            
            // Cerca se la priorità configurata è contenuta nella stringa importata
            var regolaContenuta = slaSetup.Regole.FirstOrDefault(r => 
                prioritaPulita.Contains(r.Priorita, StringComparison.OrdinalIgnoreCase) ||
                r.Priorita.Contains(prioritaPulita, StringComparison.OrdinalIgnoreCase));
            
            if (regolaContenuta != null)
                return regolaContenuta;

            // Ultima prova: cerca per parole chiave
            return slaSetup.Regole.FirstOrDefault(r => 
                ContienePriorita(prioritaPulita, r.Priorita));
        }

        private string PulisciPriorita(string priorita)
        {
            if (string.IsNullOrWhiteSpace(priorita))
                return string.Empty;

            // Rimuovi emoji e simboli comuni
            var prioritaPulita = priorita;
            
            // Rimuovi emoji circolari colorati comuni
            prioritaPulita = prioritaPulita.Replace("??", "").Replace("??", "").Replace("??", "")
                                         .Replace("??", "").Replace("??", "").Replace("??", "")
                                         .Replace("?", "").Replace("?", "");
            
            // Rimuovi altri simboli comuni
            prioritaPulita = prioritaPulita.Replace("?", "").Replace("?", "").Replace("?", "")
                                         .Replace("?", "").Replace("?", "").Replace("?", "")
                                         .Replace("?", "").Replace("?", "").Replace("!", "");
            
            // Rimuovi spazi multipli e trim
            while (prioritaPulita.Contains("  "))
            {
                prioritaPulita = prioritaPulita.Replace("  ", " ");
            }
            
            return prioritaPulita.Trim();
        }

        private bool ContienePriorita(string prioritaImportata, string prioritaConfigurata)
        {
            if (string.IsNullOrWhiteSpace(prioritaImportata) || string.IsNullOrWhiteSpace(prioritaConfigurata))
                return false;

            var paroleImportate = prioritaImportata.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var prioritaConf = prioritaConfigurata.ToLowerInvariant();

            // Controlla se qualche parola della priorità importata corrisponde alla priorità configurata
            foreach (var parola in paroleImportate)
            {
                if (prioritaConf.Equals(parola, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // Controlla anche l'inverso: se la priorità configurata è contenuta in una delle parole
            return paroleImportate.Any(parola => parola.Contains(prioritaConf) || prioritaConf.Contains(parola));
        }
    }

    [XmlRoot("SlaSetupCollection")]
    public class SlaSetupCollection
    {
        [XmlElement("SlaSetup")]
        public List<SlaSetup> Setup { get; set; } = new();
    }
}