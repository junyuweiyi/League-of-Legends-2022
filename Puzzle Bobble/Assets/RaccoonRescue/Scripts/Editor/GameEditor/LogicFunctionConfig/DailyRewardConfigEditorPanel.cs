using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEngine;

public class DailyRewardConfigEditorPanel : GameEditorPanel
{
    bool active;
    List2SerSerialize.SubList2SerSerialize selectDay;

    protected override void OnDraw()
    {
        active = EditorGUILayout.Foldout(active, "每日奖励");
        if (active)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.BeginVertical();

            if (selectDay != null)
            {
                DrawDayDetail(_levelEditorBase.DailyRewardConfigs.List.IndexOf(selectDay), selectDay);
                GUILayout.Space(10);
            }


            var items = _levelEditorBase.DailyRewardConfigs;
            GUILayout.BeginHorizontal();
            for (int day = 0; day <= 20; day++)
            {

                if (day > items.List.Count)
                    break;

                if (day < items.List.Count)
                {
                    var item = items.List[day];
                    float buttonSize = selectDay == item ? 65 : 50;
                    if (GUILayout.Button($"第{day + 1}天", new GUILayoutOption[] { GUILayout.Width(buttonSize), GUILayout.Height(buttonSize) }))
                    {
                        selectDay = item;
                    }
                }
                else
                {
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("+", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(25) }))
                    {
                        var add = new List2SerSerialize.SubList2SerSerialize();
                        items.List.Add(add);
                        selectDay = add;
                        Save();
                    }
                    if (items.List.Count > 0 &&
                        GUILayout.Button("-", new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(25) }))
                    {
                        items.List.RemoveAt(items.List.Count - 1);
                        selectDay = null;
                        Save();
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    void DrawDayDetail(int day, List2SerSerialize.SubList2SerSerialize dayData)
    {
        GUILayout.BeginVertical();
        GUILayout.Label($"第{day + 1}天奖励", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < 5; i++)
        {
            if (i > dayData.List.Count)
                break;
            if (i < dayData.List.Count)
            {
                var item = dayData.List[i];
                GUILayout.BeginVertical();
                if (item.Draw())
                {
                    Save();
                }
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical();
                if (GUILayout.Button("添加当天奖励", new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(25) }))
                {
                    dayData.List.Add(new Item());
                    Save();
                }
                if (dayData.List.Count > 0 &&
                    GUILayout.Button("删除当天奖励", new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(25) }))
                {
                    dayData.List.RemoveAt(dayData.List.Count - 1);
                    Save();
                }
                GUILayout.EndVertical();
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
}
