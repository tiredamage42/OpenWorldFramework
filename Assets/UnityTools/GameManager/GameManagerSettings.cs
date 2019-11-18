

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;
using UnityTools.InitializationSceneWorkflow;

namespace UnityTools.Internal {
    
    // [CreateAssetMenu(menuName="Unity Tools/Internal/Game Manager Settings", fileName="UnityTools_GameManagerSettings")]
    public class GameManagerSettings : GameSettingsObjectSingleton<GameManagerSettings> {
        public FastTravelComponenet newGameSpawn;

        // #if UNITY_EDITOR
        [Tooltip("Use To Skip To A Certain Spawn Automatically After Starting Initial Scene in Editor")]
        public FastTravelComponenet editorSkipToSpawn;
        // #endif

        public GameObject playerPrefab;
        public int maxSaveSlots = 6;
        public ActionsInterfaceController actionsController;
    }



    #if UNITY_EDITOR
    [CustomEditor(typeof(GameManagerSettings))]
    public class GameManagerSettingsEditor : Editor {


        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            GUITools.Space(3);
            bool playInitialSceneFirst = InitializationScenes.LoadInitialOnPlay;
            GUI.backgroundColor = playInitialSceneFirst ? GUITools.blue : GUITools.white;
            if (GUILayout.Button("Play Initial Scene First In Editor")) {
                InitializationScenes.LoadInitialOnPlay = !playInitialSceneFirst;
            }
            GUI.backgroundColor = GUITools.white;

            if (GUILayout.Button("Open Master Scene In Editor")) {
                EditorSceneManager.OpenScene(InitializationScenes.instance.mainInitializationScenePath);
            }

            GUITools.Space();
            
        }
    }


    #endif
}