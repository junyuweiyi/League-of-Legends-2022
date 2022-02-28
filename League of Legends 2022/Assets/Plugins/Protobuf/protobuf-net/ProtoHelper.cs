using Google.Protobuf;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProtoBuf
{
    public sealed class ProtoHelper
    {
        //public static bool UseReflection = true;
        public static byte[] EncodeWithName(IMessage p)
        {
            var type = PType.GetPType(p);
            var name = type.FullName;
            using (var ms = new MemoryStream())
            {
                //if (UseReflection)
                //{
                //    Serializer.Serialize(ms, p);
                //}
                //else
                {
                    var cos = new CodedOutputStream(ms);
                    p.Encode(cos);
                    cos.Flush();
                }
                var nbs = Encoding.UTF8.GetBytes(name);
                int nblen = nbs.Length;
                if (nblen > 255)
                {
                    throw new Exception("PB:name->" + name + " is To Long " + nblen + " > 255");
                }
                var buffer = new byte[ms.Length + nbs.Length + 1];
                buffer[0] = (byte)((nblen >> 0) & 0xFF);
                Buffer.BlockCopy(nbs, 0, buffer, 1, nblen);
                ms.Position = 0;
                ms.Read(buffer, 1 + nblen, (int)ms.Length);
                return buffer;
            }
        }

        public static IMessage DecodeWithName(byte[] b, out string name)
        {
            var bytesLen = b[0];
            name = Encoding.UTF8.GetString(b, 1, bytesLen);
            using (var ms = new MemoryStream(b, 1 + bytesLen, b.Length - 1 - bytesLen))
            {
                Type T = PType.FindType(name);
                if (name.Contains(".")) { name = name.Substring(name.LastIndexOf('.') + 1); }
                //if (UseReflection)
                //{
                //    return Serializer.Deserialize(T, ms) as IMessage;
                //}
                //else
                {
                    var o = PType.CreateInstance(T) as IMessage;
                    o.Decode(new CodedInputStream(ms));
                    return o;
                }
            }
        }
    }
}
