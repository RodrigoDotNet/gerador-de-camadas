using System;
using System.Collections.Generic;
using System.Linq;
using DataDrain.ORM.Generator.Apoio.Base;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Generator.Apoio.HistoricosConexao
{
    internal class HistoricoRegistro : HistoricoBase
    {

        private readonly GeraHashSha1 _sh1;

        public HistoricoRegistro()
        {
            _sh1=new GeraHashSha1();
        }

        public override List<DadosUsuario> CarregaConexoes()
        {
            var conexoes = RegistroWindows.RetornaTodasSubChaves(RegistroWindows.ChaveConexoes);

            return (from conexao in conexoes
                    let parametros = RegistroWindows.RetornaTodosValoresChave(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, conexao))
                    select new DadosUsuario
                        {
                            ID = conexao,
                            Usuario = parametros.FirstOrDefault(p => p.Key == "Usuario").Value,
                            Senha = _sh1.Descriptografa(parametros.FirstOrDefault(p => p.Key == "Senha").Value).ToSecureString(),
                            Servidor = parametros.FirstOrDefault(p => p.Key == "Servidor").Value,
                            DataBase = parametros.FirstOrDefault(p => p.Key == "DataBase").Value,
                            Porta = parametros.FirstOrDefault(p => p.Key == "Porta").Value.ToInt32(),
                            TrustedConnection = parametros.FirstOrDefault(p => p.Key == "TrustedConnection").Value == "true"
                        }).ToList();
        }


        public override void SalvaConexao(DadosUsuario dadosLogin)
        {
            if (dadosLogin==null || string.IsNullOrWhiteSpace(dadosLogin.ID))
            {
                throw new ArgumentNullException("dadosLogin", "ID do canexão não pode ser nulo ou vazio");
            }

            
            var conexoes = CarregaConexoes();

            var conexao = conexoes.FirstOrDefault(c =>
                            c.Usuario == dadosLogin.Usuario &&
                            c.Senha.SecureStringEqual(dadosLogin.Senha) &&
                            c.Servidor == dadosLogin.Servidor &&
                            c.DataBase == dadosLogin.DataBase &&
                            c.Porta == dadosLogin.Porta);

            if (conexao != null)
            {
                dadosLogin.ID = conexao.ID;
            }

            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.ID), "Usuario", dadosLogin.Usuario.Trim());
            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.ID), "Senha", _sh1.Criptografa(dadosLogin.Senha.ConvertToString()));
            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.ID), "Servidor", dadosLogin.Servidor.Trim());
            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.ID), "DataBase", "");
            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.ID), "Porta", dadosLogin.Porta.ToString());
            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.ID), "TrustedConnection", dadosLogin.TrustedConnection ? "true" : "false");
        }
    }
}