using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DataDrain.ORM.Generator.Apoio
{
    public class RegExpression
    {
        /// <summary>
        /// REtorna uma lista com as Regular Expression padrão
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="Exception"></exception>
        public static List<DadosRegExpression> RetornaRegularExpressions()
        {
            try
            {
                var pathXml = string.Format("{0}\\xmlRegExp.xml", AppDomain.CurrentDomain.BaseDirectory);
                var expressions = new List<DadosRegExpression> {new DadosRegExpression {Nome = "Sem Regular Expression", Expression = "sem"}};
                if (RegularExpressionStatus())
                {
                    var xdoc = XDocument.Load(pathXml);

                    var conexoes = from conexao in xdoc.Descendants("tipos")
                                   select new
                                              {
                                                  Children = conexao.Descendants("tipo")
                                              };

                    expressions.AddRange(from conexao in conexoes
                                         from cnn in conexao.Children
                                         where cnn != null
                                         select new DadosRegExpression
                                                    {
                                                        Nome = cnn.Element("nome").Value,
                                                        Expression = cnn.Element("expression").Value
                                                    });

                    expressions.Add(new DadosRegExpression {Nome = "Criar Reg. Expression", Expression = "manual"});
                }
                return expressions;
            }
            catch (System.IO.IOException ex)
            {
                throw new Exception(ex.Message,ex);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Verifica se esta ativado o uso de Regular Expression na geração de objetos
        /// </summary>
        /// <returns></returns>
        public static bool RegularExpressionStatus()
        {
            var pathXml = string.Format("{0}\\xmlRegExp.xml", AppDomain.CurrentDomain.BaseDirectory);
            return System.IO.File.Exists(pathXml);
        }

    }
}
