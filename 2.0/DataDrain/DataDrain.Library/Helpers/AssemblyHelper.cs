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
    }
}
