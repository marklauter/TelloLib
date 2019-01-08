using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Tello.Emulator.SDKV2
{
    internal abstract class UdpServer
    {
        public UdpServer(int port)
        {
            _endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
        }

        private readonly IPEndPoint _endpoint;
        private bool _running = false;

        public void Start()
        {
            if (!_running)
            {
                _running = true;
                RunServer();
            }
        }

        public void Stop()
        {
            _running = false;
        }

        private async void RunServer()
        {
            await Task.Run(async () =>
            {
                var wait = new SpinWait();
                using (var client = new UdpClient())
                {
                    client.Connect(_endpoint);
                    while (_running)
                    {
                        var datagram = await GetDatagram();
                        await client.SendAsync(datagram, datagram.Length);
                        Debug.WriteLine("datagram sent");
                        wait.SpinOnce();
                    }
                }
            });
        }

        protected abstract Task<byte[]> GetDatagram();
    }
}
