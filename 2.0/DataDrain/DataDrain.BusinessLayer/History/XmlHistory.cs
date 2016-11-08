using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using DataDrain.Library.Cryptography;
using DataDrain.Library.Helpers;
using DataDrain.Rules.SuportObjects;

namespace DataDrain.BusinessLayer.History
{

    public sealed class XmlHistory : BaseHistory
    {
        public override List<DatabaseUser> CarregaConexoes(string nomeProvedor)
        {
            try
            {
                var sh1 = new HashSha1();
                var pathXml = string.Format("{0}\\conexoes.xml", AppDomain.CurrentDomain.BaseDirectory);
                var servidores = new List<DatabaseUser>();

                if (File.Exists(pathXml))
                {
                    if (SerializerHelper.VerificaXml(pathXml))
                    {
                        var xdoc = XDocument.Load(pathXml);

                        var conexoes = from conexao in xdoc.Descendants("conexoes")
                                       select new
                                                  {
                                                      Children = conexao.Descendants("conexao")
                                                  };

                        servidores.AddRange(from conexao in conexoes
                                            from cnn in conexao.Children
                                            where cnn != null 
                                            select new DatabaseUser
                                                       {
                                                           ServerAddress = cnn.Element("servidor").Value,
                                                           DatabaseName = cnn.Element("database").Value,
                                                           UserName = cnn.Element("usuario").Value,
                                                           Password = sh1.Descriptografa(cnn.Element("senha").Value.Trim()),
                                                           Port = cnn.Element("porta").ToString().ToInt32(),
                                                           NomeProvedor = cnn.Element("nomeProvedor").Value
                                                       });
                    }
                }
                return servidores.Where(p => p.NomeProvedor == nomeProvedor).ToList();
            }
            catch
            {
                throw;
            }
        }

        public override void SalvaConexao(DatabaseUser dadosLogin)
        {
            var sh1 = new HashSha1();
            var pathXml = string.Format("{0}\\conexoes.xml", AppDomain.CurrentDomain.BaseDirectory);
            var existe = System.IO.File.Exists(pathXml);
            var xmlDoc = new XmlDocument();

            if (dadosLogin==null)
            {
                return;
            }

            try
            {
                if (ValidaExistente(CarregaConexoes(dadosLogin.NomeProvedor), dadosLogin))
                {
                    var doc = XElement.Load(pathXml);

                    var singleBook = (from b in doc.Elements("conexao")
                                      where b.Element("servidor").Value == dadosLogin.ServerAddress
                                            && b.Element("usuario").Value == dadosLogin.UserName
                                      select b);

                    foreach (var xe in singleBook)
                    {
                        xe.SetElementValue("senha", sh1.Criptografa(dadosLogin.Password));
                        xe.SetElementValue("data", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Local));
                    }

                    var xmlSemAssinatura = Regex.Replace(doc.ToString(), @"<Signature(.|\n)*?Signature>", string.Empty);

                    SerializerHelper.AssinaXml(xmlSemAssinatura, pathXml);
                }
                else
                {
                    if (existe)
                    {
                        xmlDoc.Load(pathXml);
                    }
                    else
                    {
                        xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "ISO-8859-1", "no"));
                    }

                    var maquinaId = xmlDoc.CreateElement("maquina");
                    var servidor = xmlDoc.CreateElement("servidor");
                    var database = xmlDoc.CreateElement("database");
                    var usuario = xmlDoc.CreateElement("usuario");
                    var porta = xmlDoc.CreateElement("porta");
                    var senha = xmlDoc.CreateElement("senha");
                    var nomeProvedor = xmlDoc.CreateElement("nomeProvedor");

                    maquinaId.AppendChild(xmlDoc.CreateTextNode(dadosLogin.MachineId));
                    servidor.AppendChild(xmlDoc.CreateTextNode(dadosLogin.ServerAddress));
                    database.AppendChild(xmlDoc.CreateTextNode(dadosLogin.DatabaseName));
                    usuario.AppendChild(xmlDoc.CreateTextNode(dadosLogin.UserName));
                    senha.AppendChild(xmlDoc.CreateTextNode(sh1.Criptografa(dadosLogin.Password)));
                    porta.AppendChild(xmlDoc.CreateTextNode(dadosLogin.Port.ToString()));
                    nomeProvedor.AppendChild(xmlDoc.CreateTextNode(dadosLogin.NomeProvedor));

                    var dataConexao = xmlDoc.CreateElement("data");
                    dataConexao.AppendChild(xmlDoc.CreateTextNode(XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Local)));

                    var conexao = xmlDoc.CreateElement("conexao");
                    conexao.AppendChild(maquinaId);
                    conexao.AppendChild(servidor);
                    conexao.AppendChild(database);
                    conexao.AppendChild(usuario);
                    conexao.AppendChild(senha);
                    conexao.AppendChild(porta);
                    conexao.AppendChild(nomeProvedor);
                    conexao.AppendChild(dataConexao);

                    if (existe)
                    {
                        var conexoes = xmlDoc.SelectSingleNode("conexoes");
                        if (conexoes != null)
                        {
                            conexoes.AppendChild(conexao);
                            xmlDoc.AppendChild(conexoes);
                        }
                    }
                    else
                    {
                        var acoes = xmlDoc.CreateElement("conexoes");
                        acoes.AppendChild(conexao);
                        xmlDoc.AppendChild(acoes);
                    }
                    var xmlSemAssinatura = Regex.Replace(xmlDoc.OuterXml, @"<Signature(.|\n)*?Signature>", string.Empty);
                    SerializerHelper.AssinaXml(xmlSemAssinatura, pathXml);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
