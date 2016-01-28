using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using DataDrain.Mapping;
using DataDrain.ORM.Generator.Apoio.Base;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Generator.Apoio.HistoricosConexao
{
    internal class HistoricoSqlite : HistoricoBase
    {
        private readonly string _strConnection = string.Format(@"Data Source={0}data\axia.db; Version=3;", AppDomain.CurrentDomain.BaseDirectory);
        private readonly GeraHashSha1 _sh1;

        public HistoricoSqlite()
        {
            _sh1 = new GeraHashSha1();
            string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(exeDir);
        }

        public override List<DadosUsuario> CarregaConexoes(string nomeProvedor)
        {
            using (var cnn = new SQLiteConnection(_strConnection))
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT * FROM HistoricoConexao WHERE Provider = @nomeProvedor;";
                cmd.Parameters.AddWithValue("nomeProvedor", nomeProvedor);
                cnn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    return dr.MapToEntities<DadosUsuario>();
                }
            }
        }


        public override void SalvaConexao(DadosUsuario dadosLogin)
        {
            if (dadosLogin == null || string.IsNullOrWhiteSpace(dadosLogin.ID))
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
                cmd.Parameters.AddWithValue("usuario", dadosLogin.Usuario);
                cmd.Parameters.AddWithValue("servidor", dadosLogin.Servidor);
                cmd.Parameters.AddWithValue("porta", dadosLogin.Porta);

                cnn.Open();

                DadosUsuario dadosConexao;

                using (var dr = cmd.ExecuteReader())
                {
                    dadosConexao = dr.MapToEntities<DadosUsuario>().FirstOrDefault();

                    if (dadosConexao != null)
                    {
                        dadosConexao.Senha = _sh1.Descriptografa(dadosConexao.Senha);
                    }
                }

                if (dadosConexao == null)
                {
                    cmd.CommandText = @"INSERT INTO HistoricoConexao (ID, MaquinaID, Usuario, Senha, Servidor, [DataBase], Porta, TrustedConnection, Provider)  
                                        VALUES (@id, @maquinaID, @usuario, @senha, @servidor, @db, @porta, @trustedConnection, @nomeProvedor);";
                    cmd.Parameters.AddWithValue("id", dadosLogin.ID);
                    cmd.Parameters.AddWithValue("maquinaID", dadosLogin.MaquinaID);
                    cmd.Parameters.AddWithValue("senha", _sh1.Criptografa(dadosLogin.Senha));
                    cmd.Parameters.AddWithValue("db", dadosLogin.DataBase);
                    cmd.Parameters.AddWithValue("trustedConnection", dadosLogin.TrustedConnection);

                    cmd.ExecuteNonQuery();
                }
                else
                {
                    cmd.CommandText = @"UPDATE HistoricoConexao SET MaquinaID = @maquinaID, Usuario = @usuario, Senha = @senha, Servidor = @servidor, [DataBase] = @db, Porta = @porta, TrustedConnection = @trustedConnection, Provider = @nomeProvedor
                                        WHERE ID = @id;";
                    cmd.Parameters.AddWithValue("id", dadosLogin.ID);
                    cmd.Parameters.AddWithValue("maquinaID", dadosLogin.MaquinaID);
                    cmd.Parameters.AddWithValue("senha", _sh1.Criptografa(dadosLogin.Senha));
                    cmd.Parameters.AddWithValue("db", dadosLogin.DataBase);
                    cmd.Parameters.AddWithValue("trustedConnection", dadosLogin.TrustedConnection);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
