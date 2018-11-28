using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Udp.Sender
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("port is required arg");
                return;
            }

            var port = Int32.Parse(args[0]);
            Console.WriteLine($"sending on port {port}");
            SendUdp(port);
        }

        private static void SendUdp(int port)
        {
            using (var client = new UdpClient())
            {
                while (true)
                {
                    Console.WriteLine("enter message to send or exit to quit:");
                    var message = Console.ReadLine();
                    if (message.ToLower() == "exit")
                        break;

                    var datagram = Encoding.ASCII.GetBytes(message);
                    client.Send(datagram, message.Length, "127.0.0.1", port);
                    client.BeginReceive(OnReceive, client);
                }
            }
        }

        private static void OnReceive(IAsyncResult ar)
        {
            var client = ar.AsyncState as UdpClient;
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            var response = client.EndReceive(ar, ref endpoint);
            var message = Encoding.ASCII.GetString(response);
            Console.WriteLine($"{DateTime.Now}: {response.Length} bytes received: '{message}'");
        }
    }
}
