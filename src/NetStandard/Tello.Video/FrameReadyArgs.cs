using System;

namespace Tello.Video
{
    public class FrameReadyArgs
    {
        public FrameReadyArgs(VideoFrame frame)
        {
            Frame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public VideoFrame Frame { get; }
    }
}
