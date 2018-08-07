using System.Text;

namespace Tello.Core
{
    /// <summary>
    /// CRC extensions for datagram packets
    /// </summary>
    internal static class DatagramExtensions
    {
        // frame check sequence table
        // more info: https://www.cisco.com/en/US/docs/internetworking/troubleshooting/guide/tr1904.html        private static readonly int[] _fcsTable = { 0, 4489, 8978, 12955, 17956, 22445, 25910, 29887, 35912, 40385, 44890, 48851, 51820, 56293, 59774, 63735, 4225, 264, 13203, 8730, 22181, 18220, 30135, 25662, 40137, 36160, 49115, 44626, 56045, 52068, 63999, 59510, 8450, 12427, 528, 5017, 26406, 30383, 17460, 21949, 44362, 48323, 36440, 40913, 60270, 64231, 51324, 55797, 12675, 8202, 4753, 792, 30631, 26158, 21685, 17724, 48587, 44098, 40665, 36688, 64495, 60006, 55549, 51572, 16900, 21389, 24854, 28831, 1056, 5545, 10034, 14011, 52812, 57285, 60766, 64727, 34920, 39393, 43898, 47859, 21125, 17164, 29079, 24606, 5281, 1320, 14259, 9786, 57037, 53060, 64991, 60502, 39145, 35168, 48123, 43634, 25350, 29327, 16404, 20893, 9506, 13483, 1584, 6073, 61262, 65223, 52316, 56789, 43370, 47331, 35448, 39921, 29575, 25102, 20629, 16668, 13731, 9258, 5809, 1848, 65487, 60998, 56541, 52564, 47595, 43106, 39673, 35696, 33800, 38273, 42778, 46739, 49708, 54181, 57662, 61623, 2112, 6601, 11090, 15067, 20068, 24557, 28022, 31999, 38025, 34048, 47003, 42514, 53933, 49956, 61887, 57398, 6337, 2376, 15315, 10842, 24293, 20332, 32247, 27774, 42250, 46211, 34328, 38801, 58158, 62119, 49212, 53685, 10562, 14539, 2640, 7129, 28518, 32495, 19572, 24061, 46475, 41986, 38553, 34576, 62383, 57894, 53437, 49460, 14787, 10314, 6865, 2904, 32743, 28270, 23797, 19836, 50700, 55173, 58654, 62615, 32808, 37281, 41786, 45747, 19012, 23501, 26966, 30943, 3168, 7657, 12146, 16123, 54925, 50948, 62879, 58390, 37033, 33056, 46011, 41522, 23237, 19276, 31191, 26718, 7393, 3432, 16371, 11898, 59150, 63111, 50204, 54677, 41258, 45219, 33336, 37809, 27462, 31439, 18516, 23005, 11618, 15595, 3696, 8185, 63375, 58886, 54429, 50452, 45483, 40994, 37561, 33584, 31687, 27214, 22741, 18780, 15843, 11370, 7921, 3960 };

        internal static int CaclulateFCS16(this byte[] bytes, int len)
        {
            int result = 0xFFFF;
            if (bytes != null)
            {
                result = 0x3692;
                for (var i = 0; i < len; ++i)
                {
                    result = _fcsTable[((result ^ bytes[i]) & 0xFF)] ^ result >> 8;
                }
            }
            return result;
        }

        /// <summary>
        ///  write fcs16 crc into the last 2 bytes of the datagram
        /// </summary>
        /// <param name="datagram"></param>
        internal static void SetFCS16(this byte[] datagram)
        {
            if ((datagram == null) || (datagram.Length <= 2)) return;

            int fcs16 = datagram.CaclulateFCS16(datagram.Length - 2);
            datagram[(datagram.Length - 2)] = ((byte)(fcs16 & 0xFF));
            datagram[(datagram.Length - 1)] = ((byte)(fcs16 >> 8 & 0xFF));
        }

        private static readonly short[] _uCRCTable = { 0, 94, 188, 226, 97, 63, 221, 131, 194, 156, 126, 32, 163, 253, 31, 65, 157, 195, 33, 127, 252, 162, 64, 30, 95, 1, 227, 189, 62, 96, 130, 220, 35, 125, 159, 193, 66, 28, 254, 160, 225, 191, 93, 3, 128, 222, 60, 98, 190, 224, 2, 92, 223, 129, 99, 61, 124, 34, 192, 158, 29, 67, 161, 255, 70, 24, 250, 164, 39, 121, 155, 197, 132, 218, 56, 102, 229, 187, 89, 7, 219, 133, 103, 57, 186, 228, 6, 88, 25, 71, 165, 251, 120, 38, 196, 154, 101, 59, 217, 135, 4, 90, 184, 230, 167, 249, 27, 69, 198, 152, 122, 36, 248, 166, 68, 26, 153, 199, 37, 123, 58, 100, 134, 216, 91, 5, 231, 185, 140, 210, 48, 110, 237, 179, 81, 15, 78, 16, 242, 172, 47, 113, 147, 205, 17, 79, 173, 243, 112, 46, 204, 146, 211, 141, 111, 49, 178, 236, 14, 80, 175, 241, 19, 77, 206, 144, 114, 44, 109, 51, 209, 143, 12, 82, 176, 238, 50, 108, 142, 208, 83, 13, 239, 177, 240, 174, 76, 18, 145, 207, 45, 115, 202, 148, 118, 40, 171, 245, 23, 73, 8, 86, 180, 234, 105, 55, 213, 139, 87, 9, 235, 181, 54, 104, 138, 212, 149, 203, 41, 119, 244, 170, 72, 22, 233, 183, 85, 11, 136, 214, 52, 106, 43, 117, 151, 201, 74, 20, 246, 168, 116, 42, 200, 150, 21, 75, 169, 247, 182, 232, 10, 84, 215, 137, 107, 53 };

        internal static int CalculateUCRC(this byte[] bytes, int length)
        {
            int result = 119, j = 0, i = 0;
            while (length-- != 0)
            {
                j = bytes[i++] ^ result;
                result = _uCRCTable[j >= 0 ? j : j + 256];
            }
            return result;
        }

        /// <summary>
        /// write ucrc datagram at position length-1 
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="length"></param>
        internal static void SetUCRC(this byte[] datagram, int length)
        {
            if (datagram == null || datagram.Length == 0 || length <= 2)
            {
                return;
            }

            int ucrc = datagram.CalculateUCRC(length - 1);
            datagram[(length - 1)] = ((byte)(ucrc & 0xFF));
        }

        /// <summary>
        /// update the datagram with the fcs16 and ucrc values in the datagram
        /// </summary>
        /// <param name="datagram"></param>
        internal static void SetCRC(this byte[] datagram)
        {
            datagram.SetUCRC(4);
            datagram.SetFCS16();
        }

        private static ushort _sequence = 1;
        internal static void SetSequence(this byte[] datagram)
        {
            //todo: this is not threadsafe
            datagram[7] = (byte)(_sequence & 0xff);
            datagram[8] = (byte)((_sequence >> 8) & 0xff);
            ++_sequence;
        }

        public static byte[] GetDatagram(this Commands message)
        {
            switch (message)
            {
                default:
                case Commands.ConnectionRequest: return Encoding.UTF8.GetBytes("conn_req:\x00\x00");
                case Commands.TakeOff:  return new byte[] { 0xcc, 0x58, 0x00, 0x7c, 0x68, 0x54, 0x00, 0xe4, 0x01, 0xc2, 0x16 };
                case Commands.Land:     return new byte[] { 0xcc, 0x60, 0x00, 0x27, 0x68, 0x55, 0x00, 0xe5, 0x01, 0x00, 0xba, 0xc7 };
            }
        }
    }
}
