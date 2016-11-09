
namespace DataDrain.Library.RegExp
{
    public sealed class DadosRegExpression
    {
        public DadosRegExpression(string nome,string regularExpression)
        {
            Nome = nome;
            Expression = regularExpression;
        }

        public string Nome { get; private set; }

        public string Expression { get; private set; }
    }
}
