
using UnityEditor;
using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools.GameSettingsSystem.Internal {
    public class GameSettingsList
    {
        static bool _disableRefresh;
        public static bool disableRefresh {
            get {return _disableRefresh;}
            set {_disableRefresh = value;}
        }
        
        public static void RefreshGameSettingsList () {
    #if UNITY_EDITOR

            if (disableRefresh) return;
            // dont update when in play mode or if our game settings object is missing
            if (Application.isPlaying || GameSettings.settings == null) return;

            // update the array of all game settings objects in the project
            GameSettings.settings = AssetTools.FindAssetsByType<GameSettingsObject>(logToConsole: true).ToArray();
            EditorUtility.SetDirty(GameSettings.gameSettings);
    #endif
        }
        
    }
}

