using System;
using Microsoft.SPOT;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Text;

namespace Maple
{
    public class MapleServer
    {
        private HttpListener server;
        private Thread connection;
        private Thread broadcast;

        /// <param name="prefix">http or https</param>
        public MapleServer(string prefix, int port)
        {
            server = new HttpListener(prefix, port);
            server.Start();
        }

        public void Start()
        {
            connection = new Thread(Context);
            connection.Start();
        }

        public MapleServer() : this("http", -1) { }

        protected void Context()
        {
            // Get classes that implement IRequestHandler
            var type = typeof(IRequestHandler);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var handlers = new ArrayList();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var t in types)
                {
                    if (t.BaseType != null)
                    {
                        var interfaces = t.BaseType.GetInterfaces();
                        if (interfaces.Length > 0)
                        {
                            foreach (var inter in interfaces)
                            {
                                if (inter == typeof(IRequestHandler))
                                {
                                    handlers.Add(t);
                                }
                            }
                        }
                    }
                }
            }

            while (true)
            {
                try
                {

                    HttpListenerContext context = server.GetContext();
                    string[] urlQuery = context.Request.RawUrl.Substring(1).Split('?');
                    string[] urlParams = urlQuery[0].Split('/');
                    string methodName = context.Request.HttpMethod + urlParams[0];

                    Debug.Print("Received " + context.Request.HttpMethod + " " + context.Request.RawUrl + " - Invoking " + methodName);

                    // convention for method is "{http method}{method name}"
                    // would love to convert this to two attributes: HttpGet, Mapping/Route
                    bool wasMethodFound = false;

                    foreach (var handler in handlers)
                    {
                        var methods = ((Type)handler).GetMethods();
                        foreach (var method in methods)
                        {
                            if (method.Name.ToLower() == methodName.ToLower())
                            {
                                var h = ((Type)handler).GetConstructor(new Type[] { typeof(HttpListenerContext) }).Invoke(new object[] { context });
                                try
                                {
                                    method.Invoke(h, null);
                                }
                                catch (Exception ex)
                                {
                                    context.Response.StatusCode = 500;
                                    context.Response.Close();
                                }
                                wasMethodFound = true;
                                break;
                            }
                        }
                        if (wasMethodFound) break;
                    }

                    if (!wasMethodFound)
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                    }
                }
                catch (SocketException e)
                {
                    Debug.Print("Socked Exception: " + e.ToString());
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }

        public void Broadcast(string message, int port)
        {
            Broadcast(message, port, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="port"></param>
        /// <param name="interval">in milliseconds</param>
        public void Broadcast(string message, int port, int interval)
        {
            broadcast = new Thread(delegate () { BroadcastWorker(message, port, interval); });
            broadcast.Start();
        }

        protected void BroadcastWorker(string message, int port, int interval)
        {
            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Any, port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                var payload = Encoding.UTF8.GetBytes(message);

                while (true)
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    socket.Bind(broadcastEndPoint);
                    socket.SendTo(payload, broadcastEndPoint);
                    socket.Close();
                    Debug.Print(message + " broadcasted");

                    if (interval > 0)
                    {
                        Thread.Sleep(interval);
                        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                socket.Close();
            }
        }
    }
}