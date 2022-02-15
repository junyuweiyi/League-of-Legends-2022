/********************************************************************
	created:	2020/09/15
	author:		ZYL
	
	purpose:	
*********************************************************************/
using System;
using System.Net.Sockets;
using UnityEngine;
using IMessage = Google.Protobuf.IMessage;
using System.Threading;
using System.Security.Cryptography;

namespace iFramework
{
    /// <summary>
    /// 网络发送器
    /// </summary>
    internal class TCPSender
    {
        private TcpClient _tcpClient;
        private AESCryptor _cryptor;
        private readonly CSPacketPool _packetPool;
        private bool _disposed;
        private Thread _thread;

        public TCPSender(TcpClient tcpClient)
        {
            _packetPool = new CSPacketPool();
            _tcpClient = tcpClient;
            _disposed = false;

            _thread = new Thread(() => SendThread())
            {
                IsBackground = true
            };
            _thread.Start();
        }

        public void Dispose()
        {
            _disposed = true;
        }

        public bool Disposed { get { return _disposed; } }

        public void SetCryptKey(byte[] key)
        {
            _cryptor = new AESCryptor(key);
        }

        private void SendThread()
        {
            while(!_disposed)
            {
                try
                {
                    if (_tcpClient.Connected)
                    {
                        CSPacket packet = _packetPool.TakeSendPacket();
                        if (packet != null)
                        {                           
                            try
                            {
                                packet.Pack(_cryptor);
                                NetworkStream networkStream = _tcpClient.GetStream();
                                networkStream.Write(packet.Buff, 0, packet.Length);
                            }
                            catch (Exception msg)
                            {
                                Debug.LogError($"Error send msg to server, msgID: {packet.MsgID} MsgName {ProtoBuf.PType.MsgID2Name(packet.MsgID)} . Error: {msg.Message}" );
                                _tcpClient.Close();
                            }

                            _packetPool.GivebackFreePacket(packet);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.GetType() != typeof(ObjectDisposedException))
                    {
                        Debug.LogError("Network sender run error:" + e.Message);
                    }
                    _tcpClient.Close();
                }

                Thread.Sleep(1);
            }            
        }


        /// <summary>
        /// 将消息打包并放入发送队列
        /// </summary>
        public void SendMsg(IMessage msg, bool crypted)
        {
            //必须有要发送的消息
            if (null == msg)
            {
                return;
            }
            if (crypted && _cryptor == null)
            {
                crypted = false;
                Debug.LogError("crypt key not set, can not send crypted msg.");
            }
            CSPacket packet = _packetPool.TakeFreePacket();
            packet.Init(msg, crypted);
            _packetPool.PushSendPacket(packet);
        }

        public void SendMsg(uint msgID, byte[] buffer, int buffLen, bool crypted)
        {
            //必须有要发送的消息
            if (null == buffer)
            {
                return;
            }
            if (crypted && _cryptor == null)
            {
                crypted = false;
                Debug.LogError("crypt key not set, can not send crypted msg.");
            }
            CSPacket packet = _packetPool.TakeFreePacket();
            packet.Init(msgID, buffer, buffLen, crypted);
            _packetPool.PushSendPacket(packet);
        }
    }
}