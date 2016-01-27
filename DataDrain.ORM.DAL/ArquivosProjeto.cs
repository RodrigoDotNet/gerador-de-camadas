using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL
{
    public class ArquivosProjeto
    {
        public static void Gerar(string caminho, List<KeyValuePair<TipoObjetoBanco, List<DadosColunas>>> objetosMapeaveis, string nameSpace, bool assinarProjeto, bool gerarAppConfig, string caminhoStrongName, string versaoFramework, string xmlLog4Net, string provider)
        {

            var guidProjInterface = Guid.NewGuid().ToString().ToUpper();
            var guidProjTO = Guid.NewGuid().ToString().ToUpper().ToUpper();
            var guidProjDAL = Guid.NewGuid().ToString().ToUpper().ToUpper();
            var guidProjBLL = Guid.NewGuid().ToString().ToUpper().ToUpper();
            var guidProjSolution = Guid.NewGuid().ToString().ToUpper().ToUpper();

            var arquivosInterface = (from KeyValuePair<TipoObjetoBanco, List<DadosColunas>> t in objetosMapeaveis select string.Format("<Compile Include=\"IPersistence{0}.cs\" />", Gerador.RetornaNomeClasseAjustado(t.Key.NomeObjeto).Replace(" ", ""))).ToList();
            var arquivosTO = (from KeyValuePair<TipoObjetoBanco, List<DadosColunas>> t in objetosMapeaveis select string.Format("<Compile Include=\"{0}TO.cs\" />", Gerador.RetornaNomeClasseAjustado(t.Key.NomeObjeto))).ToList();
            var arquivosDAL = (from KeyValuePair<TipoObjetoBanco, List<DadosColunas>> t in objetosMapeaveis select string.Format("<Compile Include=\"{0}DAL.cs\" />", Gerador.RetornaNomeClasseAjustado(t.Key.NomeObjeto))).ToList();
            var arquivosBLL = (from KeyValuePair<TipoObjetoBanco, List<DadosColunas>> t in objetosMapeaveis select string.Format("<Compile Include=\"{0}BLL.cs\" />", Gerador.RetornaNomeClasseAjustado(t.Key.NomeObjeto))).ToList();

            const string caminhoProjetoTo = "OTetalpmet";
            const string caminhoAssemblyTo = "ofnIylbmessA";
            const string caminhoProjetoInterface = "secafretnIetalpmeT";
            const string caminhoProjetoDAL = "LADetalpmet";
            const string caminhoProjetoBLL = "LLBetalpmet";
            const string caminhoProjetoSolution = "noituloS";

            var sbLog4Net = new System.Text.StringBuilder();

            sbLog4Net.AppendLine("<Reference Include=\"log4net, Version=1.2.10.0, Culture=neutral, PublicKeyToken=692fbea5521e1304, processorArchitecture=AMD64\">");
            sbLog4Net.AppendLine("<SpecificVersion>False</SpecificVersion>");
            sbLog4Net.AppendLine("<HintPath>..\\log4net.dll</HintPath>");
            sbLog4Net.AppendLine("</Reference>");

            #region Verifica se o projeto deve ser assinado

            var templateAssembly = Gerador.RetornaTextoBase(caminhoAssemblyTo);
            var tmpAssInfo = string.Empty;

            if (assinarProjeto)
            {
                if (string.IsNullOrEmpty(caminhoStrongName))
                {
                    var nomeSnkFile = string.Format("{0}.snk", nameSpace);

                    GerarKey(string.Format("{0}\\{1}", caminho, nomeSnkFile));
                    tmpAssInfo = string.Format("[assembly: AssemblyKeyFile({0}\"{1}{2}\")]", "@", "..\\", nomeSnkFile);
                }
                else
                {
                    tmpAssInfo = string.Format("[assembly: AssemblyKeyFile({0}\"{1}\")]", "@", caminhoStrongName);
                }
            }

            templateAssembly = templateAssembly.Replace("{sing}", tmpAssInfo);

            #endregion

            #region Gera Projeto Interface

            var trocasInterface = new Hashtable
                                          {
                                              {"[guid]", guidProjInterface},
                                              {"[namespace]", nameSpace},
                                              {"[arquivos]", string.Join("\n", arquivosInterface.ToArray())}
                                          };

            var projInterface = Gerador.RetornaTextoBase(caminhoProjetoInterface);
            projInterface = trocasInterface.Cast<DictionaryEntry>().Aggregate(projInterface, (current, entry) => current.Replace(entry.Key.ToString(), entry.Value.ToString()));
            projInterface = projInterface.Replace("[versao]", versaoFramework);

            var assemblyInterface = templateAssembly.Replace("[log]", "");
            assemblyInterface = assemblyInterface.Replace("[namespace]", nameSpace);
            assemblyInterface = assemblyInterface.Replace("[guid]", guidProjInterface);


            File.WriteAllText(string.Format("{0}\\Interfaces\\{1}Interfaces.csproj", caminho, nameSpace), projInterface);
            File.WriteAllText(string.Format("{0}\\Interfaces\\Properties\\AssemblyInfo.cs", caminho), assemblyInterface);

            #endregion

            #region Gera Projeto TO

            var trocasTo = new Hashtable
                                   {
                                       {"[guid]", guidProjTO},
                                       {"[namespace]", nameSpace},
                                       {"[guidInterface]", guidProjInterface},
                                       {"[arquivos]", string.Join("\n", arquivosTO.ToArray())}
                                   };

            var projTO = Gerador.RetornaTextoBase(caminhoProjetoTo);
            projTO = trocasTo.Cast<DictionaryEntry>().Aggregate(projTO, (current, entry) => current.Replace(entry.Key.ToString(), entry.Value.ToString()));
            projTO = projTO.Replace("[versao]", versaoFramework);

            var assemblyTO = templateAssembly.Replace("[camada]", "TO").Replace("[log]", "");
            assemblyTO = assemblyTO.Replace("[namespace]", nameSpace);
            assemblyTO = assemblyTO.Replace("[guid]", guidProjTO);


            File.WriteAllText(string.Format("{0}\\TO\\{1}TO.csproj", caminho, nameSpace), projTO);
            File.WriteAllText(string.Format("{0}\\TO\\Properties\\AssemblyInfo.cs", caminho), assemblyTO);

            #endregion

            #region Gera Projeto DAL

            var trocasDAL = new Hashtable
                                    {
                                        {"[guid]", guidProjDAL},
                                        {"[namespace]", nameSpace},
                                        {"[guidInterface]", guidProjInterface},
                                        {"[guidTO]",guidProjTO},
                                        {"[provider]",string.Format("<Compile Include=\"DataDrain\\Factories\\{0}.cs\" />", provider)},
                                        {"[arquivos]", string.Join("\n", arquivosDAL.ToArray())},
                                        {"[log]",!string.IsNullOrWhiteSpace(xmlLog4Net)?sbLog4Net.ToString():""},
                                    };

            var projDAL = Gerador.RetornaTextoBase(caminhoProjetoDAL);



            projDAL = trocasDAL.Cast<DictionaryEntry>().Aggregate(projDAL, (current, entry) => current.Replace(entry.Key.ToString(), entry.Value.ToString()));
            projDAL = projDAL.Replace("[versao]", versaoFramework);


            var assemblyDAL = templateAssembly.Replace("[camada]", "DAL").Replace("[log]", !string.IsNullOrWhiteSpace(xmlLog4Net) ? "[assembly: log4net.Config.XmlConfigurator(Watch = true)]" : ""); ;
            assemblyDAL = assemblyDAL.Replace("[namespace]", nameSpace);
            assemblyDAL = assemblyDAL.Replace("[guid]", guidProjDAL);


            File.WriteAllText(string.Format("{0}\\DAL\\{1}DAL.csproj", caminho, nameSpace), projDAL);
            File.WriteAllText(string.Format("{0}\\DAL\\Properties\\AssemblyInfo.cs", caminho), assemblyDAL);

            #endregion

            #region Gera Projeto BLL

            var trocasBLL = new Hashtable
                                    {
                                        {"[guid]", guidProjBLL},
                                        {"[namespace]", nameSpace},
                                        {"[guidInterface]", guidProjInterface},
                                        {"[guidTO]",guidProjTO},
                                        {"[guidDAL]",guidProjDAL},
                                        {"[arquivos]", string.Join("\n", arquivosBLL.ToArray())},
                                        {"[log]",!string.IsNullOrWhiteSpace(xmlLog4Net)?sbLog4Net.ToString():""}
                                    };

            var projBLL = Gerador.RetornaTextoBase(caminhoProjetoBLL);
            projBLL = trocasBLL.Cast<DictionaryEntry>().Aggregate(projBLL, (current, entry) => current.Replace(entry.Key.ToString(), entry.Value.ToString()));
            projBLL = projBLL.Replace("[versao]", versaoFramework);

            var assemblyBLL = templateAssembly.Replace("[camada]", "bll").Replace("[log]", !string.IsNullOrWhiteSpace(xmlLog4Net) ? "[assembly: log4net.Config.XmlConfigurator(Watch = true)]" : ""); ; ;
            assemblyBLL = assemblyBLL.Replace("[namespace]", nameSpace);
            assemblyBLL = assemblyBLL.Replace("[guid]", guidProjBLL);


            File.WriteAllText(string.Format("{0}\\BLL\\{1}BLL.csproj", caminho, nameSpace), projBLL);
            File.WriteAllText(string.Format("{0}\\BLL\\Properties\\AssemblyInfo.cs", caminho), assemblyBLL);

            #endregion

            #region Gera Projeto Solution

            var trocasSolution = new Hashtable
                                         {
                                             {"[guid]", guidProjSolution},
                                             {"[namespace]", nameSpace},
                                             {"[guidTO]", guidProjTO},
                                             {"[guidDAL]", guidProjDAL},
                                             {"[guidInterface]",guidProjInterface},
                                             {"[guidBLL]",guidProjBLL}
                                         };

            var projSolution = Gerador.RetornaTextoBase(caminhoProjetoSolution);

            switch (versaoFramework)
            {
                case "v2.0":
                    projSolution = projSolution.Replace("2010", "2008");
                    projSolution = projSolution.Replace("11.00", "10.00");
                    break;
                case "v4.5":
                    projSolution = projSolution.Replace("2010", "2012");
                    projSolution = projSolution.Replace("11.00", "12.00");
                    break;
            }

            projSolution = trocasSolution.Cast<DictionaryEntry>().Aggregate(projSolution, (current, entry) => current.Replace(entry.Key.ToString(), entry.Value.ToString()));

            File.WriteAllText(string.Format("{0}\\{1}.sln", caminho, nameSpace), projSolution);

            #endregion

            Directory.Move(string.Format("{0}\\TO", caminho), string.Format("{0}\\{1}TO", caminho, nameSpace.Replace(".", "")));
            Directory.Move(string.Format("{0}\\DAL", caminho), string.Format("{0}\\{1}DAL", caminho, nameSpace.Replace(".", "")));
            Directory.Move(string.Format("{0}\\BLL", caminho), string.Format("{0}\\{1}BLL", caminho, nameSpace.Replace(".", "")));
            Directory.Move(string.Format("{0}\\Interfaces", caminho), string.Format("{0}\\{1}Interfaces", caminho, nameSpace.Replace(".", "")));

        }


        /// <summary>
        /// Gera uma chave de assinatura
        /// </summary>
        /// <param name="keyName"></param>
        private static void GerarKey(string keyName)
        {

            var cspParams = new CspParameters
            {
                ProviderType = 1,
                Flags = CspProviderFlags.UseArchivableKey,
                KeyNumber = (int)KeyNumber.Signature
            };

            var rsaProvider = new RSACryptoServiceProvider(cspParams);

            var array = rsaProvider.ExportCspBlob(!rsaProvider.PublicOnly);

            using (var fs = new FileStream(keyName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(array, 0, array.Length);
            }

        }

    }
}
