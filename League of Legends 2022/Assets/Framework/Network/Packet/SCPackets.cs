/********************************************************************
	created:	2020/09/15
	author:		ZYL
	
	purpose:	
*********************************************************************/
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using IMessage = Google.Protobuf.IMessage;

namespace iFramework
{    
    internal class SCPackets
    {
        public AESCryptor Cryptor { get; set; }
        private Queue<SCMsg> _messages = new Queue<SCMsg>();

        private void AddMsg(SCMsg msg)
        {
            lock(_messages)
            {
                _messages.Enqueue(msg);
            }
        }

        public SCMsg GetMsg()
        {
            SCMsg msg = null;

            lock (_messages)
            {
                if (_messages.Count > 0)
                {
                    msg = _messages.Dequeue();
                }
            }

            return msg;
        }

        public bool TryUnpack(byte[] buf, int offset, int bufLen, out int unpackedLen)
        {
            unpackedLen = 0;
            if (bufLen < NetDef.kSCHeadLen)
            {
                return false;
            }

            int pkgLen = PackHelper.UnPackInt(buf, ref offset);
            if (bufLen < pkgLen)
            {
                return false;
            }

            byte flag = PackHelper.UnPackByte(buf, ref offset);
            uint msgID = (uint)PackHelper.UnPackInt(buf, ref offset);
            int crc = PackHelper.UnPackInt(buf, ref offset);
            pkgLen -= NetDef.kSCHeadLen;

            if ((flag & NetDef.kFlag_CRC) > 0)
            {
                int realCRC = CRC.Check(buf, offset, pkgLen);
                if (crc != realCRC)
                {
                    throw new Exception("CRC not match, " + crc + " / " + realCRC);
                }
            }

            byte[] newBuf = null;
            if ((flag & NetDef.kFlag_Crypto) > 0)
            {
                newBuf = Cryptor.Crypt(buf, offset, pkgLen);
            }


            if ((flag & NetDef.kFlag_Compress) > 0)
            {
                if (newBuf != null)
                {
                    newBuf = ZipHelper.UnZip(newBuf);
                }
                else
                {
                    newBuf = ZipHelper.UnZip(buf, offset, pkgLen);
                }
            }

            SCMsg msg = new SCMsg() { msgID = msgID };
            if (ProtoBuf.PType.MsgIDExist((uint)msgID))
            {
                if (newBuf == null)
                {
                    msg.msg = BytesToMsg(msgID, buf, offset, pkgLen);
                }
                else
                {
                    msg.msg = BytesToMsg(msgID, newBuf, 0, newBuf.Length);
                }
            }
            else
            {
                if (newBuf == null)
                {
                    newBuf = new byte[pkgLen];
                    Buffer.BlockCopy(buf, offset, newBuf, 0, pkgLen);
                } 
                msg.buffer = newBuf;
            }

            AddMsg(msg);
            unpackedLen = pkgLen + NetDef.kSCHeadLen;

            return true;
        }

        public IMessage BytesToMsg(uint msgID, byte[] bytes, int offset, int len)
        {
            IMessage msg = null;
            using (var ms = new MemoryStream(bytes, offset, len))
            {
                try
                {
                    var T = ProtoBuf.PType.MsgID2Type(msgID);
                    msg = Activator.CreateInstance(T) as IMessage;                    
                    using (var codeStream = new CodedInputStream(ms))
                    {
                        msg.Decode(codeStream);
                    }
                }
                catch (Exception e)
                {
                    
                }
            }

            return msg;
        }
    }
}
