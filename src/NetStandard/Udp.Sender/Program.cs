using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Tello.Udp;

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
            TestTelloUdp2Tello();
            //TestTelloUdp(port);
            //TestRawUdp(port);
        }

        private static void TestTelloUdp2Tello()
        {
            Console.WriteLine($"sending on port {8889}");

            using (var client = new Client("192.168.10.1", 8889))
            {
                client.ResponseReceived += Tello_ResponseReceived;
                if (!client.Connect())
                {
                    Console.WriteLine("failed to connect to udp");
                    Console.ReadKey();
                    return;
                }

                while (true)
                {
                    Console.WriteLine("enter connect to tello or exit to quit:");
                    var message = Console.ReadLine();
                    if (message.ToLower() == "exit")
                    {
                        break;
                    }

                    var datagram = Encoding.UTF8.GetBytes("conn_req:\x00\x00");
                    datagram[datagram.Length - 2] = 0x96;
                    datagram[datagram.Length - 1] = 0x17;

                    var request = new Request(datagram, false, false)
                    {
                        UserData = 0
                    };
                    client.Send(request);
                }
            }
        }

        private static void Tello_ResponseReceived(object sender, ResponseReceivedArgs e)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{DateTime.Now} - {e.Request.Id}::{e.Response.Id} - {e.Response.Datagram.Length} bytes received from {e.EndPoint.Address}:{e.EndPoint.Port}");
            for (var i = 0; i < e.Response.Datagram.Length; ++i)
            {
                if (i > 0 && i % 2 == 0)
                {
                    builder.Append(" ");
                }

                builder.Append(e.Response.Datagram[i].ToString("X2"));
            }
            builder.AppendLine();
            builder.Append("----------------------");
            Console.WriteLine(builder.ToString());
        }

        private static void TestTelloUdp(int port)
        {
            Console.WriteLine($"sending on port {port}");

            using (var client = new Client("127.0.0.1", port))
            {
                client.ResponseReceived += Client_ResponseReceived;
                client.Connect();
                while (true)
                {
                    Console.WriteLine("enter message to send or exit to quit:");
                    var message = Console.ReadLine();
                    if (message.ToLower() == "exit")
                    {
                        break;
                    }

                    var datagram = Encoding.UTF8.GetBytes("conn_req:\x96\x17");
                    var request = new Request(datagram, false, false);
                    client.Send(request);
                }
            }
        }

        private static void Client_ResponseReceived(object sender, ResponseReceivedArgs e)
        {
            var message = Encoding.ASCII.GetString(e.Response.Datagram);
            Console.WriteLine($"{DateTime.Now} - {e.Request.Id}::{e.Response.Id} - {e.Response.Datagram.Length} bytes received: '{message}'");
        }

        private static void TestRawUdp(int port)
        {
            Console.WriteLine($"sending on port {port}");

            using (var client = new UdpClient())
            {
                while (true)
                {
                    Console.WriteLine("enter message to send or exit to quit:");
                    var message = Console.ReadLine();
                    if (message.ToLower() == "exit")
                    {
                        break;
                    }

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
