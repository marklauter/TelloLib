using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Tello.Udp;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


//https://stackoverflow.com/questions/25323434/mediaelement-set-source-from-stream

//this has promise
//https://github.com/cisco/openh264

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

////https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/process-media-frames-with-mediaframereader

////https://github.com/Microsoft/Windows-universal-samples/blob/dev/Samples/SimpleCommunication/cs/CaptureDevice.cs
//    here's an idea: 
//capture video from camera using MediaCapture (see the sample TCP project I cloned last night)
////https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/basic-photo-video-and-audio-capture-with-mediacapture
//then push the video via UDP to own self and try to show it in media element

//    the memory stream gets too big and then crashes

namespace Tello.Video
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

        }

        private MediaTranscoder _transcoder = new MediaTranscoder()
        {
        };

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //foo();
            //var x = new FileOpenPicker();

            SetupMedia();

            var stateReceiver = new Receiver(8890);
            stateReceiver.DatagramReceived += StateReceiver_DatagramReceived;
            stateReceiver.BeginReceiving();
            Debug.WriteLine($"listening on port 8890");

            var videoReceiver = new Receiver(11111);
            videoReceiver.DatagramReceived += VideoReceiver_DatagramReceived;
            videoReceiver.BeginReceiving();
            Debug.WriteLine($"listening on port 11111");
            Debug.WriteLine("==============================================");

        }

        private object _gate = new object();
        private byte[] _frame;
        private List<byte[]> _frameSegments = new List<byte[]>();
        private int _frameCount = 0;
        private ulong _packetCount = 0;
        private void VideoReceiver_DatagramReceived(object sender, ReceiverDatagramArgs e)
        {
            lock (_gate)
            {
                _frameSegments.Add(e.Datagram);
            }

            ++_packetCount;

            if (e.Datagram.Length != 1460)
            {
                Debug.WriteLine($"frame end detected? packet: {_packetCount }");
            }

            if (e.Datagram[0] == 0 && e.Datagram[1] == 0 && e.Datagram[2] == 0 && e.Datagram[3] == 1)
            {
                ++_frameCount;
                Debug.WriteLine($"new frame detected. packet: {_packetCount }");
            }
            //// new frame
            //if (e.Datagram[0] == 0 && e.Datagram[1] == 0 && e.Datagram[2] == 0 && e.Datagram[3] == 1)
            //{
            //    Debug.WriteLine("new frame detected");

            //    // 1. terminate old frame
            //    lock (_gate)
            //    {
            //        var frameSize = 0;
            //        for (var i = 0; i < _frameSegments.Count; ++i)
            //        {
            //            frameSize += _frameSegments[i].Length;
            //        }
            //        _frame = new byte[frameSize];
            //        var offset = 0;
            //        for (var i = 0; i < _frameSegments.Count; ++i)
            //        {
            //            Array.Copy(_frameSegments[0], 0, _frame, offset, _frameSegments[0].Length);
            //            offset += _frameSegments[0].Length;
            //        }

            //        // 2. add new frame start to segment list
            //        var segmentCount = _frameSegments.Count;
            //        _frameSegments.Clear();
            //        _frameSegments.Capacity = segmentCount;
            //    }
            //}
        }

        private static void StateReceiver_DatagramReceived(object sender, ReceiverDatagramArgs e)
        {
            Debug.WriteLine($"TELLO STATE: {Encoding.UTF8.GetString(e.Datagram)}");
        }

        private bool _mediaInitialized = false;
        private MemoryStream _stream = new MemoryStream();
        private void SetupMedia()
        {
            if (!_mediaInitialized)
            {
                var vep = VideoEncodingProperties.CreateH264();
                //vep.Bitrate = 32;
                vep.Height = 720;
                vep.Width = 960;

                var vsd = new VideoStreamDescriptor(vep);

                var mss = new MediaStreamSource(vsd)
                {
                    IsLive = true,
                    BufferTime = TimeSpan.FromSeconds(0.0)
                };

                mss.SampleRequested += Mss_SampleRequested;
                mss.Starting += Mss_Starting;
                mss.Closed += Mss_Closed;
                mss.SampleRendered += Mss_SampleRendered;

                _mediaPlayerElement.SetMediaStreamSource(mss);
                _mediaInitialized = true;
            }
        }

        private void Mss_SampleRendered(MediaStreamSource sender, MediaStreamSourceSampleRenderedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private void Mss_Closed(MediaStreamSource sender, MediaStreamSourceClosedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private void Mss_Starting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private DateTime? _started;
        private void Mss_SampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            List<byte[]> segments;
            lock (_gate)
            {
                segments = new List<byte[]>(_frameSegments);
                _frameSegments.Clear();
                _frameSegments.Capacity = segments.Count;
            }

            var sampleSize = 0;
            for (var i = 0; i < segments.Count; ++i)
            {
                sampleSize += segments[i].Length;
            }
            var sample = new byte[sampleSize];
            var offset = 0;
            for (var i = 0; i < segments.Count; ++i)
            {
                Array.Copy(segments[i], 0, sample, offset, segments[i].Length);
                offset += segments[i].Length;
            }

            args.Request.ReportSampleProgress(100);
            if (!_started.HasValue)
            {
                _started = DateTime.Now;
            }
            args.Request.Sample = MediaStreamSample.CreateFromBuffer(sample.AsBuffer(), DateTime.Now - _started.Value);
            args.Request.Sample.Duration = TimeSpan.FromSeconds(_frameCount / 32.0);
            _frameCount = 0;

        }
    }
}
