using System.Collections.Generic;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.Rules.Interfaces
{
    public interface IInformationSchemaTable
    {
        List<DatabaseObjectMap> ListAllTables(string dataBaseName, DatabaseUser usr);

        List<ColumnInfo> ListAllFieldsFromTable(string dataBaseName, string tableName, DatabaseUser usr);

        List<ForeignKey> ListForeignKeysTable(string tableName, string dataBaseName, DatabaseUser usr);         
    }
}
