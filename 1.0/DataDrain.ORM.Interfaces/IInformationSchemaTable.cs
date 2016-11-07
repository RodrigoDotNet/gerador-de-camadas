using System.Collections.Generic;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Interfaces
{
    public interface IInformationSchemaTable
    {
        List<DadosObjeto> ListaAllTables(string dataBaseName, DadosUsuario usr);

        List<DadosColunas> ListAllFieldsFromTable(string dataBaseName, string tableName, DadosUsuario usr);

        string RetornaEnum(string dataBaseName, string tableName, string colName, DadosUsuario usr);

        List<DadosAssociation> RetornaMapeamentoFK(string nomeTabela, string dataBaseName, DadosUsuario usr);         
    }
}
