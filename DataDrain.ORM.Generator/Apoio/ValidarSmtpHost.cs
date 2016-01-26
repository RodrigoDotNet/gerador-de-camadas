using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace DataDrain.ORM.Generator.Apoio
{
    public sealed class ValidarSmtpHost
    {
        /// <summary>
        /// valida se o servidor smtp é valido
        /// </summary>
        /// <param name="host"></param>
        /// <param name="porta"></param>
        /// <returns></returns>
        public static bool Validar(string host, int porta)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(host, porta);
                    using (var stream = client.GetStream())
                    using (var sslStream = new SslStream(stream))
                    {
                        sslStream.AuthenticateAsClient(host);
                        using (var writer = new StreamWriter(sslStream))
                        using (var reader = new StreamReader(sslStream))
                        {
                            var retorno = reader.ReadLine();
                            return retorno != null && (retorno.Contains("250") || retorno.Contains("220"));
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
