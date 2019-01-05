﻿using System;
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

            _frameComposer = new FrameComposer(frameRate, bitRate, bufferTime, bytesPerSample);
            _frameComposer.FrameReady += _frameComposer_FrameReady;
        }

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

        private readonly FrameComposer _frameComposer;
        private readonly UdpReceiver _udpReceiver;

        public bool TryReadFrame(out VideoFrame frame, TimeSpan timeout)
        {
            return _frameComposer.TryGetFrame(out frame, timeout);
        }

        public VideoFrameCollection ReadFramesAsync(MediaStreamSourceSampleRequest request, TimeSpan timeout, int frameCount)
        {
            var stopwatch = Stopwatch.StartNew();
            var collection = new VideoFrameCollection();

            while (collection.Count < frameCount && stopwatch.Elapsed < timeout && _frameComposer.TryGetFrame(out var frame, timeout))
            {
                collection.Add(frame);
                var progress = (uint)(collection.Count / frameCount * 100);
                request.ReportSampleProgress(progress);
            }

            if (stopwatch.ElapsedMilliseconds > collection.Duration.TotalMilliseconds)
            {
                Debug.WriteLine($"ReadSample: took too long! process duration: {stopwatch.ElapsedMilliseconds}ms, sample duration: {collection.Duration.TotalMilliseconds}ms");
            }

            return collection;
        }
    }
}