using System.IO;
using System.Linq;
using DataDrain.ORM.DAL.Templates;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.DAL
{
    public class ArquivosORM
    {
        /// <summary>
        /// Gera os arquivos base para o ORM
        /// </summary>
        /// <param name="caminho">caminho para gravação dos arquivos</param>
        /// <param name="strNamespace">NameSpace usado no projeto</param>
        public static void GravaArquivosBaseOrm(ParametrosCriarProjetos parametro, string nomeProvider)
        {
            GeraArquivosLINQ(parametro, nomeProvider);
            GeraArquivosBLL(parametro.CaminhoDestino, parametro.NameSpace);
            GeraArquivosTO(parametro.CaminhoDestino);
            GeraArquivosInterface(parametro.CaminhoDestino, parametro.NameSpace);

        }

        private static void GeraArquivosInterface(string caminho, string strNamespace)
        {
            var strongNamePermission = Template.RetornaValor("StrongNamePermission");

            File.WriteAllText(string.Format("{0}\\Interfaces\\StrongNamePermission.cs", caminho), strongNamePermission.Value.Replace("{namespace}", strNamespace));
        }

        private static void GeraArquivosTO(string caminho)
        {
            var domainValidator = Template.RetornaValor("DomainValidator");

            File.WriteAllText(string.Format("{0}\\TO\\AttributeValidators\\DomainValidator.cs", caminho), domainValidator.Value);
        }

        private static void GeraArquivosBLL(string caminho, string strNamespace)
        {
            var validar = Template.RetornaValor("Validar");
            var crudBaseBll = Template.RetornaValor("CrudBaseBLL");

            File.WriteAllText(string.Format("{0}\\BLL\\Validacao\\Validar.cs", caminho), validar.Value.Replace("TesteBLL.Validacao", string.Format("{0}BLL.Validacao", strNamespace)));
            File.WriteAllText(string.Format("{0}\\BLL\\Base\\CrudBaseBLL.cs", caminho), crudBaseBll.Value.Replace("Corp", strNamespace));
        }

        private static void GeraArquivosLINQ(ParametrosCriarProjetos parametro, string nomeProvider)
        {

            // ReSharper disable ResourceItemNotResolved
            var dbExpressions = Template.RetornaValor("DbExpressions").Value;
            var dbExpressionVisitor = Template.RetornaValor("DbExpressionVisitor").Value;
            var expressionVisitor = Template.RetornaValor("ExpressionVisitor").Value;
            var queryExecutor = Template.RetornaValor("QueryExecutor").Value;
            var queryLanguage = Template.RetornaValor("QueryLanguage").Value;
            var queryMapping = Template.RetornaValor("QueryMapping").Value;
            var queryTypeSystem = Template.RetornaValor("QueryTypeSystem").Value;
            var sqlFormatter = Template.RetornaValor("SqlFormatter").Value;

            var ISqlFormatter = Template.RetornaValor("ISqlFormatter").Value;
            var provider = Template.RetornaValor(nomeProvider).Value;

            var funcoesCrud = parametro.TiposObjetosAcaoBanco.Aggregate(Template.RetornaValor("FuncoesCrud").Value, (current, param) => current.Replace(param.Key, param.Value));

            var sqlLanguage = Template.RetornaValor("SqlLanguage").Value;

            var crudBase = parametro.TiposObjetosAcaoBanco.Aggregate(Template.RetornaValor("CrudBase").Value, (current, param) => current.Replace(param.Key, param.Value));
            var singleton = parametro.TiposObjetosAcaoBanco.Aggregate(Template.RetornaValor("Singleton").Value, (current, param) => current.Replace(param.Key, param.Value));
            
            var eTipoConsulta = Template.RetornaValor("ETipoConsulta").Value;
            var cmdMap = parametro.TiposObjetosAcaoBanco.Aggregate(Template.RetornaValor("CmdMap").Value, (current, param) => current.Replace(param.Key, param.Value));
            var transaction = parametro.TiposObjetosAcaoBanco.Aggregate(Template.RetornaValor("Transaction").Value, (current, param) => current.Replace(param.Key, param.Value));

            var fastInvoke = Template.RetornaValor("FastInvoke").Value;
            var mapExtension = Template.RetornaValor("MapExtension").Value;
            var cachingMannager = Template.RetornaValor("CachingMannager").Value;
            var eCacheAcao = Template.RetornaValor("ECacheAcao").Value;
            var cacheChangedEventArgs = Template.RetornaValor("CacheChangedEventArgs").Value;
            var cacheChangedEventHandler = Template.RetornaValor("CacheChangedEventHandler").Value;
            var cachingProvider = Template.RetornaValor("ICachingProvider").Value;


            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\DbExpressions.cs", parametro.CaminhoDestino), dbExpressions);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\DbExpressionVisitor.cs", parametro.CaminhoDestino), dbExpressionVisitor);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\ExpressionVisitor.cs", parametro.CaminhoDestino), expressionVisitor);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryExecutor.cs", parametro.CaminhoDestino), queryExecutor);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryLanguage.cs", parametro.CaminhoDestino), queryLanguage);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryMapping.cs", parametro.CaminhoDestino), queryMapping);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryTypeSystem.cs", parametro.CaminhoDestino), queryTypeSystem);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\SqlFormatter.cs", parametro.CaminhoDestino), sqlFormatter);

            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\FastInvoke.cs", parametro.CaminhoDestino), fastInvoke);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\MapExtension.cs", parametro.CaminhoDestino), mapExtension);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\CachingMannager.cs", parametro.CaminhoDestino), cachingMannager);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Enuns\\ECacheAcao.cs", parametro.CaminhoDestino), eCacheAcao);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Events\\CacheChangedEventArgs.cs", parametro.CaminhoDestino), cacheChangedEventArgs);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Events\\CacheChangedEventHandler.cs", parametro.CaminhoDestino), cacheChangedEventHandler);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Interfaces\\ICachingProvider.cs", parametro.CaminhoDestino), cachingProvider);



            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\Factories\\ISqlFormatter.cs", parametro.CaminhoDestino), ISqlFormatter.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\Factories\\{1}.cs", parametro.CaminhoDestino, nomeProvider), provider.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));

            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\FuncoesCrud.cs", parametro.CaminhoDestino), funcoesCrud.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\SqlLanguage.cs", parametro.CaminhoDestino), sqlLanguage.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));

            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\Base\\CrudBase.cs", parametro.CaminhoDestino), crudBase.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)).Replace("{namespace}", parametro.NameSpace));
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\Conexao\\Singleton.cs", parametro.CaminhoDestino), singleton.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\Enumeradores\\ETipoConsulta.cs", parametro.CaminhoDestino), eTipoConsulta.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\CommandMap\\CmdMap.cs", parametro.CaminhoDestino), cmdMap);
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\CommandMap\\Transaction.cs", parametro.CaminhoDestino), transaction);

        }
    }
}
