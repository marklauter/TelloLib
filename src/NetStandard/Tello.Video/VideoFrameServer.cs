using System;
using System.Diagnostics;
using Tello.Udp;
using Windows.Media.Core;

namespace Tello.Video
{
    public sealed class VideoFrameServer
    {
        public VideoFrameServer(double frameRate, int bitRate, TimeSpan bufferTime, int bytesPerSample = 1460, int port = 11111)
        {
            _udpReceiver = new UdpReceiver(port);
            _udpReceiver.DatagramReceived += _udpReceiver_DatagramReceived;

            //_frameComposer = new FrameComposer(frameRate, bitRate, bufferTime, bytesPerSample);
            //_frameComposer = new FrameComposer2(frameRate, bufferTime);
            _frameComposer = new FrameComposer3(frameRate);
            _frameComposer.FrameReady += _frameComposer_FrameReady;
        }

        private readonly FrameComposer3 _frameComposer;
        private readonly UdpReceiver _udpReceiver;

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

        //public bool TryReadFrame(out VideoFrame frame, TimeSpan timeout)
        //{
        //    return _frameComposer.TryGetFrame(out frame, timeout);
        //}

        //public VideoFrameCollection ReadFrames(MediaStreamSourceSampleRequest request, TimeSpan timeout, int maxFrameCount)
        //{
        //    var stopwatch = Stopwatch.StartNew();
        //    var collection = new VideoFrameCollection();

        //    while (collection.Count < maxFrameCount && stopwatch.Elapsed < timeout && _frameComposer.TryGetFrame(out var frame, timeout))
        //    {
        //        collection.Add(frame);
        //        var progress = (uint)(collection.Count / maxFrameCount * 100);
        //        request.ReportSampleProgress(progress);
        //    }

        //    if (stopwatch.ElapsedMilliseconds > collection.Duration.TotalMilliseconds)
        //    {
        //        Debug.WriteLine($"ReadFrames: took too long! process duration: {stopwatch.ElapsedMilliseconds}ms, sample duration: {collection.Duration.TotalMilliseconds}ms");
        //    }

        //    return collection;
        //}

        //public VideoFrameCollection ReadAllFrames()
        //{
        //    var stopwatch = Stopwatch.StartNew();
        //    var frames = _frameComposer.FlushBuffer();
        //    if (frames != null && frames.Length > 0)
        //    {
        //        var collection = new VideoFrameCollection(frames);
        //        if (stopwatch.ElapsedMilliseconds > collection.Duration.TotalMilliseconds)
        //        {
        //            Debug.WriteLine($"ReadAllFrames: took too long! process duration: {stopwatch.ElapsedMilliseconds}ms, sample duration: {collection.Duration.TotalMilliseconds}ms");
        //        }
        //        return collection;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
    }
}

