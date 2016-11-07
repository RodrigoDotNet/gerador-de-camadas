using System.Drawing;
using DataDrain.Rules.Interfaces;

namespace DataDrain.Rules.SuportObjects
{
    public sealed class ProviderInfo
    {
        public string MinimalDatabaseVersion { get; set; }

        public string ProviderVersion { get; set; }

        public string TableMapping { get; set; }

        public string ViewMapping { get; set; }

        public string StoredProcedureMapping { get; set; }

        public IInformationSchema Provider { get; set; }
        public Image ImageLogo { get; set; }
    }
}
