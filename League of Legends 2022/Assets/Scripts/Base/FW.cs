/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	游戏启动类。
                初始化游戏框架，提供框架类的快速获取接口；
                初始化游戏状态机，游戏其他功能和流程交由状态机管理。
*********************************************************************/
using UnityEngine;
using iFramework;

public class FW : MonoBehaviour
{
    #region Framework Managers
    public static IDataMgr DataMgr { get; private set; }
    public static IObjectPoolMgr PoolMgr { get; private set; }
    public static IFSMMgr FSMMgr { get; private set; }
    public static INetworkMgr NetMgr { get; private set; }
    public static IResourceMgr ResourceMgr { get; private set; }
    public static IUIMgr UIMgr { get; private set; }
    public static ITimerMgr Timer { get; private set; }
    public static ILocalizationMgr Localization { get; private set; }
    public static ILegacyAudioMgr AudioMgr { get; private set; }
    public static IWwiseAudioMgr AudioWwise { get; private set; }

    /// <summary>
    /// 事件管理器<br/>
    /// 尽量少用事件管理器，只有对象之间无法直接建立关联时才用
    /// </summary>
    /// <value></value>
    public static IEventMgr EventMgr { get; private set; }
    #endregion


    #region Framework Objects
    public static IFSM GameFSM { get; private set; }
    public static IObjectPool Pool { get; private set; }
    public static IMaterialSpawner MaterialSpawner { get; private set; }
    #endregion

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitFramework();
    }

    private void Start()
    {
        InitGameStates();
    }

    void Update()
    {
        Framework.Update(Time.deltaTime, Time.unscaledDeltaTime);
    }

    void FixedUpdate()
    {
        Physics.SyncTransforms();
    }

    private void OnApplicationQuit()
    {
        // 正常情况下不会走到这里，停止编辑器和手机运行时被杀掉会走到这里
        Utility.UnloadActiveScene();
        Framework.Shutdown();
    }

    private void InitFramework()
    {
        DataMgr = Framework.GetModule<IDataMgr>();
        PoolMgr = Framework.GetModule<IObjectPoolMgr>();
        NetMgr = Framework.GetModule<INetworkMgr>();
        ResourceMgr = Framework.GetModule<IResourceMgr>();
        Timer = Framework.GetModule<ITimerMgr>();
        Localization = Framework.GetModule<ILocalizationMgr>();
        EventMgr = Framework.GetModule<IEventMgr>();
        AudioMgr = Framework.GetModule<ILegacyAudioMgr>();
        AudioWwise = Framework.GetModule<IWwiseAudioMgr>();

        FSMMgr = Framework.GetModule<IFSMMgr>();
        UIMgr = Framework.GetModule<IUIMgr>();
        MaterialSpawner = Framework.GetModule<IMaterialSpawner>();

        Pool = PoolMgr.CreatePool(transform, "MainPool");

        PoolMgr.Initialize(ResourceMgr);
        DataMgr.Initialize(ResourceMgr, @"Assets/AssetBundles/Data/");
        Localization.Initialize(DataMgr);
        UIMgr.Initialize(ResourceMgr, @"Assets/AssetBundles/UIs/", Pool, Localization, EventMgr, Timer);
        AudioMgr.Initialize(ResourceMgr, @"Assets/AssetBundles/Audios/");
        AudioWwise.Initialize(ResourceMgr, @"Assets/AssetBundles/SoundBanks/");
    }

    private void InitGameStates()
    {
        GameFSM = FSMMgr.CreateFSM<RootState>("GameState", new RootState());

        GameFSM.Start();
    }

    public static void RestartGame()
    {
        GameFSM.PostMessage(new RestartGameMsg());
    }

    /// <summary>
    /// 一般不要直接调用这个函数
    /// </summary>
    public static void RestartGameImmediately()
    {
        UIMgr.CloseAll();
        Utility.UnloadActiveScene();

        AudioWwise.Clear();
        Timer.ClearAll();
        ResourceMgr.ClearAll();
        GameFSM.Restart();
        GL.Clear(false, true, Color.black); // 避免重启游戏时某些手机花屏
    }
}
