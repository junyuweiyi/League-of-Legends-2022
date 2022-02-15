/********************************************************************
	created:	2020/10/21
	author:		ZYL
	
	purpose:	与服务器握手，交换加密key
*********************************************************************/

using System;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace iFramework
{
    class CryptoKeyExchanger
    {
        private Action<bool, byte[]> _onComplete;
        private TcpClient _tcpClient;
        private float _recvTimeout;
        private byte[] _recievedBuf = new byte[128];
        private int _recievedLen = 0;
        private AESCryptor _cryptor;

        public CryptoKeyExchanger(TcpClient tcpClient, string rsaKey, Action<bool, byte[]> onComplete)
        {
            _onComplete = onComplete;
            if (tcpClient == null || !tcpClient.Connected)
            {
                onComplete?.Invoke(false, null);
                return;
            }

            _tcpClient = tcpClient;
            byte[] key = new byte[16];
            RandomNumberGenerator.Create().GetBytes(key);
            _cryptor = new AESCryptor(key);

            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(rsaKey);
            byte[] encryptedKey = rsa.Encrypt(key, true);
            byte[] len = new byte[4];
            PackHelper.PackInt(encryptedKey.Length + len.Length, len);
            tcpClient.GetStream().Write(len, 0, len.Length);
            tcpClient.GetStream().Write(encryptedKey, 0, encryptedKey.Length);
            _recvTimeout = 30;//30秒超时
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (_recvTimeout <= 0) return;

            if (!_tcpClient.Connected)
            {
                _recvTimeout = -1;
                _onComplete?.Invoke(false, null);
                return;
            }

            _recvTimeout -= realElapseSeconds;
            if (_recvTimeout <= 0)
            {
                // Time out
                _onComplete?.Invoke(false, null);
                return;
            }

            NetworkStream netStream = _tcpClient.GetStream();
            if (netStream.DataAvailable)
            {
                int readLen = netStream.Read(_recievedBuf, _recievedLen, 128 - _recievedLen);
                _recievedLen += readLen;
                if (_recievedLen > 4)
                {
                    int packLen = PackHelper.UnPackInt(_recievedBuf);
                    if (_recievedLen == packLen)
                    {
                        byte[] newkey = null;
                        try
                        {
                            newkey = _cryptor.Crypt(_recievedBuf, 4, packLen - 4);
                        }
                        catch
                        {
                            UnityEngine.Debug.LogError("CryptoKeyExchanger：协议内容无效，无法获得key！");
                        }
                        _recvTimeout = -1;
                        _onComplete?.Invoke(newkey != null, newkey);
                    }
                    else if (_recievedLen > packLen)
                    {
                        UnityEngine.Debug.LogError("CryptoKeyExchanger：握手过程中收到多余数据！！");
                        _recvTimeout = -1;
                        _onComplete?.Invoke(false, null);
                    }
                }
            }
        }
    }
}
