using System.Collections.Generic;

using Clifton.Kademlia.Common;

namespace Clifton.Kademlia
{
    public class FindResult
    {
        public bool Found { get; set; }
        public List<Contact> FoundContacts { get; set; }
        public Contact FoundBy { get; set; }
        public string FoundValue { get; set; }
    }
}
