using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Tello.Emulator.SDKV2
{
    internal sealed class VideoServer : UdpServer
    {
        private int _sampleIndex = 0;
        private readonly byte[] _videoData;
        private readonly Sample[] _sampleDefs;
        private TimeSpan _previousTimeIndex = TimeSpan.FromSeconds(0.0);

        public VideoServer(int port, byte[] videoData, Sample[] sampleDefs) : base(port)
        {
            _videoData = videoData;
            _sampleDefs = sampleDefs;
        }


        protected override Task<byte[]> GetDatagram()
        {
            var sampleDef = _sampleDefs[_sampleIndex];

            // sleep to simulate Tello's actual sample rate
            //await Task.Delay(TimeSpan.FromMilliseconds(1));
            Thread.Sleep(2);

            // get the video sample 
            var sample = new byte[sampleDef.Length];
            Array.Copy(_videoData, sampleDef.Offset, sample, 0, sampleDef.Length);

            // advance the sample index, wrap at the end
            _sampleIndex = (_sampleIndex + 1) % _sampleDefs.Length;

            return Task.FromResult(sample);
        }
    }
}
