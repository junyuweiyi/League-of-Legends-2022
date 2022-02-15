/********************************************************************
	created:	2020/09/14
	author:		ZYL
	
	purpose:	TCP连接实例
*********************************************************************/
using System;
using IMessage = Google.Protobuf.IMessage;

namespace iFramework
{
    public class TCPInstance
    {
        private TCPConnecter _connecter;
        private TCPReceiver _receiver;
        private TCPSender _sender;
        private NetEventDispatcher _eventDispatcher;
        private NetworkConnectType _connectType;
        private CryptoKeyExchanger _keyExchanger;
        private string _rsaPubKey =
              "<RSAKeyValue><Modulus>yNHKo0x4m4PMsMKjP3iOCD5EBKjpk1LeCwZI21Tt+ZMxJ7oXmhR9VVLnyfwA3K78IuHwr1RuWVInG/RS2jChqI9ZSRJ4JS161JKkBOLNHzKyzskULlGE/l4RSLizQMVVwXnVedwPN8gi/Ey88nwHPjRvt+4t4rFH3vNoh9BBbWk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        #region interface
        public string Host { get; set; }

        public int Port { get; set; }

        /// <summary>
        /// 异步连接
        /// </summary>
        public void Connect()
        {
            if (_connecter != null && _connecter.Connected())
            {
                UnityEngine.Debug.LogFormat("{0}.{1} AlreadyConnect {2} {3}", nameof(TCPInstance), nameof(Connect), Host, Port);
                _eventDispatcher.TriggerConnectEvent(NetworkConnectResult.AlreadyConnect);
                return;
            }

            _connecter?.Dispose();
            _connecter = new TCPConnecter();
            _connecter.onConnectResult += OnConnectResult;
            _connecter.onDisconnect += OnDisconnect;

            UnityEngine.Debug.LogFormat("{0}.{1} Connect {2} {3}", nameof(TCPInstance), nameof(Connect), Host, Port);
            _connecter.Connect(Host, Port);
        }

        /// <summary>
        /// 异步重连：使用原有IP、Port重新连接服务器，原有注册的网络回调函数保持有效
        /// </summary>
        public void Reconnect()
        {
            ResetConnect();
            Connect();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            _connecter?.Disconnect();
        }

        /// <summary>
        /// 获得连接状态
        /// </summary>
        /// <returns>连接状态</returns>
        public bool Connected
        {
            get
            {
                if (_connecter != null)
                {
                    return _connecter.Connected();
                }

                return false;
            }
        }

        /// <summary>
        /// 设置RSA加密的公钥,如果需要请在连接前设置
        /// </summary>
        /// <param name="key"></param>
        public void SetRSAPublicKey(string key)
        {
            _rsaPubKey = key;
        }

        /// <summary>
        /// 设置AES加密key，请在连接成功后设置（提前设置无效）
        /// </summary>
        /// <param name="key"></param>
        public void SetAESKey(byte[] key)
        {
            _receiver?.SetCryptKey(key);
            _sender?.SetCryptKey(key);
        }

        public void SetSendAESKey(byte[] key)
        {
            _sender?.SetCryptKey(key);
        }

        public void SetRecvAESKey(byte[] key)
        {
            _receiver?.SetCryptKey(key);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="crypted">是否加密，如果为false，会由网络模块内部策略决定是否加密。
        ///     敏感协议建议传入参数true，其他协议用false。
        /// </param>
        public void SendMsg(IMessage msg, bool crypted = false)
        {
#if ENABLE_MSG_LOG
            UnityEngine.Debug.LogFormat("Network: SendMsg {0}", msg);
#endif
            _sender?.SendMsg(msg, crypted);
        }

        public void SendMsg(uint msgID, byte[] buffer, int buffLen = -1, bool crypted = false)
        {
            _sender?.SendMsg(msgID, buffer, buffLen, crypted);
        }

        public void RegisterMsg<T>(object owner, Action<T> handler)
        {
            _eventDispatcher.RegisterMsg<T>(owner, handler);
        }

        public void UnregisterMsg<T>(object owner)
        {
            _eventDispatcher.UnregisterMsg<T>(owner);
        }

        public void RegisterMsg(Type msgType, object owner, Action<IMessage> handler)
        {
            _eventDispatcher.RegisterMsg(msgType, owner, handler);
        }

        public void UnregisterMsg(Type msgType, object owner)
        {
            _eventDispatcher.UnregisterMsg(msgType, owner);
        }

        public void RegisterMsg(object owner, uint msgID, Action<uint, byte[]> handler)
        {
            _eventDispatcher.RegisterMsg(owner, msgID, handler);
        }

        public void UnregisterMsg(object owner, uint msgID)
        {
            _eventDispatcher.UnregisterMsg(owner, msgID);
        }

        public void UnregisterMsgs(object owner)
        {
            _eventDispatcher.UnregisterMsgs(owner);
        }

        public void RegisterOneTimeMsg<T>(Action<IMessage> handler)
        {
            _eventDispatcher.RegisterOneTimeMsg<T>(handler);
        }

        public void RegisterOneTimeMsg(Type msgType, Action<IMessage> handler)
        {
            _eventDispatcher.RegisterOneTimeMsg(msgType, handler);
        }

        public void UnregisterOneTimeMsg<T>(Action<IMessage> handler)
        {
            _eventDispatcher.UnregisterOneTimeMsg<T>(handler);
        }

        public void UnregisterOneTimeMsg(Type msgType, Action<IMessage> handler)
        {
            _eventDispatcher.UnregisterOneTimeMsg(msgType, handler);
        }

        public void ClearAllOneTimeMsgs()
        {
            _eventDispatcher.ClearAllOneTimeMsgs();
        }

        public void RegisterConnect(Action<NetworkConnectResult> handler)
        {
            _eventDispatcher.RegisterConnectEvent(handler);
        }

        public void UnregisterConnect(Action<NetworkConnectResult> handler)
        {
            _eventDispatcher.UnregisterConnectEvent(handler);
        }

        public void RegisterDisconnect(Action handler)
        {
            _eventDispatcher.RegisterDisconnectEvent(handler);
        }

        public void UnregisterDisconnect(Action handler)
        {
            _eventDispatcher.UnregisterDisconnectEvent(handler);
        }
        #endregion


        #region internal
        internal bool Disposed { get; private set; }

        internal TCPInstance(string host, int port, NetworkConnectType connectType)
        {
            Host = host;
            Port = port;
            _connectType = connectType;
            Disposed = false;
            _eventDispatcher = new NetEventDispatcher();
        }


        internal void Dispose()
        {
            Disposed = true;

            _connecter?.Dispose();
            _receiver?.Dispose();
            _sender?.Dispose();

            _connecter = null;
            _receiver = null;
            _sender = null;
            _eventDispatcher = null;
        }

        internal void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (!Disposed)
            {
                if ((_receiver != null && _receiver.Disposed) || (_sender != null && _sender.Disposed))
                {
                    Dispose();
                    return;
                }

                _eventDispatcher.Update(elapseSeconds, realElapseSeconds);
                _receiver?.Update(elapseSeconds, realElapseSeconds);
                _connecter?.Update(elapseSeconds, realElapseSeconds);
                _keyExchanger?.Update(elapseSeconds, realElapseSeconds);
            }
        }

        private void ResetConnect()
        {
            _eventDispatcher.StopDispactch = true;
            _eventDispatcher.ClearAllOneTimeMsgs();

            _connecter?.Dispose();
            _connecter = null;
            _receiver?.Dispose();
            _receiver = null;
            _sender?.Dispose();
            _sender = null;

            _eventDispatcher.StopDispactch = false;
        }

        void OnConnectResult(NetworkConnectResult result)
        {
            if (result == NetworkConnectResult.Succeed)
            {
                if (_connectType == NetworkConnectType.Default)
                {
                    _receiver = new TCPReceiver(_connecter.TCPClient, _eventDispatcher);
                    _sender = new TCPSender(_connecter.TCPClient);
                }
                else if (_connectType == NetworkConnectType.ExchangeKey)
                {
                    _keyExchanger = new CryptoKeyExchanger(_connecter.TCPClient, _rsaPubKey, OnExchangeKey);
                    return;
                }
            }
            _eventDispatcher.TriggerConnectEvent(result);
        }

        void OnDisconnect()
        {
            _eventDispatcher.TriggerDisConnectEvent();
        }

        void OnExchangeKey(bool success, byte[] aesKey)
        {
            if (success)
            {
                _receiver = new TCPReceiver(_connecter.TCPClient, _eventDispatcher);
                _sender = new TCPSender(_connecter.TCPClient);
                _keyExchanger = null;
                SetAESKey(aesKey);
                _eventDispatcher.TriggerConnectEvent(NetworkConnectResult.Succeed);
            }
            else
            {
                _keyExchanger = null;
                ResetConnect();
                _eventDispatcher.TriggerConnectEvent(NetworkConnectResult.Failed);
            }
        }
        #endregion
    }
}