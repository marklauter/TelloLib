using System;
using System.IO;
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

            //var port = Int32.Parse(args[0]);
            //            Console.WriteLine($"listening on port {port}");

            Listen2();
        }

        private static void Listen2()
        {
        //    var stateReceiver = new Receiver(8890);
        //    stateReceiver.DatagramReceived += StateReceiver_DatagramReceived;
        //    stateReceiver.BeginReceiving();
        //    Console.WriteLine($"listening on port 8890");

            var videoReceiver = new Receiver(11111);
            videoReceiver.DatagramReceived += VideoReceiver_DatagramReceived;
            videoReceiver.BeginReceiving();
            Console.WriteLine($"listening on port 11111");
            Console.WriteLine("==============================================");
            Console.WriteLine("press any key to stop");
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.ReadKey();
            //if (stream != null)
            //{
            //    stream.Close();
            //}

            //while (true)
            //{
            //    Task.Yield();
            //}
        }

        private static void StateReceiver_DatagramReceived(object sender, ReceiverDatagramArgs e)
        {
            Console.WriteLine($"TELLO STATE: {Encoding.UTF8.GetString(e.Datagram)}");
        }

        private static long _size = 0;
        //private static FileStream stream;
        private static void VideoReceiver_DatagramReceived(object sender, ReceiverDatagramArgs e)
        {
            //if (stream == null)
            //{
            //    stream = new FileStream("tello.mp4", FileMode.OpenOrCreate);
            //}
            //stream.Write(e.Datagram, 0, e.Datagram.Length);

            //var builder = new StringBuilder();
            _size += e.Datagram.Length;
            //builder.AppendLine($"{DateTime.Now}: {e.Datagram.Length} bytes received from {e.RemoteEndpoint.Address}:{e.RemoteEndpoint.Port}");
            //builder.AppendLine("----------------------");
            if (e.Datagram.Length != 1460)
            {
                //builder.AppendLine($"frame size: {_size}");
                Console.WriteLine($"frame size: {_size}");
                _size = 0;
            }


            //builder.AppendLine( Encoding.UTF8.GetString(e.Datagram));
            //builder.AppendLine("----------------------");

            //for (var i = 0; i < e.Datagram.Length; ++i)
            //{
            //    if (i > 0 && i % 2 == 0)
            //    {
            //        builder.Append(" ");
            //    }

            //    builder.Append(e.Datagram[i].ToString("X2"));
            //}
            //builder.AppendLine();
            //builder.AppendLine("==============================================");
            //Console.WriteLine(builder.ToString());
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
