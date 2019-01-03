using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
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
        #region ctor
        public MainPage()
        {
            InitializeComponent();

            _commandResponseListView.ItemsSource = _telloCommandReponse;
            _tello.ResponseReceived += _tello_ResponseReceived;

            StartStateReciever();
            _videoServer.Start();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            InitializeVideo();
        }
        #endregion

        #region video
        private readonly VideoServer _videoServer = new VideoServer(11111);

        //private byte[] _fullvideo;

        private bool _videoInitialized = false;
        private void InitializeVideo()
        {
            if (!_videoInitialized)
            {
                _videoInitialized = true;

                //using (var file = File.OpenRead("tello.mp4"))
                //{
                //    _fullvideo = new byte[file.Length];
                //    file.Read(_fullvideo, 0, (int)file.Length);
                //    _dataReady = true;
                //}

                //var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///AppData/goodvideo.mp4", UriKind.RelativeOrAbsolute));
                //var clip = await MediaClip.CreateFromFileAsync(file);
                //var videoEncodingProperties = clip.GetVideoEncodingProperties();

                var vep = VideoEncodingProperties.CreateH264();
                //2352698.5700033255736614566012637
                //vep.Bitrate = 2350000;
                vep.Height = 720;
                vep.Width = 960;

                var vsd = new VideoStreamDescriptor(vep);

                var mss = new MediaStreamSource(vsd)
                {
                    IsLive = true,
                    //BufferTime = TimeSpan.FromMilliseconds(333.33) // approximately 10 frames
                };

                mss.SampleRequested += Mss_SampleRequested;
                mss.Starting += Mss_Starting;
                mss.Closed += Mss_Closed;
                mss.SampleRendered += Mss_SampleRendered;
                //mss.SwitchStreamsRequested += Mss_SwitchStreamsRequested;

                _mediaElement.SetMediaStreamSource(mss);
                //_mediaElement.RealTimePlayback = true;

                Debug.WriteLine("media element initialized");
            }
        }

        private long _lagTest = 0;
        private void Mss_SampleRendered(MediaStreamSource sender, MediaStreamSourceSampleRenderedEventArgs args)
        {
            ++_lagTest;
            if (_lagTest % 30 == 0)
            {
                Debug.WriteLine($"sample lag: {args.SampleLag}");
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
            //_dataReady = false;
        }

        //private MediaStreamSourceStartingRequestDeferral _startDeferral = null;
        private async void Mss_Starting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Debug.WriteLine("Mss_Starting");
            var deferral = args.Request.GetDeferral();
            try
            {
                Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms media player waiting");
                while (!_videoServer.DataReady)
                {
                    await Task.Yield();
                }
            }
            finally
            {
                deferral.Complete();
            }
            Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms media player started");
        }

        //private bool _dataReady = false;
        //private void _videoServer_DataReady(object sender, EventArgs e)
        //{
        //    if (!_dataReady)
        //    {
        //        Debug.WriteLine("data ready");
        //        _dataReady = true;
        //        if (_startDeferral != null)
        //        {
        //            Debug.WriteLine("start continued");
        //            _startDeferral.Complete();
        //            _startDeferral = null;
        //        }
        //    }

        //    if (_sampleDeferral != null)
        //    {
        //        Debug.WriteLine("sample continued");
        //        _sampleDeferral.Complete();
        //        _sampleDeferral = null;
        //    }
        //}

        private void Mss_SampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            //args.Request.Sample = MediaStreamSample.CreateFromBuffer(_fullvideo.AsBuffer(), DateTime.Now - _started.Value);

            //var sample = await _videoServer.ReadSampleAsync(args.Request);
            //args.Request.Sample = MediaStreamSample.CreateFromBuffer(sample.Content.AsBuffer(), sample.TimeIndex);
            //args.Request.Sample.Duration = sample.Duration;

            Debug.Write("+");
            var frame = _videoServer.ReadVideoFrame(args.Request);
            args.Request.Sample = MediaStreamSample.CreateFromBuffer(frame.Content.AsBuffer(), frame.TimeIndex);
            args.Request.Sample.Duration = VideoFrame.DurationPerFrame;
            Debug.Write("-");
        }
        #endregion

        #region tello state

        private readonly Receiver _stateReceiver = new Receiver(8890);

        private void StartStateReciever()
        {
            _stateReceiver.DatagramReceived += StateReceiver_DatagramReceived;
            _stateReceiver.Start();
            Debug.WriteLine($"state receiver listening on port 8890");
        }

        private async void StateReceiver_DatagramReceived(object sender, ReceiverDatagramArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { _telloStateText.Text = $"TELLO STATE: {Encoding.UTF8.GetString(e.Datagram)}"; });
        }
        #endregion

        #region tello commands

        private readonly Transceiver _tello = new Transceiver("192.168.10.1", 8889);

        private ObservableCollection<string> _telloCommandReponse = new ObservableCollection<string>();

        private const int _sendVideo = 999;

        private async void _tello_ResponseReceived(object sender, ResponseReceivedArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Response.Datagram);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (e.Request.UserData == _sendVideo && message.ToLower() == "ok")
                {
                    _mediaElement.Play();
                }
                _telloCommandReponse.Insert(0, message);
            });
        }

        private void _connectButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _telloCommandReponse.Insert(0, "connecting to tello");
            try
            {
                _tello.Connect();
            }
            catch (Exception ex)
            {
                _telloCommandReponse.Insert(0, ex.ToString());
            }

            _telloCommandReponse.Insert(0, "sending 'command' command");
            var request = new Request(Encoding.ASCII.GetBytes("command"), false, false);
            _tello.Send(request);
        }

        private void _takeoffButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _telloCommandReponse.Insert(0, "sending 'takeoff' command");
            var request = new Request(Encoding.ASCII.GetBytes("takeoff"), false, false);
            _tello.Send(request);
        }

        private void _landButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _telloCommandReponse.Insert(0, "sending 'land' command");
            var request = new Request(Encoding.ASCII.GetBytes("land"), false, false);
            _tello.Send(request);
        }

        private void _startVideoButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _telloCommandReponse.Insert(0, "sending 'streamon' (start video) command");
            var request = new Request(Encoding.ASCII.GetBytes("streamon"), false, false)
            {
                UserData = _sendVideo
            };
            _tello.Send(request);
        }

        private void _stopVideoButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _mediaElement.Stop();

            _telloCommandReponse.Insert(0, "sending 'streamon' (stop video) command");
            var request = new Request(Encoding.ASCII.GetBytes("streamoff"), false, false);
            _tello.Send(request);
        }

        private void _checkBatteryButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _telloCommandReponse.Insert(0, "sending 'battery?' command");
            var request = new Request(Encoding.ASCII.GetBytes("battery?"), false, false);
            _tello.Send(request);
        }
        #endregion
    }
}
