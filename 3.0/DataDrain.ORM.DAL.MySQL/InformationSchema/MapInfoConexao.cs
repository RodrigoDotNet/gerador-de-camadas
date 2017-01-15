using DataDrain.ORM.Interfaces;

namespace DataDrain.ORM.DAL.MySQL.InformationSchema
{
    public sealed class MapInfoConexao : IInfoConexao
    {
        /// <summary>
        /// Porta padrão de conexão
        /// </summary>
        public int PortaPadrao { get { return 3306; } }

        /// <summary>
        /// Versão minima recomendada do Banco de dados
        /// </summary>
        public string VersaoMinima { get { return "MySQL 5.1"; } }

        /// <summary>
        /// Informa se o banco suporta Trusted Connection
        /// </summary>
        public bool TrustedConnection { get { return false; } }
    }
}
