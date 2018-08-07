using System;
using System.Net;

namespace Tello.Core
{
    public class UdpMessengerArgs
    {
        public UdpMessengerArgs(IPEndPoint endPoint) : base()
        {
            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
        }

        public IPEndPoint EndPoint { get; }
    }
}
