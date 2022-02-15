/********************************************************************
	created:	2020/10/20
	author:		ZYL
	
	purpose:	
*********************************************************************/
using System;
using System.Net;
using System.Security.Cryptography;

namespace iFramework
{
    class PackHelper
    {
        public static void PackByte(byte val, byte[] buf, ref int offset)
        {
            PackByte(val, buf, offset);
            offset += 1;
        }

        public static void PackInt(int val, byte[] buf, ref int offset)
        {
            PackInt(val, buf, offset);
            offset += 4;
        }

        public static void PackByte(byte val, byte[] buf, int offset = 0)
        {
            buf[offset] = val;
        }

        public static void PackInt(int val, byte[] buf, int offset = 0)
        {
            int netVal = IPAddress.HostToNetworkOrder(val);
            byte[] intBuf = BitConverter.GetBytes(netVal);
            for (int i = 0; i < intBuf.GetLength(0); i++)
            {
                buf[offset + i] = intBuf[i];
            }
        }

        public static byte UnPackByte(byte[] buf, ref int offset)
        {
            byte val = UnPackByte(buf, offset);
            offset += 1;
            return val;
        }

        public static int UnPackInt(byte[] buf, ref int offset)
        {
            int val = UnPackInt(buf, offset);
            offset += 4;
            return val;
        }

        public static byte UnPackByte(byte[] buf, int offset = 0)
        {
            byte val = buf[offset];
            return val;
        }

        public static int UnPackInt(byte[] buf, int offset = 0)
        {
            int netVal = BitConverter.ToInt32(buf, offset);
            int val = IPAddress.NetworkToHostOrder(netVal);
            return val;
        }
    }
}
