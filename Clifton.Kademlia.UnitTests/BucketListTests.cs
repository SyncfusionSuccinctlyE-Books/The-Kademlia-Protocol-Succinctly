using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Kademlia;
using Clifton.Kademlia.Common;

namespace UnitTests2
{
    [TestClass]
    public class BucketListTests
    {
        [TestMethod]
        public void GetCloseContactsOrderedTest()
        {
            Contact sender = new Contact(null, ID.RandomID);
            Node node = new Node(new Contact(null, ID.RandomID), new VirtualStorage());
            List<Contact> contacts = new List<Contact>();
            // Force multiple buckets.
            100.ForEach(() => contacts.Add(new Contact(null, ID.RandomID)));
            contacts.ForEach(c => node.BucketList.AddContact(c));
            ID key = ID.RandomID;            // Pick an ID
            List<Contact> closest = node.FindNode(sender, key).contacts;

            Assert.IsTrue(closest.Count == Constants.K, "Expected K contacts to be returned.");

            // The contacts should be in ascending order with respect to the key.
            var distances = closest.Select(c => c.ID ^ key).ToList();
            var distance = distances[0];

            // Verify distances are in ascending order:
            distances.Skip(1).ForEach(d =>
            {
                Assert.IsTrue(distance < d, "Expected contacts ordered by distance.");
                distance = d;
            });

            // Verify the contacts with the smallest distances were returned from all possible distances.
            var lastDistance = distances[distances.Count - 1];
            var others = node.BucketList.Buckets.SelectMany(b => b.Contacts.Except(closest)).Where(c => (c.ID ^ key) < lastDistance);
            Assert.IsTrue(others.Count() == 0, "Expected no other contacts with a smaller distance than the greatest distance to exist.");
        }
    }
}
