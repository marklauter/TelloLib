using System;

namespace Tello.Udp
{
    public class Response : Message
    {
        public Response(Guid id, byte[] datagram) : base(id, datagram) { }
    }
}
