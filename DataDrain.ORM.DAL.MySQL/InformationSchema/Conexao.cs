using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;
using MySql.Data.MySqlClient;

namespace DataDrain.ORM.DAL.MySQL.InformationSchema
{
    internal static class Conexao
    {
        public static MySqlConnection RetornaConexaoBase(DadosUsuario usr)
        {
            return new MySqlConnection(string.Format("Data Source={0};Initial Catalog={1};UID={2}; Password={3};Port={4};", usr.Servidor, usr.DataBase, usr.Usuario, usr.Senha.ConvertToString(), usr.Porta));
        }
            
    }
}
