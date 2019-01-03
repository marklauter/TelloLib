using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Tello.Udp
{
    public class Receiver 
    {
        public Receiver(int port) : base()
        {
            Port = port;
        }

        public event EventHandler<ReceiverDatagramArgs> DatagramReceived;

        public int Port { get; }

        private bool _receiving = false;
        public async void Start()
        {
            if (!_receiving)
            {
                _receiving = true;
                await Task.Run(() => { Listen(); });
            }
        }

        public void Stop()
        {
            _receiving = false;
        }

        private async void Listen()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var client = new UdpClient(Port))
            {
                while (_receiving)
                {
                    var receiveResult = await client.ReceiveAsync();
                    var eventArgs = new ReceiverDatagramArgs(receiveResult.Buffer, receiveResult.RemoteEndPoint);
                    DatagramReceived?.Invoke(this, eventArgs);
                    await Task.Yield();
                }
            }
        }
    }
}
