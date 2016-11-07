using System.Collections.Generic;

namespace DataDrain.Rules.SuportObjects
{
    public sealed class Configuration
    {
        public Configuration()
        {
            VersaoFramework = "v4.5";
            TiposObjetosAcaoBanco = new Dictionary<string, string>();
            ObjetosMapeaveis = new List<DatabaseObjectInfo>();
        }

        public string DestinationPath { get; set; }
        public DatabaseUser User { get; set; }
        public List<DatabaseObjectInfo> ObjetosMapeaveis { get; set; }
        public string NameSpace { get; set; }
        public bool GerarAppConfig { get; set; }
        public bool AssinarProjeto { get; set; }
        public string CaminhoStrongName { get; set; }
        public string VersaoFramework { get; set; }
        public bool MapWcf { get; set; }
        public bool MapLinq { get; set; }
        public Dictionary<string, string> TiposObjetosAcaoBanco { get; set; }
    }
}
