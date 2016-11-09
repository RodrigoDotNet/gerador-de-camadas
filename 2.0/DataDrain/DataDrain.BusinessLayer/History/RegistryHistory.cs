using System;
using System.Collections.Generic;
using System.Linq;
using DataDrain.Library.Cryptography;
using DataDrain.Library.ExtensionMethods;
using DataDrain.Library.Registry;
using DataDrain.Rules.SuportObjects;
using DataDrain.Library.Helpers;

namespace DataDrain.BusinessLayer.History
{
    public sealed class RegistryHistory : BaseHistory
    {
        private readonly HashSha1 _sh1;

        public RegistryHistory()
        {
            _sh1 = new HashSha1();
        }

        public override List<DatabaseUser> CarregaConexoes(string nomeProvedor)
        {
            var conexoes = RegistroWindows.RetornaTodasSubChaves(RegistroWindows.ChaveConexoes);

            return (from conexao in conexoes
                    let parametros = RegistroWindows.RetornaTodosValoresChave(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, conexao))
                    select new DatabaseUser
                    {
                        UserId = conexao,
                        UserName = parametros.FirstOrDefault(p => p.Key == "Usuario").Value,
                        Password = _sh1.Descriptografa(parametros.FirstOrDefault(p => p.Key == "Senha").Value),
                        ServerAddress = parametros.FirstOrDefault(p => p.Key == "Servidor").Value,
                        DatabaseName = parametros.FirstOrDefault(p => p.Key == "DataBase").Value,
                        Port = parametros.FirstOrDefault(p => p.Key == "Porta").Value.ToInt32(),
                        IsTrustedConnection = parametros.FirstOrDefault(p => p.Key == "TrustedConnection").Value == "true",
                        NomeProvedor = parametros.FirstOrDefault(p => p.Key == "NomeProvedor").Value
                    }).Where(p => p.NomeProvedor == nomeProvedor).ToList();
        }


        public override void SalvaConexao(DatabaseUser dadosLogin)
        {
            if (dadosLogin == null || string.IsNullOrWhiteSpace(dadosLogin.UserId))
            {
                throw new ArgumentNullException("dadosLogin", "ID do canexão não pode ser nulo ou vazio");
            }


            var conexoes = CarregaConexoes(dadosLogin.NomeProvedor);

            var conexao = conexoes.FirstOrDefault(c =>
                            c.UserName == dadosLogin.UserName &&
                            c.ServerAddress == dadosLogin.ServerAddress &&
                            c.DatabaseName == dadosLogin.DatabaseName &&
                            c.Port == dadosLogin.Port);

            if (conexao != null)
            {
                dadosLogin.UserId = conexao.UserId;
            }

            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.UserId), "Usuario", dadosLogin.UserName.Trim());
            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.UserId), "Senha", _sh1.Criptografa(dadosLogin.Password));
            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.UserId), "Servidor", dadosLogin.ServerAddress.Trim());
            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.UserId), "DataBase", "");
            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.UserId), "Porta", dadosLogin.Port.ToString());
            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.UserId), "TrustedConnection", dadosLogin.IsTrustedConnection ? "true" : "false");
            RegistroWindows.GravaValor(string.Format("{0}\\{1}", RegistroWindows.ChaveConexoes, dadosLogin.UserId), "NomeProvedor", dadosLogin.NomeProvedor);
        }
    }
}