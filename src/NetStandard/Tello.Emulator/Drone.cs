﻿using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Tello.Udp;

namespace Tello.Emulator
{
    //https://dl-cdn.ryzerobotics.com/downloads/Tello/Tello%20SDK%202.0%20User%20Guide.pdf
    //notes:
    // video reports on 11111
    // state reports on 8890
    // command listener receives on 8889

    public class Drone
    {
        public Drone()
        {
            _udpReceiver = new UdpReceiver(8889);
            _udpReceiver.DatagramReceived += _udpReceiver_DatagramReceived;

            _droneState = new DroneState();
            _stateServer = new StateServer(_droneState);
            _videoServer = new VideoServer();
            _commandInterpreter = new CommandInterpreter(_droneState, _videoServer, _stateServer);
            _batteryTimer = new Timer(UpdateBattery);
        }

        private void UpdateBattery(object state)
        {
            if (_poweredOn)
            {
                _droneState.BatteryPercentage = 100 - (int)((DateTime.Now - _poweredOnTime).TotalMinutes / 15.0 * 100);
                if(_droneState.BatteryPercentage < 1)
                {
                    PowerOff();
                    Debug.WriteLine("battery died");
                }
            }
        }

        private readonly Timer _batteryTimer;
        private bool _poweredOn = false;
        private DateTime _poweredOnTime;
        private readonly UdpReceiver _udpReceiver;
        private readonly DroneState _droneState;
        private readonly VideoServer _videoServer;
        private readonly StateServer _stateServer;
        private readonly CommandInterpreter _commandInterpreter;

        private void _udpReceiver_DatagramReceived(object sender, DatagramReceivedArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Datagram);
            var response = _commandInterpreter.Interpret(message);
            if (!String.IsNullOrEmpty(response))
            {
                e.Reply = Encoding.UTF8.GetBytes(response);
            }
        }

        public void PowerOn()
        {
            if (!_poweredOn)
            {
                _poweredOn = true;
                _udpReceiver.Start();
                _poweredOnTime = DateTime.Now;
            }
        }

        public void PowerOff()
        {
            if (_poweredOn)
            {
                _poweredOn = false;
                _udpReceiver.Stop();
            }
        }
    }
}
