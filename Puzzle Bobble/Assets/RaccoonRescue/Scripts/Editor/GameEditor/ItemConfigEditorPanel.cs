using UnityEditor;
using UnityEngine;
using System.Linq;

public class ItemConfigEditorPanel : IGameEditorPanel
{
    ItemConfig selectItemConfig;
    void IGameEditorPanel.Draw(LevelEditor levelEditor, LevelEditorBase levelEditorBase)
    {
        if (selectItemConfig != null)
        {
            EditorGUILayout.BeginHorizontal();
            var save_value = selectItemConfig.Icon;
            selectItemConfig.Icon = (Sprite)EditorGUILayout.ObjectField(selectItemConfig.Icon, typeof(Sprite), new GUILayoutOption[] {
                GUILayout.Width (75), GUILayout.Height (75)
            });
            if (save_value != selectItemConfig.Icon)
                levelEditor.SaveItem();

            GUILayout.Space(10);

            //道具ID
            EditorGUILayout.LabelField("道具ID", new GUILayoutOption[] {
                GUILayout.Width (50)
            });
            var save_value2 = selectItemConfig.ItemID;
            selectItemConfig.ItemID = EditorGUILayout.IntField(selectItemConfig.ItemID, new GUILayoutOption[] { GUILayout.Width(115), });
            if (save_value2 != selectItemConfig.ItemID)
                levelEditor.SaveItem();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("道具Type", new GUILayoutOption[] {
                GUILayout.Width (100)
            });
            var save_value3 = selectItemConfig.ItemType;
            selectItemConfig.ItemType = (ItemType)EditorGUILayout.EnumPopup(selectItemConfig.ItemType, new GUILayoutOption[] { GUILayout.Width(115), });
            if (save_value3 != selectItemConfig.ItemType)
                levelEditor.SaveItem();


            var save_value4 = selectItemConfig.ItemSubType;
            if (selectItemConfig.ItemType == ItemType.Boost)
            {
                EditorGUILayout.LabelField("BoostType", new GUILayoutOption[] { GUILayout.Width(100) });
                selectItemConfig.ItemSubType = (int)(BoostType)EditorGUILayout.EnumPopup((BoostType)selectItemConfig.ItemSubType, new GUILayoutOption[] { GUILayout.Width(115), });
            }
            else if (selectItemConfig.ItemType == ItemType.Powerup)
            {
                EditorGUILayout.LabelField("Powerup", new GUILayoutOption[] { GUILayout.Width(100) });
                selectItemConfig.ItemSubType = (int)(Powerups)EditorGUILayout.EnumPopup((Powerups)selectItemConfig.ItemSubType, new GUILayoutOption[] { GUILayout.Width(115), });
            }
            if (save_value4 != selectItemConfig.ItemSubType)
                levelEditor.SaveItem();

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        ShowItemConfigList(levelEditor, levelEditorBase);
    }

    void ShowItemConfigList(LevelEditor levelEditor, LevelEditorBase levelEditorBase)
    {
        GUILayout.BeginVertical();
        for (int j = 0; j < 20; j++)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < 6; i++)
            {
                int index = j * 6 + i;
                if (index > levelEditorBase.itemConfig.Count)
                    break;

                if (index < levelEditorBase.itemConfig.Count)
                {
                    ItemConfig item = levelEditorBase.itemConfig[index];
                    if (item.Icon != null)
                    {
                        Texture2D tex = item.Icon.texture;
                        if (GUILayout.Button(tex, new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
                        {
                            selectItemConfig = item;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) }))
                        {
                            selectItemConfig = item;
                        }
                    }
                }
                else
                {
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("+", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(25) }))
                    {
                        //selectedTab = 1;
                        selectItemConfig = new ItemConfig();
                        selectItemConfig.ItemID = levelEditorBase.itemConfig.Max(p => p.ItemID) + 1;
                        levelEditorBase.itemConfig.Add(selectItemConfig);
                        levelEditor.SaveItem();
                    }
                    if (GUILayout.Button("-", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(25) }))
                    {
                        selectItemConfig = new ItemConfig();
                        levelEditorBase.itemConfig.RemoveAt(levelEditorBase.itemConfig.Count - 1);
                        levelEditor.SaveItem();
                    }
                    GUILayout.EndVertical();
                }

            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }
}
