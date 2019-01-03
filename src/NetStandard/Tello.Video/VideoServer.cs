using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Windows.Media.Core;

//todo: create tello video emulator
//todo: try feeding individual frames to getsample
//todo: try timing the UDP speed

namespace Tello.Video
{
    public sealed class VideoServer
    {
        private readonly int _port;
        public VideoServer(int port = 11111)
        {
            _port = port;
        }

        #region controls
        private bool _running = false;
        public async void Start()
        {
            Debug.WriteLine($"video server starting on port: {_port}");
            if (!_running)
            {
                _running = true;
                _frameComposer.Start();
                await Task.Run(() => { Listen(); });
            }
        }

        public void Stop()
        {
            _frameComposer.Stop();
            Debug.WriteLine($"video server on port {_port} stopped");
            _running = false;
        }
        #endregion

        private FrameComposer _frameComposer = new FrameComposer();

        public bool DataReady => _frameComposer.FramesReady;

        internal async void Listen()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var client = new UdpClient(_port))
            {
                Debug.WriteLine($"video server listening on port: {_port}");
                while (_running)
                {
                    _frameComposer.AddSample(client.Receive(ref endPoint));
                    await Task.Yield();
                }
            }
        }

        public VideoFrame ReadVideoFrame()
        {
            return _frameComposer.ReadFrame();
        }

        public VideoSample ReadSample(MediaStreamSourceSampleRequest request)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var frame = _frameComposer.ReadFrame();
            if (frame == null)
            {
                return null;
            }

            var sample = new VideoSample(frame);
            for (var i = 0; i < 4; ++i)
            {
                frame = _frameComposer.ReadFrame();
                if (frame == null)
                {
                    break;
                }
                sample.AddFrame(frame);
                var progress = (uint)((i + 1) / 5 * 100);
                request.ReportSampleProgress(progress);
            }
            request.ReportSampleProgress(100);
            if (stopwatch.ElapsedMilliseconds > sample.Duration.TotalMilliseconds)
            {
                Debug.WriteLine($"ReadSample: took too long! {stopwatch.ElapsedMilliseconds}ms, sample.duration {sample.Duration.TotalMilliseconds}ms");
            }
            return sample;
        }
    }

    public sealed class VideoSample
    {
        public VideoSample(VideoFrame frame)
        {
            TimeIndex = frame.TimeIndex;
            AddFrame(frame);
        }

        public void AddFrame(VideoFrame frame)
        {
            Frames.Add(frame);
            _sample.Write(frame.Content, 0, frame.Size);
            Duration += VideoFrame.DurationPerFrame;
            Size += frame.Size;
        }

        private MemoryStream _sample = new MemoryStream();
        public byte[] Content => _sample.ToArray();
        public TimeSpan Duration { get; private set; } = TimeSpan.FromSeconds(0.0);
        public List<VideoFrame> Frames { get; } = new List<VideoFrame>();
        public long Size { get; private set; } = 0;
        public TimeSpan TimeIndex { get; }
    }
}
