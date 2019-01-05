using System;
using System.Diagnostics;
using System.Threading;
using Tello.Udp;

namespace Tello.Video
{
    public class SampleReadyArgs
    {
        public SampleReadyArgs(byte[] sample)
        {
            Sample = sample;
        }

        public byte[] Sample { get; }
    }

    public sealed class VideoServer
    {
        public VideoServer(int port = 11111, int bufferSize = 1204)
        {
            _udpReceiver = new UdpReceiver(port);
            _udpReceiver.DatagramReceived += _udpReceiver_DatagramReceived;

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }
            _samples = new RingBuffer<byte[]>(bufferSize);
        }

        #region fields
        public event EventHandler<SampleReadyArgs> SampleReady;
        private readonly UdpReceiver _udpReceiver;
        private readonly RingBuffer<byte[]> _samples;
        #endregion

        #region controls
        public void Start()
        {
            _udpReceiver.Start();
        }

        public void Stop()
        {
            _udpReceiver.Stop();
        }
        #endregion

        private void _udpReceiver_DatagramReceived(object sender, DatagramReceivedArgs e)
        {
            if (_samples != null)
            {
                _samples.Push(e.Datagram);
            }

            SampleReady?.Invoke(this, new SampleReadyArgs(e.Datagram));
        }

        public bool TryGetSample(out byte[] sample, TimeSpan timeout)
        {
            var spin = new SpinWait();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!_samples.TryPop(out sample) && stopwatch.Elapsed < timeout)
            {
                spin.SpinOnce();
            }
            return sample != null;
        }
    }
}
