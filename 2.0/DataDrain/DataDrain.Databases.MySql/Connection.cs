using DataDrain.Rules.SuportObjects;
using MySql.Data.MySqlClient;

namespace DataDrain.Databases.MySql
{
    internal static class Connection
    {
        public static MySqlConnection DefaultConnection(DatabaseUser usr)
        {
            return new MySqlConnection(string.Format("Data Source={0};Initial Catalog={1};UID={2}; Password={3};Port={4};", usr.ServerAddress, usr.DatabaseName, usr.UserName, usr.Password, usr.Port));
        }
    }
}
