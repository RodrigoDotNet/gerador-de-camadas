using System.Collections.Generic;


namespace DataDrain.ORM.Interfaces.Objetos
{
    public class ParametrosCriarProjetos
    {
        public ParametrosCriarProjetos()
        {
            VersaoFramework = "v4.5";
        }

        public string CaminhoDestino { get; set; }
        public DadosUsuario DadosConexao { get; set; }
        public List<KeyValuePair<TipoObjetoBanco, List<DadosColunas>>> ObjetosMapeaveis { get; set; }
        public string NameSpace { get; set; }
        public bool GerarAppConfig { get; set; }
        public bool AssinarProjeto { get; set; }
        public string CaminhoStrongName { get; set; }
        public string VersaoFramework { get; set; }
        public string XmlLog4Net { get; set; }
        public bool MapWcf { get; set; }
        public bool MapLinq { get; set; }
    }
}
