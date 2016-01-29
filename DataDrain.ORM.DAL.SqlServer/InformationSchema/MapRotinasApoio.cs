using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL.SqlServer.InformationSchema
{
    public sealed class MapRotinasApoio : IRotinasApoio
    {
        /// <summary>
        /// Retorna o tipo de dados referente ao .NET 
        /// </summary>
        /// <param name="tipoDados"></param>
        /// <returns></returns>
        public string RetornaTipoDadosDotNet(string tipoDados)
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

                case "xml":
                    return "System.Xml.Linq.XElement";

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
        public string ConvertTipoClrToSql(string tipoDados)
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

        /// <summary>
        /// Cria todos os arquivos de mapeamento
        /// </summary>
        /// <param name="parametros"></param>
        public void CriarArquivosProjeto(ParametrosCriarProjetos parametros)
        {
            try
            {
                DiretoriosBase.Criar(parametros.CaminhoDestino);
                ArquivosORM.GravaArquivosBaseOrm(parametros, "SqlServerFormatter");

                if (parametros.MapWcf)
                {
                    ProjetoWcf.CriarProjeto(parametros);
                }

                ArquivosObjetos.Log = !string.IsNullOrWhiteSpace(parametros.XmlLog4Net);
                ArquivosObjetos.Gerar(parametros);
                ArquivosProjeto.Gerar(parametros.CaminhoDestino, parametros.ObjetosMapeaveis, parametros.NameSpace, parametros.AssinarProjeto, parametros.GerarAppConfig, parametros.CaminhoStrongName, parametros.VersaoFramework, parametros.XmlLog4Net, "MySqlFormatter");
                ArquivoAppConfig.Gerar(parametros.CaminhoDestino, parametros.DadosConexao, parametros.GerarAppConfig, parametros.XmlLog4Net);
            }
            catch
            {
                throw;
            }
        }
    }
}
