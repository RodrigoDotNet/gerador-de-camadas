using System.Collections.Generic;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.Rules.Interfaces
{
    public interface IInformationSchemaView
    {
        List<DatabaseObjectMap> ListAllViews(string dataBaseName, DatabaseUser usr);

        List<ColumnInfo> ListAllFieldsFromViews(string dataBaseName, string viewName, DatabaseUser usr);
    }
}
