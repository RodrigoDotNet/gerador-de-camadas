using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace DataDrain.Library.Helpers
{
    public static class SerializerHelper
    {
        private const string PublicPrivateKey = "<RSAKeyValue><Modulus>0V+SKkEZHR2epioblaBNYgZKTk7nDHuy1GOAYlSnxJqceXtW2y5iAbSlY/ud7Nw4Q+WhqyGXQt4yfJaBUv+ARdWZwZw9Y/dC9PQnXH70cADfWwE72PcUBv1jSN4EkY0tlzvTGR0i20o6yUoM8JVtzoeaPT+rZhNZ7zdll/2x4n0=</Modulus><Exponent>AQAB</Exponent><P>9E4tb3J6AOoCkU690em0l82ZHY/OBeUlq/Gbu3WbwPT2SObMAO8YtEHQAvBFMIvWzxmx9+RYN0MUFX2L8Mm0xQ==</P><Q>22VSNWL7A5Pr1VcQfftlugweStYDGiwgoLgr2tckQRHSlEqa4/mXV+tq4emM75WuDDT3eh4ExDvSwVFMiw6CWQ==</Q><DP>8PJZpV0GLvic56r9U5ZnYoCHzrwRuYayjM6RhWUM5FW0wVm/QfyOJNnzybImyyZgCYmSGKbAymAB4uAbsLN+8Q==</DP><DQ>FYCFoRPVVBFwQJVq2V4FJ0m+wkjdPvhqLjY+nhENzY7Im54ANi2lBondDM0N8gEycKHUS1Sb1Puj+SxVmx9N6Q==</DQ><InverseQ>niNqRYKOBCMza8TGzxBXovBv4Y1NI2UCZlMxyG3fGvsnXHaqMzwEvyewHmA5odCs/WjYTg6mKEihC7wkSNv2YQ==</InverseQ><D>P3hEW/DOByiCfobHQ+2LZ8rWXbTHj94z0PS055oYHZ5tDtqX8uu1pCS5+nw9XJ9JozOKddFCBHBNR71sSNfRb02GhrrUU/heR+U/v/V+JZBgGj7rqwa/+9/W7FzsWWZghjDjvZ4T7CXsgB0niPUwAilr48t1kZ8ctAZrzHQ1oyE=</D></RSAKeyValue>";

        /// <summary>
        /// Assina o XML de origem subistituindo o antigo
        /// </summary>
        /// <param name="pathXml">Caminho do xml de origem</param>
        /// <returns></returns>
        public static bool AssinaXml(string pathXml)
        {
            try
            {
                if (string.IsNullOrEmpty(pathXml))
                {
                    return false;
                }

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(pathXml);

                var xml = new StringWriter(new StringBuilder(xmlDoc.OuterXml));

                var rsaKey = RetornaAssinaturaRsa(xml);

                AssinaXml(xmlDoc, rsaKey);
                xmlDoc.Save(pathXml);
                xml.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Assina e salva o fonte de xml
        /// </summary>
        /// <param name="xmlSource"></param>
        /// <param name="pathXmlDestino"></param>
        /// <returns></returns>
        public static KeyValuePair<bool, string> AssinaXml(string xmlSource, string pathXmlDestino)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlSource))
                {
                    throw new ArgumentNullException("xmlSource", "Parametro não pode ser nulo");
                }

                if (string.IsNullOrEmpty(pathXmlDestino))
                {
                    throw new ArgumentNullException("pathXmlDestino", "Parametro não pode ser nulo");
                }

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlSource);

                using (var xml = new StringWriter(new StringBuilder(xmlDoc.OuterXml)))
                {
                    var rsaKey = RetornaAssinaturaRsa(xml);

                    AssinaXml(xmlDoc, rsaKey);
                    xmlDoc.Save(pathXmlDestino);
                    xml.Dispose();
                }

                return new KeyValuePair<bool, string>(true, "");
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }

        /// <summary>
        /// Verifica se o XML assinado foi alterado
        /// </summary>
        /// <param name="pathXml"></param>
        /// <returns></returns>
        public static bool VerificaXml(string pathXml)
        {
            if (string.IsNullOrWhiteSpace(pathXml))
            {
                return false;
            }

            if (!File.Exists(pathXml))
            {
                return false;
            }

            var doc = new XmlDocument();
            doc.Load(pathXml);

            if (doc == null)
            {
                throw new ArgumentException("Doc");
            }

            var key = RetornaRsaProvider();
            var signedXml = new SignedXml(doc);
            var nodeList = doc.GetElementsByTagName("Signature");

            if (nodeList.Count <= 0)
            {
                return false;
                //throw new CryptographicException("Documento não assinado.");
            }

            if (nodeList.Count >= 2)
            {
                throw new CryptographicException("Documento possui mais de uma assinatura.");
            }

            signedXml.LoadXml((XmlElement)nodeList[0]);

            return signedXml.CheckSignature(key);
        }


        #region Métodos Privados

        private static RSACryptoServiceProvider RetornaAssinaturaRsa(StringWriter str)
        {
            var saida = RetornaRsaProvider();

            var byteArray = Encoding.ASCII.GetBytes(str.ToString());
            var stream = new MemoryStream(byteArray);
            saida.SignData(stream, CryptoConfig.MapNameToOID("SHA1"));
            stream.Dispose();
            return saida;
        }

        private static RSACryptoServiceProvider RetornaRsaProvider()
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(PublicPrivateKey);

            return rsa;
        }

        private static void AssinaXml(XmlDocument xmlDoc, AsymmetricAlgorithm key)
        {

            if (xmlDoc == null)
            {
                throw new ArgumentException("xmlDoc");
            }
            if (key == null)
            {
                throw new ArgumentException("Key");
            }

            var signedXml = new SignedXml(xmlDoc) { SigningKey = key };

            var reference = new Reference { Uri = "" };

            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            signedXml.AddReference(reference);
            signedXml.ComputeSignature();

            var xmlDigitalSignature = signedXml.GetXml();

            if (xmlDoc.DocumentElement != null)
            {
                xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
            }
        }

        #endregion

    }
}
