/********************************************************************
	created:	2020/09/15
	author:		ZYL
	
	purpose:	
*********************************************************************/
using Google.Protobuf;
using System;
using System.IO;
using IMessage = Google.Protobuf.IMessage;

namespace iFramework
{
    /// <summary>
    /// CS网络包对象
    /// </summary>
    public class CSPacket
    {
        private static Random random = new Random();
        public byte[] Buff = new byte[NetDef.kMaxCSPktLen];
        public int Length { get; set; }
        public uint MsgID { get; private set; }
        private bool _crypted;

        public void Init(IMessage msg, bool crypted)
        {
            // 打包协议体
            Length = MsgToBytes(msg);
            MsgID = ProtoBuf.PType.MsgName2ID(msg.GetType().Name);
            _crypted = crypted;
        }

        public void Init(uint msgID, byte[] buffer, int buffLen, bool crypted)
        {
            Length = buffLen > 0 ? buffLen : buffer.Length;
            MsgID = msgID;
            _crypted = crypted;
            Buffer.BlockCopy(buffer, 0, Buff, NetDef.kCSHeadLen, Length);
        }

        public int MsgToBytes(IMessage msg)
        {
            int len = 0;
            using (var ms = new MemoryStream(Buff, NetDef.kCSHeadLen, NetDef.kMaxCSPktLen - NetDef.kCSHeadLen))
            {
                using (var cos = new CodedOutputStream(ms))
                {
                    msg.Encode(cos);
                    cos.Flush();
                    len = (int)cos.Position;
                }
            }
            return len;
        }


        public void Pack(AESCryptor cryptor)
        {
            byte flag = (byte)random.Next();
            flag &= NetDef.kFlag_Mask;

            // 压缩
            if (Length > 1024)
            {
                int len = ZipHelper.Zip(Buff, NetDef.kCSHeadLen, Length, Buff, NetDef.kCSHeadLen);
                if (len >= 0)
                {
                    Length = len;
                    flag |= NetDef.kFlag_Compress;
                }
            }

            // 加密
            if (_crypted || (random.Next() % 10 < 2))
            {
                if (cryptor != null)
                {
                    byte[] crypted = cryptor.Crypt(Buff, NetDef.kCSHeadLen, Length);
                    Buffer.BlockCopy(crypted, 0, Buff, NetDef.kCSHeadLen, crypted.Length);
                    flag |= NetDef.kFlag_Crypto;
                }
            }

            // 循环校验
            int crc = random.Next();
            if (_crypted || (random.Next() % 10 < 2))
            {
                crc = CRC.Check(Buff, NetDef.kCSHeadLen, Length);
                flag |= NetDef.kFlag_CRC;
            }

            // 包头
            Length += NetDef.kCSHeadLen;

            int offset = 0;
            PackHelper.PackInt(Length, Buff, ref offset);
            PackHelper.PackByte(flag, Buff, ref offset);
            PackHelper.PackInt((int)MsgID, Buff, ref offset);
            PackHelper.PackInt(crc, Buff, ref offset);
        }
    }
}
