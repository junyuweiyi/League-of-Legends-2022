using UnityEditor;
using UnityEngine;

namespace GameEditor.ToolBox
{
    /// <summary>
    /// 在Hierarchy面板每个对象右侧添加一个勾选框
    /// </summary>
    [InitializeOnLoad]
    public static class VisibleInHierarchy
    {
        static VisibleInHierarchy()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowOnGui;

            var showCheckbox = EditorPrefs.GetBool("ShowHierarchyCheckbox", true);
            Menu.SetChecked("GameObject/显示Hierarchy界面的勾勾", showCheckbox);
        }

        private static void HierarchyWindowOnGui(int instanceId, Rect selectionRect)
        {
            var showCheckbox = EditorPrefs.GetBool("ShowHierarchyCheckbox", true);
            if (!showCheckbox) return;
            // 计算CheckBox的位置和尺寸
            var rectCheckBox = new Rect(selectionRect);
            rectCheckBox.x += rectCheckBox.width - 18;
            rectCheckBox.width = 18;

            var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (go == null)
            {
                return;
            }

            // if (PrefabUtility.IsAnyPrefabInstanceRoot(go))
            // {
            //     rectCheckBox.x -= 18;
            // }

            // 绘制CheckBox
            var oldValue = go.activeSelf;
            var newValue = GUI.Toggle(rectCheckBox, oldValue, string.Empty);
            if (oldValue != newValue)
            {
                Undo.RecordObject(go, "Toggle Active State");
                go.SetActive(newValue);
                EditorUtility.SetDirty(go);
            }
        }

        [MenuItem("GameObject/显示Hierarchy界面的勾勾")]
        private static void ToggleCheckbox()
        {
            var curVal = EditorPrefs.GetBool("ShowHierarchyCheckbox", true);
            curVal = !curVal;
            EditorPrefs.SetBool("ShowHierarchyCheckbox", curVal);
            Menu.SetChecked("GameObject/显示Hierarchy界面的勾勾", curVal);
            EditorApplication.RepaintHierarchyWindow();
        }

        [MenuItem("Tools/Hierarchy/切换GameObject激活状态 _F1")]
        private static void ToggleActiveState()
        {
            var objs = Selection.objects;
            Undo.RecordObjects(objs, "ToggleGameObjectActive");
            foreach (var obj in objs)
            {
                if (obj is GameObject go && go != null && go.scene.IsValid())
                {
                    go.SetActive(!go.activeSelf);
                    EditorUtility.SetDirty(go);
                }
            }
        }
    }
}
