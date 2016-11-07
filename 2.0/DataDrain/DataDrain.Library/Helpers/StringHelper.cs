using System.Text.RegularExpressions;

namespace DataDrain.Library.Helpers
{
    public static class StringHelper
    {
        public static string RemoveInvalidCharacters(string text)
        {
            var strTextoOk = text;

            if (!string.IsNullOrEmpty(text))
            {
                strTextoOk = Regex.Replace(strTextoOk, "[áàâãª]", "a");
                strTextoOk = Regex.Replace(strTextoOk, "[ÁÀÂÃ]", "A");
                strTextoOk = Regex.Replace(strTextoOk, "[éèê]", "e");
                strTextoOk = Regex.Replace(strTextoOk, "[ÉÈÊ]", "e");
                strTextoOk = Regex.Replace(strTextoOk, "[íìî]", "i");
                strTextoOk = Regex.Replace(strTextoOk, "[ÍÌÎ]", "I");
                strTextoOk = Regex.Replace(strTextoOk, "[óòôõº]", "o");
                strTextoOk = Regex.Replace(strTextoOk, "[ÓÒÔÕ]", "O");
                strTextoOk = Regex.Replace(strTextoOk, "[úùû]", "u");
                strTextoOk = Regex.Replace(strTextoOk, "[ÚÙÛ]", "U");
                strTextoOk = Regex.Replace(strTextoOk, "[ç]", "c");
                strTextoOk = Regex.Replace(strTextoOk, "[Ç]", "C");
                strTextoOk = Regex.Replace(strTextoOk, @"^[\d-]*\s*", "");
            }
            return strTextoOk;
        }
    }
}
