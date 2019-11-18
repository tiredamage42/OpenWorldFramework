

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityTools.GameSettingsSystem;
using UnityTools.Internal;
using System;

using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;

using UnityTools.InitializationSceneWorkflow;

namespace UnityTools {

    // added at first initialization...
    public abstract class InitializationSingleTon<T> : Singleton<T> where T : MonoBehaviour {

    }

    public class GameManager : Singleton<GameManager>
    {
        /*
            start and awake should only happen during the initial scene load
        */
        protected override void Awake() {
            base.Awake();
            if (!thisInstanceErrored) {
                
                SceneLoading.onSceneLoadStart += OnSceneLoadStart;
                SaveLoad.onSaveGame += OnSaveGame;
                SceneManager.sceneLoaded += OnSceneLoaded;
                
                if (GameManagerSettings.instance.actionsController != null) 
                    GameManagerSettings.instance.actionsController.InitializeActionsInterface();
            
                // get all types of InitializationSingleTon's
                Type[] results = SystemTools.FindAllDerivedTypes(typeof(InitializationSingleTon<>));
                
                // add tehm to this gameObject
                for (int i = 0; i < results.Length; i++) {
                    gameObject.AddComponent(results[i]);
                }
                
            }
        }   

        const string playerSaveKey = "GAMEPLAYER";
        void OnSaveGame (List<string> allActiveLoadedScenes) {
            SaveLoad.gameSaveState.UpdateSaveState (playerSaveKey, new ActorState(Actor.playerActor));
        }

        void OnSceneLoaded (Scene scene, LoadSceneMode mode)
        {
            if (GameManager.isInMainMenuScene)
                return;
            if (!SaveLoad.isLoadingSaveSlot)
                return;
            
            ActorState savedPlayer = (ActorState)SaveLoad.gameSaveState.LoadSaveStateObject(playerSaveKey);
            SaveObject.OnObjectLoad(Actor.playerActor, Actor.playerActor, savedPlayer);
        }
        
        public bool showPause;

        void Update () {
            CheckPauseObjectsForNulls();
            showPause = isPaused;
        }
        
        public static int maxSaveSlots { get { return GameManagerSettings.instance.maxSaveSlots; } }
        public static bool isQuitting;

        void OnApplicationQuit () {
            isQuitting = true;
        }

        void Start () {
            if (!thisInstanceErrored) {
                SceneLoading.SetActiveScene(InitializationScenes.mainInitializationScene);

                SaveLoad.LoadSettingsOptions();

                // #if UNITY_EDITOR
                StartCoroutine(SkipToScene());
                // #endif

                // just to not get stuck in full screen mode...
                StartCoroutine(QuitDebug());
            }

        }
        IEnumerator QuitDebug() {
            yield return new WaitForSecondsRealtime(60);
            QuitApplication();
        }
            
        // #if UNITY_EDITOR
        IEnumerator SkipToScene() {
            yield return new WaitForSecondsRealtime(3);
            Debug.Log("Skippng to scene");
            GameManagerSettings.instance.editorSkipToSpawn.DoFastTravel();
        }
        // #endif


        public static event Action onLoadMainMenu, onExitMainMenu;

        static void OnSceneLoadStart (string targetScene, LoadSceneMode mode) {
            if (mode != LoadSceneMode.Additive) {

                if (targetScene == InitializationScenes.mainInitializationScene) {
                    
                    if (onLoadMainMenu != null)
                        onLoadMainMenu();
                
                    DestroyPlayer();
                }
                else {
                    BuildPlayer();

                    if (isInMainMenuScene) {
                        if (onExitMainMenu != null)
                            onExitMainMenu();
                    }
                } 
            }   
        }


         #region PAUSE_GAME
        public static bool isPaused { get { return pauseObjects.Count != 0; } }
        
        public static event Action<bool> onPause;
        static List<object> pauseObjects = new List<object>();

        static void CheckPauseObjectsForNulls () {

            bool wasPaused = pauseObjects.Count > 0;
            for (int i = pauseObjects.Count -1; i >= 0; i--) {
                if (pauseObjects[i] == null) {
                    pauseObjects.RemoveAt(i);
                }
            }
            if (wasPaused && pauseObjects.Count == 0) {
                if (onPause != null) 
                    onPause(false);
            }
        }


        static void DoPause (object pauseObject) {
            bool wasEmpty = pauseObjects.Count == 0;
            pauseObjects.Add(pauseObject);
            if (wasEmpty) {
                if (onPause != null) 
                    onPause(true);
            }
        }
        static void DoUnpause (object pauseObject) {
            pauseObjects.Remove(pauseObject);
            if (pauseObjects.Count == 0) {
                if (onPause != null) onPause(false);
            }
            else {
                for (int i =0 ; i < pauseObjects.Count; i++) {
                    Debug.Log(pauseObjects[i]);
                }
            }
        }

        public static void PauseGame (object pauseObject) {
            if (pauseObject == null) return;
            if (!pauseObjects.Contains(pauseObject)) {
                DoPause(pauseObject);
            }
        }

        public static void UnpauseGame (object pauseObject) {
            if (pauseObject == null) return;
            if (pauseObjects.Contains(pauseObject)) {
                DoUnpause(pauseObject);
            }
        }

        public static void TogglePase (object pauseObject) {
            if (pauseObject == null) return;
            if (pauseObjects.Contains(pauseObject)) 
                DoUnpause(pauseObject);
            else 
                DoPause(pauseObject);
        }

        #endregion




        #region PLAYER
        public static bool playerExists { get { return Actor.playerActor != null; } }
        public static Actor playerActor {
            get {
                if (!playerExists) Debug.LogError("Player Actor not instantiated!!!");
                return Actor.playerActor;
            }
        }
        public static Camera playerCamera { get { return PlayerCamera.myCamera; } }
        static void BuildPlayer () {
            if (!playerExists) {
                GameObject.Instantiate(GameManagerSettings.instance.playerPrefab);
            }
        }
        static void DestroyPlayer () {
            if (playerExists) {
                GameObject.Destroy(playerActor.gameObject);
            }
        }
        #endregion

        public static bool isInMainMenuScene { get { return SceneLoading.activeScene == InitializationScenes.mainInitializationScene; } }        
        
        public static void StartNewGame () {
            DestroyPlayer();
            SaveLoad.ClearGameSaveState();
            GameManagerSettings.instance.newGameSpawn.DoFastTravel();
        }

        public static void QuitToMainMenu () {
            SaveLoad.ClearGameSaveState();
            InitializationScenes.LoadInitializationScene();
        }

        // public static string[] GetAllScenes () {
        //     int sceneCount = SceneManager.sceneCountInBuildSettings;
        //     string[] allScenes = new string[sceneCount];
        //     for (int i = 0; i < sceneCount; i++)
        //     {
        //         string path = SceneUtility.GetScenePathByBuildIndex(i);
        //         string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        //         allScenes[i] = sceneName;
        //     }
        //     return allScenes;
        // }


        public static void QuitApplication () {
            SaveLoad.SaveSettingsOptions();

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit ();
    #endif
        }
    }   
}