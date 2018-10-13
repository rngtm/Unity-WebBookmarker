using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.IMGUI.Controls;

namespace WebBookmarker
{
    internal static class CustomUI
    {
        private static GUIStyle m_TableListStyle;
        private static readonly GUILayoutOption[] ControlRectOption = new GUILayoutOption[]
        {
            GUILayout.ExpandHeight(true),
            GUILayout.ExpandWidth(true),
        };

        private static GUIStyle CreateTableListStyle()
        {
            var style = new GUIStyle("CN Box");
            style.margin.top = 0;
            style.padding.left = 3;
            return style;
        }

        public static void RenderTable(TreeView treeView, ref Vector2 scroll)
        {
            if (m_TableListStyle == null)
            {
                m_TableListStyle = CreateTableListStyle();
            }

            EditorGUILayout.BeginVertical(m_TableListStyle);
            GUILayout.Space(2f);
            scroll = EditorGUILayout.BeginScrollView(scroll);

            var controlRect = EditorGUILayout.GetControlRect(ControlRectOption);
            treeView?.OnGUI(controlRect);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        public static GUIStyle CreateColorLabelStyle(Color color)
        {
            var style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = color;
            return style;
        }
    }
}
