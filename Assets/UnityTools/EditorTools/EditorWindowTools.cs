#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEditor;
using System.Linq;

namespace UnityTools.EditorTools {
    public static class EditorWindowTools 
    {
        // TODO: move this to unity tools module
        public static Type[] GetAllDerivedTypes(this AppDomain aAppDomain, Type aType)
        {
            var result = new List<Type>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(aType))
                        result.Add(type);
                }
            }
            return result.ToArray();
        }
 
        public static Rect GetEditorMainWindowPos()
        {
            var containerWinType = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject)).Where(t => t.Name == "ContainerWindow").FirstOrDefault();
            if (containerWinType == null)
                throw new System.MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");

            var showModeField = containerWinType.GetField("m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var positionProperty = containerWinType.GetProperty("position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (showModeField == null || positionProperty == null)
                throw new System.MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
            
            var windows = Resources.FindObjectsOfTypeAll(containerWinType);
            foreach (var win in windows)
            {
                var showmode = (int)showModeField.GetValue(win);
                if (showmode == 4) // main window
                {
                    var pos = (Rect)positionProperty.GetValue(win, null);
                    return pos;
                }
            }
            throw new System.NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
        }
 
        public static T CenterWindow<T>(this T window) where T : EditorWindow
        {
            var main = GetEditorMainWindowPos();
            var pos = window.position;
            float w = (main.width - pos.width)*0.5f;
            float h = (main.height - pos.height)*0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            window.position = pos;
            return window;
        }
        public static T SetSize<T>(this T window, float width, float height) where T : EditorWindow
        {
            var pos = window.position;
            pos.width = width;
            pos.height = height;
            window.position = pos;
            return window;
        }
    
        public static T OpenWindowNextToInspector<T> (string title) where T : EditorWindow {
            return EditorWindow.GetWindow<T>(title, true, Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll"));
        }
    }
}
#endif