using System;
using System.Collections.Generic;
using System.Numerics;

namespace Clifton.Kademlia.Interfaces
{
    public interface IRpcError
    {
        bool HasError { get; }
    }

    public interface IContact
    {
        IProtocol Protocol { get; set; }
        ID ID { get; }
    }

    public interface IKBucket
    {
        BigInteger Low { get; }
        BigInteger High { get; }
    }

    public interface IProtocol
    {
        IRpcError Ping(IContact sender);
        (List<IContact> contacts, IRpcError error) FindNode(IContact sender, IID key);
        (List<IContact> contacts, string val, IRpcError error) FindValue(IContact sender, IID key);
        IRpcError Store(IContact sender, IID key, string val, bool isCached = false, int expirationTimeSec = 0);
    }

    public interface IStorage : IEnumerable<BigInteger>
    {
        bool HasValues { get; }
        bool Contains(IID key);
        bool TryGetValue(IID key, out string val);
        string Get(IID key);
        string Get(BigInteger key);
        DateTime GetTimeStamp(BigInteger key);
        void Set(IID key, string value, int expirationTimeSec = 0);
        int GetExpirationTimeSec(BigInteger key);
        void Remove(BigInteger key);

        /// <summary>
        /// Updates the republish timestamp.
        /// </summary>
        void Touch(BigInteger key);
    }
}
