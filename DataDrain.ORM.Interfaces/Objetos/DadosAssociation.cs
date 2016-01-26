using System;

namespace DataDrain.ORM.Interfaces.Objetos
{
    public class DadosAssociation
    {
        public String Tabela { get; set; }
        public String Coluna { get; set; }
        public String TabelaFK { get; set; }
        public String ColunaFK { get; set; }
        public String Constraint { get; set; }
    }
}
