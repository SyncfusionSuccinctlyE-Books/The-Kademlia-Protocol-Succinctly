using System.Numerics;

using Clifton.Kademlia.Common;

namespace Clifton.Kademlia.Protocols
{
    public abstract class BaseRequest
    {
        public object Protocol { get; set; }
        public string ProtocolName { get; set; }
        public BigInteger RandomID { get; set; }
        public BigInteger Sender { get; set; }

        public BaseRequest()
        {
            RandomID = ID.RandomID.Value;
        }
    }

    public class FindNodeRequest : BaseRequest
    {
        public BigInteger Key { get; set; }
    }

    public class FindValueRequest : BaseRequest
    {
        public BigInteger Key { get; set; }
    }

    public class PingRequest : BaseRequest { }

    public class StoreRequest : BaseRequest
    {
        public BigInteger Key { get; set; }
        public string Value { get; set; }
        public bool IsCached { get; set; }
        public int ExpirationTimeSec { get; set; }
    }

    public interface ITcpSubnet
    {
        int Subnet { get; set; }
    }

    public class FindNodeSubnetRequest : FindNodeRequest, ITcpSubnet
    {
        public int Subnet { get; set; }
    }

    public class FindValueSubnetRequest : FindValueRequest, ITcpSubnet
    {
        public int Subnet { get; set; }
    }

    public class PingSubnetRequest : PingRequest, ITcpSubnet
    {
        public int Subnet { get; set; }
    }

    public class StoreSubnetRequest : StoreRequest, ITcpSubnet
    {
        public int Subnet { get; set; }
    }
}
