
namespace DataDrain.ORM.Interfaces.Objetos
{
    public class DadosColunas
    {
        /// <summary>
        /// Define o tipo de atualização do Campo
        /// </summary>
        public enum ETipoSync
        {
            /// <summary>
            /// Atualiza o campo no Insert e Update
            /// </summary>
            Always=0,
            /// <summary>
            /// Atualiza a proriedade somente no insert
            /// </summary>
            OnInsert,
            /// <summary>
            /// Atualiza a proriedade somente no update
            /// </summary>
            OnUpdate,
            /// <summary>
            /// Nunca atualiza a propriedade
            /// </summary>
            Never
        }

        public DadosColunas()
        {
            RegExp = "sem";
        }

        public string Coluna { get; set; }
        public string Tipo { get; set; }
        public int Tamanho { get; set; }
        public bool AceitaNull { get; set; }
        public bool Pk { get; set; }

        /// <summary>
        /// Usada para recuperar os ultimo dado inserido em caso de ser identity
        /// </summary>
        public bool Identity { get; set; }
        /// <summary>
        /// Usado para quando tiver Regular Expressions
        /// </summary>
        public string RegExp { get; set; }

        /// <summary>
        /// Verifica o valor padrão da coluna
        /// Estou usando para evitar que colunas com o valor '(getdate())'
        /// sejam atualizadas ou cadastradas
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Indica se a coluna sera cadastrada/atualizada
        /// </summary>
        public bool Sync { get; set; }

        /// <summary>
        /// Define o tipo de atualização do Campo
        /// </summary>
        public ETipoSync TipoSync { get; set; }

    }
}