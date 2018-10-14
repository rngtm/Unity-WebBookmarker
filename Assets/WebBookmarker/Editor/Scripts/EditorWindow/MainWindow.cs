using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace WebBookmarker
{
    public class MainWindow : EditorWindow
    {
        readonly Color DarkRed = new Color(0.9f, 0f, 0f, 1f);
        readonly GUIContent ContentFileNotFound = new GUIContent("bookmark data not found");
        private GUIStyle m_RedTextLabelStyle = null;
        private GUIStyle RedTextLabelStyle => m_RedTextLabelStyle ?? (m_RedTextLabelStyle = CustomUI.CreateColorLabelStyle(DarkRed));

        private UrlData urlData = null;
        private UrlTreeView m_TreeView;
        private Vector2 m_TableScroll = new Vector2(0f, 0f);
        private UrlAddPopupContent m_PopupContent;
        private UrlAddPopupContent PopupContent => m_PopupContent ?? (m_PopupContent = CreatePopupContent());
        private bool m_IsOpenPopup = false;
        private bool m_NeedOpenPopup = false;
        private int m_InsertPosition = 0;
        private UrlTreeViewItem m_EditTarget;
        [SerializeField] private BookmarkData m_Data;

        [MenuItem(EditorSettings.MENU_TEXT, false, EditorSettings.MENU_ORDER)]
        static void Open()
        {
            GetWindow<MainWindow>(EditorSettings.WINDOW_TITLE);
        }
        private void OnEnable()
        {
            m_Data = m_Data ?? (BookmarkData)AssetDatabase.LoadAssetAtPath(EditorSettings.DATA_PATH, typeof(BookmarkData));
            UpdateTreeView();
        }

        private void OnFocus()
        {
            UpdateTreeView();
        }
        private void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(m_IsOpenPopup);

            if (m_Data == null)
            {
                EditorGUILayout.LabelField(ContentFileNotFound, RedTextLabelStyle);
                EditorGUI.BeginChangeCheck();
                m_Data = (BookmarkData)EditorGUILayout.ObjectField(m_Data, typeof(BookmarkData), false);
                UpdateTreeView();
            }

            if (m_TreeView == null)
            {
                UpdateTreeView();
            }

            DrawHeader();
            CustomUI.RenderTable(m_TreeView, ref m_TableScroll); // URL一覧を表示
            EditorGUI.EndDisabledGroup();
            
            if (m_NeedOpenPopup)
            {
                m_NeedOpenPopup = false;
                var rect = GUILayoutUtility.GetLastRect(); // OnGUI以外で呼ぶとエラー
                PopupWindow.Show(rect, PopupContent);
            }
        }
        private UrlAddPopupContent CreatePopupContent() // ポップアップ作成
        {
            var popup = new UrlAddPopupContent();
            popup.SetOnOpenAction(() => // ポップアップが開いたときの処理
            {
                m_IsOpenPopup = true;
            });
            popup.SetOnCloseAction(() =>  // ポップアップが閉じたときの処理
            {
                m_IsOpenPopup = false;
                UpdateTreeView();
            });
            popup.SetOnClickAddAction((data) => // URL追加処理
            {   
                m_Data.InsertAt(m_InsertPosition, data);
                PopupContent.editorWindow.Close();
                UpdateTreeView();
            });
            return popup;
        }
        private void UpdateTreeView()
        {
            if (m_Data == null) { return; }
            m_Data.DeleteEmptyItems();
            EditorUtility.SetDirty(m_Data);

            m_TreeView = new UrlTreeView(this);
            m_TreeView.SetDeleteButtonAction((Action<UrlTreeViewItem>)((item) =>
            {
                m_Data.DeleteAt((int)item.id);
                m_TreeView.RegisterURLs(m_Data.GetURLAll().ToArray());
            }));

            if (m_Data != null)
            {
                m_TreeView.RegisterURLs(m_Data.GetURLAll().ToArray());
            }
            else
            {
                m_TreeView.Clear();
            }
            Repaint();
        }
        private void DrawHeader()
        {
            EditorGUI.BeginDisabledGroup(m_Data == null);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("Add", EditorStyles.toolbarButton))
                {
                    StartInsertLast(null);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Edit", EditorStyles.toolbarButton))
                {
                    UrlEditWindow.Open();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }
        internal void StartInsert(int index, UrlData template)
        {
            m_NeedOpenPopup = true;
            m_InsertPosition = index;
            PopupContent.SetUrl(template);
        }
        internal void StartInsertLast(UrlData template) // URLの追加
        {
            m_NeedOpenPopup = true;
            m_InsertPosition = m_Data.Count;
            PopupContent.SetUrl(template);
        }
        internal void DeleteAt(int id) // URL削除
        {
            m_Data.DeleteAt(id);
            UpdateTreeView();
        }
        internal void PingData() // ブックマークデータをハイライト表示
        {
            if (m_Data != null)
            {
                EditorGUIUtility.PingObject(m_Data);
            }
        }
    }
}
