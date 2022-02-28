using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Google.Protobuf
{
    public interface IMessage
    {
        void Encode(CodedOutputStream writer);
        void Decode(CodedInputStream reader);
    }
}
