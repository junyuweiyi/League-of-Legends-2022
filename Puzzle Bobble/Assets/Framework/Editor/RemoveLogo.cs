using UnityEditor;
using UnityEditor.Callbacks;
public class RemoveLogo
{
    [InitializeOnLoadMethod]
    public static void ClearLogo()
    {
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;
    }

    [PostProcessScene]
    public static void ClearLogo2()
    {
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;
    }
}
