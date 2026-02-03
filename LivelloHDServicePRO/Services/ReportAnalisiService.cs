using System;
using System.Collections.Generic;
using System.Linq;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Services
{
    /// <summary>
    /// Servizio per generare report di analisi sui dati SLA
    /// </summary>
    public class ReportAnalisiService
    {
        private readonly SlaSetupService _slaSetupService;

        public ReportAnalisiService()
        {
            _slaSetupService = new SlaSetupService();
        }

        /// <summary>
        /// Genera il report completo di analisi
        /// </summary>
        public ReportData GeneraReport(List<SlaRecord> records, OrariLavorativiSetup? clienteSetup)
        {
            var reportData = new ReportData
            {
                DataGenerazione = DateTime.Now,
                TotalTickets = records.Count,
                TicketsRisolti = records.Count(r => r.DataChiusura.HasValue),
                TicketsInCorso = records.Count(r => !r.DataChiusura.HasValue),
                PeriodoAnalisi = GetPeriodoAnalisi(records)
            };

            // Calcola tempi medi per priorità
            reportData.TempiMediPerPriorita = CalcolaTempiMediPerPriorita(records);
            
            // Analizza i proprietari
            reportData.AnalisiProprietari = AnalizzaProprietari(records, clienteSetup);
            
            // ?? Analizza proprietari per priorità
            reportData.AnalisiProprietariPerPriorita = AnalizzaProprietariPerPriorita(records, clienteSetup);
            
            // ?? Analisi dettagliata delle risorse
            reportData.AnalisiDettagliataRisorse = AnalizzaRisorseDettagliatamente(records, clienteSetup);
            
            // Distribuzione ticket
            reportData.DistribuzionePriorita = CalcolaDistribuzionePriorita(records);
            reportData.DistribuzioneProprietario = CalcolaDistribuzioneProprietario(records);
            
            // Performance SLA
            reportData.SlaPerformanceData = CalcolaSlaPerformance(records);
            
            // Riepilogo generale
            reportData.RiepilogoGenerale = CalcolaRiepilogoGenerale(records, reportData);

            return reportData;
        }

        private string GetPeriodoAnalisi(List<SlaRecord> records)
        {
            if (!records.Any()) return "Nessun dato";

            var dataMin = records.Where(r => r.DataCreazione.HasValue).Min(r => r.DataCreazione!.Value);
            var dataMax = records.Where(r => r.DataCreazione.HasValue).Max(r => r.DataCreazione!.Value);

            return $"{dataMin:dd/MM/yyyy} - {dataMax:dd/MM/yyyy}";
        }

        private List<TempoMedioPriorita> CalcolaTempiMediPerPriorita(List<SlaRecord> records)
        {
            return records
                .Where(r => !string.IsNullOrEmpty(r.Priorita))
                .GroupBy(r => r.Priorita!)
                .Select(g => new TempoMedioPriorita
                {
                    Priorita = g.Key,
                    NumeroTickets = g.Count(),
                    TempoMedioTMC = CalcolaTempoMedio(g, r => r.TmcTimeSpan),
                    TempoMedioTMS = CalcolaTempoMedio(g, r => r.TmsTimeSpan),
                    TempoMedioTEFF = CalcolaTempoMedio(g, r => r.TeffTimeSpan),
                    TempoMedioTSOSP = CalcolaTempoMedio(g, r => r.TsospTimeSpan)
                })
                .OrderByDescending(t => t.NumeroTickets)
                .ToList();
        }

        private List<AnalisiProprietario> AnalizzaProprietari(List<SlaRecord> records, OrariLavorativiSetup? clienteSetup)
        {
            var slaSetup = CaricaSlaSetup(clienteSetup);

            return records
                .Where(r => !string.IsNullOrEmpty(r.Proprietario))
                .GroupBy(r => r.Proprietario!)
                .Select(g => new AnalisiProprietario
                {
                    NomeProprietario = g.Key,
                    TotalTickets = g.Count(),
                    TicketsRisolti = g.Count(r => r.DataChiusura.HasValue),
                    TempiPerPriorita = CalcolaTempiProprietarioPerPriorita(g.ToList()),
                    ValutazioneComplessiva = ValutaProprietario(g.ToList(), slaSetup)
                })
                .OrderByDescending(p => p.ValutazioneNumerica) // Prima i migliori
                .ThenByDescending(p => p.TotalTickets) // Poi per volume
                .ToList();
        }

        private List<AnalisiProprietarioPriorita> AnalizzaProprietariPerPriorita(List<SlaRecord> records, OrariLavorativiSetup? clienteSetup)
        {
            var slaSetup = CaricaSlaSetup(clienteSetup);

            return records
                .Where(r => !string.IsNullOrEmpty(r.Proprietario) && !string.IsNullOrEmpty(r.Priorita))
                .GroupBy(r => new { r.Proprietario, r.Priorita })
                .Select(g => new AnalisiProprietarioPriorita
                {
                    NomeProprietario = g.Key.Proprietario!,
                    Priorita = g.Key.Priorita!,
                    NumeroTickets = g.Count(),
                    TicketsRisolti = g.Count(r => r.DataChiusura.HasValue),
                    TempoMedioTMC = CalcolaTempoMedio(g, r => r.TmcTimeSpan),
                    TempoMedioTMS = CalcolaTempoMedio(g, r => r.TmsTimeSpan),
                    TempoMedioTEFF = CalcolaTempoMedio(g, r => r.TeffTimeSpan),
                    TempoMedioTSOSP = CalcolaTempoMedio(g, r => r.TsospTimeSpan),
                    TicketsFuoriSLA = ContaTicketsFuoriSLA(g.ToList()),
                    ValutazionePriorita = ValutaProprietarioPerPriorita(g.ToList(), g.Key.Priorita!, slaSetup)
                })
                .OrderByDescending(p => p.ValutazioneNumerica) // Prima i migliori
                .ThenBy(p => p.NomeProprietario) // Poi alfabetico
                .ThenByDescending(p => p.NumeroTickets) // Infine per volume
                .ToList();
        }

        private ValutazioneQualita ValutaProprietarioPerPriorita(List<SlaRecord> records, string priorita, SlaSetup? slaSetup)
        {
            if (slaSetup == null || !records.Any()) return ValutazioneQualita.NonValutabile;

            var regola = _slaSetupService.FindRegolaByPriorita(slaSetup, priorita);
            if (regola == null) return ValutazioneQualita.NonValutabile;

            var tempoMedioTMC = CalcolaTempoMedio(records, r => r.TmcTimeSpan);
            var tempoMedioTEFF = CalcolaTempoMedio(records, r => r.TeffTimeSpan);

            // Calcola le soglie SLA (semplificato - 8 ore lavorative al giorno)
            var sogliaTMC = TimeSpan.FromHours((regola.GiorniPresaInCarico * 8) + regola.OrePresaInCarico);
            var sogliaTEFF = TimeSpan.FromHours((regola.GiorniRisoluzione * 8) + regola.OreRisoluzione);

            var valutazioneTMC = ValutaTempo(tempoMedioTMC, sogliaTMC);
            var valutazioneTEFF = ValutaTempo(tempoMedioTEFF, sogliaTEFF);

            // Restituisce la valutazione peggiore tra TMC e T-EFF
            if (valutazioneTMC == ValutazioneQualita.Critico || valutazioneTEFF == ValutazioneQualita.Critico) 
                return ValutazioneQualita.Critico;
            if (valutazioneTMC == ValutazioneQualita.Migliorabile || valutazioneTEFF == ValutazioneQualita.Migliorabile) 
                return ValutazioneQualita.Migliorabile;
            if (valutazioneTMC == ValutazioneQualita.Discreto || valutazioneTEFF == ValutazioneQualita.Discreto) 
                return ValutazioneQualita.Discreto;
            if (valutazioneTMC == ValutazioneQualita.Ottimo && valutazioneTEFF == ValutazioneQualita.Ottimo) 
                return ValutazioneQualita.Ottimo;

            return ValutazioneQualita.NonValutabile;
        }

        private Dictionary<string, TempiProprietario> CalcolaTempiProprietarioPerPriorita(List<SlaRecord> recordsProprietario)
        {
            return recordsProprietario
                .Where(r => !string.IsNullOrEmpty(r.Priorita))
                .GroupBy(r => r.Priorita!)
                .ToDictionary(
                    g => g.Key,
                    g => new TempiProprietario
                    {
                        TempoMedioTMC = CalcolaTempoMedio(g, r => r.TmcTimeSpan),
                        TempoMedioTMS = CalcolaTempoMedio(g, r => r.TmsTimeSpan),
                        TempoMedioTEFF = CalcolaTempoMedio(g, r => r.TeffTimeSpan),
                        NumeroTickets = g.Count()
                    });
        }

        private ValutazioneQualita ValutaProprietario(List<SlaRecord> records, SlaSetup? slaSetup)
        {
            if (slaSetup == null || !records.Any()) return ValutazioneQualita.NonValutabile;

            var valutazioni = new List<ValutazioneQualita>();

            // Raggruppa per priorità e valuta ogni gruppo
            var gruppiPriorita = records.GroupBy(r => r.Priorita).Where(g => !string.IsNullOrEmpty(g.Key));

            foreach (var gruppo in gruppiPriorita)
            {
                var regola = _slaSetupService.FindRegolaByPriorita(slaSetup, gruppo.Key!);
                if (regola == null) continue;

                var tempoMedioTMC = CalcolaTempoMedio(gruppo, r => r.TmcTimeSpan);
                var tempoMedioTEFF = CalcolaTempoMedio(gruppo, r => r.TeffTimeSpan);

                // Calcola le soglie SLA (semplificato - 8 ore lavorative al giorno)
                var sogliaTMC = TimeSpan.FromHours((regola.GiorniPresaInCarico * 8) + regola.OrePresaInCarico);
                var sogliaTEFF = TimeSpan.FromHours((regola.GiorniRisoluzione * 8) + regola.OreRisoluzione);

                valutazioni.Add(ValutaTempo(tempoMedioTMC, sogliaTMC));
                valutazioni.Add(ValutaTempo(tempoMedioTEFF, sogliaTEFF));
            }

            // Restituisce la valutazione peggiore
            if (valutazioni.Contains(ValutazioneQualita.Critico)) return ValutazioneQualita.Critico;
            if (valutazioni.Contains(ValutazioneQualita.Migliorabile)) return ValutazioneQualita.Migliorabile;
            if (valutazioni.Contains(ValutazioneQualita.Discreto)) return ValutazioneQualita.Discreto;
            if (valutazioni.Contains(ValutazioneQualita.Ottimo)) return ValutazioneQualita.Ottimo;

            return ValutazioneQualita.NonValutabile;
        }

        private ValutazioneQualita ValutaTempo(TimeSpan tempoEffettivo, TimeSpan sogliaSkla)
        {
            if (tempoEffettivo == TimeSpan.Zero || sogliaSkla == TimeSpan.Zero) 
                return ValutazioneQualita.NonValutabile;

            var percentuale = tempoEffettivo.TotalHours / sogliaSkla.TotalHours;

            return percentuale switch
            {
                >= 0.9 => ValutazioneQualita.Critico,      // >= 90% del limite
                >= 0.7 => ValutazioneQualita.Migliorabile, // 70-89% del limite
                >= 0.5 => ValutazioneQualita.Discreto,     // 50-69% del limite
                _ => ValutazioneQualita.Ottimo             // < 50% del limite
            };
        }

        private List<DistribuzionePriorita> CalcolaDistribuzionePriorita(List<SlaRecord> records)
        {
            var totalRecords = records.Count;
            if (totalRecords == 0) return new List<DistribuzionePriorita>();

            return records
                .Where(r => !string.IsNullOrEmpty(r.Priorita))
                .GroupBy(r => r.Priorita!)
                .Select(g => new DistribuzionePriorita
                {
                    Priorita = g.Key,
                    NumeroTickets = g.Count(),
                    Percentuale = (double)g.Count() / totalRecords * 100,
                    TicketsFuoriSLA = ContaTicketsFuoriSLA(g.ToList())
                })
                .OrderByDescending(d => d.NumeroTickets)
                .ToList();
        }

        private List<DistribuzioneProprietario> CalcolaDistribuzioneProprietario(List<SlaRecord> records)
        {
            var totalRecords = records.Count;
            if (totalRecords == 0) return new List<DistribuzioneProprietario>();

            return records
                .Where(r => !string.IsNullOrEmpty(r.Proprietario))
                .GroupBy(r => r.Proprietario!)
                .Select(g => new DistribuzioneProprietario
                {
                    Proprietario = g.Key,
                    NumeroTickets = g.Count(),
                    Percentuale = (double)g.Count() / totalRecords * 100,
                    TicketsRisolti = g.Count(r => r.DataChiusura.HasValue)
                })
                .OrderByDescending(d => d.NumeroTickets)
                .ToList();
        }

        private List<SlaPerformance> CalcolaSlaPerformance(List<SlaRecord> records)
        {
            return new List<SlaPerformance>
            {
                CalcolaSlaPerformanceCategoria(records, "TMC", r => r.TMCFuoriSLA),
                CalcolaSlaPerformanceCategoria(records, "T-EFF", r => r.TEFFFuoriSLA)
            };
        }

        private SlaPerformance CalcolaSlaPerformanceCategoria(List<SlaRecord> records, string categoria, Func<SlaRecord, string?> slaSelector)
        {
            var recordsConSLA = records.Where(r => !string.IsNullOrWhiteSpace(slaSelector(r))).ToList();
            var ticketsFuoriSLA = recordsConSLA.Count(r => IsFuoriSLA(slaSelector(r)));

            return new SlaPerformance
            {
                Categoria = categoria,
                TicketsTotali = recordsConSLA.Count,
                TicketsEntroSLA = recordsConSLA.Count - ticketsFuoriSLA,
                TicketsFuoriSLA = ticketsFuoriSLA
            };
        }

        private RiepilogoStatistiche CalcolaRiepilogoGenerale(List<SlaRecord> records, ReportData reportData)
        {
            var riepilogo = new RiepilogoStatistiche
            {
                TempoMedioGlobaleTMC = CalcolaTempoMedio(records, r => r.TmcTimeSpan),
                TempoMedioGlobaleTMS = CalcolaTempoMedio(records, r => r.TmsTimeSpan),
                TempoMedioGlobaleTEFF = CalcolaTempoMedio(records, r => r.TeffTimeSpan)
            };

            // Calcola percentuali complessive
            var totalConSLA = records.Count(r => !string.IsNullOrWhiteSpace(r.TMCFuoriSLA) || !string.IsNullOrWhiteSpace(r.TEFFFuoriSLA));
            var totalFuoriSLA = records.Count(r => IsFuoriSLA(r.TMCFuoriSLA) || IsFuoriSLA(r.TEFFFuoriSLA));
            
            riepilogo.PercentualeComplessivaFuoriSLA = totalConSLA > 0 ? (double)totalFuoriSLA / totalConSLA * 100 : 0;
            riepilogo.PercentualeComplessivaEntroSLA = 100 - riepilogo.PercentualeComplessivaFuoriSLA;

            // Trova migliore e peggiore proprietario
            if (reportData.AnalisiProprietari.Any())
            {
                riepilogo.ProprietarioMigliore = reportData.AnalisiProprietari
                    .Where(p => p.ValutazioneComplessiva == ValutazioneQualita.Ottimo)
                    .OrderByDescending(p => p.TotalTickets)
                    .FirstOrDefault()?.NomeProprietario ?? "Nessuno";

                riepilogo.ProprietarioCritico = reportData.AnalisiProprietari
                    .Where(p => p.ValutazioneComplessiva == ValutazioneQualita.Critico)
                    .OrderByDescending(p => p.TotalTickets)
                    .FirstOrDefault()?.NomeProprietario ?? "Nessuno";
            }

            // Trova priorità più/meno problematiche
            if (reportData.DistribuzionePriorita.Any())
            {
                riepilogo.PrioritaPiuProblematica = reportData.DistribuzionePriorita
                    .OrderByDescending(p => p.PercentualeFuoriSLA)
                    .First().Priorita;

                riepilogo.PrioritaMenoProblematica = reportData.DistribuzionePriorita
                    .OrderBy(p => p.PercentualeFuoriSLA)
                    .First().Priorita;
            }

            return riepilogo;
        }

        // Metodi di supporto
        private TimeSpan CalcolaTempoMedio(IEnumerable<SlaRecord> records, Func<SlaRecord, TimeSpan?> timeSelector)
        {
            var tempi = records.Select(timeSelector).Where(t => t.HasValue && t.Value > TimeSpan.Zero).ToList();
            if (!tempi.Any()) return TimeSpan.Zero;

            var totalTicks = tempi.Sum(t => t!.Value.Ticks);
            return new TimeSpan(totalTicks / tempi.Count);
        }

        private int ContaTicketsFuoriSLA(List<SlaRecord> records)
        {
            return records.Count(r => IsFuoriSLA(r.TMCFuoriSLA) || IsFuoriSLA(r.TEFFFuoriSLA));
        }

        private bool IsFuoriSLA(string? slaValue)
        {
            if (string.IsNullOrWhiteSpace(slaValue)) return false;
            return slaValue.StartsWith("+") || 
                   (!slaValue.Contains("Entro SLA", StringComparison.OrdinalIgnoreCase) &&
                    !slaValue.Equals("N/D", StringComparison.OrdinalIgnoreCase));
        }

        private SlaSetup? CaricaSlaSetup(OrariLavorativiSetup? clienteSetup)
        {
            if (clienteSetup == null) return null;

            var slaSetupList = _slaSetupService.CaricaSlaSetup();
            return slaSetupList.FirstOrDefault(s => s.NomeCliente.Equals(clienteSetup.NomeCliente, StringComparison.OrdinalIgnoreCase));
        }

        private List<AnalisiDettagliataRisorsa> AnalizzaRisorseDettagliatamente(List<SlaRecord> records, OrariLavorativiSetup? clienteSetup)
        {
            var slaSetup = CaricaSlaSetup(clienteSetup);
            var risultati = new List<AnalisiDettagliataRisorsa>();

            // Calcola medie globali per comparazioni
            var tempoMedioGlobaleTMC = CalcolaTempoMedio(records, r => r.TmcTimeSpan);
            var tempoMedioGlobaleTEFF = CalcolaTempoMedio(records, r => r.TeffTimeSpan);

            var proprietari = records.Where(r => !string.IsNullOrEmpty(r.Proprietario))
                                   .GroupBy(r => r.Proprietario!);

            foreach (var gruppo in proprietari)
            {
                var ticketsProprietario = gruppo.ToList();
                var analisi = new AnalisiDettagliataRisorsa
                {
                    NomeProprietario = gruppo.Key,
                    TicketTotali = ticketsProprietario.Count,
                    TicketChiusi = ticketsProprietario.Count(t => t.DataChiusura.HasValue),
                    TicketInCorso = ticketsProprietario.Count(t => !t.DataChiusura.HasValue),
                    
                    // Tempi medi
                    TempoMedioTMC = CalcolaTempoMedio(ticketsProprietario, r => r.TmcTimeSpan),
                    TempoMedioTMS = CalcolaTempoMedio(ticketsProprietario, r => r.TmsTimeSpan),
                    TempoMedioTEFF = CalcolaTempoMedio(ticketsProprietario, r => r.TeffTimeSpan),
                    TempoMedioTSOSP = CalcolaTempoMedio(ticketsProprietario, r => r.TsospTimeSpan),
                    
                    // SLA Performance
                    TicketsEntroSLA = ticketsProprietario.Count - ContaTicketsFuoriSLA(ticketsProprietario),
                    TicketsFuoriSLA = ContaTicketsFuoriSLA(ticketsProprietario),
                    
                    // Distribuzione per priorità
                    TicketsPerPriorita = ticketsProprietario.Where(t => !string.IsNullOrEmpty(t.Priorita))
                                                          .GroupBy(t => t.Priorita!)
                                                          .ToDictionary(g => g.Key, g => g.Count()),
                    
                    // Valutazione
                    ValutazioneComplessiva = ValutaProprietario(ticketsProprietario, slaSetup)
                };

                // Calcola deviazioni dalle medie
                analisi.DeviazioneDallaMediaTMC = CalcolaDeviazione(analisi.TempoMedioTMC, tempoMedioGlobaleTMC);
                analisi.DeviazioneDallaMediaTEFF = CalcolaDeviazione(analisi.TempoMedioTEFF, tempoMedioGlobaleTEFF);

                // Genera analisi qualitativa
                GeneraAnalisiQualitativa(analisi, ticketsProprietario, slaSetup);

                // Calcola tendenze mensili
                analisi.TendenzeMensili = CalcolaTendenzeMensili(ticketsProprietario);

                risultati.Add(analisi);
            }

            // Determina posizioni relative
            DeterminaPosizioniRelative(risultati);

            return risultati.OrderByDescending(r => r.VotazioneNumerica)
                           .ThenByDescending(r => r.TicketTotali)
                           .ToList();
        }

        private double CalcolaDeviazione(TimeSpan tempoIndividuale, TimeSpan tempoMedio)
        {
            if (tempoMedio == TimeSpan.Zero) return 0;
            return ((tempoIndividuale.TotalHours - tempoMedio.TotalHours) / tempoMedio.TotalHours) * 100;
        }

        private void GeneraAnalisiQualitativa(AnalisiDettagliataRisorsa analisi, List<SlaRecord> tickets, SlaSetup? slaSetup)
        {
            // Punti di forza
            if (analisi.PercentualeRisoluzione >= 95)
                analisi.PuntiDiForza.Add("Eccellente tasso di risoluzione dei ticket");
            
            if (analisi.PercentualeEntroSLA >= 85)
                analisi.PuntiDiForza.Add("Ottimo rispetto degli SLA");
            
            if (analisi.DeviazioneDallaMediaTEFF < -20)
                analisi.PuntiDiForza.Add("Tempi di risoluzione superiori alla media del team");

            // Gestisce bene le priorità alte
            if (analisi.TicketsPerPriorita.ContainsKey("Alta") && analisi.TicketsPerPriorita["Alta"] > analisi.TicketTotali * 0.3)
                analisi.PuntiDiForza.Add("Gestisce efficacemente ticket ad alta priorità");

            // Aree di miglioramento
            if (analisi.PercentualeRisoluzione < 85)
                analisi.AreeDiMiglioramento.Add("Tasso di risoluzione sotto la soglia ottimale");
            
            if (analisi.PercentualeFuoriSLA > 25)
                analisi.AreeDiMiglioramento.Add("Frequenti violazioni degli SLA");
            
            if (analisi.DeviazioneDallaMediaTEFF > 30)
                analisi.AreeDiMiglioramento.Add("Tempi di risoluzione significativamente sopra la media");

            if (analisi.TempoMedioTMC.TotalHours > 4)
                analisi.AreeDiMiglioramento.Add("Tempi di presa in carico troppo lunghi");

            // Suggerimenti per azioni
            if (analisi.PercentualeFuoriSLA > 30)
                analisi.SuggerimentiAzioni.Add("Prioritizzare formazione su gestione tempi e SLA");
            
            if (analisi.TempoMedioTMC.TotalHours > 6)
                analisi.SuggerimentiAzioni.Add("Implementare sistema di notifiche per ticket non assegnati");
            
            if (analisi.DeviazioneDallaMediaTEFF > 25)
                analisi.SuggerimentiAzioni.Add("Analizzare processo di risoluzione per identificare inefficienze");
            
            if (!analisi.PuntiDiForza.Any())
                analisi.SuggerimentiAzioni.Add("Pianificare sessioni di mentoring con colleghi più esperti");
            
            // Suggerimenti positivi per top performer
            if (analisi.VotazioneNumerica >= 8)
                analisi.SuggerimentiAzioni.Add("Considerare come mentor per altri membri del team");

            // Sintesi analitica
            var sintesi = $"Risorsa con {analisi.TicketTotali} ticket gestiti ";
            
            if (analisi.VotazioneNumerica >= 8)
                sintesi += "con prestazioni eccellenti. ";
            else if (analisi.VotazioneNumerica >= 6)
                sintesi += "con prestazioni buone. ";
            else if (analisi.VotazioneNumerica >= 4)
                sintesi += "con prestazioni da migliorare. ";
            else
                sintesi += "con prestazioni critiche. ";

            if (analisi.PercentualeFuoriSLA > 25)
                sintesi += "Attenzione particolare richiesta per il rispetto degli SLA. ";
            
            if (analisi.DeviazioneDallaMediaTEFF < -15)
                sintesi += "Tempi di risoluzione migliori della media del team. ";
            else if (analisi.DeviazioneDallaMediaTEFF > 25)
                sintesi += "Tempi di risoluzione da ottimizzare. ";

            sintesi += $"Specializzato principalmente in ticket di priorità {analisi.PrioritaPiuGestita}.";
            
            analisi.SintesiAnalitica = sintesi;
        }

        private List<TendenzaMensile> CalcolaTendenzeMensili(List<SlaRecord> tickets)
        {
            var tendenze = new List<TendenzaMensile>();
            
            // Raggruppa per mese (ultimi 6 mesi)
            var ticketsConDate = tickets.Where(t => t.DataCreazione.HasValue).ToList();
            if (!ticketsConDate.Any()) return tendenze;

            var ultimiMesi = ticketsConDate.GroupBy(t => new { 
                    Anno = t.DataCreazione!.Value.Year, 
                    Mese = t.DataCreazione.Value.Month 
                })
                .OrderByDescending(g => g.Key.Anno)
                .ThenByDescending(g => g.Key.Mese)
                .Take(6)
                .ToList();

            TendenzaMensile? mesePrecedente = null;

            foreach (var gruppo in ultimiMesi.OrderBy(g => g.Key.Anno).ThenBy(g => g.Key.Mese))
            {
                var ticketsMese = gruppo.ToList();
                var tendenza = new TendenzaMensile
                {
                    Mese = $"{gruppo.Key.Anno}-{gruppo.Key.Mese:D2}",
                    MeseDisplay = $"{GetNomeMese(gruppo.Key.Mese)} {gruppo.Key.Anno}",
                    TicketsGestiti = ticketsMese.Count,
                    PercentualeRisoluzione = ticketsMese.Count > 0 ? 
                        (double)ticketsMese.Count(t => t.DataChiusura.HasValue) / ticketsMese.Count * 100 : 0,
                    TempoMedioTEFF = CalcolaTempoMedio(ticketsMese, t => t.TeffTimeSpan)
                };

                // Determina tendenza rispetto al mese precedente
                if (mesePrecedente != null)
                {
                    var diffRisoluzione = tendenza.PercentualeRisoluzione - mesePrecedente.PercentualeRisoluzione;
                    
                    if (diffRisoluzione > 5)
                    {
                        tendenza.TendenzaIcon = "UP";
                        tendenza.TendenzaDescrizione = "Miglioramento";
                    }
                    else if (diffRisoluzione < -5)
                    {
                        tendenza.TendenzaIcon = "DOWN";
                        tendenza.TendenzaDescrizione = "Peggioramento";
                    }
                    else
                    {
                        tendenza.TendenzaIcon = "STABLE";
                        tendenza.TendenzaDescrizione = "Stabile";
                    }
                }

                tendenze.Add(tendenza);
                mesePrecedente = tendenza;
            }

            return tendenze.OrderByDescending(t => t.Mese).ToList();
        }

        private void DeterminaPosizioniRelative(List<AnalisiDettagliataRisorsa> analisi)
        {
            var totalCount = analisi.Count;
            var top20Percent = (int)Math.Ceiling(totalCount * 0.2);
            var bottom20Percent = (int)Math.Ceiling(totalCount * 0.2);

            var ordinate = analisi.OrderByDescending(a => a.VotazioneNumerica).ToList();

            for (int i = 0; i < ordinate.Count; i++)
            {
                if (i < top20Percent)
                    ordinate[i].PosizioneRelativa = "Top 20% del team";
                else if (i >= totalCount - bottom20Percent)
                    ordinate[i].PosizioneRelativa = "Bottom 20% del team";
                else
                    ordinate[i].PosizioneRelativa = "Nella media del team";
            }
        }

        private string GetNomeMese(int mese)
        {
            return mese switch
            {
                1 => "Gennaio", 2 => "Febbraio", 3 => "Marzo", 4 => "Aprile",
                5 => "Maggio", 6 => "Giugno", 7 => "Luglio", 8 => "Agosto",
                9 => "Settembre", 10 => "Ottobre", 11 => "Novembre", 12 => "Dicembre",
                _ => "Sconosciuto"
            };
        }
    }
}