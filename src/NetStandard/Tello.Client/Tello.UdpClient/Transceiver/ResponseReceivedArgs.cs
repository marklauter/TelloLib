using System;
using System.Net;

namespace Tello.Udp
{
    public class ResponseReceivedArgs : EventArgs
    {
        public ResponseReceivedArgs(IPEndPoint endPoint, Request request, Response response, DateTime requestTime)
        {
            Response = response ?? throw new ArgumentNullException(nameof(response));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            RequestTime = requestTime;
        }

        public IPEndPoint EndPoint { get; }
        public Request Request { get; }
        public Response Response { get; }
        public DateTime RequestTime { get; }
    }
}
