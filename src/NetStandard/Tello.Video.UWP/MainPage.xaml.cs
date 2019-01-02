using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Tello.Udp;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
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

namespace Tello.Video.UWP
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

        private readonly VideoServer _videoServer = new VideoServer();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //var stateReceiver = new Receiver(8890);
            //stateReceiver.DatagramReceived += StateReceiver_DatagramReceived;
            //stateReceiver.BeginReceiving();
            //Debug.WriteLine($"listening on port 8890");

            //var videoReceiver = new Receiver(11111);
            //videoReceiver.DatagramReceived += VideoReceiver_DatagramReceived;
            //videoReceiver.BeginReceiving();
            //Debug.WriteLine($"listening on port 11111");
            //Debug.WriteLine("==============================================");

            //using(var file = File.OpenRead("tello.mp4"))
            //{
            //    _fullvideo = new byte[file.Length];
            //    file.Read(_fullvideo, 0, (int)file.Length);
            //}

            SetupMedia();

            _videoServer.DataReady += _videoServer_DataReady;
            _videoServer.Start(11111);
        }

        //private byte[] _fullvideo;

        //private object _gate = new object();
        //private byte[] _frame;
        //private List<byte[]> _frameSegments = new List<byte[]>();
        //private int _frameCount = 0;
        //private ulong _packetCount = 0;
        //private void VideoReceiver_DatagramReceived(object sender, ReceiverDatagramArgs e)
        //{
        //    lock (_gate)
        //    {
        //        _frameSegments.Add(e.Datagram);
        //    }

        //    ++_packetCount;

        //    if (e.Datagram.Length != 1460)
        //    {
        //        Debug.WriteLine($"frame end detected? packet: {_packetCount }");
        //    }

        //    if (e.Datagram[0] == 0 && e.Datagram[1] == 0 && e.Datagram[2] == 0 && e.Datagram[3] == 1)
        //    {
        //        ++_frameCount;
        //        Debug.WriteLine($"new frame detected. packet: {_packetCount }");
        //    }
        //    //// new frame
        //    //if (e.Datagram[0] == 0 && e.Datagram[1] == 0 && e.Datagram[2] == 0 && e.Datagram[3] == 1)
        //    //{
        //    //    Debug.WriteLine("new frame detected");

        //    //    // 1. terminate old frame
        //    //    lock (_gate)
        //    //    {
        //    //        var frameSize = 0;
        //    //        for (var i = 0; i < _frameSegments.Count; ++i)
        //    //        {
        //    //            frameSize += _frameSegments[i].Length;
        //    //        }
        //    //        _frame = new byte[frameSize];
        //    //        var offset = 0;
        //    //        for (var i = 0; i < _frameSegments.Count; ++i)
        //    //        {
        //    //            Array.Copy(_frameSegments[0], 0, _frame, offset, _frameSegments[0].Length);
        //    //            offset += _frameSegments[0].Length;
        //    //        }

        //    //        // 2. add new frame start to segment list
        //    //        var segmentCount = _frameSegments.Count;
        //    //        _frameSegments.Clear();
        //    //        _frameSegments.Capacity = segmentCount;
        //    //    }
        //    //}
        //}

        private static void StateReceiver_DatagramReceived(object sender, ReceiverDatagramArgs e)
        {
            Debug.WriteLine($"TELLO STATE: {Encoding.UTF8.GetString(e.Datagram)}");
        }

        private bool _mediaInitialized = false;
        private void SetupMedia()
        {
            if (!_mediaInitialized)
            {
                _mediaInitialized = true;

                var vep = VideoEncodingProperties.CreateH264();
                //vep.Bitrate = 3000000;
                vep.Height = 720;
                vep.Width = 960;

                var vsd = new VideoStreamDescriptor(vep);

                var mss = new MediaStreamSource(vsd)
                {
                    IsLive = true,
                    BufferTime = TimeSpan.FromMilliseconds(250)
                };

                mss.SampleRequested += Mss_SampleRequested;
                mss.Starting += Mss_Starting;
                mss.Closed += Mss_Closed;
                //mss.SampleRendered += Mss_SampleRendered;
                //mss.SwitchStreamsRequested += Mss_SwitchStreamsRequested;

                _mediaPlayerElement.SetMediaStreamSource(mss);

                Debug.WriteLine("media element initialized");
            }
        }

        //private void Mss_SwitchStreamsRequested(MediaStreamSource sender, MediaStreamSourceSwitchStreamsRequestedEventArgs args)
        //{
        //    //Debug.WriteLine("Mss_SwitchStreamsRequested");
        //}

        //private void Mss_SampleRendered(MediaStreamSource sender, MediaStreamSourceSampleRenderedEventArgs args)
        //{
        //    //Debug.WriteLine("Mss_SampleRendered");
        //}

        private void Mss_Closed(MediaStreamSource sender, MediaStreamSourceClosedEventArgs args)
        {
            Debug.WriteLine("Mss_Closed");
            _dataReady = false;
        }

        private MediaStreamSourceStartingRequestDeferral _startDeferral = null;
        private void Mss_Starting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
        {
            Debug.WriteLine("Mss_Starting");
            if (!_dataReady)
            {
                Debug.WriteLine("start deferred");
                _startDeferral = args.Request.GetDeferral();
            }
            if(_sampleDeferral != null)
            {
                _sampleDeferral.Complete();
                _sampleDeferral = null;
            }
        }

        private bool _dataReady = false;
        private void _videoServer_DataReady(object sender, EventArgs e)
        {
            if (!_dataReady)
            {
                Debug.WriteLine("data ready");
                _dataReady = true;
                if (_startDeferral != null)
                {
                    Debug.WriteLine("start continued");
                    _startDeferral.Complete();
                    _startDeferral = null;
                }
            }

            if (_sampleDeferral != null)
            {
                Debug.WriteLine("sample continued");
                _sampleDeferral.Complete();
                _sampleDeferral = null;
            }
        }

        //private Stopwatch _watch = new Stopwatch();
        private DateTime? _started;
        private MediaStreamSourceSampleRequestDeferral _sampleDeferral = null;
        private void Mss_SampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
          //  _watch.Start();
            var sample = _videoServer.GetSample();
            //Debug.WriteLine($"get sample: {_watch.ElapsedMilliseconds}");
            //_watch.Restart();
            if (sample != null)
            {
                if (!_started.HasValue)
                {
                    _started = DateTime.Now;
                }
                args.Request.Sample = MediaStreamSample.CreateFromBuffer(sample.AsBuffer(), DateTime.Now - _started.Value);
                //Debug.WriteLine($"create media sample: {_watch.ElapsedMilliseconds}");

                //var seconds = sample.Length * 8 / 3000000.0;
                //args.Request.Sample.Duration = TimeSpan.FromSeconds(seconds);
            }
            else
            {
                Debug.WriteLine("sample deferred");
                _sampleDeferral = args.Request.GetDeferral();
            }
        }
    }
}
