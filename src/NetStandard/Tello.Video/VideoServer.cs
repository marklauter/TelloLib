using System;
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

        private readonly byte[][] _samples = new byte[Int16.MaxValue][];
        private int _offset = 0;
        private int _head = 0;
        private int _tail = 0;

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
                Debug.WriteLine($"video server listening on port: {port}");
                while (Running)
                {
                    var datagram = client.Receive(ref endPoint);
                    Debug.WriteLine($"datagram received. len: {datagram.Length}, _tail: {_tail}");
                    _gate.EnterWriteLock();
                    try
                    {
                        _samples[_tail] = datagram;
                        // mod keeps the tail from exceeding max array index
                        _tail = (_tail + 1) % Int16.MaxValue;
                        
                    }
                    finally
                    {
                        _gate.ExitWriteLock();
                    }
                }
            }
        }

        public byte[] GetSample()
        {
            byte[] sample = null;
            _gate.EnterReadLock();
            try
            {
                sample = _samples[_head];
                _samples[_head] = null;
                _head = (_head + 1) % Int16.MaxValue;
            }
            finally
            {
                _gate.ExitReadLock();
            }

            Debug.WriteLine($"GetSample() returning sample {_head}");
            return sample;
        }
    }
}
