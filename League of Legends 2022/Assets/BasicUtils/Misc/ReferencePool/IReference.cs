
namespace iFramework
{
    /// <summary>
    /// 引用接口。
    /// </summary>
    public interface IReference
    {
        /// <summary>
        /// 清理引用。
        /// </summary>
        void Clear();
    }
}

public static class ReferencePoolExtensions
{
    public static void Release(this iFramework.IReference r)
    {
        iFramework.ReferencePool.Release(r);
    }
}