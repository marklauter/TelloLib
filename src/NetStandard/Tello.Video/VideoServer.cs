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
    public class VideoServer
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
                await Task.Run(() => { Listen(); });
            }
        }

        public void Stop()
        {
            _running = false;
            Debug.WriteLine($"video server on port {_port} stopped");
        }
        #endregion

        private FrameComposer _frameComposer = new FrameComposer();

        internal void Listen()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var client = new UdpClient(_port))
            {
                Debug.WriteLine($"video server listening on port: {_port}");
                while (_running)
                {
                    _frameComposer.AddSample(client.Receive(ref endPoint));
                }
            }
        }

        public async Task<VideoFrame> ReadSampleAsync(MediaStreamSourceSampleRequest request)
        {
            // try reading 10 samples at a time
            //var sample = new VideoSample(await _frameComposer.ReadFrameAsync(request));
            //for (var i=0;i<9; ++i)
            //{
            //    sample.AddFrame(await _frameComposer.ReadFrameAsync(request));
            //}
            return _frameComposer.ReadFrameAsync(request);
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
