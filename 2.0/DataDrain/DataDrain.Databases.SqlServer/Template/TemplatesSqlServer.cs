using System;
using System.Collections.Generic;
using DataDrain.Rules.Interfaces;

namespace DataDrain.Databases.SqlServer.Template
{
    public sealed class TemplatesSqlServer : ITemplateText
    {
        public Dictionary<string,string> ListAllValues()
        {
            throw new NotImplementedException();
        }

        public KeyValuePair<string, string> GelValue(string key)
        {
            throw new NotImplementedException();
        }
    }
}
