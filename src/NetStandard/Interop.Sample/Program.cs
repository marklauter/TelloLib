using System;
using System.Runtime.InteropServices;

namespace Interop.Sample
{ 
    class Program
    {
        [DllImport("/cpp/InteropSampleCpp", SetLastError = true)]
        static extern void test([MarshalAs(UnmanagedType.LPStr)]string data);

        static void Main(string[] args)
        {
            Console.WriteLine("Enter some text and the DLL will write it to the console.");
            var data = Console.ReadLine();
            Console.WriteLine("You entered:");
            test(data);
            Console.WriteLine("Press Enter to quit.");
            Console.ReadKey();
        }
    }
}
