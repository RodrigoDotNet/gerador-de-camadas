using System.Collections.Generic;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Interfaces
{
    public interface IInformationSchemaProcedure
    {
        List<DadosObjeto> ListaAllStoredProcedures(string dataBaseName, DadosUsuario usr);

        List<DadosStoredProceduresParameters> ListaAllStoredProceduresParameters(string dataBaseName, DadosUsuario usr, string procedureName);

        List<DadosColunas> ListAllFieldsFromStoredProcedure(string dataBaseName, string procedureName, List<DadosStoredProceduresParameters> parametros, DadosUsuario usr);
    }
}
