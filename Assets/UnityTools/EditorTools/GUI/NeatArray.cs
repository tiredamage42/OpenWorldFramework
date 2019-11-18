
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityTools.EditorTools {
    
    #if UNITY_EDITOR
    /*
        more intuitive array/list handling for gui 
    */

    [CustomPropertyDrawer(typeof(NeatArrayAttribute))] 
    public class NeatArrayAttributeDrawer : PropertyDrawer
    {

        static GUIContent _isShownContent;
        protected static GUIContent isShownContent { get { 
            if (_isShownContent == null) _isShownContent = BuiltInIcons.GetIcon("animationvisibilitytoggleon", "Hide"); 
            return _isShownContent;
        } }
        static GUIContent _hiddenContent;
        protected static GUIContent hiddenContent { get { 
            if (_hiddenContent == null) _hiddenContent = BuiltInIcons.GetIcon("animationvisibilitytoggleoff", "Show"); 
            return _hiddenContent;
        } }
        static GUIContent _addContent;
        protected static GUIContent addContent { get { 
            if (_addContent == null) _addContent = BuiltInIcons.GetIcon("Toolbar Plus", "Add New Element"); 
            return _addContent;
        } }
        static GUIContent _deleteContent;
        protected static GUIContent deleteContent { get { 
            if (_deleteContent == null) _deleteContent = BuiltInIcons.GetIcon("Toolbar Minus", "Delete Element"); 
            return _deleteContent;
        } }

        const string displayedName = "displayed";
        protected const string listName = "list";

        void MakeSureSizeIsOK (SerializedProperty prop, int enforceSize) {
            
            if (enforceSize < 0)
                return;

            if (prop.arraySize != enforceSize) {
                if (prop.arraySize > enforceSize) {
                    prop.ClearArray();
                }
                
                int c = enforceSize - prop.arraySize;
                for (int i = 0; i < c; i++) {
                    prop.InsertArrayElementAtIndex(prop.arraySize);
                }
            }
        }

        protected bool DrawDisplayedToggle (Rect pos, SerializedProperty prop) {
            SerializedProperty displayed = prop.FindPropertyRelative(displayedName);
            if (GUITools.IconButton(pos.x, pos.y, displayed.boolValue ? isShownContent : hiddenContent, GUITools.white)){
                displayed.boolValue = !displayed.boolValue;
            }
            return displayed.boolValue;
        }

        protected void DrawArrayTitle (Rect pos, SerializedProperty prop, GUIContent label, float xOffset) {
            
            label.text += " [" + prop.arraySize + "]";
            pos.x = xOffset;
            GUITools.Label(pos, label, GUITools.black, GUITools.boldLabel);
        }

        protected virtual void OnAddNewElement (SerializedProperty newElement) {

        }
        protected virtual void OnDeleteElement (SerializedProperty deleteElement) {
        
        }


        protected void DrawAddElement (Rect pos, SerializedProperty prop, float indent1, bool displayedValue) {
            GUI.enabled = displayedValue;
            if (GUITools.IconButton(indent1, pos.y, addContent, displayedValue ? GUITools.green : GUITools.white)) {
                prop.InsertArrayElementAtIndex(prop.arraySize);
                SerializedProperty p = prop.GetArrayElementAtIndex(prop.arraySize - 1);
                if (p.propertyType == SerializedPropertyType.ObjectReference) {
                    p.objectReferenceValue = null;
                }

                OnAddNewElement (p);
            }
            GUI.enabled = true;
        }

        protected void StartArrayDraw (Rect pos, ref SerializedProperty prop, ref GUIContent label, out float indent1, out float indent2, out float indent2Width, out bool displayedValue) {
            indent1 = pos.x + GUITools.iconButtonWidth;
            indent2 = indent1 + GUITools.iconButtonWidth;
            indent2Width = pos.width - GUITools.iconButtonWidth * 2;
            
            EditorGUI.BeginProperty(pos, label, prop);

            displayedValue = DrawDisplayedToggle ( pos, prop );

            // the property we want to draw is the list child
            prop = prop.FindPropertyRelative(listName);

            DrawBox ( pos, prop, ref label, indent1, displayedValue );
        }

        protected void DrawBox (Rect pos, SerializedProperty prop, ref GUIContent label, float indent1, bool displayedValue) {
            string lbl = label.text;
            string tooltip = label.tooltip;
            
            float h = CalculateHeight(prop, displayedValue);
            label.text = lbl;
            label.tooltip = tooltip;
            
            pos.x = indent1;
            pos.width -= GUITools.iconButtonWidth;
            pos.height = h + GUITools.singleLineHeight * .1f;

            GUITools.Box ( pos, GUITools.shade );
        }
            

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            NeatArrayAttribute att = attribute as NeatArrayAttribute;

            float indent1, indent2, indent2Width;
            bool displayedValue;
            StartArrayDraw ( pos, ref prop, ref label, out indent1, out indent2, out indent2Width, out displayedValue );

            if (att != null) {
                MakeSureSizeIsOK(prop, att.enforceSize);
            }
            
            if (att == null || att.enforceSize < 0) {

                DrawAddElement ( pos, prop, indent1, displayedValue );
            }

            float xOffset = (att == null || att.enforceSize < 0 ? indent2 : indent1) + GUITools.toolbarDividerSize;

            DrawArrayTitle ( pos, prop, label, xOffset );
            
            if (displayedValue) {
                int indexToDelete = -1;

                pos.x = xOffset;
                pos.y += GUITools.singleLineHeight;
                pos.width = indent2Width - GUITools.toolbarDividerSize * 2;
                pos.height = EditorGUIUtility.singleLineHeight;

                for (int i = 0; i < prop.arraySize; i++) {
                    if (att == null || att.enforceSize < 0) {
                        if (GUITools.IconButton(indent1, pos.y, deleteContent, GUITools.red))
                            indexToDelete = i;
                    }
                    
                    SerializedProperty p = prop.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(pos, p, true);

                    pos.y += EditorGUI.GetPropertyHeight(p, true);
                }
                
                if (indexToDelete != -1) {

                    SerializedProperty p = prop.GetArrayElementAtIndex(indexToDelete);
                    
                    OnDeleteElement(p);

                    if (p.propertyType == SerializedPropertyType.ObjectReference) {
                        prop.DeleteArrayElementAtIndex(indexToDelete);
                    }
                    prop.DeleteArrayElementAtIndex(indexToDelete);
                }
            }

            EditorGUI.EndProperty();
        }

        protected float CalculateHeight (SerializedProperty prop, bool displayed) {
            if (!displayed) return GUITools.singleLineHeight;
            float h = GUITools.singleLineHeight;
            int arraySize = prop.arraySize;
            for (int i = 0; i < arraySize; i++) h += EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(i), true);
            return h;
        }
    
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return CalculateHeight(prop.FindPropertyRelative(listName), prop.FindPropertyRelative(displayedName).boolValue) + GUITools.singleLineHeight * .25f;
        }
    }
    #endif


    /*
        when we need custom classes to wrap elements with attributes
    */
    [System.Serializable] public class NeatArrayElement { };

    #if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(NeatArrayElement), true)] 
    public class NeatArrayElementDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative(NeatArray.elementName), GUITools.noContent, true);
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight;
        }
    }
    #endif

    public class NeatArray {
        public const string elementName = "element";
    }
    
    //the actual attribute
    public class NeatArrayAttribute : PropertyAttribute { 
        public int enforceSize;
        public NeatArrayAttribute () {
            enforceSize = -1;
        }
        public NeatArrayAttribute(int enforceSize) {
            this.enforceSize = enforceSize;
        }
    }
    
    [Serializable] public class NeatBoolList : NeatListWrapper<bool> { public NeatBoolList() : base() { } public NeatBoolList(List<bool> list) : base(list) { } }
    [Serializable] public class NeatBoolArray : NeatArrayWrapper<bool> { public NeatBoolArray() : base() { } public NeatBoolArray(bool[] list) : base(list) { } }
    
    [Serializable] public class NeatStringList : NeatListWrapper<string> { public NeatStringList() : base() { } public NeatStringList(List<string> list) : base(list) { } }
    [Serializable] public class NeatStringArray : NeatArrayWrapper<string> { public NeatStringArray() : base() { } public NeatStringArray(string[] list) : base(list) { } }
    
    [Serializable] public class NeatIntList : NeatListWrapper<int> { public NeatIntList() : base() { } public NeatIntList(List<int> list) : base(list) { } }
    [Serializable] public class NeatIntArray : NeatArrayWrapper<int> { public NeatIntArray() : base() { } public NeatIntArray(int[] list) : base(list) { } }
    
    [Serializable] public class NeatFloatList : NeatListWrapper<float> { public NeatFloatList() : base() { } public NeatFloatList(List<float> list) : base(list) { } }
    [Serializable] public class NeatFloatArray : NeatArrayWrapper<float> { public NeatFloatArray() : base() { } public NeatFloatArray(float[] list) : base(list) { } }

    [Serializable] public class NeatAudioClipList : NeatListWrapper<AudioClip> {}
    [Serializable] public class NeatAudioClipArray : NeatArrayWrapper<AudioClip> {}

    [Serializable] public class NeatAnimationClipList : NeatListWrapper<AnimationClip> {}
    [Serializable] public class NeatAnimationClipArray : NeatArrayWrapper<AnimationClip> {}

    [Serializable] public class NeatAudioSourceList : NeatListWrapper<AudioSource> {}
    [Serializable] public class NeatAudioSourceArray : NeatArrayWrapper<AudioSource> {}

    [Serializable] public class NeatTransformList : NeatListWrapper<Transform> {}
    [Serializable] public class NeatTransformArray : NeatArrayWrapper<Transform> {}

    [Serializable] public class NeatGameObjectList : NeatListWrapper<GameObject> {}
    [Serializable] public class NeatGameObjectArray : NeatArrayWrapper<GameObject> {}

    [Serializable] public class NeatKeyCodeList : NeatListWrapper<KeyCode> { public NeatKeyCodeList() : base() { } public NeatKeyCodeList(List<KeyCode> list) : base(list) { } }
    [Serializable] public class NeatKeyCodeArray : NeatArrayWrapper<KeyCode> { public NeatKeyCodeArray() : base() { } public NeatKeyCodeArray(KeyCode[] list) : base(list) { } }

    public class NeatArrayWrapper<T> {

        public void CopyFrom (T[] list) {
            System.Array.Resize(ref this.list, list.Length);
            for (int i = 0; i < list.Length; i++)
                this.list[i] = list[i];
        }
        
        public NeatArrayWrapper (T[] list) { this.list = list; }
        public NeatArrayWrapper () { }

        public T GetRandom(T defaultValue) { return list.GetRandom<T>(defaultValue); }
        
        public T[] list;
        public int Length { get { return list.Length; } }
        public T this[int index] { get { return list[index]; } }
        public bool displayed;
        public static implicit operator T[](NeatArrayWrapper<T> c) { return c.list; }
    }
    public class NeatListWrapper<T> {

        public void CopyFrom (List<T> list) {
            this.list.Clear();
            this.list.AddRange(list);
        }

        public NeatListWrapper (List<T> list) { this.list = list; }
        public NeatListWrapper () { }

        public T GetRandom(T defaultValue) { return list.GetRandom<T>(defaultValue); }
            
        public List<T> list;
        public bool displayed;
        public int Count { get { return list.Count; } }
        public T this[int index] { get { return list[index]; } }
        public static implicit operator List<T>(NeatListWrapper<T> c) { return c.list; }
        
        
    }
}


