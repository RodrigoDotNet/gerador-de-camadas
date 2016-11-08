using System.Deployment.Application;
using System.Drawing;
using System.Reflection;
using System.Resources;

namespace DataDrain.Library.Helpers
{
    public static class AssemblyHelper
    {
        public static Image GetProviderLogo(Assembly assembly)
        {
            var nomesResources = assembly.GetManifestResourceNames();
            var rm = new ResourceManager(nomesResources[0].Replace(".resources", string.Empty), assembly);
            return ((Image)(rm.GetObject("logo")));
        }

        public static string RetornaVersao()
        {
            return string.Format("Versão: {0}", ApplicationDeployment.IsNetworkDeployed
                ? ApplicationDeployment.CurrentDeployment.CurrentVersion :
                Assembly.GetExecutingAssembly().GetName().Version);
        }
    }
}
