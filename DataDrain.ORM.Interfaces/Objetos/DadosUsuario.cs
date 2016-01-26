
using System;
using System.Security;

namespace DataDrain.ORM.Interfaces.Objetos
{
    public class DadosUsuario
    {

        public DadosUsuario()
        {
            ID = Guid.NewGuid().ToString("N");
        }

        public string ID { get; set; }
        public string MaquinaID { get; set; }
        public string Usuario { get; set; }
        public SecureString Senha { get; set; }
        public string Servidor { get; set; }
        public string DataBase { get; set; }
        public int Porta { get; set; }
        public bool TrustedConnection { get; set; }
    }
}
