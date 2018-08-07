using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Tello.Core;

namespace Test.Tello.Core
{
    [TestClass]
    public class Quaternion_Test
    {
        // test cases : http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/steps/index.htm
        [TestMethod]
        public void ToEulerAngle()
        {
            var quaternion = new Quaternion(1F, 0.0F, 0.0F, 0.0F);
            var eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.7071F, 0.0F, 0.7071F, 0.0F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.0F, 0.0F, 1F, 0.0F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(180, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.7071F, 0.0F, -0.7071F, 0.0F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(-90, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.7071F, 0.0F, 0.0F, 0.7071F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.5F, 0.5F, 0.5F, 0.5F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.0F, 0.7071F, 0.7071F, 0.0F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(180.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.5F, -0.5F, -0.5F, 0.5F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.7071F, 0.0F, 0.0F, -0.7071F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.5F, -0.5F, 0.5F, -0.5F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.0F, -0.7071F, 0.7071F, 0.0F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(180.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.5F, 0.5F, -0.5F, -0.5F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.7071F, 0.7071F, 0.0F, 0.0F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.5F, 0.5F, 0.5F, -0.5F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.0F, 0.0F, 0.7071F, -0.7071F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(180.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.5F, 0.5F, -0.5F, 0.5F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Roll, 0));
            
            quaternion = new Quaternion(0.0F, 1.0F, 0.0F, 0.0F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(180.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.0F, 0.7071F, 0.0F, -0.7071F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(180.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.0F, 0.0F, 0.0F, 1.0F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(180.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(180.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.0F, 0.7071F, 0.0F, 0.7071F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(180.0, Math.Round(eulerAngle.Roll, 0));
            
            quaternion = new Quaternion(0.7071F, -0.7071F, 0.0F, 0.0F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.5F, -0.5F, 0.5F, 0.5F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(90.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.0F, 0.0F, 0.7071F, 0.7071F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(180.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Roll, 0));

            quaternion = new Quaternion(0.5F, -0.5F, -0.5F, -0.5F);
            eulerAngle = quaternion.ToEulerAngle().ToDegrees();
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Yaw, 0));
            Assert.AreEqual(0.0, Math.Round(eulerAngle.Pitch, 0));
            Assert.AreEqual(-90.0, Math.Round(eulerAngle.Roll, 0));
        }



    }
}

