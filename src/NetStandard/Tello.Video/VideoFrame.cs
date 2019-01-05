﻿using System;

namespace Tello.Video
{
    public sealed class VideoFrame
    {
        public VideoFrame(byte[] content, long frameIndex, TimeSpan timeIndex, TimeSpan duration)
        {
            Content = content;
            Index = frameIndex;
            TimeIndex = timeIndex;
            Duration = duration;
            Size = content.Length;
        }

        public TimeSpan Duration { get; } = TimeSpan.FromSeconds(1 / 30.0);

        public byte[] Content { get; }
        public long Index { get; }
        public TimeSpan TimeIndex { get; }
        public int Size { get; }
    }
}
