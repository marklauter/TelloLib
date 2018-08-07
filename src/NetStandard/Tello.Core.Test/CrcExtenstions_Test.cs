using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Tello.Core;

namespace Test.Tello.Core
{
    [TestClass]
    public class CrcExtenstions_Test
    {
        [TestMethod]
        public void FCS16_NewVsOld()
        {
            var helloWorld = Encoding.ASCII.GetBytes("Hello, world!");
            var bytes = new byte[helloWorld.Length + 2];
            for (var i = 0; i < helloWorld.Length; ++i) bytes[i] = helloWorld[i];

            var newFCS16 = bytes.CaclulateFCS16(bytes.Length - 2);
            Assert.AreEqual(32723, newFCS16);
        }

        [TestMethod]
        public void UCrc_NewVsOld()
        {
            var helloWorld = Encoding.ASCII.GetBytes("Hello, world!");
            var bytes = new byte[helloWorld.Length + 2];

            var newUCRC = bytes.CalculateUCRC(4);
            Assert.AreEqual(46, newUCRC);
        }
    }
}
