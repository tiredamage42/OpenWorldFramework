

using UnityEditor;
using UnityEngine;

using UnityTools;
namespace OpenWorldFramework {
    [InitializeOnLoad] public class OpenWorldEditorUtils {
        static OpenWorldEditorUtils () {
            BuildSettings.AddBuildWindowIgnorePattern(OpenWorld.openWorldSceneKey);
            BuildSettings.AddBuildWindowIgnorePattern(OpenWorld.openWorldSettingsScene);
            
            OpenWorldSettingsEditor.UpdateSceneAssetNames();
            EditorApplication.projectChanged += OpenWorldSettingsEditor.UpdateSceneAssetNames;
        }
    }
}
