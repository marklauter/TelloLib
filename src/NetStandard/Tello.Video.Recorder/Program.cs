using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Tello.Video.Recorder
{
    internal class Sample
    {
        public bool IsFrameStart { get; set; }
        public long Offset { get; set; }
        public int Length { get; set; }
        public TimeSpan TimeIndex { get; set; }
    }

    internal class Program
    {
        static FileStream _videoFile = new FileStream("tello.video", FileMode.OpenOrCreate);
        static FileStream _sampleFile = new FileStream("tello.samples.json", FileMode.OpenOrCreate);
        static long _sampleOffset = 0;
        static Stopwatch _stopwatch = new Stopwatch();

        private static void Main(string[] args)
        {
            Console.WriteLine("Tello H.264 Video Recorder based on SDK 2.0");
            Console.WriteLine("Video and sample meta data is useful for the Tello Emulator found here: ");
            Console.WriteLine("https://github.com/marklauter/TelloLib");

            _sampleFile.Write(Encoding.UTF8.GetBytes("["), 0, 1);
            using (var client = new UdpClient(11111))
            {
                client.BeginReceive(DatagramReceived, client);
                var wait = new SpinWait();
                while (_stopwatch.Elapsed.TotalSeconds <= 30)
                {
                    Console.Write($"\r{_stopwatch.Elapsed}");
                    wait.SpinOnce();
                }
            }
            _sampleFile.Write(Encoding.UTF8.GetBytes("]"), 0, 1);
        }

        static long _sampleCount = 0;
        private static void DatagramReceived(IAsyncResult ar)
        {
            Console.Write($"\r                                   {_sampleCount++}");

            if (!_stopwatch.IsRunning)
                _stopwatch.Start();

            var client = (UdpClient)ar.AsyncState;

            IPEndPoint endpoint = null;
            var datagram = client.EndReceive(ar, ref endpoint);
            client.BeginReceive(DatagramReceived, client);

            _videoFile.Write(datagram);
            var sample = new Sample
            {
                IsFrameStart = datagram.Length > 4 && datagram[0] == 0x00 && datagram[1] == 0x00 && datagram[2] == 0x00 && datagram[3] == 0x01,
                Offset = _sampleOffset,
                Length = datagram.Length,
                TimeIndex = _stopwatch.Elapsed
            };
            var sampleBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sample) + ",");
            _sampleFile.Write(sampleBytes, 0, sampleBytes.Length);

            _sampleOffset += datagram.Length;
        }
    }
}
