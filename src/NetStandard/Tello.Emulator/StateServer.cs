using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Tello.Emulator.SDKV2
{
    internal sealed class StateServer : UdpServer
    {
        public StateServer(int port, DroneState droneState) : base(port)
        {
            _droneState = droneState ?? throw new System.ArgumentNullException(nameof(droneState));
        }

        private readonly DroneState _droneState;

        protected async override Task<byte[]> GetDatagram()
        {
            // 5Hz state reporting
            await Task.Delay(200);
            var state = _droneState.ToString();
            //Debug.WriteLine(state);
            return Encoding.UTF8.GetBytes(state);
        }
    }
}
