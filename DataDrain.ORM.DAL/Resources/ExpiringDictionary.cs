using System;
using System.Collections.Generic;
using System.Linq;

namespace TesteDAL.Apoio.Cache
{
    public class ExpiringDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Variaveis

        private readonly Dictionary<TKey, ExpiringValueHolder<TValue>> _innerDictionary;
        private readonly TimeSpan _expiryTimeSpan;
        private System.Timers.Timer timer;

        #endregion

        private void DestoryExpiredItems(TKey key)
        {
            if (_innerDictionary.ContainsKey(key))
            {
                var value = _innerDictionary[key];

                if (value.Expiry < DateTime.Now)
                {
                    _innerDictionary.Remove(key);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minutosCache">tempo em minutos que o objeto ficaria na memooria</param>
        public ExpiringDictionary(Int32 minutosCache)
        {
            if (minutosCache <= 0)
            {
                minutosCache = 5;
            }

            _expiryTimeSpan = new TimeSpan(0, 0, minutosCache, 0);
            _innerDictionary = new Dictionary<TKey, ExpiringValueHolder<TValue>>();

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += (s, e) =>
            {
                foreach (var item in _innerDictionary.Where(item => item.Value.Expiry < DateTime.Now))
                {
                    _innerDictionary.Remove(item.Key);
                }
            };
            timer.Start();
        }

        public void Add(TKey key, TValue value)
        {
            DestoryExpiredItems(key);

            _innerDictionary.Add(key, new ExpiringValueHolder<TValue>(value, _expiryTimeSpan));
        }

        public bool ContainsKey(TKey key)
        {
            DestoryExpiredItems(key);

            return _innerDictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            DestoryExpiredItems(key);

            return _innerDictionary.Remove(key);
        }

        public ICollection<TKey> Keys
        {
            get { return _innerDictionary.Keys; }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var returnval = false;
            DestoryExpiredItems(key);

            if (_innerDictionary.ContainsKey(key))
            {
                value = _innerDictionary[key].Value;
                returnval = true;
            }
            else { value = default(TValue); }

            return returnval;
        }

        public ICollection<TValue> Values
        {
            get { return _innerDictionary.Values.Select(vals => vals.Value).ToList(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                DestoryExpiredItems(key);
                return _innerDictionary[key].Value;
            }
            set
            {
                DestoryExpiredItems(key);
                _innerDictionary[key] = new ExpiringValueHolder<TValue>(value, _expiryTimeSpan);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            DestoryExpiredItems(item.Key);

            _innerDictionary.Add(item.Key, new ExpiringValueHolder<TValue>(item.Value, _expiryTimeSpan));
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }

        public int Count
        {
            get { return _innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
