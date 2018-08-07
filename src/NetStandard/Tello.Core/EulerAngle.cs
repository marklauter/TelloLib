using System;

namespace Tello.Core
{
    public struct EulerAngle : IEquatable<EulerAngle>
    {
        public EulerAngle(double roll, double pitch, double yaw, AngleTypes angleType)
        {
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
            AngleType = angleType;
        }

        public readonly double Roll;
        public readonly double Pitch;
        public readonly double Yaw;

        public readonly AngleTypes AngleType;

        public EulerAngle ToRadians()
        {
            return AngleType == AngleTypes.Radians ? this :
                new EulerAngle(
                    Roll / (180 / Math.PI),
                    Pitch / (180 / Math.PI), 
                    Yaw / (180 / Math.PI),
                    AngleTypes.Radians);
        }

        public EulerAngle ToDegrees()
        {
            return AngleType == AngleTypes.Degrees ? this :
                new EulerAngle(
                    Roll * (180 / Math.PI),
                    Pitch * (180 / Math.PI),
                    Yaw * (180 / Math.PI),
                    AngleTypes.Degrees);
        }

        //Quaterniond toQuaternion(double pitch, double roll, double yaw)
        //{
        //    Quaterniond q;
        //    // Abbreviations for the various angular functions
        //    double cy = cos(yaw * 0.5);
        //    double sy = sin(yaw * 0.5);
        //    double cr = cos(roll * 0.5);
        //    double sr = sin(roll * 0.5);
        //    double cp = cos(pitch * 0.5);
        //    double sp = sin(pitch * 0.5);

        //    q.w() = cy * cr * cp + sy * sr * sp;
        //    q.x() = cy * sr * cp - sy * cr * sp;
        //    q.y() = cy * cr * sp + sy * sr * cp;
        //    q.z() = sy * cr * cp - cy * sr * sp;
        //    return q;
        //}

        public bool Equals(EulerAngle other)
        {
            return Yaw == other.Yaw && Pitch == other.Pitch && Roll == other.Roll;
        }
    }
}
