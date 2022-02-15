/********************************************************************
	created:	2020/09/15
	author:		ZYL
	
	purpose:	
*********************************************************************/
using Google.Protobuf;
using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;

namespace iFramework
{
    internal class SCMsg
    {
        public uint msgID;
        public IMessage msg;
        public byte[] buffer;        
    }
    /// <summary>
    /// 接受消息的线程实例
    /// </summary>
    internal class TCPReceiver
    {
        private TcpClient _tcpClient;
        private AESCryptor _cryptor;
        private NetEventDispatcher _eventDispacher;
        private SCPackets _packets;
        private bool _disposed;
        private Thread _thread;

        public TCPReceiver(TcpClient tcpClient, NetEventDispatcher dispatcher)
        {
            _packets = new SCPackets();
            _tcpClient = tcpClient;
            _eventDispacher = dispatcher;
            _disposed = false;

            _thread = new Thread(() => ReceiveThread())
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
            _packets.Cryptor = _cryptor;
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_disposed) return;

            // dispatch msg
            SCMsg msg = _packets.GetMsg();
            if (msg != null)
            {
                _eventDispacher.TriggerMsgEvent(msg);
            }            
        }

        private void ReceiveThread()
        {            
            byte[] recvBuf = new byte[NetDef.kMaxSCPktLen];
            int recvedLen = 0;
            

            while (!_disposed)
            {
                try
                {
                    if (_tcpClient.Connected)
                    {
                        NetworkStream netStream = _tcpClient.GetStream();
                        if (netStream.DataAvailable)
                        {
                            int readLen = netStream.Read(recvBuf, recvedLen, NetDef.kMaxSCPktLen - recvedLen);
                            recvedLen += readLen;

                            while (_packets.TryUnpack(recvBuf, 0, recvedLen, out int unpackLen))
                            {
                                recvedLen -= unpackLen;
                                if (recvedLen > 0)
                                {
                                    Buffer.BlockCopy(recvBuf, unpackLen, recvBuf, 0, recvedLen);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.GetType() != typeof(ObjectDisposedException))
                    {
                        Debug.LogError("receive data exception: " + e.Message + e.TargetSite);
                    }
                    _tcpClient.Close();
                }

                Thread.Sleep(1);
            }            
        }
    }
}