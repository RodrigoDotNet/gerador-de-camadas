using System;
using System.Linq;
using System.Reflection;

namespace {namespace}Interfaces
{
    /// <summary>
    /// Classe responsavel pela permissão de uso do sistema
    /// </summary>
    public class StrongNamePermission
    {
        /// <summary>
        /// Valida se o Assembly que esta usando a classe é assinado como o mesmo StrongName
        /// </summary>
        /// <param name="callerAssembly">Assembly que esta chamando o projeto</param>
        /// <param name="currentAssembly">Assembly do projeto</param>
        protected StrongNamePermission(Assembly callerAssembly, Assembly currentAssembly)
        {
            if (!currentAssembly.GetName().GetPublicKey().SequenceEqual(callerAssembly.GetName().GetPublicKey()))
            {
                throw new Exception(string.Format("O Assembly '{0}' não possui permissão de uso.", callerAssembly.GetName().FullName));
            }
        }
    }
}
