using System.Diagnostics;
using System.IO;
using System.Text;
using Tello.Udp;
using Windows.Media.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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

        private void VideoReceiver_DatagramReceived(object sender, ReceiverDatagramArgs e)
        {
            _stream.Write(e.Datagram, 0, e.Datagram.Length);
            if (e.Datagram.Length != 1460)
            {
                Debug.WriteLine("frame received");
            }
            foo();
        }

        private static void StateReceiver_DatagramReceived(object sender, ReceiverDatagramArgs e)
        {
            Debug.WriteLine($"TELLO STATE: {Encoding.UTF8.GetString(e.Datagram)}");
        }

        private MemoryStream _stream = new MemoryStream();
        private bool _mediaInitialized = false;
        private void foo()
        {
            if (!_mediaInitialized)
            {
                var mediaSource = MediaSource.CreateFromStream(_stream.AsRandomAccessStream(), "video/mp4");
                _mediaPlayerElement.Source = mediaSource;
            }
        }
    }

}
