using System;
using System.Collections.Generic;
using System.Linq;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Services
{
    public class AutoMappingService
    {
        private static readonly Dictionary<string, List<string>> FieldKeywords = new()
        {
            [nameof(SlaRecord.Proprietario)] = new() { "proprietario", "owner", "assegnato", "responsabile", "assigned", "assignee" },
            [nameof(SlaRecord.NumeroCaso)] = new() { "numero", "caso", "ticket", "id", "case", "num", "codice", "code" },
            [nameof(SlaRecord.Titolo)] = new() { "titolo", "title", "oggetto", "subject", "summary", "riassunto" },
            [nameof(SlaRecord.DataCreazione)] = new() { "creazione", "created", "apertura", "data", "opening", "opened", "create" },
            [nameof(SlaRecord.Priorita)] = new() { "priorità", "priority", "urgenza", "importance", "livello" },
            [nameof(SlaRecord.TipoCaso)] = new() { "tipo", "type", "categoria", "category", "kind", "class" },
            [nameof(SlaRecord.Descrizione)] = new() { "descrizione", "description", "dettaglio", "details", "note", "corpo" },
            [nameof(SlaRecord.NoteChiusura)] = new() { "note", "chiusura", "closure", "risoluzione", "resolution", "close" },
            [nameof(SlaRecord.MotivoStato)] = new() { "motivo", "stato", "status", "reason", "causa", "cause" },
            [nameof(SlaRecord.Contatto)] = new() { "contatto", "contact", "richiedente", "utente", "user", "cliente", "customer" },
            [nameof(SlaRecord.DataPresaInCarico)] = new() { "presa", "carico", "assigned", "assegnazione", "inizio", "start" },
            [nameof(SlaRecord.DataInizioSospensione)] = new() { "inizio", "sospensione", "suspend", "start", "pausa", "pause" },
            [nameof(SlaRecord.DataFineSospensione)] = new() { "fine", "sospensione", "suspend", "end", "ripresa", "resume" },
            [nameof(SlaRecord.DataChiusura)] = new() { "chiusura", "closed", "risolto", "resolved", "fine", "end", "complete" },
            [nameof(SlaRecord.DataValidazione)] = new() { "validazione", "validation", "approvazione", "approval", "verifica" },
            [nameof(SlaRecord.RoadmapAssociata)] = new() { "roadmap", "associata", "piano", "plan", "progetto", "project" },
            [nameof(SlaRecord.StatoImpegnoAttivita)] = new() { "stato", "impegno", "attività", "activity", "task", "work" },
            [nameof(SlaRecord.DataScadenzaAttivita)] = new() { "scadenza", "attività", "deadline", "due", "termine", "limit" },
            [nameof(SlaRecord.RilascioRoadmapInTest)] = new() { "test", "rilascio", "roadmap", "release", "deploy" },
            [nameof(SlaRecord.RilascioRoadmapInProduzione)] = new() { "produzione", "rilascio", "roadmap", "production", "release", "deploy" }
        };

        public List<ColumnMappingModel> AutoMapColumns(List<ExcelColumnInfo> excelColumns)
        {
            var mappings = new List<ColumnMappingModel>();

            foreach (var fieldName in FieldKeywords.Keys)
            {
                var mapping = new ColumnMappingModel
                {
                    FieldName = fieldName,
                    DisplayName = GetDisplayName(fieldName),
                    IsRequired = IsRequiredField(fieldName),
                    IsDateField = IsDateField(fieldName),
                    ExcelColumnIndex = -1
                };

                // Find matching Excel column
                var matchingColumn = FindBestMatch(fieldName, excelColumns);
                if (matchingColumn != null)
                {
                    mapping.ExcelColumnIndex = matchingColumn.Index;
                    mapping.ExcelColumnName = matchingColumn.Name;
                }

                mappings.Add(mapping);
            }

            return mappings;
        }

        private ExcelColumnInfo? FindBestMatch(string fieldName, List<ExcelColumnInfo> excelColumns)
        {
            var keywords = FieldKeywords[fieldName];
            ExcelColumnInfo? bestMatch = null;
            int bestScore = 0;

            foreach (var column in excelColumns)
            {
                var columnName = column.Name.ToLower().Trim();
                int score = 0;

                // Exact match gets highest score
                foreach (var keyword in keywords)
                {
                    if (columnName.Equals(keyword.ToLower()))
                    {
                        return column; // Exact match, return immediately
                    }

                    // Partial match scoring
                    if (columnName.Contains(keyword.ToLower()))
                    {
                        score += keyword.Length; // Longer keywords get higher scores
                    }

                    // Check if keyword is contained in column name
                    if (keyword.ToLower().Contains(columnName) && columnName.Length > 2)
                    {
                        score += columnName.Length / 2;
                    }
                }

                // Bonus for common patterns
                if (IsDateField(fieldName) && (columnName.Contains("data") || columnName.Contains("date")))
                {
                    score += 5;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = column;
                }
            }

            return bestScore > 0 ? bestMatch : null;
        }

        private static string GetDisplayName(string fieldName)
        {
            return fieldName switch
            {
                nameof(SlaRecord.Proprietario) => "Proprietario",
                nameof(SlaRecord.NumeroCaso) => "Numero Caso",
                nameof(SlaRecord.Titolo) => "Titolo",
                nameof(SlaRecord.DataCreazione) => "Data Creazione",
                nameof(SlaRecord.Priorita) => "Priorità",
                nameof(SlaRecord.TipoCaso) => "Tipo Caso",
                nameof(SlaRecord.Descrizione) => "Descrizione",
                nameof(SlaRecord.NoteChiusura) => "Note di Chiusura",
                nameof(SlaRecord.MotivoStato) => "Motivo Stato",
                nameof(SlaRecord.Contatto) => "Contatto",
                nameof(SlaRecord.DataPresaInCarico) => "Data Presa in Carico",
                nameof(SlaRecord.DataInizioSospensione) => "Data Inizio Sospensione",
                nameof(SlaRecord.DataFineSospensione) => "Data Fine Sospensione",
                nameof(SlaRecord.DataChiusura) => "Data Chiusura",
                nameof(SlaRecord.DataValidazione) => "Data Validazione",
                nameof(SlaRecord.RoadmapAssociata) => "Roadmap Associata",
                nameof(SlaRecord.StatoImpegnoAttivita) => "Stato Impegno Attività",
                nameof(SlaRecord.DataScadenzaAttivita) => "Data Scadenza Attività",
                nameof(SlaRecord.RilascioRoadmapInTest) => "Rilascio Roadmap in Test",
                nameof(SlaRecord.RilascioRoadmapInProduzione) => "Rilascio Roadmap in Produzione",
                _ => fieldName
            };
        }

        private static bool IsRequiredField(string fieldName)
        {
            return fieldName is nameof(SlaRecord.NumeroCaso) or 
                               nameof(SlaRecord.Titolo) or 
                               nameof(SlaRecord.DataCreazione);
        }

        private static bool IsDateField(string fieldName)
        {
            return fieldName.Contains("Data");
        }
    }
}