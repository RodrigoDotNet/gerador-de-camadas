using System.Collections.Generic;
using System.Linq;
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

        private static List<IInformationSchema> GetInstallProviders()
        {
            throw new System.NotImplementedException();
        }
    }
}
