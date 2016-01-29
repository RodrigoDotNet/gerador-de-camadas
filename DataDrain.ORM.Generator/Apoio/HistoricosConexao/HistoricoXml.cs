using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using DataDrain.ORM.Generator.Apoio.Base;
using DataDrain.ORM.Interfaces;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Generator.Apoio.HistoricosConexao
{

    internal class HistoricoXml : HistoricoBase
    {
        public override List<DadosUsuario> CarregaConexoes(string nomeProvedor)
        {
            try
            {
                var sh1 = new GeraHashSha1();
                var pathXml = string.Format("{0}\\conexoes.xml", AppDomain.CurrentDomain.BaseDirectory);
                var servidores = new List<DadosUsuario>();

                if (File.Exists(pathXml))
                {
                    if (EmSerialize.VerificaXml(pathXml))
                    {
                        var xdoc = XDocument.Load(pathXml);

                        var conexoes = from conexao in xdoc.Descendants("conexoes")
                                       select new
                                                  {
                                                      Children = conexao.Descendants("conexao")
                                                  };

                        servidores.AddRange(from conexao in conexoes
                                            from cnn in conexao.Children
                                            where cnn != null && cnn.Element("maquina").Value == frmGerador.IdComputador
                                            select new DadosUsuario
                                                       {
                                                           Servidor = cnn.Element("servidor").Value,
                                                           DataBase = cnn.Element("database").Value,
                                                           Usuario = cnn.Element("usuario").Value,
                                                           Senha = sh1.Descriptografa(cnn.Element("senha").Value.Trim()),
                                                           Porta = cnn.Element("porta").ToString().ToInt32(),
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

        public override void SalvaConexao(DadosUsuario dadosLogin)
        {
            var sh1 = new GeraHashSha1();
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
                                      where b.Element("servidor").Value == dadosLogin.Servidor
                                            && b.Element("usuario").Value == dadosLogin.Usuario
                                      select b);

                    foreach (var xe in singleBook)
                    {
                        xe.SetElementValue("senha", sh1.Criptografa(dadosLogin.Senha));
                        xe.SetElementValue("data", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Local));
                        //use the ReplaceContent method to do the replacement for all attribures
                        //this will remove all other attributes and save only the price attribute
                        //xe.ReplaceAttributes(new XAttribute("price", "32$"));
                    }

                    var xmlSemAssinatura = Regex.Replace(doc.ToString(), @"<Signature(.|\n)*?Signature>", string.Empty);

                    EmSerialize.AssinaXml(xmlSemAssinatura, pathXml);
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

                    maquinaId.AppendChild(xmlDoc.CreateTextNode(dadosLogin.MaquinaID));
                    servidor.AppendChild(xmlDoc.CreateTextNode(dadosLogin.Servidor));
                    database.AppendChild(xmlDoc.CreateTextNode(dadosLogin.DataBase));
                    usuario.AppendChild(xmlDoc.CreateTextNode(dadosLogin.Usuario));
                    senha.AppendChild(xmlDoc.CreateTextNode(sh1.Criptografa(dadosLogin.Senha)));
                    porta.AppendChild(xmlDoc.CreateTextNode(dadosLogin.Porta.ToString()));
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
                    EmSerialize.AssinaXml(xmlSemAssinatura, pathXml);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
