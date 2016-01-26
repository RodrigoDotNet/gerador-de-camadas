using System.Linq;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Este validador verifica se o valor é um dos valores especificados em um conjunto. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DomainValidator : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        private readonly object[] _domain;
        public DomainValidator(object[] domain)
        {
            _domain = domain;
        }

        public override bool IsValid(object value)
        {
            var domainArray = _domain;

            if (value != null)
            {
                if (domainArray.Any(value.Equals))
                {
                    return true;
                }
                var campos = _domain.Select(i => i.ToString()).ToList();
                ErrorMessage = string.Format("O valor '{0}' não foi encontrado no dominio '{1}'", value, string.Join(",", campos.ToArray()));
            }
            ErrorMessage = "O valor não pode ser nulo";
            return false;
        }
    }
}
