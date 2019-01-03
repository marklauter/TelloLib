using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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

    public sealed class FrameComposer
    {
        #region controls
        private bool _running = false;
        public async void Start()
        {
            if (!_running)
            {
                _running = true;
                await Task.Run(() => { ComposeFrames(); });
            }
        }
        public void Stop()
        {
            _running = false;
        }
        #endregion

        #region queues
        private readonly ConcurrentQueue<byte[]> _inputSamples = new ConcurrentQueue<byte[]>();
        private readonly ConcurrentQueue<VideoFrame> _outputFrames = new ConcurrentQueue<VideoFrame>();
        #endregion

        public bool FramesReady { get; private set; }

        private void ComposeFrames()
        {
            long byteCount = 0;
            long frameIndex = 0;
            MemoryStream frameStream = null;
            var fpsWatch = new Stopwatch();

            while (true)
            {
                // TryDequeue checks IsEmpty 
                if (_inputSamples.TryDequeue(out var sample))
                {
                    if (!fpsWatch.IsRunning)
                    {
                        fpsWatch.Start();
                    }

                    // scan sample for 0x00 0x00 0x00 0x01 - H264 NALU delimiter
                    if (sample.Length > 4 && sample[0] == 0x00 && sample[1] == 0x00 && sample[2] == 0x00 && sample[3] == 0x01)
                    {
                        // close out existing frame
                        if (frameStream != null)
                        {
                            var frame = new VideoFrame(frameStream.ToArray(), frameIndex, frameIndex / fpsWatch.Elapsed.TotalSeconds, byteCount);
                            _outputFrames.Enqueue(frame);
                            ++frameIndex;
                            FramesReady = true;

                            // write a frame sample to debug output ~ every second so we can see how we're doing with performance
                            if (frameIndex % 30 == 0)
                            {
                                Debug.WriteLine($"\nkeyframe: {frame}");
                            }
                        }
                        frameStream = new MemoryStream(1024 * 16);
                    }

                    if (frameStream != null)
                    {
                        frameStream.Write(sample, 0, sample.Length);
                        byteCount += sample.Length;
                    }
                }
            }
        }

        public void AddSample(byte[] sample)
        {
            _inputSamples.Enqueue(sample);
        }

        public VideoFrame ReadFrame()
        {
            Stopwatch stopwatch = null;
            VideoFrame frame = null;
            while (!_outputFrames.TryDequeue(out frame))
            {
                if (stopwatch == null)
                {
                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                }

                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    Debug.WriteLine("ReadFrame timed out");
                    break;
                }
            }

            return frame;
        }
    }
}
