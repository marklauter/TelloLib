using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

            Listen(port);
        }

        private static void Listen(int port)
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
