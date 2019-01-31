#define __USE_EMULATOR

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
using static Tello.Udp.UdpTransceiver;


//https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/process-media-frames-with-mediaframereader
//https://github.com/Microsoft/Windows-universal-samples/blob/dev/Samples/SimpleCommunication/cs/CaptureDevice.cs

//https://stackoverflow.com/questions/33259763/uwp-enable-local-network-loopback

namespace Tello.Video.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region tello or emulator 
#if USE_EMULATOR
        // emulated tello
        private readonly UdpTransceiver _tello = new UdpTransceiver("127.0.0.1", 8889);
        private readonly UdpLoopbackReceiver _stateReceiver = new UdpLoopbackReceiver("127.0.0.1", 8890);
        private readonly VideoFrameLoopbackServer _frameServer = new VideoFrameLoopbackServer(32, TimeSpan.FromSeconds(1), "127.0.0.1", 11111);
#else
        // real tello
        private readonly UdpTransceiver _tello = new UdpTransceiver("192.168.10.1", 8889, TimeSpan.FromSeconds(60));
        private readonly UdpListener _stateReceiver = new UdpListener(8890);
        private readonly VideoFrameServer _frameServer = new VideoFrameServer(32, TimeSpan.FromMilliseconds(500), 11111);
#endif
        #endregion

        #region ctor
        public MainPage()
        {
            InitializeComponent();

            _commandResponseListView.ItemsSource = _telloCommandReponse;

            _stateReceiver.DatagramReceived += StateReceiver_DatagramReceived;
            Debug.WriteLine($"state receiver listening on port 8890");

            _frameServer.FrameReady += _frameServer_FrameReady;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            InitializeVideo();
        }
        #endregion

        #region video
        private bool _videoInitialized = false;
        private void InitializeVideo()
        {
            if (!_videoInitialized)
            {
                _videoInitialized = true;

                var vep = VideoEncodingProperties.CreateH264();
                //vep.Bitrate = 3750000;
                vep.Height = 720;
                vep.Width = 960;

                var mss = new MediaStreamSource(new VideoStreamDescriptor(vep))
                {
                    // never turn live on
                    //IsLive = true,
                    BufferTime = TimeSpan.FromSeconds(0.0)
                };

                mss.SampleRequested += Mss_SampleRequested;
                mss.Starting += Mss_Starting;
                mss.Closed += Mss_Closed;
                //mss.SampleRendered += Mss_SampleRendered;
                //mss.SwitchStreamsRequested += Mss_SwitchStreamsRequested;

                _mediaElement.SetMediaStreamSource(mss);
                //_mediaElement.BufferingProgressChanged += _mediaElement_BufferingProgressChanged;
                // never turn real time playback on
                //_mediaElement.RealTimePlayback = true;

                Debug.WriteLine("media element initialized");
            }
        }

        //private void _mediaElement_BufferingProgressChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    Debug.WriteLine($"\n_mediaElement_BufferingProgressChanged: {(100 *_mediaElement.BufferingProgress).ToString("F1")}%");
        //}

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

        private TimeSpan _mediaPosition = TimeSpan.FromSeconds(0);
        private readonly TimeSpan _frameTimeout = TimeSpan.FromSeconds(5);
        private Stopwatch _sampleWatch = new Stopwatch();
        private long _sampleRequestCount = 0;

        private async void Mss_SampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            if (!_sampleWatch.IsRunning)
            {
                _sampleWatch.Start();
            }

            //Debug.Write("+");

            // test flush
            //var frames = _frameServer.ReadAllFrames();
            //if (frames != null )
            //{
            //    Debug.Write($"F:{frames.Count}");
            //    args.Request.Sample = MediaStreamSample.CreateFromBuffer(frames.Content.AsBuffer(), frames.TimeIndex);
            //    args.Request.Sample.Duration = frames.Duration;
            //}
            //else
            //{
            //    Debug.Write($"NULL");
            //}

            // test multiple frames
            //var timeout = TimeSpan.FromMilliseconds(_frameTimeout.TotalMilliseconds * 5);
            //var stopwatch = Stopwatch.StartNew();
            //var frames = _frameServer.ReadFrames(args.Request, timeout, 5);
            //args.Request.Sample = MediaStreamSample.CreateFromBuffer(frames.Content.AsBuffer(), frames.TimeIndex);
            //args.Request.Sample.Duration = frames.Duration;
            //if (stopwatch.Elapsed > timeout)
            //{
            //    Debug.Write($" TO: {stopwatch.ElapsedMilliseconds.ToString("#,#")}ms ");
            //}

            // test single framees
            //var stopwatch = Stopwatch.StartNew();

            var sample = _frameServer.GetSample(_frameTimeout);
            if (sample != null && sample.Count > 0)
            {
                //Debug.Write("T");
                args.Request.Sample = MediaStreamSample.CreateFromBuffer(sample.Content.AsBuffer(), sample.TimeIndex);
                args.Request.Sample.Duration = sample.Duration;
                if (_sampleRequestCount % 32 == 0)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        _mediaPosition = _mediaElement.Position;
                        _mediaElement.Position = sample.TimeIndex;
                    });

                    Debug.WriteLine($"\nRQT: {_sampleWatch.Elapsed} - STI: {sample.TimeIndex} - MEP: {_mediaPosition}, RQCNT: {_sampleRequestCount}, SCNT: {sample.Count}, SDUR: {sample.Duration}, {(uint)(_sampleRequestCount / _sampleWatch.Elapsed.TotalSeconds)}RQ/s");
                }
            }
            ++_sampleRequestCount;

            //if (_frameServer.TryReadFrame(out var frame, _frameTimeout))
            //{
            //    //Debug.Write("T");
            //    args.Request.Sample = MediaStreamSample.CreateFromBuffer(frame.Content.AsBuffer(), frame.TimeIndex);
            //    args.Request.Sample.Duration = frame.Duration;
            //    if (frame.Index % (32 * 5) == 0)
            //    {
            //        Debug.WriteLine($"\nSR {_sampleWatch.Elapsed} - {frame.TimeIndex}: F#{frame.Index}, R#{_sampleRequestCount}, {(uint)(_sampleRequestCount / _sampleWatch.Elapsed.TotalSeconds)}R/s");
            //    }
            //}
            //++_sampleRequestCount;
            //else
            //{
            //    Debug.Write("F");
            //    if (stopwatch.Elapsed > _frameTimeout)
            //    {
            //        Debug.Write($" TO: {stopwatch.ElapsedMilliseconds.ToString("#,#")}ms ");
            //    }
            //}

            //Debug.Write("-");
        }
        #endregion

        #region tello state
        private async void StateReceiver_DatagramReceived(object sender, DatagramReceivedArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { _telloStateText.Text = $"TELLO STATE: {Encoding.UTF8.GetString(e.Datagram)}"; });
        }
        #endregion

        #region tello commands

        private ObservableCollection<string> _telloCommandReponse = new ObservableCollection<string>();

        private bool _receivingVideo = false;

        private void ShowMessage(string message)
        {
            //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            //{
                _telloCommandReponse.Insert(0, message);
            //});
        }

        private async void SendMessage(string message, Action<Response> onSuccess = null)
        {
            ShowMessage($"sending: {message}");

            var response = await _tello.SendAsync(Encoding.ASCII.GetBytes(message));
            if (response.IsSuccess)
            {
                var responseMessage = response.GetMessage().ToLowerInvariant();
                if (responseMessage == "ok")
                {
                    ShowMessage($"success: {message} in {response.ElapsedMS}ms, {responseMessage}");
                    onSuccess?.Invoke(response);
                }
                else
                {
                    ShowMessage($"fail: {message} returned message {responseMessage}");
                }
            }
            else
            {
                ShowMessage($"fail: {message} with message {response.Message}");
            }
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

            SendMessage("command", (response) =>
            {
                _stateReceiver.Start();
                _frameServer.Start();
            });
        }

        private void _takeoffButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SendMessage("takeoff");
        }

        private void _landButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SendMessage("land");
        }

        private void _goButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SendMessage("takeoff");
            SendMessage("forward 100");
            SendMessage("left 100");
            SendMessage("back 100");
            SendMessage("right 100");
            SendMessage("time?");
            SendMessage("land");
        }

        private void _startVideoButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _telloCommandReponse.Insert(0, "sending 'streamon' (start video) command");
            SendMessage("streamon", (response) =>
            {
                _receivingVideo = true;
            });
        }

        private void _stopVideoButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _mediaElement.Stop();
            _telloCommandReponse.Insert(0, "sending 'streamoff' (stop video) command");
            SendMessage("streamoff", (response) =>
            {
                _receivingVideo = false;
                _mediaElement.Stop();
            });
        }

        private void _checkBatteryButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SendMessage("battery?");
        }
        #endregion

    }
}
