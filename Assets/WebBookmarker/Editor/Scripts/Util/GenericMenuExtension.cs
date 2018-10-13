using UnityEngine;
using UnityEditor;

namespace WebBookmarker
{
    internal static class GenericMenuExtension
    {
        public static void AddClickItem(this GenericMenu menu, GUIContent content, bool active, GenericMenu.MenuFunction func)
        {
            if (active)
            {
                menu.AddItem(content, false, func);
            }
            else
            {
                menu.AddDisabledItem(content);
            }
        }
    }
}
