/********************************************************************
	created:	2020/09/14
	author:		ZYL
	
	purpose:	网络管理模块接口
*********************************************************************/

namespace iFramework
{
    /// <summary>
    /// 网络连接过程的结果状态
    /// </summary>
    public enum NetworkConnectResult
    {
        Failed = 0,             //连接失败
        Succeed = 1,            //连接成功
        AlreadyConnect = 2,     //已经连接
        Unkown = 3              //未知
    }
    /// <summary>
    /// 网络连接方式
    /// </summary>
    public enum NetworkConnectType
    {
        Default = 0,        //简单连接
        ExchangeKey = 1     //连接后与服务器握手，交换AES key
    }
    public interface INetworkMgr
    {
        /// <summary>
        /// 创建TCP对象
        /// </summary>
        /// <param name="host">服务器地址（IP或URL），如果使用ipv6地址，请提前判断是否支持ipv6</param>
        /// <param name="port">端口</param>
        /// <param name="connectType">连接方式</param>
        /// <returns></returns>
        TCPInstance CreateTcpInstance(string host, int port, NetworkConnectType connectType);
        /// <summary>
        /// 销毁TCP对象
        /// </summary>
        /// <param name="instance"></param>
        void DestoryTcpInstance(TCPInstance instance);
        /// <summary>
        /// 系统是否支持IPv6
        /// </summary>
        /// <returns></returns>
        bool SupportsIPv6();
    }
}
