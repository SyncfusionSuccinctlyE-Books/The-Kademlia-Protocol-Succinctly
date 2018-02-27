using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;

using Newtonsoft.Json;

using Clifton.Kademlia.Common;

namespace Clifton.Kademlia
{
    /// <summary>
    /// In-memory storage, used for node cache store if not explicitly specified.
    /// </summary>
    public class VirtualStorage : IStorage
    {
        [JsonIgnore]
        public List<BigInteger> Keys { get { return new List<BigInteger>(store.Keys); } }

        protected ConcurrentDictionary<BigInteger, StoreValue> store;

        public VirtualStorage()
        {
            store = new ConcurrentDictionary<BigInteger, StoreValue>();
        }

        public bool TryGetValue(ID key, out string val)
        {
            val = null;
            StoreValue sv;
            bool ret = store.TryGetValue(key.Value, out sv);

            if (ret)
            {
                val = sv.Value;
            }

            return ret;
        }

        public bool Contains(ID key)
        {
            return store.ContainsKey(key.Value);
        }

        public string Get(ID key)
        {
            return store[key.Value].Value;
        }

        public string Get(BigInteger key)
        {
            return store[key].Value;
        }

        public DateTime GetTimeStamp(BigInteger key)
        {
            return store[key].RepublishTimeStamp;
        }

        public int GetExpirationTimeSec(BigInteger key)
        {
            return store[key].ExpirationTime;
        }

		/// <summary>
		/// Updates the republish timestamp.
		/// </summary>
		public void Touch(BigInteger key)
        {
            store[key].RepublishTimeStamp = DateTime.Now;
        }

        public void Set(ID key, string val, int expirationTime)
        {
            store[key.Value] = new StoreValue() { Value = val, ExpirationTime = expirationTime };
            Touch(key.Value);
        }

        public void Remove(BigInteger key)
        {
            store.TryRemove(key, out _);
        }
    }
}
