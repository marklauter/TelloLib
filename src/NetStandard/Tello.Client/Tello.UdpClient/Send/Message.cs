using System;

namespace Tello.Udp
{
    public abstract class Message
    {
        public Message(Guid id, byte[] datagram) : base()
        {
            if (datagram == null)
            {
                throw new ArgumentNullException(nameof(datagram));
            }

            if (datagram.Length == 0)
            {
                throw new ArgumentException(nameof(datagram));
            }

            Id = id;

            // making a deep copy so I can log the difference between the original datagram and the post CRC datagram
            // this is temporary until the app is working
            Datagram = new byte[datagram.Length];
            datagram.CopyTo(Datagram, 0);
        }

        public Guid Id { get; }
        public byte[] Datagram { get; }
    }
}
