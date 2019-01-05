using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        // 1k buffer stores 1k samples
        public VideoServer(int port = 11111, int bufferSize = 1024)
        {
            _samples = new RingBuffer<byte[]>(bufferSize);
            _udpReceiver = new UdpReceiver(port);
            _udpReceiver.DatagramReceived += _udpReceiver_DatagramReceived;
        }

        public event EventHandler<SampleReadyArgs> SampleReady;
        private readonly UdpReceiver _udpReceiver;
        private readonly RingBuffer<byte[]> _samples;

        public void Start()
        {
            _udpReceiver.Start();
        }

        public void Stop()
        {
            _udpReceiver.Stop();
        }

        private void _udpReceiver_DatagramReceived(object sender, DatagramReceivedArgs e)
        {
            _samples.Push(e.Datagram);
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

        public Task<byte[][]> GetSamplesAsync(int count, TimeSpan timeout)
        {
            return Task.Run(() =>
            {
                var samples = new byte[count][];
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                for (var i = 0; i < count; ++i)
                {
                    if (!TryGetSample(out samples[i], stopwatch.Elapsed - timeout) || stopwatch.Elapsed > timeout)
                    {
                        break;
                    }
                }
                return samples;
            });
        }
    }
}
