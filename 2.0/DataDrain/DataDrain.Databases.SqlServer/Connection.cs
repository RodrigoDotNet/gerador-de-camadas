using System.Data.SqlClient;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.Databases.SqlServer
{
    internal sealed class Connection
    {
        private const string StringConnection = "Data Source={0};Database={1};UID={2}; Password={3}";

        private const string TrustedStringConnection = "Data Source={0};Database={1};Trusted_Connection=True;";

        public static SqlConnection DefaultConnection(DatabaseUser usr)
        {
            return usr.IsTrustedConnection
                ? new SqlConnection(string.Format(TrustedStringConnection, usr.ServerAddress, usr.DatabaseName))
                : new SqlConnection(string.Format(StringConnection, usr.ServerAddress, usr.DatabaseName, usr.UserName, usr.Password));

        }
    }
}
