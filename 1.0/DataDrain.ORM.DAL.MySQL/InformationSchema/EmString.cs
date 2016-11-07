using System.Text.RegularExpressions;

namespace DataDrain.ORM.DAL.MySQL.InformationSchema
{
    internal static class EmString
    {
        public static string RemoveNumeros(this string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return texto;
            }

            return Regex.Replace(texto, "[0-9]", "");
        }
    }
}
