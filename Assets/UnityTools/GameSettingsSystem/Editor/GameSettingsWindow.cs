using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityTools.EditorTools;

namespace UnityTools.GameSettingsSystem.Internal {

    /*
        for easier editing, game settings objects that are in the project can be shown in this window
        and edited
    */

    [Serializable] public class GameSettingsWindow : EditorWindow {

        [MenuItem(ProjectTools.defaultMenuItemSection + "Unity Tools/Game Settings", false, ProjectTools.defaultMenuItemPriority)]
		static void OpenWindow () {
            EditorWindowTools.OpenWindowNextToInspector<GameSettingsWindow>("Game Settings");
		}

        public GameSettingsObject selectedSettings;
        SerializedObject windowObject;
        SerializedProperty selectedSettingsProp;
        UnityEditor.Editor settingsEditor;
        void OnEnable () {
            windowObject = new SerializedObject(this);    
            selectedSettingsProp = windowObject.FindProperty("selectedSettings");
        }

        List<AssetSelectorElement> OnAssetsLoaded(List<AssetSelectorElement> originals) {

            // update choices to reflect display names
            for (int i = 0; i < originals.Count; i++) {
                originals[i].displayName = (originals[i].asset as GameSettingsObject).DisplayName();
            }

            // remove where we dont want to show in this window
            return originals.Where( e => (e.asset as GameSettingsObject).ShowInGameSettingsWindow() ).ToList();
        }

        void CreateEditor ( ) {
            settingsEditor = UnityEditor.Editor.CreateEditor(selectedSettings);
        }

        void OnChangedCurrentObject () {
            MonoBehaviour.DestroyImmediate(settingsEditor);
            if (selectedSettings != null) CreateEditor();
        }

        static Vector2 scrollPosition;

        void OnGUI () {
            GUITools.Space(3);
            EditorGUI.BeginChangeCheck();

            windowObject.Update();

            if (GUILayout.Button("Refresh Game Settings")) {
                GameSettingsList.RefreshGameSettingsList();
            }

            EditorGUILayout.LabelField("Choose Settings:", EditorStyles.boldLabel);
            AssetSelector.Draw(typeof(GameSettingsObject), selectedSettingsProp, GUIContent.none, OnAssetsLoaded);
            windowObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck()) OnChangedCurrentObject();
            
            EditorGUILayout.Space();

            if (selectedSettings == null) return;

            if (settingsEditor == null) CreateEditor();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            // draw the object
            settingsEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
        }   
    }
}