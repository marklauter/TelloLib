using System.Text;

namespace Tello.Core
{
    /// <summary>
    /// CRC extensions for datagram packets
    /// </summary>
    internal static class DatagramExtensions
    {
        

        public static byte[] GetDatagram(this Commands message)
        {
            switch (message)
            {
                default:
                case Commands.ConnectionRequest: return Encoding.UTF8.GetBytes("conn_req:\x00\x00");
                case Commands.TakeOff:  return new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x68, 0x54, 0x00, 0xe4, 0x01, 0xc2, 0x16 };
                case Commands.Land:     return new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x55, 0x00, 0xe5, 0x01, 0x00, 0xba, 0xc7 };
            }
        }
    }
}
