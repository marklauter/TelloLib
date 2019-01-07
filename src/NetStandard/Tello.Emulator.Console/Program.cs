using System;
using System.Diagnostics;
using Tello.Emulator.SDKV2;

namespace Tello.EmulatorConsole
{
    class Log : ILog
    {
        public void Write(string message)
        {
            Console.Write(message);
            Debug.Write(message);
        }

        public void WriteLine(string message)
        {
            Console.WriteLine(message);
            Debug.WriteLine(message);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Tello SDK V2.0 Emulator");
            var drone = new Drone(new Log());
            drone.PowerOn();

            Console.WriteLine("press any key to quit");
            var key = Console.ReadKey();

            drone.PowerOff();
        }
    }
}
