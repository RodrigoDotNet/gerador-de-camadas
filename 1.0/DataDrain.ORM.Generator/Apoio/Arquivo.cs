using System.Linq;

namespace DataDrain.ORM.Generator.Apoio
{
    public class Arquivo
    {
        public static string RetornaNomevalidoArquivo(string nome)
        {
            var novoNome = new string(nome.ToCharArray().Where(c => !System.IO.Path.GetInvalidPathChars().ToList().Contains(c)).ToArray()).Replace("\\", "");

            return novoNome;
        }
    }
}
