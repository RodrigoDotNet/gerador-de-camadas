using DataDrain.Rules.Interfaces;

namespace DataDrain.Databases.SqlServer
{
    public sealed class MapInfoConexao : IInfoConnection
    {
        /// <summary>
        /// Porta padrão de conexão
        /// </summary>
        public int DefaultPort { get { return 1433; } }

        /// <summary>
        /// Versão minima recomendada do Banco de dados
        /// </summary>
        public string MinimalVersion { get { return "SQL Server 2008"; } }

        /// <summary>
        /// Informa se o banco suporta Trusted Connection
        /// </summary>
        public bool IsTrustedConnection { get { return true; } }
    }
}
