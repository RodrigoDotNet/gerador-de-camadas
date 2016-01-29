using DataDrain.ORM.Interfaces.Objetos;

namespace DataDrain.ORM.Interfaces
{
    public interface IRotinasApoio
    {
        string RetornaTipoDadosDotNet(string tipoDados);

        string ConvertTipoClrToSql(string tipoDados);

        void CriarArquivosProjeto(ParametrosCriarProjetos parametros);
    }
}
