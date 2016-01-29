using System;


namespace DataDrain.ORM.Interfaces.Objetos
{
    public class DadosUsuario
    {

        public DadosUsuario()
        {
            ID = Guid.NewGuid().ToString("N");
            DataBase = "";
        }

        public string ID { get; set; }
        public string MaquinaID { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public string Servidor { get; set; }
        public string DataBase { get; set; }
        public int Porta { get; set; }
        public bool TrustedConnection { get; set; }
        public string NomeProvedor { get; set; }
    }
}
