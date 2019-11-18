


using UnityEngine;
using UnityEditor;
using UnityTools.GameSettingsSystem;

namespace OpenWorldFramework {

    // uncomment if we need to create a replacement...
    // [CreateAssetMenu(menuName="Open World Workflow/Open World Settings", fileName="OpenWorldSettings")]
    public class OpenWorldSettings : GameSettingsObject
    {
        public float cellSize = 200;
        public int gridResolution = 15;
        public int lod0_distance = 1;
        public int lod1_distance = 3;

        [HideInInspector] public string[] worldSceneNames_0;
        [HideInInspector] public string[] worldSceneNames_1;
        [HideInInspector] public Vector2Int[] worldSceneGrids;


        #if UNITY_EDITOR
        [Header("Editor Only:")]
        public string scenesListDirectory = "Assets/OpenWorldScenes/";

        [Header("Cell Display")]
        public float gizmoCubeHeight = 250;
        public float gizmoCubeDepth = 10;
        public Color gizmoCubeColor0 = new Color(1, 0, 0, .5f);
        public Color gizmoCubeColor1 = new Color(0, 0, 1, .25f);

        [HideInInspector] public SceneAsset[] sceneAssets_0;
        [HideInInspector] public SceneAsset[] sceneAssets_1;
        [HideInInspector] public SceneAsset openWorldSettingsSceneAsset;

        #endif

        string GetOppositeVersion (string[] checks, string[] opposites, string name) {
            for (int i = 0; i < checks.Length; i++) {
                if (checks[i] == name) {
                    return opposites[i];
                }
            }
            return null;
        }
        public string GetLOD0Version (string lod1sceneName) {
            return GetOppositeVersion (worldSceneNames_1, worldSceneNames_0, lod1sceneName);
        }
        public string GetLOD1Version (string lod0sceneName) {
            return GetOppositeVersion (worldSceneNames_0, worldSceneNames_1, lod0sceneName);
        }

        public Vector2Int Scene2Grid (string name, int lod) {
            string[] worldSceneNames = lod == 0 ? worldSceneNames_0 : worldSceneNames_1;
            for (int i = 0; i < worldSceneNames.Length; i++) {
                if (worldSceneNames[i] == name) {
                    return worldSceneGrids[i];
                }
            }
            Debug.LogError("Scene: " + name + " doesnt have an associated grid cell...");
            return new Vector2Int(-1, -1);
        }

        public string Grid2Scene (Vector2Int grid, int lod, bool debug = true) {
            for (int i = 0; i < worldSceneGrids.Length; i++) {
                if (worldSceneGrids[i] == grid) {
                    return lod == 0 ? worldSceneNames_0[i] : worldSceneNames_1[i];
                }
            }
            if (debug) Debug.LogError("Couldnt Find Scene For Grid " + grid);
            return null;
        }
    }
}
