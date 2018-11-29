using System;
using System.Collections.Concurrent;
using System.Text;
using Tello.Udp;
using System.Linq;

namespace Tello.Core
{
    public enum ConnectionStates
    {
        Disconnected,
        Connecting,
        Connected,
    }

    public class TelloClient : IDisposable
    {
        public delegate void ConnectionStageChangingDeligate(ConnectionStates oldState, ConnectionStates newState);
        public static event ConnectionStageChangingDeligate ConnectionStateChanging;

        public TelloClient()
        {
            _client = new Client(IP, PORT);
            _client.ResponseReceived += _client_ResponseReceived;
        }

        private const string IP = "192.168.10.1";
        private const int PORT = 8889;
        private readonly Client _client;
        private readonly ConcurrentDictionary<Guid, Request> _requests = new ConcurrentDictionary<Guid, Request>();
        private ConnectionStates _connectionState = ConnectionStates.Disconnected;
        public ConnectionStates ConnectionState
        {
            get => _connectionState;
            private set
            {
                if (_connectionState != value)
                {
                    ConnectionStateChanging?.Invoke(ConnectionState, value);
                    _connectionState = value;
                }
            }
        }

        public void Connect()
        {
            ConnectionState = ConnectionStates.Connecting;
            _client.Connect();

            var request = RequestFactory.GetRequest(Commands.ConnectionRequest);
            _requests.TryAdd(request.Id, request);
            _client.Send(request);
        }

        private void _client_ResponseReceived(object sender, ResponseReceivedArgs e)
        {
            _requests.TryGetValue(e.Request.Id, out var request);
            if (!request.Datagram.SequenceEqual(e.Request.Datagram))
            {
                throw new Exception("request/response mismatch");
            }

            var command = (Commands)e.Request.UserData;
            switch (command)
            {
                case Commands.ConnectionRequest:
                    if (ConnectionState == ConnectionStates.Connecting)
                    {
                        var message = Encoding.UTF8.GetString(e.Response.Datagram);
                        if (message.StartsWith("conn_ack"))
                        {
                            ConnectionState = ConnectionStates.Connected;
                        }
                    }
                    break;
                case Commands.TakeOff:
                    break;
                case Commands.Land:
                    break;
                default:
                    break;
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
