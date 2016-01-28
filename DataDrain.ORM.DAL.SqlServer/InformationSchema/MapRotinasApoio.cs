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
                    return "Single";

                case "date":
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                case "timestamp":
                    return "DateTime";

                case "time":
                    return "System.TimeSpan";

                case "bit":
                case "bool":
                    return "Boolean";

                case "decimal":
                case "numeric":
                case "smallmoney":
                case "money":
                    return "Decimal";

                case "float":
                case "double":
                    return "Double";

                case "xml":
                    return "System.Xml.Linq.XElement";

                case "datetimeoffset":
                    return "System.DateTimeOffset";

                case "varbinary":
                    return "byte[]";

                default:
                    return "String";

            }
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
