using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Services
{
    public class ExcelExportService
    {
        public void ExportToExcel(List<SlaRecord> records, List<ColumnVisibilityModel> columnVisibilities, string filePath)
        {
            if (records == null || !records.Any())
            {
                throw new ArgumentException("Nessun record da esportare.");
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("SLA Export");

            // Ottieni le colonne visibili
            var visibleColumns = GetVisibleColumns(columnVisibilities);
            
            // Imposta le intestazioni
            SetupHeaders(worksheet, visibleColumns);
            
            // Aggiungi i dati
            AddDataRows(worksheet, records, visibleColumns);
            
            // Formatta il foglio
            FormatWorksheet(worksheet, visibleColumns.Count);
            
            // Salva il file
            workbook.SaveAs(filePath);
        }

        private List<ColumnInfo> GetVisibleColumns(List<ColumnVisibilityModel> columnVisibilities)
        {
            var allColumns = new List<ColumnInfo>
            {
                // Colonne originali
                new("Proprietario", nameof(SlaRecord.Proprietario), "Proprietario"),
                new("Numero Caso", nameof(SlaRecord.NumeroCaso), "Numero Caso"),
                new("Titolo", nameof(SlaRecord.Titolo), "Titolo"),
                new("Data Creazione", nameof(SlaRecord.DataCreazione), "Data Creazione"),
                new("Data Presa in Carico", nameof(SlaRecord.DataPresaInCarico), "Data Presa in Carico"),
                new("Data Inizio Sospensione", nameof(SlaRecord.DataInizioSospensione), "Data Inizio Sospensione"),
                new("Data Fine Sospensione", nameof(SlaRecord.DataFineSospensione), "Data Fine Sospensione"),
                new("Data Chiusura", nameof(SlaRecord.DataChiusura), "Data Chiusura"),
                new("Priorità", nameof(SlaRecord.Priorita), "Priorità"),
                new("Tipo Caso", nameof(SlaRecord.TipoCaso), "Tipo Caso"),
                new("Descrizione", nameof(SlaRecord.Descrizione), "Descrizione"),
                new("Motivo Stato", nameof(SlaRecord.MotivoStato), "Motivo Stato"),
                new("Contatto", nameof(SlaRecord.Contatto), "Contatto"),
                new("Roadmap Associata", nameof(SlaRecord.RoadmapAssociata), "Roadmap Associata"),
                new("Stato Impegno Attività", nameof(SlaRecord.StatoImpegnoAttivita), "Stato Impegno Attività"),
                
                // Colonne SLA calcolate (sempre alla fine)
                new("TMC", nameof(SlaRecord.TMC), "TMC"),
                new("TMS", nameof(SlaRecord.TMS), "TMS"),
                new("TSOSP", nameof(SlaRecord.TSOSP), "TSOSP"),
                new("T-EFF", nameof(SlaRecord.TEFF), "T-EFF"),
                new("TMC Fuori SLA", nameof(SlaRecord.TMCFuoriSLA), "TMC Fuori SLA"),
                new("T-EFF Fuori SLA", nameof(SlaRecord.TEFFFuoriSLA), "T-EFF Fuori SLA")
            };

            // Filtra solo le colonne visibili
            if (columnVisibilities != null && columnVisibilities.Any())
            {
                var visibleColumnNames = columnVisibilities
                    .Where(c => c.IsVisible)
                    .Select(c => c.DisplayName)
                    .ToHashSet();

                return allColumns.Where(col => 
                    visibleColumnNames.Contains(col.DisplayName) || 
                    IsSlaCalculatedColumn(col.PropertyName)).ToList();
            }

            return allColumns;
        }

        private bool IsSlaCalculatedColumn(string propertyName)
        {
            var slaColumns = new[] { 
                nameof(SlaRecord.TMC), 
                nameof(SlaRecord.TMS), 
                nameof(SlaRecord.TSOSP), 
                nameof(SlaRecord.TEFF),
                nameof(SlaRecord.TMCFuoriSLA),
                nameof(SlaRecord.TEFFFuoriSLA)
            };
            
            return slaColumns.Contains(propertyName);
        }

        private void SetupHeaders(IXLWorksheet worksheet, List<ColumnInfo> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = columns[i].DisplayName;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }

        private void AddDataRows(IXLWorksheet worksheet, List<SlaRecord> records, List<ColumnInfo> columns)
        {
            for (int rowIndex = 0; rowIndex < records.Count; rowIndex++)
            {
                var record = records[rowIndex];
                var excelRow = rowIndex + 2; // +2 perché Excel inizia da 1 e la riga 1 è l'header

                for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                {
                    var column = columns[colIndex];
                    var cell = worksheet.Cell(excelRow, colIndex + 1);
                    
                    var value = GetPropertyValue(record, column.PropertyName);
                    SetCellValue(cell, value, column.PropertyName);
                    
                    // Applica formattazione speciale per colonne SLA
                    ApplySlaFormatting(cell, value, column.PropertyName);
                }
            }
        }

        private object? GetPropertyValue(SlaRecord record, string propertyName)
        {
            var property = typeof(SlaRecord).GetProperty(propertyName);
            return property?.GetValue(record);
        }

        private void SetCellValue(IXLCell cell, object? value, string propertyName)
        {
            if (value == null)
            {
                cell.Value = "";
                return;
            }

            // Gestione speciale per le date
            if (value is DateTime dateTime)
            {
                cell.Value = dateTime;
                cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            }
            else
            {
                cell.Value = value.ToString();
            }
        }

        private void ApplySlaFormatting(IXLCell cell, object? value, string propertyName)
        {
            if (value == null) return;

            var stringValue = value.ToString();
            if (string.IsNullOrWhiteSpace(stringValue)) return;

            // Formattazione per colonne SLA Fuori SLA
            if (propertyName == nameof(SlaRecord.TMCFuoriSLA))
            {
                if (stringValue.StartsWith("+"))
                {
                    // TMC fuori SLA - Rosso
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 205, 210);
                    cell.Style.Font.FontColor = XLColor.FromArgb(183, 28, 28);
                    cell.Style.Font.Bold = true;
                }
                else if (stringValue.Contains("Entro SLA", StringComparison.OrdinalIgnoreCase))
                {
                    // Entro SLA - Verde
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(200, 230, 201);
                    cell.Style.Font.FontColor = XLColor.FromArgb(27, 94, 32);
                    cell.Style.Font.Bold = true;
                }
            }
            else if (propertyName == nameof(SlaRecord.TEFFFuoriSLA))
            {
                if (stringValue.StartsWith("+"))
                {
                    // T-EFF fuori SLA - Arancione
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 224, 178);
                    cell.Style.Font.FontColor = XLColor.FromArgb(230, 81, 0);
                    cell.Style.Font.Bold = true;
                }
                else if (stringValue.Contains("Entro SLA", StringComparison.OrdinalIgnoreCase))
                {
                    // Entro SLA - Verde
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(200, 230, 201);
                    cell.Style.Font.FontColor = XLColor.FromArgb(27, 94, 32);
                    cell.Style.Font.Bold = true;
                }
            }
            // Formattazione per colonne SLA standard
            else if (IsSlaCalculatedColumn(propertyName))
            {
                cell.Style.Font.FontName = "Consolas";
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
        }

        private void FormatWorksheet(IXLWorksheet worksheet, int columnCount)
        {
            // Auto-fit colonne
            worksheet.Columns(1, columnCount).AdjustToContents();
            
            // Imposta altezza minima per le righe
            worksheet.Rows().Height = 20;
            
            // Congela la prima riga (header)
            worksheet.SheetView.FreezeRows(1);
            
            // Applica bordi a tutta la tabella
            var dataRange = worksheet.Range(1, 1, worksheet.LastRowUsed().RowNumber(), columnCount);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;
        }

        public string GetSuggestedFileName()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"SLA_Export_{timestamp}.xlsx";
        }
    }

    public record ColumnInfo(string DisplayName, string PropertyName, string ExportHeader);
}