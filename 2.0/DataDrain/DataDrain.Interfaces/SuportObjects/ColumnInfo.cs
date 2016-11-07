using DataDrain.Rules.Enuns;

namespace DataDrain.Rules.SuportObjects
{
    public sealed class ColumnInfo
    {
        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public string Type { get; set; }

        public int Size { get; set; }

        public bool IsNullability { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsIdentity { get; set; }

        public string RegularExpression { get; set; }

        public string DefaultValue { get; set; }

        public bool IsSync { get; set; }
        
        public EColumnSync ColumnSync { get; set; }

    }
}