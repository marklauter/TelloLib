using System.Text;
using System.Threading.Tasks;

namespace Tello.Emulator
{
    internal sealed class StateServer : UdpServer
    {
        public StateServer(DroneState droneState)
        {
            _droneState = droneState ?? throw new System.ArgumentNullException(nameof(droneState));
        }

        private readonly DroneState _droneState;

        protected override byte[] GetDatagram()
        {
            // 10Hz state reporting
            Task.Delay(100);
            return Encoding.UTF8.GetBytes(_droneState.ToString());
        }
    }
}
