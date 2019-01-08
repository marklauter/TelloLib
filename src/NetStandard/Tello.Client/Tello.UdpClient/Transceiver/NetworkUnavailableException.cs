using System;
using System.Runtime.Serialization;

namespace Tello.Udp
{
    public class NetworkUnavailableException : Exception
    {
        public NetworkUnavailableException()
        {
        }

        public NetworkUnavailableException(string message) : base(message)
        {
        }

        public NetworkUnavailableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NetworkUnavailableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
