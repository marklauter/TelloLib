using System;

namespace Tello.Core
{
    public struct EulerAngle : IEquatable<EulerAngle>
    {
        public EulerAngle(double roll, double pitch, double yaw, AngleTypes angleType)
        {
            Roll = X = roll;
            Pitch = Y = pitch;
            Yaw = Z = yaw;
            AngleType = angleType;
        }

        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        public readonly double Roll;
        public readonly double Pitch;
        public readonly double Yaw;

        public readonly AngleTypes AngleType;

        public EulerAngle ToRadians()
        {
            return AngleType == AngleTypes.Radians ? this :
                new EulerAngle(
                    Yaw / (180 / Math.PI),
                    Pitch / (180 / Math.PI),
                    Roll / (180 / Math.PI),
                    AngleTypes.Radians);
        }

        public EulerAngle ToDegrees()
        {
            return AngleType == AngleTypes.Degrees ? this :
                new EulerAngle(
                    Yaw * (180 / Math.PI),
                    Pitch * (180 / Math.PI),
                    Roll * (180 / Math.PI),
                    AngleTypes.Degrees);
        }

        public bool Equals(EulerAngle other)
        {
            return Yaw == other.Yaw && Pitch == other.Pitch && Roll == other.Roll;
        }
    }
}
