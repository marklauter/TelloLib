using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Tello.Video
{
    public class VideoServer
    {
        public VideoServer()
        {
        }

        private readonly ReaderWriterLockSlim _gate = new ReaderWriterLockSlim();
        private bool _running = false;
        private bool Running
        {
            get
            {
                var running = false;
                _gate.EnterReadLock();
                try
                {
                    running = _running;
                }
                finally
                {
                    _gate.ExitReadLock();
                }
                return running;
            }
            set
            {
                _gate.EnterWriteLock();
                try
                {
                    _running = value;
                }
                finally
                {
                    _gate.ExitWriteLock();
                }
            }
        }

        public async void Start(int port = 11111)
        {
            if (!Running)
            {
                Running = true;
                await Task.Run(() => { Listen(port); });
            }
        }

        public void Stop()
        {
            Running = false;
        }

        private readonly ConcurrentQueue<byte[]> _samples = new ConcurrentQueue<byte[]>();

        internal void Listen(int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var client = new UdpClient(port))
            {
                while (Running)
                {
                    var datagram = client.Receive(ref endPoint);
                    _samples.Enqueue(datagram);
                }
            }
        }

        public byte[] GetSample()
        {
            if (_samples.TryDequeue(out var sample))
            {
                return sample;
            }
            return null;
        }
    }
}
