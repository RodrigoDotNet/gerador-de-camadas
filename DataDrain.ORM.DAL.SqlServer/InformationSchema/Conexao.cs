using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;
using System.Data.SqlClient;

namespace DataDrain.ORM.DAL.SqlServer.InformationSchema
{
    internal sealed class Conexao
    {
        private const string StringConnection = "Data Source={0};Database={1};UID={2}; Password={3}";

        private const string TrustedStringConnection = "Data Source={0};Database={1};Trusted_Connection=True;";

        public static SqlConnection RetornaConexaoBase(DadosUsuario usr)
        {
            return usr.TrustedConnection
                ? new SqlConnection(string.Format(TrustedStringConnection, usr.Servidor, usr.DataBase))
                : new SqlConnection(string.Format(StringConnection, usr.Servidor, usr.DataBase, usr.Usuario, usr.Senha.ConvertToString()));

        }
    }
}
