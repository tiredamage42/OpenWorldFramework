using UnityEditor;

namespace UnityTools.GameSettingsSystem.Internal {
    /*
        update the game settings holder every time the project changes, to keep track of all the
        game settings objects in the project, the holder is located in a Resources folder, so we dont
        have to worry about having references to any of our game settings objects during builds
    */
    [InitializeOnLoad] public class GameSettingsEditor {
        static GameSettingsEditor () {
            
            GameSettingsList.RefreshGameSettingsList();

            EditorApplication.projectChanged += GameSettingsList.RefreshGameSettingsList;
        }
    }
}