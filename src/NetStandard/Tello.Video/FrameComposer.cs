﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tello.Video
{
    public class FrameReadyArgs
    {
        public FrameReadyArgs(VideoFrame frame)
        {
            Frame = frame;
        }

        public VideoFrame Frame { get; }
    }

    public sealed class FrameComposer
    {
        public FrameComposer(double frameRate, int bitRate, TimeSpan bufferTime)
        {
            _frameRate = frameRate;
            _bitRate = bitRate;
            _frames = new RingBuffer<VideoFrame>((int)(_frameRate * bufferTime.TotalSeconds));

            _frameDuration = TimeSpan.FromSeconds(1 / _frameRate);

            var bytesPerSecond = _bitRate / 8;
            var samplesPerSecond = bytesPerSecond / _bytesPerSample;
            _samples = new RingBuffer<byte[]>((int)(samplesPerSecond * bufferTime.TotalSeconds * 2));
        }

        public event EventHandler<FrameReadyArgs> FrameReady;

        #region fields
        private readonly int _bytesPerSample = 1460;
        private readonly TimeSpan _frameDuration;
        private readonly double _frameRate;
        private readonly int _bitRate;
        private readonly RingBuffer<byte[]> _samples;
        private readonly RingBuffer<VideoFrame> _frames;
        #endregion

        #region controls
        private bool _running = false;
        public async void Start()
        {
            if (!_running)
            {
                _paused = false;
                _running = true;
                await Task.Run(() => { ComposeFrames(); });
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

        private VideoFrame UnsafeQueueFrame(MemoryStream stream, ref long frameIndex)
        {
            var frame = new VideoFrame(stream.ToArray(), frameIndex, TimeSpan.FromSeconds(frameIndex / _frameRate), _frameDuration);
            _frames.Push(frame);
            FrameReady?.Invoke(this, new FrameReadyArgs(frame));
            ++frameIndex;
            return frame;
        }

        private void ComposeFrames()
        {
            long byteCount = 0;
            long frameIndex = 0;
            MemoryStream stream = null;
            var frameRateWatch = new Stopwatch();

            while (_running)
            {
                if (_paused)
                {
                    continue;
                }

                if (_samples.TryPop(out var sample))
                {
                    if (!frameRateWatch.IsRunning)
                    {
                        frameRateWatch.Start();
                    }

                    if (IsNewFrame(sample))
                    {
                        // close out existing frame
                        if (stream != null)
                        {
                            var frame = UnsafeQueueFrame(stream, ref frameIndex);
                            // write a frame sample to debug output ~every 5 seconds so we can see how we're doing with performance
                            if (frameIndex % _frameRate * 5 == 0)
                            {
                                Debug.WriteLine($"{frame.TimeIndex}: #{frame.Index}, compositing rate: {(frameIndex / frameRateWatch.Elapsed.TotalSeconds).ToString("#,#")}f/s, {frame.Size.ToString("#,#")}B, {((uint)(byteCount * 8 / frame.TimeIndex.TotalSeconds)).ToString("#,#")}b/s");
                            }
                        }
                        stream = new MemoryStream(1024 * 16);
                    }

                    // don't start writing until we have a frame start
                    if (stream != null)
                    {
                        stream.Write(sample, 0, sample.Length);
                        byteCount += sample.Length;
                    }
                }
            }
        }
        #endregion

        public void AddSample(byte[] sample)
        {
            _samples.Push(sample);
        }

        public Task<VideoFrame> GetFrameAsync(TimeSpan timeout)
        {
            return Task.Run(() =>
            {
                var wait = new SpinWait();
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                VideoFrame frame = null;
                while (!_frames.TryPop(out frame) && stopwatch.Elapsed < timeout)
                {
                    wait.SpinOnce();
                }
                return frame;
            });
        }
    }
}
