using System.Collections.Generic;
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;

// namespace CustomEditorTools {
namespace UnityTools.EditorTools {

    /*
        Draw project asset selection in a dropdown list

        press the button next to the field to refresh asset references
    */    
    public class AssetSelectionAttribute : PropertyAttribute {
        public Type type;   
        public virtual List<AssetSelectorElement> OnAssetsLoaded (List<AssetSelectorElement> originals) { return originals; }
        public AssetSelectionAttribute(Type type) { this.type = type; }
    }

    public class AssetSelectorElement {

        public Object asset;
        public string displayName;

        public AssetSelectorElement(Object asset) {
            this.asset = asset;
            this.displayName = asset != null ? asset.name : "[ None ]";
        }    
    }

    
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(AssetSelectionAttribute), true)]
    class AssetSelectionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            
            if (property.propertyType != SerializedPropertyType.ObjectReference) {
                Debug.LogWarning("Field :: " + property.displayName + " is not an object reference type, cannot draw with asset selector");
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            AssetSelectionAttribute att = attribute as AssetSelectionAttribute;
            Type type = att.type;// SerializedProperties.GetType(property);
            
            AssetSelector.Draw(type, position, property, label, att.OnAssetsLoaded);
        }
    }

    public class AssetSelector {
        static Dictionary<Type, AssetSelector> allAssetSelectors = new Dictionary<Type, AssetSelector> ();
        static AssetSelector GetAssetSelector (Type type, Func<List<AssetSelectorElement>, List<AssetSelectorElement>> onAssetsLoaded) {
                    
            AssetSelector selector;
            if (allAssetSelectors.TryGetValue(type, out selector)) {
                if (selector != null) {
                    return selector;
                }
            }
            allAssetSelectors[type] = new AssetSelector(type, onAssetsLoaded);
            return allAssetSelectors[type];
        }

        public static void Draw (Type type, SerializedProperty property, GUIContent gui, Func<List<AssetSelectorElement>, List<AssetSelectorElement>> onAssetsLoaded) {
            GetAssetSelector(type, onAssetsLoaded).Draw(property, gui, onAssetsLoaded);
        }
        public static void Draw (Type type, Rect position, SerializedProperty property, GUIContent gui, Func<List<AssetSelectorElement>, List<AssetSelectorElement>> onAssetsLoaded) {
            GetAssetSelector(type, onAssetsLoaded).Draw(position, property, gui, onAssetsLoaded);
        }
        public static Object Draw (Type type, Rect position, Object current, GUIContent gui, Func<List<AssetSelectorElement>, List<AssetSelectorElement>> onAssetsLoaded) {
            return GetAssetSelector(type, onAssetsLoaded).Draw(position, current, gui, onAssetsLoaded);
        }
        public static Object Draw (Type type, Object current, GUIContent gui, Func<List<AssetSelectorElement>, List<AssetSelectorElement>> onAssetsLoaded) {
            return GetAssetSelector(type, onAssetsLoaded).Draw(current, gui, onAssetsLoaded);
        }

        Type type;
        string[] allNames;
        List<AssetSelectorElement> elements;
        
        GUIContent _resetButtonContent;
        GUIContent resetButtonContent {
            get {
                if (_resetButtonContent == null) _resetButtonContent = BuiltInIcons.GetIcon("ViewToolZoom", "Update Asset References");// new GUIContent("", "Update Asset References");
                return _resetButtonContent;
            }
        }
        public AssetSelector (Type type, Func<List<AssetSelectorElement>, List<AssetSelectorElement>> onAssetsLoaded) {
            this.type = type;
            UpdateAssetReferences( false, onAssetsLoaded );
        }

        public void UpdateAssetReferences (bool logToConsole, Func<List<AssetSelectorElement>, List<AssetSelectorElement>> onAssetsLoaded) {
            elements = BuildElements(logToConsole);
            if (onAssetsLoaded != null) {
                elements = onAssetsLoaded(elements);
            }
            elements.Insert(0, new AssetSelectorElement(null));

            allNames = new string[elements.Count];
            for (int i = 0; i < elements.Count; i++) 
                allNames[i] = elements[i].displayName;
        }

        List<AssetSelectorElement> BuildElements(bool logToConsole) {
            List<Object> assets = AssetTools.FindAssetsByType(type, logToConsole);

            List<AssetSelectorElement> r = new List<AssetSelectorElement>();
            for (int i = 0; i < assets.Count; i++) {
                r.Add(new AssetSelectorElement(assets[i]));
            }
            return r;
        }

        // TODO: include headers and attributes
        public void Draw (SerializedProperty property, GUIContent gui, Func<List<AssetSelectorElement>, List<AssetSelectorElement>> onAssetsLoaded) {
            property.objectReferenceValue = Draw( property.objectReferenceValue, gui, onAssetsLoaded );
        }

        public void Draw (Rect position, SerializedProperty property, GUIContent gui, Func<List<AssetSelectorElement>, List<AssetSelectorElement>> onAssetsLoaded) {
            property.objectReferenceValue = Draw(position, property.objectReferenceValue, gui, onAssetsLoaded);
        }
        
        int GetActiveIndex (Object current) {
            for (int i =0 ; i < elements.Count; i++) {
                if (elements[i].asset == current) return i;
            }
            return -1;
        }

        public Object Draw (Rect pos, Object current, GUIContent gui, Func<List<AssetSelectorElement>, List<AssetSelectorElement>> onAssetsLoaded) {
            
            float buttonStart = pos.width - GUITools.iconButtonWidth;

            //draw field
            int selected = EditorGUI.Popup (new Rect(pos.x, pos.y, buttonStart, pos.height), gui.text, GetActiveIndex(current), allNames);
            
            // draw reset button
            if (GUITools.IconButton(pos.x + buttonStart, pos.y, resetButtonContent, GUITools.white)){
            
                UpdateAssetReferences(true, onAssetsLoaded);
            }
            
            return selected < 0 ? null : elements[selected].asset;
        }
        
        public Object Draw (Object current, GUIContent gui, Func<List<AssetSelectorElement>, List<AssetSelectorElement>> onAssetsLoaded) {
            EditorGUILayout.BeginHorizontal();
            
            //draw field
            int selected = EditorGUILayout.Popup (gui, GetActiveIndex(current), allNames);

            // draw reset button
            if (GUITools.IconButton(resetButtonContent, GUITools.white)){    
                UpdateAssetReferences(true, onAssetsLoaded);
            }
            
            EditorGUILayout.EndHorizontal();
            
            return selected < 0 ? null : elements[selected].asset;
        }
    }
#endif

}
