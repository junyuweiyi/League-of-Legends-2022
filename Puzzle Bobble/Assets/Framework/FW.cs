using UnityEngine;
using iFramework;

public class FW
{
    #region Framework Managers
    public static IDataMgr DataMgr { get; private set; }
    public static IResourceMgr ResourceMgr { get; private set; }
    public static ILocalizationMgr Localization { get; private set; }
    #endregion

    public static void InitFramework()
    {
        DataMgr = new DataMgr();
        ResourceMgr = new ResourceMgr();
        Localization = new LocalizationMgr();

        DataMgr.Initialize(ResourceMgr, "");
        Localization.Initialize();
    }   
}
