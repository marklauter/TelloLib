namespace Tello.Emulator.SDKV2
{
    internal sealed class VideoServer : UdpServer
    {
        public VideoServer(int port) : base(port) { }

        protected override byte[] GetDatagram()
        {
            //todo: read video from camera or stream or something
            //todo: to really emulate the tello the samples need to be the right size and time apart
            return new byte[100];
        }
    }
}
