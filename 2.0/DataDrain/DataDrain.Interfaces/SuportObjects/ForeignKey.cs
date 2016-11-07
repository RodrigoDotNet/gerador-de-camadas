namespace DataDrain.Rules.SuportObjects
{
    public sealed class ForeignKey
    {
        public string TableName { get; set; }

        public string Column { get; set; }

        public string TableFk { get; set; }

        public string ColumnFk { get; set; }

        public string Constraint { get; set; }
    }
}
