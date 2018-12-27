using System;
using System.Net;

namespace Tello.Udp
{
    public class ReceiverDatagramArgs : EventArgs
    {
        public ReceiverDatagramArgs(byte[] datagram, IPEndPoint remoteEndpoint)
        {
            Datagram = datagram;
            RemoteEndpoint = remoteEndpoint;
        }
        public byte[] Datagram { get; }
        public IPEndPoint RemoteEndpoint { get; }
    }
}
