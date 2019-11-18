// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityTools.EditorTools;
namespace UnityTools {

    /*
        for use in editor
    */
    [System.Serializable] public class MiniTransform {
        public Vector3 position, rotation, scale;


        void Initialize (Vector3 position, Vector3 rotation, Vector3 scale) {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public MiniTransform (Vector3 position, Vector3 rotation, Vector3 scale) {
            Initialize(position, rotation, scale);
        }
        public MiniTransform (Vector3 position, Quaternion rotation, Vector3 scale) {
            Initialize(position, rotation.eulerAngles, scale);
        }
        public MiniTransform (Vector3 position, Quaternion rotation) {
            Initialize(position, rotation.eulerAngles, Vector3.one);
        }
        public MiniTransform (Vector3 position, Vector3 rotation) {
            Initialize(position, rotation, Vector3.one);
        }
        public MiniTransform (Vector3 position) {
            Initialize(position, Vector3.zero, Vector3.one);
        }
    }

    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MiniTransform))]
    class MiniTransformDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            EditorGUI.BeginProperty(pos, label, prop);
            
            GUITools.Box(new Rect(pos.x, pos.y, pos.width, EditorGUIUtility.singleLineHeight * 3 + GUITools.singleLineHeight), GUITools.shade);
            pos.height = EditorGUIUtility.singleLineHeight;
            
            EditorGUI.LabelField(pos, label, GUITools.boldLabel);
            pos.y += EditorGUIUtility.singleLineHeight;
            
            float w = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 75;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("position"), true);
            pos.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("rotation"), true);
            pos.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("scale"), true);
            EditorGUIUtility.labelWidth = w;
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight * 4;
        }
    }

    #endif





    public static class TransformUtils 
    {

        static Dictionary<int, CharacterController> transform2CC = new Dictionary<int, CharacterController>();

        public static void WarpTo (this Transform transform, Vector3 position, Quaternion rotation) {
            int id = transform.GetInstanceID();

            if (!transform2CC.ContainsKey(id)) {
                transform2CC[id] = transform.GetComponent<CharacterController>();
            }

            CharacterController cc = transform2CC[id];
            bool wasEnabled = cc != null && cc.enabled;
            if (wasEnabled) cc.enabled = false;
            transform.SetTransform(position, rotation);
            if (wasEnabled) cc.enabled = true;
        }

        public static void SetParent (this Transform transform, Transform parent, Vector3 localPosition) {
            if (transform.parent != parent) transform.SetParent(parent);
            transform.localPosition = localPosition;
        }
        public static void SetParent (this Transform transform, Transform parent, Vector3 localPosition, Quaternion localRotation) {
            transform.SetParent(parent, localPosition);
            transform.localRotation = localRotation;
        }
        public static void SetParent (this Transform transform, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale) {
            transform.SetParent(parent, localPosition, localRotation);
            transform.localScale = localScale;
        }

        public static void SetTransform (this Transform transform, Vector3 position, Quaternion rotation) {
            transform.position = position;
            transform.rotation = rotation;
        }
        public static void SetLocalTransform (this Transform transform, Vector3 position, Quaternion rotation) {
            transform.localPosition = position;
            transform.localRotation = rotation;
        }



        public static void SetTransform (this Transform transform, MiniTransform settings, TransformBehavior behavior) {
            if (settings == null) return;
            if (behavior == null || behavior.position) transform.position = settings.position;
            if (behavior == null || behavior.rotation) transform.rotation = Quaternion.Euler(settings.rotation);
            if (behavior == null || behavior.scale) transform.localScale = settings.scale;
        }
        public static void SetTransform (this Transform transform, Transform parent, MiniTransform settings, TransformBehavior behavior) {
            if (settings == null) return;
            if (transform.parent != parent) transform.SetParent(parent);
            if (behavior == null || behavior.position) transform.localPosition = settings.position;
            if (behavior == null || behavior.rotation) transform.localRotation = Quaternion.Euler(settings.rotation);
            if (behavior == null || behavior.scale) transform.localScale = settings.scale;
        }

        public static void SetTransform (this Transform transform, TransformBehavior behavior, int index) {
            transform.SetTransform(GetTransform(behavior, index), behavior);
        }
        public static void SetTransform (this Transform transform, Transform parent, TransformBehavior behavior, int index) {
            transform.SetTransform(parent, GetTransform(behavior, index), behavior);
        }
        public static void SetTransform (this Transform transform, TransformBehavior behavior, string name) {
            transform.SetTransform(GetTransform(behavior, name), behavior);
        }
        public static void SetTransform (this Transform transform, Transform parent, TransformBehavior behavior, string name) {
            transform.SetTransform(parent, GetTransform(behavior, name), behavior);
        }
        
        static Dictionary<int, Dictionary<string, MiniTransform>> transformDictionaries = new Dictionary<int, Dictionary<string, MiniTransform>>();
        static MiniTransform GetTransform (TransformBehavior behavior, string name) {
            Dictionary<string, MiniTransform> dictionary;
            if (!transformDictionaries.TryGetValue(behavior.GetInstanceID(), out dictionary)) {

                dictionary = new Dictionary<string, MiniTransform>();

                for (int i = 0; i < behavior.transforms.Length; i++) 
                    dictionary.Add(behavior.transforms[i].name, behavior.transforms[i].transform);
                
                transformDictionaries[behavior.GetInstanceID()] = dictionary;
            }
            
            MiniTransform transform;
            if ( dictionary.TryGetValue(name, out transform) ) 
                return transform;
            
            Debug.LogWarning("Transform name " + name + " not found on Transform Behavior: " + behavior.name);
            return null;
        }
        static MiniTransform GetTransform (TransformBehavior behavior, int index) {
            if (index < 0 || index >= behavior.transforms.Length) {
                Debug.LogWarning("Index " + index + " is out of range on Transform Behavior: " + behavior.name);
                return null;
            }
            return behavior.transforms[index].transform;
            
        }
    }
}
