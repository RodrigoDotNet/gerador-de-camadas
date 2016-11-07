using System.Collections.Generic;
using System.Data.SQLite;

namespace DataDrain.ORM.DAL.Templates
{
    internal sealed class Template
    {
        private const string StrConnection = @"Data Source=data\axia.db; Version=3; FailIfMissing=True; Foreign Keys=True;";

        public static List<KeyValuePair<string, string>> RetornaValores()
        {
            var valores = new List<KeyValuePair<string, string>>();

            using (var cnn = new SQLiteConnection(StrConnection))
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT Chave, Valor FROM Templates;";
                cnn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        valores.Add(new KeyValuePair<string, string>(dr.GetString(0), dr.GetString(1)));
                    }
                }
            }

            return valores;
        }

        public static KeyValuePair<string, string> RetornaValor(string chave)
        {
            using (var cnn = new SQLiteConnection(StrConnection))
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT Chave, Valor FROM Templates WHERE Chave = @chave;";
                cmd.Parameters.AddWithValue("chave", chave);
                cnn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        return new KeyValuePair<string, string>(dr.GetString(0), dr.GetString(1));
                    }
                    return new KeyValuePair<string, string>();
                }
            }
        }
    }
}
