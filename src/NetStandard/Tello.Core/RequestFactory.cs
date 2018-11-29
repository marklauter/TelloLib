using System.Text;
using Tello.Udp;

namespace Tello.Core
{
    internal static class RequestFactory
    {
        public static Request GetRequest(Commands command)
        {
            byte[] datagram = null;
            switch (command)
            {
                default:
                case Commands.Connect:
                    datagram = Encoding.UTF8.GetBytes("conn_req:\x00\x00");
                    datagram[datagram.Length - 2] = 0x96;
                    datagram[datagram.Length - 1] = 0x17;
                    break;
                case Commands.TakeOff:
                    datagram = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x68, 0x54, 0x00, 0xe4, 0x01, 0xc2, 0x16 };
                    break;
                case Commands.Land:
                    datagram = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x55, 0x00, 0xe5, 0x01, 0x00, 0xba, 0xc7 };
                    break;
            }
            return new Request(datagram, false, false)
            {
                UserData = (int)command
            };
        }
    }
}
