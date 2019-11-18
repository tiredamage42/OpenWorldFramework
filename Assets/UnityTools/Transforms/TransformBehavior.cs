using UnityEngine;
using System;

using UnityEditor;
using UnityTools.EditorTools;
namespace UnityTools {

    /*
        easier way to define transforms in editor and prototype
        
        positions/rotations/scales
    */
    [CreateAssetMenu(menuName="Unity Tools/Transform Behavior", fileName="New Transform Behavior")]
    public class TransformBehavior : ScriptableObject {
        [System.Serializable] public class TransformArray : NeatArrayWrapper<Transform> {  }
        
        [System.Serializable] public class Transform {
            public string name;
            public MiniTransform transform;
        }

        public bool position = true;
        public bool rotation = true;
        public bool scale = true;
        
        [NeatArray] public TransformArray transforms;
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TransformBehavior.Transform))]
    class TransformBehaviorTransformDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            EditorGUI.BeginProperty(pos, label, prop);
            float w = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;
            EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width, EditorGUIUtility.singleLineHeight), prop.FindPropertyRelative("name"), true);
            EditorGUIUtility.labelWidth = w;
            EditorGUI.PropertyField(new Rect(pos.x, pos.y + EditorGUIUtility.singleLineHeight, pos.width, EditorGUIUtility.singleLineHeight), prop.FindPropertyRelative("transform"), true);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight + EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("transform"), true);
        }
    }

    #endif
        
}

