using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Clifton.Kademlia.Common;

namespace Clifton.Kademlia.Protocols
{
    public class TcpSubnetServer : Server
    {
        protected Dictionary<int, INode> subnets;

        protected Dictionary<string, Type> routePackets = new Dictionary<string, Type>
        {
            {"//Ping", typeof(PingSubnetRequest) },
            {"//Store", typeof(StoreSubnetRequest) },
            {"//FindNode", typeof(FindNodeSubnetRequest) },
            {"//FindValue", typeof(FindValueSubnetRequest) },
        };

        /// <summary>
        /// Instantiate the server, listening on the specified url and port.
        /// </summary>
        /// <param name="url">Of the form http://127.0.0.1 or https or domain name.  No trailing forward slash.</param>
        /// <param name="port">The port number.</param>
        public TcpSubnetServer(string url, int port) : base(url, port)
        {
            subnets = new Dictionary<int, INode>();
        }

        public void RegisterProtocol(int subnet, INode node)
        {
            subnets[subnet] = node;
        }

        protected override async void ProcessRequest(HttpListenerContext context)
        {
            string data = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();

            if (context.Request.HttpMethod == "POST")
            {
                Type requestType;
                string path = context.Request.RawUrl;
                // Remove "//"
                // Prefix our call with "Server" so that the method name is unambiguous.
                string methodName = "Server" + path.Substring(2);

                if (routePackets.TryGetValue(path, out requestType))
                {
                    CommonRequest commonRequest = JsonConvert.DeserializeObject<CommonRequest>(data);
                    int subnet = ((ITcpSubnet)JsonConvert.DeserializeObject(data, requestType)).Subnet;
                    INode node;

                    if (subnets.TryGetValue(subnet, out node))
                    {
                        await Task.Run(() => CommonRequestHandler(methodName, commonRequest, node, context));
                    }
                    else
                    {
                        SendErrorResponse(context, new ErrorResponse() { ErrorMessage = "Method not recognized." });
                    }
                }
                else
                {
                    SendErrorResponse(context, new ErrorResponse() { ErrorMessage = "Subnet node not found." });
                }

                context.Response.Close();
            }
        }
    }
}