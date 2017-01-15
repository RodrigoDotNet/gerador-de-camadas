

namespace DataDrain.ORM.Interfaces
{
    public interface IInfoConexao
    {
        int PortaPadrao { get; }

        string VersaoMinima { get; }

        bool TrustedConnection { get; }
    }
}
