using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEditor;
using UnityTools.EditorTools;

namespace UnityTools.Internal {
    [System.Serializable] public class MessageParameters : NeatArrayWrapper<MessageParameter> { }

    public abstract class MessageParameter : ScriptableObject {
        public abstract object GetParamObject ();

        #if UNITY_EDITOR
        public abstract void DrawGUI (Rect pos);
        public abstract void DrawGUIFlat (Rect pos);
        public virtual float GetPropertyHeight () { 
            return GUITools.singleLineHeight; 
        }
        #endif
    }




    #if UNITY_EDITOR

    // Neat arrays with elements that use parameter objects should draw with this
    public class NeatArrayWithMessageParametersInElements : NeatArrayAttributeDrawer {

        SerializedProperty GetParamsListProperty (SerializedProperty element) {
            return element.FindPropertyRelative("parameters").FindPropertyRelative("list");
        }

        /*
            CLEAR THE PARAMETERS WHEN CREATING A NEW ELEMENT (IN ARRAY)
            SO DIFFERENT ELEMENTS DONT REFERENCE SAME PARAMETER OBJECTS
        */
        protected override void OnAddNewElement (SerializedProperty newElement) {
            GetParamsListProperty(newElement).ClearArray();
        }
        /*
            DELETE PARAMETER OBJECTS ASSOCIATED WITH THE ARRAY ELEMENT BEING DELETED
        */
        protected override void OnDeleteElement (SerializedProperty deleteElement) {

            SerializedProperty paramsList = GetParamsListProperty(deleteElement);

            Object baseObject = deleteElement.serializedObject.targetObject;
            bool isAsset = AssetDatabase.Contains(baseObject);
                
            for (int i = paramsList.arraySize - 1; i >= 0; i--) {
                SerializedProperty deletedParameter = paramsList.GetArrayElementAtIndex(i);
                Object deletedParameterObj = deletedParameter.objectReferenceValue;
                    
                if (deletedParameterObj != null) paramsList.DeleteArrayElementAtIndex(i);
                paramsList.DeleteArrayElementAtIndex(i);
                    
                    
                Object.DestroyImmediate(deletedParameterObj, true);
                    
                if (isAsset) {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(baseObject));
                    AssetDatabase.SaveAssets(); 
                }
            }
        }
    }
    

    /*
        DRAW MESSAGE FUNCTION PARAMETERS LIST
        (NEEDS SPECIAL CONSIDERATION SINCE EACH PARAMETER IS AN SCRIPTABLE OBJECT REFERENCE, IN ORDER TO
            SUPPORT POLYMORPHISM IN THE SAME ARRAY...
        )
    */

    [CustomPropertyDrawer(typeof(MessageParameters))] 
    public class MessageParametersDrawer : NeatArrayAttributeDrawer
    {
        static GUIContent _chooseTypeContent;
        static GUIContent chooseTypeContent {
            get {
                if (_chooseTypeContent == null) _chooseTypeContent = BuiltInIcons.GetIcon("ClothInspector.SelectTool", "Choose Parameter Type");
                return _chooseTypeContent;
            }
        }
        static float paramsLabelW;
        
        static GUIContent _paramsLabel;
        static GUIContent paramsLabel {
            get {
                if (_paramsLabel == null) {
                    _paramsLabel = new GUIContent("Params: ");
                    paramsLabelW = GUITools.boldLabel.CalcSize(_paramsLabel).x;
                }
                return _paramsLabel;
            }
        }
        
        public static void DrawFlat (Rect pos, SerializedProperty prop) {
            // the property we want to draw is the list child
            prop = prop.FindPropertyRelative(listName);
            int arraySize = prop.arraySize;
            float propW = (pos.width - paramsLabelW) * (1f/arraySize);

            pos.width = paramsLabelW;
            GUITools.Label(pos, paramsLabel, GUITools.black, GUITools.boldLabel);
            pos.x += paramsLabelW;

            pos.width = propW;

            for (int i = 0; i < arraySize; i++) {
                Object o = prop.GetArrayElementAtIndex(i).objectReferenceValue;
                if (o != null) {
                    EditorGUI.BeginChangeCheck();
                    ((MessageParameter)o).DrawGUIFlat(pos);
                    if (EditorGUI.EndChangeCheck()) {
                        EditorUtility.SetDirty(o);
                        EditorUtility.SetDirty(prop.serializedObject.targetObject);
                    }                        
                }
                pos.x += propW;
            }
        }

        
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {

            float indent1, indent2, indent2Width;
            bool displayedValue;
            StartArrayDraw ( pos, ref prop, ref label, out indent1, out indent2, out indent2Width, out displayedValue );

            DrawAddElement ( pos, prop, indent1, displayedValue );
            
            float xOffset = indent2 + GUITools.toolbarDividerSize;
            DrawArrayTitle ( pos, prop, label, xOffset );
            
            if (displayedValue) {
                Object baseObject = prop.serializedObject.targetObject;
                bool isAsset = AssetDatabase.Contains(baseObject);
                
                float o = GUITools.iconButtonWidth + GUITools.toolbarDividerSize;

                float dividerX = xOffset + GUITools.iconButtonWidth;

                pos.x = xOffset + o;
                pos.y = pos.y + GUITools.singleLineHeight;
                pos.width = (indent2Width - o) - GUITools.toolbarDividerSize * 2;

                int indexToDelete = -1;

                for (int i = 0; i < prop.arraySize; i++) {

                    if (GUITools.IconButton(indent1, pos.y, deleteContent, GUITools.red))
                        indexToDelete = i;
                    
                    SerializedProperty p = prop.GetArrayElementAtIndex(i);
                    Object obj = p.objectReferenceValue;

                    if (GUITools.IconButton(xOffset, pos.y, chooseTypeContent, GUITools.white)) {
                        System.Type[] paramTypes = typeof(MessageParameter).FindDerivedTypes(false);

                        GenericMenu menu = new GenericMenu();

                        Vector2 mousePos = Event.current.mousePosition;
                        menu.DropDown(new Rect(mousePos.x, mousePos.y, 0, 0));

                        for (int x = 0; x < paramTypes.Length; x++) {
                            bool isType = obj != null && obj.GetType() == paramTypes[x];
                            menu.AddItem(
                                new GUIContent(paramTypes[x].Name.Split('_')[0]), 
                                isType,
                                (newType) => {
                                    if (!isType) {
                                        System.Type newT = paramTypes[(int)newType];

                                        Object.DestroyImmediate(obj, true);
                                        var newObj = ScriptableObject.CreateInstance(newT);
                                        newObj.name = newT.Name;

                                        if (isAsset) {
                                            AssetDatabase.AddObjectToAsset(newObj, baseObject);
                                            AssetDatabase.SaveAssets(); 
                                        }

                                        p.objectReferenceValue = newObj;
                                        p.serializedObject.ApplyModifiedProperties();
                                        EditorUtility.SetDirty(baseObject);
                                    }
                                },
                                x
                            );
                        }
                        menu.ShowAsContext();
                    }


                    GUITools.DrawToolbarDivider(dividerX, pos.y);

                    obj = p.objectReferenceValue;
        
                    if (obj != null) {
                        
                        EditorGUI.BeginChangeCheck();
                        ((MessageParameter)obj).DrawGUI(pos);
                        if (EditorGUI.EndChangeCheck()) {
                            EditorUtility.SetDirty(obj);
                            EditorUtility.SetDirty(baseObject);
                        }
                    }
                    pos.y += EditorGUI.GetPropertyHeight(p, true);
                }

                
                if (indexToDelete != -1) {
                    SerializedProperty deleted = prop.GetArrayElementAtIndex(indexToDelete);
                    
                    Object deletedObj = deleted.objectReferenceValue;
                    
                    if (deletedObj != null) prop.DeleteArrayElementAtIndex(indexToDelete);
                    
                    prop.DeleteArrayElementAtIndex(indexToDelete);
                    
                    Object.DestroyImmediate(deletedObj, true);
                    
                    if (isAsset) {
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(baseObject));
                        AssetDatabase.SaveAssets(); 
                    }                
                }
            }

            EditorGUI.EndProperty();
        }
    }
    /*
        JUST TO SPECIFY THE HEIGHT WHEN CHECKING CONDITIONS PARAMETER AS A SERIALIZED PROPERTY
        (FOR ConditionsParametersDrawer BASE CLASS CHECK HEIGHT)
    */
    [CustomPropertyDrawer(typeof(MessageParameter))] 
    class MessageParameterDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            Object o = prop.objectReferenceValue;
            if (o == null) return GUITools.singleLineHeight;
            return ((MessageParameter)o).GetPropertyHeight();
        }
    }
    #endif
}
