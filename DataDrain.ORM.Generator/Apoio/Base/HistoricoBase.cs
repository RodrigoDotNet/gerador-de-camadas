using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Generator.Apoio.Base
{
    public abstract class HistoricoBase
    {
        /// <summary>
        /// Retorna uma lista de nomes de servidores
        /// </summary>
        /// <returns></returns>
        public AutoCompleteStringCollection RetornaNomeServidores()
        {
            var servidores = CarregaConexoes();
            var dadosLista = new AutoCompleteStringCollection();

            foreach (var serv in servidores.GroupBy(s => s.Servidor))
            {
                dadosLista.Add(serv.Key);
            }
            return dadosLista;
        }

        /// <summary>
        /// Retorna uma lista de nomes de servidores
        /// </summary>
        /// <returns></returns>
        public List<string> RetornaListaNomeServidores()
        {
            var servidores = CarregaConexoes();

            return servidores.GroupBy(s => s.Servidor).Select(serv => serv.Key).ToList();
        }

        public abstract List<DadosUsuario> CarregaConexoes();
        public abstract void SalvaConexao(DadosUsuario dadosLogin);

        public bool ValidaExistente(IEnumerable<DadosUsuario> servidores, DadosUsuario login)
        {
            var retorno = servidores.FirstOrDefault(s => s.Servidor == login.Servidor && s.Usuario == login.Usuario);

            return retorno != null && !string.IsNullOrEmpty(retorno.Servidor);
        }

        /// <summary>
        /// Retorna uma lista dos nomes de usuario do servidor alvo
        /// </summary>
        /// <param name="nomeServidor"></param>
        /// <returns></returns>
        public AutoCompleteStringCollection RetornaNomeLogins(string nomeServidor)
        {
            var servidores = CarregaConexoes();

            var dadosLista = new AutoCompleteStringCollection();

            foreach (var serv in servidores.Where(s => s.Servidor == nomeServidor).GroupBy(s => s.Usuario))
            {
                dadosLista.Add(serv.Key);
            }
            return dadosLista;

        }

        /// <summary>
        /// Retorna uma lista dos nomes de usuario do servidor alvo
        /// </summary>
        /// <param name="nomeServidor"></param>
        /// <returns></returns>
        public List<string> RetornaListaNomeLogins(string nomeServidor)
        {
            var servidores = CarregaConexoes();

            if (!string.IsNullOrWhiteSpace(nomeServidor))
            {
                return servidores.Where(s => s.Servidor == nomeServidor).GroupBy(s => s.Usuario).Select(serv => serv.Key).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Retorna a senha do usuario
        /// </summary>
        /// <param name="nomeServidor">nome do servidor</param>
        /// <param name="usuario">nome do usuario</param>
        /// <returns></returns>
        public string RetornaSenhaLogins(string nomeServidor, string usuario)
        {
            var servidores = CarregaConexoes();

            var senha = "";

            foreach (var serv in servidores.Where(s => s.Servidor == nomeServidor && s.Usuario == usuario))
            {
                senha = serv.Senha.ConvertToString();
            }
            return senha;
        }
    }
}