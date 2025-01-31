﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.IO
{
    public class OrmProjectFiles
    {
        public static void Create(Configuration parametros, string provider)
        {

            var guidProjInterface = Guid.NewGuid().ToString().ToUpper();
            var guidProjTO = Guid.NewGuid().ToString().ToUpper().ToUpper();
            var guidProjDal = Guid.NewGuid().ToString().ToUpper().ToUpper();
            var guidProjBll = Guid.NewGuid().ToString().ToUpper().ToUpper();
            var guidProjWcf = Guid.NewGuid().ToString().ToUpper().ToUpper();


            var arquivosInterface = (from DatabaseObjectInfo t in parametros.ObjetosMapeaveis select string.Format("<Compile Include=\"IPersistence{0}.cs\" />", FileFactory.RetornaNomeClasseAjustado(t.Name).Replace(" ", ""))).ToList();
            var arquivosTO = (from DatabaseObjectInfo t in parametros.ObjetosMapeaveis select string.Format("<Compile Include=\"{0}TO.cs\" />", FileFactory.RetornaNomeClasseAjustado(t.Name))).ToList();
            var arquivosDal = (from DatabaseObjectInfo t in parametros.ObjetosMapeaveis select string.Format("<Compile Include=\"{0}DAL.cs\" />", FileFactory.RetornaNomeClasseAjustado(t.Name))).ToList();
            var arquivosBll = (from DatabaseObjectInfo t in parametros.ObjetosMapeaveis select string.Format("<Compile Include=\"{0}BLL.cs\" />", FileFactory.RetornaNomeClasseAjustado(t.Name))).ToList();

            var sbLog4Net = new StringBuilder();

            sbLog4Net.AppendLine("<Reference Include=\"log4net, Version=1.2.10.0, Culture=neutral, PublicKeyToken=692fbea5521e1304, processorArchitecture=AMD64\">");
            sbLog4Net.AppendLine("<SpecificVersion>False</SpecificVersion>");
            sbLog4Net.AppendLine("<HintPath>..\\log4net.dll</HintPath>");
            sbLog4Net.AppendLine("</Reference>");

            var templateAssembly = AssinaProjeto(parametros);

            GeraProjetoInterface(parametros, guidProjInterface, arquivosInterface, templateAssembly);
            GeraProjetoTo(parametros, guidProjTO, guidProjInterface, arquivosTO, templateAssembly);
            GeraProjetoDal(parametros, provider, guidProjDal, guidProjInterface, guidProjTO, arquivosDal, sbLog4Net, templateAssembly);
            GeraProjetoBll(parametros, guidProjBll, guidProjInterface, guidProjTO, guidProjDal, arquivosBll, sbLog4Net, templateAssembly);
            
            if (parametros.MapWcf)
            {
                GeraProjetoWcf(parametros, guidProjWcf);
            }
            else
            {
                guidProjWcf = null;
            }
            
            GeraSoluction(parametros, guidProjTO, guidProjDal, guidProjInterface, guidProjBll, guidProjWcf);

            Directory.Move(string.Format("{0}\\TO", parametros.DestinationPath), string.Format("{0}\\{1}TO", parametros.DestinationPath, parametros.NameSpace.Replace(".", "")));
            Directory.Move(string.Format("{0}\\DAL", parametros.DestinationPath), string.Format("{0}\\{1}DAL", parametros.DestinationPath, parametros.NameSpace.Replace(".", "")));
            Directory.Move(string.Format("{0}\\BLL", parametros.DestinationPath), string.Format("{0}\\{1}BLL", parametros.DestinationPath, parametros.NameSpace.Replace(".", "")));
            Directory.Move(string.Format("{0}\\Interfaces", parametros.DestinationPath), string.Format("{0}\\{1}Interfaces", parametros.DestinationPath, parametros.NameSpace.Replace(".", "")));

        }

        private static void GeraSoluction(Configuration parametros, string guidProjTO, string guidProjDAL, string guidProjInterface, string guidProjBLL, string guidProjWcf)
        {
            var trocasSolution = new Hashtable
            {
                {"[guid]", Guid.NewGuid().ToString().ToUpper().ToUpper()},
                {"[namespace]", parametros.NameSpace},
                {"[guidTO]", guidProjTO},
                {"[guidDAL]", guidProjDAL},
                {"[guidInterface]", guidProjInterface},
                {"[guidBLL]", guidProjBLL}
            };

            var projSolution = FileFactory.RetornaTextoBase("noituloS");

            if (parametros.MapWcf)
            {
                projSolution = projSolution.Replace("[guidWcf]", string.Format("Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{0}Wcf\", \"{0}Wcf\\{0}Wcf.csproj\", \"{{53034ACE-081D-4CDA-8075-D3B158C7DAF9}}\"{1}EndProject", parametros.NameSpace, Environment.NewLine));
                projSolution = projSolution.Replace("[guidGlobalWcf]", string.Format("{{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU{1}{{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU{1}{{{0}}}.Release|Any CPU.ActiveCfg = Release|Any CPU{1}{{{0}}}.Release|Any CPU.Build.0 = Release|Any CPU", guidProjWcf, Environment.NewLine));
            }
            else
            {
                projSolution = projSolution.Replace("[guidWcf]", "");
                projSolution = projSolution.Replace("[guidGlobalWcf]", "");
            }

            switch (parametros.VersaoFramework)
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

            File.WriteAllText(string.Format("{0}\\{1}.sln", parametros.DestinationPath, parametros.NameSpace), projSolution);
        }

        private static void GeraProjetoBll(Configuration parametros, string guidProjBLL, string guidProjInterface, string guidProjTO, string guidProjDAL, List<string> arquivosBLL, StringBuilder sbLog4Net, string templateAssembly)
        {
            var trocasBLL = new Hashtable
            {
                {"[guid]", guidProjBLL},
                {"[namespace]", parametros.NameSpace},
                {"[guidInterface]", guidProjInterface},
                {"[guidTO]", guidProjTO},
                {"[guidDAL]", guidProjDAL},
                {"[arquivos]", string.Join("\n", arquivosBLL.ToArray())},
            };

            var projBLL = FileFactory.RetornaTextoBase("LLBetalpmet").Replace("[log]", "");
            projBLL = trocasBLL.Cast<DictionaryEntry>().Aggregate(projBLL, (current, entry) => current.Replace(entry.Key.ToString(), entry.Value.ToString()));
            projBLL = projBLL.Replace("[versao]", parametros.VersaoFramework);

            var assemblyBLL = templateAssembly.Replace("[camada]", "bll").Replace("[log]", "");
            assemblyBLL = assemblyBLL.Replace("[namespace]", parametros.NameSpace);
            assemblyBLL = assemblyBLL.Replace("[guid]", guidProjBLL);


            File.WriteAllText(string.Format("{0}\\BLL\\{1}BLL.csproj", parametros.DestinationPath, parametros.NameSpace), projBLL);
            File.WriteAllText(string.Format("{0}\\BLL\\Properties\\AssemblyInfo.cs", parametros.DestinationPath), assemblyBLL);
        }

        private static void GeraProjetoDal(Configuration parametros, string provider, string guidProjDAL, string guidProjInterface, string guidProjTO, List<string> arquivosDAL, StringBuilder sbLog4Net, string templateAssembly)
        {
            var trocasDAL = new Hashtable
            {
                {"[guid]", guidProjDAL},
                {"[namespace]", parametros.NameSpace},
                {"[guidInterface]", guidProjInterface},
                {"[guidTO]", guidProjTO},
                {"[provider]", string.Format("<Compile Include=\"DataDrain\\Factories\\{0}.cs\" />", provider)},
                {"[arquivos]", string.Join("\n", arquivosDAL.ToArray())},
            };

            var projDAL = FileFactory.RetornaTextoBase("SSLADetalpmet").Replace("[log]", "");

            projDAL = trocasDAL.Cast<DictionaryEntry>().Aggregate(projDAL, (current, entry) => current.Replace(entry.Key.ToString(), entry.Value.ToString()));
            projDAL = projDAL.Replace("[versao]", parametros.VersaoFramework);


            var assemblyDAL = templateAssembly.Replace("[camada]", "DAL").Replace("[log]", "");
            
            assemblyDAL = assemblyDAL.Replace("[namespace]", parametros.NameSpace);
            assemblyDAL = assemblyDAL.Replace("[guid]", guidProjDAL);


            File.WriteAllText(string.Format("{0}\\DAL\\{1}DAL.csproj", parametros.DestinationPath, parametros.NameSpace), projDAL);
            File.WriteAllText(string.Format("{0}\\DAL\\Properties\\AssemblyInfo.cs", parametros.DestinationPath), assemblyDAL);
        }

        private static void GeraProjetoTo(Configuration parametros, string guidProjTO, string guidProjInterface, List<string> arquivosTO, string templateAssembly)
        {
            var trocasTo = new Hashtable
            {
                {"[guid]", guidProjTO},
                {"[namespace]", parametros.NameSpace},
                {"[guidInterface]", guidProjInterface},
                {"[arquivos]", string.Join("\n", arquivosTO.ToArray())}
            };

            var projTO = FileFactory.RetornaTextoBase("OTetalpmet");
            projTO = trocasTo.Cast<DictionaryEntry>().Aggregate(projTO, (current, entry) => current.Replace(entry.Key.ToString(), entry.Value.ToString()));
            projTO = projTO.Replace("[versao]", parametros.VersaoFramework);

            var assemblyTO = templateAssembly.Replace("[camada]", "TO").Replace("[log]", "");
            assemblyTO = assemblyTO.Replace("[namespace]", parametros.NameSpace);
            assemblyTO = assemblyTO.Replace("[guid]", guidProjTO);


            File.WriteAllText(string.Format("{0}\\TO\\{1}TO.csproj", parametros.DestinationPath, parametros.NameSpace), projTO);
            File.WriteAllText(string.Format("{0}\\TO\\Properties\\AssemblyInfo.cs", parametros.DestinationPath), assemblyTO);
        }

        private static void GeraProjetoInterface(Configuration parametros, string guidProjInterface, List<string> arquivosInterface, string templateAssembly)
        {
            var trocasInterface = new Hashtable
            {
                {"[guid]", guidProjInterface},
                {"[namespace]", parametros.NameSpace},
                {"[arquivos]", string.Join("\n", arquivosInterface.ToArray())}
            };

            var projInterface = FileFactory.RetornaTextoBase("secafretnIetalpmeT");
            projInterface = trocasInterface.Cast<DictionaryEntry>().Aggregate(projInterface, (current, entry) => current.Replace(entry.Key.ToString(), entry.Value.ToString()));
            projInterface = projInterface.Replace("[versao]", parametros.VersaoFramework);

            var assemblyInterface = templateAssembly.Replace("[log]", "");
            assemblyInterface = assemblyInterface.Replace("[namespace]", parametros.NameSpace);
            assemblyInterface = assemblyInterface.Replace("[guid]", guidProjInterface);

            File.WriteAllText(string.Format("{0}\\Interfaces\\{1}Interfaces.csproj", parametros.DestinationPath, parametros.NameSpace), projInterface);
            File.WriteAllText(string.Format("{0}\\Interfaces\\Properties\\AssemblyInfo.cs", parametros.DestinationPath), assemblyInterface);
        }

        private static void GeraProjetoWcf(Configuration parametros, string guidProjWcf)
        {
            var servicos = new List<string>();
            var servicosCode = new List<string>();
            var servicosInterface = new List<string>();

            Directory.CreateDirectory(string.Format("{0}\\{1}Wcf\\", parametros.DestinationPath, parametros.NameSpace));
            Directory.CreateDirectory(string.Format("{0}\\{1}Wcf\\Interface", parametros.DestinationPath, parametros.NameSpace));
            Directory.CreateDirectory(string.Format("{0}\\{1}Wcf\\Properties", parametros.DestinationPath, parametros.NameSpace));

            var assemblyInfoWcf = FileFactory.RetornaTextoBase("AssemblyInfoWcf").Replace("{namespace}", parametros.NameSpace).Replace("{guid}", Guid.NewGuid().ToString("D"));
            File.WriteAllText(string.Format("{0}\\{1}Wcf\\Properties\\AssemblyInfo.cs", parametros.DestinationPath, parametros.NameSpace), assemblyInfoWcf);

            foreach (var classe in parametros.ObjetosMapeaveis)
            {
                File.WriteAllText(string.Format("{0}\\{1}Wcf\\{2}.cs", parametros.DestinationPath, parametros.NameSpace, classe.Name), string.Format("<%@ ServiceHost Language=\"C#\" Debug=\"true\" Service=\"CorpWcf.Aceite\" CodeBehind=\"{0}.svc.cs\" %>", classe.Name));

                var corpoServico = FileFactory.RetornaTextoBase("corpoServico").Replace("{namespace}", parametros.NameSpace).Replace("{classe}", classe.Name);
                File.WriteAllText(string.Format("{0}\\{1}Wcf\\{2}.svc.cs", parametros.DestinationPath, parametros.NameSpace, classe.Name), corpoServico);

                var corpoInterfaceServico = FileFactory.RetornaTextoBase("corpoInterfaceServico").Replace("{namespace}", parametros.NameSpace).Replace("{classe}", classe.Name); ;
                File.WriteAllText(string.Format("{0}\\{1}Wcf\\Interface\\I{2}.cs", parametros.DestinationPath, parametros.NameSpace, classe.Name), corpoInterfaceServico);

                servicos.Add(string.Format("<Content Include=\"{0}.svc\" />", classe.Name));
                servicosCode.Add(string.Format("<Compile Include=\"{0}.svc.cs\">{1}<DependentUpon>{0}.svc</DependentUpon>{1}</Compile>", classe.Name, Environment.NewLine));
                servicosInterface.Add(string.Format("<Compile Include=\"Interface\\I{0}.cs\" />", classe.Name));
            }

            var corpoProjetoWcf = FileFactory.RetornaTextoBase("corpoProjetoWcf").Replace("{namespace}", parametros.NameSpace)
                .Replace("{servicos}", string.Join("\n", servicos))
                .Replace("{servicosCode}", string.Join("\n", servicosCode))
                .Replace("[guid]", guidProjWcf)
                .Replace("{servicosInterface}", string.Join("\n", servicosInterface));

            File.WriteAllText(string.Format("{0}\\{1}Wcf\\{2}Wcf.csproj", parametros.DestinationPath, parametros.NameSpace, parametros.NameSpace), corpoProjetoWcf);

            var corpoWebConfig = FileFactory.RetornaTextoBase("corpoWebConfig");
            File.WriteAllText(string.Format("{0}\\{1}Wcf\\Web.config", parametros.DestinationPath, parametros.NameSpace), corpoWebConfig);

        }

        private static string AssinaProjeto(Configuration parametros)
        {
            var templateAssembly = FileFactory.RetornaTextoBase("ofnIylbmessA");
            var tmpAssInfo = string.Empty;

            if (parametros.AssinarProjeto)
            {
                if (string.IsNullOrEmpty(parametros.CaminhoStrongName))
                {
                    var nomeSnkFile = string.Format("{0}.snk", parametros.NameSpace);

                    GerarKey(string.Format("{0}\\{1}", parametros.DestinationPath, nomeSnkFile));
                    tmpAssInfo = string.Format("[assembly: AssemblyKeyFile({0}\"{1}{2}\")]", "@", "..\\", nomeSnkFile);
                }
                else
                {
                    tmpAssInfo = string.Format("[assembly: AssemblyKeyFile({0}\"{1}\")]", "@", parametros.CaminhoStrongName);
                }
            }

            templateAssembly = templateAssembly.Replace("{sing}", tmpAssInfo);
            return templateAssembly;
        }

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
