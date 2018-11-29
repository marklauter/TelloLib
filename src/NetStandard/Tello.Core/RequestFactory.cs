using System.Text;
using Tello.Udp;

namespace Tello.Core
{
    internal static class RequestFactory
    {
        public static Request GetRequest(Commands command)
        {
            switch (command)
            {
                default:
                case Commands.ConnectionRequest:
                    return new Request(Encoding.UTF8.GetBytes("conn_req:\x96\x17"), false, false)
                    {
                        UserData = (int)command
                    };
                case Commands.TakeOff:
                    return new Request(new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x68, 0x54, 0x00, 0xe4, 0x01, 0xc2, 0x16 })
                    {
                        UserData = (int)command
                    };
                case Commands.Land:
                    return new Request(new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x55, 0x00, 0xe5, 0x01, 0x00, 0xba, 0xc7 })
                    {
                        UserData = (int)command
                    };

            }
        }
    }
}
