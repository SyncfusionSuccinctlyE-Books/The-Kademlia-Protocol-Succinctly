using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Kademlia;
using Clifton.Kademlia.Common;
using Clifton.Kademlia.Protocols;

namespace UnitTests2
{
    [TestClass]
    public class TcpSubnetTests
    {
        protected string localIP = "http://127.0.0.1";
        protected int port = 2720;
        protected TcpSubnetServer server;

        [TestInitialize]
        public void Initialize()
        {
            server = new TcpSubnetServer(localIP, port);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            server.Stop();
        }

        [TestMethod]
        public void PingRouteTest()
        {
            TcpSubnetProtocol p1 = new TcpSubnetProtocol(localIP, port, 1);
            TcpSubnetProtocol p2 = new TcpSubnetProtocol(localIP, port, 2);
            ID ourID = ID.RandomID;
            Contact c1 = new Contact(p1, ourID);
            Node n1 = new Node(c1, new VirtualStorage());
            Node n2 = new Node(new Contact(p2, ID.RandomID), new VirtualStorage());
            server.RegisterProtocol(p1.Subnet, n1);
            server.RegisterProtocol(p2.Subnet, n2);
            server.Start();

            p2.Ping(c1);
        }

        [TestMethod]
        public void StoreRouteTest()
        {
            TcpSubnetProtocol p1 = new TcpSubnetProtocol(localIP, port, 1);
            TcpSubnetProtocol p2 = new TcpSubnetProtocol(localIP, port, 2);
            ID ourID = ID.RandomID;
            Contact c1 = new Contact(p1, ourID);
            Node n1 = new Node(c1, new VirtualStorage());
            Node n2 = new Node(new Contact(p2, ID.RandomID), new VirtualStorage());
            server.RegisterProtocol(p1.Subnet, n1);
            server.RegisterProtocol(p2.Subnet, n2);
            server.Start();

            Contact sender = new Contact(p1, ID.RandomID);
            ID testID = ID.RandomID;
            string testValue = "Test";
            p2.Store(sender, testID, testValue);
            Assert.IsTrue(n2.Storage.Contains(testID), "Expected remote peer to have value.");
            Assert.IsTrue(n2.Storage.Get(testID) == testValue, "Expected remote peer to contain stored value.");
        }

        [TestMethod]
        public void FindNodesRouteTest()
        {
            TcpSubnetProtocol p1 = new TcpSubnetProtocol(localIP, port, 1);
            TcpSubnetProtocol p2 = new TcpSubnetProtocol(localIP, port, 2);
            ID ourID = ID.RandomID;
            Contact c1 = new Contact(p1, ourID);
            Node n1 = new Node(c1, new VirtualStorage());
            Node n2 = new Node(new Contact(p2, ID.RandomID), new VirtualStorage());

            // Node 2 knows about another contact, that isn't us (because we're excluded.)
            ID otherPeer = ID.RandomID;
            n2.BucketList.Buckets[0].Contacts.Add(new Contact(new TcpSubnetProtocol(localIP, port, 3), otherPeer));

            server.RegisterProtocol(p1.Subnet, n1);
            server.RegisterProtocol(p2.Subnet, n2);
            server.Start();

            ID id = ID.RandomID;
            List<Contact> ret = p2.FindNode(c1, id).contacts;

            Assert.IsTrue(ret.Count == 1, "Expected 1 contact.");
            Assert.IsTrue(ret[0].ID == otherPeer, "Expected contact to the other peer (not us).");
        }

        [TestMethod]
        public void FindValueRouteTest()
        {
            TcpSubnetProtocol p1 = new TcpSubnetProtocol(localIP, port, 1);
            TcpSubnetProtocol p2 = new TcpSubnetProtocol(localIP, port, 2);
            ID ourID = ID.RandomID;
            Contact c1 = new Contact(p1, ourID);
            Node n1 = new Node(c1, new VirtualStorage());
            Node n2 = new Node(new Contact(p2, ID.RandomID), new VirtualStorage());

            server.RegisterProtocol(p1.Subnet, n1);
            server.RegisterProtocol(p2.Subnet, n2);
            server.Start();

            ID testID = ID.RandomID;
            string testValue = "Test";
            p2.Store(c1, testID, testValue);

            Assert.IsTrue(n2.Storage.Contains(testID), "Expected remote peer to have value.");
            Assert.IsTrue(n2.Storage.Get(testID) == testValue, "Expected remote peer to contain stored value.");

            var ret = p2.FindValue(c1, testID);

            Assert.IsTrue(ret.contacts == null, "Expected to find value.");
            Assert.IsTrue(ret.val == testValue, "Value does not match expected value from peer.");
        }

        [TestMethod]
        public void UnresponsiveNodeTest()
        {
            TcpSubnetProtocol p1 = new TcpSubnetProtocol(localIP, port, 1);
            TcpSubnetProtocol p2 = new TcpSubnetProtocol(localIP, port, 2);
            p2.Responds = false;
            ID ourID = ID.RandomID;
            Contact c1 = new Contact(p1, ourID);
            Node n1 = new Node(c1, new VirtualStorage());
            Node n2 = new Node(new Contact(p2, ID.RandomID), new VirtualStorage());

            server.RegisterProtocol(p1.Subnet, n1);
            server.RegisterProtocol(p2.Subnet, n2);
            server.Start();

            ID testID = ID.RandomID;
            string testValue = "Test";
            RpcError error = p2.Store(c1, testID, testValue);

            Assert.IsTrue(error.TimeoutError, "Expected timeout.");
        }
    }
}
