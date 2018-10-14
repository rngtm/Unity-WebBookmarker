using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace WebBookmarker
{
    public class UrlEditWindow : EditorWindow
    {
        private const float TitleTextFieldWidth = 90f;

        private readonly Color DarkRed = new Color(0.9f, 0f, 0f, 1f);
        private readonly GUIContent ContentFileNotFound = new GUIContent("bookmark data not found");
        private const float TextFieldSizeY = 16f;
        private const float TextFieldMarginLeft = 1f;
        private const float TextFieldMarginRight = 1f;
        private const float TextFieldMarginTop = 2f;
        private const float TextFieldMarginBottom = 3f;
        private const float FieldSpace = 4f;
        private const string ListHeaderText = "URL List";
        private const string DeleteButtonLabel = "Remove";
        [NonSerialized] private ReorderableList m_List;
        [NonSerialized] private BookmarkData m_Data;
        [NonSerialized] private GUIStyle m_RedTextLabelStyle = null;
        [NonSerialized] private GUIStyle m_ButtonStyle = null;
        [NonSerialized] private GUIStyle m_TextFieldStyle = null;
        private GUIStyle RedTextLabelStyle => m_RedTextLabelStyle ?? (m_RedTextLabelStyle = CustomUI.CreateColorLabelStyle(DarkRed));
        private GUIStyle DeleteButtonStyle => m_ButtonStyle ?? (m_ButtonStyle = CreateDeleteButtonStyle());
        private GUIStyle TextFieldStyle => m_TextFieldStyle ?? (m_TextFieldStyle = CreateTextFieldStyle());

        private GUIStyle CreateDeleteButtonStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 10;
            return style;
        }

        private GUIStyle CreateTextFieldStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.textField);
            style.alignment = TextAnchor.MiddleLeft;
            return style;
        }


        [MenuItem(EditorSettings.MENU_TEXT_EDITOR, false, EditorSettings.MENU_ORDER_EDITOR)]
        public static void Open()
        {
            var window = GetWindow<UrlEditWindow>(EditorSettings.WINDOW_TITLE_EDITOR);
        }

        private void OnEnable()
        {
            m_Data = m_Data ?? (BookmarkData)AssetDatabase.LoadAssetAtPath(EditorSettings.DATA_PATH, typeof(BookmarkData));
        }

        private void OnGUI()
        {
            if (m_Data == null)
            {
                EditorGUILayout.LabelField(ContentFileNotFound, RedTextLabelStyle);
                m_Data = (BookmarkData)EditorGUILayout.ObjectField(m_Data, typeof(BookmarkData), false);
            }

            if (m_List == null)
            {
                CreateList();
            }
            m_List.DoLayoutList();
        }

        private void CreateList()
        {
            m_List = new ReorderableList(m_Data.GetList(), typeof(UrlData));
            m_List.elementHeight = TextFieldSizeY + TextFieldMarginTop + TextFieldMarginBottom;

            m_List.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.x += TextFieldMarginLeft;
                rect.y += TextFieldMarginTop;
                rect.height = TextFieldSizeY;

                var element = (UrlData)m_List.list[index];

                var titleTextRect = new Rect(rect);
                titleTextRect.width = TitleTextFieldWidth;
                element.Title = EditorGUI.TextField(titleTextRect, element.Title, TextFieldStyle);

                var urlTextRect = new Rect(rect);
                urlTextRect.x = titleTextRect.xMax + FieldSpace;
                urlTextRect.width -= titleTextRect.width + FieldSpace + TextFieldMarginRight;
                element.URL = EditorGUI.TextField(urlTextRect, element.URL, TextFieldStyle);
            };

            m_List.onChangedCallback += (ReorderableList list) =>
            {
                EditorUtility.SetDirty(m_Data);
            };

            // ヘッダー
            Rect headerRect = default(Rect);
            m_List.drawHeaderCallback = (rect) =>
            {
                headerRect = rect;
                rect.x -= 1;
                GUI.Label(rect, "Editor");
    
            };

            // フッター
            m_List.drawFooterCallback = (rect) =>
            {
                //rect.x = headerRect.x - headerRect.width + 40;
                rect.y = headerRect.y + 3f; // ヘッダー位置に合わせる
                ReorderableList.defaultBehaviours.DrawFooter(rect, m_List);
            };
        }
    }
}