using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using System.IO;
using UnityEngine.SceneManagement;

using System;

using UnityTools;
using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;
using UnityTools.GameSettingsSystem.Internal;

using OpenWorldFramework.Terrain;

namespace OpenWorldFramework {
    [CustomEditor(typeof(OpenWorldSettings))]
    public class OpenWorldSettingsEditor : Editor {

        static void GetOpenWorldSceneAssetsInDirectory (OpenWorldSettings settings, out SceneAsset openWorldSettingsSceneAsset, out SceneAsset[] lod0s, out SceneAsset[] lod1s) {
            openWorldSettingsSceneAsset = null;

            List<SceneAsset> sceneAssets0 = new List<SceneAsset>();
            List<SceneAsset> sceneAssets1 = new List<SceneAsset>();
            string dir = GetDirectory(settings);
            if (Directory.Exists(dir)) {
                int unityDirectoryLength = Application.dataPath.Length-6;
                DirectoryInfo di = new DirectoryInfo(dir);
                foreach (FileInfo file in di.GetFiles()) {
                    string name = file.FullName;
                    if (!name.EndsWith(".meta")) {
                        if (name.EndsWith(".unity")) {
                            bool isSettingsScene = name.Contains(OpenWorld.openWorldSettingsScene);
                            bool isCellScene = !isSettingsScene && name.Contains(OpenWorld.openWorldSceneKey);
                            if (isSettingsScene || isCellScene) {
                                SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(name.Substring(unityDirectoryLength));
                                if (isCellScene) {
                                    (name.Contains(OpenWorld.lod0Check) ? sceneAssets0 : sceneAssets1).Add(asset);
                                }
                                else {
                                    openWorldSettingsSceneAsset = asset;
                                }
                            }
                        }
                    }
                }
            }
            lod0s = sceneAssets0.ToArray();
            lod1s = sceneAssets1.ToArray();
        }

        static void AddScenesToBuildSettings (OpenWorldSettings settings) {
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            
            List<EditorBuildSettingsScene> finalScenes = new List<EditorBuildSettingsScene>();
            
            // add all non open world scenes taht were already in the build settings...
            for (int i = 0; i < buildScenes.Length; i++) {
                string path = buildScenes[i].path;

                if (string.IsNullOrEmpty(path))
                    continue;
                    
                if (path.Contains(OpenWorld.openWorldSceneKey) || path.Contains(OpenWorld.openWorldSettingsScene))
                    continue;
                
                finalScenes.Add(buildScenes[i]);
            }


            // add the settings scene
            finalScenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(settings.openWorldSettingsSceneAsset), true));
            
            // add lod0 and lod1 scenes...
            for (int i = 0; i < settings.sceneAssets_0.Length; i++) finalScenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(settings.sceneAssets_0[i]), true));
            for (int i = 0; i < settings.sceneAssets_1.Length; i++) finalScenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(settings.sceneAssets_1[i]), true));
            
            EditorBuildSettings.scenes = finalScenes.ToArray();
        }
        

        static void UpdateWorldNamesPerSceneList (SceneAsset[] sceneAssets, ref string[] worldSceneNames) {
            int l = sceneAssets.Length;
            if (l != worldSceneNames.Length) 
                worldSceneNames = new string[l];
            
            for (int i = 0; i < l; i++) {
                worldSceneNames[i] = sceneAssets[i] == null ? string.Empty : Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sceneAssets[i]));
            }
        }
        static readonly string[] keySplit = new string[] { OpenWorld.openWorldSceneKey };
        

        static void _UpdateSceneAssetNames (OpenWorldSettings settings) {

            GetOpenWorldSceneAssetsInDirectory(settings, out settings.openWorldSettingsSceneAsset, out settings.sceneAssets_0, out settings.sceneAssets_1);
            AddScenesToBuildSettings(settings);
            UpdateWorldNamesPerSceneList (settings.sceneAssets_0, ref settings.worldSceneNames_0);
            UpdateWorldNamesPerSceneList (settings.sceneAssets_1, ref settings.worldSceneNames_1);

            int l = settings.sceneAssets_0.Length;
            if (l != settings.worldSceneGrids.Length) 
                settings.worldSceneGrids = new Vector2Int[l];
            
            for (int i = 0; i < l; i++) {
                if (settings.sceneAssets_0[i] != null) {
                    string[] xy = (settings.worldSceneNames_0[i].Split(keySplit, StringSplitOptions.None)[0].Split('@')[0]).Split('_');
                    settings.worldSceneGrids[i] = new Vector2Int( int.Parse(xy[0]), int.Parse(xy[1]) );
                }
                else {
                    settings.worldSceneGrids[i] = new Vector2Int( -1, -1 );
                }
            }
        }
        public static void UpdateSceneAssetNames () {
            OpenWorldSettings settings = GameSettings.GetSettings<OpenWorldSettings>();
            if (settings != null)
                _UpdateSceneAssetNames(settings);
        }
        
        OpenWorldSettings _s;
        OpenWorldSettings openWorldSettings {
            get {
                if (_s == null) _s = target as OpenWorldSettings;
                return _s;
            }
        }

        public static string GetDirectory(OpenWorldSettings settings) {
            if (!settings.scenesListDirectory.EndsWith("/")) 
                return settings.scenesListDirectory + "/";
            return settings.scenesListDirectory;
        }
        public string GetDirectory () {
            return GetDirectory(openWorldSettings);
        }
            
        


        public string Grid2Scene (Vector2Int grid, int lod, bool debug) {
            return Grid2Scene (openWorldSettings, grid, lod, debug);
        }

        public static string Grid2Scene (OpenWorldSettings settings, Vector2Int grid, int lod, bool debug) {

            string scene = settings.Grid2Scene(grid, lod, debug);
            if (scene != null) {
                return GetDirectory(settings) + scene + ".unity";
            }
            return scene;
        }

        
        void BuildScene (int x, int y, int lod, string directory) {
            string sceneName = x.ToString() + "_" + y.ToString() + "@LOD" + lod.ToString() + OpenWorld.openWorldSceneKey + "WorldScene";
            Scene scene = EditorSceneManager.GetSceneByName(sceneName);

            bool wasOpen = scene.IsValid();
            if (!wasOpen) {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

                GameObject g = new GameObject("Editor_CellDisplay");
                g.AddComponent<CellGizmo>();
                EditorSceneManager.MoveGameObjectToScene(g, scene);
                // PrefabUtility.InstantiatePrefab(openWorldSettings.cellDisplayPrefab, scene);
            }

            EditorSceneManager.SaveScene(scene, directory + sceneName + ".unity");
            if (!wasOpen)
                EditorSceneManager.CloseScene(scene, true);   
        }

        void BuildSettingsSceneIfNull (string directory) {
            if (openWorldSettings.openWorldSettingsSceneAsset == null) {
                if (!Directory.Exists(directory)) 
                    Directory.CreateDirectory(directory);
                
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(scene, directory + OpenWorld.openWorldSettingsScene + ".unity");
            }
        }

        void UpdateWorldToResolution () {
            GameSettingsList.disableRefresh = true;
            string directory = this.GetDirectory();
            UpdateSceneAssetNames();
            
            BuildSettingsSceneIfNull(directory);
            
            int countInDirectory = openWorldSettings.sceneAssets_0.Length;
            int newTotalCount = openWorldSettings.gridResolution * openWorldSettings.gridResolution;
            
            if (countInDirectory < newTotalCount) ExpandWorld(directory);
            else if (countInDirectory > newTotalCount) ShrinkWorld(directory);
            
            UpdateSceneAssetNames();       
            GameSettingsList.disableRefresh = false;     

            AssetDatabase.SaveAssets();
        }

        void ExpandWorld (string directory) {
            
            if (!Directory.Exists(directory)) 
                Directory.CreateDirectory(directory);
            
            UpdateSceneAssetNames();
            
            for (int y = 0; y < openWorldSettings.gridResolution; y++) {
                for (int x = 0; x < openWorldSettings.gridResolution; x++) {
                    if (Grid2Scene(new Vector2Int(x, y), 0, false) == null) {
                        BuildScene (x, y, 0, directory);
                        BuildScene (x, y, 1, directory);
                    }
                }    
            }
        }

        void ShrinkWorld (string directory){
            
            if (!Directory.Exists(directory))
                return;
            
            HashSet<Vector2Int> usedCells = new HashSet<Vector2Int>();
            for (int y = 0; y < openWorldSettings.gridResolution; y++) {
                for (int x = 0; x < openWorldSettings.gridResolution; x++) {
                    usedCells.Add(new Vector2Int(x, y));
                }    
            }

            if (!EditorUtility.DisplayDialog("Shrink World Scenes", "Are you sure you want to DELETE the excess cell scenes in directory: " + directory + "?", "Yes", "No")) return;
            if (!EditorUtility.DisplayDialog("Shrink World Scenes", "Are you absolutely sure?", "Yes", "No")) return;
            
            // for (int i = 0; i < openWorldSettings.sceneAssets_0.Length; i++) {
            for (int i = openWorldSettings.sceneAssets_0.Length - 1; i >= 0; i--) {
            
                string assetPath_0 = AssetDatabase.GetAssetPath(openWorldSettings.sceneAssets_0[i]); 
                Vector2Int cell = openWorldSettings.Scene2Grid(Path.GetFileNameWithoutExtension(assetPath_0), 0);
                if (!usedCells.Contains(cell)) {
                    Debug.Log("Deleteing file in shrink: " + assetPath_0);
                    string assetPath_1 = AssetDatabase.GetAssetPath(openWorldSettings.sceneAssets_1[i]); 
                    AssetDatabase.DeleteAsset(assetPath_0);
                    AssetDatabase.DeleteAsset(assetPath_1);
                }   
            }
        }

        bool SettingsObjectIsOk (out string msg) {
            msg = "";
            int worldCount = openWorldSettings.gridResolution * openWorldSettings.gridResolution;
            if (openWorldSettings.sceneAssets_0.Length != worldCount) {
                if (openWorldSettings.sceneAssets_0.Length < worldCount) {
                    msg += "Not Every Cell Has A Scene Built... Update World Scenes To Build New Scenes Before Continuing";
                }
                else {
                    int res = (int)Mathf.Sqrt(openWorldSettings.sceneAssets_0.Length);
                    msg += "Too Many Scenes (" + res + "x" + res + ") For World Grid Resolution (" + openWorldSettings.gridResolution + "x" + openWorldSettings.gridResolution + ").  Update World Scenes To Delete Excess Scenes";
                }
            }
            if (openWorldSettings.openWorldSettingsSceneAsset == null) {
                msg += "\n\nNo Settings Scene Exists... Update World Scenes To Build A New World Settings Scene";
            }
            return string.IsNullOrEmpty(msg);
        }


        UnityEngine.Terrain splitTerrain;

        
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            GUITools.Space(2);


            string msg;
            bool isOk = SettingsObjectIsOk (out msg);

            if (!isOk) {
                EditorGUILayout.HelpBox(msg, MessageType.Error);   
                if (GUILayout.Button("Update World To Cell Resolution")) {
                    UpdateWorldToResolution();
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                }
            }
            else {

                if (!Application.isPlaying) {

                    EditorGUILayout.LabelField("Grid Scenes:", GUITools.boldLabel);
                    DrawSelectScenesGrid();
                    
                    GUITools.Space(3);
                    EditorGUILayout.LabelField("Split Terrain Into World:", GUITools.boldLabel);
                    splitTerrain = (UnityEngine.Terrain)EditorGUILayout.ObjectField(splitTerrain, typeof(UnityEngine.Terrain), true);
                    if (splitTerrain != null && GUILayout.Button("Split Terrain Into World")) {
                        OpenWorldFramework.Terrain.TerrainTools.SplitTerrainIntoWorldCells(openWorldSettings, splitTerrain, 257, 256, -1);//256 );
                    }
                    GUITools.Space();
                    GUI.backgroundColor = GUITools.red;
                    if (GUILayout.Button("Remove Terrains From World")) {
                        OpenWorldFramework.Terrain.TerrainTools.RemoveTerrainsFromWorld(openWorldSettings);
                    }
                    GUI.backgroundColor = GUITools.white;
                }
            }
        }

        void DrawSelectScenesGrid () {
            EditorGUILayout.BeginHorizontal();
            GUITools.Space(3);
            EditorGUILayout.BeginVertical();
            
            HashSet<Vector2Int> lod0Loaded, lod1Loaded;
            GetLoadedGrids (out lod0Loaded, out lod1Loaded);
            
            for (int y = openWorldSettings.gridResolution - 1; y >= 0; y--) {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < openWorldSettings.gridResolution; x++) {
                    Vector2Int grid = new Vector2Int(x, y);
                    string gridString = x.ToString()+","+y.ToString();
                    if (GUITools.IconButton(new GUIContent(gridString, gridString), lod0Loaded.Contains(grid) ? GUITools.blue : (lod1Loaded.Contains(grid) ? GUITools.gray : GUITools.white))) {
                        OpenGridSceneInEditor(grid);
                    }
                }    
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
            

        void GetLoadedGrids (out HashSet<Vector2Int> lod0Loaded, out HashSet<Vector2Int> lod1Loaded) {
            lod0Loaded = new HashSet<Vector2Int>();
            lod1Loaded = new HashSet<Vector2Int>();
            
            for (int i =0 ; i < EditorSceneManager.sceneCount; i++) {
                Scene s = EditorSceneManager.GetSceneAt(i);
                if (s.name.Contains(OpenWorld.openWorldSceneKey)) {
                    if (s.name.Contains(OpenWorld.lod0Check)) 
                        lod0Loaded.Add(openWorldSettings.Scene2Grid(s.name, 0));
                    else if (s.name.Contains(OpenWorld.lod1Check))
                        lod1Loaded.Add(openWorldSettings.Scene2Grid(s.name, 1));
                }
            }
        }


        List<string> GetLoadedNonWorldScenes () {
            List<string> openNonWorldScenes = new List<string>();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++) {
                Scene s = EditorSceneManager.GetSceneAt(i);
                if (!s.name.Contains(OpenWorld.openWorldSceneKey)) {
                    if (!s.name.Contains(OpenWorld.openWorldSettingsScene)) {
                        openNonWorldScenes.Add(s.path);
                    }
                }
            }
            return openNonWorldScenes;
        }

        void OpenGridNeighbors (Vector2Int grid) {
            int range = openWorldSettings.lod1_distance;
            for (int x = -range; x <= range; x++) {
                for (int y = -range; y <= range; y++) {
                    if (x == 0 && y == 0)
                        continue;
                    
                    Vector2Int neighbor = new Vector2Int(grid.x + x, grid.y + y);
                    if (neighbor.x < 0 || neighbor.x >= openWorldSettings.gridResolution) continue;
                    if (neighbor.y < 0 || neighbor.y >= openWorldSettings.gridResolution) continue;
                    

                    int dist = GridManagement.GridDistance(neighbor, grid);
                    OpenSceneIfWithinRange (neighbor, dist, openWorldSettings.lod1_distance, 1);
                    OpenSceneIfWithinRange (neighbor, dist, openWorldSettings.lod0_distance, 0);
                }   
            }
        }

        void OpenSceneIfWithinRange (Vector2Int neighbor, int cellDistance, int threshold, int lod) {
            if (cellDistance <= threshold) {
                string neighborScene = Grid2Scene(neighbor, lod, true);
                if (neighborScene != null) 
                    EditorSceneManager.OpenScene( neighborScene, OpenSceneMode.Additive );
            }   
        }

        void OpenGridSceneInEditor (Vector2Int grid) {
            string selectedScene0 = Grid2Scene(grid, 0, true);
            string selectedScene1 = Grid2Scene(grid, 1, true);
                         
            if (selectedScene0 != null) {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {

                    List<string> openNonWorldScenes = GetLoadedNonWorldScenes();

                    EditorSceneManager.OpenScene(GetDirectory() + OpenWorld.openWorldSettingsScene + ".unity", OpenSceneMode.Single);
            
                    EditorSceneManager.OpenScene( selectedScene1, OpenSceneMode.Additive );
                    EditorSceneManager.OpenScene( selectedScene0, OpenSceneMode.Additive );
                    
                    OpenGridNeighbors(grid);
                    
                    for (int i = 0; i < openNonWorldScenes.Count; i++) {
                        EditorSceneManager.OpenScene( openNonWorldScenes[i], OpenSceneMode.Additive );
                    }
                }
            }
        }
    }

}