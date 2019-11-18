
using UnityEngine;
using UnityEditor;

namespace CustomEditorTools {

    public class EnumFlagsAttribute : PropertyAttribute { }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))] class EnumFlagsDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            prop.intValue = EditorGUI.MaskField( pos, label, prop.intValue, prop.enumNames );
        }
    }
    #endif
}
