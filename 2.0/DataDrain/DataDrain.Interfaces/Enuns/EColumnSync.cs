
namespace DataDrain.Rules.Enuns
{
    /// <summary>
    /// Define o tipo de atualização do Campo
    /// </summary>
    public enum EColumnSync
    {
        /// <summary>
        /// Atualiza o campo no Insert e Update
        /// </summary>
        Always = 0,
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
}
