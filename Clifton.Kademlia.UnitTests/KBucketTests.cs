using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Kademlia;
using Clifton.Kademlia.Common;

namespace UnitTests2
{
    [TestClass]
    public class KBucketTests
    {
        [TestMethod, ExpectedException(typeof(TooManyContactsException))]
        public void TooManyContactsTest()
        {
            KBucket kbucket = new KBucket();

            // Add max # of contacts.
            Constants.K.ForEach(n => kbucket.AddContact(new Contact(null, new ID(n))));

            // Add one more.
            kbucket.AddContact(new Contact(null, new ID(21)));
        }
    }
}
