using System.Collections.Generic;
using DataDrain.Rules.Enuns;

namespace DataDrain.Rules.SuportObjects
{
    public sealed class DatabaseObjectInfo
    {
        public DatabaseObjectInfo(string databaseObjectName, EDatabaseObjectType type, List<StoredProcedureParameter> parameters = null, string query = null)
        {
            Name = databaseObjectName;
            StoredProcedureParameters = parameters;
            QuerySql = query;
            DatabaseObjectType = type;
        }

        public string Name { get; private set; }

        public EDatabaseObjectType DatabaseObjectType { get; private set; }

        public List<ForeignKey> ForeignKeys { get; set; }

        public List<StoredProcedureParameter> StoredProcedureParameters { get; private set; }

        public string QuerySql { get; private set; }

        public List<ColumnInfo> Columns { get; set; }

        public void AjustaParametros(List<StoredProcedureParameter> parametros)
        {
            if (parametros != null && parametros.Count > 0)
            {
                StoredProcedureParameters = parametros;
            }
        }
    }
}
