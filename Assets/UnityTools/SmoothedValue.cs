using UnityEngine;

using UnityEditor;
using UnityTools.EditorTools;

namespace UnityTools {

    public enum SmoothMethod { Lerp, MoveTowards, SmoothDamp };
    [System.Serializable] public class SmoothedValue 
    {
        public SmoothMethod smooth;
        public float speed = 1;
        float velocity;    

        public float Smooth (float value, float target, float deltaTime) {
            switch (smooth) {
                case SmoothMethod.Lerp:
                    value = Mathf.Lerp(value, target, deltaTime * speed);
                    break;
                case SmoothMethod.MoveTowards:
                    value = Mathf.MoveTowards(value, target, deltaTime * speed);                    
                    break;
                case SmoothMethod.SmoothDamp:
                    value = Mathf.SmoothDamp(value, target, ref velocity, speed);
                    break;
            }
            return value;
        }
    }


    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SmoothedValue))]
    class SmoothedValueDrawer : PropertyDrawer {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            float w = EditorGUIUtility.labelWidth;

            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.LabelField(position, label);

            float wL = position.width - w;
            float wLhalf = wL * .5f;

            EditorGUI.PropertyField(new Rect(position.x + w, position.y, wLhalf, position.height), property.FindPropertyRelative("smooth"), GUITools.noContent, true);
            
            GUIContent speedLabel = new GUIContent("Speed");
            float slw = 55;
            
            float lblOffset = ((w + wLhalf) - 10);
            EditorGUI.LabelField(new Rect(position.x + lblOffset, position.y, slw, position.height), speedLabel);

            EditorGUI.PropertyField(new Rect(position.x + lblOffset + (slw-10), position.y, wLhalf - (slw-10), position.height), property.FindPropertyRelative("speed"), GUITools.noContent, true);
            
            EditorGUI.EndProperty();
        }
    }

    #endif
}
