using System.Security;
using DataDrain.IO;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.Databases.SqlServer
{
    public sealed class SupportMap : ISupport
    {
        /// <summary>
        /// Retorna o tipo de dados referente ao .NET 
        /// </summary>
        /// <param name="tipoDados"></param>
        /// <returns></returns>
        public string DataTypeNetFramework(string tipoDados)
        {
            switch (tipoDados)
            {

                case "smallint":
                case "tinyint":
                case "mediumint":
                case "int":
                    return "int";

                case "bigint":
                    return "long";

                case "real":
                    return "float";

                case "date":
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                case "timestamp":
                    return "DateTime";

                case "time":
                    return "TimeSpan";

                case "bit":
                case "bool":
                    return "bool";

                case "decimal":
                case "numeric":
                case "smallmoney":
                case "money":
                    return "decimal";

                case "float":
                case "double":
                    return "Double";

                case "datetimeoffset":
                    return "System.DateTimeOffset";

                case "varbinary":
                case "image":
                case "rowversion":
                    return "byte[]";

                case "sql_variant":
                    return "object";

                default:
                    return "String";

            }
        }

        /// <summary>
        /// Retorna o tipo de dados referente ao do sql server
        /// </summary>
        /// <param name="tipoDados"></param>
        /// <returns></returns>
        public string ConvertTypeClrToSql(string tipoDados)
        {
            switch (tipoDados.ToLower())
            {

                case "char":
                    return "char";

                case "char[]":
                    return "char[]";

                case "datetime":
                    return "timestamp";

                case "decimal":
                    return "decimal";

                case "double":
                    return "double";

                case "int16":
                    return "tinyint";

                case "int32":
                    return "int";

                case "int64":
                    return "bigint";

                case "single":
                    return "float";

                default:
                    return "varchar";
            }
        }

        public void CreateProjectFiles(Configuration parametros, ITemplateText dictionaryTemplates)
        {
            var returnCreateDirectory = OrmDirectory.Create(parametros.DestinationPath);

            if (returnCreateDirectory.Key)
            {
                new OrmBasicFiles(dictionaryTemplates).Create(parametros, "SqlServerFormatter");
                OrmObjectFiles.Create(parametros,dictionaryTemplates);
                OrmProjectFiles.Create(parametros, "SqlServerFormatter");
                AppConfig.WriteFile(parametros);
            }
            else
            {
                throw new SecurityException(returnCreateDirectory.Value);
            }
        }
    }
}
