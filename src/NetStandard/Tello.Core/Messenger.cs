using System;
using System.Net;
using System.Net.Sockets;

namespace Tello.Core
{
    public class Messenger : IDisposable
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly IPEndPoint _endPoint;
        private UdpClient _client = null;

        //#region events
        public event EventHandler<ResponseReceivedArgs> ResponseReceived;
        //public event EventHandler<ConnectionStateChangedArgs> ConnectionStateChanged;
        //#endregion

        public Messenger(string ip, int port)
        {
            if (string.IsNullOrEmpty(ip)) throw new ArgumentNullException(nameof(ip));

            _ip = ip;
            _port = port;
            _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public string Destination => $"{_ip}:{_port}";
        public Int64 Sent { get; private set; }

        public void Connect()
        {
            Disconnect();

            _client = new UdpClient();
            _client.Connect(_endPoint);
        }

        public void Disconnect()
        {
            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
            }
        }

        private bool _isDisposed = false;
        public void Dispose()
        {
            Disconnect();
            _isDisposed = true;
        }

        private class ReceiverState
        {
            public ReceiverState(UdpClient client, IPEndPoint endPoint, Commands command, byte[] request, DateTime sentTime)
            {
                Client = client ?? throw new ArgumentNullException(nameof(client));
                EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
                Request = request ?? throw new ArgumentNullException(nameof(request));
                Command = command;
                SentTime = sentTime;
            }

            public UdpClient Client { get; }
            public IPEndPoint EndPoint { get; }
            public byte[] Request { get; }
            public DateTime SentTime { get; }
            public Commands Command { get; }
        }

        public void SendCommand(Commands command)
        {
            if (_client == null) return;

            var datagram = command.GetDatagram();
            datagram.SetSequence();
            datagram.SetCRC();

            var state = new ReceiverState(_client, _endPoint, command, datagram, DateTime.Now);

            Sent += _client.Send(datagram, datagram.Length);
            _client.BeginReceive(OnReceive, state);
        }
        
        public void OnReceive(IAsyncResult ar)
        {
            if (_isDisposed) return;

            var state = (ReceiverState)ar.AsyncState;
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            var response = state.Client.EndReceive(ar, ref endpoint);

            ResponseReceived?.Invoke(this, new ResponseReceivedArgs(endpoint, state.Command, response, state.Request, state.SentTime));
        }
    }
}
