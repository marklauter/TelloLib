using System;
using System.Collections.Generic;
using System.IO;

namespace Tello.Video
{
    public sealed class VideoFrameCollection
    {
        public VideoFrameCollection(VideoFrame frame)
        {
            TimeIndex = frame.TimeIndex;
            Add(frame);
        }

        public void Add(VideoFrame frame)
        {
            _frames.Add(frame);
            _sample.Write(frame.Content, 0, frame.Size);
            Duration += frame.Duration;
            Size += frame.Size;
        }

        private readonly List<VideoFrame> _frames = new List<VideoFrame>();
        private readonly MemoryStream _sample = new MemoryStream();

        public byte[] Content => _sample.ToArray();
        public TimeSpan Duration { get; private set; } = TimeSpan.FromSeconds(0.0);
        public long Size { get; private set; } = 0;
        public TimeSpan TimeIndex { get; }

        public VideoFrame this[int index] => _frames[index];
        public int Count => _frames.Count;
    }
}
