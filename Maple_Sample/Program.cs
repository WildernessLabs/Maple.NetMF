using System;
using Microsoft.SPOT;
using Maple;
using Netduino.Foundation.Network;
using System.Threading;

namespace Maple_Sample
{
    public class Program
    {
        public static void Main()
        {
            MapleServer server = new MapleServer();

            Initializer.NetworkConnected += (s, e) =>
            {
                // start maple server and send name broadcast address
                server.Start("my server", Initializer.CurrentNetworkInterface.IPAddress);
            };
            Initializer.InitializeNetwork();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
