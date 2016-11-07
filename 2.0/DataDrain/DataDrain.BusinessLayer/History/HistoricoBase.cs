using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.BusinessLayer.History
{
    public abstract class HistoricoBase
    {
        public List<string> RetornaListaNomeServidores(string nomeProvedor)
        {
            var servidores = CarregaConexoes(nomeProvedor);

            return servidores.GroupBy(s => s.ServerAddress).Select(serv => serv.Key).ToList();
        }

        public abstract List<DatabaseUser> CarregaConexoes(string nomeProvedor);
        public abstract void SalvaConexao(DatabaseUser dadosLogin);

        public bool ValidaExistente(IEnumerable<DatabaseUser> servidores, DatabaseUser login)
        {
            var retorno = servidores.FirstOrDefault(s => s.ServerAddress == login.ServerAddress && s.UserName == login.UserName);

            return retorno != null && !string.IsNullOrEmpty(retorno.ServerAddress);
        }

        /// <summary>
        /// Retorna uma lista dos nomes de usuario do servidor alvo
        /// </summary>
        /// <param name="nomeServidor"></param>
        /// <param name="nomeProvedor"></param>
        /// <returns></returns>
        public List<string> RetornaNomeLogins(string nomeServidor, string nomeProvedor)
        {
            var servidores = CarregaConexoes(nomeProvedor);

            return servidores.Where(s => s.ServerAddress == nomeServidor)
                .GroupBy(s => s.UserName).Select(serv => serv.Key).ToList();

        }

        /// <summary>
        /// Retorna uma lista dos nomes de usuario do servidor alvo
        /// </summary>
        /// <param name="nomeServidor"></param>
        /// <param name="nomeProvedor"></param>
        /// <returns></returns>
        public List<string> RetornaListaNomeLogins(string nomeServidor, string nomeProvedor)
        {
            var servidores = CarregaConexoes(nomeProvedor);

            if (!string.IsNullOrWhiteSpace(nomeServidor))
            {
                return servidores.Where(s => s.ServerAddress == nomeServidor).GroupBy(s => s.UserName).Select(serv => serv.Key).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Retorna a senha do usuario
        /// </summary>
        /// <param name="nomeServidor">nome do servidor</param>
        /// <param name="usuario">nome do usuario</param>
        /// <param name="nomeProvedor"></param>
        /// <returns></returns>
        public string RetornaSenhaLogins(string nomeServidor, string usuario, string nomeProvedor)
        {
            var sha = new GeraHashSha1();
            var servidores = CarregaConexoes(nomeProvedor);

            var senha = "";

            foreach (var serv in servidores.Where(s => s.ServerAddress == nomeServidor && s.UserName == usuario))
            {
                senha = sha.Descriptografa(serv.Password);
            }
            return senha;
        }
    }
}
