using System.Collections.Generic;

namespace LivelloHDServicePRO.Models
{
    public class ColumnMappingModel
    {
        public string FieldName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int ExcelColumnIndex { get; set; } = -1;
        public string ExcelColumnName { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsDateField { get; set; }
    }

    public class ExcelColumnInfo
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SampleValue { get; set; } = string.Empty;
    }
}