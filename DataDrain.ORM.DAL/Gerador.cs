using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataDrain.ORM.DAL.Templates;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL
{
    public enum ETipoObjeto
    {
        Tabela = 0,
        View,
        Procedure
    }

    public class Gerador
    {
        public bool Log { get; set; }

        #region .: Métodos :.

        private static string RemoveCaracteresInvalidos(string strTexto)
        {
            var strTextoOk = strTexto;

            if (!string.IsNullOrEmpty(strTexto))
            {
                strTextoOk = Regex.Replace(strTextoOk, "[áàâãª]", "a");
                strTextoOk = Regex.Replace(strTextoOk, "[ÁÀÂÃ]", "A");
                strTextoOk = Regex.Replace(strTextoOk, "[éèê]", "e");
                strTextoOk = Regex.Replace(strTextoOk, "[ÉÈÊ]", "e");
                strTextoOk = Regex.Replace(strTextoOk, "[íìî]", "i");
                strTextoOk = Regex.Replace(strTextoOk, "[ÍÌÎ]", "I");
                strTextoOk = Regex.Replace(strTextoOk, "[óòôõº]", "o");
                strTextoOk = Regex.Replace(strTextoOk, "[ÓÒÔÕ]", "O");
                strTextoOk = Regex.Replace(strTextoOk, "[úùû]", "u");
                strTextoOk = Regex.Replace(strTextoOk, "[ÚÙÛ]", "U");
                strTextoOk = Regex.Replace(strTextoOk, "[ç]", "c");
                strTextoOk = Regex.Replace(strTextoOk, "[Ç]", "C");
                strTextoOk = Regex.Replace(strTextoOk, @"^[\d-]*\s*", "");

                //var filtroRegex = new Regex("([^a-zA-Z]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
                //strTextoOk = filtroRegex.Replace(strTextoOk, "");

            }
            return strTextoOk;

        }


        private static string VerificaNullable(string nomeTipo, bool aceitaNull)
        {
            if (aceitaNull)
            {
                switch (nomeTipo.Trim().ToLower())
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
            return Template.RetornaValor(keyObj).Value;
        }

        private static void CriaPropriedades(List<DadosColunas> colunas, StringBuilder retorno, IEnumerable<DadosAssociation> listaFk, List<string> enumeradores, ParametrosCriarProjetos parametros)
        {
            colunas = colunas.OrderByDescending(c => c.Pk).ToList();

            foreach (var coluna in colunas)
            {
                var colunaRequirida = false;

                if (parametros.MapLinq)
                {
                    if (coluna.Pk || coluna.AceitaNull == false)
                    {
                        retorno.AppendFormat("[Required, DisplayName(\"{0}\")]{1}", coluna.Coluna, Environment.NewLine);
                        colunaRequirida = !coluna.AceitaNull;
                    }

                    if (enumeradores != null)
                    {
                        var coluna1 = coluna;
                        foreach (var dom in enumeradores.Where(domain => coluna1.Coluna == domain.Split('|')[0]).Select(domain => string.Format("[DomainValidator(new object[]<{0}>)]{1}", domain.Split('|')[1], Environment.NewLine)).Select(prodDomain => prodDomain.Replace("<", "{").Replace(">", "}").Replace("'", "\"")))
                        {
                            retorno.Append(dom);
                        }
                    }


                    //valida o tamanho do campo de texto
                    if (coluna.Tipo.ToLower().Contains("char") || coluna.Tipo.ToLower() == "string")
                    {
                        switch (colunaRequirida)
                        {
                            case true:
                                retorno.AppendFormat("[StringLength({0})]{1}", coluna.Tamanho, Environment.NewLine);
                                break;
                            default:
                                retorno.AppendFormat("[StringLength({0}), DisplayName(\"{1}\")]{2}", coluna.Tamanho, coluna.Coluna, Environment.NewLine);
                                break;
                        }
                    }
                    //valida se o campo possui validação personalizada
                    if (!string.IsNullOrEmpty(coluna.RegExp))
                    {
                        retorno.AppendLine(string.Format("[RegularExpression(@\"{0}\")]", coluna.RegExp));
                    }
                    retorno.Append(RetornaMapColumn(coluna)).Append(Environment.NewLine);
                }

                if (parametros.MapWcf)
                {
                    retorno.AppendFormat("[DataMember]{0}", Environment.NewLine);
                }

                retorno.Append(string.Format("public {0}{1} {2} {{ get; set; }}", coluna.Tipo, VerificaNullable(coluna.Tipo, coluna.AceitaNull), RemoveCaracteresInvalidos(coluna.Coluna)) + Environment.NewLine + Environment.NewLine);
            }

            retorno.AppendFormat("{0}#endregion{0}{0}", Environment.NewLine);

            CriaAssociacoes(retorno, listaFk);

            retorno.Append("}" + Environment.NewLine);
            retorno.Append("}" + Environment.NewLine);
        }

        private static void CriaAssociacoes(StringBuilder retorno, IEnumerable<DadosAssociation> listaFk)
        {

            if (listaFk == null)
            {
                return;
            }

            retorno.AppendLine("#region .: Asociações :.").AppendLine();

            foreach (var fk in listaFk)
            {
                retorno.AppendFormat("//public {0}TO {0} § get; set; ª", RetornaNomeClasseAjustado(fk.TabelaFK)).AppendLine().AppendLine();
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

        private static string RetornaMapColumn(DadosColunas coluna)
        {

            var retCol = string.Format("[Column(Storage = \"{0}\", {1}{2}AutoSync = AutoSync.{3})]",
                coluna.Coluna,
                (coluna.Pk ? "IsPrimaryKey = true," : ""),
                (coluna.Identity ? "IsDbGenerated = true," : ""),
                Enum.GetName(typeof(DadosColunas.ETipoSync), coluna.TipoSync)
                );

            return retCol;

        }

        private static void CriaCabecario(string nomeTabela, string strNamespace, StringBuilder retorno, ParametrosCriarProjetos parametros)
        {
            retorno.AppendFormat("using System;{0}", Environment.NewLine);

            if (parametros.MapLinq)
            {
                retorno.AppendFormat("using System.ComponentModel;{0}", Environment.NewLine);
                retorno.AppendFormat("using System.ComponentModel.DataAnnotations;{0}", Environment.NewLine);
                retorno.AppendFormat("using System.Data.Linq.Mapping;{0}", Environment.NewLine);
            }

            retorno.AppendFormat("using System.Runtime.Serialization;{0}", Environment.NewLine);
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
            retorno.AppendFormat("#region .: Propriedades Referente á Tabela :.{0}{0}", Environment.NewLine);
        }


        public static string RetornaNomeClasseAjustado(string input)
        {
            var culture = new CultureInfo("pt-BR", false).TextInfo;
            var filtroRegex = new Regex("([^a-zA-Z]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            var arrayNome = filtroRegex.Replace(input, "|").Split('|');
            return arrayNome.Aggregate(string.Empty, (current, t) => string.Format("{0}{1}", current, culture.ToTitleCase(t.ToString(new CultureInfo("pt-BR", false)))));
        }

        #endregion

        /// <summary>
        /// Cria o TO da tabela selecionada
        /// </summary>
        /// <param name="colunas">Colunas usadas no TO</param>
        /// <param name="nomeTabela">nome da classe TO</param>
        /// <param name="strNamespace">Namespace do Projeto</param>
        /// <param name="listaFk">lista de FK do objeto FK </param>
        /// <param name="enumeradores">Lista de Enuns do bando de dados </param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public string GerarClasseTO(List<DadosColunas> colunas, string nomeTabela, string strNamespace, List<DadosAssociation> listaFk, List<string> enumeradores, ParametrosCriarProjetos parametros)
        {
            var retorno = new StringBuilder();

            CriaCabecario(nomeTabela, strNamespace, retorno, parametros);

            CriaEnumeradores(enumeradores, retorno);

            CriaPropriedades(colunas, retorno, listaFk, enumeradores, parametros);

            return retorno.ToString();
        }

        public string GerarClasseBLL(string nomeTabela, string strNamespace, TipoObjetoBanco.ETipoObjeto tipoObjeto, List<DadosStoredProceduresParameters> parametros)
        {

            string textoBase;
            switch (tipoObjeto)
            {
                case TipoObjetoBanco.ETipoObjeto.Tabela:
                    textoBase = RetornaTextoBase("padraoBLLNativo");
                    break;
                case TipoObjetoBanco.ETipoObjeto.Procedure:
                    textoBase = RetornaTextoBase("padraoBLLProc");
                    textoBase = textoBase.Replace("{parametros}", string.Join(", ", parametros.Select(p => string.Format("{0} {1}", p.ParameterDotNetType, p.ParameterName))));
                    textoBase = textoBase.Replace("{parametros2}", string.Join(", ", parametros.Select(p => p.ParameterName)));
                    break;
                case TipoObjetoBanco.ETipoObjeto.View:
                    textoBase = RetornaTextoBase("padraoBLLNativo");
                    textoBase = Regex.Replace(textoBase, "#region .: CRUD :.(.|\n)*?#endregion", string.Empty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("tipoObjeto");
            }
            textoBase = Log ? textoBase.Replace("[log]", "Log.Error(ex.Message, ex);").Replace("[logHeader]", string.Format("private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof({0}BLL));", nomeTabela)) : textoBase.Replace("[log]", "").Replace("[logHeader]", "");

            return textoBase.Replace("{namespace}", strNamespace).Replace("{classe}", (nomeTabela));

        }

        public string GerarClasseDAL(string nomeTabela, string strNamespace, TipoObjetoBanco.ETipoObjeto tipoObjeto, List<DadosStoredProceduresParameters> parametros)
        {

            string textoBase;
            switch (tipoObjeto)
            {
                case TipoObjetoBanco.ETipoObjeto.Tabela:
                    textoBase = RetornaTextoBase("padraoDALNativo");
                    break;
                case TipoObjetoBanco.ETipoObjeto.Procedure:
                    textoBase = RetornaTextoBase("padraoDALProc");
                    textoBase = textoBase.Replace("{parametros}", string.Join(", ", parametros.Select(p => string.Format("{0} {1}", p.ParameterDotNetType, p.ParameterName))));

                    var sbParametros = new StringBuilder();

                    foreach (var parametro in parametros)
                    {
                        sbParametros.AppendLine(string.Format("cmd.Parameters.AddWithValue(\"{0}\", {0});", parametro.ParameterName));
                    }

                    textoBase = textoBase.Replace("{carregaParametros}", sbParametros.ToString());

                    break;
                case TipoObjetoBanco.ETipoObjeto.View:
                    textoBase = RetornaTextoBase("padraoDALNativo");
                    textoBase = Regex.Replace(textoBase, "#region .: Persistencia :.(.|\n)*?#endregion", string.Empty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("tipoObjeto");
            }
            textoBase = Log ? textoBase.Replace("[log]", "Log.Error(ex.Message, ex);").Replace("[logHeader]", string.Format("private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof({0}DAL));", nomeTabela)) : textoBase.Replace("[log]", "").Replace("[logHeader]", "");

            return textoBase.Replace("{namespace}", strNamespace).Replace("{classe}", (nomeTabela));

        }

        public string GerarInterfaceDAL(string nomeArquivo, string nameSpace, TipoObjetoBanco.ETipoObjeto tipoObjeto, List<DadosStoredProceduresParameters> parametros)
        {

            string textoBase;
            switch (tipoObjeto)
            {
                case TipoObjetoBanco.ETipoObjeto.Tabela:
                    textoBase = RetornaTextoBase("interfaceCRUD");
                    break;
                case TipoObjetoBanco.ETipoObjeto.View:
                    textoBase = RetornaTextoBase("interfaceViewProcedure");
                    break;
                case TipoObjetoBanco.ETipoObjeto.Procedure:
                    textoBase = "";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("tipoObjeto");
            }

            return textoBase.Replace("{namespace}", nameSpace).Replace("{classe}", (nomeArquivo));

        }
    }
}
