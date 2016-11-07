using System.Deployment.Application;
using System.Reflection;

namespace DataDrain.ORM.Generator.Apoio
{
    internal sealed class Versao
    {
        public static string RetornaVersao()
        {
            return string.Format("Versão: {0}", ApplicationDeployment.IsNetworkDeployed
                ? ApplicationDeployment.CurrentDeployment.CurrentVersion :
                Assembly.GetExecutingAssembly().GetName().Version);
        }
    }
}
