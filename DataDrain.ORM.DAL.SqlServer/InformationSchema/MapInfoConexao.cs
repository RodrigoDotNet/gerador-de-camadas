using DataDrain.ORM.Interfaces;

namespace DataDrain.ORM.DAL.SqlServer.InformationSchema
{
    public sealed class MapInfoConexao : IInfoConexao
    {
        /// <summary>
        /// Porta padrão de conexão
        /// </summary>
        public int PortaPadrao { get { return 1433; } }

        /// <summary>
        /// Versão minima recomendada do Banco de dados
        /// </summary>
        public string VersaoMinima { get { return "SQL Server 2008"; } }

        /// <summary>
        /// Informa se o banco suporta Trusted Connection
        /// </summary>
        public bool TrustedConnection { get { return true; } }
    }
}
