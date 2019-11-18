using System.Collections.Generic;
using UnityEngine;

using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

using UnityEditor;
using UnityTools.GameSettingsSystem.Internal;
using UnityTools;



namespace OpenWorldFramework.Terrain {
    public class TerrainTools 
    {

        public static void RemoveTerrainsFromWorld (OpenWorldSettings openWorldSettings) {
            
            if (!EditorUtility.DisplayDialog("Remove Terrains", "Are you sure you want to remove terrain objects from all world scenes?", "Yes", "No"))
                return;

            string directory = OpenWorldSettingsEditor.GetDirectory(openWorldSettings);
            
            GameSettingsList.disableRefresh = true;
            
            for (int i = 0; i < openWorldSettings.worldSceneNames_1.Length; i++) {
                string scenePath = directory + openWorldSettings.worldSceneNames_1[i] + ".unity";

                Scene scene = EditorSceneManager.GetSceneByName(openWorldSettings.worldSceneNames_1[i]);

                bool wasOpen = scene.IsValid();
                if (!wasOpen) 
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                GameObject[] rootObjects = scene.GetRootGameObjects();
                for (int x = 0; x < rootObjects.Length; x++) {
                    TerrainChunk terrain = rootObjects[x].GetComponentInChildren<TerrainChunk>(true);
                    if (terrain != null) {
                        MonoBehaviour.DestroyImmediate(terrain.gameObject);
                        break;
                    }
                }   

                EditorSceneManager.SaveScene(scene, scenePath);                
                if (!wasOpen) 
                    EditorSceneManager.CloseScene(scene, true);   
            }

            GameSettingsList.disableRefresh = false;

            AssetDatabase.SaveAssets();
        }

        public static void SplitTerrainIntoWorldCells ( 
            OpenWorldSettings openWorldSettings, UnityEngine.Terrain terrainToSplit, 
            int heightMapResolution, int alphaMapResolution, int detailMapResolution 
        ) {
            
            int splits = UnityTools.TerrainTools.CalculateChunksInTerrain(terrainToSplit, openWorldSettings.cellSize);

            int maxGrids = openWorldSettings.gridResolution;
            if (splits > maxGrids) {
                Debug.LogError("Splitting would require: " + splits + "x" + splits + " cell resolution..., current max is: " + maxGrids + "x" + maxGrids + ". Try increasing cell size or cell resolution...");
                return;
            }

            GameSettingsList.disableRefresh = true;
            Dictionary<Vector2Int, UnityEngine.Terrain> splitTerrains = UnityTools.TerrainTools.SplitTerrainIntoChunks(
                terrainToSplit, openWorldSettings.cellSize, 
                heightMapResolution, alphaMapResolution, detailMapResolution
            );
            if (splitTerrains != null) {
                SplitTerrainsIntoScenes (openWorldSettings, splitTerrains);
            }
            GameSettingsList.disableRefresh = false;
        }
        
        static void SplitTerrainsIntoScenes (OpenWorldSettings openWorldSettings, Dictionary<Vector2Int, UnityEngine.Terrain> splitTerrains) {
            foreach (var grid in splitTerrains.Keys) {
                string scenePath = OpenWorldSettingsEditor.Grid2Scene(openWorldSettings, grid, 1, true);
                if (!string.IsNullOrEmpty(scenePath)) {
                    Scene scene = EditorSceneManager.GetSceneByName(Path.GetFileNameWithoutExtension(scenePath));
                    bool wasOpen = scene.IsValid();
                    
                    if (!wasOpen) 
                        scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    
                    splitTerrains[grid].gameObject.AddComponent<TerrainChunk>();

                    EditorSceneManager.MoveGameObjectToScene(splitTerrains[grid].gameObject, scene);

                    EditorSceneManager.SaveScene(scene, scenePath);
                    
                    if (!wasOpen) 
                        EditorSceneManager.CloseScene(scene, true);   
                }
                else {
                    Debug.LogError("Terrain at grid: " + grid + " is out of world scenes range... Keepign in Scene: " + EditorSceneManager.GetActiveScene().name );
                }
            }
        }
    }
}