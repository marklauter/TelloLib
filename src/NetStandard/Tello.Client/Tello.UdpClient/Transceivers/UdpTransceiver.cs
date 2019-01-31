using Sumo.Retry;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tello.Udp
{

    public class UdpTransceiver : IDisposable
    {
        public UdpTransceiver(string ip, int port, TimeSpan timeout) : base()
        {
            if (String.IsNullOrEmpty(ip))
            {
                throw new ArgumentNullException(nameof(ip));
            }

            _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Destination = $"{ip}:{port}";

            _timeout = timeout;
        }

        private readonly FilterRetryPolicy _connectionRetryPolicy =
            new FilterRetryPolicy(120, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(1), new Type[] { typeof(NetworkUnavailableException) });

        private readonly IPEndPoint _endPoint;
        private readonly TimeSpan _timeout;
        private UdpClient _client = null;

        public event EventHandler Connected;
        public event EventHandler Disconnected;
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
                    _client.Connect(_endPoint);
                });
                Connected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
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
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsNetworkAvailable => NetworkInterface.GetIsNetworkAvailable();
        public bool IsConnected { get; private set; }

        public class Response
        {
            internal void SetReceiveResult(UdpReceiveResult result)
            {
                Datagram = result.Buffer;
                RemoteEndpoint = result.RemoteEndPoint;
                IsSuccess = true;
                Message = "ok";
            }

            public byte[] Datagram { get; private set; }

            public string GetString()
            {
                return Encoding.UTF8.GetString(Datagram);
            }

            public IPEndPoint RemoteEndpoint { get; private set; }

            public long ElapsedMS { get; internal set; }

            public bool IsSuccess { get; internal set; } = false;

            /// <summary>
            /// check message when IsSucess == false
            /// </summary>
            public string Message { get; internal set; }
            public Exception Exception { get; internal set; }
        }

        public Task<Response> SendAsync(byte[] datagram)
        {
            return Task.Run(async () =>
            {
                var response = new Response();

                try
                {
                    if (_client == null || !IsConnected)
                    {
                        response.Message = "not connected";
                        return response;
                    }

                    var spinWait = new SpinWait();
                    var timer = Stopwatch.StartNew();

                    await _client.SendAsync(datagram, datagram.Length);

                    while (_client.Available == 0 && timer.Elapsed <= _timeout)
                    {
                        spinWait.SpinOnce();
                    }

                    response.ElapsedMS = timer.ElapsedMilliseconds;

                    if (_client.Available == 0)
                    {
                        response.Message = "timed out";
                        return response;
                    }

                    response.SetReceiveResult(await _client.ReceiveAsync());
                }
                catch(Exception ex)
                {
                    response.Exception = ex;
                    response.Message = $"{ex.GetType().Name} - {ex.Message}";
                }
                return response;
            });
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
