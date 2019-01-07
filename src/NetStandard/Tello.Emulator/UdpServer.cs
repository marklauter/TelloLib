using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Tello.Emulator.SDKV2
{
    internal abstract class UdpServer
    {
        public UdpServer(int port)
        {
            _port = port;
        }

        private bool _running = false;
        private readonly int _port;

        public async void Start()
        {
            if (!_running)
            {
                _running = true;
                await Task.Run(() => { RunServer(); });
            }
        }

        public void Stop()
        {
            _running = false;
        }

        private void RunServer()
        {
            using (var client = new UdpClient())
            {
                client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port));
                while (_running)
                {
                    var datagram = GetDatagram();
                    client.Send(datagram, datagram.Length);
                }
            }
        }

        protected abstract byte[] GetDatagram();
    }
}
