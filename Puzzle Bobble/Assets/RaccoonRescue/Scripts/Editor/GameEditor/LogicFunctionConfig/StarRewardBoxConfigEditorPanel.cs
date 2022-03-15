using UnityEditor;
using UnityEngine;

public class StarRewardBoxConfigEditorPanel : IGameEditorPanel
{
    bool starReward;
    void IGameEditorPanel.Draw(LevelEditor levelEditor, LevelEditorBase levelEditorBase)
    {
        starReward = EditorGUILayout.Foldout(starReward, "星星宝箱");
        if (starReward)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.BeginVertical();
            var save_value = levelEditorBase.StarRewardBoxConfigs.MaxStar;
            levelEditorBase.StarRewardBoxConfigs.MaxStar = EditorGUILayout.IntField("要求星星数", levelEditorBase.StarRewardBoxConfigs.MaxStar, new GUILayoutOption[] {
                GUILayout.Width(200),
                GUILayout.MaxWidth(200),
            });
            if (save_value != levelEditorBase.StarRewardBoxConfigs.MaxStar)
                levelEditor.SaveItem();

            var save_value1 = levelEditorBase.StarRewardBoxConfigs.Gems;
            levelEditorBase.StarRewardBoxConfigs.Gems = EditorGUILayout.IntField("必得金币数", levelEditorBase.StarRewardBoxConfigs.Gems, new GUILayoutOption[] {
                GUILayout.Width(200),
                GUILayout.MaxWidth(200),
            });
            if (save_value1 != levelEditorBase.StarRewardBoxConfigs.Gems)
                levelEditor.SaveItem();
            GUILayout.Space(10);
            DrawStarRewardPoolConfig(levelEditor, levelEditorBase);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }


    void DrawStarRewardPoolConfig(LevelEditor levelEditor, LevelEditorBase levelEditorBase)
    {
        var itemRandoms = levelEditorBase.StarRewardBoxConfigs.rewardPool;
        GUILayout.BeginVertical();
        for (int i = 0; i <= 20; i++)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < 3; j++)
            {
                int itemIndex = i * 3 + j;
                if (itemIndex > itemRandoms.Count)
                    break;

                if (itemIndex < itemRandoms.Count)
                {
                    var randomItem = itemRandoms[itemIndex];
                    GUILayout.BeginVertical();
                    if (randomItem.item.Draw())
                    {
                        levelEditor.SaveItem();
                    }
                    var save_value = randomItem.weight;
                    randomItem.weight = EditorGUILayout.IntField("概率1-100", randomItem.weight, new GUILayoutOption[] {
                        GUILayout.Width(200),
                        GUILayout.MaxWidth(200),
                    });

                    GUILayout.EndVertical();
                    if (randomItem.weight != save_value)
                        levelEditor.SaveItem();
                }
                else
                {
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("+", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(25) }))
                    {
                        itemRandoms.Add(new ItemRandom());
                        levelEditor.SaveItem();
                    }
                    if (itemRandoms.Count > 0 &&
                        GUILayout.Button("-", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(25) }))
                    {
                        itemRandoms.RemoveAt(itemRandoms.Count - 1);
                        levelEditor.SaveItem();
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
        GUILayout.EndVertical();
    }
}