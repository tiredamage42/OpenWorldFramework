using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.SceneManagement;

namespace UnityTools {

    public class SceneLoading 
    {
        public static event Action<string, LoadSceneMode> onSceneLoadStart;
        public static event Action<string, float, LoadSceneMode> onSceneLoadUpdate;
        public static event Action<string, LoadSceneMode> onSceneLoadEnd;

        // called when we load a single scene, non additively, 
        // any scenes that are unloaded as a result are passed in as the list
        // as we "exit" them
        public static event Action<List<string>> onSceneExit;

        // called when we manually unload a scene
        public static event Action<string> onSceneUnload;
        
        
        
        // to keep track of game pause
        static SceneLoading sceneLoadingPauseObject = new SceneLoading();

        static void BroadcastSceneLoadStart (string loadScene, LoadSceneMode loadSceneMode) {
            if (loadSceneMode != LoadSceneMode.Additive)
                GameManager.PauseGame(sceneLoadingPauseObject);
            
            if (onSceneLoadStart != null) 
                onSceneLoadStart(loadScene, loadSceneMode);
        }   
    
        static void BroadcastSceneLoadEnd (string loadScene, LoadSceneMode loadSceneMode) {
            if (onSceneLoadEnd != null) 
                onSceneLoadEnd(loadScene, loadSceneMode);
            
            if (loadSceneMode != LoadSceneMode.Additive) {
                if (loadingScenesThatPause.Count == 0)
                    GameManager.UnpauseGame(sceneLoadingPauseObject);
            }
        }

        static void BroadcastSceneExits () {
            if (onSceneExit != null) 
                onSceneExit(currentLoadedScenes);
            currentLoadedScenes.Clear();
        }


        // caching the active scene name, because Scene.name was creating garbage when 
        // checking every frame....
        public static string activeScene;
        // public static Scene activeScene { get { return SceneManager.GetActiveScene(); } }
        public static void SetActiveScene (string scene) {
            SceneManager.SetActiveScene( SceneManager.GetSceneByName( scene ) );
            activeScene = scene;
        }

        // the scene the player is currently in, in case there's multiple loaded additively
        // setting the active scene changes lighting settings / fog, so this is a workaround
        // to have a different sort of "active scene"
        public static string playerScene;
        public static void SetPlayerScene ( string scene ) {
            playerScene = scene;
        }

        // all the currently loaded scenes
        public static List<string> currentLoadedScenes = new List<string>();


        static List<string> unloadingScenes = new List<string>();
        static List<string> loadingScenes = new List<string>();
        static List<string> loadingScenesThatPause = new List<string>();
        public static bool IsSceneLoadingOrUnloading (string scene) {
            return loadingScenes.Contains(scene) || unloadingScenes.Contains(scene);
        }    


        public static bool UnloadSceneAsync ( string scene ) {

            Scene sceneObj = SceneManager.GetSceneByName(scene);
            // check if the scene is even loaded....
            if (!sceneObj.IsValid()) {
                Debug.LogError("Scene Not Loaded... '" + scene + "'");
                currentLoadedScenes.Remove(scene);
                return true;
            }

            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneObj);
            if (operation != null) {
                
                unloadingScenes.Add(scene);
                currentLoadedScenes.Remove(scene);
                
                // call event
                if (onSceneUnload != null) 
                    onSceneUnload(scene);
                
                // have unity unload the scene
                UpdateManager.instance.StartCoroutine(_UnLoadSceneAsync(operation, scene));
                return true;
            }

            return false;
        }

        static IEnumerator _UnLoadSceneAsync(AsyncOperation operation, string scene) {
            yield return operation;
            unloadingScenes.Remove(scene);
        }

        

        // define an override, to have custom loading logic
        // return true if the override is used...
        static Func<string, Action<LoadSceneMode>, Action<string, LoadSceneMode>, LoadSceneMode, bool> sceneLoadOverride;
        public static void SetSceneLoadOverride (Func<string, Action<LoadSceneMode>, Action<string, LoadSceneMode>, LoadSceneMode, bool> sceneLoadOverride) {
            SceneLoading.sceneLoadOverride = sceneLoadOverride;
        }

        public static bool LoadSceneAsync (string scene, Action<LoadSceneMode> onSceneStartLoad, Action<string, LoadSceneMode> onSceneLoaded, LoadSceneMode loadSceneMode, bool useSingleLoadEffects) {
            // check if we should run our overridden logic, if any...
            if (sceneLoadOverride != null) {
                if (sceneLoadOverride(scene, onSceneStartLoad, onSceneLoaded, loadSceneMode)) {
                    return true;
                }
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(scene, loadSceneMode);
            if (operation != null) {

                LoadSceneMode fakedMode = useSingleLoadEffects ? LoadSceneMode.Single : loadSceneMode;

                // add to currently loading scenes
                loadingScenes.Add(scene);
                
                // check if loading should pause the game...
                if (fakedMode == LoadSceneMode.Single) 
                    loadingScenesThatPause.Add(scene);

                
                BroadcastSceneLoadStart(scene, fakedMode);

                // if we're not loading additively "exit" all currently loaded scenes
                if (loadSceneMode == LoadSceneMode.Single) {
                    BroadcastSceneExits();
                }
                
                if (onSceneStartLoad != null) 
                    onSceneStartLoad(fakedMode);
            
                UpdateManager.instance.StartCoroutine(_LoadSceneAsync(operation, scene, onSceneLoaded, loadSceneMode, useSingleLoadEffects));
                return true;
            }
            
            return false;
        }

        static IEnumerator _LoadSceneAsync(AsyncOperation operation, string scene, Action<string, LoadSceneMode> onSceneLoaded, LoadSceneMode loadSceneMode, bool useSingleLoadEffects)
        {
            LoadSceneMode fakedMode = useSingleLoadEffects ? LoadSceneMode.Single : loadSceneMode;

            // Debug.Log("Loading from scene: " + scene);
            operation.allowSceneActivation = false;
            float progress = 0;
            while (progress < 1f)
            {
                progress = Mathf.Clamp01(operation.progress / 0.9f);
                if (onSceneLoadUpdate != null) 
                    onSceneLoadUpdate(scene, progress, fakedMode);

                yield return null;
            }

            currentLoadedScenes.Add(scene);
            
            operation.allowSceneActivation = true;
            // let the scene activate for frame

            // SceneManager.onSceneLoaded gets called here...
            yield return null;
            
            // now scene is considered "loaded"

            if (loadSceneMode == LoadSceneMode.Single)
                SetActiveScene(scene);
                
            if (fakedMode == LoadSceneMode.Single)
                SetPlayerScene(scene);
     
            loadingScenes.Remove(scene);
            loadingScenesThatPause.Remove(scene);
            BroadcastSceneLoadEnd(scene, fakedMode);
            
            if (onSceneLoaded != null) 
                onSceneLoaded(scene, fakedMode);
        }
    }
}
