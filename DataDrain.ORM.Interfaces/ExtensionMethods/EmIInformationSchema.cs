using System.Linq;

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
    }
}
