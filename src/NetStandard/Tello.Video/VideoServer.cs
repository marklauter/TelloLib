using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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

        private readonly ReaderWriterLockSlim _rungate = new ReaderWriterLockSlim();
        private bool _running = false;
        private bool Running
        {
            get
            {
                var running = false;
                _rungate.EnterReadLock();
                try
                {
                    running = _running;
                }
                finally
                {
                    _rungate.ExitReadLock();
                }
                return running;
            }
            set
            {
                _rungate.EnterWriteLock();
                try
                {
                    _running = value;
                }
                finally
                {
                    _rungate.ExitWriteLock();
                }
            }
        }

        private readonly ConcurrentQueue<byte[]> _samples = new ConcurrentQueue<byte[]>();


        public async void Start(int port = 11111)
        {
            Debug.WriteLine($"video server starting on port: {port}");
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


        internal void Listen(int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var client = new UdpClient(port))
            {
                var index = -1;
                var datagrams = new byte[24][];
                Debug.WriteLine($"video server listening on port: {port}");
                while (Running)
                {
                    datagrams[++index] = client.Receive(ref endPoint);
                    if (index == 23)
                    {
                        index = -1;
                        BuildSample(datagrams);
                        datagrams = new byte[24][];
                    }
                }
            }
        }

        internal async void BuildSample(byte[][] datagrams)
        {
            await Task.Run(() =>
            {
                var size = 0;
                for (var i = 0; i < datagrams.Length; ++i)
                {
                    size += datagrams[i].Length;
                }

                var buffer = new byte[size];
                var offset = 0;
                for (var i = 0; i < datagrams.Length; ++i)
                {
                    var datagram = datagrams[i];
                    Array.Copy(datagram, 0, buffer, offset, datagram.Length);
                    offset += datagram.Length;
                }

                _samples.Enqueue(buffer);
                Debug.WriteLine($"sample built: {_samples.Count}");
            });
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
