using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
        private static void Main(string[] args)
        {
            Console.WriteLine("Tello H.264 Video Recorder based on SDK 2.0");
            Console.WriteLine("Video and sample meta data is useful for the Tello Emulator found here: ");
            Console.WriteLine("https://github.com/marklauter/TelloLib");
            Console.WriteLine("press 'q' to quit");

            IPEndPoint endpoint = null;

            var videoFile = new FileStream("tello.video", FileMode.OpenOrCreate);
            var sampleFile = new FileStream("tello.samples.json", FileMode.OpenOrCreate);

            var sampleOffset = 0L;
            var stopwatch = new Stopwatch();

            var cki = Console.Read();
            sampleFile.Write(Encoding.UTF8.GetBytes("["), 0, 1);
            using (var client = new UdpClient(11111))
            {
                while (cki != 13)
                {
                    var datagram = client.Receive(ref endpoint);
                    if (!stopwatch.IsRunning)
                        stopwatch.Start();

                    videoFile.Write(datagram);
                    var sample = new Sample
                    {
                        IsFrameStart = datagram.Length > 4 && datagram[0] == 0x00 && datagram[1] == 0x00 && datagram[2] == 0x00 && datagram[3] == 0x01,
                        Offset = sampleOffset,
                        Length = datagram.Length,
                        TimeIndex = stopwatch.Elapsed
                    };
                    var sampleBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sample));
                    sampleFile.Write(sampleBytes, 0, sampleBytes.Length);

                    sampleOffset += datagram.Length;

                    cki = Console.Read();
                    Console.Write($"\r{stopwatch.Elapsed} - {cki}");
                }
            }
            sampleFile.Write(Encoding.UTF8.GetBytes("]"), 0, 1);
        }
    }
}
