using System;
using DataDrain.Library.ORM.Caching.Enuns;

namespace DataDrain.Library.ORM.Caching.Events
{
    public class CacheChangedEventArgs : EventArgs
    {
        public CacheChangedEventArgs(string chave, ECacheAcao acao)
        {
            Chave = chave;
            Status = acao;
        }

        public string Chave { get; private set; }

        public ECacheAcao Status { get; private set; }
    }
}
