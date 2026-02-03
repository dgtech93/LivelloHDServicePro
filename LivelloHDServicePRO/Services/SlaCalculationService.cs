using System;
using System.Collections.Generic;
using System.Linq;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Services
{
    public class SlaCalculationService
    {
        private readonly FestivitaService _festivitaService;
        private readonly SlaSetupService _slaSetupService;
        private List<Festivita>? _festivita;

        public SlaCalculationService()
        {
            _festivitaService = new FestivitaService();
            _slaSetupService = new SlaSetupService();
        }

        public SlaDetailInfo CalculateSlaDetails(SlaRecord record, OrariLavorativiSetup? clienteSetup)
        {
            var detailInfo = new SlaDetailInfo
            {
                Record = record,
                ClienteSetup = clienteSetup,
                DataApertura = record.DataCreazione,
                DataPresaInCarico = record.DataPresaInCarico,
                DataInizioSospensione = record.DataInizioSospensione,
                DataFineSospensione = record.DataFineSospensione,
                DataChiusura = record.DataChiusura
            };

            // Carica festività se necessario
            if (clienteSetup?.ConsideraFestivita == true)
            {
                _festivita = _festivitaService.CaricaFestivita();
            }

            // Carica setup SLA se disponibile
            if (clienteSetup != null)
            {
                var slaSetupList = _slaSetupService.CaricaSlaSetup();
                detailInfo.SlaSetup = slaSetupList.FirstOrDefault(s => s.NomeCliente.Equals(clienteSetup.NomeCliente, StringComparison.OrdinalIgnoreCase));
                
                if (detailInfo.SlaSetup != null)
                {
                    detailInfo.SlaRegola = _slaSetupService.FindRegolaByPriorita(detailInfo.SlaSetup, record.Priorita);
                }
            }

            // Calcola TMC (Tempo Medio di Carico)
            if (detailInfo.DataApertura.HasValue && detailInfo.DataPresaInCarico.HasValue)
            {
                detailInfo.TmcResult = CalculatePeriod(
                    detailInfo.DataApertura.Value,
                    detailInfo.DataPresaInCarico.Value,
                    "TMC",
                    clienteSetup);
            }

            // Calcola TMS (Tempo Medio di Soluzione)
            if (detailInfo.DataApertura.HasValue && detailInfo.DataChiusura.HasValue)
            {
                detailInfo.TmsResult = CalculatePeriod(
                    detailInfo.DataApertura.Value,
                    detailInfo.DataChiusura.Value,
                    "TMS",
                    clienteSetup);
            }

            // Calcola TSOSP (Tempo Sospensione)
            if (detailInfo.DataInizioSospensione.HasValue && detailInfo.DataFineSospensione.HasValue)
            {
                detailInfo.TsospResult = CalculatePeriod(
                    detailInfo.DataInizioSospensione.Value,
                    detailInfo.DataFineSospensione.Value,
                    "TSOSP",
                    clienteSetup);
            }

            // Calcola confronti SLA se abbiamo sia i risultati che le regole
            if (detailInfo.HasSlaComparison)
            {
                CalculateSlaComparisons(detailInfo, clienteSetup!);
            }

            return detailInfo;
        }

        private SlaCalculationResult CalculatePeriod(DateTime dataInizio, DateTime dataFine, string tipoCalcolo, OrariLavorativiSetup? clienteSetup)
        {
            var result = new SlaCalculationResult
            {
                DataInizio = dataInizio,
                DataFine = dataFine,
                TipoCalcolo = tipoCalcolo
            };

            try
            {
                if (dataFine <= dataInizio)
                {
                    result.ErrorMessage = "Data fine deve essere successiva alla data inizio";
                    return result;
                }

                if (clienteSetup == null)
                {
                    result.ErrorMessage = "Nessun setup cliente configurato";
                    return result;
                }

                var currentDate = dataInizio.Date;
                var endDate = dataFine.Date;
                var totalWorkingTime = TimeSpan.Zero;
                var giorniConsiderati = 0;
                
                // Contatori per giorni esclusi
                var giorniNonLavorativiEsclusi = 0;
                var festivitaEscluse = 0;
                var totaleGiorniPeriodo = (int)(endDate - currentDate).TotalDays + 1;

                // Primo giorno
                var orePrimoGiorno = CalculateFirstDayHours(dataInizio, currentDate == endDate ? dataFine : DateTime.MinValue, clienteSetup);
                result.OrePrimoGiorno = orePrimoGiorno;
                if (orePrimoGiorno > TimeSpan.Zero)
                {
                    totalWorkingTime += orePrimoGiorno;
                    giorniConsiderati++;
                }

                // Giorni intermedi
                currentDate = currentDate.AddDays(1);
                var giorniIntermedi = 0;
                var oreGiorniIntermedi = TimeSpan.Zero;

                while (currentDate < endDate)
                {
                    if (IsWorkingDay(currentDate, clienteSetup))
                    {
                        var oreLavorativeGiorno = GetDailyWorkingHours(currentDate, clienteSetup);
                        oreGiorniIntermedi += oreLavorativeGiorno;
                        totalWorkingTime += oreLavorativeGiorno;
                        giorniIntermedi++;
                        giorniConsiderati++;
                    }
                    else
                    {
                        // Conta i giorni esclusi
                        if (!IsWorkingDayOfWeek(currentDate, clienteSetup))
                        {
                            giorniNonLavorativiEsclusi++;
                        }
                        else if (clienteSetup.ConsideraFestivita && _festivita != null && _festivitaService.IsFestivita(_festivita, currentDate))
                        {
                            festivitaEscluse++;
                        }
                    }
                    currentDate = currentDate.AddDays(1);
                }

                // Ultimo giorno (se diverso dal primo)
                var oreUltimoGiorno = TimeSpan.Zero;
                if (dataInizio.Date != dataFine.Date)
                {
                    oreUltimoGiorno = CalculateLastDayHours(dataFine, clienteSetup);
                    result.OreUltimoGiorno = oreUltimoGiorno;
                    if (oreUltimoGiorno > TimeSpan.Zero)
                    {
                        totalWorkingTime += oreUltimoGiorno;
                        giorniConsiderati++;
                    }
                }

                result.GiorniIntermedi = giorniIntermedi;
                result.OreGiorniIntermedi = oreGiorniIntermedi;
                result.TotaleGiorniConsiderati = giorniConsiderati;
                result.TotaleOre = totalWorkingTime;
                result.GiorniNonLavorativiEsclusi = giorniNonLavorativiEsclusi;
                result.FestivitaEscluse = festivitaEscluse;
                result.TotaleGiorniPeriodo = totaleGiorniPeriodo;
                result.CalcoloValido = true;

                // Crea dettaglio calcolo
                CreateCalculationDetail(result, dataInizio, dataFine);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Errore durante il calcolo: {ex.Message}";
            }

            return result;
        }

        private TimeSpan CalculateFirstDayHours(DateTime dataInizio, DateTime dataFine, OrariLavorativiSetup clienteSetup)
        {
            var dayOfWeek = dataInizio.DayOfWeek;
            var giornoLavorativo = GetWorkingDay(dayOfWeek, clienteSetup);

            if (!giornoLavorativo.IsLavorativo || !IsWorkingDay(dataInizio.Date, clienteSetup))
            {
                return TimeSpan.Zero;
            }

            var timeInizio = dataInizio.TimeOfDay;
            var timeFine = dataFine != DateTime.MinValue ? dataFine.TimeOfDay : new TimeSpan(23, 59, 59);

            return CalculateHoursInDay(timeInizio, timeFine, giornoLavorativo);
        }

        private TimeSpan CalculateLastDayHours(DateTime dataFine, OrariLavorativiSetup clienteSetup)
        {
            var dayOfWeek = dataFine.DayOfWeek;
            var giornoLavorativo = GetWorkingDay(dayOfWeek, clienteSetup);

            if (!giornoLavorativo.IsLavorativo || !IsWorkingDay(dataFine.Date, clienteSetup))
            {
                return TimeSpan.Zero;
            }

            var timeInizio = TimeSpan.Zero;
            var timeFine = dataFine.TimeOfDay;

            return CalculateHoursInDay(timeInizio, timeFine, giornoLavorativo);
        }

        private TimeSpan CalculateHoursInDay(TimeSpan timeInizio, TimeSpan timeFine, GiornoLavorativo giornoLavorativo)
        {
            if (giornoLavorativo.IsContinuativo)
            {
                return CalculateContinuousHours(timeInizio, timeFine, giornoLavorativo);
            }
            else
            {
                return CalculateSplitHours(timeInizio, timeFine, giornoLavorativo);
            }
        }

        private TimeSpan CalculateContinuousHours(TimeSpan timeInizio, TimeSpan timeFine, GiornoLavorativo giornoLavorativo)
        {
            var startWork = giornoLavorativo.OraInizioGiornata;
            var endWork = giornoLavorativo.OraFineGiornata;

            // Se l'ora di inizio è dopo la fine del lavoro, nessuna ora lavorativa
            if (timeInizio >= endWork)
                return TimeSpan.Zero;

            // Se l'ora di fine è prima dell'inizio del lavoro, nessuna ora lavorativa
            if (timeFine <= startWork)
                return TimeSpan.Zero;

            // Aggiusta gli orari ai limiti lavorativi
            var effectiveStart = timeInizio < startWork ? startWork : timeInizio;
            var effectiveEnd = timeFine > endWork ? endWork : timeFine;

            return effectiveEnd - effectiveStart;
        }

        private TimeSpan CalculateSplitHours(TimeSpan timeInizio, TimeSpan timeFine, GiornoLavorativo giornoLavorativo)
        {
            var oreMattina = TimeSpan.Zero;
            var orePomeriggio = TimeSpan.Zero;

            // Calcola ore mattina
            var startMattina = giornoLavorativo.OraInizioMattina;
            var endMattina = giornoLavorativo.OraFineMattina;

            if (timeInizio < endMattina && timeFine > startMattina)
            {
                var effectiveStartMattina = timeInizio < startMattina ? startMattina : timeInizio;
                var effectiveEndMattina = timeFine > endMattina ? endMattina : timeFine;
                
                if (effectiveEndMattina > effectiveStartMattina)
                {
                    oreMattina = effectiveEndMattina - effectiveStartMattina;
                }
            }

            // Calcola ore pomeriggio
            var startPomeriggio = giornoLavorativo.OraInizioPomeriggio;
            var endPomeriggio = giornoLavorativo.OraFinePomeriggio;

            if (timeInizio < endPomeriggio && timeFine > startPomeriggio)
            {
                var effectiveStartPomeriggio = timeInizio < startPomeriggio ? startPomeriggio : timeInizio;
                var effectiveEndPomeriggio = timeFine > endPomeriggio ? endPomeriggio : timeFine;
                
                if (effectiveEndPomeriggio > effectiveStartPomeriggio)
                {
                    orePomeriggio = effectiveEndPomeriggio - effectiveStartPomeriggio;
                }
            }

            return oreMattina + orePomeriggio;
        }

        private TimeSpan GetDailyWorkingHours(DateTime date, OrariLavorativiSetup clienteSetup)
        {
            var dayOfWeek = date.DayOfWeek;
            var giornoLavorativo = GetWorkingDay(dayOfWeek, clienteSetup);

            if (!giornoLavorativo.IsLavorativo)
                return TimeSpan.Zero;

            if (giornoLavorativo.IsContinuativo)
            {
                return giornoLavorativo.OraFineGiornata - giornoLavorativo.OraInizioGiornata;
            }
            else
            {
                var oreMattina = giornoLavorativo.OraFineMattina - giornoLavorativo.OraInizioMattina;
                var orePomeriggio = giornoLavorativo.OraFinePomeriggio - giornoLavorativo.OraInizioPomeriggio;
                return oreMattina + orePomeriggio;
            }
        }

        private bool IsWorkingDay(DateTime date, OrariLavorativiSetup clienteSetup)
        {
            var dayOfWeek = date.DayOfWeek;
            var giornoLavorativo = GetWorkingDay(dayOfWeek, clienteSetup);

            // Verifica se è un giorno lavorativo
            if (!giornoLavorativo.IsLavorativo)
                return false;

            // Verifica se è una festività (se il cliente considera le festività)
            if (clienteSetup.ConsideraFestivita && _festivita != null)
            {
                return !_festivitaService.IsFestivita(_festivita, date);
            }

            return true;
        }

        private bool IsWorkingDayOfWeek(DateTime date, OrariLavorativiSetup clienteSetup)
        {
            var dayOfWeek = date.DayOfWeek;
            var giornoLavorativo = GetWorkingDay(dayOfWeek, clienteSetup);
            return giornoLavorativo.IsLavorativo;
        }

        private GiornoLavorativo GetWorkingDay(DayOfWeek dayOfWeek, OrariLavorativiSetup clienteSetup)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => clienteSetup.Lunedi,
                DayOfWeek.Tuesday => clienteSetup.Martedi,
                DayOfWeek.Wednesday => clienteSetup.Mercoledi,
                DayOfWeek.Thursday => clienteSetup.Giovedi,
                DayOfWeek.Friday => clienteSetup.Venerdi,
                DayOfWeek.Saturday => clienteSetup.Sabato,
                DayOfWeek.Sunday => clienteSetup.Domenica,
                _ => clienteSetup.Lunedi
            };
        }

        private void CreateCalculationDetail(SlaCalculationResult result, DateTime dataInizio, DateTime dataFine)
        {
            var detail = $"Calcolo {result.TipoCalcolo}:\n";
            detail += $"Periodo: {dataInizio:dd/MM/yyyy HH:mm} - {dataFine:dd/MM/yyyy HH:mm}\n\n";
            
            if (result.OrePrimoGiorno > TimeSpan.Zero)
            {
                detail += $"Primo giorno: {result.OrePrimoGiornoFormatted}\n";
            }
            else if (dataInizio.Date != dataFine.Date)
            {
                detail += "Primo giorno: 00:00:00 (fuori orario lavorativo)\n";
            }
            
            if (result.GiorniIntermedi > 0)
            {
                detail += $"Giorni intermedi ({result.GiorniIntermedi}): {result.OreGiorniIntermediFormatted}\n";
                detail += $"  (Ogni giorno lavorativo completo)\n";
            }
            
            if (result.OreUltimoGiorno > TimeSpan.Zero)
            {
                detail += $"Ultimo giorno: {result.OreUltimoGiornoFormatted}\n";
            }
            else if (dataInizio.Date != dataFine.Date)
            {
                detail += "Ultimo giorno: 00:00:00 (fuori orario lavorativo)\n";
            }
            
            detail += $"--- RIEPILOGO ---\n";
            detail += $"Totale giorni nel periodo: {result.TotaleGiorniPeriodo}\n";
            detail += $"Giorni lavorativi considerati: {result.TotaleGiorniConsiderati}\n";
            detail += $"Giorni non lavorativi esclusi: {result.GiorniNonLavorativiEsclusi}\n";
            detail += $"Festivita escluse: {result.FestivitaEscluse}\n";
            detail += $"Totale ore lavorative: {result.TotaleOreFormatted}\n\n";
            
            // Calcolo dettagliato con verifica
            detail += $"DETTAGLIO CALCOLO:\n";
            detail += result.GetCalculationSummary();
            detail += $"\n\nNOTA: Il totale considera solo le ore lavorative effettive\n";
            detail += $"secondo il setup orari del cliente selezionato.";

            result.DettaglioCalcolo = detail;
        }

        private void CalculateSlaComparisons(SlaDetailInfo detailInfo, OrariLavorativiSetup clienteSetup)
        {
            if (detailInfo.SlaRegola == null) return;

            // Confronto TMC
            if (detailInfo.HasValidTmc && detailInfo.TmcResult != null)
            {
                var tempoSlaPresaInCarico = _slaSetupService.CalculateWorkingHoursFromSla(
                    detailInfo.SlaRegola.GiorniPresaInCarico, 
                    detailInfo.SlaRegola.OrePresaInCarico, 
                    clienteSetup);

                detailInfo.TmcComparison = CreateSlaComparison(
                    "TMC",
                    detailInfo.TmcResult.TotaleOre,
                    tempoSlaPresaInCarico);
            }

            // Confronto T-EFF (calcolato come TMS - TSOSP)
            if (detailInfo.HasValidTms && detailInfo.TmsResult != null)
            {
                var tempoEffettivo = detailInfo.TmsResult.TotaleOre;
                if (detailInfo.HasValidTsosp && detailInfo.TsospResult != null)
                {
                    tempoEffettivo = detailInfo.TmsResult.TotaleOre - detailInfo.TsospResult.TotaleOre;
                }

                var tempoSlaRisoluzione = _slaSetupService.CalculateWorkingHoursFromSla(
                    detailInfo.SlaRegola.GiorniRisoluzione,
                    detailInfo.SlaRegola.OreRisoluzione,
                    clienteSetup);

                detailInfo.TeffComparison = CreateSlaComparison(
                    "T-EFF",
                    tempoEffettivo,
                    tempoSlaRisoluzione);
            }
        }

        private SlaComparisonResult CreateSlaComparison(string tipoSla, TimeSpan tempoEffettivo, TimeSpan tempoSla)
        {
            var entroSla = tempoEffettivo <= tempoSla;
            var differenza = entroSla ? tempoSla - tempoEffettivo : tempoEffettivo - tempoSla;

            return new SlaComparisonResult
            {
                TipoSla = tipoSla,
                TempoEffettivo = tempoEffettivo,
                TempoSla = tempoSla,
                EntroSla = entroSla,
                Differenza = differenza
            };
        }
    }
}