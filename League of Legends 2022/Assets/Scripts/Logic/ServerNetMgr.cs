using Google.Protobuf;
using System;
using iFramework;

public class ServerNetMgr : GameMgr<ServerNetMgr>
{
    public Action<long> OnUserDisconnect;

    public void SendMsg(long userID, IMessage msg)
    {
    }

    public void RegisterMsg<T>(object owner, Action<long, T> handler) where T : IMessage
    {
    }

    public void UnregisterMsg<T>(object owner)
    {
    }

    public void UnregisterMsgs(object owner)
    {
    }
}