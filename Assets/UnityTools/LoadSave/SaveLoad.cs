using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using UnityEngine.SceneManagement;
          
namespace UnityTools {

    [System.Serializable] public class GameSaveStateInfo {
        public string sceneName;
        public string dateTimeSaved;
        // maybe a screenshot ?

        public GameSaveStateInfo(string sceneName) {
            this.sceneName = sceneName;
            dateTimeSaved = System.DateTime.UtcNow.ToString("HH:mm dd MMMM, yyyy");
        }
        public override string ToString() {
            return sceneName + "\n" + dateTimeSaved;
        }
    }

    public class SaveState {

        public Dictionary<string, object> saveState = new Dictionary<string, object>();
        public void Reinitialize (Dictionary<string, object> saveState) {
            Clear();
            this.saveState = saveState;
        }
        public void Clear () {
            saveState.Clear();
        }

        public void UpdateSaveState (string key, object savedObject) {
            saveState[key] = savedObject;
        }
        public bool SaveStateContainsKey (string key) {
            return saveState.ContainsKey(key);
        }
        public object LoadSaveStateObject(string key) {
            object o;
            if (saveState.TryGetValue(key, out o)) return o;
            return null;
        }        
    }
        
    public class SaveLoad 
    {
        public static SaveState gameSaveState = new SaveState();
        public static SaveState settingsSaveState = new SaveState();
        

        public static event Action<List<string>> onSaveGame;
        public static bool isLoadingSaveSlot;
        
        static string GetGameStatePath (int slot, string extension) {
            return Application.persistentDataPath + "/SaveSate" + slot.ToString() + "." + extension;
        }
        static string GetGameSavePath (int slot) { return GetGameStatePath(slot, "save"); }
        static string GetGameSaveInfoPath (int slot) { return GetGameStatePath(slot, "info"); }
        static string GetGameSettingsOptionsPath () {
            return Application.persistentDataPath + "/GameSettingsOptions.save";
        }

        // call when going to main menu, or starting new game
        public static void ClearGameSaveState () {
            gameSaveState.Clear();
        }

        public static bool SaveExists (int slot) {
            return File.Exists(GetGameSavePath(slot));
        }

        public static void SaveGameState (int slot) {

            if (GameManager.isInMainMenuScene) {
                Debug.LogWarning("Cant save in main menu scene");
                return;
            }
            Debug.Log("Saving game");

            

            // TODO: pass in list of all currently loaded scenes

            // Scene[] allActiveLoadedScenes = SceneLoading.allCurrentLoadedScenes;
            // let everyone know we're saving
            if (onSaveGame != null) onSaveGame(SceneLoading.currentLoadedScenes);

            // TODO: make this the scene player is currently part of
            // keep track of the scene we were in when saving
            string playerScene = SceneLoading.playerScene;
            
            SystemTools.SaveToFile(new GameSaveStateInfo(playerScene), GetGameSaveInfoPath(slot));
            SystemTools.SaveToFile(gameSaveState.saveState, GetGameSavePath(slot));
        }

        public static GameSaveStateInfo GetSaveDescription (int slot) {
            if (!SaveExists(slot))
                return null;
            return (GameSaveStateInfo)SystemTools.LoadFromFile(GetGameSaveInfoPath(slot));
        }

        public static void LoadGameState (int slot) {

            string savePath = GetGameSavePath(slot);

            if (!File.Exists(savePath)) {
                Debug.LogError("No Save File Found For Slot " + slot.ToString());
                return;
            }

            Debug.Log("Starting Load");
            
            isLoadingSaveSlot = true;

            GameSaveStateInfo savedStateInfo = GetSaveDescription(slot);

            string sceneFromSave = savedStateInfo.sceneName;
            
            // load the actual save state
            Action<LoadSceneMode> onSceneStartLoad = (mode) => gameSaveState.Reinitialize( (Dictionary<string, object>)SystemTools.LoadFromFile(savePath) );
            
            Action<string, LoadSceneMode> onSceneLoaded = (s, mode) => isLoadingSaveSlot = false;
            
            if (!SceneLoading.LoadSceneAsync(sceneFromSave, onSceneStartLoad, onSceneLoaded, LoadSceneMode.Single, false)) {
                isLoadingSaveSlot = false;
            }
        }

        public static event Action onSettingsOptionsLoaded, onSaveSettingsOptions;

        // call when starting up game
        public static void LoadSettingsOptions () {
            string savePath = GetGameSettingsOptionsPath();
            if (!File.Exists(savePath)) return;
            Debug.Log("Starting Settings Load");
            settingsSaveState.Reinitialize( (Dictionary<string, object>)SystemTools.LoadFromFile(savePath) );
            if (onSettingsOptionsLoaded != null) onSettingsOptionsLoaded();
        }
        // call when we're done editng any settings, or when we're quittin ghte application
        public static void SaveSettingsOptions () {
            Debug.Log("Saving Settings");
            // let everyone know we're saving settings
            if (onSaveSettingsOptions != null) onSaveSettingsOptions();
            SystemTools.SaveToFile(settingsSaveState.saveState, GetGameSettingsOptionsPath());
        }
    }
}
