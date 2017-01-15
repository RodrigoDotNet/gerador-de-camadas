using System.Collections.Generic;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Interfaces
{
    public interface IInformationSchemaView
    {
        List<DadosObjeto> ListaAllViews(string dataBaseName, DadosUsuario usr);

        List<DadosColunas> ListAllFieldsFromViews(string dataBaseName, string viewsName, DadosUsuario usr);
    }
}
