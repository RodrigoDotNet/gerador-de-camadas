using System.IO;
using DataDrain.ORM.DAL.Templates;

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
            GeraArquivosLINQ(caminho, nomeProvider, strNamespace);
            GeraArquivosBLL(caminho, strNamespace);
            GeraArquivosTO(caminho);
            GeraArquivosInterface(caminho, strNamespace);

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

            File.WriteAllText(string.Format("{0}\\BLL\\Validacao\\Validar.cs", caminho), validar.Value.Replace("TesteBLL.Validacao", string.Format("{0}BLL.Validacao", strNamespace)));
        }

        private static void GeraArquivosLINQ(string caminho, string nomeProvider, string strNamespace)
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

            var funcoesCrud = Template.RetornaValor("FuncoesCrud").Value;
            var sqlLanguage = Template.RetornaValor("SqlLanguage").Value;

            var crudBase = Template.RetornaValor("CrudBase").Value;
            var singleton = Template.RetornaValor("Singleton").Value;
            var eTipoConsulta = Template.RetornaValor("ETipoConsulta").Value;
            var cmdMap = Template.RetornaValor("CmdMap").Value;
            var transaction = Template.RetornaValor("Transaction").Value;

            var fastInvoke = Template.RetornaValor("FastInvoke").Value;
            var mapExtension = Template.RetornaValor("MapExtension").Value;
            var cachingMannager = Template.RetornaValor("CachingMannager").Value;
            var eCacheAcao = Template.RetornaValor("ECacheAcao").Value;
            var cacheChangedEventArgs = Template.RetornaValor("CacheChangedEventArgs").Value;
            var cacheChangedEventHandler = Template.RetornaValor("CacheChangedEventHandler").Value;
            var cachingProvider = Template.RetornaValor("ICachingProvider").Value;


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
