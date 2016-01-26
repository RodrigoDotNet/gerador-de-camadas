using System;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace Apoio.Conexao
{
    internal static class Singleton
    {
        public static MySqlConnection RetornaConexao()
        {
            return new MySqlConnection(RetornaConnectionString());
        }

        private static string RetornaConnectionString()
        {
            try
            {
                var strConexao = ConfigurationManager.ConnectionStrings["STRING_CONNECTION"].ConnectionString;

                return strConexao;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
