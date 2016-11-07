using System.IO;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.IO
{
    public class OrmObjectFiles
    {
        public static bool Log { get; set; }

        /// <summary>
        /// Gera os arquivos ORM (TO/DAL/BLL)
        /// </summary>
        public static void Create(Configuration parammeter, ITemplateText dictionaryTemplates)
        {
            foreach (var objetoOrm in parammeter.ObjetosMapeaveis)
            {
                GeraClassesOrm(objetoOrm, parammeter, dictionaryTemplates);
            }
        }

        private static void GeraClassesOrm(DatabaseObjectInfo objectDatabase, Configuration configuration, ITemplateText dictionaryTemplates)
        {
            var gerador = new FileFactory(dictionaryTemplates) { Log = Log };
            var nomeArquivo = FileFactory.RetornaNomeClasseAjustado(objectDatabase.Name);

            var arquivo = gerador.GeneratePocoFiles(nomeArquivo,objectDatabase, configuration);
            var arquivoBLL = gerador.GenerateBusinessFiles(nomeArquivo, objectDatabase, configuration);
            var arquivoDAL = gerador.GenerateDataAccesFiles(nomeArquivo, objectDatabase, configuration);

            File.WriteAllText(string.Format("{0}\\TO\\{1}TO.cs", configuration.DestinationPath, nomeArquivo), arquivo);
            File.WriteAllText(string.Format("{0}\\BLL\\{1}BLL.cs", configuration.DestinationPath, nomeArquivo), arquivoBLL);
            File.WriteAllText(string.Format("{0}\\DAL\\{1}DAL.cs", configuration.DestinationPath, nomeArquivo), arquivoDAL);
        }
    }
}
