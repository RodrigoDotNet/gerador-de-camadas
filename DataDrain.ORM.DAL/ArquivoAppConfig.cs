using System;
using System.IO;
using System.Text;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL
{
    public class ArquivoAppConfig
    {
        public static void Gerar(string caminho, DadosUsuario dadosConexao, bool gerar, string log4Net)
        {

            if (gerar)
            {
                var sbAppConfig = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(log4Net))
                {
                    var temp = log4Net.Replace("[connectionString]", string.Format("server={0};User Id={1};database={2};Password={3};Allow Zero Datetime=True;Allow User Variables=True;Convert Zero Datetime=True;Treat Blobs As UTF8=True;", dadosConexao.Servidor, dadosConexao.Usuario, dadosConexao.DataBase, dadosConexao.Senha));
                    temp = temp.Replace("[connectionStrings]", string.Format("<connectionStrings>{0}{1}{0}</connectionStrings>{0}", Environment.NewLine, string.Format("<add name=\"STRING_CONNECTION\" connectionString=\"server={0};User Id={1};database={2};Password={3};Allow Zero Datetime=True;Allow User Variables=True;Convert Zero Datetime=True;Treat Blobs As UTF8=True;\" providerName=\"MySql.Data.MySqlClient\" />", dadosConexao.Servidor, dadosConexao.Usuario, dadosConexao.DataBase, dadosConexao.Senha)));
                    sbAppConfig = new StringBuilder(temp);
                }
                else
                {
                    sbAppConfig.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                    sbAppConfig.AppendLine("<configuration>");
                    sbAppConfig.AppendLine("<connectionStrings>");
                    sbAppConfig.AppendLine(string.Format("<add name=\"STRING_CONNECTION\" connectionString=\"server={0};User Id={1};database={2};Password={3};Allow Zero Datetime=True;Allow User Variables=True;Convert Zero Datetime=True;Treat Blobs As UTF8=True;\" providerName=\"MySql.Data.MySqlClient\" />", dadosConexao.Servidor, dadosConexao.Usuario, dadosConexao.DataBase, dadosConexao.Senha));
                    sbAppConfig.AppendLine("</connectionStrings>");
                    sbAppConfig.AppendLine("</configuration>");
                }

                File.WriteAllText(string.Format("{0}\\app.config", caminho), sbAppConfig.ToString());
            }

        }
    }
}
