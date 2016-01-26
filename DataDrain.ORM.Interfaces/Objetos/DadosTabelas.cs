
namespace DataDrain.ORM.Interfaces.Objetos
{
    public class DadosTabelas
    {
        public string TabelaNome { get; set; }
        public string ColunaNome { get; set; }
        public int ColunaOrdem { get; set; }
        public string ColunaValorPadrao { get; set; }
        public bool ColunaIsNull { get; set; }
        public string ColunaDataType { get; set; }
        public string ColunaMaximumLength { get; set; }
        public bool ColunaChavePrimaria { get; set; }

    }
}
