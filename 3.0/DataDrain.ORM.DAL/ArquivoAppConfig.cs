using System;
using System.IO;
using System.Text;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL
{
    public class ArquivoAppConfig
    {
        public static void Gerar(ParametrosCriarProjetos parametros)
        {
            if (!parametros.GerarAppConfig) return;

            var sbAppConfig = new StringBuilder();

            sbAppConfig.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sbAppConfig.AppendLine("<configuration>");
            sbAppConfig.AppendLine("<connectionStrings>");
            sbAppConfig.AppendLine(string.Format("<add name=\"STRING_CONNECTION\" connectionString=\"Data Source = {0}; Initial Catalog = {2}; " +
                                                 "Persist Security Info = True; User ID = {1}; Password = {3}; " +
                                                 "\" providerName=\"System.Data.SqlClient\" />", parametros.DadosConexao.Servidor, 
                                                 parametros.DadosConexao.Usuario, parametros.DadosConexao.DataBase, parametros.DadosConexao.Senha));
            sbAppConfig.AppendLine("</connectionStrings>");
            sbAppConfig.AppendLine("</configuration>");

            File.WriteAllText(string.Format("{0}\\app.config", parametros.CaminhoDestino), sbAppConfig.ToString());
        }
    }
}
