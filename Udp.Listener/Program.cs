using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Tello.Udp;

namespace Udp.Listener
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
            Console.WriteLine($"listening on port {port}");

            Listen2(port);
        }

        private static void Listen2(int port)
        {
            var receiver = new Receiver(port);
            receiver.DatagramReceived += Receiver_DatagramReceived;
            receiver.BeginReceiving();
            Console.WriteLine("press any key to stop");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.ReadKey();


            //while (true)
            //{
            //    Task.Yield();
            //}
        }

        private static void Receiver_DatagramReceived(object sender, ReceiverDatagramArgs e)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{DateTime.Now}: {e.Datagram.Length} bytes received from {e.RemoteEndpoint.Address}:{e.RemoteEndpoint.Port}");
            builder.AppendLine("----------------------");

            builder.AppendLine( Encoding.UTF8.GetString(e.Datagram));
            builder.AppendLine("----------------------");

            for (var i = 0; i < e.Datagram.Length; ++i)
            {
                if (i > 0 && i % 2 == 0)
                {
                    builder.Append(" ");
                }

                builder.Append(e.Datagram[i].ToString("X2"));
            }
            builder.AppendLine();
            builder.AppendLine("==============================================");
            Console.WriteLine(builder.ToString());
        }

        private static void Listen1(int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var client = new UdpClient(port))
            {
                while (true)
                {
                    var data = client.Receive(ref endPoint);

                    //var response = Encoding.ASCII.GetBytes("OK");
                    //client.Send(response, response.Length, endPoint);
                    var s = Encoding.UTF8.GetString(data);
                    Console.WriteLine(s);

                    var builder = new StringBuilder();
                    builder.AppendLine($"{DateTime.Now}: {data.Length} bytes received from {endPoint.Address}:{endPoint.Port}");
                    for (var i = 0; i < data.Length; ++i)
                    {
                        if (i > 0 && i % 2 == 0)
                        {
                            builder.Append(" ");
                        }

                        builder.Append(data[i].ToString("X2"));
                    }
                    builder.AppendLine();
                    builder.Append("----------------------");
                    Console.WriteLine(builder.ToString());
                }
            }
        }
    }
}
