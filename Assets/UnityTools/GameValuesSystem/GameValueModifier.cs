using UnityEngine;

using UnityEditor;
using UnityTools.EditorTools;
namespace UnityTools {

    [System.Serializable] public class GameValueModifierArray : NeatArrayWrapper<GameValueModifier> { }
    [System.Serializable] public class GameValueModifierArray2D { 
        public bool displayed;
        [NeatArray] public GameValueModifierArray[] list; 
    }
    
    
    [System.Serializable] public class GameValueModifier {
        public bool isPermanent {
            get {
                return modifyValueComponent == GameValue.GameValueComponent.BaseValue
                    || modifyValueComponent == GameValue.GameValueComponent.BaseMinValue
                    || modifyValueComponent == GameValue.GameValueComponent.BaseMaxValue;
            }
        }

        public Conditions conditions;
        
        public GameValueModifier (GameValue.GameValueComponent modifyValueComponent, ModifyBehavior modifyBehavior, float modifyValue) {
            this.modifyValueComponent = modifyValueComponent;
            this.modifyBehavior = modifyBehavior;
            this.modifyValue = modifyValue;
        }

        public GameValueModifier () {
            count = 1;
        }
        
        public GameValueModifier (GameValueModifier template, int count, int key, string description) {
            this.key = key;
            this.count = count;
            this.description = description;

            gameValueName = template.gameValueName;
            modifyValueComponent = template.modifyValueComponent;
            modifyBehavior = template.modifyBehavior;
            modifyValue = template.modifyValue;
        }

        public string description;
        [HideInInspector] public int key;
        [HideInInspector] public int count = 1;
        int getCount { get { return isStackable ? count : 1; } }
        public bool isStackable;
        public string gameValueName = "Game Value Name";
        public GameValue.GameValueComponent modifyValueComponent;
        
        public enum ModifyBehavior { Add, Multiply, Set };
        public ModifyBehavior modifyBehavior;
        public float modifyValue = 0;

        public bool showConditions;

        public float Modify(float baseValue) {
            if (modifyBehavior == ModifyBehavior.Set)
                return modifyValue;
            else if (modifyBehavior == ModifyBehavior.Add)
                return baseValue + (modifyValue * getCount);
            else if (modifyBehavior == ModifyBehavior.Multiply)
                return baseValue * (modifyValue * getCount);
            return baseValue;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GameValueModifier))] public class GameValueModifierDrawer : PropertyDrawer
    {
        static GUIContent _stackableContent;
        static GUIContent stackableContent { get { 
            if (_stackableContent == null) _stackableContent = BuiltInIcons.GetIcon("UnityEditor.SceneHierarchyWindow", "Is Stackable"); 
            return _stackableContent;
        } }

        static GUIContent _showConditionsContent;
        static GUIContent showConditionsContent { get { 
            if (_showConditionsContent == null) _showConditionsContent = new GUIContent("?", "Show Conditions"); 
            return _showConditionsContent;
        } }
        
        static readonly float[] widths = new float[] { 60, 100, 90, 80, 60, 15, };
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;

            int i = 0;

            float origX = position.x;
            float origW = position.width;
            
            position.width = widths[i];
            EditorGUI.PropertyField(position, property.FindPropertyRelative("modifyBehavior"), GUITools.noContent);
            position.x += widths[i++];
                        
            GUITools.StringFieldWithDefault ( position.x, position. y, widths[i], EditorGUIUtility.singleLineHeight, property.FindPropertyRelative("gameValueName"), "Value Name");
            position.x+= widths[i++];

            position.width = widths[i];
            EditorGUI.PropertyField(position, property.FindPropertyRelative("modifyValueComponent"), GUITools.noContent);
            position.x+= widths[i++];
            
            position.width = widths[i];
            EditorGUI.PropertyField(position, property.FindPropertyRelative("modifyValue"), GUITools.noContent);
            position.x+= widths[i++];

            GUITools.DrawToggleButton(property.FindPropertyRelative("isStackable"), stackableContent, position.x, position.y, GUITools.blue, GUITools.white);
            position.x += GUITools.iconButtonWidth;

            SerializedProperty showConditions = property.FindPropertyRelative("showConditions");
            SerializedProperty conditionsProp = property.FindPropertyRelative("conditions");

            if (!showConditions.boolValue) {
                if (conditionsProp.FindPropertyRelative("list").arraySize > 0) {
                    showConditions.boolValue = true;
                }
            }

            GUITools.DrawToggleButton(showConditions, showConditionsContent, position.x, position.y, GUITools.blue, GUITools.white);
            
            if (showConditions.boolValue) {
                position.x = origX;
                position.y += EditorGUIUtility.singleLineHeight;
                position.width = origW;
                EditorGUI.PropertyField(position, conditionsProp, new GUIContent("Conditions"));
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty showConditions = property.FindPropertyRelative("showConditions");

            return GUITools.singleLineHeight + (showConditions.boolValue ? EditorGUI.GetPropertyHeight(property.FindPropertyRelative("conditions"), true) : 0);
        }
    }
#endif
}
