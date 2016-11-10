using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DataDrain.Library.Helpers;
using DataDrain.Rules.Interfaces;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.BusinessLayer
{
    public sealed class InformationSchemaBL
    {

        public static List<ProviderInfo> GetAllProviders()
        {
            var providers = GetInstallProviders();

            return (from provider in providers
                    let assembly = provider.GetType().Assembly
                    select new ProviderInfo
                    {
                        MinimalDatabaseVersion = string.Format("Versão minima Banco: {0}", provider.InfoConnection.MinimalVersion),
                        ProviderVersion = string.Format("Versão Provider: {0}", assembly.GetName().Version),
                        TableMapping = string.Format("Mapeamento de Tabelas: {0}", (provider.IsTableMapping ? "Sim" : "Não")),
                        ViewMapping = string.Format("Mapeamento de Views: {0}", (provider.IsViewMapping ? "Sim" : "Não")),
                        StoredProcedureMapping = string.Format("Mapeamento de Procedure: {0}", (provider.IsStoredProcedureMapping ? "Sim" : "Não")),
                        ImageLogo = AssemblyHelper.GetProviderLogo(assembly),
                        Provider = provider
                    }).ToList();
        }

        private static IEnumerable<IInformationSchema> GetInstallProviders()
        {
            return new List<IInformationSchema>
            {
                new Databases.MySql.Map(),
                new Databases.SqlServer.Map()
            };
        }

        private static void CarregaAssembliesDeReferencia(Assembly assemblySandBox, List<string> currentAssemblyReferences)
        {
            var referencedAssemblies = assemblySandBox.GetReferencedAssemblies();

            var newAssemblies = referencedAssemblies.ToList();

            foreach (var assembly in newAssemblies)
            {
                CarregaAssembliesDeReferencia(Assembly.ReflectionOnlyLoadFrom(RetornaCaminhoAssembly(assembly)), currentAssemblyReferences);
                Assembly.Load(assembly.FullName);
            }
        }

        private static string RetornaCaminhoAssembly(AssemblyName assembly)
        {
            if (string.IsNullOrWhiteSpace(assembly.CodeBase))
            {

                var caminhoArquivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("{0}.dll", assembly.FullName.Split(',')[0]));

                if (File.Exists(caminhoArquivo))
                {
                    return caminhoArquivo;
                }
                return Assembly.ReflectionOnlyLoad(assembly.FullName).Location;
            }

            var codeBase = assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static bool IsManagedAssembly(string fileName)
        {
            var dataDictionaryRva = new uint[16];
            var dataDictionarySize = new uint[16];

            using (Stream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fs))
                {
                    //PE Header starts @ 0x3C (60). Its a 4 byte header.
                    fs.Position = 0x3C;
                    var peHeader = reader.ReadUInt32();

                    //Moving to PE Header start location...
                    fs.Position = peHeader;
                    reader.ReadUInt32();

                    //We can also show all these value, but we will be       
                    //limiting to the CLI header test.
                    reader.ReadUInt16();
                    reader.ReadUInt16();
                    reader.ReadUInt32();
                    reader.ReadUInt32();
                    reader.ReadUInt32();
                    reader.ReadUInt16();
                    reader.ReadUInt16();

                    // Now we are at the end of the PE Header and from here, the PE Optional Headers starts... To go directly to the datadictionary, we'll increase the stream’s current position to with 96 (0x60). 96 because, 28 for Standard fields 68 for NT-specific fields From here DataDictionary starts...and its of total 128 bytes. DataDictionay has 16 directories in total, doing simple maths 128/16 = 8. So each directory is of 8 bytes. In this 8 bytes, 4 bytes is of RVA and 4 bytes of Size. btw, the 15th directory consist of CLR header! if its 0, its not a CLR file :)
                    var dataDictionaryStart = Convert.ToUInt16(Convert.ToUInt16(fs.Position) + 0x60);
                    fs.Position = dataDictionaryStart;
                    for (var i = 0; i < 15; i++)
                    {
                        dataDictionaryRva[i] = reader.ReadUInt32();
                        dataDictionarySize[i] = reader.ReadUInt32();
                    }
                }
            }

            return dataDictionaryRva[14] != 0;
        }
    }
}
