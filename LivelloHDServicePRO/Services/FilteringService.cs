using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Services
{
    public class FilteringService
    {
        public static List<SlaRecord> ApplyFilters(
            List<SlaRecord> originalData,
            List<FilterModel> columnFilters,
            string quickFilterId = "none",
            string textFilter = "")
        {
            var filteredData = originalData.AsEnumerable();

            // Applica filtro testo rapido
            if (!string.IsNullOrWhiteSpace(textFilter))
            {
                filteredData = ApplyTextFilter(filteredData, textFilter);
            }

            // ?? APPLICA FILTRI PER COLONNA CON LOGICA OR PER STESSA PROPRIETÀ
            filteredData = ApplyColumnFiltersWithProperLogic(filteredData, columnFilters.Where(f => f.IsValid));

            // Applica filtro veloce migliorato
            filteredData = ApplyQuickFilter(filteredData, quickFilterId);

            return filteredData.ToList();
        }

        /// <summary>
        /// Applica i filtri per colonna con logica corretta:
        /// - Filtri per colonne diverse = AND (restrittivo)
        /// - Filtri per stessa colonna = OR (espansivo)
        /// </summary>
        private static IEnumerable<SlaRecord> ApplyColumnFiltersWithProperLogic(IEnumerable<SlaRecord> data, IEnumerable<FilterModel> filters)
        {
            var filtersList = filters.ToList();
            if (!filtersList.Any()) return data;

            // Raggruppa i filtri per proprietà
            var filtersByProperty = filtersList.GroupBy(f => f.SelectedProperty).ToList();

            System.Diagnostics.Debug.WriteLine($"?? Applicazione filtri:");
            System.Diagnostics.Debug.WriteLine($"   ?? Proprietà con filtri: {filtersByProperty.Count}");
            foreach (var group in filtersByProperty)
            {
                System.Diagnostics.Debug.WriteLine($"   • {FilterService.GetPropertyDisplayName(group.Key)}: {group.Count()} filtri");
            }

            return data.Where(record =>
            {
                // Per ogni gruppo di proprietà, applica logica OR
                // Tra proprietà diverse, applica logica AND
                return filtersByProperty.All(propertyGroup =>
                {
                    var propertyName = propertyGroup.Key;
                    var filtersForProperty = propertyGroup.ToList();

                    // All'interno della stessa proprietà: OR
                    var matchesAnyFilter = filtersForProperty.Any(filter =>
                        EvaluateFilterCondition(record, filter));

                    if (filtersForProperty.Count > 1)
                    {
                        System.Diagnostics.Debug.WriteLine($"      ?? OR logic per {propertyName}: {matchesAnyFilter}");
                    }

                    return matchesAnyFilter;
                });
            });
        }

        private static IEnumerable<SlaRecord> ApplyTextFilter(IEnumerable<SlaRecord> data, string textFilter)
        {
            var filterLower = textFilter.ToLowerInvariant();
            
            return data.Where(record =>
                (record.Proprietario?.ToLowerInvariant().Contains(filterLower) ?? false) ||
                (record.NumeroCaso?.ToLowerInvariant().Contains(filterLower) ?? false) ||
                (record.Titolo?.ToLowerInvariant().Contains(filterLower) ?? false) ||
                (record.Priorita?.ToLowerInvariant().Contains(filterLower) ?? false) ||
                (record.TipoCaso?.ToLowerInvariant().Contains(filterLower) ?? false) ||
                (record.Descrizione?.ToLowerInvariant().Contains(filterLower) ?? false) ||
                (record.MotivoStato?.ToLowerInvariant().Contains(filterLower) ?? false) ||
                (record.Contatto?.ToLowerInvariant().Contains(filterLower) ?? false));
        }

        private static IEnumerable<SlaRecord> ApplyAdvancedColumnFilter(IEnumerable<SlaRecord> data, FilterModel filter)
        {
            return data.Where(record => EvaluateFilterCondition(record, filter));
        }

        private static bool EvaluateFilterCondition(SlaRecord record, FilterModel filter)
        {
            var propertyValue = GetPropertyValue(record, filter.SelectedProperty);

            return filter.FilterOperator switch
            {
                "equals" => propertyValue?.Equals(filter.SelectedValue, StringComparison.OrdinalIgnoreCase) ?? false,
                "not_equals" => !string.Equals(propertyValue, filter.SelectedValue, StringComparison.OrdinalIgnoreCase),
                "contains" => propertyValue?.Contains(filter.SelectedValue, StringComparison.OrdinalIgnoreCase) ?? false,
                "wildcard" => EvaluateWildcardPattern(propertyValue, filter.SelectedValue),
                "empty" => string.IsNullOrWhiteSpace(propertyValue),
                "not_empty" => !string.IsNullOrWhiteSpace(propertyValue),
                "multiple" => EvaluateMultipleValues(propertyValue, filter.ParsedValues),
                "greater" => CompareValues(propertyValue, filter.SelectedValue, ">"),
                "less" => CompareValues(propertyValue, filter.SelectedValue, "<"),
                "date_from" => CompareDates(record, filter.SelectedProperty, filter.SelectedValue, ">="),
                "date_to" => CompareDates(record, filter.SelectedProperty, filter.SelectedValue, "<="),
                _ => false
            };
        }

        private static bool EvaluateWildcardPattern(string? propertyValue, string pattern)
        {
            if (string.IsNullOrWhiteSpace(propertyValue) || string.IsNullOrWhiteSpace(pattern))
                return false;

            // Se non contiene *, tratta come equals
            if (!pattern.Contains('*'))
                return propertyValue.Equals(pattern, StringComparison.OrdinalIgnoreCase);

            var propValue = propertyValue.ToLowerInvariant();
            var patternLower = pattern.ToLowerInvariant();

            // Pattern: *testo (finisce con)
            if (patternLower.StartsWith('*') && !patternLower.EndsWith('*'))
            {
                var suffix = patternLower.Substring(1);
                return propValue.EndsWith(suffix);
            }
            
            // Pattern: testo* (inizia con)
            if (!patternLower.StartsWith('*') && patternLower.EndsWith('*'))
            {
                var prefix = patternLower.Substring(0, patternLower.Length - 1);
                return propValue.StartsWith(prefix);
            }
            
            // Pattern: *testo* (contiene)
            if (patternLower.StartsWith('*') && patternLower.EndsWith('*'))
            {
                var middle = patternLower.Substring(1, patternLower.Length - 2);
                return propValue.Contains(middle);
            }

            // Pattern complesso con più * - implementazione semplificata
            // Sostituisce * con regex equivalente
            var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(patternLower)
                                        .Replace("\\*", ".*") + "$";
            
            try
            {
                return System.Text.RegularExpressions.Regex.IsMatch(propValue, regexPattern, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            catch
            {
                // Fallback su contains se regex fallisce
                return propValue.Contains(patternLower.Replace("*", ""));
            }
        }

        private static bool EvaluateMultipleValues(string? propertyValue, List<string> filterValues)
        {
            if (string.IsNullOrWhiteSpace(propertyValue) || !filterValues.Any()) 
                return false;

            return filterValues.Any(filterValue => 
            {
                // Se il singolo valore contiene *, usa il wildcard
                if (filterValue.Contains('*'))
                {
                    return EvaluateWildcardPattern(propertyValue, filterValue);
                }
                // Altrimenti confronto normale
                else
                {
                    return propertyValue.Equals(filterValue, StringComparison.OrdinalIgnoreCase);
                }
            });
        }

        private static bool CompareValues(string? propertyValue, string filterValue, string operation)
        {
            if (string.IsNullOrWhiteSpace(propertyValue) || string.IsNullOrWhiteSpace(filterValue))
                return false;

            // Prova prima confronto numerico
            if (double.TryParse(propertyValue, out double propNum) && 
                double.TryParse(filterValue, out double filterNum))
            {
                return operation switch
                {
                    ">" => propNum > filterNum,
                    "<" => propNum < filterNum,
                    _ => false
                };
            }

            // Fallback a confronto stringa
            var comparison = string.Compare(propertyValue, filterValue, StringComparison.OrdinalIgnoreCase);
            return operation switch
            {
                ">" => comparison > 0,
                "<" => comparison < 0,
                _ => false
            };
        }

        private static bool CompareDates(SlaRecord record, string propertyName, string filterValue, string operation)
        {
            if (!DateTime.TryParse(filterValue, out DateTime filterDate))
                return false;

            var dateValue = GetDatePropertyValue(record, propertyName);
            if (!dateValue.HasValue) return false;

            return operation switch
            {
                ">=" => dateValue.Value.Date >= filterDate.Date,
                "<=" => dateValue.Value.Date <= filterDate.Date,
                _ => false
            };
        }

        private static DateTime? GetDatePropertyValue(SlaRecord record, string propertyName)
        {
            return propertyName switch
            {
                nameof(SlaRecord.DataCreazione) => record.DataCreazione,
                nameof(SlaRecord.DataPresaInCarico) => record.DataPresaInCarico,
                nameof(SlaRecord.DataInizioSospensione) => record.DataInizioSospensione,
                nameof(SlaRecord.DataFineSospensione) => record.DataFineSospensione,
                nameof(SlaRecord.DataChiusura) => record.DataChiusura,
                nameof(SlaRecord.DataValidazione) => record.DataValidazione,
                nameof(SlaRecord.DataScadenzaAttivita) => record.DataScadenzaAttivita,
                _ => null
            };
        }

        private static IEnumerable<SlaRecord> ApplyQuickFilter(IEnumerable<SlaRecord> data, string quickFilterId)
        {
            var now = DateTime.Now;
            var today = now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            return quickFilterId switch
            {
                "not_resolved" => data.Where(r => !r.DataChiusura.HasValue),
                "not_taken" => data.Where(r => !r.DataPresaInCarico.HasValue),
                "out_of_sla" => data.Where(r => IsOutOfSla(r.TMCFuoriSLA) || IsOutOfSla(r.TEFFFuoriSLA)),
                "tmc_out_of_sla" => data.Where(r => IsOutOfSla(r.TMCFuoriSLA)),
                "teff_out_of_sla" => data.Where(r => IsOutOfSla(r.TEFFFuoriSLA)),
                "today" => data.Where(r => r.DataCreazione?.Date == today),
                "this_week" => data.Where(r => r.DataCreazione?.Date >= startOfWeek),
                "this_month" => data.Where(r => r.DataCreazione?.Date >= startOfMonth),
                "suspended" => data.Where(r => r.DataInizioSospensione.HasValue && !r.DataFineSospensione.HasValue),
                _ => data // "none" or any other value
            };
        }

        private static bool IsOutOfSla(string slaValue)
        {
            if (string.IsNullOrWhiteSpace(slaValue)) return false;
            
            // Considera fuori SLA se inizia con '+' (es: "+02:30:00")
            // o se non contiene "Entro SLA"
            return slaValue.StartsWith("+") || 
                   (!slaValue.Contains("Entro SLA", StringComparison.OrdinalIgnoreCase) &&
                    !slaValue.Equals("N/D", StringComparison.OrdinalIgnoreCase) &&
                    !slaValue.Contains("Errore", StringComparison.OrdinalIgnoreCase));
        }

        private static string? GetPropertyValue(SlaRecord record, string propertyName)
        {
            return propertyName switch
            {
                nameof(SlaRecord.Proprietario) => record.Proprietario,
                nameof(SlaRecord.NumeroCaso) => record.NumeroCaso,
                nameof(SlaRecord.Priorita) => record.Priorita,
                nameof(SlaRecord.TipoCaso) => record.TipoCaso,
                nameof(SlaRecord.MotivoStato) => record.MotivoStato,
                nameof(SlaRecord.Contatto) => record.Contatto,
                nameof(SlaRecord.RoadmapAssociata) => record.RoadmapAssociata,
                nameof(SlaRecord.StatoImpegnoAttivita) => record.StatoImpegnoAttivita,
                nameof(SlaRecord.TMC) => record.TMC,
                nameof(SlaRecord.TMS) => record.TMS,
                nameof(SlaRecord.TSOSP) => record.TSOSP,
                nameof(SlaRecord.TEFF) => record.TEFF,
                nameof(SlaRecord.TMCFuoriSLA) => record.TMCFuoriSLA,
                nameof(SlaRecord.TEFFFuoriSLA) => record.TEFFFuoriSLA,
                // Date convertite a stringhe
                nameof(SlaRecord.DataCreazione) => record.DataCreazione?.ToString("dd/MM/yyyy HH:mm"),
                nameof(SlaRecord.DataPresaInCarico) => record.DataPresaInCarico?.ToString("dd/MM/yyyy HH:mm"),
                nameof(SlaRecord.DataInizioSospensione) => record.DataInizioSospensione?.ToString("dd/MM/yyyy HH:mm"),
                nameof(SlaRecord.DataFineSospensione) => record.DataFineSospensione?.ToString("dd/MM/yyyy HH:mm"),
                nameof(SlaRecord.DataChiusura) => record.DataChiusura?.ToString("dd/MM/yyyy HH:mm"),
                nameof(SlaRecord.DataValidazione) => record.DataValidazione?.ToString("dd/MM/yyyy HH:mm"),
                nameof(SlaRecord.DataScadenzaAttivita) => record.DataScadenzaAttivita?.ToString("dd/MM/yyyy HH:mm"),
                _ => null
            };
        }

        public static Dictionary<string, List<string>> GetAvailableValuesForAllProperties(List<SlaRecord> records)
        {
            var result = new Dictionary<string, List<string>>();
            var properties = FilterService.GetFilterableProperties();

            foreach (var property in properties)
            {
                result[property] = FilterService.GetUniqueValues(records, property);
            }

            return result;
        }
    }
}