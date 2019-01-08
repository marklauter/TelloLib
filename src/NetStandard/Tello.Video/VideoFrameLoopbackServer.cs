using System;
using Tello.Udp;

namespace Tello.Video
{
    public sealed class VideoFrameLoopbackServer
    {
        //public VideoFrameServer(double frameRate, int bitRate, TimeSpan bufferTime, int bytesPerSample = 1460, int port = 11111)
        public VideoFrameLoopbackServer(double frameRate, TimeSpan bufferTime, string host, int port = 11111)
        {
            _udpReceiver = new UdpLoopbackReceiver(host, port);
            _udpReceiver.DatagramReceived += _udpReceiver_DatagramReceived;

            //_frameComposer = new FrameComposer(frameRate, bitRate, bufferTime, bytesPerSample);
            _frameComposer = new FrameComposer2(frameRate, bufferTime);
            //_frameComposer = new FrameComposer3(frameRate);
            _frameComposer.FrameReady += _frameComposer_FrameReady;
        }

        //private readonly FrameComposer _frameComposer;
        private readonly FrameComposer2 _frameComposer;
        //private readonly FrameComposer3 _frameComposer;
        private readonly UdpLoopbackReceiver _udpReceiver;

        public event EventHandler<FrameReadyArgs> FrameReady;

        #region controls
        public void Start()
        {
            _frameComposer.Start();
            _udpReceiver.Start();
        }

        public void Pause()
        {
            _frameComposer.Pause();
        }

        public void Resume()
        {
            _frameComposer.Resume();
        }

        public void Stop()
        {
            _udpReceiver.Stop();
            _frameComposer.Stop();
        }
        #endregion

        private void _udpReceiver_DatagramReceived(object sender, DatagramReceivedArgs e)
        {
            _frameComposer.AddSample(e.Datagram);
        }

        private void _frameComposer_FrameReady(object sender, FrameReadyArgs e)
        {
            FrameReady?.Invoke(this, e);
        }

        public VideoFrameCollection GetSample(TimeSpan timeout)
        {
            return _frameComposer.GetFrames(timeout);
        }

        public bool TryReadFrame(out VideoFrame frame, TimeSpan timeout)
        {
            return _frameComposer.TryGetFrame(out frame, timeout);
        }
    }
}

