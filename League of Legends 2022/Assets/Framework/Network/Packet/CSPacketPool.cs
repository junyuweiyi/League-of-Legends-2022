/********************************************************************
	created:	2020/09/15
	author:		ZYL
	
	purpose:	
*********************************************************************/
using System.Collections.Generic;

namespace iFramework
{
    public class CSPacketPool
    {
        /// <summary>
        /// 空闲包列表
        /// </summary>
        private readonly Queue<CSPacket> _freePackets = new Queue<CSPacket>();
        /// <summary>
        /// 待发送包列表
        /// </summary>
        private readonly Queue<CSPacket> _sendPackets = new Queue<CSPacket>();

        /// <summary>
        /// 获得一个空的包
        /// </summary>
        /// <returns></returns>
        public CSPacket TakeFreePacket()
        {
            lock(_freePackets)
            {
                if (_freePackets.Count == 0)
                {
                    _freePackets.Enqueue(new CSPacket());
                }

                var packet = _freePackets.Dequeue();
                packet.Length = 0;
                return packet;
            }
        }

        /// <summary>
        /// 放回一个空的包
        /// </summary>
        /// <param name="bufObject">空的Buf</param>
        public void GivebackFreePacket(CSPacket packet)
        {
            lock(_freePackets)
            {
                _freePackets.Enqueue(packet);
            }
        }

        /// <summary>
        /// 取1个待发送包
        /// </summary>
        /// <returns>使用中的Buf</returns>
        public CSPacket TakeSendPacket()
        {
            lock(_sendPackets)
            {
                if (_sendPackets.Count == 0)
                {
                    return null;
                }

                var packet = _sendPackets.Dequeue();
                return packet;
            }
        }

        public void PushSendPacket(CSPacket packet)
        {
            lock(_sendPackets)
            {
                _sendPackets.Enqueue(packet);
            }
        }
    }
}
