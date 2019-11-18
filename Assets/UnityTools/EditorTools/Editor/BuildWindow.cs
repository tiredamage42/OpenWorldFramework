
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace UnityTools.EditorTools {

    // custom class so it can be drawn as asset selector iwithin array
    [System.Serializable] public class SceneAssetArrayElement : NeatArrayElement { [AssetSelection(typeof(SceneAsset))] public SceneAsset element; }
    [System.Serializable] public class SceneAssetArray : NeatArrayWrapper<SceneAssetArrayElement> {  }

    [System.Serializable] public class BuildWindow : EditorWindow
    {
        //% (ctrl on Windows, cmd on macOS), # (shift), & (alt).

        [MenuItem(ProjectTools.defaultMenuItemSection + "Unity Tools/Editor Tools/Build Window", false, ProjectTools.defaultMenuItemPriority)]
		static void OpenWindow () {
            EditorWindowTools.OpenWindowNextToInspector<BuildWindow>("Build");
		}

        [NeatArray] public SceneAssetArray scenes;
        SerializedObject windowSO;
        int topSpaces = 5;

        SerializedProperty scenesList { get { return windowSO.FindProperty("scenes").FindPropertyRelative("list"); } }
        void UpdateToReflectSettings() {

            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            int l = buildScenes.Length;

            SerializedProperty scenes = scenesList;

            int u = 0;
            for (int i = 0; i < l; i++) {

                if (PathContainsBuildSettingsIgnore(buildScenes[i].path))
                    continue;
                    
                if (string.IsNullOrEmpty(buildScenes[i].path))
                    continue;

                if (u >= scenes.arraySize) 
                    scenes.InsertArrayElementAtIndex(scenes.arraySize);

                SerializedProperty scene = scenes.GetArrayElementAtIndex(u).FindPropertyRelative(NeatArray.elementName);
                scene.objectReferenceValue = AssetDatabase.LoadAssetAtPath(buildScenes[i].path, typeof(SceneAsset));

                u++;
            }
        }

        void UpdateSettings() {

            EditorBuildSettingsScene[] originalBuildScenes = EditorBuildSettings.scenes;
            int l = originalBuildScenes.Length;

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

            for (int i = 0; i < originalBuildScenes.Length; i++) {
                if (PathContainsBuildSettingsIgnore(originalBuildScenes[i].path)) {
                    buildScenes.Add(originalBuildScenes[i]);
                }
            }
            
            SerializedProperty scenes = scenesList;
            for (int i = 0; i < scenes.arraySize; i++) {
                SerializedProperty scene = scenes.GetArrayElementAtIndex(i).FindPropertyRelative(NeatArray.elementName);
                if (scene.objectReferenceValue != null) {
                    buildScenes.Add( new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(scene.objectReferenceValue), true) );
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        bool PathContainsBuildSettingsIgnore (string path) {
            for (int i = BuildSettings.buildWindowIgnorePatterns.Count - 1; i >= 0; i--) {
                string pattern = BuildSettings.buildWindowIgnorePatterns[i];

                if (string.IsNullOrEmpty(pattern)) {
                    BuildSettings.buildWindowIgnorePatterns.RemoveAt(i);
                    continue;
                }
                if (path.Contains(pattern)) {
                    return true;
                }
            }
            return false;
        }
        
        Vector2 scrollPos;
        void OnGUI () {

            if (windowSO == null) windowSO = new SerializedObject(this);
            
            GUITools.Space(topSpaces);

            scrollPos = GUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.LabelField("Scenes to build:", GUITools.boldLabel);

            UpdateToReflectSettings ();
            windowSO.ApplyModifiedProperties();

            EditorGUI.BeginChangeCheck();
            
            windowSO.Update();
            
            EditorGUILayout.PropertyField(windowSO.FindProperty("scenes"), true);
            

            GUILayout.EndScrollView();
            if (EditorGUI.EndChangeCheck()) {
                UpdateSettings();
            }

            windowSO.ApplyModifiedProperties();
        }
    }    
}