using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TesteBLL.Validacao
{
    /// <summary>
    /// Classe responsavel pela validação dos DataAnnotations
    /// </summary>
    public class Validar
    {

        /// <summary>
        /// Verifica se o objeto é valido para ser enviado ao banco de dados
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generic">Classe a ser validada</param>
        /// <param name="formatar">indica se deve exibir a quebra de linha após cada erro em html</param>
        /// <returns>boleano de confirmação</returns>
        public static bool Validate<T>(T generic, bool formatar)
        {
            var retorno = from prop in generic.GetType().GetProperties() let err = ValidateProperty(generic, prop.Name) where !String.IsNullOrEmpty(err) select string.Format("{0}: {1}{2}", prop.Name, err, ((formatar) ? "<br>" : ""));

            var erros = retorno as string[] ?? retorno.ToArray();
            if (!erros.Any())
            {
                return true;
            }

            var errMessage = new System.Text.StringBuilder();
            foreach (var erro in erros)
            {
                errMessage.AppendLine(erro);
            }
            throw new ValidationException(errMessage.ToString());
        }

        private static string ValidateProperty<T>(T generic, string propertyName)
        {
            var info = generic.GetType().GetProperty(propertyName);
            if (!info.CanWrite)
            {
                return null;
            }

            var value = info.GetValue(generic, null);
            IEnumerable<string> errorInfos = (from va in info.GetCustomAttributes(true).OfType<ValidationAttribute>() where !va.IsValid(value) select va.FormatErrorMessage(string.Empty)).ToList();

            return errorInfos.Any() ? errorInfos.FirstOrDefault() : null;
        }
    }
}
