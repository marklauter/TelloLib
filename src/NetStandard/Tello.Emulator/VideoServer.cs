namespace Tello.Emulator
{
    internal sealed class VideoServer : UdpServer
    {
        protected override byte[] GetDatagram()
        {
            //todo: read video from camera or stream or something
            //todo: to really emulate the tello the samples need to be the right size and time apart
            return new byte[100];
        }
    }
}
