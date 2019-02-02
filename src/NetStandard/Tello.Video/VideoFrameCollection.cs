using System;
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

        public VideoFrameCollection(VideoFrame[] frames)
        {
            if (frames == null || frames.Length == 0)
            {
                throw new ArgumentNullException(nameof(frames));
            }

            var frame = frames[0];
            Count = frames.Length;
            TimeIndex = frame.TimeIndex;
            for (var i = 0; i < frames.Length; ++i)
            {
                frame = frames[i];
                _sample.Write(frame.Content, 0, frame.Size);
                Duration += frame.Duration;
            }
        }

        public void Add(VideoFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            if (Count == 0)
            {
                TimeIndex = frame.TimeIndex;
            }

            _sample.Write(frame.Content, 0, frame.Size);
            Duration += frame.Duration;
            ++Count;
        }

        //private readonly List<VideoFrame> _frames = new List<VideoFrame>();
        private readonly MemoryStream _sample = new MemoryStream();

        public byte[] Content => _sample.ToArray();
        public TimeSpan Duration { get; private set; } = TimeSpan.FromSeconds(0.0);
        public long Size => _sample.Length;
        public TimeSpan TimeIndex { get; private set; }

        //public VideoFrame this[int index] => _frames[index];
        //public int Count => _frames.Count;
        public int Count { get; private set; } = 0;
    }
}
