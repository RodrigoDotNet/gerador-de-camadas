using DataDrain.Rules.Interfaces;

namespace DataDrain.Databases.MySql
{
    public sealed class MapInfoConexao : IInfoConnection
    {
        /// <summary>
        /// Porta padrão de conexão
        /// </summary>
        public int DefaultPort { get { return 3306; } }

        /// <summary>
        /// Versão minima recomendada do Banco de dados
        /// </summary>
        public string MinimalVersion { get { return "MySQL 5.1"; } }

        /// <summary>
        /// Informa se o banco suporta Trusted Connection
        /// </summary>
        public bool IsTrustedConnection { get { return false; } }
    }
}
