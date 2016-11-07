using System.Collections.Generic;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.Rules.Interfaces
{
    public interface IInformationSchema
    {
        List<string> ListAllDatabases(DatabaseUser usr);

        KeyValuePair<bool, string> TestConnection(DatabaseUser usr);

        IInformationSchemaTable TableMapping { get; }

        IInformationSchemaView ViewMapping { get; }

        IInformationSchemaProcedure StoredProcedureMapping { get; }

        IInfoConnection InfoConnection { get; }

        ISupport Support { get; }

        ITemplateText DictionaryOfTemplates { get; }

        bool IsTableMapping { get; }

        bool IsViewMapping { get; }

        bool IsStoredProcedureMapping { get; }

        Dictionary<string, string> AdoNetConnectionObjects { get; }

        List<ColumnInfo> MapQuery(string sql, List<StoredProcedureParameter> parameters, DatabaseUser usr);
    }
}
