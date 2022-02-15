/********************************************************************
	created:	2020/09/15
	author:		ZYL
	
	purpose:	网络模块常量定义
*********************************************************************/

namespace iFramework
{
    /// <summary>
    /// 网络连接状态
    /// </summary>
    internal enum NetworkConnectState
    {
        None = 0,               //没有连接对象
        Connected = 1,          //连接上了
        Unconnected = 2         //未连接上
    }

    internal class NetDef
    {
        public const int kVersion = 1;      // 协议版本号
        public const int kCSHeadLen = 13;    // CS协议头长度(字节数)
        public const int kSCHeadLen = 13;
        public const int kMaxCSPktLen = 20000;  // TODO：根据项目具体情况修改这个值
        public const int kMaxSCPktLen = 2000000; // TODO：根据项目具体情况修改这个值
        public const byte kFlag_Crypto = 0x02;
        public const byte kFlag_Compress = 0x08;
        public const byte kFlag_CRC = 0x20;
        public const byte kFlag_Mask = 0xD5;
    }
}
