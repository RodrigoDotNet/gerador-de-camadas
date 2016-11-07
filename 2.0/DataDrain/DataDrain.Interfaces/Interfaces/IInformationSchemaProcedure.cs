using System.Collections.Generic;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.Rules.Interfaces
{
    public interface IInformationSchemaProcedure
    {
        List<DatabaseObjectMap> ListAllStoredProcedures(string dataBaseName, DatabaseUser usr);

        List<StoredProcedureParameter> ListAllStoredProceduresParameters(string dataBaseName, DatabaseUser usr, string procedureName);

        List<ColumnInfo> ListAllFieldsFromStoredProcedure(string dataBaseName, string procedureName, List<StoredProcedureParameter> parametros, DatabaseUser usr);
    }
}
