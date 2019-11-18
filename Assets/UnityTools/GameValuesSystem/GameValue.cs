using System.Collections.Generic;
using UnityEngine;

using System;

using UnityTools.EditorTools;

using UnityEditor;

namespace UnityTools {
    /*
        a float value wrapper,

        we can cap it dynamically, and add modifiers to it
    */

    [System.Serializable] public class GameValueArray : NeatArrayWrapper<GameValue> { }
    [System.Serializable] public class GameValueList : NeatListWrapper<GameValue> { }
    
    [System.Serializable] public class GameValue
    {


        static GameValue GetGameValue (Dictionary<string, GameValue> gameValues, string name) {
            if (gameValues.ContainsKey(name)) return gameValues[name];
            return null;
        }


        public static void AddModifiers (Dictionary<string, GameValue> gameValues, GameValueModifier[] mods, int count, string description, bool assertPermanent, GameObject subject, GameObject target) {
            for (int i =0 ; i < mods.Length; i++) {
                if (assertPermanent && !mods[i].isPermanent) continue;
                if (Conditions.ConditionsMet (mods[i].conditions, subject, target)) {
                    GameValue gameValue = GetGameValue(gameValues, mods[i].gameValueName);
                    if (gameValue != null) {
                        gameValue.AddModifier(mods[i], count, (description + i.ToString()).GetPersistentHashCode(), description);
                    }
                }
            }
        }
        public static void RemoveModifiers (Dictionary<string, GameValue> gameValues, GameValueModifier[] mods, int count, string description) {
            for (int i =0 ; i < mods.Length; i++) {
                GameValue gameValue = GetGameValue(gameValues, mods[i].gameValueName);
                if (gameValue != null) {
                    gameValue.RemoveModifier(mods[i], count, (description + i.ToString()).GetPersistentHashCode());
                }
            }
        }



        public enum GameValueComponent { BaseValue, BaseMinValue, BaseMaxValue, Value, MinValue, MaxValue };
        public string name;
        [TextArea] public string description;
        public float baseValue;


        public bool randomInitialization;
        public float initMin, initMax;

        public bool valueCapped;
        public float capMin, capMax = 500;

        public bool showAdvanced;

        
        public GameValue(string name, float baseValue, Vector2 baseMinMax, string description){
            this.name = name;
            this.baseValue = baseValue;

            valueCapped = true;
            capMin = baseMinMax.x;
            capMax = baseMinMax.y;
            initMax = initMin = baseValue;
            randomInitialization = false;
            
            this.description = description;
        }
        public GameValue(string name, float baseValue, string description){
            this.name = name;
            this.baseValue = baseValue;

            valueCapped = false;

            initMax = initMin = baseValue;
            randomInitialization = false;
            
            this.description = description;
        }

        public GameValue (GameValue template) {
            this.name = template.name;
            
            this.randomInitialization = template.randomInitialization;

            if (randomInitialization) {
                this.baseValue = UnityEngine.Random.Range(template.initMin, template.initMax);
            }
            else {
                this.baseValue = template.initMin;
            }
            
            valueCapped = template.valueCapped;
            capMin = template.capMin;
            capMax = template.capMax;
            initMin = template.initMin;
            initMax = template.initMax;
            
            this.description = template.description;
        }

        public void ReInitialize () {
            if (randomInitialization) {
                this.baseValue = UnityEngine.Random.Range(initMin, initMax);
            }
            else {
                this.baseValue = initMin;
            }
        }


        // delta, current, min, max
        event System.Action<float, float, float, float> onValueChange;
        public void AddChangeListener (Action<float, float, float, float> listener) {
            onValueChange += listener;
        }
        public void RemoveChangeListener (Action<float, float, float, float> listener) {
            onValueChange -= listener;
        }

        //not using a dictionary in order to keep thses serializable by unity
        [HideInInspector] public List<GameValueModifier> modifiers = new List<GameValueModifier>();


        public static string ModifyBehaviorString (GameValueModifier.ModifyBehavior modifyBehavior) {
            if (modifyBehavior == GameValueModifier.ModifyBehavior.Set)
                return "Set";
            else if (modifyBehavior == GameValueModifier.ModifyBehavior.Add)
                return "+";
            else if (modifyBehavior == GameValueModifier.ModifyBehavior.Multiply) 
                return "x";
            return "";
        }
    
        public string GetModifiersSummary () {
            string r = "";

            for (int i = 0; i < modifiers.Count; i++) {
                GameValueModifier m = modifiers[i];
                r += m.description + ": " +  m.modifyValueComponent + " " + ModifyBehaviorString(m.modifyBehavior) + m.modifyValue + (m.isStackable ? "(" + m.count + ")" : "") + "\n";
            }

            return r;
        }


        float GetModifiedValue (GameValueComponent checkType, float value, bool clamp, float min = float.MinValue, float max = float.MaxValue) {
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].modifyValueComponent == checkType) {
                    value = modifiers[i].Modify(value);
                }
            }
            if (clamp)
                return Mathf.Clamp(value, min, max);
            
            return value;
        }
        public float GetValue () {
            return GetModifiedValue (GameValueComponent.Value, baseValue, valueCapped, GetMinValue(false), GetMaxValue(false));
        }
        public float GetMinValue (bool showUncappedWarning=true) {
            if (valueCapped) return GetModifiedValue(GameValueComponent.MinValue, capMin, false);
            if (showUncappedWarning) Debug.LogWarning("Game Value '" + name + "' is uncapped, GetMinValue will always return 0");
            return 0;
            
        }
        public float GetMaxValue (bool showUncappedWarning=true) {
            if (valueCapped) return GetModifiedValue(GameValueComponent.MaxValue, capMax, false);
            if (showUncappedWarning) Debug.LogWarning("Game Value '" + name + "' is uncapped, GetMaxValue will always return 0");   
            return 0;
        }
            
        public float GetValueComponent (GameValueComponent checkType) {
            switch (checkType) {
                case GameValueComponent.Value:
                    return GetValue();
                case GameValueComponent.MinValue:
                    return GetMinValue();
                case GameValueComponent.MaxValue:
                    return GetMaxValue();
                case GameValueComponent.BaseValue:
                    return baseValue;
                case GameValueComponent.BaseMinValue:
                    if (!valueCapped) {
                        Debug.LogWarning("Game Value '" + name + "' is uncapped, BaseMinValue will always return 0");
                        return 0;
                    }
                    return capMin;
                case GameValueComponent.BaseMaxValue:
                    if (!valueCapped) {
                        Debug.LogWarning("Game Value '" + name + "' is uncapped, BaseMaxValue will always return 0");
                        return 0;
                    }
                    
                    return capMax;
            }
            return 0;
        }

    
        GameValueModifier GetModifier (int key) {    
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].key == key) {
                    return modifiers[i];
                }
            }
            return null;
        }

        void BroadcastValueChange (float origValue, float minVal, float maxVal) {
            if (onValueChange != null) {
                // delta, current, min, max
                float newVal = GetValue();
                onValueChange(newVal - origValue, newVal, minVal, maxVal);
            }
        }

        void ModifyPermanent (GameValueModifier modifier) {
            float origValue = GetValue();
            
            if (modifier.modifyValueComponent == GameValueComponent.BaseValue) {
                baseValue = modifier.Modify(baseValue);
            }

            if (modifier.modifyValueComponent == GameValueComponent.BaseMinValue) {
                if (!valueCapped) Debug.LogWarning("Game Value '" + name + "' is uncapped, BaseMinValue cant be modified");
                else capMin = modifier.Modify(capMin);
            }
            if (modifier.modifyValueComponent == GameValueComponent.BaseMaxValue) {
                if (!valueCapped) Debug.LogWarning("Game Value '" + name + "' is uncapped, BaseMaxValue cant be modified");
                else capMax = modifier.Modify(capMax);
            }

            float minVal = GetMinValue(false);
            float maxVal = GetMaxValue(false);
                            
            if (valueCapped) {
                //clamp the base value
                baseValue = Mathf.Clamp(baseValue, minVal, maxVal);
            }

            BroadcastValueChange ( origValue, minVal, maxVal );
        }

        // anything modifying base values is permanent, and doesnt get stored in 
        // our modifiers list
        public void AddModifier (GameValueModifier modifier, int count, int key, string description) {
            if (modifier.isPermanent) {
                ModifyPermanent(modifier);
                return;
            }
            
            float origValue = GetValue();

            if (!valueCapped) {
                if (modifier.modifyValueComponent == GameValueComponent.MinValue) {
                    Debug.LogWarning("Game Value '" + name + "' is uncapped, MinValue cant be modified");
                    return;
                }
                if (modifier.modifyValueComponent == GameValueComponent.MaxValue) {
                    Debug.LogWarning("Game Value '" + name + "' is uncapped, MaxValue cant be modified");
                    return;
                }   
            }

            GameValueModifier existingModifier = GetModifier ( key );
            if (existingModifier != null) {
                if (existingModifier.description != description) {
                    Debug.LogWarning("Game Value '" + name + "' Description mismatch for same key! 1) " + description + " :: 2) " + existingModifier.description);
                }
                existingModifier.count += count;
            }
            else {
                modifiers.Add(new GameValueModifier(modifier, count, key, description));
            }

            BroadcastValueChange ( origValue, GetMinValue(false), GetMaxValue(false) );
        }

        public void RemoveModifier (GameValueModifier modifier, int count, int key){
            if (modifier.isPermanent) return;
            
            GameValueModifier existingModifier = GetModifier ( key );
            if (existingModifier != null) {
                float origValue = GetValue();
                
                existingModifier.count -= count;
                if (existingModifier.count <= 0) {
                    modifiers.Remove(existingModifier);
                }

                BroadcastValueChange ( origValue, GetMinValue(), GetMaxValue() );
            }
        }
    }

    
#if UNITY_EDITOR
    
    [CustomPropertyDrawer(typeof(GameValue))] public class GameValueDrawer : PropertyDrawer
    {
        static GUIContent _showAdvancedGUI;
        static GUIContent showAdvancedGUI {
            get {
                if (_showAdvancedGUI == null) _showAdvancedGUI = BuiltInIcons.GetIcon("TerrainInspector.TerrainToolSettings", "Show Advanced");
                return _showAdvancedGUI;
            }
        }
        static GUIContent _cappedGUI;
        static GUIContent cappedGUI {
            get {
                if (_cappedGUI == null) _cappedGUI = BuiltInIcons.GetIcon("AssemblyLock", "Value Ranged");
                return _cappedGUI;
            }
        }

        static GUIContent _randomInitGUI;
        static GUIContent randomInitGUI {
            get {
                if (_randomInitGUI == null) _randomInitGUI = BuiltInIcons.GetIcon("Preset.Context", "Random Initialization");
                return _randomInitGUI;
            }
        }

        void PrintModifiersSummary (Rect pos, SerializedProperty prop) {
            
            SerializedProperty modifiers = prop.FindPropertyRelative("modifiers");
            
            for (int i = 0; i < modifiers.arraySize; i++) {
                SerializedProperty m = modifiers.GetArrayElementAtIndex(i);
                EditorGUI.LabelField(pos, 
                    m.FindPropertyRelative("description").stringValue + ": " +  
                    ((GameValue.GameValueComponent)m.FindPropertyRelative("modifyValueComponent").enumValueIndex) + " " + 
                    GameValue.ModifyBehaviorString((GameValueModifier.ModifyBehavior)m.FindPropertyRelative("modifyBehavior").enumValueIndex) + 
                    m.FindPropertyRelative("modifyValue").floatValue + 
                    (m.FindPropertyRelative("isStackable").boolValue ? "(" + m.FindPropertyRelative("count").intValue + ")" : "")
                );
                pos.y += EditorGUIUtility.singleLineHeight;
            }            
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);

            pos.height = EditorGUIUtility.singleLineHeight;

            float origX = pos.x;
            float origW = pos.width;


            SerializedProperty valueCappedProp = prop.FindPropertyRelative("valueCapped");
            SerializedProperty randomInitProp = prop.FindPropertyRelative("randomInitialization");
            
            
            bool randomInit = randomInitProp.boolValue;
            bool valueCapped = valueCappedProp.boolValue;

            float spaceWithoutButtons = pos.width - GUITools.iconButtonWidth * 3;

            float offset = GUITools.iconButtonWidth * 3 + (spaceWithoutButtons * .5f);
    
            GUITools.StringFieldWithDefault ( pos.x, pos.y, pos.width - offset, pos.height, prop.FindPropertyRelative("name"), "Value Name");
                        
            randomInit = GUITools.DrawToggleButton(randomInitProp, randomInitGUI, pos.x + (pos.width - GUITools.iconButtonWidth * 3), pos.y, GUITools.blue, GUITools.white);
            valueCapped = GUITools.DrawToggleButton(valueCappedProp, cappedGUI, pos.x + (pos.width - GUITools.iconButtonWidth * 2), pos.y, GUITools.blue, GUITools.white);
            
            bool showAdvanced = GUITools.DrawToggleButton(prop.FindPropertyRelative("showAdvanced"), showAdvancedGUI, pos.x + (pos.width - GUITools.iconButtonWidth), pos.y, GUITools.blue, GUITools.white);
            
            pos.x += spaceWithoutButtons * .5f;
            
            float labelWidth = 40;
            pos.width = labelWidth;
            EditorGUI.LabelField(pos, new GUIContent("Initial:"));
            pos.x += pos.width;
            
            pos.width = ((spaceWithoutButtons * .5f) - labelWidth) * (randomInit ? .5f : 1);
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("initMin"), GUITools.noContent);
            pos.x += pos.width;

            if (randomInit) {
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("initMax"), GUITools.noContent);
                pos.x += pos.width;
            }
            

            if (valueCapped) {
                pos.y += EditorGUIUtility.singleLineHeight;
                pos.x = origX;
            
                labelWidth = 75;
                float w4 = (origW - labelWidth) * .5f;

            
                pos.width = labelWidth;
                EditorGUI.LabelField(pos, new GUIContent("Min/Max:"));
                pos.x += pos.width;

                pos.width = w4;
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("capMin"), GUITools.noContent);
                pos.x += pos.width;
                
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("capMax"), GUITools.noContent);
                pos.x += pos.width;
            }

            
            if (showAdvanced) {
                GUITools.StringFieldWithDefault ( origX, pos.y, origW, EditorGUIUtility.singleLineHeight * 3, prop.FindPropertyRelative("description"), "Description...");

                if (Application.isPlaying) {
                    pos.x = origX;
                    pos.y += EditorGUIUtility.singleLineHeight * 3;
                    pos.width = origW;
                    
                    EditorGUI.PropertyField( pos, prop.FindPropertyRelative("baseValue"), true);
                    
                    pos.y += EditorGUIUtility.singleLineHeight;
                    PrintModifiersSummary (pos, prop);
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            if (prop.FindPropertyRelative("showAdvanced").boolValue) {
                float h = GUITools.singleLineHeight * (prop.FindPropertyRelative("valueCapped").boolValue ? 4 : 3);
                if (Application.isPlaying) {
                    h += GUITools.singleLineHeight;
                    h += GUITools.singleLineHeight * prop.FindPropertyRelative("modifiers").arraySize;
                }
                return h;
            }
            
            return GUITools.singleLineHeight * (prop.FindPropertyRelative("valueCapped").boolValue ? 2 : 1);
        }
    }
    
#endif
    
}
