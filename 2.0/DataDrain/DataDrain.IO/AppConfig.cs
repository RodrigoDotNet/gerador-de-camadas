using System.IO;
using System.Text;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.IO
{
    public static class AppConfig
    {
        public static void WriteFile(Configuration parametros)
        {
            if (!parametros.GerarAppConfig) return;

            var sbAppConfig = new StringBuilder();

            sbAppConfig.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sbAppConfig.AppendLine("<configuration>");
            sbAppConfig.AppendLine("<connectionStrings>");
            sbAppConfig.AppendLine(string.Format("<add name=\"STRING_CONNECTION\" connectionString=\"Data Source = {0}; Initial Catalog = {2}; " +
                                                 "Persist Security Info = True; User ID = {1}; Password = {3}; " +
                                                 "\" providerName=\"System.Data.SqlClient\" />", parametros.User.ServerAddress, 
                                                 parametros.User.UserName, parametros.User.DatabaseName, parametros.User.Password));
            sbAppConfig.AppendLine("</connectionStrings>");
            sbAppConfig.AppendLine("</configuration>");

            File.WriteAllText(string.Format("{0}\\app.config", parametros.DestinationPath), sbAppConfig.ToString());
        }
    }
}
