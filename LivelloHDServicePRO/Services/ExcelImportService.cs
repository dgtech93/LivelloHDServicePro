using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Services
{
    public class ExcelImportService
    {
        public List<ExcelColumnInfo> GetExcelColumns(string filePath)
        {
            var columns = new List<ExcelColumnInfo>();
            
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int colCount = worksheet.Dimension.Columns;

                for (int col = 1; col <= colCount; col++)
                {
                    var headerValue = GetCellValue(worksheet, 1, col);
                    var sampleValue = worksheet.Dimension.Rows > 1 ? GetCellValue(worksheet, 2, col) : "";

                    columns.Add(new ExcelColumnInfo
                    {
                        Index = col,
                        Name = string.IsNullOrWhiteSpace(headerValue) ? $"Colonna {col}" : headerValue,
                        SampleValue = sampleValue.Length > 50 ? sampleValue.Substring(0, 47) + "..." : sampleValue
                    });
                }
            }

            return columns;
        }

        public List<SlaRecord> ImportFromExcel(string filePath, List<ColumnMappingModel> columnMappings)
        {
            var records = new List<SlaRecord>();
            
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                // Skip header row (row 1)
                for (int row = 2; row <= rowCount; row++)
                {
                    var record = new SlaRecord();

                    foreach (var mapping in columnMappings.Where(m => m.ExcelColumnIndex > 0))
                    {
                        if (mapping.IsDateField)
                        {
                            var dateValue = GetDateValue(worksheet, row, mapping.ExcelColumnIndex);
                            SetPropertyValue(record, mapping.FieldName, dateValue);
                        }
                        else
                        {
                            var stringValue = GetCellValue(worksheet, row, mapping.ExcelColumnIndex);
                            SetPropertyValue(record, mapping.FieldName, stringValue);
                        }
                    }

                    records.Add(record);
                }
            }

            return records;
        }

        public List<SlaRecord> ImportFromExcel(string filePath)
        {
            // Fallback method using default column order
            var records = new List<SlaRecord>();
            
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                // Skip header row (row 1)
                for (int row = 2; row <= rowCount; row++)
                {
                    var record = new SlaRecord
                    {
                        Proprietario = GetCellValue(worksheet, row, 1),
                        NumeroCaso = GetCellValue(worksheet, row, 2),
                        Titolo = GetCellValue(worksheet, row, 3),
                        DataCreazione = GetDateValue(worksheet, row, 4),
                        Priorita = GetCellValue(worksheet, row, 5),
                        TipoCaso = GetCellValue(worksheet, row, 6),
                        Descrizione = GetCellValue(worksheet, row, 7),
                        NoteChiusura = GetCellValue(worksheet, row, 8),
                        MotivoStato = GetCellValue(worksheet, row, 9),
                        Contatto = GetCellValue(worksheet, row, 10),
                        DataPresaInCarico = GetDateValue(worksheet, row, 11),
                        DataInizioSospensione = GetDateValue(worksheet, row, 12),
                        DataFineSospensione = GetDateValue(worksheet, row, 13),
                        DataChiusura = GetDateValue(worksheet, row, 14),
                        DataValidazione = GetDateValue(worksheet, row, 15),
                        RoadmapAssociata = GetCellValue(worksheet, row, 16),
                        StatoImpegnoAttivita = GetCellValue(worksheet, row, 17),
                        DataScadenzaAttivita = GetDateValue(worksheet, row, 18),
                        RilascioRoadmapInTest = GetCellValue(worksheet, row, 19),
                        RilascioRoadmapInProduzione = GetCellValue(worksheet, row, 20)
                    };

                    records.Add(record);
                }
            }

            return records;
        }

        private void SetPropertyValue(SlaRecord record, string propertyName, object? value)
        {
            var property = typeof(SlaRecord).GetProperty(propertyName);
            if (property == null) return;

            if (value == null)
            {
                property.SetValue(record, null);
                return;
            }

            if (property.PropertyType == typeof(string))
            {
                property.SetValue(record, value.ToString());
            }
            else if (property.PropertyType == typeof(DateTime?) && value is DateTime dateValue)
            {
                property.SetValue(record, dateValue);
            }
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            if (col <= 0 || col > worksheet.Dimension.Columns || row <= 0 || row > worksheet.Dimension.Rows)
                return string.Empty;

            var cellValue = worksheet.Cells[row, col].Value;
            return cellValue?.ToString() ?? string.Empty;
        }

        private DateTime? GetDateValue(ExcelWorksheet worksheet, int row, int col)
        {
            if (col <= 0 || col > worksheet.Dimension.Columns || row <= 0 || row > worksheet.Dimension.Rows)
                return null;

            var cellValue = worksheet.Cells[row, col].Value;
            
            if (cellValue == null)
                return null;

            if (cellValue is DateTime dateTime)
                return dateTime;

            if (DateTime.TryParse(cellValue.ToString(), out DateTime parsedDate))
                return parsedDate;

            return null;
        }
    }
}