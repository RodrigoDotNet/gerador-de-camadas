
namespace DataDrain.Rules.Interfaces
{
    public interface IInfoConnection
    {
        int DefaultPort { get; }

        string MinimalVersion { get; }

        bool IsTrustedConnection { get; }
    }
}
