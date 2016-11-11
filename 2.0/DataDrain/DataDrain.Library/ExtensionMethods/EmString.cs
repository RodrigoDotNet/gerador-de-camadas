using System;
using System.Collections.Generic;
using System.Globalization;

namespace DataDrain.Library.ExtensionMethods
{
    public static class EmString
    {
        public static int ToInt32(this string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                int saida;
                int.TryParse(input, out saida);
                return saida;
            }
            return 0;
        }

        public static T ConvertToEnum<T>(this string value)
        {
            if (typeof(T).BaseType != typeof(Enum))
            {
                throw new InvalidCastException();
            }
            if (Enum.IsDefined(typeof(T), value) == false)
            {
                throw new InvalidCastException();
            }
            return (T)Enum.Parse(typeof(T), value);
        }

        public static string PrimeiraLetraMaiuscula(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            var letters = source.ToCharArray();

            letters[0] = char.ToUpper(letters[0]);

            return new string(letters);
        }

        public static Type RetornaTipo(this string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                int saida;
                DateTime saidaD;

                if (int.TryParse(input, out saida))
                {
                    return typeof(int);
                }
                if (DateTime.TryParse(input, out saidaD))
                {
                    return typeof(DateTime);
                }
            }

            return typeof(string);
        }

        public static IEnumerable<int> AllIndexesOf(this string str, string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    break;
                yield return index;
            }
        }

        public static bool ContainsInsensitive(this string texto, string palavra)
        {
            if (string.IsNullOrWhiteSpace(texto) || string.IsNullOrWhiteSpace(palavra))
            {
                return false;
            }

            var culture = new CultureInfo("pt-BR");
            return culture.CompareInfo.IndexOf(texto, palavra, CompareOptions.IgnoreCase) > -1;
        }

        public static bool ContainsCaseInsensitive(this string texto, string textoProcurado)
        {
            if (string.IsNullOrWhiteSpace(texto) || string.IsNullOrWhiteSpace(textoProcurado))
            {
                return false;
            }

            return new CultureInfo("pt-BR").CompareInfo.IndexOf(texto, textoProcurado.Trim(), CompareOptions.IgnoreCase) >= 0;
        }
    }
}
