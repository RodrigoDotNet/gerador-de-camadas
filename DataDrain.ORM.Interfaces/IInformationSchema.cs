using System.Collections.Generic;
using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Interfaces
{
    public interface IInformationSchema
    {
        List<string> ListAllDatabases(DadosUsuario usr);

        KeyValuePair<bool, string> TestarConexao(DadosUsuario usr);

        IInformationSchemaTable MapeamentoTabela { get; }

        IInformationSchemaView MapeamentoView { get; }

        IInformationSchemaProcedure MapeamentoProcedure { get; }

        IInfoConexao InfoConexao { get; }

        IRotinasApoio RotinasApoio { get; }

        bool CompativelMapeamentoTabela { get; }

        bool CompativelMapeamentoView { get; }

        bool CompativelMapeamentoProcedure { get; }
    }
}
