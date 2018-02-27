using System.Collections.Generic;

using Clifton.Kademlia;

namespace KademliaDemo
{
    public class VirtualProtocol : IProtocol
    {
        public Node Node { get; set; }
        public bool Responds { get; set; }

        /// <summary>
        /// For unit testing with deferred node setup.
        /// </summary>
        public VirtualProtocol(bool responds = true)
        {
            Responds = responds;
        }

        /// <summary>
        /// Register the in-memory node with our virtual protocol.
        /// </summary>
        public VirtualProtocol(Node node, bool responds = true)
        {
            Node = node;
            Responds = responds;
        }

        public bool Ping(Contact sender)
        {
            // Ping still adds/updates the sender's contact.
            if (Responds)
            {
                Node.Ping(sender);
            }

            return Responds;
        }

        /// <summary>
        /// Get the list of contacts for this node closest to the key.
        /// </summary>
        public (List<Contact> contacts, bool timeoutError) FindNode(Contact sender, ID key)
        {
            return (Node.FindNode(sender, key).contacts, false);
        }

        /// <summary>
        /// Returns either contacts or null if the value is found.
        /// </summary>
        public (List<Contact> contacts, string val, bool timeoutError) FindValue(Contact sender, ID key)
        {
            var (contacts, val) = Node.FindValue(sender, key);

            return (contacts, val, false);
        }

        /// <summary>
        /// Stores the key-value on the remote peer.
        /// </summary>
        public bool Store(Contact sender, ID key, string val, bool isCached = false, int expTimeSec = 0)
        {
            Node.Store(sender, key, val, isCached, expTimeSec);

            return false;
        }
    }
}
