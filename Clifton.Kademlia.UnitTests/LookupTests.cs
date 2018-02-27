#define IGNORE_SLOW_TESTS

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Kademlia;
using Clifton.Kademlia.Common;

namespace UnitTests2
{
    [TestClass]
    public class LookupTests
    {
        protected Router router;
        protected List<Node> nodes;
        protected ID key;
        protected List<Contact> contactsToQuery;
        protected List<Contact> closerContacts;
        protected List<Contact> fartherContacts;
        protected List<Contact> closerContactsAltComputation;
        protected List<Contact> fartherContactsAltComputation;
        Contact theNearestContactedNode;
        BigInteger distance;

        /// <summary>
        /// Given that all the nodes we're contacting are nodes *being* contacted, 
        /// the result should be no new nodes to contact.
        /// </summary>
        [TestMethod]
        public void NoNodesToQueryTest()
        {
            // Setup
            router = new Router(new Node(new Contact(null, ID.Mid), new VirtualStorage()));

            nodes = new List<Node>();
            20.ForEach((n) => nodes.Add(new Node(new Contact(null, new ID(BigInteger.Pow(new BigInteger(2), n))), new VirtualStorage())));

            // Fixup protocols:
            nodes.ForEach(n => n.OurContact.Protocol = new VirtualProtocol(n));

            // Our contacts:
            nodes.ForEach(n => router.Node.BucketList.AddContact(n.OurContact));

            // Each peer needs to know about the other peers except of course itself.
            nodes.ForEach(n => nodes.Where(nOther => nOther != n).ForEach(nOther => n.BucketList.AddContact(nOther.OurContact)));

            // Select the key such that n ^ 0 == n
            // This ensures that the distance metric uses only the node ID, which makes for an integer difference for distance, not an XOR distance.
            key = ID.Zero;          
            contactsToQuery = router.Node.BucketList.Buckets[0].Contacts;   // all contacts are in one bucket.

            closerContacts = new List<Contact>();
            fartherContacts = new List<Contact>();

            contactsToQuery.ForEach(c =>
            {
                router.GetCloserNodes(key, c, router.RpcFindNodes, closerContacts, fartherContacts, out var _, out var _);
            });

            Assert.IsTrue(closerContacts.ExceptBy(contactsToQuery, c=>c.ID).Count() == 0, "No new nodes expected.");
            Assert.IsTrue(fartherContacts.ExceptBy(contactsToQuery, c=>c.ID).Count() == 0, "No new nodes expected.");
        }

        /// <summary>
        /// Creates a single bucket with node ID's 2^i for i in [0, K) and
        /// 1. use a key with ID.Value == 0 to that distance computation is an integer difference
        /// 2. use an ID.Value == ID.Max for our node ID so all other nodes are closer.
        /// </summary>
        [TestMethod]
        public void SimpleAllCloserContacts()
        {
            // Setup
            // By selecting our node ID to zero, we ensure that all distances of other nodes are > the distance to our node.
            router = new Router(new Node(new Contact(null, ID.Max), new VirtualStorage()));

            nodes = new List<Node>();
            Constants.K.ForEach((n) => nodes.Add(new Node(new Contact(null, new ID(BigInteger.Pow(new BigInteger(2), n))), new VirtualStorage())));

            // Fixup protocols:
            nodes.ForEach(n => n.OurContact.Protocol = new VirtualProtocol(n));

            // Our contacts:
            nodes.ForEach(n => router.Node.BucketList.AddContact(n.OurContact));

            // Each peer needs to know about the other peers except of course itself.
            nodes.ForEach(n => nodes.Where(nOther => nOther != n).ForEach(nOther => n.BucketList.AddContact(nOther.OurContact)));

            // Select the key such that n ^ 0 == n
            // This ensures that the distance metric uses only the node ID, which makes for an integer difference for distance, not an XOR distance.
            key = ID.Zero;
            contactsToQuery = router.Node.BucketList.Buckets[0].Contacts;   // all contacts are in one bucket.

            var contacts = router.Lookup(key, router.RpcFindNodes, true).contacts;

            Assert.IsTrue(contacts.Count == Constants.K, "Expected k closer contacts.");
            Assert.IsTrue(router.CloserContacts.Count == Constants.K, "All contacts should be closer.");
            Assert.IsTrue(router.FartherContacts.Count == 0, "Expected no farther contacts.");
        }

        /// <summary>
        /// Creates a single bucket with node ID's 2^i for i in [0, K) and
        /// 1. use a key with ID.Value == 0 to that distance computation is an integer difference
        /// 2. use an ID.Value == 0 for our node ID so all other nodes are farther.
        /// </summary>
        [TestMethod]
        public void SimpleAllFartherContacts()
        {
            // Setup
            // By selecting our node ID to zero, we ensure that all distances of other nodes are > the distance to our node.
            router = new Router(new Node(new Contact(null, ID.Zero), new VirtualStorage()));

            nodes = new List<Node>();
            Constants.K.ForEach((n) => nodes.Add(new Node(new Contact(null, new ID(BigInteger.Pow(new BigInteger(2), n))), new VirtualStorage())));

            // Fixup protocols:
            nodes.ForEach(n => n.OurContact.Protocol = new VirtualProtocol(n));

            // Our contacts:
            nodes.ForEach(n => router.Node.BucketList.AddContact(n.OurContact));

            // Each peer needs to know about the other peers except of course itself.
            nodes.ForEach(n => nodes.Where(nOther => nOther != n).ForEach(nOther => n.BucketList.AddContact(nOther.OurContact)));

            // Select the key such that n ^ 0 == n
            // This ensures that the distance metric uses only the node ID, which makes for an integer difference for distance, not an XOR distance.
            key = ID.Zero;
            // contactsToQuery = router.Node.BucketList.Buckets[0].Contacts;   // all contacts are in one bucket.

            var contacts = router.Lookup(key, router.RpcFindNodes, true).contacts;

            Assert.IsTrue(contacts.Count == 0, "Expected no closer contacts.");
            Assert.IsTrue(router.CloserContacts.Count == 0, "Did not expected closer contacts.");
            Assert.IsTrue(router.FartherContacts.Count == Constants.K, "All contacts should be farther.");
        }

        [TestMethod]
#if IGNORE_SLOW_TESTS
        [Ignore]
#endif
        public void GetCloserNodesTest()
        {
            // Seed with different random values
            100.ForEach(seed =>
            {
                ID.rnd = new Random(seed);
                Setup();

                contactsToQuery.ForEach(c =>
                {
                    router.GetCloserNodes(key, c, router.RpcFindNodes, closerContacts, fartherContacts, out var _, out var _);
                });

                // Test whether the results are correct:  

                GetAltCloseAndFar(contactsToQuery, closerContactsAltComputation, fartherContactsAltComputation);

                Assert.IsTrue(closerContacts.Count == closerContactsAltComputation.Count, "Closer computations do not match.");
                Assert.IsTrue(fartherContacts.Count == fartherContactsAltComputation.Count, "Farther computations do not match.");
            });
        }

        [TestMethod]
#if IGNORE_SLOW_TESTS
        [Ignore]
#endif
        public void LookupTest()
        {
            // Seed with different random values
            100.ForEach(seed =>
            {
                ID.rnd = new Random(seed);
                Setup();

                List<Contact> closeContacts = router.Lookup(key, router.RpcFindNodes, true).contacts;
                List<Contact> contactedNodes = new List<Contact>(closeContacts);

                // Is the above call returning the correct number of close contacts?
                // The unit test for this is sort of lame.  We should get at least as many contacts 
                // as when calling GetCloserNodes.  

                GetAltCloseAndFar(contactsToQuery, closerContactsAltComputation, fartherContactsAltComputation);

                Assert.IsTrue(closeContacts.Count >= closerContactsAltComputation.Count, "Expected at least as many contacts.");

                // Technically, we can't even test whether the contacts returned in GetCloserNodes exists
                // in router.Lookup because it may have found nodes even closer, and it only returns K nodes!
                // We can overcome this by eliminating the Take in the return of router.Lookup().

                closerContactsAltComputation.ForEach(c => Assert.IsTrue(closeContacts.Contains(c)));
            });
        }

        protected void Setup()
        {
            // Setup
            router = new Router(new Node(new Contact(null, ID.RandomID), new VirtualStorage()));

            nodes = new List<Node>();
            100.ForEach(() =>
            {
                Contact contact = new Contact(new VirtualProtocol(), ID.RandomID);
                Node node = new Node(contact, new VirtualStorage());
                ((VirtualProtocol)contact.Protocol).Node = node;
                nodes.Add(node);
            });

            // Fixup protocols:
            nodes.ForEach(n => n.OurContact.Protocol = new VirtualProtocol(n));

            // Our contacts:
            nodes.ForEach(n => router.Node.BucketList.AddContact(n.OurContact));

            // Each peer needs to know about the other peers except of course itself.
            nodes.ForEach(n => nodes.Where(nOther => nOther != n).ForEach(nOther => n.BucketList.AddContact(nOther.OurContact)));

            // Pick a random bucket, or bucket where the key is in range, otherwise we're defeating the purpose of the algorithm.
            key = ID.RandomID;            // Pick an ID
            // DO NOT DO THIS:
            // List<Contact> nodesToQuery = router.Node.BucketList.GetCloseContacts(key, router.Node.OurContact.ID).Take(Constants.ALPHA).ToList();

            contactsToQuery = router.Node.BucketList.GetKBucket(key).Contacts.Take(Constants.ALPHA).ToList();
            // or:
            // contactsToQuery = router.FindClosestNonEmptyKBucket(key).Contacts.Take(Constants.ALPHA).ToList();

            closerContacts = new List<Contact>();
            fartherContacts = new List<Contact>();

            closerContactsAltComputation = new List<Contact>();
            fartherContactsAltComputation = new List<Contact>();
            theNearestContactedNode = contactsToQuery.OrderBy(n => n.ID ^ key).First();
            distance = theNearestContactedNode.ID.Value ^ key.Value;
        }

        protected void GetAltCloseAndFar(List<Contact> contactsToQuery, List<Contact> closer, List<Contact> farther)
        {
            // For each node (ALPHA == K for testing) in our bucket (nodesToQuery) we're going to get k nodes closest to the key:
            foreach (Contact contact in contactsToQuery)
            {
                // Find the node we're contacting:
                Node contactNode = nodes.Single(n => n.OurContact == contact);

                // Close contacts except ourself and the nodes we're contacting.
                // Note that of all the contacts in the bucket list, many of the k returned
                // by the GetCloseContacts call are contacts we're querying, so they are being excluded.
                var closeContactsOfContactedNode =
                    contactNode.
                        BucketList.
                        GetCloseContacts(key, router.Node.OurContact.ID).
                        ExceptBy(contactsToQuery, c => c.ID.Value);

                foreach (Contact closeContactOfContactedNode in closeContactsOfContactedNode)
                {
                    // Which of these contacts are closer?
                    if ((closeContactOfContactedNode.ID ^ key) < distance)
                    {
                        closer.AddDistinctBy(closeContactOfContactedNode, c => c.ID.Value);
                    }

                    // Which of these contacts are farther?
                    if ((closeContactOfContactedNode.ID ^ key) >= distance)
                    {
                        farther.AddDistinctBy(closeContactOfContactedNode, c => c.ID.Value);
                    }
                }
            }
        }
    }
}
