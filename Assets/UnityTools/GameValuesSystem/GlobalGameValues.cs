using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;
namespace UnityTools {

    [CreateAssetMenu(menuName="Unity Tools/Global Game Values Object", fileName="GlobalValuesObject")]
    public class GlobalGameValues : GameSettingsObject
    {
        static Dictionary<string, GameValue> globalValuesDict;

        public static void AddGlobalValue (GameValue gameValue) {
            InitializeDictionaryIfNull();
            GameValue gv = new GameValue(gameValue);
            globalValuesDict[gv.name] = gv;
        }

        static void InitializeDictionaryIfNull () {
            if (globalValuesDict == null) {
                globalValuesDict = new Dictionary<string, GameValue>();
                List<GlobalGameValues> allValuesObjs = GameSettings.GetSettingsOfType<GlobalGameValues>();

                for (int i = 0; i < allValuesObjs.Count; i++) {
                    for (int x = 0; x < allValuesObjs[i].gameValues.Length; x++) {
                        GameValue gv = new GameValue(allValuesObjs[i].gameValues[x]);
                        globalValuesDict[gv.name] = gv;
                    }
                }
            }
        }

        public static bool ValueExists (string name, out GameValue value) {
            InitializeDictionaryIfNull();
            return globalValuesDict.TryGetValue(name, out value);
        }
        public static bool ValueExists (string name) {
            return ValueExists(name, out _);
        }

        public static GameValue GetGameValue (string name) {
            InitializeDictionaryIfNull();
            
            GameValue value;
            if (ValueExists(name, out value))
                return value;
            
            Debug.LogWarning("Global Value: '" + name + "' does not exist");
            return null;
        }
        public static float GetGlobalValueComponent (string name, GameValue.GameValueComponent component) {
            GameValue gv = GetGameValue(name);
            if (gv == null)
                return 0;
            
            return gv.GetValueComponent(component);
        }
        public static float GetGlobalValue (string name) {
            return GetGlobalValueComponent(name, GameValue.GameValueComponent.Value);
        }
        
        [NeatArray] public GameValueArray gameValues;


        #if UNITY_EDITOR
        public static void DrawGlobalValueSelector (Rect pos, SerializedProperty prop) {
            if (globalGameValueSelector == null) {
                globalGameValueSelector = new GlobalGameValueSelector();
            }
            globalGameValueSelector.Draw (pos, prop, GUITools.noContent);
        }

        static GlobalGameValueSelector globalGameValueSelector;
        #endif
    }

    #if UNITY_EDITOR
        
    public class GlobalGameValueSelector {
        string[] elements;
        
        GUIContent _resetButtonContent;
        GUIContent resetButtonContent {
            get {
                if (_resetButtonContent == null) _resetButtonContent = BuiltInIcons.GetIcon("ViewToolZoom", "Update Global Values References");
                return _resetButtonContent;
            }
        }
        public GlobalGameValueSelector () {
            UpdateAssetReferences( );
        }

        public void UpdateAssetReferences () {
            elements = BuildElements();
        }

        string[] BuildElements() {
            List<string> r = new List<string>();
            List<GlobalGameValues> allValuesObjs = GameSettings.GetSettingsOfType<GlobalGameValues>();
            for (int i = 0; i < allValuesObjs.Count; i++) {
                for (int x = 0; x < allValuesObjs[i].gameValues.Length; x++) {
                    r.Add(allValuesObjs[i].gameValues[x].name);
                }
            }
            return r.ToArray();
        }

        // TODO: include headers and attributes
        public void Draw (SerializedProperty property, GUIContent gui) {
            property.stringValue = Draw( property.stringValue, gui );
        }

        public void Draw (Rect position, SerializedProperty property, GUIContent gui) {
            property.stringValue = Draw(position, property.stringValue, gui);
        }
        
        int GetActiveIndex (string current) {
            for (int i =0 ; i < elements.Length; i++) {
                if (elements[i] == current) return i;
            }
            return -1;
        }

        public string Draw (Rect pos, string current, GUIContent gui) {
            
            float buttonStart = pos.width - GUITools.iconButtonWidth;

            //draw field
            int selected = EditorGUI.Popup (new Rect(pos.x, pos.y, buttonStart, pos.height), gui.text, GetActiveIndex(current), elements);
            
            // draw reset button
            if (GUITools.IconButton(pos.x + buttonStart, pos.y, resetButtonContent, GUITools.white)){
            
                UpdateAssetReferences();
            }
            
            return selected < 0 ? null : elements[selected];
        }
        
        public string Draw (string current, GUIContent gui) {
            EditorGUILayout.BeginHorizontal();
            
            //draw field
            int selected = EditorGUILayout.Popup (gui, GetActiveIndex(current), elements);

            // draw reset button
            if (GUITools.IconButton(resetButtonContent, GUITools.white)){    
                UpdateAssetReferences();
            }
            
            EditorGUILayout.EndHorizontal();
            
            return selected < 0 ? null : elements[selected];
        }

    }
    #endif
}
