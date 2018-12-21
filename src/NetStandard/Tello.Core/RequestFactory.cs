using System.Text;
using Tello.Udp;

namespace Tello.Core
{
    public static class RequestFactory
    {
        public static Request GetRequest(Commands command, params object[] args)
        {
            byte[] datagram = null;
            var setSeq = false;
            var setCrc = false;
            switch (command)
            {
                default:
                case Commands.Connect:
                    datagram = Encoding.UTF8.GetBytes("conn_req:\x00\x00");
                    datagram[datagram.Length - 2] = 0x96;
                    datagram[datagram.Length - 1] = 0x17;
                    break;
                case Commands.TakeOff:
                    setSeq = setCrc = true;
                    datagram = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x68, 0x54, 0x00, 0xe4, 0x01, 0xc2, 0x16 };
                    break;
                case Commands.ThrowTakeOff:
                    setSeq = setCrc = true;
                    datagram = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x48, 0x5d, 0x00, 0xe4, 0x01, 0xc2, 0x16 };
                    break;
                case Commands.Land:
                    setSeq = setCrc = true;
                    datagram = new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x55, 0x00, 0xe5, 0x01, 0x00, 0xba, 0xc7 };
                    break;
                case Commands.RequestIFrame:
                    datagram = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x60, 0x25, 0x00, 0x00, 0x00, 0x6c, 0x95 };
                    break;
                case Commands.SetMaxHeight:
                    setSeq = setCrc = true;
                    datagram = new byte[] { 0xcc, 0x68, 0x00, 0x27, 0x68, 0x58, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5b, 0xc5 };
                    var height = (int)args[0];
                    datagram[9] = (byte)(height & 0xFF);
                    datagram[10] = (byte)((height >> 8) & 0xFF);
                    break;
                case Commands.QueryUnk:
                    setSeq = setCrc = true;
                    datagram = new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x48, 0xff, 0x00, 0x06, 0x00, 0xe9, 0xb3 };
                    datagram[5] = (byte)args[0];
                    break;
            }
            return new Request(datagram, setSeq, setCrc)
            {
                UserData = (int)command
            };
        }
    }
}
