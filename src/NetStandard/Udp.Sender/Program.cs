using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tello.Core;
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
            TestTelloUdp2TelloTxt();
            //TestTelloUdp2Tello();
            //TestTelloUdp(port);
            //TestRawUdp(port);
        }

        //https://dl-cdn.ryzerobotics.com/downloads/Tello/Tello%20SDK%202.0%20User%20Guide.pdf

        private static bool _video = false;
        private static void TestTelloUdp2TelloTxt()
        {
            // real tello
            //using (var client = new Transceiver("192.168.10.1", 8889))
            // emulated tello
            using (var client = new UdpTransceiver("127.0.0.1", 8889))
            {
                client.ResponseReceived += Tello_ResponseReceivedTxt;
                try
                {
                    client.Connect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"failed to connect to udp. ex: {ex}");
                }

                Console.WriteLine("");
                Console.WriteLine("=================================");
                Console.WriteLine($"sending on port {8889}");
                Console.WriteLine("commands (not case sensitive): ");
                Console.WriteLine("connect to tello: C");
                Console.WriteLine("disconnect: D");
                Console.WriteLine("take off: T");
                Console.WriteLine("land: L");
                Console.WriteLine("forward: F");
                Console.WriteLine("backward: B");
                Console.WriteLine("toggle video: V");
                Console.WriteLine("get battery: P");
                Console.WriteLine("quit: Q");
                Console.WriteLine("custom command: K");
                Console.WriteLine("=================================");

                while (true)
                {
                    var message = Console.ReadKey();
                    if (message.Key.ToString().ToLower() == "q")
                    {
                        break;
                    }
                    if(message.Key.ToString().ToLower() == "k")
                    {
                        Console.WriteLine("");
                        Console.Write("enter custom command: ");
                        var command = Console.ReadLine();
                        if (!string.IsNullOrEmpty(command))
                        {
                            var request = new Request(Encoding.ASCII.GetBytes(command), false, false)
                            {
                                UserData = 0
                            };
                            client.Send(request);
                        }
                    }
                    if (message.Key.ToString().ToLower() == "d")
                    {
                        client.Disconnect();
                    }
                    if (message.Key.ToString().ToLower() == "c")
                    {
                        if (!client.IsConnected)
                        {
                            try
                            {
                                client.Connect();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"failed to connect to udp. ex: {ex}");
                            }
                        }

                        //var datagram = Encoding.UTF8.GetBytes("conn_req:\x00\x00");
                        //datagram[datagram.Length - 2] = 0x96;
                        //datagram[datagram.Length - 1] = 0x17;

                        var request = new Request(Encoding.ASCII.GetBytes("command"), false, false)
                        {
                            UserData = 0
                        };
                        client.Send(request);

                        Task.Delay(1000 * 10);
                        request = new Request(Encoding.ASCII.GetBytes("battery?"), false, false)
                        {
                            UserData = 6
                        };
                        client.Send(request);
                    }
                    if (message.Key.ToString().ToLower() == "t")
                    {
                        //var request = RequestFactory.GetRequest(Commands.TakeOff);
                        //var request = new Request(new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x68, 0x54, 0x00, 0xe4, 0x01, 0xc2, 0x16 })
                        //{
                        //    UserData = 1
                        //};
                        var request = new Request(Encoding.ASCII.GetBytes("takeoff"), false, false)
                        {
                            UserData = 1
                        };
                        client.Send(request);
                    }
                    if (message.Key.ToString().ToLower() == "l")
                    {
                        //var request = new Request(new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x55, 0x00, 0xe5, 0x01, 0x00, 0xba, 0xc7 })
                        //{
                        //    UserData = 2
                        //};
                        //client.Send(request);
                        var request = new Request(Encoding.ASCII.GetBytes("land"), false, false)
                        {
                            UserData = 2
                        };
                        client.Send(request);
                    }
                    if (message.Key.ToString().ToLower() == "f")
                    {
                        //var request = new Request(new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x55, 0x00, 0xe5, 0x01, 0x00, 0xba, 0xc7 })
                        //{
                        //    UserData = 2
                        //};
                        //client.Send(request);
                        var request = new Request(Encoding.ASCII.GetBytes("forward 100"), false, false)
                        {
                            UserData = 3
                        };
                        client.Send(request);
                    }
                    if (message.Key.ToString().ToLower() == "b")
                    {
                        //var request = new Request(new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x55, 0x00, 0xe5, 0x01, 0x00, 0xba, 0xc7 })
                        //{
                        //    UserData = 2
                        //};
                        //client.Send(request);
                        var request = new Request(Encoding.ASCII.GetBytes("back 100"), false, false)
                        {
                            UserData = 4
                        };
                        client.Send(request);
                    }
                    if (message.Key.ToString().ToLower() == "v")
                    {
                        //var request = RequestFactory.GetRequest(Commands.TakeOff);
                        //var request = new Request(new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x68, 0x54, 0x00, 0xe4, 0x01, 0xc2, 0x16 })
                        //{
                        //    UserData = 1
                        //};
                        if (!_video)
                        {
                            Console.WriteLine("enabling video capture");
                            var request = new Request(Encoding.ASCII.GetBytes("streamon"), false, false)
                            {
                                UserData = 5
                            };
                            client.Send(request);
                            _video = true;
                        }
                        else
                        {
                            Console.WriteLine("disabling video capture");
                            var request = new Request(Encoding.ASCII.GetBytes("streamoff"), false, false)
                            {
                                UserData = 5
                            };
                            client.Send(request);
                            _video = false;
                        }
                    }
                    if (message.Key.ToString().ToLower() == "p")
                    {
                        //var request = RequestFactory.GetRequest(Commands.TakeOff);
                        //var request = new Request(new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x68, 0x54, 0x00, 0xe4, 0x01, 0xc2, 0x16 })
                        //{
                        //    UserData = 1
                        //};
                        var request = new Request(Encoding.ASCII.GetBytes("battery?"), false, false)
                        {
                            UserData = 6
                        };
                        client.Send(request);
                    }

                    //if (message.ToLower() == "s")
                    //{
                    //    var request = RequestFactory.GetRequest(Commands.RequestIFrame);
                    //    request.UserData = 5;
                    //    client.Send(request);
                    //}
                }
            }
        }

        private static void Tello_ResponseReceivedTxt(object sender, ResponseReceivedArgs e)
        {
            //var builder = new StringBuilder();
            //builder.AppendLine();
            //builder.AppendLine("RESPONSE RECEIVED");
            //builder.AppendLine($"{DateTime.Now} - {e.Request.Id}::{e.Response.Id} - {e.Response.Datagram.Length} bytes received from {e.EndPoint.Address}:{e.EndPoint.Port}");
            //builder.AppendLine($"{DateTime.Now} - {e.Response.Datagram.Length} bytes received from {e.EndPoint.Address}:{e.EndPoint.Port}");
            //for (var i = 0; i < e.Response.Datagram.Length; ++i)
            //{
            //    if (i > 0 && i % 2 == 0)
            //    {
            //        builder.Append(" ");
            //    }

            //    builder.Append(e.Response.Datagram[i].ToString("X2"));
            //}
            //builder.AppendLine();
            //builder.AppendLine("----------------------");
            
            var sent = Encoding.UTF8.GetString(e.Request.Datagram);
            var received = Encoding.UTF8.GetString(e.Response.Datagram);
            Console.WriteLine($"MESSAGE SENT: {sent}, MESSAGE RECEIVED: {received}");

            //builder.AppendLine("message");
            //builder.AppendLine(message);
            //Console.WriteLine(builder.ToString());

            Console.WriteLine("");
            Console.WriteLine("=================================");
            Console.WriteLine($"sending on port {8889}");
            Console.WriteLine("commands (not case sensitive): ");
            Console.WriteLine("connect to tello: C");
            Console.WriteLine("disconnect: D");
            Console.WriteLine("take off: T");
            Console.WriteLine("land: L");
            Console.WriteLine("forward: F");
            Console.WriteLine("backward: B");
            Console.WriteLine("toggle video: V");
            Console.WriteLine("get battery: P");
            Console.WriteLine("quit: Q");
            Console.WriteLine("custom command: K");
            Console.WriteLine("=================================");
            Console.WriteLine("");

            //var bytes = e.Response.Datagram;
            //var cmdId = (bytes[5] | (bytes[6] << 8));
            //Console.WriteLine($"command id: {cmdId}");
            //var state = new FlyData();
            //if (cmdId == 86)//state command
            //{
            //    //update
            //    state.Update(bytes.Skip(9).ToArray());
            //    Console.WriteLine("--------------------------------");
            //    Console.WriteLine($"BATT: {state.batteryPercentage}%");
            //    Console.WriteLine();
            //}
        }

        private static void TestTelloUdp2Tello()
        {
            Console.WriteLine($"sending on port {8889}");

            using (var client = new UdpTransceiver("192.168.10.1", 8889))
            {
                client.ResponseReceived += Tello_ResponseReceived;
                try
                {
                    client.Connect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"failed to connect to udp. ex: {ex}");
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

                    if (message.ToLower() == "c")
                    {
                        var datagram = Encoding.UTF8.GetBytes("conn_req:\x00\x00");
                        datagram[datagram.Length - 2] = 0x96;
                        datagram[datagram.Length - 1] = 0x17;

                        var request = new Request(datagram, false, false)
                        {
                            UserData = 0
                        };
                        client.Send(request);
                    }
                    if (message.ToLower() == "t")
                    {
                        var request = RequestFactory.GetRequest(Commands.TakeOff);
                        //var request = new Request(new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x68, 0x54, 0x00, 0xe4, 0x01, 0xc2, 0x16 })
                        //{
                        //    UserData = 1
                        //};
                        client.Send(request);
                    }
                    if (message.ToLower() == "l")
                    {
                        var request = new Request(new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x55, 0x00, 0xe5, 0x01, 0x00, 0xba, 0xc7 })
                        {
                            UserData = 2
                        };
                        client.Send(request);
                    }
                    if (message.ToLower() == "s")
                    {
                        var request = RequestFactory.GetRequest(Commands.RequestIFrame);
                        request.UserData = 3;
                        client.Send(request);
                    }
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

            var message = Encoding.UTF8.GetString(e.Response.Datagram);
            Console.WriteLine($"message: {message}");

            var bytes = e.Response.Datagram;
            var cmdId = (bytes[5] | (bytes[6] << 8));
            Console.WriteLine($"command id: {cmdId}");
            var state = new FlyData();
            if (cmdId == 86)//state command
            {
                //update
                state.Update(bytes.Skip(9).ToArray());
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"BATT: {state.batteryPercentage}%");
                Console.WriteLine();
            }
        }

        private static void TestTelloUdp(int port)
        {
            Console.WriteLine($"sending on port {port}");

            using (var client = new UdpTransceiver("127.0.0.1", port))
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
