using System;
using System.IO;
using System.Reflection;
using System.Resources;

namespace DataDrain.ORM.DAL
{
    public class ArquivosORM
    {
        /// <summary>
        /// Gera os arquivos base para o ORM
        /// </summary>
        /// <param name="caminho">caminho para gravação dos arquivos</param>
        /// <param name="strNamespace">NameSpace usado no projeto</param>
        public static void GravaArquivosBaseOrm(string caminho, string strNamespace, string nomeProvider)
        {

            var assembly = Assembly.GetExecutingAssembly();
            var nomesResources = assembly.GetManifestResourceNames();
            var rm = new ResourceManager(nomesResources[0].Replace(".resources", string.Empty), assembly);

            GeraArquivosLINQ(caminho, rm, nomeProvider, strNamespace);
            GeraArquivosBLL(caminho, strNamespace, rm);
            GeraArquivosTO(caminho, rm);
            GeraArquivosInterface(caminho, strNamespace, rm);

        }

        private static void GeraArquivosInterface(string caminho, string strNamespace, ResourceManager rm)
        {

            // ReSharper disable ResourceItemNotResolved
            var entity = ((string)(rm.GetObject("IEntity")));
            var StrongNamePermission = ((string)(rm.GetObject("StrongNamePermission")));
            // ReSharper restore ResourceItemNotResolved

            File.WriteAllText(string.Format("{0}\\Interfaces\\IEntity.cs", caminho), entity.Replace("{namespace}", strNamespace));
            File.WriteAllText(string.Format("{0}\\Interfaces\\StrongNamePermission.cs", caminho), StrongNamePermission.Replace("{namespace}", strNamespace));

        }

        private static void GeraArquivosTO(string caminho, ResourceManager rm)
        {

            // ReSharper disable ResourceItemNotResolved
            var domainValidator = ((string)(rm.GetObject("DomainValidator")));
            // ReSharper restore ResourceItemNotResolved

            File.WriteAllText(string.Format("{0}\\TO\\AttributeValidators\\DomainValidator.cs", caminho), domainValidator);

        }

        private static void GeraArquivosBLL(string caminho, string strNamespace, ResourceManager rm)
        {

            // ReSharper disable ResourceItemNotResolved
            var validar = ((string)(rm.GetObject("Validar")));
            // ReSharper restore ResourceItemNotResolved

            File.WriteAllText(string.Format("{0}\\BLL\\Validacao\\Validar.cs", caminho), validar.Replace("TesteBLL.Validacao", string.Format("{0}BLL.Validacao", strNamespace)));

        }

        private static void GeraArquivosLINQ(string caminho, ResourceManager rm, string nomeProvider, string strNamespace)
        {

            // ReSharper disable ResourceItemNotResolved
            var dbExpressions = ((string)(rm.GetObject("DbExpressions")));
            var dbExpressionVisitor = ((string)(rm.GetObject("DbExpressionVisitor")));
            var expressionVisitor = ((string)(rm.GetObject("ExpressionVisitor")));
            var queryExecutor = ((string)(rm.GetObject("QueryExecutor")));
            var queryLanguage = ((string)(rm.GetObject("QueryLanguage")));
            var queryMapping = ((string)(rm.GetObject("QueryMapping")));
            var queryTypeSystem = ((string)(rm.GetObject("QueryTypeSystem")));
            var sqlFormatter = ((string)(rm.GetObject("SqlFormatter")));

            var ISqlFormatter = ((string)(rm.GetObject("ISqlFormatter")));
            var provider = ((string)(rm.GetObject(nomeProvider)));

            var funcoesCrud = ((string)(rm.GetObject("FuncoesCrud")));
            var sqlLanguage = ((string)(rm.GetObject("SqlLanguage")));

            var crudBase = ((string)(rm.GetObject("CrudBase")));
            var singleton = ((string)(rm.GetObject("Singleton")));
            var eTipoConsulta = ((string)(rm.GetObject("ETipoConsulta")));
            var cmdMap = ((string)(rm.GetObject("CmdMap")));
            var transaction = ((string)(rm.GetObject("Transaction")));

            var fastInvoke = ((string)(rm.GetObject("FastInvoke")));
            var mapExtension = ((string)(rm.GetObject("MapExtension")));
            var cachingMannager = ((string)(rm.GetObject("CachingMannager")));
            var eCacheAcao = ((string)(rm.GetObject("ECacheAcao")));
            var cacheChangedEventArgs = ((string)(rm.GetObject("CacheChangedEventArgs")));
            var cacheChangedEventHandler = ((string)(rm.GetObject("CacheChangedEventHandler")));
            var cachingProvider = ((string)(rm.GetObject("ICachingProvider")));


            // ReSharper restore ResourceItemNotResolved

            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\DbExpressions.cs", caminho), dbExpressions);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\DbExpressionVisitor.cs", caminho), dbExpressionVisitor);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\ExpressionVisitor.cs", caminho), expressionVisitor);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryExecutor.cs", caminho), queryExecutor);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryLanguage.cs", caminho), queryLanguage);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryMapping.cs", caminho), queryMapping);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\QueryTypeSystem.cs", caminho), queryTypeSystem);
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\TSqlORM\\SqlFormatter.cs", caminho), sqlFormatter);

            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\FastInvoke.cs", caminho), fastInvoke);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\MapExtension.cs", caminho), mapExtension);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\CachingMannager.cs", caminho), cachingMannager);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Enuns\\ECacheAcao.cs", caminho), eCacheAcao);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Events\\CacheChangedEventArgs.cs", caminho), cacheChangedEventArgs);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Events\\CacheChangedEventHandler.cs", caminho), cacheChangedEventHandler);
            File.WriteAllText(string.Format("{0}\\DAL\\ORM\\Caching\\Interfaces\\ICachingProvider.cs", caminho), cachingProvider);



            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\Factories\\ISqlFormatter.cs", caminho), ISqlFormatter.Replace("TesteDAL", string.Format("{0}DAL", strNamespace)));
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\Factories\\{1}.cs", caminho, nomeProvider), provider.Replace("TesteDAL", string.Format("{0}DAL", strNamespace)));

            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\FuncoesCrud.cs", caminho), funcoesCrud.Replace("TesteDAL", string.Format("{0}DAL", strNamespace)));
            File.WriteAllText(string.Format("{0}\\DAL\\DataDrain\\SqlLanguage.cs", caminho), sqlLanguage.Replace("TesteDAL", string.Format("{0}DAL", strNamespace)));

            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\Base\\CrudBase.cs", caminho), crudBase.Replace("TesteDAL", string.Format("{0}DAL", strNamespace)).Replace("{namespace}", strNamespace));
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\Conexao\\Singleton.cs", caminho), singleton.Replace("TesteDAL", string.Format("{0}DAL", strNamespace)));
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\Enumeradores\\ETipoConsulta.cs", caminho), eTipoConsulta.Replace("TesteDAL", string.Format("{0}DAL", strNamespace)));
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\CommandMap\\CmdMap.cs", caminho), cmdMap);
            File.WriteAllText(string.Format("{0}\\DAL\\Apoio\\CommandMap\\Transaction.cs", caminho), transaction);

        }
    }
}
