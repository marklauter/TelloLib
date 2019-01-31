using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Tello.Udp
{
    public sealed class UdpListener : IUdpListener
    {
        public UdpListener(int port) : base()
        {
            Port = port;
        }

        public event EventHandler<DatagramReceivedArgs> DatagramReceived;

        public int Port { get; }
        public bool IsActive { get; private set; } = false;

        public void Start()
        {
            if (!IsActive)
            {
                IsActive = true;
                Listen();
            }
        }

        public void Stop()
        {
            IsActive = false;
        }

        private async void Listen()
        {
            await Task.Run(async () =>
            {
                using (var client = new UdpClient(Port))
                {
                    while (IsActive)
                    {
                        var receiveResult = await client.ReceiveAsync();
                        var eventArgs = new DatagramReceivedArgs(receiveResult.Buffer, receiveResult.RemoteEndPoint);
                        DatagramReceived?.Invoke(this, eventArgs);
                        if(eventArgs.Reply != null)
                        {
                            await client.SendAsync(eventArgs.Reply, eventArgs.Reply.Length, eventArgs.RemoteEndpoint);
                        }
                    }
                }
            });
        }
    }
}
