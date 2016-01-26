using System.IO;
using System.Linq;
using System.Reflection;

namespace DataDrain.ORM.Interfaces
{
    public static class EmIInformationSchema
    {
        /// <summary>
        /// Retorna o FullName do Dot NET Provider usado pelo Map
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string DotNetDataProviderFullName(this IInformationSchema p)
        {
            var asbl = p.GetType().Assembly.GetReferencedAssemblies().FirstOrDefault(a => a.FullName.ToLower().Contains(RetornaNomemap(p)));

            return asbl != null ? asbl.FullName : string.Empty;
        }

        private static string RetornaNomemap(IInformationSchema p)
        {
            var nomeMap = p.GetType().FullName.Split('.')[3].ToLower();

            switch (nomeMap)
            {
                case "postgres":
                    return "npgsql";

                default:
                    return nomeMap;
            }
        }

        /// <summary>
        /// Retorna o Path do Dot NET Provider usado pelo Map
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string DotNetDataProviderPath(this IInformationSchema p)
        {
            var asbl = p.GetType().Assembly.GetReferencedAssemblies().FirstOrDefault(a => a.FullName.ToLower().Contains(RetornaNomemap(p)));

            return asbl != null ? Assembly.ReflectionOnlyLoad(asbl.FullName).Location : string.Empty;
        }

        public static bool CopiarProvider(this IInformationSchema p, string caminho)
        {
            if (string.IsNullOrWhiteSpace(caminho)) return false;

            if (Directory.Exists(caminho))
            {
                var diretorio = string.Format("{0}\\Provider", caminho);
                var arquivo = p.DotNetDataProviderPath();

                if (!string.IsNullOrWhiteSpace(arquivo))
                {
                    Directory.CreateDirectory(diretorio);

                    File.Copy(arquivo, string.Format("{0}\\{1}", diretorio, Path.GetFileName(arquivo)));

                    return true;
                }
            }
            return false;
        }
    }
}
