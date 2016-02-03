using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;

namespace DataDrain.ORM.Generator
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

        public static Type RetornaTipo(this string input)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                int saida;
                DateTime saidaD;

                if (Int32.TryParse(input, out saida))
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

        /// <summary>
        /// Convert a string para Enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">Não foi possivel converter a string para o enum</exception>
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

        /// <summary>
        /// Converte uma string para SecureString 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static SecureString ToSecureString(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                input = string.Empty;
            }
            var secureString = new SecureString();
            input.ToCharArray().ToList().ForEach(secureString.AppendChar);

            return secureString;
        }

        public static IEnumerable<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    break;
                yield return index;
            }
        }

        public static object ConverteParaTipo(this object valor)
        {
            if (valor == null || valor.ToString() == "null")
            {
                return null;
            }

            DateTime data;
            int numerico;
            long numericoGrande;
            ulong numericoMuitoGrande;
            double numericoDecimalGrande;
            TimeSpan hora;

            if (DateTime.TryParse(valor.ToString(), out data)) { return data; }

            if (int.TryParse(valor.ToString(), out numerico)) { return numerico; }

            if (long.TryParse(valor.ToString(), out numericoGrande)) { return numericoGrande; }

            if (ulong.TryParse(valor.ToString(), out numericoMuitoGrande)) { return numericoMuitoGrande; }

            if (double.TryParse(valor.ToString(), out numericoDecimalGrande)) { return numericoDecimalGrande; }

            if (TimeSpan.TryParse(valor.ToString(), out hora)) { return hora; }

            return valor;
        }

        public static string RetornaTipo(this object valor)
        {
            if (valor == null || valor.ToString() == "null")
            {
                return "object";
            }

            DateTime data;
            int numerico;
            long numericoGrande;
            ulong numericoMuitoGrande;
            double numericoDecimalGrande;
            TimeSpan hora;

            if (DateTime.TryParse(valor.ToString(), out data)) { return "DateTime"; }

            if (int.TryParse(valor.ToString(), out numerico)) { return "int"; }

            if (long.TryParse(valor.ToString(), out numericoGrande)) { return "long"; }

            if (ulong.TryParse(valor.ToString(), out numericoMuitoGrande)) { return "ulong"; }

            if (double.TryParse(valor.ToString(), out numericoDecimalGrande)) { return "double"; }

            if (TimeSpan.TryParse(valor.ToString(), out hora)) { return "DateTime"; }

            return "string";
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

    }
}
