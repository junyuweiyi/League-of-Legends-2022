/********************************************************************
	created:	2020/09/15
	author:		ZYL
	
	purpose:	
*********************************************************************/
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace iFramework
{
    /// <summary>
    /// 网络连接器
    /// </summary>
    internal class TCPConnecter
    {
        public Action onDisconnect;
        public Action<NetworkConnectResult> onConnectResult;

        public TcpClient TCPClient { get; private set; } // TcpClient 只能连接一次，一旦 Close() 就不能再用

        private ConnectState _connectState = ConnectState.NotStarted;

        const float kConnectTimeout = 10f;

        private float _connectElapsedTime = 0f;

        public TCPConnecter()
        {
            TCPClient = new TcpClient(GetAddressFamily());
        }

        public void Dispose()
        {
            if (TCPClient == null)
                return;

            try
            {
                if (_connectState == ConnectState.Connected)
                {
                    SetDisconnect();
                }
            }
            finally
            {
                _connectState = ConnectState.NotStarted;
            }
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (TCPClient == null) return;

            if (_connectState == ConnectState.Connecting)
            {
                _connectElapsedTime += realElapseSeconds;
                if (_connectElapsedTime >= kConnectTimeout)
                {
                    Debug.LogWarningFormat("{0}.{1} 连接超时", nameof(TCPConnecter), nameof(Update));
                    _connectState = ConnectState.ConnectOver;

                    // 看文档 Close() 不会抛异常，会触发 BeginConnect() 的回调
                    TCPClient.Close();
                    TCPClient = null;
                }
            }

            if (_connectState == ConnectState.ConnectOver)
            {
                if (TCPClient != null && TCPClient.Connected)
                {
                    _connectState = ConnectState.Connected;
                    SendReslut(NetworkConnectResult.Succeed);
                }
                else
                {
                    _connectState = ConnectState.ConnectFailed;
                    SendReslut(NetworkConnectResult.Failed);
                }
            }

            // 这里即使 TCPClient != null，TCPClient.Connected 也可能抛异常，因为调用 Close() 后内部 Socket 为 null
            if (TCPClient != null &&
                !TCPClient.Connected &&
                _connectState == ConnectState.Connected)
            {
                SetDisconnect();
            }
        }

        /// <summary>
        /// 开始异步连接
        /// </summary>
        /// 
        public void Connect(string host, int port)
        {
            if (TCPClient == null)
                return;

            //如果已经连接了，则直接返回结果，不进行连接动作
            if (TCPClient.Connected)
            {
                SendReslut(NetworkConnectResult.AlreadyConnect);
                return;
            }

            try
            {
                _connectState = ConnectState.Connecting;
                _connectElapsedTime = 0f;
                TCPClient.BeginConnect(host, port, OnConnectOver, TCPClient);
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("{0}.{1} 网络连接错误：{2}", nameof(TCPConnecter), nameof(OnConnectOver), e);
                _connectState = ConnectState.ConnectOver;
            }
        }

        /// <summary>
        /// 反连接
        /// </summary>
        public void Disconnect()
        {
            if (TCPClient == null)
                return;

            if (!TCPClient.Connected)
            {
                return;
            }

            SetDisconnect();
        }

        private void SetDisconnect()
        {
            TCPClient.Close();
            TCPClient = null;

            _connectState = ConnectState.NotStarted;
            onDisconnect?.Invoke();
        }

        /// <summary>
        /// 异步连接完成
        /// </summary>
        /// <param name="asyncResult">异步连接的结果</param>
        private void OnConnectOver(IAsyncResult asyncResult)
        {
            //检查连接状态
            TcpClient tcpClient = (TcpClient)asyncResult.AsyncState;
            // 调用 Close() 后内部 Socket 为 null
            if (tcpClient != null && tcpClient.Client != null)
            {
                try
                {
                    tcpClient.EndConnect(asyncResult);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("{0}.{1} 网络连接错误：{2}", nameof(TCPConnecter), nameof(OnConnectOver), e);
                }
                _connectState = ConnectState.ConnectOver;
            }
            else
            {
                Debug.LogWarningFormat("{0}.{1} 网络连接错误：null", nameof(TCPConnecter), nameof(OnConnectOver));
            }
        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result">结果</param>
        public void SendReslut(NetworkConnectResult result)
        {
            onConnectResult?.Invoke(result);
        }

        public bool Connected()
        {
            if (TCPClient != null)
            {
                return TCPClient.Connected;
            }

            return false;
        }

        private AddressFamily GetAddressFamily()
        {
            if (Socket.OSSupportsIPv6)
            {
                return AddressFamily.InterNetworkV6;
            }
            return AddressFamily.InterNetwork;
        }

        enum ConnectState
        {
            NotStarted,
            Connecting,
            ConnectOver,
            Connected,
            ConnectFailed,
        }
    }
}