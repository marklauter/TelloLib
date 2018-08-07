﻿using System;

namespace Tello.Core
{
    public struct Quaternion : IEquatable<Quaternion>
    {
        public Quaternion(float w, float x, float y, float z)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }

        public readonly float W;
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public bool Equals(Quaternion other)
        {
            return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        }

        //https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
        //http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
        public EulerAngle ToEulerAngle()
        {
            double sqw = W * W;
            double sqx = X * X;
            double sqy = Y * Y;
            double sqz = Z * Z;

            double unit = sqw + sqx + sqy + sqz;
            double test = X * Y + Z * W;

            var pole = test > (0.499 * unit) ? 1 : test < (-0.499 * unit) ? -1 : 0;

            switch (pole)
            {
                case 0:
                default:
                    {
                        // roll = x-axis
                        var roll = Math.Atan2(
                            2.0 * (W * X + Y * Z),
                            1.0 - 2.0 * (X * X + Y * Y));

                        // pitch = y-axis
                        var sinp = 2.0 * (W * Y - Z * X);
                        var pitch = Math.Abs(sinp) >= 1 ? Math.Sign(sinp) * Math.PI / 2 : Math.Asin(sinp);

                        // yaw = z-axis
                        var yaw = Math.Atan2(
                            2.0 * (W * Z + X * Y),
                            1.0 - 2.0 * (Y * Y - Z * Z));

                        return new EulerAngle(roll, pitch, yaw, AngleTypes.Radians);
                    }
                case 1: // north pole
                    {
                        // roll = x-axis
                        var roll = 0;

                        // pitch = y-axis
                        var pitch = Math.PI / 2;

                        // yaw = z-axis
                        var yaw = 2.0 * Math.Atan2(X, W);

                        return new EulerAngle(roll, pitch, yaw, AngleTypes.Radians);
                    }
                case -1: // south pole
                    {
                        // roll = x-axis
                        var roll = 0;

                        // pitch = y-axis
                        var pitch = -(Math.PI / 2);

                        // yaw = z-axis
                        var yaw = -2.0 * Math.Atan2(X, W);

                        return new EulerAngle(roll, pitch, yaw, AngleTypes.Radians);
                    }
            }
        }
    }
}
