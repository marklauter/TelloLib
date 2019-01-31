using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tello.Udp;

namespace Tello.Core
{
    public enum ConnectionStates
    {
        Disconnected,
        Connecting,
        Connected,
    }

    //todo: finish adding all tello commands
    public class FlightController : IDisposable
    {
        public delegate void ConnectionStageChangingDeligate(ConnectionStates oldState, ConnectionStates newState);
        public static event ConnectionStageChangingDeligate ConnectionStateChanging;

        public FlightController()
        {
            _client = new UdpTransceiver(IP, PORT, TimeSpan.FromSeconds(10));
        }

        private const string IP = "192.168.10.1";
        private const int PORT = 8889;

        private readonly UdpTransceiver _client;
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

        /// <summary>
        /// establish connection and command session with tello
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            if (ConnectionState == ConnectionStates.Disconnected)
            {
                ConnectionState = ConnectionStates.Connecting;
                _client.Connect();

                var response = await _client.SendAsync(Encoding.UTF8.GetBytes("command"));
                if (response.IsSuccess)
                {
                    var message = response.GetMessage();
                    if (message == "ok")
                    {
                        ConnectionState = ConnectionStates.Connected;
                    }
                    else
                    {
                        ConnectionState = ConnectionStates.Disconnected;
                    }
                }
                else
                {
                    ConnectionState = ConnectionStates.Disconnected;
                }
            }
            return ConnectionState == ConnectionStates.Connected;
        }

        public void Disconnect()
        {
            if (ConnectionState != ConnectionStates.Disconnected)
            {
                ConnectionState = ConnectionStates.Disconnected;
                _client.Disconnect();
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
