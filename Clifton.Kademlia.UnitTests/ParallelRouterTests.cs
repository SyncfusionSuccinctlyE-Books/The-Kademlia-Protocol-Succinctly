using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Kademlia;
using Clifton.Kademlia.Common;

namespace UnitTests2
{
    [TestClass]
    public class ParallelRouterTests
    {
        [TestMethod]
        public void ParallelLocalStoreFoundValueTest()
        {
            VirtualProtocol vp = new VirtualProtocol();
            Dht dht = new Dht(ID.RandomID, vp, () => new VirtualStorage(), new ParallelRouter());
            vp.Node = dht.Router.Node;
            ID key = ID.RandomID;
            string val = "Test";
            dht.Store(key, val);
            string retval = dht.FindValue(key).val;
            Assert.IsTrue(retval == val, "Expected to get back what we stored");
        }

        // This test creates a single contact and stores the value in that contact.  
        // We set up the ID's so that the contact's ID is less (XOR metric) than our peer's ID, 
        // and we use a key of ID.Zero to prevent further complexities when computing the distance.  
        [TestMethod]
        public void ParallelValueStoredInCloserNodeTest()
        {
            VirtualProtocol vp1 = new VirtualProtocol();
            VirtualProtocol vp2 = new VirtualProtocol();
            VirtualStorage store1 = new VirtualStorage();
            VirtualStorage store2 = new VirtualStorage();

            // Ensures that all nodes are closer, because ID.Max ^ n < ID.Max when n > 0.
            Dht dht = new Dht(ID.Max, vp1, new ParallelRouter(), store1, store1, new VirtualStorage());
            vp1.Node = dht.Router.Node;

            ID contactID = ID.Mid;      // a closer contact.
            Contact otherContact = new Contact(vp2, contactID);
            Node otherNode = new Node(otherContact, store2);
            vp2.Node = otherNode;

            // Add this other contact to our peer list.
            dht.Router.Node.BucketList.AddContact(otherContact);

            // We want an integer distance, not an XOR distance.
            ID key = ID.Zero;

            // Set the value in the other node, to be discovered by the lookup process.
            string val = "Test";
            otherNode.SimpleStore(key, val);

            Assert.IsFalse(store1.Contains(key), "Expected our peer to NOT have cached the key-value.");
            Assert.IsTrue(store2.Contains(key), "Expected other node to HAVE cached the key-value.");

            // Try and find the value, given our Dht knows about the other contact.
            string retval = dht.FindValue(key).val;

            Assert.IsTrue(retval == val, "Expected to get back what we stored");
        }

        /// <summary>
        /// As compared to the test above, we can change the setup of the ID's and verify that the we find the value in a farther node.
        /// </summary>
        [TestMethod]
        public void ParallelValueStoredInFartherNodeTest()
        {
            VirtualProtocol vp1 = new VirtualProtocol();
            VirtualProtocol vp2 = new VirtualProtocol();
            VirtualStorage store1 = new VirtualStorage();
            VirtualStorage store2 = new VirtualStorage();

            // Ensures that all nodes are closer, because ID.Max ^ n < ID.Max when n > 0.
            Dht dht = new Dht(ID.Zero, vp1, new ParallelRouter(), store1, store1, new VirtualStorage());
            vp1.Node = dht.Router.Node;

            ID contactID = ID.Max;      // a farther contact.
            Contact otherContact = new Contact(vp2, contactID);
            Node otherNode = new Node(otherContact, store2);
            vp2.Node = otherNode;

            // Add this other contact to our peer list.
            dht.Router.Node.BucketList.AddContact(otherContact);

            // We want an integer distance, not an XOR distance.
            ID key = ID.One;

            // Set the value in the other node, to be discovered by the lookup process.
            string val = "Test";
            otherNode.SimpleStore(key, val);

            Assert.IsFalse(store1.Contains(key), "Expected our peer to NOT have cached the key-value.");
            Assert.IsTrue(store2.Contains(key), "Expected other node to HAVE cached the key-value.");

            // Try and find the value, given our Dht knows about the other contact.
            string retval = dht.FindValue(key).val;

            Assert.IsTrue(retval == val, "Expected to get back what we stored");
        }

        /// <summary>
        /// Here we test that when we store a value to our peer, it also gets propagated to another peer that our peer knows about.
        /// </summary>
        [TestMethod]
        public void ParallelValueStoredGetsPropagatedTest()
        {
            VirtualProtocol vp1 = new VirtualProtocol();
            VirtualProtocol vp2 = new VirtualProtocol();
            VirtualStorage store1 = new VirtualStorage();
            VirtualStorage store2 = new VirtualStorage();

            // Ensures that all nodes are closer, because ID.Max ^ n < ID.Max when n > 0.
            Dht dht = new Dht(ID.Max, vp1, new ParallelRouter(), store1, store1, new VirtualStorage());
            vp1.Node = dht.Router.Node;

            ID contactID = ID.Mid;      // a closer contact.
            Contact otherContact = new Contact(vp2, contactID);
            Node otherNode = new Node(otherContact, store2);
            vp2.Node = otherNode;

            // Add this other contact to our peer list.
            dht.Router.Node.BucketList.AddContact(otherContact);

            // We want an integer distance, not an XOR distance.
            ID key = ID.Zero;
            string val = "Test";

            Assert.IsFalse(store1.Contains(key), "Obviously we don't have the key-value yet.");
            Assert.IsFalse(store2.Contains(key), "And equally obvious, the other peer doesn't have the key-value yet either.");

            dht.Store(key, val);

            Assert.IsTrue(store1.Contains(key), "Expected our peer to have stored the key-value.");
            Assert.IsTrue(store2.Contains(key), "Expected the other peer to have stored the key-value.");
        }

        /// <summary>
        /// Verify that, given three nodes (the first of which is us), where node 2 has the value, 
        /// a get value also propagates to node 3.
        /// </summary>
        [TestMethod]
        public void ParallelGetValuePropagatesToCloserNodeTest()
        {
            VirtualProtocol vp1 = new VirtualProtocol();
            VirtualProtocol vp2 = new VirtualProtocol();
            VirtualProtocol vp3 = new VirtualProtocol();
            VirtualStorage store1 = new VirtualStorage();
            VirtualStorage store2 = new VirtualStorage();
            VirtualStorage store3 = new VirtualStorage();
            VirtualStorage cache3 = new VirtualStorage();

            // Ensures that all nodes are closer, because ID.Max ^ n < ID.Max when n > 0.
            Dht dht = new Dht(ID.Max, vp1, new ParallelRouter(), store1, store1, new VirtualStorage());
            vp1.Node = dht.Router.Node;

            // Setup node 2:

            ID contactID2 = ID.Mid;      // a closer contact.
            Contact otherContact2 = new Contact(vp2, contactID2);
            Node otherNode2 = new Node(otherContact2, store2);
            vp2.Node = otherNode2;

            // Add the second contact to our peer list.
            dht.Router.Node.BucketList.AddContact(otherContact2);

            // Node 2 has the value.
            // We want an integer distance, not an XOR distance.
            ID key = ID.Zero;
            string val = "Test";
            otherNode2.Storage.Set(key, val);

            // Setup node 3:

            ID contactID3 = ID.Zero.SetBit(158);      // 01000.... -- a farther contact.
            Contact otherContact3 = new Contact(vp3, contactID3);
            Node otherNode3 = new Node(otherContact3, store3, cache3);
            vp3.Node = otherNode3;

            // Add the third contact to our peer list.
            dht.Router.Node.BucketList.AddContact(otherContact3);

            Assert.IsFalse(store1.Contains(key), "Obviously we don't have the key-value yet.");
            Assert.IsFalse(store3.Contains(key), "And equally obvious, the third peer doesn't have the key-value yet either.");

            var ret = dht.FindValue(key);

            Assert.IsTrue(ret.found, "Expected value to be found.");
            Assert.IsFalse(store3.Contains(key), "Key should not be in the republish store.");
            Assert.IsTrue(cache3.Contains(key), "Key should be in the cache store.");
            Assert.IsTrue(cache3.GetExpirationTimeSec(key.Value) == Constants.EXPIRATION_TIME_SECONDS / 2, "Expected 12 hour expiration.");
        }

        [TestMethod]
        public void ParallelBootstrapWithinBootstrappingBucketTest()
        {
            // We need 22 virtual protocols.  One for the bootstrap peer,
            // 10 for the nodes the bootstrap peer knows about, and 10 for the nodes
            // one of those nodes knows about, and one for us to rule them all.
            VirtualProtocol[] vp = new VirtualProtocol[22];
            22.ForEach((i) => vp[i] = new VirtualProtocol());

            // Us
            Dht dhtUs = new Dht(ID.RandomID, vp[0], () => new VirtualStorage(), new ParallelRouter());
            vp[0].Node = dhtUs.Router.Node;

            // Our bootstrap peer
            Dht dhtBootstrap = new Dht(ID.RandomID, vp[1], () => new VirtualStorage(), new ParallelRouter());
            vp[1].Node = dhtBootstrap.Router.Node;
            Node n = null;

            // Our boostrapper knows 10 contacts
            10.ForEach((i) =>
            {
                Contact c = new Contact(vp[i + 2], ID.RandomID);
                n = new Node(c, new VirtualStorage());
                vp[i + 2].Node = n;
                dhtBootstrap.Router.Node.BucketList.AddContact(c);
            });

            // One of those nodes, in this case the last one we added to our bootstrapper
            // for convenience, knows about 10 other contacts.
            10.ForEach((i) =>
            {
                Contact c = new Contact(vp[i + 12], ID.RandomID);
                Node n2 = new Node(c, new VirtualStorage());
                vp[i + 12].Node = n;
                n.BucketList.AddContact(c);     // Note we're adding these contacts to the 10th node.
            });

            dhtUs.Bootstrap(dhtBootstrap.Router.Node.OurContact);

            Assert.IsTrue(dhtUs.Router.Node.BucketList.Buckets.Sum(c => c.Contacts.Count) == 11, "Expected our peer to get 11 contacts.");
        }

        [TestMethod]
        public void ParallelBootstrapOutsideBootstrappingBucketTest()
        {
            // We need 32 virtual protocols.  One for the bootstrap peer,
            // 20 for the nodes the bootstrap peer knows about, 10 for the nodes
            // one of those nodes knows about, and one for us to rule them all.
            VirtualProtocol[] vp = new VirtualProtocol[32];
            32.ForEach((i) => vp[i] = new VirtualProtocol());

            // Us, ID doesn't matter.
            Dht dhtUs = new Dht(ID.RandomID, vp[0], () => new VirtualStorage(), new ParallelRouter());
            vp[0].Node = dhtUs.Router.Node;

            // Our bootstrap peer
            // All ID's are < 2^159
            Dht dhtBootstrap = new Dht(ID.Zero.RandomizeBeyond(Constants.ID_LENGTH_BITS - 1), vp[1], () => new VirtualStorage(), new ParallelRouter());
            vp[1].Node = dhtBootstrap.Router.Node;
            Node n = null;

            // Our boostrapper knows 20 contacts
            20.ForEach((i) =>
            {
                ID id;

                // All ID's are < 2^159 except the last one, which is >= 2^159
                // which will force a bucket split for _us_
                if (i < 19)
                {
                    id = ID.Zero.RandomizeBeyond(Constants.ID_LENGTH_BITS - 1);
                }
                else
                {
                    id = ID.Max;
                }

                Contact c = new Contact(vp[i + 2], id);
                n = new Node(c, new VirtualStorage());
                vp[i + 2].Node = n;
                dhtBootstrap.Router.Node.BucketList.AddContact(c);
            });

            // One of those nodes, in this case specifically the last one we added to our bootstrapper
            // so that it isn't in the bucket of our bootstrapper, we add 10 contacts.  The ID's of
            // those contacts don't matter.
            10.ForEach((i) =>
            {
                Contact c = new Contact(vp[i + 22], ID.RandomID);
                Node n2 = new Node(c, new VirtualStorage());
                vp[i + 22].Node = n;
                n.BucketList.AddContact(c);     // Note we're adding these contacts to the 10th node.
            });

            dhtUs.Bootstrap(dhtBootstrap.Router.Node.OurContact);

            Assert.IsTrue(dhtUs.Router.Node.BucketList.Buckets.Sum(c => c.Contacts.Count) == 31, "Expected our peer to have 31 contacts.");
        }
    }
}
