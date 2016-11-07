using System;

namespace DataDrain.Rules.SuportObjects
{
    public sealed class DatabaseUser
    {
        public DatabaseUser()
        {
            UserId = Guid.NewGuid().ToString("N");
        }

        public string UserId { get; private set; }

        public string MachineId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string ServerAddress { get; set; }

        public string DatabaseName { get; set; }

        public int Port { get; set; }

        public bool IsTrustedConnection { get; set; }

        public string NomeProvedor { get; set; }
    }
}
