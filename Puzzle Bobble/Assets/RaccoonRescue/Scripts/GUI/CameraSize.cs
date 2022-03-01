using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class CameraSize : MonoBehaviour
{
    public int kGameBackgroundWidth = 1536;

    private void Awake()
    {
        Adapter();
    }

    private void Update()
    {
        Adapter();
    }


    void Adapter()
    {
        /**********核心公式：当 orthographicSize = 屏幕高度/2/pixels per unit时，摄像机大小正好与屏幕大小(移动端的实际屏幕大小)相等。*******/
        //假设当前是1080 * 2400的手机，当前游戏背景宽度是1536。
        //根据上方核心公式 -> orthographicSize = 2400 / 2 / 100 = 12, 求出当前屏幕大小相等的摄像机大小orthographicSize。
        //然后观察Scene，发现此orthographicSize时并不能看完全部的游戏背景的宽度
        //故想看1536的宽度的话，根据当前屏幕比例，求出1536对应的高度, 再根据核心公式代入高度计算出当前的orthographicSize：
        //-> 想看的高度 = 1536 / 屏幕比 = 3413.333   此高度下，orthographicSize = 3413.3333 / 2 / 100 = 17.2。 17.2刚好可以看完1536的宽度
        float screenRatio = Screen.width * 1f / Screen.height;
        var wantedWidth = kGameBackgroundWidth;
        var wantedHeight = wantedWidth / screenRatio;
        var orthographicSize = wantedHeight / 100 / 2; //100是 sprite的 pixels per unit
        this.GetComponent<Camera>().orthographicSize = orthographicSize;
    }
}