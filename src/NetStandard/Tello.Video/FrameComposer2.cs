using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tello.Video
{
    /// <summary>
    /// FrameComposer2 is the best of the three versions
    /// </summary>
    internal sealed class FrameComposer2
    {
        public FrameComposer2(double frameRate, TimeSpan bufferTime)
        {
            _frameRate = frameRate;
            _frameDuration = TimeSpan.FromSeconds(1 / _frameRate);

            _frames = new RingBuffer<VideoFrame>((int)(_frameRate * bufferTime.TotalSeconds));
        }

        public event EventHandler<FrameReadyArgs> FrameReady;

        #region fields
        private long _byteCount = 0;
        private long _frameIndex = 0;
        private readonly Stopwatch _frameRateWatch = new Stopwatch();
        private MemoryStream _stream = null;
        private readonly TimeSpan _frameDuration;
        private readonly double _frameRate;
        private readonly RingBuffer<VideoFrame> _frames;
        #endregion

        #region controls
        private bool _running = false;
        public void Start()
        {
            if (!_running)
            {
                _paused = false;
                _running = true;
            }
        }

        private bool _paused = false;
        public void Pause()
        {
            _paused = true;
        }

        public void Resume()
        {
            _paused = false;
        }

        public void Stop()
        {
            _running = false;
            _paused = false;
        }
        #endregion

        #region frame composition
        private bool IsNewFrame(byte[] sample)
        {
            // check sample for 0x00 0x00 0x00 0x01 header - H264 NALU frame start delimiter
            return sample.Length > 4 && sample[0] == 0x00 && sample[1] == 0x00 && sample[2] == 0x00 && sample[3] == 0x01;
        }

        private VideoFrame QueueFrame(MemoryStream stream, long frameIndex)
        {
            var frame = new VideoFrame(stream.ToArray(), frameIndex, TimeSpan.FromSeconds(frameIndex / _frameRate), _frameDuration);
            _frames.Push(frame);
            FrameReady?.Invoke(this, new FrameReadyArgs(frame));
            return frame;
        }

        private void ComposeFrame(byte[] sample)
        {
            if (_paused || !_running)
            {
                return;
            }

            if (IsNewFrame(sample))
            {
                // close out existing frame
                if (_stream != null)
                {
                    var frame = QueueFrame(_stream, _frameIndex);
                    Interlocked.Increment(ref _frameIndex);
                    // write a frame sample to debug output ~every 5 seconds so we can see how we're doing with performance
                    if (_frameIndex % (_frameRate * 5) == 0)
                    {
                        Debug.WriteLine($"\nFC {frame.TimeIndex}: f#{frame.Index}, composition rate: {(frame.Index / _frameRateWatch.Elapsed.TotalSeconds).ToString("#,#")}f/s, bit rate: {((uint)(_byteCount * 8 / frame.TimeIndex.TotalSeconds)).ToString("#,#")}b/s");
                    }
                }
                else
                {
                    // first frame, so start timer
                    _frameRateWatch.Start();
                }
                _stream = new MemoryStream(1024 * 16);
            }

            // don't start writing until we have a frame start
            if (_stream != null)
            {
                _stream.Write(sample, 0, sample.Length);
                Interlocked.Add(ref _byteCount, sample.Length);
            }
        }
        #endregion

        public void AddSample(byte[] sample)
        {
            ComposeFrame(sample);
        }

        internal VideoFrameCollection GetFrames(TimeSpan timeout)
        {
            return new VideoFrameCollection(_frames.Flush());
        }

        private Stopwatch _frameStopWatch = new Stopwatch();
        SpinWait _frameWait = new SpinWait();
        public bool TryGetFrame(out VideoFrame frame, TimeSpan timeout)
        {
            _frameStopWatch.Restart();
            while (!_frames.TryPop(out frame) && _frameStopWatch.Elapsed < timeout)
            {
                _frameWait.SpinOnce();
            }
            return frame != null;
        }
    }
}
