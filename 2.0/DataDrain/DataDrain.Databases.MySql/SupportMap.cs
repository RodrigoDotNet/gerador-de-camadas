using System.Security;
using DataDrain.IO;
using DataDrain.ORM.DAL.MySQL.InformationSchema;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.Databases.MySql
{
    public sealed class SupportMap : ISupport
    {
        public string DataTypeNetFramework(string tipoDados)
        {
            tipoDados = tipoDados.Replace("(", "").Replace(")", "").RemoveNumeros();

            switch (tipoDados.ToLower())
            {
                case "enum":
                    return "string";

                case "smallint":
                case "tinyint":
                case "mediumint":
                case "year":
                case "int":
                case "integer":
                    return "int";

                case "bigint":
                case "serial":
                    return "long";

                case "real":
                    return "Double";

                case "datetime":
                case "datetime2":
                case "timestamp":
                case "date":
                case "time":
                    return "DateTime";

                case "bit":
                case "bool":
                case "boolean":
                    return "bool";

                case "decimal":
                case "dec":
                case "numeric":
                case "smallmoney":
                case "money":
                    return "decimal";

                case "double":
                    return "double";

                case "float":
                    return "float";

                case "point":
                case "linestring":
                case "polygon":
                case "multipoint":
                case "multilineString":
                case "multipolygon":
                    return "MySqlGeometry";


                default:
                    if (tipoDados.ToLower().Contains("blob") || tipoDados.ToLower().Contains("binary"))
                    {
                        return "byte[]";
                    }
                    if (tipoDados.ToLower().Contains("char"))
                    {
                        return "string";
                    }
                    break;
            }
            return "string";
        }

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
                new OrmBasicFiles(dictionaryTemplates).Create(parametros, "MySqlFormatter");
                OrmObjectFiles.Create(parametros, dictionaryTemplates);
                OrmProjectFiles.Create(parametros, "MySqlFormatter");
                AppConfig.WriteFile(parametros);
            }
            else
            {
                throw new SecurityException(returnCreateDirectory.Value);
            }
        }
    }
}