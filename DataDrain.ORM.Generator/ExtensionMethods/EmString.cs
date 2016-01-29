using System;
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
    }
}
