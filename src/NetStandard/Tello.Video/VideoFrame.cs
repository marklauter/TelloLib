using System;
using System.Collections.Generic;
using System.Text;

namespace Tello.Video
{
    public sealed class VideoFrame
    {
        public VideoFrame(byte[] content, long frameIndex, double fps, long ttlBytesProcessed)
        {
            Content = content;
            Index = frameIndex;
            FramesPerSecond = fps;
            Size = content.Length;
            TimeIndex = TimeSpan.FromSeconds(frameIndex / 30.0);
            TtlBytesProcessed = ttlBytesProcessed;
        }

        public static TimeSpan DurationPerFrame { get; } = TimeSpan.FromSeconds(1 / 30.0);

        public byte[] Content { get; }
        public long Index { get; }
        public double FramesPerSecond { get; }
        public int Size { get; }
        public TimeSpan TimeIndex { get; }
        public long TtlBytesProcessed { get; }

        public override string ToString()
        {
            return $"{TimeIndex}: #{Index}, {(int)FramesPerSecond}f/s, {Size.ToString("#,#")}B, {((uint)(TtlBytesProcessed * 8 / TimeIndex.TotalSeconds)).ToString("#,#")}b/s";
        }
    }
}
