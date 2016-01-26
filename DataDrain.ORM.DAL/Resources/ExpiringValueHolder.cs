using System;

namespace TesteDAL.Apoio.Cache
{
    internal class ExpiringValueHolder<T>
    {
        public T Value { get; private set; }

        public DateTime Expiry { get; private set; }

        public ExpiringValueHolder(T value, TimeSpan expiresAfter)
        {
            Value = value;
            Expiry = DateTime.Now.Add(expiresAfter);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
