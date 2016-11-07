using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataDrain.Library.Helpers;
using DataDrain.Rules.Enuns;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.IO
{
    public class FileFactory
    {
        private static ITemplateText _template;

        public FileFactory(ITemplateText template)
        {
            _template = template;
        }

        public bool Log { get; set; }

        private static string CheckIsNullable(string nameType, bool fieldAcceptNull)
        {
            if (fieldAcceptNull)
            {
                switch (nameType.Trim().ToLower())
                {
                    case "int":
                    case "double":
                    case "bool":
                    case "char":
                    case "float":
                    case "byte":
                    case "int16":
                    case "int32":
                    case "int64":
                    case "datetime":
                    case "sbyte":
                    case "short":
                    case "ushort":
                    case "uint":
                    case "long":
                    case "ulong":
                    case "decimal":
                    case "timespan":
                        return "?";
                }
            }
            return "";
        }

        internal static string RetornaTextoBase(string keyObj)
        {
            return _template.GelValue(keyObj).Value;
        }

        private static void CriaPropriedades(List<ColumnInfo> colunas, StringBuilder retorno, IEnumerable<ForeignKey> listaFk, Configuration parametros)
        {
            colunas = colunas.OrderByDescending(c => c.IsPrimaryKey).ToList();

            foreach (var coluna in colunas)
            {
                var colunaRequirida = false;

                if (parametros.MapLinq)
                {
                    if (coluna.IsPrimaryKey || coluna.IsNullability == false)
                    {
                        retorno.AppendFormat("[Required, DisplayName(\"{0}\")]{1}", coluna.ColumnName, Environment.NewLine);
                        colunaRequirida = !coluna.IsNullability;
                    }

                    if (coluna.Type.ToLower().Contains("char") || coluna.Type.ToLower() == "string")
                    {
                        switch (colunaRequirida)
                        {
                            case true:
                                retorno.AppendFormat("[StringLength({0})]{1}", coluna.Size, Environment.NewLine);
                                break;
                            default:
                                retorno.AppendFormat("[StringLength({0}), DisplayName(\"{1}\")]{2}", coluna.Size, coluna.ColumnName, Environment.NewLine);
                                break;
                        }
                    }
                    //valida se o campo possui validação personalizada
                    if (!string.IsNullOrEmpty(coluna.RegularExpression))
                    {
                        retorno.AppendLine(string.Format("[RegularExpression(@\"{0}\")]", coluna.RegularExpression));
                    }
                    retorno.Append(RetornaMapColumn(coluna)).Append(Environment.NewLine);
                }

                if (parametros.MapWcf)
                {
                    retorno.AppendFormat("[DataMember]{0}", Environment.NewLine);
                }

                retorno.Append(string.Format("public {0}{1} {2} {{ get; set; }}", coluna.Type, CheckIsNullable(coluna.Type, coluna.IsNullability), StringHelper.RemoveInvalidCharacters(coluna.ColumnName)) + Environment.NewLine + Environment.NewLine);
            }

            CriaAssociacoes(retorno, listaFk);

            retorno.Append("}" + Environment.NewLine);
            retorno.Append("}" + Environment.NewLine);
        }

        private static void CriaAssociacoes(StringBuilder retorno, IEnumerable<ForeignKey> listaFk)
        {

            if (listaFk == null)
            {
                return;
            }

            retorno.AppendLine("#region .: Asociações :.").AppendLine();

            foreach (var fk in listaFk)
            {
                retorno.AppendFormat("//public {0}TO {0} § get; set; ª", RetornaNomeClasseAjustado(fk.TableFk)).AppendLine().AppendLine();
            }

            retorno.AppendLine("#endregion");
        }

        private static void CriaEnumeradores(IEnumerable<string> enumeradores, StringBuilder retorno)
        {
            if (enumeradores == null)
            {
                return;
            }

            retorno.AppendLine();
            retorno.AppendLine("#region .: Enumeradores :.").AppendLine();

            foreach (var en in enumeradores)
            {
                retorno.AppendLine(string.Format("{0}{1}{1}", en, Environment.NewLine));
            }

            retorno.AppendLine("#endregion");
            retorno.AppendLine();
        }

        private static string RetornaMapColumn(ColumnInfo coluna)
        {

            var retCol = string.Format("[Column(Storage = \"{0}\", {1}{2}AutoSync = AutoSync.{3})]",
                coluna.ColumnName,
                (coluna.IsPrimaryKey ? "IsPrimaryKey = true," : ""),
                (coluna.IsIdentity ? "IsDbGenerated = true," : ""),
                Enum.GetName(typeof(EColumnSync), coluna.ColumnSync)
                );

            return retCol;

        }

        private static void CriaCabecario(string nomeTabela, string strNamespace, StringBuilder retorno, Configuration parametros)
        {
            retorno.AppendFormat("using System;{0}", Environment.NewLine);

            if (parametros.MapLinq)
            {
                retorno.AppendFormat("using System.ComponentModel;{0}", Environment.NewLine);
                retorno.AppendFormat("using System.ComponentModel.DataAnnotations;{0}", Environment.NewLine);
                retorno.AppendFormat("using System.Data.Linq.Mapping;{0}", Environment.NewLine);
            }
            if (parametros.MapWcf)
            {
                retorno.AppendFormat("using System.Runtime.Serialization;{0}", Environment.NewLine);
            }

            retorno.AppendLine();
            retorno.AppendFormat("{0}{1}", string.Format("namespace {0}TO", strNamespace), Environment.NewLine);
            retorno.AppendFormat("{{{0}", Environment.NewLine);

            if (parametros.MapLinq)
            {
                retorno.AppendFormat("{0}{1}", string.Format("[Table(Name = \"{0}\")]", nomeTabela), Environment.NewLine);
            }
            if (parametros.MapWcf)
            {
                retorno.AppendFormat("{0}{1}", "[DataContract]", Environment.NewLine);
            }

            retorno.AppendFormat("{0}{1}", string.Format("public class {0}TO", RetornaNomeClasseAjustado(nomeTabela)), Environment.NewLine);
            retorno.AppendFormat("{{{0}{0}", Environment.NewLine);
        }


        public static string RetornaNomeClasseAjustado(string input)
        {
            var culture = new CultureInfo("pt-BR", false).TextInfo;
            var filtroRegex = new Regex("([^a-zA-Z]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            var arrayNome = filtroRegex.Replace(input, "|").Split('|');
            return arrayNome.Aggregate(string.Empty, (current, t) => string.Format("{0}{1}", current, culture.ToTitleCase(t.ToString(new CultureInfo("pt-BR", false)))));
        }

        public string GeneratePocoFiles(string tableName, DatabaseObjectInfo objectDatabase, Configuration configuration)
        {
            var retorno = new StringBuilder();

            CriaCabecario(tableName, configuration.NameSpace, retorno, configuration);

            CriaPropriedades(objectDatabase.Columns, retorno, objectDatabase.ForeignKeys, configuration);

            return retorno.ToString();
        }

        public string GenerateBusinessFiles(string nomeTabela, DatabaseObjectInfo objectDatabase, Configuration configuration)
        {

            string textoBase;
            switch (objectDatabase.DatabaseObjectType)
            {
                case EDatabaseObjectType.Tabela:
                    textoBase = RetornaTextoBase("padraoBLLNativo");
                    break;
                case EDatabaseObjectType.Query:
                case EDatabaseObjectType.Procedure:
                    textoBase = RetornaTextoBase("padraoBLLProc");
                    textoBase = textoBase.Replace("{parametros}", string.Join(", ", objectDatabase.StoredProcedureParameters.Select(p => string.Format("{0} {1}", p.ParameterDotNetType, p.ParameterName))));
                    textoBase = textoBase.Replace("{parametros2}", string.Join(", ", objectDatabase.StoredProcedureParameters.Select(p => p.ParameterName)));
                    break;
                case EDatabaseObjectType.View:
                    textoBase = RetornaTextoBase("padraoBLLNativo");
                    textoBase = Regex.Replace(textoBase, "#region .: CRUD :.(.|\n)*?#endregion", string.Empty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("tipoObjeto");
            }
            textoBase = Log ? textoBase.Replace("[log]", "Log.Error(ex.Message, ex);").Replace("[logHeader]", string.Format("private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof({0}BLL));", nomeTabela)) : textoBase.Replace("[log]", "").Replace("[logHeader]", "");

            return textoBase.Replace("{namespace}", configuration.NameSpace).Replace("{classe}", (nomeTabela));

        }

        public string GenerateDataAccesFiles(string nomeTabela, DatabaseObjectInfo objectDatabase, Configuration configuration)
        {

            string textoBase;
            switch (objectDatabase.DatabaseObjectType)
            {
                case EDatabaseObjectType.Tabela:
                    textoBase = RetornaTextoBase("padraoDALNativo");
                    break;
                case EDatabaseObjectType.Query:
                case EDatabaseObjectType.Procedure:
                    textoBase = RetornaTextoBase("padraoDALProc");
                    textoBase = textoBase.Replace("{parametros}", string.Join(", ", objectDatabase.StoredProcedureParameters.Select(p => string.Format("{0} {1}", p.ParameterDotNetType, p.ParameterName))));

                    var sbParametros = new StringBuilder();

                    foreach (var parametro in objectDatabase.StoredProcedureParameters)
                    {
                        sbParametros.AppendLine(string.Format("cmd.Parameters.AddWithValue(\"{0}\", {0});", parametro.ParameterName));
                    }

                    textoBase = textoBase.Replace("{carregaParametros}", sbParametros.ToString());
                    textoBase = textoBase.Replace("{query}", objectDatabase.DatabaseObjectType == EDatabaseObjectType.Procedure ? nomeTabela : objectDatabase.QuerySql);
                    textoBase = textoBase.Replace("{proc}", objectDatabase.DatabaseObjectType == EDatabaseObjectType.Procedure ? "cmd.CommandType = CommandType.StoredProcedure;" : "");

                    break;
                case EDatabaseObjectType.View:
                    textoBase = RetornaTextoBase("padraoDALNativo");
                    textoBase = Regex.Replace(textoBase, "#region .: Persistencia :.(.|\n)*?#endregion", string.Empty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("tipoObjeto");
            }
            textoBase = Log ? textoBase.Replace("[log]", "Log.Error(ex.Message, ex);").Replace("[logHeader]", string.Format("private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof({0}DAL));", nomeTabela)) : textoBase.Replace("[log]", "").Replace("[logHeader]", "");

            return textoBase.Replace("{namespace}", configuration.NameSpace).Replace("{classe}", (nomeTabela));

        }
    }
}
