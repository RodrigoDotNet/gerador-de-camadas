using System.Collections.Generic;
using System.Linq;
using Apoio.CommandMap;
using Apoio.Enumeradores;
using MySql.Data.MySqlClient;

namespace Apoio.Mapping
{
    internal static class Transaction
    {
        #region .: Métodos :.

        /// <summary>
        /// Executa lista de comando sem retorno gerando um comando pré compilado
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clsGeneric">lista generica</param>
        /// <param name="cnn">conexão com o BD</param>
        /// <param name="tipoComando">Tipo de consulta</param>
        public static void ExecutaListaComandosSemRetorno<T>(List<T> clsGeneric, MySqlConnection cnn, ETipoConsulta tipoComando)
        {
            MySqlTransaction trans = null;

            try
            {
                if (clsGeneric.Count > 0)
                {

                    var cmdMaster = CmdMap.CreateDbCommand(clsGeneric.FirstOrDefault(), tipoComando);

                    cmdMaster.Transaction = cnn.BeginTransaction();

                    trans = cmdMaster.Transaction;

                    cmdMaster.ExecuteNonQuery();

                    foreach (var cmd in clsGeneric.Skip(1).Select(generic => CmdMap.CarregaValoresDbCommand(generic, tipoComando, cmdMaster)))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
            }
            catch
            {
                if (trans != null) trans.Rollback();
                throw;
            }
        }

        #endregion
    }
}
