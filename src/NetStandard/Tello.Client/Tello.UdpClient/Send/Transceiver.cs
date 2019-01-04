using Sumo.Retry;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Tello.Udp
{
    public class Transceiver : IDisposable
    {
        public Transceiver(string ip, int port) : base()
        {
            if (String.IsNullOrEmpty(ip))
            {
                throw new ArgumentNullException(nameof(ip));
            }

            _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Destination = $"{ip}:{port}";
        }

        private readonly FilterRetryPolicy _connectionRetryPolicy =
            new FilterRetryPolicy(120, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(1), new Type[] { typeof(NetworkUnavailableException) });

        private readonly IPEndPoint _endPoint;
        private UdpClient _client = null;

        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;
        public event EventHandler<ResponseReceivedArgs> ResponseReceived;
        public string Destination { get; }

        public async void Connect()
        {
            if (IsConnected)
            {
                return;
            }

            IsConnected = true;
            try
            {
                await WithRetry.InvokeAsync(_connectionRetryPolicy, () =>
                {
                    if (!IsNetworkAvailable)
                    {
                        Debug.WriteLine("NetworkUnavailableException");
                        throw new NetworkUnavailableException();
                    }

                    _client = new UdpClient();
                    //{
                    //    ExclusiveAddressUse = false
                    //};
                    _client.Connect(_endPoint);
                });
                OnConnect?.Invoke(this, EventArgs.Empty);
            }
            catch(Exception ex)
            {
                IsConnected = false;
                Debug.WriteLine(ex);
                throw;
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                if (_client != null)
                {
                    _client.Close();
                    _client.Dispose();
                    _client = null;
                }
                IsConnected = false;
                OnDisconnect?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsNetworkAvailable => NetworkInterface.GetIsNetworkAvailable();
        public bool IsConnected { get; private set; }

        private class ReceiverState
        {
            internal ReceiverState(UdpClient client, IPEndPoint endPoint, Request request, DateTime sentTime)
            {
                Client = client ?? throw new ArgumentNullException(nameof(client));
                EndPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
                Request = request ?? throw new ArgumentNullException(nameof(request));
            }

            internal UdpClient Client { get; }
            internal IPEndPoint EndPoint { get; }
            internal DateTime SentTime { get; }
            internal Request Request { get; }
        }

        public async void Send(Request request)
        {
            if (_client != null && IsConnected)
            {
                var state = new ReceiverState(_client, _endPoint, request, DateTime.Now);
                await _client.SendAsync(request.Datagram, request.Datagram.Length);
                _client.BeginReceive(OnReceive, state);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            if (!_isDisposed && IsConnected)
            {
                var state = (ReceiverState)ar.AsyncState;
                if (state.Client != null && state.Client.Client != null)
                {
                    var request = state.Request;
                    var endpoint = new IPEndPoint(IPAddress.Any, 0);
                    var response = new Response(state.Request.Id, state.Client.EndReceive(ar, ref endpoint));

                    ResponseReceived?.Invoke(this, new ResponseReceivedArgs(endpoint, state.Request, response, state.SentTime));
                }
            }
        }

        #region IDisposable Support
        private bool _isDisposed = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Disconnect();
                }

                _isDisposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
