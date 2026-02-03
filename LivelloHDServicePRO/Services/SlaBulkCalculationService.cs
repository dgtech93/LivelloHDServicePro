using System;
using System.Collections.Generic;
using System.Linq;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Services
{
    public class SlaBulkCalculationService
    {
        private readonly SlaCalculationService _slaCalculationService;
        private readonly SlaSetupService _slaSetupService;

        public SlaBulkCalculationService()
        {
            _slaCalculationService = new SlaCalculationService();
            _slaSetupService = new SlaSetupService();
        }

        public void CalculateSlaBulk(List<SlaRecord> records, OrariLavorativiSetup? clienteSetup)
        {
            CalculateSlaBulk(records, clienteSetup, false);
        }

        public void CalculateSlaBulk(List<SlaRecord> records, OrariLavorativiSetup? clienteSetup, bool useTodayForMissingDates)
        {
            if (clienteSetup == null)
            {
                // Se non c'è setup cliente, impostiamo tutti i valori come non disponibili
                foreach (var record in records)
                {
                    SetRecordAsNotAvailable(record);
                }
                return;
            }

            // Carica il setup SLA per questo cliente
            var slaSetupList = _slaSetupService.CaricaSlaSetup();
            var slaSetup = slaSetupList.FirstOrDefault(s => s.NomeCliente.Equals(clienteSetup.NomeCliente, StringComparison.OrdinalIgnoreCase));

            foreach (var record in records)
            {
                try
                {
                    // Applica la logica "Usa Data Odierna se non compilate" 
                    if (useTodayForMissingDates)
                    {
                        ApplyTodayDateLogic(record);
                    }

                    // Calcola i dettagli SLA per questo record usando ESATTAMENTE lo stesso metodo del dettaglio
                    var detailInfo = _slaCalculationService.CalculateSlaDetails(record, clienteSetup);

                    // TMC - Tempo Medio di Carico (sempre formato hh:mm:ss)
                    if (detailInfo.HasValidTmc && detailInfo.TmcResult != null)
                    {
                        record.TMC = detailInfo.TmcResult.TotaleOreFormatted;
                        record.TmcTimeSpan = detailInfo.TmcResult.TotaleOre;
                    }
                    else
                    {
                        record.TMC = "N/D";
                        record.TmcTimeSpan = null;
                    }

                    // TMS - Tempo Medio di Soluzione (sempre formato hh:mm:ss)
                    if (detailInfo.HasValidTms && detailInfo.TmsResult != null)
                    {
                        record.TMS = detailInfo.TmsResult.TotaleOreFormatted;
                        record.TmsTimeSpan = detailInfo.TmsResult.TotaleOre;
                    }
                    else
                    {
                        record.TMS = "N/D";
                        record.TmsTimeSpan = null;
                    }

                    // TSOSP - Tempo Sospensione (sempre formato hh:mm:ss)
                    if (detailInfo.HasValidTsosp && detailInfo.TsospResult != null)
                    {
                        record.TSOSP = detailInfo.TsospResult.TotaleOreFormatted;
                        record.TsospTimeSpan = detailInfo.TsospResult.TotaleOre;
                    }
                    else
                    {
                        record.TSOSP = "00:00:00";
                        record.TsospTimeSpan = TimeSpan.Zero;
                    }

                    // Calcola T-EFF (Tempo Effettivo = TMS - TSOSP)
                    if (record.TmsTimeSpan.HasValue)
                    {
                        var tsospValue = record.TsospTimeSpan ?? TimeSpan.Zero;
                        var teffValue = record.TmsTimeSpan.Value - tsospValue;

                        if (teffValue < TimeSpan.Zero)
                            teffValue = TimeSpan.Zero;

                        record.TEFF = FormatTimeSpanHHMMSS(teffValue);
                        record.TeffTimeSpan = teffValue;
                    }
                    else
                    {
                        record.TEFF = "N/D";
                        record.TeffTimeSpan = null;
                    }

                    // Calcola SLA violations se abbiamo setup SLA
                    if (slaSetup != null)
                    {
                        CalculateSlaViolations(record, slaSetup, clienteSetup);
                    }
                    else
                    {
                        record.TMCFuoriSLA = "N/D Setup";
                        record.TEFFFuoriSLA = "N/D Setup";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore nel calcolo SLA per record {record.NumeroCaso}: {ex.Message}");
                    SetRecordAsError(record);
                }
            }
        }

        private void ApplyTodayDateLogic(SlaRecord record)
        {
            var today = DateTime.Now;

            // Se non c'è data di Chiusura, usa data attuale per calcolare TMS
            if (record.DataCreazione.HasValue && !record.DataChiusura.HasValue)
            {
                record.DataChiusura = today;
            }

            // Se non c'è data Presa in Carico, usa data attuale per calcolare TMC
            if (record.DataCreazione.HasValue && !record.DataPresaInCarico.HasValue)
            {
                record.DataPresaInCarico = today;
            }

            // Se c'è data Inizio Sospensione ma non Fine Sospensione, usa data attuale
            if (record.DataInizioSospensione.HasValue && !record.DataFineSospensione.HasValue)
            {
                record.DataFineSospensione = today;
            }
        }

        private void CalculateSlaViolations(SlaRecord record, SlaSetup slaSetup, OrariLavorativiSetup clienteSetup)
        {
            try
            {
                var regola = _slaSetupService.FindRegolaByPriorita(slaSetup, record.Priorita);
                if (regola == null)
                {
                    record.TMCFuoriSLA = "Priorità non trovata";
                    record.TEFFFuoriSLA = "Priorità non trovata";
                    return;
                }

                // Calcola violazione TMC
                if (record.TmcTimeSpan.HasValue)
                {
                    // Per ora usiamo un calcolo semplificato - questo metodo dovrebbe essere implementato
                    record.TMCFuoriSLA = CalculateSlaViolationString("TMC", record.TmcTimeSpan.Value, regola, clienteSetup);
                }
                else
                {
                    record.TMCFuoriSLA = "N/D";
                }

                // Calcola violazione T-EFF
                if (record.TeffTimeSpan.HasValue)
                {
                    // Per ora usiamo un calcolo semplificato - questo metodo dovrebbe essere implementato
                    record.TEFFFuoriSLA = CalculateSlaViolationString("T-EFF", record.TeffTimeSpan.Value, regola, clienteSetup);
                }
                else
                {
                    record.TEFFFuoriSLA = "N/D";
                }
            }
            catch (Exception ex)
            {
                record.TMCFuoriSLA = $"Errore: {ex.Message}";
                record.TEFFFuoriSLA = $"Errore: {ex.Message}";
            }
        }

        private void SetRecordAsNotAvailable(SlaRecord record)
        {
            record.TMC = "N/D Cliente";
            record.TMS = "N/D Cliente";
            record.TSOSP = "N/D Cliente";
            record.TEFF = "N/D Cliente";
            record.TMCFuoriSLA = "N/D Setup";
            record.TEFFFuoriSLA = "N/D Setup";
        }

        private void SetRecordAsError(SlaRecord record)
        {
            record.TMC = "Errore";
            record.TMS = "Errore";
            record.TSOSP = "Errore";
            record.TEFF = "Errore";
            record.TMCFuoriSLA = "Errore";
            record.TEFFFuoriSLA = "Errore";
        }

        private string FormatTimeSpanHHMMSS(TimeSpan timeSpan)
        {
            var totalHours = (int)timeSpan.TotalHours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;

            return $"{totalHours:D2}:{minutes:D2}:{seconds:D2}";
        }

        private string CalculateSlaViolationString(string tipoSla, TimeSpan tempoEffettivo, SlaRegola regola, OrariLavorativiSetup clienteSetup)
        {
            try
            {
                // Calcolo semplificato delle soglie SLA (8 ore lavorative per giorno)
                TimeSpan slaThreshold;
                
                if (tipoSla == "TMC")
                {
                    var totalHours = (regola.GiorniPresaInCarico * 8) + regola.OrePresaInCarico;
                    slaThreshold = TimeSpan.FromHours(totalHours);
                }
                else if (tipoSla == "T-EFF")
                {
                    var totalHours = (regola.GiorniRisoluzione * 8) + regola.OreRisoluzione;
                    slaThreshold = TimeSpan.FromHours(totalHours);
                }
                else
                {
                    return "Tipo SLA non supportato";
                }

                // Confronta il tempo effettivo con la soglia SLA
                if (tempoEffettivo <= slaThreshold)
                {
                    return "Entro SLA";
                }
                else
                {
                    var differenza = tempoEffettivo - slaThreshold;
                    var totalHours = (int)differenza.TotalHours;
                    var minutes = differenza.Minutes;
                    var seconds = differenza.Seconds;
                    return $"+{totalHours:D2}:{minutes:D2}:{seconds:D2}";
                }
            }
            catch (Exception ex)
            {
                return $"Errore: {ex.Message}";
            }
        }
    }
}