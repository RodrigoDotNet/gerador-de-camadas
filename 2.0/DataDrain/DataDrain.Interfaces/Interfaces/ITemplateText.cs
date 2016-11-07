using System.Collections.Generic;

namespace DataDrain.Rules.Interfaces
{
    public interface ITemplateText
    {
        Dictionary<string, string> ListAllValues();

        KeyValuePair<string, string> GelValue(string key);
    }
}
