using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace WebBookmarker
{
    public class UrlTreeViewItem : TreeViewItem
    {
        public UrlData Data { get; set; } = new UrlData();
    }

    public class UrlTreeView : TreeView
    {
        static readonly int RowHeight = 12;
        static readonly string SortedColumnIndexStateKey = "WebBookmarkerTreeView_sortedColumnIndex";
        static readonly GUIContent ContentMenuOpen = new GUIContent("開く");
        static readonly GUIContent ContentMenuEdit = new GUIContent("編集");
        static readonly GUIContent ContentMenuInsert = new GUIContent("URLの挿入");
        static readonly GUIContent ContentMenuDelete = new GUIContent("削除");
        static readonly GUIContent ContentButtonAdd = new GUIContent("Add");

        static readonly int DefaultSortedColumnIndex = 1;
        public IReadOnlyList<TreeViewItem> CurrentBindingItems;
        private System.Action<UrlTreeViewItem> m_OnClickDeleteButtonActon = (item) => { };
        private MainWindow m_MainWindow;
        private GUIStyle m_LabelStyle;
        private UrlTreeViewItem m_EditTarget = null;
        private UrlData m_EditData = new UrlData();

        private bool IsEditing => m_EditTarget != null;

        public UrlTreeView(MainWindow mainWindow) // constructer
            : this(new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[]
            {
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("Title"), // name
                    autoResize = false,
                    width = 120f,
                    },
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("URL"),
                    },
            })))
        {

            m_MainWindow = mainWindow;
        }
        public UrlTreeView(TreeViewState state, MultiColumnHeader header) // constructer
            : base(state, header)
        {
            rowHeight = RowHeight;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            header.sortingChanged += Header_sortingChanged;
            header.ResizeToFit();
            Reload();

            header.sortedColumnIndex = SessionState.GetInt(SortedColumnIndexStateKey, DefaultSortedColumnIndex);
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            HandleKeyDown();
            HandleMouseDown();
        }

        protected override void ContextClicked() // 右クリックメニュー
        {
            base.ContextClicked();

            int id = -1;
            UrlTreeViewItem item = null;
            if (HasSelection())
            {
                id = GetSelection()[0];
                item = (UrlTreeViewItem)GetRows()[id];
            }

            base.ContextClicked();
            GenericMenu menu = new GenericMenu();

            menu.AddClickItem(ContentMenuOpen, item != null && !string.IsNullOrEmpty(item.Data.URL), () =>
            {
                System.Diagnostics.Process.Start(item.Data.URL);
            });
            menu.AddClickItem(ContentMenuEdit, item != null, () =>
            {
                StartEdit(id);
            });
            menu.AddSeparator("");
            menu.AddItem(ContentMenuInsert, false, () =>
            {
                if (item != null)
                {
                    m_MainWindow.StartInsert(id + 1, new UrlData());
                }
                else
                {
                    m_MainWindow.StartInsertLast(new UrlData());
                }
            });
            menu.AddClickItem(ContentMenuDelete, item != null, () =>
            {
                m_MainWindow.DeleteAt(id);
            });
            menu.ShowAsContext();
        }
        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            var item = (UrlTreeViewItem)GetRows()[id];
            var openURL = item.Data.URL;
            if (!string.IsNullOrEmpty(openURL))
            {
                System.Diagnostics.Process.Start(openURL);
            }
        }
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }
        public void SetDeleteButtonAction(System.Action<UrlTreeViewItem> action)
        {
            m_OnClickDeleteButtonActon = action;
        }
        protected override void RowGUI(RowGUIArgs args) // draw gui
        {
            if (m_LabelStyle == null)
            {
                m_LabelStyle = new GUIStyle(EditorStyles.label);
            }

            var item = args.item as UrlTreeViewItem;
            for (var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                var rect = args.GetCellRect(visibleColumnIndex);
                var columnIndex = args.GetColumn(visibleColumnIndex);

                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStyle.alignment = TextAnchor.UpperLeft;

                var textFieldStyle = EditorStyles.textField;

                bool isEdit = m_EditTarget != null && m_EditData != null && item.id == m_EditTarget.id;
                switch (columnIndex)
                {
                    case 0:
                        if (isEdit)
                        {
                            rect.x += 2;
                            rect.width -= 4;
                            rect.y += 1;
                            rect.height -= 2;
                            m_EditData.Title = EditorGUI.TextField(rect, m_EditData.Title, textFieldStyle);
                        }
                        else
                        {
                            rect.x += 4;
                            EditorGUI.LabelField(rect, item.Data.Title, labelStyle);
                        }
                        break;
                    case 1:
                        if (isEdit)
                        {
                            rect.x += 2;
                            rect.width -= 4;
                            rect.y += 1;
                            rect.height -= 2;
                            m_EditData.URL = EditorGUI.TextField(rect, m_EditData.URL, textFieldStyle);
                        }
                        else
                        {
                            EditorGUI.LabelField(rect, item.Data.URL, labelStyle);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                }
            }
        }
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { depth = -1 };
            if (CurrentBindingItems == null || CurrentBindingItems.Count == 0)
            {
                var children = new List<TreeViewItem>();
                CurrentBindingItems = children;
            }

            root.children = CurrentBindingItems as List<TreeViewItem>;
            return root;
        }
        public void RegisterURLs(UrlData[] datas)
        {
            var root = new TreeViewItem { depth = -1 };
            var children = new List<TreeViewItem>();
            for (int dataIndex = 0; dataIndex < datas.Length; dataIndex++)
            {
                var data = datas[dataIndex];
                children.Add(new UrlTreeViewItem
                {
                    id = dataIndex,
                    Data = data,
                });
            }

            CurrentBindingItems = children;
            root.children = CurrentBindingItems as List<TreeViewItem>;
            Reload();
        }
        public void Clear()
        {
            var root = new TreeViewItem { depth = -1 };
            var children = new List<TreeViewItem>();
            CurrentBindingItems = children;
            root.children = CurrentBindingItems as List<TreeViewItem>;
            Reload();
        }
        private void Header_sortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SessionState.SetInt(SortedColumnIndexStateKey, multiColumnHeader.sortedColumnIndex);
            var index = multiColumnHeader.sortedColumnIndex;
            var ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);
            var items = rootItem.children.Cast<UrlTreeViewItem>();

            // sorting
            IOrderedEnumerable<UrlTreeViewItem> orderedEnumerable;
            switch (index)
            {
                case 0:
                    orderedEnumerable = ascending ? items.OrderBy(item => item.Data.Title) : items.OrderByDescending(item => item.Data.Title);
                    break;
                case 1:
                    orderedEnumerable = ascending ? items.OrderBy(item => item.Data.URL) : items.OrderByDescending(item => item.Data.URL);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }

            CurrentBindingItems = rootItem.children = orderedEnumerable.Cast<TreeViewItem>().ToList();
            for (int i = 0; i < CurrentBindingItems.Count; i++)
            {
                var item = CurrentBindingItems[i];
                item.id = i;
            }
            BuildRows(rootItem);
        }
        private void StartEdit(int id)
        {
            var item = (UrlTreeViewItem)GetRows()[id];
            m_EditData.Title = item.Data.Title;
            m_EditData.URL = item.Data.URL;
            m_EditTarget = item;
        }
        private void CancelEdit()
        {
            m_EditTarget = null;
        }
        private void EndEdit()
        {
            if (m_EditTarget != null)
            {
                m_EditTarget.Data.Title = m_EditData.Title;
                m_EditTarget.Data.URL = m_EditData.URL;
                m_EditTarget = null;
            }
        }
        private void HandleKeyDown()
        {
            if (Event.current.type != EventType.KeyDown) { return; }

            switch (Event.current.keyCode)
            {
                case KeyCode.Escape:
                    CancelEdit();
                    Repaint();
                    break;
                case KeyCode.Return:
                    EndEdit();
                    Repaint();
                    break;
            }
        }
        private void HandleMouseDown()
        {
            if (Event.current.type != EventType.MouseDown) { return; }

            if (Event.current.button == 0) // left click
            {
                if (IsEditing)
                {
                    EndEdit();
                }
                else
                {
                    SetSelection(new int[0]); // unselect
                }
            }
        }
    }
}
