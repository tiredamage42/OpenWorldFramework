// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityTools.EditorTools;
namespace UnityTools.Internal {
    #if UNITY_EDITOR
    
    public class FieldWithMessageDrawer : PropertyDrawer {

        protected SerializedProperty DrawRunTargetAndCallMethod (ref Rect pos, SerializedProperty prop, float callMethodWidth) {
            pos.width = 65;
            SerializedProperty runTargetProp = prop.FindPropertyRelative("runTarget");
            EditorGUI.PropertyField(pos, runTargetProp, GUITools.noContent, true);
            pos.x += pos.width;

            pos.width = callMethodWidth;
            GUITools.StringFieldWithDefault(pos.x, pos.y, pos.width, pos.height, prop.FindPropertyRelative("callMethod"), "Call Method");            
            pos.x += pos.width;
            return runTargetProp;

        }
        protected void DrawEnd (ref Rect pos, SerializedProperty prop, float origX, float origWidth, SerializedProperty runTargetProp) {
            
            bool showParameters = GUITools.DrawToggleButton(prop.FindPropertyRelative("showParameters"), new GUIContent("P", "Show Parameters"), pos.x, pos.y, GUITools.blue, GUITools.white);
            pos.x += GUITools.iconButtonWidth;
            
            if (runTargetProp.enumValueIndex == (int)RunTarget.Reference) {
                DrawReferenceTarget ( ref pos, prop, origX, origWidth);
            }
            
            DrawParameters (ref pos, prop, showParameters, origX, origWidth);
        }
        void DrawReferenceTarget (ref Rect pos, SerializedProperty prop, float origX, float origWidth) {
            pos.y += GUITools.singleLineHeight;
            pos.x = origX + GUITools.iconButtonWidth;
            pos.width = origWidth - GUITools.iconButtonWidth;

            
            EditorGUI.LabelField(pos, "Reference:");
            pos.x += 65;
            pos.width -= 65;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("referenceTarget"), GUITools.noContent, true);
        }
        void DrawParameters (ref Rect pos, SerializedProperty prop, bool showParameters, float origX, float origWidth) {
            pos.y += GUITools.singleLineHeight;
            pos.x = origX;
            pos.width = origWidth ;

            if (showParameters) {

                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("parameters"), true);
            }
            else {
                SerializedProperty paramsProp = prop.FindPropertyRelative("parameters");
                
                if (paramsProp.FindPropertyRelative("list").arraySize > 0) {
                    pos.x += GUITools.iconButtonWidth;
                    pos.width -= GUITools.iconButtonWidth;

                    MessageParametersDrawer.DrawFlat(pos, paramsProp);
                }
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            
            float h = GUITools.singleLineHeight;

            if (prop.FindPropertyRelative("showParameters").boolValue) {
                h += EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("parameters"), true);
            }
            else {
                if (prop.FindPropertyRelative("parameters").FindPropertyRelative("list").arraySize > 0) {
                    h += GUITools.singleLineHeight;
                }
            }
            if (prop.FindPropertyRelative("runTarget").enumValueIndex == (int)RunTarget.Reference) {
                h += GUITools.singleLineHeight;
            }
            return h;
        }
    }
    #endif

}
