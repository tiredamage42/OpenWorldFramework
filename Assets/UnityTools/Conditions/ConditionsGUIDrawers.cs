using UnityEngine;
using UnityEditor;
using UnityTools.EditorTools;

namespace UnityTools.Internal {
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Conditions))] class ConditionsDrawer : NeatArrayWithMessageParametersInElements { }

    /*
        DRAW A SINGLE CONDITION:
    */
    [CustomPropertyDrawer(typeof(Condition))] 
    class ConditionDrawer : FieldWithMessageDrawer {
        static readonly string[] numericalCheckOptions = new string[] { " == ", " != ", " < ", " > ", " <= ", " >= " };
        protected const float numericalCheckWidth = 40;

        static GUIContent _useGlobalValueGUI;
        static GUIContent useGlobalValueGUI {
            get {
                if (_useGlobalValueGUI == null) {
                    _useGlobalValueGUI = BuiltInIcons.GetIcon("ToolHandleGlobal", "Use Global Value Threshold");
                }
                return _useGlobalValueGUI;
            }
        }
        
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);

            float origX = pos.x;
            float origWidth = pos.width;


            SerializedProperty runTargetProp = DrawRunTargetAndCallMethod (ref pos, prop, 125);

            SerializedProperty numCheckProp = prop.FindPropertyRelative("numericalCheck");
            pos.width = numericalCheckWidth;
            numCheckProp.enumValueIndex = EditorGUI.Popup (pos, numCheckProp.enumValueIndex, numericalCheckOptions);
            pos.x += pos.width;
            
            SerializedProperty useGlobalValueThresholdProp = prop.FindPropertyRelative("useGlobalValueThreshold");
            bool useGlobalValueThreshold = useGlobalValueThresholdProp.boolValue;
            

            if (useGlobalValueThreshold) {
                pos.width = 100;
        
                GlobalGameValues.DrawGlobalValueSelector (pos, prop.FindPropertyRelative("globalValueThresholdName"));

            }
            else {

                pos.width = 60;
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("threshold"), GUITools.noContent, true);
            }
            pos.x += pos.width;


            GUITools.DrawToolbarDivider(pos.x, pos.y);
            pos.x += GUITools.toolbarDividerSize;
            
            useGlobalValueThreshold = GUITools.DrawToggleButton(useGlobalValueThresholdProp, useGlobalValueGUI, pos.x, pos.y, GUITools.blue, GUITools.white);
            pos.x += GUITools.iconButtonWidth;
            
            SerializedProperty orProp = prop.FindPropertyRelative("or");
            orProp.boolValue = GUITools.DrawToggleButton(orProp.boolValue, new GUIContent(orProp.boolValue ? "||" : "&&"), pos.x, pos.y, GUITools.white, GUITools.white);
            pos.x += GUITools.iconButtonWidth;

            DrawEnd(ref pos, prop, origX, origWidth, runTargetProp);

            EditorGUI.EndProperty();
        }
    }

    #endif
}

