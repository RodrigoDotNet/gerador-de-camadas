using System;

namespace DataDrain.ORM.Interfaces.Objetos
{
    public class DadosObjeto
    {
        public string Tipo { get; set; }
        public string Nome { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataAlteracao { get; set; }
        public int QtdRegistros { get; set; }
    }
}