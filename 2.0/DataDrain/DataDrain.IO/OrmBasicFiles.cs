using System.IO;
using System.Linq;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.IO
{
    public class OrmBasicFiles
    {
        private static ITemplateText _template;

        public OrmBasicFiles(ITemplateText template)
        {
            _template = template;
        }

        public void Create(Configuration parametro, string nomeProvider)
        {
            CreateLinqFiles(parametro, nomeProvider);
            CreateBusinessFiles(parametro.DestinationPath, parametro.NameSpace);
            CreatePocoFiles(parametro.DestinationPath);
            CreateInterfaceFiles(parametro.DestinationPath, parametro.NameSpace);

        }

        private static void CreateInterfaceFiles(string path, string strNamespace)
        {
            var strongNamePermission = _template.GelValue("StrongNamePermission");
            var iFactory = FileFactory.RetornaTextoBase("IFactory").Replace("{namespace}", strNamespace);
            var iPersistence = FileFactory.RetornaTextoBase("IPersistence").Replace("{namespace}", strNamespace);
            var iUnityOfWork = FileFactory.RetornaTextoBase("IUnityOfWork").Replace("{namespace}", strNamespace);

            File.WriteAllText(string.Format("{0}\\Interfaces\\StrongNamePermission.cs", path), strongNamePermission.Value.Replace("{namespace}", strNamespace));
            File.WriteAllText(string.Format("{0}\\Interfaces\\IFactory.cs", path), iFactory);
            File.WriteAllText(string.Format("{0}\\Interfaces\\IPersistence.cs", path), iPersistence);
            File.WriteAllText(string.Format("{0}\\Interfaces\\IUnityOfWork.cs", path), iUnityOfWork);

        }

        private static void CreatePocoFiles(string caminho)
        {
            var domainValidator = _template.GelValue("DomainValidator");

            File.WriteAllText(string.Format("{0}\\TO\\AttributeValidators\\DomainValidator.cs", caminho), domainValidator.Value);
        }

        private static void CreateBusinessFiles(string caminho, string strNamespace)
        {
            var validar = _template.GelValue("Validar");
            var crudBaseBll = _template.GelValue("CrudBaseBLL");

            File.WriteAllText(string.Format("{0}\\BLL\\Validacao\\Validar.cs", caminho), validar.Value.Replace("TesteBLL.Validacao", string.Format("{0}BLL.Validacao", strNamespace)));
            File.WriteAllText(string.Format("{0}\\BLL\\Base\\CrudBaseBLL.cs", caminho), crudBaseBll.Value.Replace("{namespace}", strNamespace));
        }

        private static void CreateLinqFiles(Configuration parametro, string nomeProvider)
        {

            // ReSharper disable ResourceItemNotResolved
            var dbExpressions = _template.GelValue("DbExpressions").Value;
            var dbExpressionVisitor = _template.GelValue("DbExpressionVisitor").Value;
            var expressionVisitor = _template.GelValue("ExpressionVisitor").Value;
            var queryExecutor = _template.GelValue("QueryExecutor").Value;
            var queryLanguage = _template.GelValue("QueryLanguage").Value;
            var queryMapping = _template.GelValue("QueryMapping").Value;
            var queryTypeSystem = _template.GelValue("QueryTypeSystem").Value;
            var sqlFormatter = _template.GelValue("SqlFormatter").Value;

            var isSqlFormatter = _template.GelValue("ISqlFormatter").Value ?? "";
            var provider = _template.GelValue(nomeProvider).Value;


            var funcoesCrud = parametro.TiposObjetosAcaoBanco.Aggregate(_template.GelValue("FuncoesCrud").Value, (current, param) => current.Replace(param.Key, param.Value)).Replace("{namespace}", parametro.NameSpace);

            var sqlLanguage = _template.GelValue("SqlLanguage").Value;

            var crudBase = parametro.TiposObjetosAcaoBanco.Aggregate(_template.GelValue("CrudBase").Value, (current, param) => current.Replace(param.Key, param.Value));
            var singleton = parametro.TiposObjetosAcaoBanco.Aggregate(_template.GelValue("Singleton").Value, (current, param) => current.Replace(param.Key, param.Value));

            var eTipoConsulta = _template.GelValue("ETipoConsulta").Value;
            var cmdMap = parametro.TiposObjetosAcaoBanco.Aggregate(_template.GelValue("CmdMap").Value, (current, param) => current.Replace(param.Key, param.Value));
            var transaction = parametro.TiposObjetosAcaoBanco.Aggregate(_template.GelValue("Transaction").Value, (current, param) => current.Replace(param.Key, param.Value));

            var fastInvoke = _template.GelValue("FastInvoke").Value;
            var mapExtension = _template.GelValue("MapExtension").Value;
            var cachingMannager = _template.GelValue("CachingMannager").Value;
            var eCacheAcao = _template.GelValue("ECacheAcao").Value;
            var cacheChangedEventArgs = _template.GelValue("CacheChangedEventArgs").Value;
            var cacheChangedEventHandler = _template.GelValue("CacheChangedEventHandler").Value;
            var cachingProvider = _template.GelValue("ICachingProvider").Value;


            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\DbExpressions.cs", parametro.DestinationPath), dbExpressions);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\DbExpressionVisitor.cs", parametro.DestinationPath), dbExpressionVisitor);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\ExpressionVisitor.cs", parametro.DestinationPath), expressionVisitor);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryExecutor.cs", parametro.DestinationPath), queryExecutor);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryLanguage.cs", parametro.DestinationPath), queryLanguage);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryMapping.cs", parametro.DestinationPath), queryMapping);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryTypeSystem.cs", parametro.DestinationPath), queryTypeSystem);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\SqlFormatter.cs", parametro.DestinationPath), sqlFormatter);

            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\FastInvoke.cs", parametro.DestinationPath), fastInvoke);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\MapExtension.cs", parametro.DestinationPath), mapExtension);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\CachingMannager.cs", parametro.DestinationPath), cachingMannager);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Enuns\\ECacheAcao.cs", parametro.DestinationPath), eCacheAcao);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Events\\CacheChangedEventArgs.cs", parametro.DestinationPath), cacheChangedEventArgs);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Events\\CacheChangedEventHandler.cs", parametro.DestinationPath), cacheChangedEventHandler);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Interfaces\\ICachingProvider.cs", parametro.DestinationPath), cachingProvider);



            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\Factories\\ISqlFormatter.cs", parametro.DestinationPath), isSqlFormatter.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\Factories\\{1}.cs", parametro.DestinationPath, nomeProvider), provider.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));

            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\FuncoesCrud.cs", parametro.DestinationPath), funcoesCrud.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\SqlLanguage.cs", parametro.DestinationPath), sqlLanguage.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));

            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\Base\\CrudBase.cs", parametro.DestinationPath), crudBase.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)).Replace("{namespace}", parametro.NameSpace));
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\Conexao\\Singleton.cs", parametro.DestinationPath), singleton.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\Enumeradores\\ETipoConsulta.cs", parametro.DestinationPath), eTipoConsulta.Replace("TesteDAL", string.Format("{0}DAL", parametro.NameSpace)));
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\CommandMap\\CmdMap.cs", parametro.DestinationPath), cmdMap);
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\CommandMap\\Transaction.cs", parametro.DestinationPath), transaction);

        }
    }
}
