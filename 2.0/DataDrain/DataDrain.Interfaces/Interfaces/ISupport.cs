using DataDrain.Rules.SuportObjects;

namespace DataDrain.Rules.Interfaces
{
    public interface ISupport
    {
        string DataTypeNetFramework(string dataType);

        string ConvertTypeClrToSql(string dataType);

        void CreateProjectFiles(Configuration parameters, ITemplateText dictionaryTemplates);
    }
}
