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
            Initializer.InitializeNetwork();

            // wait for network to initialize
            while (Initializer.CurrentNetworkInterface == null) { }

            // start maple server and send name broadcast address
            MapleServer server = new MapleServer();
            server.Start("my server", Initializer.CurrentNetworkInterface.IPAddress);
            
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
