using System;
using Tello.Emulator;

namespace Tello.EmulatorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var drone = new Drone();
            drone.PowerOn();

            Console.WriteLine("press any key to quit");
            var key = Console.ReadKey();

            drone.PowerOff();
        }
    }
}
