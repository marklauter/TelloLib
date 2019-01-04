using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Tello.Udp
{
    public class UdpReceiver
    {
        public UdpReceiver(int port) : base()
        {
            Port = port;
        }

        public event EventHandler<DatagramReceivedArgs> OnDatagramReceived;

        public int Port { get; }
        public bool Active { get; private set; } = false;

        public void Start()
        {
            if (!Active)
            {
                Active = true;
                Listen();
            }
        }

        public void Stop()
        {
            Active = false;
        }

        private async void Listen()
        {
            await Task.Run(async () =>
            {
                using (var client = new UdpClient(Port))
                {
                    while (Active)
                    {
                        var receiveResult = await client.ReceiveAsync();
                        var eventArgs = new DatagramReceivedArgs(receiveResult.Buffer, receiveResult.RemoteEndPoint);
                        OnDatagramReceived?.Invoke(this, eventArgs);
                    }
                }
            });
        }
    }
}
