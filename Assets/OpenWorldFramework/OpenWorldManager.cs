using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

using UnityTools;
using UnityTools.GameSettingsSystem;
namespace OpenWorldFramework {

    public class OpenWorldManager : InitializationSingleTon<OpenWorldManager>
    {
        
        public static void FastTravelTo (Vector3 worldPosition) {
            FastTravel.FastTravelTo(settings.Grid2Scene(GridManagement.CalculateGrid(worldPosition, settings.cellSize), 0), worldPosition);
        }
        public static void FastTravelTo (Vector2Int grid) {
            FastTravelTo(grid, null);
        }
        public static void FastTravelTo (Vector2Int grid, string fastTravelTargetName) {
            FastTravel.FastTravelTo(settings.Grid2Scene(grid, 0), fastTravelTargetName);
        }


        bool GetFastTravelDefaultPosition (string scene, out Vector3 position) {
            position = Vector3.zero;
            if (!scene.Contains(OpenWorld.openWorldSceneKey)) {
                return false;
            }
            Vector2Int grid = settings.Scene2Grid(scene, scene.Contains(OpenWorld.lod0Check) ? 0 : 1);
            float halfCell = settings.cellSize * .5f;
            position = new Vector3( grid.x * settings.cellSize + halfCell, 0, grid.y * settings.cellSize + halfCell );
            return true;
        }

        
        public static bool isInOpenWorldScene { get { return SceneLoading.activeScene == OpenWorld.openWorldSettingsScene; } }

        static OpenWorldSettings _settings;
        static OpenWorldSettings settings {
            get {
                if (_settings == null) _settings = GameSettings.GetSettings<OpenWorldSettings>();
                return _settings;
            }
        }

        struct WorldScene {
            public Vector2Int grid;
            public string scene;
            public int lod;
            public WorldScene (Vector2Int grid, string scene, int lod){
                this.grid = grid;
                this.scene = scene;
                this.lod = lod;
            }
        }


        protected override void Awake() {
            base.Awake();
            if (!thisInstanceErrored) {
                Application.backgroundLoadingPriority = ThreadPriority.Low;
                
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneLoading.onSceneExit += OnSceneExit;
                SceneLoading.onSceneUnload += OnSceneUnload;
                SceneLoading.SetSceneLoadOverride(OverrideSceneLoad);

                FastTravel.SetGetFastTravelDefaultPosition(GetFastTravelDefaultPosition);
                SaveObject.SetSaveObjectsSceneFilter (FilterSaveObjectsForScene);
        
            }
        }

        static List<MonoBehaviour> FilterSaveObjectsForScene (string scene, List<MonoBehaviour> originalList) {
            
            // check if it's an open world scene we're trying to load
            if (!scene.Contains(OpenWorld.openWorldSceneKey)) {

                // dont load or save objects to the settings scene....
                if (scene == OpenWorld.openWorldSettingsScene)
                    originalList.Clear();
                
                return originalList;
            }
            
            // non static objects should only load / save in lod 0 scenes....
            if (!scene.Contains(OpenWorld.lod0Check)) {
                originalList.Clear();
                return originalList;
            }


            Vector2Int sceneGrid = settings.Scene2Grid(scene, 0);
            float cellSize = settings.cellSize;

            // FILTER OUT IF NOT IN SCENE GRID
            for (int i = originalList.Count - 1; i >= 0; i--) {
                if (GridManagement.CalculateGrid(originalList[i].transform.position, cellSize) != sceneGrid) {
                    originalList.Remove(originalList[i]);
                }
            }
            
            return originalList;
        }

        // when we're entering the "open world" for the first time
        // we need to make sure the settings scene is laoded first and is the active scene,
        // in order to keep the lighting/ fog settings consistent for all loaded cell scenes...
        static bool OverrideSceneLoad (string scene, Action<LoadSceneMode> onSceneStartLoad, Action<string, LoadSceneMode> onSceneLoaded, LoadSceneMode loadSceneMode) {
            
            // check if it's an open world scene we're trying to load
            if (!scene.Contains(OpenWorld.openWorldSceneKey)) {
                return false;
            }
            // check if we're just additively loading the scene,
            // if we are, then let's assume the world is already loaded..
            if (loadSceneMode == LoadSceneMode.Additive) {
                return false;
            }

            // else we're going into the "open world" for the first time

            // call this after we've finished loading the open world main scene
            Action<string, LoadSceneMode> onOpenWorldMainSceneLoaded = (s, m) => {
                
                Action<string, LoadSceneMode> onTargetSceneLoaded = (s2, m2) => {
                    SceneLoading.SetPlayerScene(s2);
                    if (onSceneLoaded != null)
                        onSceneLoaded(s2, m2);
                };

                // make sure lod0 and lod1 version are loaded.
                // it's assumed that if we're single loading the open world, we're moving the player there
                // so we make sure all lods are loaded, so we dotn accidentally fall trhough unloaded terrain
                // when unpausing after load...


                bool isLOD0 = scene.Contains(OpenWorld.lod0Check);

                string lod0Version = isLOD0 ? scene : settings.GetLOD0Version(scene);
                string lod1Version = !isLOD0 ? scene : settings.GetLOD1Version(scene);
                                
                SceneLoading.LoadSceneAsync(lod0Version, null, null, LoadSceneMode.Additive, true);
                    
                // fake single load, so game keeps paused / ui's still black out, etc...
                // use finished loading callbacks for lod1 version, so we dont accidentally fall through non loaded terrrain
                SceneLoading.LoadSceneAsync(lod1Version, null, onTargetSceneLoaded, LoadSceneMode.Additive, true);
            };

            SceneLoading.LoadSceneAsync(OpenWorld.openWorldSettingsScene, onSceneStartLoad, onOpenWorldMainSceneLoaded, LoadSceneMode.Single, false);

            return true;
        }

        bool isDirty = true;
        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            isDirty = true;
        }
        void OnSceneExit (List<string> activeScenes) {
            isDirty = true;
        }
        void OnSceneUnload (string unloadedScene) {
            isDirty = true;
        }


        List<WorldScene> loadedWorldScenes;
        HashSet<Vector2Int> loadedWorldGrids0;
        HashSet<Vector2Int> loadedWorldGrids1;

        void CheckLoadedScenes () {
            if (isDirty || loadedWorldScenes == null || loadedWorldGrids0 == null || loadedWorldGrids1 == null) {
                GetLoadedWorldScenes();
                isDirty = false;
            }
        }
        


        void GetLoadedWorldScenes () {
            
            if (loadedWorldScenes == null) loadedWorldScenes = new List< WorldScene>();
            if (loadedWorldGrids0 == null) loadedWorldGrids0 = new HashSet<Vector2Int>();
            if (loadedWorldGrids1 == null) loadedWorldGrids1 = new HashSet<Vector2Int>();

            loadedWorldScenes.Clear();
            loadedWorldGrids0.Clear();
            loadedWorldGrids1.Clear();
            
            for (int i = 0; i < SceneLoading.currentLoadedScenes.Count; i++) {
                string name = SceneLoading.currentLoadedScenes[i];
                if (name.Contains(OpenWorld.openWorldSceneKey)) {

                    bool isLOD0 = name.Contains(OpenWorld.lod0Check);
                    int lod = isLOD0 ? 0 : 1;
                    Vector2Int sceneGrid = settings.Scene2Grid(name, lod);

                    loadedWorldScenes.Add(new WorldScene(sceneGrid, SceneLoading.currentLoadedScenes[i], lod));
                    
                    if (isLOD0) {
                        loadedWorldGrids0.Add(sceneGrid);
                    }
                    else {
                        loadedWorldGrids1.Add(sceneGrid);
                    }
                }
            }
        }


        bool GetCurrentGridScene (Vector2Int grid, int lod, out string scene) {
            

            for (int i = 0; i < loadedWorldScenes.Count; i++) {
                if (grid == loadedWorldScenes[i].grid) {
                    if (lod == loadedWorldScenes[i].lod) {
                        scene = loadedWorldScenes[i].scene;
                        return true;
                    }
                }
            }
            scene = null;
            return false;
        }

        void LoadGridScene (Vector2Int grid, int lod) {
            string sceneName = settings.Grid2Scene (grid, lod);
            if (!string.IsNullOrEmpty(sceneName)) {
                if (!SceneLoading.IsSceneLoadingOrUnloading( sceneName )) {
                    SceneLoading.LoadSceneAsync( sceneName, null, null, LoadSceneMode.Additive, false);
                }
            }
        }


        void CheckForDuplicateSceneLoad () {
            int sceneCount = SceneManager.sceneCount;
            string[] names = new string[sceneCount];

            for (int i = 0; i < sceneCount; i++) {

                string name = i == 0 ? SceneManager.GetSceneAt(i).name : names[i];
                for (int j = i + 1; j < sceneCount; j++) {
                    string name2 = i == 0 ? SceneManager.GetSceneAt(j).name : names[j];

                    if (i == 0) names[j] = name2;
                    
                    if (name == name2) {
                        Debug.LogError("Duplicate Scene Loaded: " + name);
                        return;    
                    }
                }
            }
        }
            
        void Update()
        {
            if (GameManager.isPaused) return;
            if (GameManager.isInMainMenuScene) return;
            if (!GameManager.playerExists) return;
            if (!isInOpenWorldScene) return;

            Camera playerCamera = GameManager.playerCamera;
            if (playerCamera == null) return;

            #if UNITY_EDITOR
            CheckForDuplicateSceneLoad();
            #endif

            CheckLoadedScenes();

            Vector3 playerPosition = playerCamera.transform.position;
            Vector2Int playerCell = GridManagement.CalculateGrid(playerPosition, settings.cellSize);
            
            int gridResolution = settings.gridResolution;

            playerCell.x = Mathf.Clamp(playerCell.x, 0, gridResolution - 1);
            playerCell.y = Mathf.Clamp(playerCell.y, 0, gridResolution - 1);
            
            string gridScene;
            if (GetCurrentGridScene(playerCell, 1, out gridScene)) {
                SceneLoading.SetPlayerScene( gridScene );
            }
            else {
                // curernt grid scene is not loaded yet...
                LoadGridScene(playerCell, 0);
                LoadGridScene(playerCell, 1);
            }

            UpdateGrids (playerCell, gridResolution);
        }

        void UnloadNonNeighbors (Vector2Int playerGrid) {
            for (int i = 0; i < loadedWorldScenes.Count; i++) {
                WorldScene loadedScene = loadedWorldScenes[i];
                if (!SceneLoading.IsSceneLoadingOrUnloading(loadedScene.scene)) {
                    int dist = loadedScene.lod == 0 ? settings.lod0_distance : settings.lod1_distance;
                    if (GridManagement.GridDistance(loadedScene.grid, playerGrid) > dist) {
                        SceneLoading.UnloadSceneAsync (loadedScene.scene);  
                    }
                }
            }
        }

        void LoadNeighbors (Vector2Int playerGrid, int gridRes) {

            int maxDistance = settings.lod1_distance;
            for ( int y = -maxDistance; y <= maxDistance; y++ ) {
                for ( int x = -maxDistance; x <= maxDistance; x++ ) {
                    
                    if (y == 0 && x == 0)
                        continue;

                    Vector2Int n = new Vector2Int(playerGrid.x + x, playerGrid.y + y);
                    
                    if (n.y < 0 || n.y >= gridRes || n.x < 0 || n.x >= gridRes)
                        continue;

                    
                    if (!loadedWorldGrids1.Contains(n))
                        LoadGridScene(n, 1);
                    
                    int cellDistance = GridManagement.GridDistance(n, playerGrid);
                    if (cellDistance <= settings.lod0_distance) {
                        if (!loadedWorldGrids0.Contains(n))
                            LoadGridScene(n, 0);
                    }
                }
            }

        }


        void UpdateGrids (Vector2Int playerGrid, int gridRes) {
            UnloadNonNeighbors(playerGrid);
            LoadNeighbors (playerGrid, gridRes);
        }
    }
}
