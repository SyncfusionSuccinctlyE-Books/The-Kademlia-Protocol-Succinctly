using System;

namespace Clifton.Kademlia.Common
{
    public class StoreValue
    {
        public string Value { get; set; }
        public DateTime RepublishTimeStamp { get; set; }
        public int ExpirationTime { get; set; }
    }
}
