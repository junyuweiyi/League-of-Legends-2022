using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ItemConfigEditorPanel : IGameEditorPanel
{
    ItemConfig selectItemConfig;
    List<ItemConfig> itemConfigs;

    void IGameEditorPanel.Draw(LevelEditor levelEditor, LevelEditorBase levelEditorBase)
    {
        if (itemConfigs == null)
            itemConfigs = levelEditorBase.itemConfig;

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

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add before", new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(25) }))
            {
                selectItemConfig = InsertBefore(selectItemConfig);
                levelEditor.SaveItem();
            }
            if (GUILayout.Button("Add after", new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(25) }))
            {
                selectItemConfig = InsertAfter(selectItemConfig);
                levelEditor.SaveItem();
            }
            if (GUILayout.Button("Delete", new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(25) }))
            {
                Remove(selectItemConfig);
                SelectDefault();
                levelEditor.SaveItem();

            }

            GUILayout.EndHorizontal();
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
                    float buttonSize = selectItemConfig == item ? 65 : 50;
                    if (item.Icon != null)
                    {
                        Texture2D tex = item.Icon.texture;
                        if (GUILayout.Button(tex, new GUILayoutOption[] { GUILayout.Width(buttonSize), GUILayout.Height(buttonSize) }))
                        {
                            selectItemConfig = item;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("", new GUILayoutOption[] { GUILayout.Width(buttonSize), GUILayout.Height(buttonSize) }))
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
                        selectItemConfig = Add();
                        levelEditor.SaveItem();
                    }
                    if (GUILayout.Button("-", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(25) }))
                    {
                        RemoveAt(levelEditorBase.itemConfig.Count - 1);
                        SelectDefault();
                        levelEditor.SaveItem();
                    }
                    GUILayout.EndVertical();
                }

            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    ItemConfig Add()
    {
        var result = new ItemConfig();
        result.ItemID = itemConfigs.Max(p => p.ItemID) + 1;
        itemConfigs.Add(result);
        return result;
    }

    void Remove(ItemConfig itemConfig)
    {
        itemConfigs.Remove(itemConfig);
    }

    void RemoveAt(int index)
    {
        itemConfigs.RemoveAt(index);
    }

    ItemConfig InsertBefore(ItemConfig reference)
    {
        var result = new ItemConfig();
        result.ItemID = itemConfigs.Max(p => p.ItemID) + 1;
        int referenceIndex = itemConfigs.IndexOf(reference);
        itemConfigs.Insert(referenceIndex, result);
        return result;
    }

    ItemConfig InsertAfter(ItemConfig reference)
    {
        var result = new ItemConfig();
        result.ItemID = itemConfigs.Max(p => p.ItemID) + 1;
        int referenceIndex = itemConfigs.IndexOf(reference);
        itemConfigs.Insert(referenceIndex + 1, result);
        return result;
    }

    void SelectDefault()
    {
        if (itemConfigs.Count == 0)
        {
            selectItemConfig = null;
            return;
        }
        selectItemConfig = itemConfigs[0];

    }
}
