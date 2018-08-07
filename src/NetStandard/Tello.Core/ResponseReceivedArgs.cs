using System;
using System.Net;

namespace Tello.Core
{
    public class ResponseReceivedArgs : UdpMessengerArgs
    {
        public ResponseReceivedArgs(IPEndPoint endpoint, Commands command, byte[] response, byte[] request, DateTime requestTime) : base(endpoint)
        {
            Response = response ?? throw new ArgumentNullException(nameof(response));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            RequestTime = requestTime;
            Command = command;
        }

        public byte[] Response { get; }
        public byte[] Request { get; }
        public DateTime RequestTime { get; }
        public Commands Command { get; }
    }
}
