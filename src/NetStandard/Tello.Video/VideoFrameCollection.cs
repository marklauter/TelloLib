using System;
using System.Collections.Generic;
using System.IO;

namespace Tello.Video
{
    public sealed class VideoFrameCollection
    {
        public VideoFrameCollection(VideoFrame frame = null)
        {
            if (frame != null)
            {
                Add(frame);
            }
        }

        public void Add(VideoFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            if(_frames.Count == 0)
            {
                TimeIndex = frame.TimeIndex;
            }

            _frames.Add(frame);

            _sample.Write(frame.Content, 0, frame.Size);

            Duration += frame.Duration;
        }

        private readonly List<VideoFrame> _frames = new List<VideoFrame>();
        private readonly MemoryStream _sample = new MemoryStream();

        public byte[] Content => _sample.ToArray();
        public TimeSpan Duration { get; private set; } = TimeSpan.FromSeconds(0.0);
        public long Size => _sample.Length;
        public TimeSpan TimeIndex { get; private set; }

        public VideoFrame this[int index] => _frames[index];
        public int Count => _frames.Count;
    }
}
