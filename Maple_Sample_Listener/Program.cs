using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Maple_Sample_Listener
{
    class Program
    {
        const int MAPLE_SERVER_BROADCASTPORT = 17756;
        static void Main(string[] args)
        {
            UdpClient udpClient = new UdpClient(MAPLE_SERVER_BROADCASTPORT);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine(returnData);
            }
            
        }
    }
}
