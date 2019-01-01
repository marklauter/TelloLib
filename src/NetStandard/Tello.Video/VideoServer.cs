using System;
using System.Diagnostics;
using System.IO;
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

        //private readonly ConcurrentQueue<byte[]> _samples = new ConcurrentQueue<byte[]>();

        private bool _dataReady = false;
        public event EventHandler DataReady;

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

        //private readonly ReaderWriterLockSlim _memgate = new ReaderWriterLockSlim();
        private object _gate = new object();
        private MemoryStream _stream = new MemoryStream();

        private int _index = 0;
        //private readonly MemoryStream[] _samples = new MemoryStream[64];

        internal void Listen(int port)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var client = new UdpClient(port))
            {
                Debug.WriteLine($"video server listening on port: {port}");

                while (Running)
                {
                    var datagram = client.Receive(ref endPoint);
                    if (!_dataReady)
                    {
                        Debug.WriteLine("data ready");
                        _dataReady = true;
                        DataReady?.Invoke(this, EventArgs.Empty);
                    }
                    _memgate.EnterReadLock();
                    var stream = _samples[_index];
                    _memgate.ExitReadLock();

                    if (stream == null)
                    {
                        stream = CreateNewBufferStream();
                    }

                    stream.Write(datagram, 0, datagram.Length);
                }
            }
        }

        //internal async void BuildSample(byte[][] datagrams)
        //{
        //    await Task.Run(() =>
        //    {
        //        var size = 0;
        //        for (var i = 0; i < datagrams.Length; ++i)
        //        {
        //            size += datagrams[i].Length;
        //        }

        //        var buffer = new byte[size];
        //        var offset = 0;
        //        for (var i = 0; i < datagrams.Length; ++i)
        //        {
        //            var datagram = datagrams[i];
        //            Array.Copy(datagram, 0, buffer, offset, datagram.Length);
        //            offset += datagram.Length;
        //        }

        //        _samples.Enqueue(buffer);
        //        Debug.WriteLine($"sample built: {_samples.Count}");
        //    });
        //}

        public MemoryStream GetSample()
        {
            _memgate.EnterReadLock();
            var index = _index;
            var stream = _samples[_index];
            _memgate.ExitReadLock();

            _memgate.EnterWriteLock();
            _samples[_index] = null;
            _index = (_index + 1) % _samples.Length;
            _memgate.ExitWriteLock();

            if (stream != null)
            {
                stream.Position = 0;
            }

            return stream;
        }

        private MemoryStream CreateNewBufferStream()
        {
            var stream = new MemoryStream(1460 * 24);
            _memgate.EnterWriteLock();
            _samples[_index] = stream;
            _memgate.ExitWriteLock();
            return stream;
        }
    }
}
