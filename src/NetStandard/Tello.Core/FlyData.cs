using System;
using System.Text;

namespace Tello.Core
{
    public class FlyData
    {
        public int flyMode;
        public int height;
        public int verticalSpeed;
        public int flySpeed;
        public int eastSpeed;
        public int northSpeed;
        public int flyTime;
        public bool flying;
        public bool downVisualState;
        public bool droneHover;
        public bool eMOpen;
        public bool onGround;
        public bool pressureState;
        public int batteryPercentage;//
        public bool batteryLow;
        public bool batteryLower;
        public bool batteryState;
        public bool powerState;
        public int droneBatteryLeft;
        public int droneFlyTimeLeft;
        public int cameraState;
        public int electricalMachineryState;
        public bool factoryMode;
        public bool frontIn;
        public bool frontLSC;
        public bool frontOut;
        public bool gravityState;
        public int imuCalibrationState;
        public bool imuState;
        public int lightStrength;
        public bool outageRecording;
        public int smartVideoExitMode;
        public int temperatureHeight;
        public int throwFlyTimer;
        public int wifiDisturb;
        public bool windState;

        //From log
        public float velX;
        public float velY;
        public float velZ;

        public float posX;
        public float posY;
        public float posZ;
        public float posUncertainty;

        public float velN;
        public float velE;
        public float velD;

        public Quaternion Quaternion;

        public int WiFiStrength { get; set; }

        public void Update(byte[] telemetry)
        {
            var index = 0;
            height = (Int16)(telemetry[index] | (telemetry[index + 1] << 8)); index += 2;
            northSpeed = (Int16)(telemetry[index] | (telemetry[index + 1] << 8)); index += 2;
            eastSpeed = (Int16)(telemetry[index] | (telemetry[index + 1] << 8)); index += 2;
            flySpeed = ((int)Math.Sqrt(Math.Pow(northSpeed, 2.0D) + Math.Pow(eastSpeed, 2.0D)));
            verticalSpeed = (Int16)(telemetry[index] | (telemetry[index + 1] << 8)); index += 2;// ah.a(paramArrayOfByte[6], paramArrayOfByte[7]);
            flyTime = telemetry[index] | (telemetry[index + 1] << 8); index += 2;// ah.a(paramArrayOfByte[8], paramArrayOfByte[9]);

            imuState = (telemetry[index] >> 0 & 0x1) == 1 ? true : false;
            pressureState = (telemetry[index] >> 1 & 0x1) == 1 ? true : false;
            downVisualState = (telemetry[index] >> 2 & 0x1) == 1 ? true : false;
            powerState = (telemetry[index] >> 3 & 0x1) == 1 ? true : false;
            batteryState = (telemetry[index] >> 4 & 0x1) == 1 ? true : false;
            gravityState = (telemetry[index] >> 5 & 0x1) == 1 ? true : false;
            windState = (telemetry[index] >> 7 & 0x1) == 1 ? true : false;
            index += 1;

            //if (paramArrayOfByte.length < 19) { }
            imuCalibrationState = telemetry[index]; index += 1;
            batteryPercentage = telemetry[index]; index += 1;
            droneFlyTimeLeft = telemetry[index] | (telemetry[index + 1] << 8); index += 2;
            droneBatteryLeft = telemetry[index] | (telemetry[index + 1] << 8); index += 2;

            //index 17
            flying = (telemetry[index] >> 0 & 0x1) == 1 ? true : false;
            onGround = (telemetry[index] >> 1 & 0x1) == 1 ? true : false;
            eMOpen = (telemetry[index] >> 2 & 0x1) == 1 ? true : false;
            droneHover = (telemetry[index] >> 3 & 0x1) == 1 ? true : false;
            outageRecording = (telemetry[index] >> 4 & 0x1) == 1 ? true : false;
            batteryLow = (telemetry[index] >> 5 & 0x1) == 1 ? true : false;
            batteryLower = (telemetry[index] >> 6 & 0x1) == 1 ? true : false;
            factoryMode = (telemetry[index] >> 7 & 0x1) == 1 ? true : false;
            index += 1;

            flyMode = telemetry[index]; index += 1;
            throwFlyTimer = telemetry[index]; index += 1;
            cameraState = telemetry[index]; index += 1;

            //if (paramArrayOfByte.length >= 22)
            electricalMachineryState = telemetry[index]; index += 1; //(paramArrayOfByte[21] & 0xFF);

            //if (paramArrayOfByte.length >= 23)
            frontIn = (telemetry[index] >> 0 & 0x1) == 1 ? true : false;//22
            frontOut = (telemetry[index] >> 1 & 0x1) == 1 ? true : false;
            frontLSC = (telemetry[index] >> 2 & 0x1) == 1 ? true : false;
            index += 1;
            temperatureHeight = (telemetry[index] >> 0 & 0x1);//23
        }

        public double[] toEuler()
        {
            float qX = Quaternion.X;
            float qY = Quaternion.Y;
            float qZ = Quaternion.Z;
            float qW = Quaternion.W;

            double sqW = qW * qW;
            double sqX = qX * qX;
            double sqY = qY * qY;
            double sqZ = qZ * qZ;
            double yaw = 0.0;
            double roll = 0.0;
            double pitch = 0.0;
            double[] retv = new double[3];
            double unit = sqX + sqY + sqZ + sqW; // if normalised is one, otherwise
                                                 // is correction factor
            double test = qW * qX + qY * qZ;
            if (test > 0.499 * unit)
            { // singularity at north pole
                yaw = 2 * Math.Atan2(qY, qW);
                pitch = Math.PI / 2;
                roll = 0;
            }
            else if (test < -0.499 * unit)
            { // singularity at south pole
                yaw = -2 * Math.Atan2(qY, qW);
                pitch = -Math.PI / 2;
                roll = 0;
            }
            else
            {
                yaw = Math.Atan2(2.0 * (qW * qZ - qX * qY),
                        1.0 - 2.0 * (sqZ + sqX));
                roll = Math.Asin(2.0 * test / unit);
                pitch = Math.Atan2(2.0 * (qW * qY - qX * qZ),
                        1.0 - 2.0 * (sqY + sqX));
            }
            retv[0] = pitch;
            retv[1] = roll;
            retv[2] = yaw;
            return retv;
        }

        //Parse some of the interesting info from the tello log stream
        public void parseLog(byte[] data)
        {
            int pos = 0;

            //A packet can contain more than one record.
            while (pos < data.Length - 2)//-2 for CRC bytes at end of packet.
            {
                if (data[pos] != 'U')//Check magic byte
                {
                    //Console.WriteLine("PARSE ERROR!!!");
                    break;
                }
                var len = data[pos + 1];
                if (data[pos + 2] != 0)//Should always be zero (so far)
                {
                    //Console.WriteLine("SIZE OVERFLOW!!!");
                    break;
                }
                var crc = data[pos + 3];
                var id = BitConverter.ToUInt16(data, pos + 4);
                var xorBuf = new byte[256];
                byte xorValue = data[pos + 6];
                switch (id)
                {
                    case 0x1d://29 new_mvo
                        for (var i = 0; i < len; i++)//Decrypt payload.
                            xorBuf[i] = (byte)(data[pos + i] ^ xorValue);
                        var index = 10;//start of the velocity and pos data.
                        var observationCount = BitConverter.ToUInt16(xorBuf, index); index += 2;
                        velX = BitConverter.ToInt16(xorBuf, index); index += 2;
                        velY = BitConverter.ToInt16(xorBuf, index); index += 2;
                        velZ = BitConverter.ToInt16(xorBuf, index); index += 2;
                        posX = BitConverter.ToSingle(xorBuf, index); index += 4;
                        posY = BitConverter.ToSingle(xorBuf, index); index += 4;
                        posZ = BitConverter.ToSingle(xorBuf, index); index += 4;
                        posUncertainty = BitConverter.ToSingle(xorBuf, index) * 10000.0f; index += 4;
                        //Console.WriteLine(observationCount + " " + posX + " " + posY + " " + posZ);
                        break;
                    case 0x0800://2048 imu
                        for (var i = 0; i < len; i++)//Decrypt payload.
                            xorBuf[i] = (byte)(data[pos + i] ^ xorValue);
                        var index2 = 10 + 48;//44 is the start of the quat data.
                        
                        var quatW = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        var quatX = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        var quatY = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        var quatZ = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        Quaternion = new Quaternion(quatW, quatX, quatY, quatZ);
                        //Console.WriteLine("qx:" + qX + " qy:" + qY+ "qz:" + qZ);

                        //var eular = toEuler(quatX, quatY, quatZ, quatW);
                        //Console.WriteLine(" Pitch:"+eular[0] * (180 / 3.141592) + " Roll:" + eular[1] * (180 / 3.141592) + " Yaw:" + eular[2] * (180 / 3.141592));

                        index2 = 10 + 76;//Start of relative velocity
                        velN = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        velE = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        velD = BitConverter.ToSingle(xorBuf, index2); index2 += 4;
                        //Console.WriteLine(vN + " " + vE + " " + vD);

                        break;

                }
                pos += len;
            }
        }

        //For saving out state info.
        public string getLogHeader()
        {
            StringBuilder sb = new StringBuilder();
            foreach (System.Reflection.FieldInfo property in this.GetType().GetFields())
            {
                sb.Append(property.Name);
                sb.Append(",");
            }
            sb.AppendLine();
            return sb.ToString();
        }
        public string getLogLine()
        {
            StringBuilder sb = new StringBuilder();
            foreach (System.Reflection.FieldInfo property in this.GetType().GetFields())
            {
                if (property.FieldType == typeof(Boolean))
                {
                    if ((Boolean)property.GetValue(this) == true)
                        sb.Append("1");
                    else
                        sb.Append("0");
                }
                else
                    sb.Append(property.GetValue(this));
                sb.Append(",");
            }
            sb.AppendLine();
            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            var count = 0;
            foreach (System.Reflection.FieldInfo property in this.GetType().GetFields())
            {
                sb.Append(property.Name);
                sb.Append(": ");
                sb.Append(property.GetValue(this));
                if (count++ % 2 == 1)
                    sb.Append(System.Environment.NewLine);
                else
                    sb.Append("      ");

            }

            return sb.ToString();
        }
    }

}
