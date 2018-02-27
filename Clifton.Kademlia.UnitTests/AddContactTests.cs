#define IGNORE_SLOW_TESTS

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Numerics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Kademlia;
using Clifton.Kademlia.Common;

namespace UnitTests2
{
	[TestClass]
	public class AddContactTests
	{
		public const string CRLF = "\r\n";

		[TestMethod]
		public void UniqueIDAddTest()
		{
            Contact dummyContact = new Contact(new VirtualProtocol(), ID.Zero);
            ((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());
            BucketList bucketList = new BucketList(ID.RandomIDInKeySpace, dummyContact);
			Constants.K.ForEach(() => bucketList.AddContact(new Contact(null, ID.RandomIDInKeySpace)));
			Assert.IsTrue(bucketList.Buckets.Count == 1, "No split should have taken place.");
			Assert.IsTrue(bucketList.Buckets[0].Contacts.Count == Constants.K, "K contacts should have been added.");									
		}

		[TestMethod]
		public void DuplicateIDTest()
		{
            Contact dummyContact = new Contact(new VirtualProtocol(), ID.Zero);
            ((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());
            BucketList bucketList = new BucketList(ID.RandomIDInKeySpace, dummyContact);
			ID id = ID.RandomIDInKeySpace;
			bucketList.AddContact(new Contact(null, id));
			bucketList.AddContact(new Contact(null, id));
			Assert.IsTrue(bucketList.Buckets.Count == 1, "No split should have taken place.");
			Assert.IsTrue(bucketList.Buckets[0].Contacts.Count == 1, "Bucket should have one contact.");
		}

		[TestMethod]
		public void BucketSplitTest()
		{
            Contact dummyContact = new Contact(new VirtualProtocol(), ID.Zero);
            ((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());
            BucketList bucketList = new BucketList(ID.RandomIDInKeySpace, dummyContact);
			Constants.K.ForEach(() => bucketList.AddContact(new Contact(null, ID.RandomIDInKeySpace)));
			bucketList.AddContact(new Contact(null, ID.RandomIDInKeySpace));
			Assert.IsTrue(bucketList.Buckets.Count > 1, "Bucket should have split into two or more buckets.");
		}

        /// <summary>
        /// Force a failed add by choosing node ID's that cause depth mod 5 != 0 to be false.
        /// </summary>
        [TestMethod]
        public void ForceFailedAddTest()
        {
            Contact dummyContact = new Contact(new VirtualProtocol(), ID.Zero);
            ((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());

            IBucketList bucketList = SetupSplitFailure();

            Assert.IsTrue(bucketList.Buckets.Count == 2, "Bucket split should have occurred.");
            Assert.IsTrue(bucketList.Buckets[0].Contacts.Count == 1, "Expected 1 contact in bucket 0.");
            Assert.IsTrue(bucketList.Buckets[1].Contacts.Count == 20, "Expected 20 contacts in bucket 1.");

			// This next contact should not split the bucket as depth == 5 and therefore adding the contact will fail.
			// Any unique ID >= 2^159 will do.
			byte[] id = new byte[20];
			id[19] = 0x80;
            Contact newContact = new Contact(dummyContact.Protocol, new ID(id));
            bucketList.AddContact(newContact);

			Assert.IsTrue(bucketList.Buckets.Count == 2, "Bucket split should not have occurred.");
			Assert.IsTrue(bucketList.Buckets[0].Contacts.Count == 1, "Expected 1 contact in bucket 0.");
			Assert.IsTrue(bucketList.Buckets[1].Contacts.Count == 20, "Expected 20 contacts in bucket 1.");

            // Verify CanSplit -> Evict did not happen.
            Assert.IsFalse(bucketList.Buckets[1].Contacts.Contains(newContact), "Expected new contact NOT to replace an older contact.");
		}

		[TestMethod]
#if IGNORE_SLOW_TESTS
        [Ignore]
#endif
        public void RandomIDDistributionTest()
		{
            Contact dummyContact = new Contact(new VirtualProtocol(), ID.Zero);
            ((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());
            Random rnd = new Random();
			byte[] buffer = new byte[20];
			List<int> contactsAdded = new List<int>();

			100.ForEach(() =>
			{
				rnd.NextBytes(buffer);
				BucketList bucketList = new BucketList(new ID(buffer), dummyContact);

				3200.ForEach(() =>
				{
					rnd.NextBytes(buffer);
                    Contact contact = new Contact(new VirtualProtocol(), new ID(buffer));
                    ((VirtualProtocol)contact.Protocol).Node = new Node(contact, new VirtualStorage());
                    bucketList.AddContact(contact);
				});

				int contacts = bucketList.Buckets.Sum(b => b.Contacts.Count);
				contactsAdded.Add(contacts);
			});

			Assert.IsTrue(contactsAdded.Average().ApproximatelyEquals(720, 20), "Unexpected distribution.");
			Assert.IsTrue(contactsAdded.Select(n=>(double)n).StdDev().ApproximatelyEquals(10, 2), "Bad distribution");
		}

		[TestMethod]
#if IGNORE_SLOW_TESTS
        [Ignore]
#endif
        public void RandomPrefixDistributionTest()
		{
            Contact dummyContact = new Contact(new VirtualProtocol(), ID.Zero);
            ((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());
            List<int> contactsAdded = new List<int>();

			100.ForEach(() =>
			{
                ID ourID = ID.RandomIDInKeySpace;
                BucketList bucketList = new BucketList(ourID, dummyContact);
                3200.ForEach(() =>
                {
                    ID id = ID.RandomIDInKeySpace;

                    if (id != ourID)
                    {
                        Contact contact = new Contact(new VirtualProtocol(), id);
                        ((VirtualProtocol)contact.Protocol).Node = new Node(contact, new VirtualStorage());
                        bucketList.AddContact(contact);
                    }
                });

                int contacts = bucketList.Buckets.Sum(b => b.Contacts.Count);
                contactsAdded.Add(contacts);
            });

			double avg = contactsAdded.Average();
			double stdev = contactsAdded.Select(n => (double)n).StdDev();
			Assert.IsTrue(contactsAdded.Average().ApproximatelyEquals(1900, 200), "Unexpected distribution: avg = " + avg);
			Assert.IsTrue(stdev.ApproximatelyEquals(800, 100), "Bad distribution: stdev = " + stdev);
		}

		[TestMethod]
#if IGNORE_SLOW_TESTS
        [Ignore]
#endif
        public void DistributionTestForEachPrefix()
		{
            Contact dummyContact = new Contact(new VirtualProtocol(), ID.Zero);
            ((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());
            Random rnd = new Random();
			StringBuilder sb = new StringBuilder();
			byte[] buffer = new byte[20];

			160.ForEach((i) =>
			{
				BucketList bucketList = new BucketList(new ID(BigInteger.Pow(new BigInteger(2), i)), dummyContact);

				3200.ForEach(() =>
				{
					rnd.NextBytes(buffer);
                    Contact contact = new Contact(new VirtualProtocol(), new ID(buffer));
                    ((VirtualProtocol)contact.Protocol).Node = new Node(contact, new VirtualStorage());
                    bucketList.AddContact(contact);
				});

				int contacts = bucketList.Buckets.Sum(b => b.Contacts.Count);
				sb.Append(i + "," + contacts + CRLF);
			});

			File.WriteAllText("prefixTest.txt", sb.ToString());
		}

		[TestMethod]
#if IGNORE_SLOW_TESTS
        [Ignore]
#endif
        public void DistributionTestForEachPrefixWithRandomPrefixDistributedContacts()
		{
            Contact dummyContact = new Contact(new VirtualProtocol(), ID.Zero);
            ((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());
            StringBuilder sb = new StringBuilder();

			160.ForEach((i) =>
			{
				BucketList bucketList = new BucketList(new ID(BigInteger.Pow(new BigInteger(2), i)), dummyContact);
                Contact contact = new Contact(new VirtualProtocol(), ID.RandomIDInKeySpace);
                ((VirtualProtocol)contact.Protocol).Node = new Node(contact, new VirtualStorage());
                3200.ForEach(() => bucketList.AddContact(contact));
				int contacts = bucketList.Buckets.Sum(b => b.Contacts.Count);
				sb.Append(i + "," + contacts + CRLF);
			});

			File.WriteAllText("prefixTest.txt", sb.ToString());
		}

        /// <summary>
        /// Tests that a non-responding contact puts the new contact into a pending list.
        /// </summary>
        [TestMethod]
        public void NonRespondingContactDelayedEvictionTest()
        {
            // Create a DHT so we have an eviction handler.
            Dht dht = new Dht(ID.Zero, new VirtualProtocol(), () => null, new Router());
            //Contact dummyContact = new Contact(new VirtualProtocol(), ID.Zero);
            //((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());

            IBucketList bucketList = SetupSplitFailure(dht.Node.BucketList);

            Assert.IsTrue(bucketList.Buckets.Count == 2, "Bucket split should have occurred.");
            Assert.IsTrue(bucketList.Buckets[0].Contacts.Count == 1, "Expected 1 contact in bucket 0.");
            Assert.IsTrue(bucketList.Buckets[1].Contacts.Count == 20, "Expected 20 contacts in bucket 1.");

            // The bucket is now full.  Pick the first contact, as it is last seen (they are added in chronological order.)
            Contact nonRespondingContact = bucketList.Buckets[1].Contacts[0];

            // Since the protocols are shared, we need to assign a unique protocol for this node for testing.
            VirtualProtocol vpUnresponding = new VirtualProtocol(((VirtualProtocol)nonRespondingContact.Protocol).Node, false);
            nonRespondingContact.Protocol = vpUnresponding;

            // Setup the next new contact (it can respond.)
            Contact nextNewContact = new Contact(dht.Contact.Protocol, ID.Zero.SetBit(159));

            bucketList.AddContact(nextNewContact);

            Assert.IsTrue(bucketList.Buckets[1].Contacts.Count == 20, "Expected 20 contacts in bucket 1.");

            // Verify CanSplit -> Evict happened.

            Assert.IsTrue(dht.PendingContacts.Count == 1, "Expected one pending contact.");
            Assert.IsTrue(dht.PendingContacts.Contains(nextNewContact), "Expected pending contact to be the 21st contact.");
            Assert.IsTrue(dht.EvictionCount.Count == 1, "Expected one contact to be pending eviction.");
        }

        /// <summary>
        /// Tests that a non-responding contact is evicted after Constant.EVICTION_LIMIT tries.
        /// </summary>
        [TestMethod]
        public void NonRespondingContactEvictedTest()
        {
            // Create a DHT so we have an eviction handler.
            Dht dht = new Dht(ID.Zero, new VirtualProtocol(), () => null, new Router());
            //Contact dummyContact = new Contact(new VirtualProtocol(), ID.Zero);
            //((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());

            IBucketList bucketList = SetupSplitFailure(dht.Node.BucketList);

            Assert.IsTrue(bucketList.Buckets.Count == 2, "Bucket split should have occurred.");
            Assert.IsTrue(bucketList.Buckets[0].Contacts.Count == 1, "Expected 1 contact in bucket 0.");
            Assert.IsTrue(bucketList.Buckets[1].Contacts.Count == 20, "Expected 20 contacts in bucket 1.");

            // The bucket is now full.  Pick the first contact, as it is last seen (they are added in chronological order.)
            Contact nonRespondingContact = bucketList.Buckets[1].Contacts[0];

            // Since the protocols are shared, we need to assign a unique protocol for this node for testing.
            VirtualProtocol vpUnresponding = new VirtualProtocol(((VirtualProtocol)nonRespondingContact.Protocol).Node, false);
            nonRespondingContact.Protocol = vpUnresponding;

            // Setup the next new contact (it can respond.)
            Contact nextNewContact = new Contact(dht.Contact.Protocol, ID.Zero.SetBit(159));

            // Hit the non-responding contact EVICTION_LIMIT times, which will trigger the eviction algorithm.
            Constants.EVICTION_LIMIT.ForEach(() => bucketList.AddContact(nextNewContact));

            Assert.IsTrue(bucketList.Buckets[1].Contacts.Count == 20, "Expected 20 contacts in bucket 1.");

            // Verify CanSplit -> Pending eviction happened.

            Assert.IsTrue(dht.PendingContacts.Count == 0, "Pending contact list should now be empty.");
            Assert.IsFalse(bucketList.Buckets.SelectMany(b => b.Contacts).Contains(nonRespondingContact), "Expected bucket to NOT contain non-responding contact.");
            Assert.IsTrue(bucketList.Buckets.SelectMany(b => b.Contacts).Contains(nextNewContact), "Expected bucket to contain new contact.");
            Assert.IsTrue(dht.EvictionCount.Count == 0, "Expected no contacts to be pending eviction.");
        }

        /// <summary>
        /// Verify that we get stored values whose keys ^ contact ID are less than stored keys ^ other contacts.
        /// </summary>
        [TestMethod]
        public void TestNewContactGetsStoredContactsTest()
        {
            // Set up a node at the midpoint.
            // The existing node has the ID 10000....
            Node existing = new Node(new Contact(null, ID.Mid), new VirtualStorage());
            string val1 = "Value 1";
            string valMid = "Value Mid";

            // The existing node stores two items, one with an ID "hash" of 1, the other with ID.Max
            // Simple storage, rather than executing the code for Store.
            existing.SimpleStore(ID.One, val1);
            existing.SimpleStore(ID.Mid, valMid);

            Assert.IsTrue(existing.Storage.Keys.Count == 2, "Expected the existing node to have two key-values.");

            // Create a contact in the existing node's bucket list that is closer to one of the values.
            // This contact has the prefix 010000....
            Contact otherContact = new Contact(null, ID.Zero.SetBit(158));
            Node other = new Node(otherContact, new VirtualStorage());
            existing.BucketList.Buckets[0].Contacts.Add(otherContact);

            // The unseen contact has a prefix 0110000....
            VirtualProtocol unseenvp = new VirtualProtocol();
            Contact unseenContact = new Contact(unseenvp, ID.Zero.SetBit(157));
            Node unseen = new Node(unseenContact, new VirtualStorage());
            unseenvp.Node = unseen;     // final fixup.

            Assert.IsTrue(unseen.Storage.Keys.Count == 0, "The unseen node shouldn't have any key-values!");

            // An unseen node pings, and we should get back valMin only, as ID.One ^ ID.Mid < ID.Max ^ ID.Mid
            existing.Ping(unseenContact);

            // Contacts             V1          V2          
            // 10000000             00...0001   10...0000
            // 01000000

            // Math:
            // c1 ^ V1      c1 ^ V2      c2 ^ V1     c2 ^ V2     
            // 100...001    000...000    010...001   110...000

            // c1 ^ V1 > c1 ^ V2, so v1 doesn't get sent to the unseen node.
            // c1 ^ V2 < c2 ^ V2, so it does get sent.

            Assert.IsTrue(unseen.Storage.Keys.Count == 1, "Expected 1 value stored in our new node.");
            Assert.IsTrue(unseen.Storage.Contains(ID.Mid), "Expected valMid to be stored.");
            Assert.IsTrue(unseen.Storage.Get(ID.Mid) == valMid, "Expected valMid value to match.");
        }

        protected IBucketList SetupSplitFailure(IBucketList bucketList = null)
        {
            // force host node ID to < 2^159 so the node ID is not in the 2^159 ... 2^160 range
            byte[] bhostID = new byte[20];
            bhostID[19] = 0x7F;
            ID hostID = new ID(bhostID);

            Contact dummyContact = new Contact(new VirtualProtocol(), hostID);
            ((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());
            bucketList = bucketList ?? new BucketList(hostID, dummyContact);

            // Also add a contact in this 0 - 2^159 range, arbitrarily something not our host ID.
            // This ensures that only one bucket split will occur after 20 nodes with ID >= 2^159 are added,
            // otherwise, buckets will in the 2^159 ... 2^160 space.
            dummyContact = new Contact(new VirtualProtocol(), ID.One);
            ((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());
            bucketList.AddContact(new Contact(dummyContact.Protocol, ID.One));

            Assert.IsTrue(bucketList.Buckets.Count == 1, "Bucket split should not have occurred.");
            Assert.IsTrue(bucketList.Buckets[0].Contacts.Count == 1, "Expected 1 contact in bucket 0.");

            // make sure contact ID's all have the same 5 bit prefix and are in the 2^159 ... 2^160 - 1 space
            byte[] bcontactID = new byte[20];
            bcontactID[19] = 0x80;
            // 1000 xxxx prefix, xxxx starts at 1000 (8)
            // this ensures that all the contacts in a bucket match only the prefix as only the first 5 bits are shared.
            // |----| shared range
            // 1000 1000 ...
            // 1000 1100 ...
            // 1000 1110 ...
            byte shifter = 0x08;
            int pos = 19;

            Constants.K.ForEach(() =>
            {
                bcontactID[pos] |= shifter;
                ID contactID = new ID(bcontactID);
                dummyContact = new Contact(new VirtualProtocol(), ID.One);
                ((VirtualProtocol)dummyContact.Protocol).Node = new Node(dummyContact, new VirtualStorage());
                bucketList.AddContact(new Contact(dummyContact.Protocol, contactID));
                shifter >>= 1;

                if (shifter == 0)
                {
                    shifter = 0x80;
                    --pos;
                }
            });

            return bucketList;
        }
    }
}
