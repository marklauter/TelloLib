using System;
using System.Collections.Generic;
using Tello.Udp;

namespace Tello.Video
{
    public class VideoServer
    {
        public VideoServer()
        {
            _receiver = new Receiver(11111);
            _receiver.DatagramReceived += _receiver_DatagramReceived;
            _receiver.BeginReceiving();
        }

        private readonly Receiver _receiver;
        private void _receiver_DatagramReceived(object sender, ReceiverDatagramArgs e)
        {
            for(var i = 0; i < e.Datagram.Length; ++i)
            {
                HandleByte(e.Datagram[i]);
            }
        }

        private void HandleByte(byte b)
        {
            switch (_state)
            {
                case State.ExpectHeaderFirstZero:
                    if(b == 0x00)
                    {
                        _state = State.ExpectHeaderSecondZero;
                    }
                    break;
                case State.ExpectHeaderSecondZero:
                    break;
                case State.ExpectHeaderThirdZero:
                    break;
                case State.ExpectHeader0x01:
                    break;
                case State.ExpectHeaderType:
                    break;
                case State.ExpectHeaderTwo:
                    break;
                case State.ExpectSizeLSB:
                    break;
                case State.ExpectSizeMSSB:
                    break;
                case State.Payload:
                    break;
                default:
                    break;
            }
        }

        private readonly List<byte> _frameBuffer;

        public event EventHandler<byte[]> FrameReceived;
        private void FrameReady(byte[] frame)
        {
            FrameReceived?.Invoke(this, frame);
        }

        public enum State
        {
            ExpectHeaderFirstZero,
            ExpectHeaderSecondZero,
            ExpectHeaderThirdZero,
            ExpectHeader0x01,

            ExpectHeaderType,
            ExpectHeaderTwo,
            ExpectSizeLSB,
            ExpectSizeMSSB,
            Payload,
        }
        private State _state = State.ExpectHeaderFirstZero;

    }
}
