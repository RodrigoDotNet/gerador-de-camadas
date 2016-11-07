using System;
using DataDrain.Rules.Enuns;

namespace DataDrain.Rules.SuportObjects
{
    public sealed class DatabaseObjectMap
    {
        public EDatabaseObjectType Type { get; set; }

        public string Name { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime? ChangeDate { get; set; }

        public int Records { get; set; }
    }
}