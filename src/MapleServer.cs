using System;
using Microsoft.SPOT;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Text;

namespace Maple
{
    public partial class MapleServer
    {
        private const int MAPLE_SERVER_BROADCASTPORT = 17756;

        private HttpListener server;
        private Thread connection;
        private ArrayList handlers;

        /// <param name="prefix">http or https</param>
        public MapleServer(string prefix, int port)
        {
            handlers = new ArrayList();
            server = new HttpListener(prefix, port);
            server.Start();
        }

        public void Start()
        {
            // spin up server
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the server to be broadcast over UDP</param>
        /// <param name="ipaddress">IP Address of the server to be broadcast over UDP</param>
        /// <param name="interval">Interval to broadcast in millis</param>
        public void Start(string name, string ipaddress, int interval=5000)
        {
            // start up broadcast on separate thread
            new Thread(new ThreadStart(delegate { Broadcast(name + "=" + ipaddress, MAPLE_SERVER_BROADCASTPORT, interval); })).Start();

            // spin up server
            Init();
        }

        public void Stop()
        {
            if (connection.IsAlive)
            {
                connection.Abort();
            }
        }

        public void AddHandler(IRequestHandler handler)
        {
            this.handlers.Add(handler);
        }

        public void RemoveHandler(IRequestHandler handler)
        {
            this.handlers.Remove(handler);
        }

        public MapleServer() : this("http", -1) { }

        protected void Init()
        {
            ThreadStart starter = delegate { Context(handlers); };
            connection = new Thread(starter);
            connection.Start();
        }

        /// <param name="data">string to be broadcast over UDP</param>
        /// <param name="interval">millis</param>
        protected void Broadcast(string data, int broadcastPort, int broadcastInterval)
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), broadcastPort);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

                while (true)
                {
                    socket.SendTo(UTF8Encoding.UTF8.GetBytes(data), remoteEndPoint);
                    Debug.Print("UDP Broadcast: " + data + ", port: " + broadcastPort);
                    Thread.Sleep(broadcastInterval);
                }
            }
        }

        protected void Context(ArrayList requestHandlers)
        {
            if (requestHandlers.Count == 0)
            {
                // Get classes that implement IRequestHandler
                var type = typeof(IRequestHandler);
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

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
                                        requestHandlers.Add(t);
                                    }
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

                    foreach (var handler in requestHandlers)
                    {
                        Type handlerType = handler is Type ? (Type)handler : handler.GetType();
                        var methods = handlerType.GetMethods();
                        foreach (var method in methods)
                        {
                            if (method.Name.ToLower() == methodName.ToLower())
                            {
                                object target = handler;
                                if (handler is Type)
                                {
                                    target = ((Type)handler).GetConstructor(new Type[] { }).Invoke(new object[] { });
                                }
                                try
                                {
                                    ((IRequestHandler)target).Context = context;
                                    method.Invoke(target, null);
                                }
                                catch (Exception ex)
                                {
                                    Debug.Print(ex.Message);
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
    }
}

// nuget pack Maple.csproj -Prop Configuration=Release -Prop Platform=AnyCPU