using System;

namespace Tello.Video
{
    public class FrameReadyArgs : EventArgs
    {
        public FrameReadyArgs(VideoFrame frame)
        {
            Frame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public VideoFrame Frame { get; }
    }
}
