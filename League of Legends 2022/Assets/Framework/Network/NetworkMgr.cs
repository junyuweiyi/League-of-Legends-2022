/********************************************************************
	created:	2020/09/14
	author:		ZYL
	
	purpose:	网络管理模块实现
*********************************************************************/
using System.Collections.Generic;
using System.Net.Sockets;

namespace iFramework
{
    internal class NetworkMgr : INetworkMgr, IFrameworkModule
    {
        private readonly List<TCPInstance> _tcpInstances = new List<TCPInstance>();

        public int Priority => 1;

        public TCPInstance CreateTcpInstance(string host, int port, NetworkConnectType connectType)
        {
            TCPInstance instance = new TCPInstance(host, port, connectType);
            _tcpInstances.Add(instance);

            return instance;
        }

        public void DestoryTcpInstance(TCPInstance instance)
        {            
            instance?.Dispose();
            _tcpInstances.Remove(instance);
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            for (int i = 0; i < _tcpInstances.Count; i++)
            {
                _tcpInstances[i].Update(elapseSeconds, realElapseSeconds);
            }
        }

        public void Shutdown()
        {
            for (int i = 0; i < _tcpInstances.Count; i++)
            {
                _tcpInstances[i].Dispose();
            }
            _tcpInstances.Clear();
        }

        public bool SupportsIPv6()
        {
            return Socket.OSSupportsIPv6;
        }
    }
}