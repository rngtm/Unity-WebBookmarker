using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace WebBookmarker
{
    public class UrlAddPopupContent : PopupWindowContent
    {
        private const string DEFAULT_CONTROL_NAME = CONTROL_NAME_TEXT_URL; // 最初にフォーカスするUI
        private const string CONTROL_NAME_EMPTY = "";
        private const string CONTROL_NAME_TEXT_TITLE = "ControlName_TitleText";
        private const string CONTROL_NAME_TEXT_URL = "ControlName_TextURL";
        private string m_URL = "";
        private string m_Title = "";
        private Action m_OnOpenAction = () => { };
        private Action m_OnCloseAction = () => { };
        private Action m_OnGUIAction = () => { };
        private Action<UrlData> m_OnClickAddAction = (data) => { };
        private GUIStyle m_TextFieldStyle;
        private GUIStyle m_ButtonStyle;
        private GUIStyle TextFieldStyle => m_TextFieldStyle ?? (m_TextFieldStyle = CreateTextFieldStyle());
        private GUIStyle ButtonStyle => m_ButtonStyle ?? (m_ButtonStyle = CreateButtonStyle());

        private readonly GUILayoutOption[] LabelLayoutOption = new GUILayoutOption[]
        {
            GUILayout.Width(48f)
        };

        public override Vector2 GetWindowSize()
        {
            return new Vector2(240, 86);
        }

        private static GUIStyle CreateTextFieldStyle()
        {
            var style = new GUIStyle(EditorStyles.textField);
            style.margin.right = 6;
            return style;
        }

        private static GUIStyle CreateButtonStyle()
        {
            var style = new GUIStyle(GUI.skin.button);
            style.margin.right = style.margin.left = 4;
            style.margin.bottom = 3;
            style.padding.top = 3;
            return style;
        }

        public void SetUrl(UrlData src)
        {
            m_Title = src?.Title;
            m_URL = src?.URL;
        }

        public void SetOnOpenAction(System.Action action) // 開いたときの処理の登録
        {
            m_OnOpenAction = action;
        }
        public void SetOnCloseAction(System.Action action)  // 閉じたときの処理の登録
        {
            m_OnCloseAction = action;
        }
        public void SetOnClickAddAction(System.Action<UrlData> action) // URL追加処理の登録
        {
            m_OnClickAddAction = action;
        }

        public override void OnGUI(Rect rect)
        {
            // key event handling
            HandleKeyDown();

            EditorGUILayout.LabelField("New URL (Enter to Add)");

            var defaultIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Title", LabelLayoutOption);
                GUI.SetNextControlName(CONTROL_NAME_TEXT_TITLE);
                m_Title = EditorGUILayout.TextField(m_Title, TextFieldStyle);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("URL", LabelLayoutOption);
                GUI.SetNextControlName(CONTROL_NAME_TEXT_URL);
                m_URL = EditorGUILayout.TextField(m_URL, TextFieldStyle);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(!IsAbleSave());
            if (GUILayout.Button("Add", ButtonStyle))
            {
                SaveURL();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel = defaultIndent;

            if (m_OnGUIAction != null)
            {
                m_OnGUIAction.Invoke();
                m_OnGUIAction = null;
            }
        }

        private void HandleKeyDown()
        {
            if (Event.current.type != EventType.KeyDown) { return; }

            switch (Event.current.keyCode)
            {
                case KeyCode.Return:
                    {
                        if (IsAbleSave())
                        {
                            SaveURL();
                            editorWindow.Close();
                        }
                    }
                    break;
                case KeyCode.Escape:
                    switch (GUI.GetNameOfFocusedControl())
                    {
                        case CONTROL_NAME_TEXT_TITLE:
                        case CONTROL_NAME_TEXT_URL:
                            if (string.IsNullOrEmpty(m_URL) && string.IsNullOrEmpty(m_Title))
                            {
                                editorWindow.Close();
                            }
                            else
                            {
                                GUI.FocusControl(CONTROL_NAME_EMPTY);
                                editorWindow.Repaint();
                            }
                            break;
                        default: // not selected
                            editorWindow.Close();
                            break;
                    }
                    break;
            }
        }

        private bool IsAbleSave()
        {
            if (string.IsNullOrEmpty(m_Title) && string.IsNullOrEmpty(m_URL))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void SaveURL()
        {
            m_OnClickAddAction.Invoke(new UrlData
            {
                Title = m_Title,
                URL = m_URL,
            });
        }

        public override void OnOpen()
        {
            m_OnGUIAction = () =>
            {
                GUI.FocusControl(DEFAULT_CONTROL_NAME);
                editorWindow.Repaint();
            };

            m_OnOpenAction.Invoke();
        }
        public override void OnClose()
        {
            m_OnCloseAction.Invoke();
        }
    }
}