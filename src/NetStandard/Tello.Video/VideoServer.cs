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
            _udpReceiver = new UdpListener(port);
            _udpReceiver.DatagramReceived += _udpReceiver_DatagramReceived;

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }
            _samples = new RingBuffer<byte[]>(bufferSize);
        }

        #region fields
        public event EventHandler<SampleReadyArgs> SampleReady;
        private readonly UdpListener _udpReceiver;
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

        private readonly Stopwatch _sampleStopwatch = new Stopwatch();
        public bool TryGetSample(out byte[] sample, TimeSpan timeout)
        {
            var wait = new SpinWait();
            _sampleStopwatch.Restart();
            while (!_samples.TryPop(out sample) && _sampleStopwatch.Elapsed < timeout)
            {
                wait.SpinOnce();
            }
            return sample != null;
        }
    }
}
