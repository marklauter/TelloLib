using System;

namespace Tello.Emulator.SDKV2
{
    internal class CommandInterpreter
    {
        public CommandInterpreter(DroneState droneState, VideoServer videoServer, StateServer stateServer, ILog log)
        {
            _droneState = droneState ?? throw new ArgumentNullException(nameof(droneState));
            _videoServer = videoServer ?? throw new ArgumentNullException(nameof(videoServer));
            _stateServer = stateServer ?? throw new ArgumentNullException(nameof(stateServer));
            _log = log;
        }

        private readonly ILog _log;
        private readonly DroneState _droneState;
        private readonly VideoServer _videoServer;
        private readonly StateServer _stateServer;

        private bool _inSDKMode = false;
        private bool _inMissionPadMode = false;
        private bool _flying = false;
        private DateTime _takeoffTime = DateTime.Now;

        private readonly string _ok = "ok";
        private readonly string _error = "error";

        public string Interpret(string message)
        {
            if (!_inSDKMode && message != "command")
            {
                Log($"{nameof(CommandInterpreter)} - not in SDK mode. message ignored: {message}");
                return null;
            }
            Log($"{nameof(CommandInterpreter)} - message received: {message}");

            try
            {
                var command = CommandParser.GetCommand(message);
                Log($"{nameof(CommandInterpreter)} - command identified: {command}");
                switch (command)
                {
                    case Commands.EnterSdkMode:
                        if (!_inSDKMode)
                        {
                            _inSDKMode = true;
                            _stateServer.Start();
                            return _ok;
                        }
                        return null;
                    case Commands.Takeoff:
                        _takeoffTime = DateTime.Now;
                        _droneState.Height = 20;
                        _flying = true;
                        return _ok;
                    case Commands.Land:
                        _droneState.Height = 0;
                        _flying = false;
                        return _ok;
                    case Commands.StartVideo:
                        _videoServer.Start();
                        return _ok;
                    case Commands.StopVideo:
                        _videoServer.Stop();
                        return _ok;
                    case Commands.Stop:
                    case Commands.EmergencyStop:
                        return _ok;
                    case Commands.Up:
                    {
                        var args = CommandParser.GetArgs(message);
                        if (args.Length != 1)
                        {
                            return _error;
                        }

                        _droneState.Height += Int32.Parse(args[0]);
                        return _ok;
                    }
                    case Commands.Down:
                    {
                        var args = CommandParser.GetArgs(message);
                        if (args.Length != 1)
                        {
                            return _error;
                        }

                        _droneState.Height -= Int32.Parse(args[0]);
                        if (_droneState.Height < 0)
                        {
                            _droneState.Height = 0;
                        }
                        return _ok;
                    }
                    case Commands.Left:
                    case Commands.Right:
                    case Commands.Forward:
                    case Commands.Back:
                    case Commands.ClockwiseTurn:
                    case Commands.CounterClockwiseTurn:
                    case Commands.Flip:
                    {
                        var args = CommandParser.GetArgs(message);
                        if (args.Length != 1)
                        {
                            return _error;
                        }
                        // there's no state to manage for horizontal movements
                        return _ok;
                    }
                    case Commands.Go:
                    {
                        var args = CommandParser.GetArgs(message);
                        if (args.Length != 4)
                        {
                            return _error;
                        }
                        // there's no state to manage for horizontal movements
                        return _ok;
                    }
                    case Commands.Curve:
                    {
                        var args = CommandParser.GetArgs(message);
                        if (args.Length != 7)
                        {
                            return _error;
                        }
                        // there's no state to manage for horizontal movements
                        return _ok;
                    }
                    case Commands.SetSpeed:
                    {
                        var args = CommandParser.GetArgs(message);
                        if (args.Length != 1)
                        {
                            return _error;
                        }
                        var speed = Int32.Parse(args[0]);
                        if (speed < 10 || speed > 100)
                        {
                            return _error;
                        }
                        _droneState.Speed = speed;
                        return _ok;
                    }
                    case Commands.SetRemoteControl:
                    {
                        var args = CommandParser.GetArgs(message);
                        if (args.Length != 4)
                        {
                            return _error;
                        }
                        if (Int32.Parse(args[0]) < -100 || Int32.Parse(args[0]) > 100
                            || Int32.Parse(args[1]) < -100 || Int32.Parse(args[1]) > 100
                            || Int32.Parse(args[2]) < -100 || Int32.Parse(args[2]) > 100
                            || Int32.Parse(args[3]) < -100 || Int32.Parse(args[3]) > 100)
                        {
                            return _error;
                        }

                        _droneState.Height += Int32.Parse(args[2]);
                        if (_droneState.Height < 0)
                        {
                            _droneState.Height = 0;
                        }
                        return _ok;
                    }
                    case Commands.SetWiFiPassword:
                    {
                        var args = CommandParser.GetArgs(message);
                        if (args.Length != 2)
                        {
                            return _error;
                        }
                        return _ok;
                    }
                    case Commands.SetMissionPadOn:
                        //if (_droneState.MissionPadDected == -1)
                        //{
                        //    return _error;
                        //}
                        _inMissionPadMode = true;
                        return _ok;
                    case Commands.SetMissionPadOff:
                        _inMissionPadMode = false;
                        return _ok;
                    case Commands.SetMissionPadDirection:
                        if (!_inMissionPadMode)
                        {
                            return null;
                        }
                        else
                        {
                            var args = CommandParser.GetArgs(message);
                            if (args.Length != 1)
                            {
                                return _error;
                            }
                            var direction = Int32.Parse(args[0]);
                            if (direction < 0 || direction > 2)
                            {
                                return _error;
                            }
                            return _ok;
                        }
                    case Commands.SetStationMode:
                    {
                        var args = CommandParser.GetArgs(message);
                        if (args.Length != 2)
                        {
                            return _error;
                        }
                        return _ok;
                    }
                    case Commands.GetSpeed:
                        return _droneState.Speed.ToString();
                    case Commands.GetBattery:
                        return _droneState.BatteryPercentage.ToString();
                    case Commands.GetTime:
                        //todo: see what format the tello returns
                        if (_flying)
                        {
                            return (DateTime.Now - _takeoffTime).ToString("hh:mm:ss");
                        }
                        return "00:00:00";
                    case Commands.GetWiFiSnr:
                        return "snr";
                    case Commands.GetSdkVersion:
                        return "2.0 emulated";
                    case Commands.GetSerialNumber:
                        return "tello emulator";
                    default:
                        return _error;
                }
            }
            catch (Exception ex)
            {
                Log($"{nameof(CommandInterpreter)} - {ex}");
                return _error;
            }
        }

        public void Log(string meesage)
        {
            if (_log != null)
            {
                _log.WriteLine(meesage);
            }
        }
    }
}
