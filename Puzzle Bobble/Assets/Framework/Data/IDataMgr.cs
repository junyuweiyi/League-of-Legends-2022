/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	数据配置档管理器，请配合数据转档工具使用
                数据表（DataTable）缓存机制说明：
                1.默认使用系统GC机制对创建的对象进行回收
                    a.如果外部持有管理器返回的DataTable对象，管理器会缓存数据表，不会销毁
                    b.如果外部没有对DataTable对象的引用，会在下次GC时被销毁
                    c.外部持有DataUnit的引用不会阻止所对应的DataTable被销毁
                2.可以调用CacheData函数显式控制数据表是否缓存
                3.未缓存DataTable的情况下，通过GetData获取同一个ID的DataUnit对象可能不是同一个对象
                    a.在一个消息循环内多次调用返回的是同一个对象
                    b.不同消息循环内调用的返回不一定
*********************************************************************/

namespace iFramework
{
    /// <summary>
    /// 数据表管理器
    /// </summary>
    public interface IDataMgr
    {
        /// <summary>
        /// 初始化，必须先初始化才能使用
        /// </summary>
        /// <param name="resMgr">资源管理器</param>
        /// <param name="dataPath">数据文件目录</param>
        void Initialize(IResourceMgr resMgr, string dataPath);
        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataFile">数据文件名，默认为数据类型同名文件</param>
        /// <returns></returns>
        DataTable<T> GetDataTable<T>(string dataFile = null) where T : DataUnit, new();
        /// <summary>
        /// 获取指定ID的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="dataFile">数据文件名，默认为数据类型同名文件</param>
        /// <returns></returns>
        T GetData<T>(int id, string dataFile = null) where T : DataUnit, new();
        /// <summary>
        /// 开/关数据表缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache"></param>
        /// <param name="dataFile">数据文件名，默认为数据类型同名文件</param>
        void CacheData<T>(bool cache, string dataFile = null) where T : DataUnit, new();
    }
}
