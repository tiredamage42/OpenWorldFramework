

using UnityEditor;
using UnityEngine;
using UnityTools.GameSettingsSystem;
using UnityEngine.SceneManagement;

namespace UnityTools.InitializationSceneWorkflow {


    [CreateAssetMenu(menuName="Initialization Scenes Workflow/Initialization Scenes Collection", fileName="InitializationScenes")]
    public class InitializationScenes : GameSettingsObjectSingleton<InitializationScenes>
    {
        
#if UNITY_EDITOR
        const string cEditorPrefLoadInitialOnPlay = "InitialSceneWorkflow.LoadInitialOnPlay";
        public static bool LoadInitialOnPlay {
            get { return EditorPrefs.GetBool(cEditorPrefLoadInitialOnPlay, false); }
            set { EditorPrefs.SetBool(cEditorPrefLoadInitialOnPlay, value); }
        }
        static bool _bypassInitializationSceneLoad;
        // set to true to play from current open editor scenes / custom scenes
        public static bool bypassInitializationSceneLoad {
            get {return _bypassInitializationSceneLoad;}
            set {_bypassInitializationSceneLoad = value;}
        }
        public string mainInitializationScenePath;
        public string[] initializationScenePaths;
#endif

        public override bool ShowInGameSettingsWindow() {
            return false;
        }
        public static void LoadInitializationScene () {
            SceneLoading.LoadSceneAsync (mainInitializationScene, null, OnInitialSceneLoaded, LoadSceneMode.Single, false);
        }
        static void OnInitialSceneLoaded (string scene, LoadSceneMode mode) {
            if (instance != null) {
                for (int i = 0; i < instance.initializationSceneNames.Length; i++) {
                    SceneLoading.LoadSceneAsync (instance.initializationSceneNames[i], null, null, LoadSceneMode.Additive, false);
                }
            }
        }
        public string[] initializationSceneNames;
        public const string mainInitializationScene = "_MainMenuScene";
        public const string initializationSceneKey = "@@";

    }
}

