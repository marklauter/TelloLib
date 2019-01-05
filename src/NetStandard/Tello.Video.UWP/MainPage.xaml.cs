using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Tello.Udp;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


//https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/process-media-frames-with-mediaframereader
//https://github.com/Microsoft/Windows-universal-samples/blob/dev/Samples/SimpleCommunication/cs/CaptureDevice.cs

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
            _frameServer.Start();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            InitializeVideo();
        }
        #endregion

        #region video
        private readonly VideoFrameServer _frameServer = new VideoFrameServer(30, 2400000, TimeSpan.FromSeconds(30), 1460, 11111);

        private bool _videoInitialized = false;
        private void InitializeVideo()
        {
            if (!_videoInitialized)
            {
                _videoInitialized = true;

                var vep = VideoEncodingProperties.CreateH264();
                vep.Bitrate = 2400000;
                vep.Height = 720;
                vep.Width = 960;

                var mss = new MediaStreamSource(new VideoStreamDescriptor(vep))
                {
                    IsLive = true,
                    BufferTime = TimeSpan.FromSeconds(2)
                };

                mss.SampleRequested += Mss_SampleRequested;
                mss.Starting += Mss_Starting;
                mss.Closed += Mss_Closed;
                //mss.SampleRendered += Mss_SampleRendered;
                //mss.SwitchStreamsRequested += Mss_SwitchStreamsRequested;

                _mediaElement.SetMediaStreamSource(mss);
                _mediaElement.BufferingProgressChanged += _mediaElement_BufferingProgressChanged;
                //_mediaElement.RealTimePlayback = true;

                _frameServer.FrameReady += _frameServer_FrameReady;

                Debug.WriteLine("media element initialized");
            }
        }

        private void _mediaElement_BufferingProgressChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Debug.WriteLine($"_mediaElement_BufferingProgressChanged: {_mediaElement.BufferingProgress}%");
        }

        private bool _framesReady = false;
        private async void _frameServer_FrameReady(object sender, FrameReadyArgs e)
        {
            if (!_receivingVideo)
            {
                _framesReady = false;
            }
            if (!_framesReady)
            {
                _framesReady = true;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    _mediaElement.Play();
                });
            }
        }

        //private long _lagTest = 0;
        //private void Mss_SampleRendered(MediaStreamSource sender, MediaStreamSourceSampleRenderedEventArgs args)
        //{
        //    ++_lagTest;
        //    if (_lagTest % 30 == 0)
        //    {
        //        Debug.WriteLine($"sample lag: {args.SampleLag}");
        //    }
        //}

        //private void Mss_SwitchStreamsRequested(MediaStreamSource sender, MediaStreamSourceSwitchStreamsRequestedEventArgs args)
        //{
        //    //Debug.WriteLine("Mss_SwitchStreamsRequested");
        //}

        private void Mss_Closed(MediaStreamSource sender, MediaStreamSourceClosedEventArgs args)
        {
            Debug.WriteLine("Mss_Closed");
        }

        private void Mss_Starting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
        {
            Debug.WriteLine("Mss_Starting");
        }

        private readonly TimeSpan _frameTimeout = TimeSpan.FromMilliseconds(250);
        private void Mss_SampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            Debug.Write("+");

            // this creates a buffer delay of ~ 5 to 10 seconds
            //var sample = _videoServer.ReadSample(args.Request);
            //if (sample != null)
            //{
            //    args.Request.Sample = MediaStreamSample.CreateFromBuffer(sample.Content.AsBuffer(), sample.TimeIndex);
            //    args.Request.Sample.Duration = sample.Duration;
            //}

            // this creates a buffer delay of ~ 2 to 3 seconds
            if (_frameServer.TryReadFrame(out var frame, _frameTimeout))
            {
                args.Request.Sample = MediaStreamSample.CreateFromBuffer(frame.Content.AsBuffer(), frame.TimeIndex);
                args.Request.Sample.Duration = frame.Duration;
            }
            Debug.Write("-");
        }
        #endregion

        #region tello state

        private readonly UdpReceiver _stateReceiver = new UdpReceiver(8890);

        private void StartStateReciever()
        {
            _stateReceiver.DatagramReceived += StateReceiver_DatagramReceived;
            _stateReceiver.Start();
            Debug.WriteLine($"state receiver listening on port 8890");
        }

        private async void StateReceiver_DatagramReceived(object sender, DatagramReceivedArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { _telloStateText.Text = $"TELLO STATE: {Encoding.UTF8.GetString(e.Datagram)}"; });
        }
        #endregion

        #region tello commands

        private readonly Transceiver _tello = new Transceiver("192.168.10.1", 8889);

        private ObservableCollection<string> _telloCommandReponse = new ObservableCollection<string>();

        private const int _startVideo = 999;
        private const int _stopVideo = 1000;
        private bool _receivingVideo = false;

        private async void _tello_ResponseReceived(object sender, ResponseReceivedArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Response.Datagram);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (e.Request.UserData == _startVideo && message.ToLower() == "ok")
                {
                    _receivingVideo = true;
                }
                if (e.Request.UserData == _stopVideo && message.ToLower() == "ok")
                {
                    _receivingVideo = false;
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
                UserData = _startVideo
            };
            _tello.Send(request);
        }

        private void _stopVideoButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _mediaElement.Stop();

            _telloCommandReponse.Insert(0, "sending 'streamon' (stop video) command");
            var request = new Request(Encoding.ASCII.GetBytes("streamoff"), false, false)
            {
                UserData = _stopVideo
            };
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
