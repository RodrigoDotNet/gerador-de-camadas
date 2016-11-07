namespace DataDrain.ORM.Interfaces.Objetos
{
    public class DadosAssociation
    {
        public string Tabela { get; set; }
        public string Coluna { get; set; }
        public string TabelaFK { get; set; }
        public string ColunaFK { get; set; }
        public string Constraint { get; set; }
    }
}
