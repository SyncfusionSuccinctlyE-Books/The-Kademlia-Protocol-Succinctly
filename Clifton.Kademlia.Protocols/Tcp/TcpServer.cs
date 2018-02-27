using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Clifton.Kademlia.Common;

namespace Clifton.Kademlia.Protocols
{
    public class TcpServer : Server
    {
        protected INode node;

        protected Dictionary<string, Type> routePackets = new Dictionary<string, Type>
        {
            {"//Ping", typeof(PingRequest) },
            {"//Store", typeof(StoreRequest) },
            {"//FindNode", typeof(FindNodeRequest) },
            {"//FindValue", typeof(FindValueRequest) },
        };

        /// <summary>
        /// Instantiate the server, listening on the specified url and port.
        /// </summary>
        /// <param name="url">Of the form http://127.0.0.1 or https or domain name.  No trailing forward slash.</param>
        /// <param name="port">The port number.</param>
        public TcpServer(string url, int port) : base(url, port)
        {
        }

        public void RegisterProtocol(INode node)
        {
            this.node = node;
        }

        protected override async void ProcessRequest(HttpListenerContext context)
        {
            string data = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();

            if (context.Request.HttpMethod == "POST")
            {
                Type requestType;
                string path = context.Request.RawUrl;
                string methodName = "Server" + path.Substring(2);

                if (routePackets.TryGetValue(path, out requestType))
                {
                    CommonRequest commonRequest = JsonConvert.DeserializeObject<CommonRequest>(data);
                    await Task.Run(() => CommonRequestHandler(methodName, commonRequest, node, context));
                }
                else
                {
                    SendErrorResponse(context, new ErrorResponse() { ErrorMessage = "Method not recognized." });
                }
            }

            context.Response.Close();
        }
   }
}