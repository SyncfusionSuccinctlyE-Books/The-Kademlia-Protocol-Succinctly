#define USE_TCP_SUBNET_PROTOCOL

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

using FlowSharpLib;
using FlowSharpServiceInterfaces;

using Clifton.Kademlia;
using Clifton.Kademlia.Common;
using Clifton.Kademlia.Protocols;

namespace KademliaDemo
{
    public partial class MainForm : Form
    {
        protected List<Dht> dhts;
        protected List<Dht> knownPeers;
        protected List<Rectangle> dhtPos;
        protected Random rnd = new Random(4);
        protected int peerBootstrappingIdx = 0;
        protected List<Peer2Peer> connections;
        protected List<Dht> firstContacts;
        protected List<Dht> bucketRefreshPeers = new List<Dht>();
        protected Dictionary<BigInteger, Color> peerColor;
        protected ID storeKey;
        protected Dht originatorDht;
        protected TcpSubnetServer server;

        // 60 peer network:
        //protected const int NUM_DHT = 60;
        //protected const int ITEMS_PER_ROW = 10;
        //protected const int XOFFSET = 30;
        //protected const int YOFFSET = 30;
        //protected const int SIZE = 30;
        //protected const int XSPACING = 60;
        //protected const int YSPACING = 80;
        //protected const int JITTER = 15;
        //protected const int NUM_KNOWN_PEERS = 5;

        // 25 peer network:
        protected const int NUM_DHT = 25;
        protected const int ITEMS_PER_ROW = 5;
        protected const int XOFFSET = 30;
        protected const int YOFFSET = 30;
        protected const int SIZE = 30;
        protected const int XSPACING = 100;
        protected const int YSPACING = 100;
        protected const int JITTER = 15;
        protected const int NUM_KNOWN_PEERS = 3;

        public MainForm()
        {
#if USE_TCP_SUBNET_PROTOCOL
            InitializeTcpSubnetServer();
#endif

            InitializeComponent();
            InitializeFlowSharp();
            InitializeDhts();
            InitializeKnownPeers();
            Shown += OnShown;
        }

        protected void InitializeFlowSharp()
        {
            var canvasService = Program.ServiceManager.Get<IFlowSharpCanvasService>();
            canvasService.CreateCanvas(pnlFlowSharp);
            canvasService.ActiveController.Canvas.EndInit();
            canvasService.ActiveController.Canvas.Invalidate();

            // Initialize Toolbox so we can drop shapes
            IFlowSharpToolboxService toolboxService = Program.ServiceManager.Get<IFlowSharpToolboxService>();

            // We don't display the toolbox, but we need a container.
            Panel pnlToolbox = new Panel();
            pnlToolbox.Visible = false;
            Controls.Add(pnlToolbox);

            toolboxService.CreateToolbox(pnlToolbox);
            toolboxService.InitializeToolbox();

            var mouseController = Program.ServiceManager.Get<IFlowSharpMouseControllerService>();
            mouseController.Initialize(canvasService.ActiveController);
            nudPeerNumber.Maximum = NUM_DHT - 1;
        }

        protected void InitializeTcpSubnetServer()
        {
            server = new TcpSubnetServer("http://127.0.0.1", 2720);
            server.Start();
        }

        protected void InitializeDhts()
        {
            dhts = new List<Dht>();
            dhtPos = new List<Rectangle>();
            peerColor = new Dictionary<BigInteger, Color>();

            NUM_DHT.ForEach((n) =>
            {

#if USE_TCP_SUBNET_PROTOCOL
                IProtocol protocol = new TcpSubnetProtocol("http://127.0.0.1", 2720, n);
#else
                IProtocol protocol = new VirtualProtocol();
#endif

                Dht dht = new Dht(ID.RandomID, protocol, () => new VirtualStorage(), new Router());
                peerColor[dht.ID.Value] = Color.Green;

#if USE_TCP_SUBNET_PROTOCOL
                server.RegisterProtocol(n, dht.Node);
#else
                ((VirtualProtocol)protocol).Node = dht.Node;
#endif

                dhts.Add(dht);
                dhtPos.Add(new Rectangle(XOFFSET + rnd.Next(-JITTER, JITTER) + (n % ITEMS_PER_ROW) * XSPACING, YOFFSET + rnd.Next(-JITTER, JITTER) + (n / ITEMS_PER_ROW) * YSPACING, SIZE, SIZE));
            });
        }

        protected void InitializeKnownPeers()
        {
            knownPeers = new List<Dht>();
            List<Dht> workingList = new List<Dht>(dhts);

            NUM_KNOWN_PEERS.ForEach(() =>
            {
                Dht knownPeer = workingList[rnd.Next(workingList.Count)];
                peerColor[knownPeer.ID.Value] = Color.Red;
                knownPeers.Add(knownPeer);
                workingList.Remove(knownPeer);
            });
        }

        private void OnShown(object sender, EventArgs e)
        {
            DrawDhts();
        }

        protected void DrawDhts()
        {
            WebSocketHelpers.ClearCanvas();
            Application.DoEvents();
            connections = new List<Peer2Peer>();

            if (!ckNodesTopmost.Checked)
            {
                dhtPos.ForEachWithIndex((p, i) => WebSocketHelpers.DropShape("Ellipse", i.ToString(), p, peerColor[dhts[i].ID.Value], ""));
            }

            Application.DoEvents();


            if (ckShowConnections.Checked)
            {
                // First the original connections...
                // dhts.Where(d=>!bucketRefreshPeers.Contains(d)).ForEachWithIndex((d, i) =>
                dhts.ForEachWithIndex((d, i) =>
                {
                    d.Node.BucketList.Buckets.SelectMany(b => b.Contacts).ForEach(c =>
                      {
                          int idx = dhts.FindIndex(target => target.ID == c.ID);
                          var otherDir = new Peer2Peer() { idx1 = idx, idx2 = i };

                      // Don't draw connector going back (idx -> i) because this is a redundant draw.  Speeds things up a little.
                      if (!connections.Contains(otherDir))
                          {
                              Point c1 = dhtPos[i].Center();
                              Point c2 = dhtPos[idx].Center();
                              WebSocketHelpers.DropConnector("DiagonalConnector", "c" + i, c1.X, c1.Y, c2.X, c2.Y, Color.Gray);
                              connections.Add(new Peer2Peer() { idx1 = i, idx2 = idx });
                              Application.DoEvents();
                          }
                      });
                });
            }

            if (ckNodesTopmost.Checked)
            {
                dhtPos.ForEachWithIndex((p, i) => WebSocketHelpers.DropShape("Ellipse", i.ToString(), p, peerColor[dhts[i].ID.Value], ""));
            }

            Application.DoEvents();

            /*
            // Then the bucket refresh connections:
            dhts.Where(d => bucketRefreshPeers.Contains(d)).ForEachWithIndex((d, i) =>
            {
                d.Node.BucketList.Buckets.SelectMany(b => b.Contacts).ForEach(c =>
                {
                    int idx = dhts.FindIndex(target => target.ID == c.ID);
                    var otherDir = new Peer2Peer() { idx1 = idx, idx2 = i };

                    // Don't draw connector going back (idx -> i) because this is a redundant draw.  Speeds things up a little.
                    if (!connections.Contains(otherDir))
                    {
                        Point c1 = dhtPos[i].Center();
                        Point c2 = dhtPos[idx].Center();
                        WebSocketHelpers.DropConnector("DiagonalConnector", "c" + i, c1.X, c1.Y, c2.X, c2.Y, Color.Purple);
                        connections.Add(new Peer2Peer() { idx1 = i, idx2 = idx });
                    }
                });
            });
            */
        }

        protected void BootstrapWithAPeer(int peerBootstrappingIdx)
        {
            Dht dht = dhts[peerBootstrappingIdx];
            var peerList = knownPeers.ExceptBy(dht, c => c.ID).ToList();
            Dht bootstrapWith = peerList[rnd.Next(peerList.Count)];
            dht.Bootstrap(bootstrapWith.Contact);
        }

        protected void EnableBucketRefresh()
        {
            btnStep.Enabled = false;
            btnRun.Enabled = false;
            btnBucketRefresh.Enabled = true;
        }

        protected void UpdatePeerColors()
        {
            foreach (Dht dht in dhts)
            {
                if (originatorDht == dht)
                {
                    peerColor[dht.ID.Value] = Color.Yellow;
                }
                else if (dht.RepublishStorage.Contains(storeKey))
                {
                    if (firstContacts.Contains(dht))
                    {
                        peerColor[dht.ID.Value] = Color.Blue;
                    }
                    else
                    {
                        peerColor[dht.ID.Value] = Color.Orange;
                    }
                }
                else if (knownPeers.Contains(dht))
                {
                    peerColor[dht.ID.Value] = Color.Red;
                }
                else
                {
                    peerColor[dht.ID.Value] = Color.Green;
                }
            }
        }

        private void btnStep_Click(object sender, EventArgs e)
        {
            if (peerBootstrappingIdx < dhts.Count)
            {
                BootstrapWithAPeer(peerBootstrappingIdx);
                ++peerBootstrappingIdx;
            }
            else
            {
                EnableBucketRefresh();
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            Enumerable.Range(peerBootstrappingIdx, dhts.Count - peerBootstrappingIdx).
                AsParallel().
                ForEach(n =>
            {
                BootstrapWithAPeer(n);
            });

            DrawDhts();
            EnableBucketRefresh();
        }

        /// <summary>
        /// Manually refresh all buckets and draw the new connections in purple.
        /// </summary>
        private void btnBucketRefresh_Click(object sender, EventArgs e)
        {
            dhts.AsParallel().ForEach(d => d.PerformBucketRefresh());
            System.Threading.Thread.Sleep(500);

            dhts.ForEachWithIndex((d, i) =>
            {
                d.Node.BucketList.Buckets.SelectMany(b => b.Contacts).ForEach(c =>
                {
                    int idx = dhts.FindIndex(target => target.ID == c.ID);
                    var current = new Peer2Peer() { idx1 = i, idx2 = idx };
                    var otherDir = new Peer2Peer() { idx1 = idx, idx2 = i };

                    // Don't draw connector going back (idx -> i) because this is a redundant draw.  Speeds things up a little.
                    if (!connections.Contains(otherDir) && !connections.Contains(current))
                    {
                        // bucketRefreshPeers.Add(d);
                        Point c1 = dhtPos[i].Center();
                        Point c2 = dhtPos[idx].Center();
                        WebSocketHelpers.DropConnector("DiagonalConnector", "c" + i, c1.X, c1.Y, c2.X, c2.Y, Color.Purple);
                        connections.Add(new Peer2Peer() { idx1 = i, idx2 = idx });
                    }
                });
            });
        }

        /// <summary>
        /// Color the originator with yellow
        /// the immediate peer we're storing the value to in blue
        /// and the peers to which the value is republished in orange:
        /// </summary>
        private void btnPublish_Click(object sender, EventArgs e)
        {
            firstContacts = new List<Dht>();
            storeKey = ID.RandomID;
            originatorDht = dhts[(int)nudPeerNumber.Value];
            originatorDht.Store(storeKey, "Test");
            System.Threading.Thread.Sleep(500);
            dhts.Where(d => d.RepublishStorage.Contains(storeKey)).ForEach(d => firstContacts.Add(d));
            UpdatePeerColors();
            DrawDhts();
        }

        private void btnRepublish_Click(object sender, EventArgs e)
        {
            dhts.ForEach(d => d.PerformStoreRepublish());
            System.Threading.Thread.Sleep(500);
            UpdatePeerColors();
            DrawDhts();
        }
    }

    public struct Peer2Peer
    {
        public int idx1;
        public int idx2;
    }
}
