using System.Drawing;
using DataDrain.ORM.Interfaces;

namespace DataDrain.ORM.Generator.Apoio
{
    public class Provider
    {
        /// <summary>
        /// Versão atual do provider
        /// </summary>
        public string Versao { get; set; }

        /// <summary>
        /// Versão minima recomendada do banco
        /// </summary>
        public string BancoMinimo { get; set; }

        /// <summary>
        /// Logo do banco
        /// </summary>
        public Image Logo { get; set; }

        public string MapeamentoTabela { get; set; }

        public string MapeamentoView { get; set; }

        public string MapeamentoProcedure { get; set; }

        /// <summary>
        /// Provider de mapeamento
        /// </summary>
        public IInformationSchema Prov { get; set; }
    }
}
