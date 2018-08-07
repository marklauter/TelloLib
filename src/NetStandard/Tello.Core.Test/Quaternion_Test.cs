using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Tello.Core;

namespace Test.Tello.Core
{
    [TestClass]
    public class Quaternion_Test
    {
        // to verify angles https://quaternions.online/
        [TestMethod]
        public void ToEulerAngle_ZValue()
        {
            var quaternion = new Quaternion(0.0F, 0.0F, 0.0F, 1.0F);

            var flyData = new FlyData()
            {
                Quaternion = quaternion
            };

            var result = flyData.toEuler();

            var eulerRadians = quaternion.ToEulerAngle();
            Assert.AreEqual(AngleTypes.Radians, eulerRadians.AngleType);
            var eulerDegrees = eulerRadians.ToDegrees();
            Assert.AreEqual(180.0, Math.Round(eulerDegrees.Z, 0));

        }

        [TestMethod]
        public void ToEulerAngle_YValue()
        {
            var quaternion = new Quaternion(0.707F, 0.0F, 0.707F, 0.0F);
            var eulerRadians = quaternion.ToEulerAngle();
            Assert.AreEqual(AngleTypes.Radians, eulerRadians.AngleType);
            var eulerDegrees = eulerRadians.ToDegrees();
            Assert.AreEqual(90.0, Math.Round(eulerDegrees.Y, 0));
        }

        [TestMethod]
        public void ToEulerAngle_XValue()
        {
            var quaternion = new Quaternion(0.707F, 1.707F, 0.0F, 0.0F);
            var eulerRadians = quaternion.ToEulerAngle();
            Assert.AreEqual(AngleTypes.Radians, eulerRadians.AngleType);
            var eulerDegrees = eulerRadians.ToDegrees();
            Assert.AreEqual(180.0, Math.Round(eulerDegrees.X, 0));
        }


    }
}
