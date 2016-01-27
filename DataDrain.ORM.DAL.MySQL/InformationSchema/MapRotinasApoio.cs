using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL.MySQL.InformationSchema
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

        /// <summary>
        /// Retorna o tipo de dados  usados para os parametros
        /// </summary>
        /// <param name="tipoDados"></param>
        public string RetornaTiposDbType(string tipoDados)
        {
            switch (tipoDados)
            {

                case "smallint":
                case "tinyint":
                case "mediumint":
                    return "DbType.Int16";

                case "int":
                case "year":
                    return "DbType.Int32";

                case "bigint":
                    return "DbType.Int64";

                case "float":
                case "double":
                    return "DbType.Double";

                case "smalldatetime":
                case "datetime":
                case "datetime2":
                case "timestamp":
                    return "DbType.DateTime";

                case "time":
                    return "DbType.TimeSpan";

                case "real":
                    return "DbType.Single";

                case "bit":
                case "bool":
                    return "DbType.Boolean";

                case "decimal":
                case "numeric":
                case "smallmoney":
                case "money":
                    return "DbType.Decimal";

                default:
                    return "DbType.String";
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
                ArquivosORM.GravaArquivosBaseOrm(parametros, "MySqlFormatter");
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

        /// <summary>
        /// Retorna os dados para uma dialog de pesquisa de arquivo
        /// </summary>
        /// <returns></returns>
        public OpenDadosFileDialog RetornaDialogPesquisa()
        {
            return new OpenDadosFileDialog();
        }
    }
}
