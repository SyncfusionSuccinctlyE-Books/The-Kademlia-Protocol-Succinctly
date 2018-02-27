using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Clifton.Kademlia.Common;

namespace Clifton.Kademlia.Protocols
{
    public abstract class Server
    {
        protected HttpListener listener;
        protected string url;
        protected int port;
        protected bool running;

        public Server(string url, int port)
        {
            this.url = url;
            this.port = port;
        }

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add(url + ":" + port + "/");
            listener.Start();
            running = true;
            Task.Run(() => WaitForConnection());
        }

        public void Stop()
        {
            running = false;
            listener.Stop();
        }

        protected virtual void WaitForConnection()
        {
            while (running)
            {
                // Wait for a connection.  Return to caller while we wait.
                HttpListenerContext context = listener.GetContext();
                ProcessRequest(context);
            }
        }

        protected abstract void ProcessRequest(HttpListenerContext context);

        protected void CommonRequestHandler(string methodName, CommonRequest commonRequest, INode node, HttpListenerContext context)
        {
#if DEBUG       // For unit testing
            if (node.OurContact.Protocol is TcpSubnetProtocol)
            {
                if (!((TcpSubnetProtocol)node.OurContact.Protocol).Responds)
                {
                    // Exceeds 500ms timeout.
                    System.Threading.Thread.Sleep(1000);
                    context.Response.Close();
                    return;         // bail now.
                }
            }
#endif

            try
            {
                MethodInfo mi = node.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
                object response = mi.Invoke(node, new object[] { commonRequest });
                SendResponse(context, response);
            }
            catch (Exception ex)
            {
                SendErrorResponse(context, new ErrorResponse() { ErrorMessage = ex.Message });
            }
        }

        protected void SendResponse(HttpListenerContext context, object resp)
        {
            context.Response.StatusCode = 200;
            SendResponseInternal(context, resp);
        }

        protected void SendErrorResponse(HttpListenerContext context, ErrorResponse resp)
        {
            context.Response.StatusCode = 400;
            SendResponseInternal(context, resp);
        }

        private void SendResponseInternal(HttpListenerContext context, object resp)
        {
            context.Response.ContentType = "text/text";
            context.Response.ContentEncoding = Encoding.UTF8;
            byte[] byteData = JsonConvert.SerializeObject(resp).to_Utf8();

            context.Response.OutputStream.Write(byteData, 0, byteData.Length);
        }
    }
}
