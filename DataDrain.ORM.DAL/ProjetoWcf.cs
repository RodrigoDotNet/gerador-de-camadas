using System;
using System.Collections.Generic;
using System.IO;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL
{
    public sealed class ProjetoWcf
    {
        public static void CriarProjeto(ParametrosCriarProjetos parametros)
        {
            var servicos = new List<string>();
            var servicosCode = new List<string>();
            var servicosInterface = new List<string>();

            Directory.CreateDirectory(string.Format("{0}\\{1}Wcf\\", parametros.CaminhoDestino, parametros.NameSpace));
            Directory.CreateDirectory(string.Format("{0}\\{1}Wcf\\Interface", parametros.CaminhoDestino, parametros.NameSpace));
            Directory.CreateDirectory(string.Format("{0}\\{1}Wcf\\Properties", parametros.CaminhoDestino, parametros.NameSpace));

            var assemblyInfoWcf = Gerador.RetornaTextoBase("AssemblyInfoWcf").Replace("{namespace}", parametros.NameSpace).Replace("{guid}", Guid.NewGuid().ToString("D"));
            File.WriteAllText(string.Format("{0}\\{1}Wcf\\Properties\\AssemblyInfo.cs", parametros.CaminhoDestino, parametros.NameSpace), assemblyInfoWcf);

            foreach (var classe in parametros.ObjetosMapeaveis)
            {
                File.WriteAllText(string.Format("{0}\\{1}Wcf\\{2}.cs", parametros.CaminhoDestino, parametros.NameSpace, classe.Key.NomeObjeto), string.Format("<%@ ServiceHost Language=\"C#\" Debug=\"true\" Service=\"CorpWcf.Aceite\" CodeBehind=\"{0}.svc.cs\" %>", classe.Key.NomeObjeto));

                var corpoServico = Gerador.RetornaTextoBase("corpoServico").Replace("{namespace}", parametros.NameSpace).Replace("{classe}", classe.Key.NomeObjeto);
                File.WriteAllText(string.Format("{0}\\{1}Wcf\\{2}.svc.cs", parametros.CaminhoDestino, parametros.NameSpace, classe.Key.NomeObjeto), corpoServico);

                var corpoInterfaceServico = Gerador.RetornaTextoBase("corpoInterfaceServico").Replace("{namespace}", parametros.NameSpace).Replace("{classe}", classe.Key.NomeObjeto); ;
                File.WriteAllText(string.Format("{0}\\{1}Wcf\\Interface\\I{2}.cs", parametros.CaminhoDestino, parametros.NameSpace, classe.Key.NomeObjeto), corpoInterfaceServico);

                servicos.Add(string.Format("<Content Include=\"{0}.svc\" />",classe.Key.NomeObjeto));
                servicosCode.Add(string.Format("<Compile Include=\"{0}.svc.cs\">{1}<DependentUpon>{0}.svc</DependentUpon>{1}</Compile>", classe.Key.NomeObjeto, Environment.NewLine));
                servicosInterface.Add(string.Format("<Compile Include=\"Interface\\I{0}.cs\" />", classe.Key.NomeObjeto));
            }

            var corpoProjetoWcf = Gerador.RetornaTextoBase("corpoProjetoWcf").Replace("{namespace}", parametros.NameSpace)
                .Replace("{servicos}",string.Join("\n",servicos))
                .Replace("{servicosCode}", string.Join("\n", servicosCode))
                .Replace("{servicosInterface}", string.Join("\n", servicosInterface));

            File.WriteAllText(string.Format("{0}\\{1}Wcf\\{2}Wcf.csproj", parametros.CaminhoDestino, parametros.NameSpace, parametros.NameSpace), corpoProjetoWcf);

            var corpoWebConfig = Gerador.RetornaTextoBase("corpoWebConfig");
            File.WriteAllText(string.Format("{0}\\{1}Wcf\\Web.config", parametros.CaminhoDestino, parametros.NameSpace), corpoWebConfig);

        }
    }
}
