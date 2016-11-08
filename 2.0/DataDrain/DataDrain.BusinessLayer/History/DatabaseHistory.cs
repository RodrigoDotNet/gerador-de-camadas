using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using DataDrain.Library.Cryptography;
using DataDrain.Library.ORM;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.BusinessLayer.History
{
    public sealed class DatabaseHistory : BaseHistory
    {
        private readonly string _strConnection = string.Format(@"Data Source={0}data\axia.db; Version=3;", AppDomain.CurrentDomain.BaseDirectory);
        private readonly HashSha1 _sh1;

        public DatabaseHistory()
        {
            _sh1 = new HashSha1();
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (exeDir != null) Directory.SetCurrentDirectory(exeDir);
        }

        public override List<DatabaseUser> CarregaConexoes(string nomeProvedor)
        {
            using (var cnn = new SQLiteConnection(_strConnection))
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT * FROM HistoricoConexao WHERE Provider = @nomeProvedor;";
                cmd.Parameters.AddWithValue("nomeProvedor", nomeProvedor);
                cnn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    return dr.MapToEntities<DatabaseUser>();
                }
            }
        }


        public override void SalvaConexao(DatabaseUser dadosLogin)
        {
            if (dadosLogin == null || string.IsNullOrWhiteSpace(dadosLogin.UserId))
            {
                return;
            }

            using (var cnn = new SQLiteConnection(_strConnection))
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM HistoricoConexao 
                                    WHERE Provider = @nomeProvedor 
                                    AND Usuario = @usuario
                                    AND Servidor = @servidor
                                    AND Porta = @porta;";

                cmd.Parameters.AddWithValue("nomeProvedor", dadosLogin.NomeProvedor);
                cmd.Parameters.AddWithValue("usuario", dadosLogin.UserName);
                cmd.Parameters.AddWithValue("servidor", dadosLogin.ServerAddress);
                cmd.Parameters.AddWithValue("porta", dadosLogin.Port);

                cnn.Open();

                DatabaseUser dadosConexao;

                using (var dr = cmd.ExecuteReader())
                {
                    dadosConexao = dr.MapToEntities<DatabaseUser>().FirstOrDefault();

                    if (dadosConexao != null)
                    {
                        dadosConexao.Password = _sh1.Descriptografa(dadosConexao.Password);
                    }
                }

                if (dadosConexao == null)
                {
                    cmd.CommandText = @"INSERT INTO HistoricoConexao (ID, MaquinaID, Usuario, Senha, Servidor, [DataBase], Porta, TrustedConnection, Provider)  
                                        VALUES (@id, @maquinaID, @usuario, @senha, @servidor, @db, @porta, @trustedConnection, @nomeProvedor);";
                    cmd.Parameters.AddWithValue("id", dadosLogin.UserId);
                    cmd.Parameters.AddWithValue("maquinaID", dadosLogin.MachineId);
                    cmd.Parameters.AddWithValue("senha", _sh1.Criptografa(dadosLogin.Password));
                    cmd.Parameters.AddWithValue("db", dadosLogin.DatabaseName);
                    cmd.Parameters.AddWithValue("trustedConnection", dadosLogin.IsTrustedConnection);

                    cmd.ExecuteNonQuery();
                }
                else
                {
                    cmd.CommandText = @"UPDATE HistoricoConexao SET MaquinaID = @maquinaID, Usuario = @usuario, Senha = @senha, Servidor = @servidor, [DataBase] = @db, Porta = @porta, TrustedConnection = @trustedConnection, Provider = @nomeProvedor
                                        WHERE ID = @id;";
                    cmd.Parameters.AddWithValue("id", dadosLogin.UserId);
                    cmd.Parameters.AddWithValue("maquinaID", dadosLogin.MachineId);
                    cmd.Parameters.AddWithValue("senha", _sh1.Criptografa(dadosLogin.Password));
                    cmd.Parameters.AddWithValue("db", dadosLogin.DatabaseName);
                    cmd.Parameters.AddWithValue("trustedConnection", dadosLogin.IsTrustedConnection);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
