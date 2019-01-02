using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Tello.Video
{
    public class VideoServer
    {
        public event EventHandler DataReady;

        private bool _running = false;
        public async void Start(int port = 11111)
        {
            _tail = 0;
            _head = 0;
            _sampleBuffer = new byte[SIZE][];
            Debug.WriteLine($"video server starting on port: {port}");
            if (!_running)
            {
                _running = true;
                await Task.Run(() => { Listen(port); });
            }
        }
        public void Stop()
        {
            _running = false;
        }

        private const int SIZE = 1024 * 10;
        private byte[][] _sampleBuffer = new byte[SIZE][];
        private uint _tail = 0;
        internal void Listen(int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var client = new UdpClient(port))
            {
                Debug.WriteLine($"video server listening on port: {port}");
                while (_running)
                {
                    var datagram = client.Receive(ref endPoint);
                    _sampleBuffer[_tail] = datagram;
                    _tail = (_tail + 1) % SIZE;
                    Debug.Write(".");
                    DataReady?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private uint _head = 0;
        public byte[] GetSample()
        {
            if (!_running)
            {
                return null;
            }

            var result = _sampleBuffer[_head];
            _sampleBuffer[_head] = null;
            _head = (_head + 1) % SIZE;
            Debug.Write("-");
            return result;
        }
    }
}
