/********************************************************************
	created:	2020/09/15
	author:		ZYL
	
	purpose:	
*********************************************************************/
using System;

namespace iFramework
{
    public class ZipHelper
    {
        public static byte[] Zip(byte[] unzip, int offset = 0, int len = -1)
        {
            len = len < 0 ? unzip.Length : len;
            var ziped = LZ4.LZ4Codec.Encode(unzip, offset, len);
            var buffer = new byte[ziped.Length + 4];
            Buffer.BlockCopy(ziped, 0, buffer, 4, ziped.Length);
            buffer[0] = (byte)((len >> 24) & 0xFF);
            buffer[1] = (byte)((len >> 16) & 0xFF);
            buffer[2] = (byte)((len >> 8) & 0xFF);
            buffer[3] = (byte)((len >> 0) & 0xFF);
            return buffer;
        }

        public static int Zip(byte[] srcBuff, int srcOffset, int srcLen, byte[] outBuff, int outOffset)
        {
            var ziped = LZ4.LZ4Codec.Encode(srcBuff, srcOffset, srcLen);
            int outLen = outBuff.Length - outOffset;
            if (ziped.Length + 4 <= outLen)
            {
                Buffer.BlockCopy(ziped, 0, outBuff, outOffset + 4, ziped.Length);
                outBuff[outOffset] = (byte)((srcLen >> 24) & 0xFF);
                outBuff[outOffset + 1] = (byte)((srcLen >> 16) & 0xFF);
                outBuff[outOffset + 2] = (byte)((srcLen >> 8) & 0xFF);
                outBuff[outOffset + 3] = (byte)((srcLen) & 0xFF);
                return ziped.Length + 4; ;
            }
            UnityEngine.Debug.LogError("ZipHelper - Zip: out buff length not enough.");
            return -1;
        }

        public static byte[] UnZip(byte[] zip, int offset = 0, int len = -1)
        {
            len = len < 0 ? zip.Length : len;
            int length = (int)(((zip[offset + 0] << 24) | (zip[offset + 1] << 16) | (zip[offset + 2] << 8) | (zip[offset + 3] << 0)) & 0xFFFFFFFF);
            return LZ4.LZ4Codec.Decode(zip, offset + 4, len - 4, length);
        }
    }
}
