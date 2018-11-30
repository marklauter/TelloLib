namespace Tello.Core
{
    public enum Commands
    {
        Connect, // no CRC
        TakeOff,
        ThrowTakeOff,
        Land,
        RequestIFrame, // no CRC
        SetMaxHeight,
        QueryUnk,
    }
}
