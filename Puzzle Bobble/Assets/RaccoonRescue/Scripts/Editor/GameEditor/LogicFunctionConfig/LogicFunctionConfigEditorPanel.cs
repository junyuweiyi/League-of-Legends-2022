using UnityEditor;
using UnityEngine;

/// <summary>
/// 逻辑功能配置项
/// </summary>
public class LogicFunctionConfigEditorPanel : GameEditorPanel
{
    public LogicFunctionConfigEditorPanel()
    {
        RegisterPanel(new StarRewardBoxConfigEditorPanel());
        RegisterPanel(new DailyRewardConfigEditorPanel());
    }    
}

public static class ItemEditorExtend
{
    public static bool Draw(this Item item)
    {
        var save_value = item.ID;
        item.ID = EditorGUILayout.IntField("道具ID", item.ID, new GUILayoutOption[] { GUILayout.Width(200) });

        var save_value2 = item.Count;
        item.Count = EditorGUILayout.IntField("道具数量", item.Count, new GUILayoutOption[] { GUILayout.Width(200) });
        return save_value != item.ID || save_value2 != item.Count;
    }
}

//public class ItemRandomPoolGroup
//{
//    public int group;
//    public ItemRandom itemRandom;
//}