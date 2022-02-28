#pragma warning disable CS1591, CS0612, CS3021, IDE1006
using Google.Protobuf;

namespace protocol
{
    //[global::ProtoBuf.ProtoContract()]
    public partial class StartSelectHeroNtf : Google.Protobuf.IMessage
    {
        public void Encode(Google.Protobuf.CodedOutputStream writer)
        {
        }
        public void Decode(Google.Protobuf.CodedInputStream reader)
        {
        }
    }
    //[global::ProtoBuf.ProtoContract()]
    public partial class CloseSelectHeroNtf : Google.Protobuf.IMessage
    {
        public void Encode(Google.Protobuf.CodedOutputStream writer)
        {
        }
        public void Decode(Google.Protobuf.CodedInputStream reader)
        {
        }
    }
    //[global::ProtoBuf.ProtoContract()]
    public partial class SelectHeroReq : Google.Protobuf.IMessage
    {
        public int HeroID { get; set; }
        public void Encode(Google.Protobuf.CodedOutputStream writer)
        {
        }
        public void Decode(Google.Protobuf.CodedInputStream reader)
        {
        }
    }

    //[global::ProtoBuf.ProtoContract()]
    public partial class SelectHeroAck : Google.Protobuf.IMessage
    {    
        //[global::ProtoBuf.ProtoMember(1)]
        public ErrCode Code { get; set; }
        public int HeroID { get; set; }
        public void Encode(Google.Protobuf.CodedOutputStream writer)
        {
        }
        public void Decode(Google.Protobuf.CodedInputStream reader)
        {
        }
    }

    //[global::ProtoBuf.ProtoContract()]
    public partial class SelectHeroNtf : Google.Protobuf.IMessage
    {
        public long UserID { get; set; }
        public int HeroID { get; set; }
        public void Encode(Google.Protobuf.CodedOutputStream writer)
        {
        }
        public void Decode(Google.Protobuf.CodedInputStream reader)
        {
        }
    }

    //[global::ProtoBuf.ProtoContract()]
    public partial class SelectHeroSummonerSkillReq : Google.Protobuf.IMessage
    {
        public int Slot { get; set; }
        public int SummonerSkillID { get; set; }
        public void Encode(Google.Protobuf.CodedOutputStream writer)
        {
        }
        public void Decode(Google.Protobuf.CodedInputStream reader)
        {
        }
    }

    //[global::ProtoBuf.ProtoContract()]
    public partial class SelectSummonerSkillAck : Google.Protobuf.IMessage
    {
        //[global::ProtoBuf.ProtoMember(1)]
        public ErrCode Code { get; set; }
        public int Slot { get; set; }
        public int SummonerSkillID { get; set; }
        public void Encode(Google.Protobuf.CodedOutputStream writer)
        {
        }
        public void Decode(Google.Protobuf.CodedInputStream reader)
        {
        }
    }

    //[global::ProtoBuf.ProtoContract()]
    public partial class SelectSummonerSkillNtf : Google.Protobuf.IMessage
    {
        public long UserID { get; set; }
        public int Slot { get; set; }
        public int SummonerSkillID { get; set; }
        public void Encode(Google.Protobuf.CodedOutputStream writer)
        {
        }
        public void Decode(Google.Protobuf.CodedInputStream reader)
        {
        }
    }
}