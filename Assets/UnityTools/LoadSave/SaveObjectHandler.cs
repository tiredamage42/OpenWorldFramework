using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

using System.Reflection;

/*
    framework for saving the state of specified components on a per scene basis

    e.g. where npcs were when going to a different scene
    or where certain objects where dropped / if they were picked up


    the system should use pooling, which instantiates whatever copies it needs,
    so if any attached components need to save their state, they should
    implement the 
        ISaveableAttachment interface

    e.g. and inventory component on npcs

    then when loading or saving the base object (npc component), it calls the load or save functionality 
    for all ISaveableAttachment components

    this way inventory loads dont try to instantiate copies that are already 
    instantiated by npc loading...
*/
    
namespace UnityTools {
    
    /*
        base for the saved representation of the objects to save
    */
    [Serializable] public abstract class SaveObjectState {

        // the saved state of all attached components that needs to be saved/loaded
        public SaveAttachmentState[] attachmentStates;
        public sVector3 position;
        public sQuaternion rotation;
        public string prefabName, prefabObjectName;

        public SaveObjectState(Poolable c) {
            this.prefabName = c.basePrefabName;
            this.prefabObjectName = c.PrefabObjectName();
            
            // get all saveable information from attached scripts, 
            // maybe a scene item has an inventory or otehr ocmponents thhat need to save their states
            ISaveAttachment[] saveables = c.GetComponents<ISaveAttachment>();
            
            attachmentStates = new SaveAttachmentState[saveables.Length];
            for (int x = 0; x < saveables.Length; x++) {
                attachmentStates[x] = new SaveAttachmentState(saveables[x].AttachmentType(), saveables[x].OnSaved());
            }
        
            this.position = c.transform.position;
            this.rotation = c.transform.rotation;
        }
    }

    [Serializable] public class SaveAttachmentState {
        public Type type;
        public object state;
        public SaveAttachmentState (Type type, object state) {
            this.type = type;
            this.state = state;
        }
    }


    public interface ISaveAttachment {
        Type AttachmentType ();
        object OnSaved ();
        void OnLoaded (object savedAttachmentInfo);
    }

    public interface ISaveObject<S> where S : SaveObjectState { 
        void LoadFromSavedObject(S savedObject); 
    }


    public static class SaveObject {

        static Func<string, List<MonoBehaviour>, List<MonoBehaviour>> filterSaveObjectsForScene;

        public static void SetSaveObjectsSceneFilter (Func<string, List<MonoBehaviour>, List<MonoBehaviour>> filterSaveObjectsForScene) {
            SaveObject.filterSaveObjectsForScene = filterSaveObjectsForScene;
        }

        public static List<C> FilterSaveObjectsForScene<C> (string scene, List<C> objects) where C : MonoBehaviour {
            if (filterSaveObjectsForScene == null) 
                return objects;
            
            return filterSaveObjectsForScene(scene, objects.Cast<MonoBehaviour>().ToList()).Cast<C>().ToList();
        }

        public static void OnObjectLoad<C, S> (C loadedObject, ISaveObject<S> loadedObjectAsSaveableObject, S savedObject) 
            where C : Poolable<C>
            where S : SaveObjectState
        {

            loadedObject.transform.WarpTo(savedObject.position, savedObject.rotation);
            loadedObjectAsSaveableObject.LoadFromSavedObject(savedObject);

            // load all saveable information to attached scripts, 
            // maybe a scene item has an inventory or otehr ocmponents thhat need to save their states
            ISaveAttachment[] saveables = loadedObject.GetComponents<ISaveAttachment>();
            for (int i = 0; i < saveables.Length; i++) {
                
                Type saveableType = saveables[i].AttachmentType();
                for (int x = 0; x < savedObject.attachmentStates.Length; x++) {
                    if (saveableType == savedObject.attachmentStates[x].type) {
                        saveables[i].OnLoaded(savedObject.attachmentStates[x].state);
                        break;
                    }
                }
            }
        }
    }

    /*
        handle saving of all objects in a scene of type
    */
    // C = component targeted
    // S = saved component type
    // T = parent type (for singleton)
    public abstract class SaveObjectHandler<C, S, T> : InitializationSingleTon<T> 
        where C : Poolable<C>
        where S : SaveObjectState
        where T : MonoBehaviour 
    {

        string objectKey;

        // disabling souldnt be happening, since these saver loader objects are 
        // persistent throughout the game
        protected override void Awake() {
            base.Awake();
            if (!thisInstanceErrored) {
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneLoading.onSceneExit += OnSceneExit;
                SceneLoading.onSceneUnload += OnSceneUnload;
                SaveLoad.onSaveGame += OnSaveGame;


                objectKey = typeof(C).FullName;


                BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

                getAllActiveObjectsInSceneMethod = typeof(C).GetMethod("GetActiveAndAvailableInstances", flags);
                getAllActiveDefaultObjectsInSceneLoadMethod = typeof(C).GetMethod("GetInstancesNotInPool", flags);
                getPoolProperty = typeof(C).GetProperty("pool", flags);
            }
        }

        MethodInfo getAllActiveObjectsInSceneMethod;
        MethodInfo getAllActiveDefaultObjectsInSceneLoadMethod;
        PropertyInfo getPoolProperty;

        List<C> GetAllActiveObjectsInScene(string scene) {
            return (List<C>)getAllActiveObjectsInSceneMethod.Invoke(null, null);
        }
        List<C> GetAllActiveDefaultObjectsInSceneLoad(string scene) {
            return (List<C>)getAllActiveDefaultObjectsInSceneLoadMethod.Invoke(null, null);
        }
        PrefabPool<C> PrefabPool () {
            return (PrefabPool<C>)getPoolProperty.GetValue(null, null);
        }
    
        void SaveObjectsInScene (string scene, bool manualSave) {

            // get all the active objects in the current scene
            List<C> activeObjects = SaveObject.FilterSaveObjectsForScene(scene, GetAllActiveObjectsInScene(scene));
        
            if (activeObjects.Count > 0) {

                // teh list of saved objects to populate
                List<S> savedObjects = new List<S>();
                
                for (int i = 0; i < activeObjects.Count; i++) {
                    savedObjects.Add((S)Activator.CreateInstance(typeof(S), new object[] { activeObjects[i] }));    
                }
                
                // give to pool again (if disabling scene)
                if (!manualSave) {
                    for (int i = 0; i < activeObjects.Count; i++) {
                        activeObjects[i].gameObject.SetActive(false);
                    }
                }

                // save the state by scene
                SaveLoad.gameSaveState.UpdateSaveState (SceneKey(scene, objectKey), savedObjects);
            }
        }

        //TODO: when saving, save all active loaded scnenes
        void OnSaveGame (List<string> allActiveLoadedScenes) {
            for (int i = 0; i < allActiveLoadedScenes.Count; i++)
                SaveObjectsInScene ( allActiveLoadedScenes[i], true );
        }

        void OnSceneExit (List<string> allActiveLoadedScenes) {

            if (GameManager.isInMainMenuScene) return;
            // dont save if we're exiting scene from manual loading another scene
            if (SaveLoad.isLoadingSaveSlot) return;

            // save the objects in this scene if we're going to another one,
            // e.g we're going to an indoor area that's a different scene, then save the objects "outdoors"
            
            for (int i = 0; i < allActiveLoadedScenes.Count; i++)
                SaveObjectsInScene ( allActiveLoadedScenes[i], false );
        }
            
        void OnSceneUnload (string unloadedScene) {

            if (GameManager.isInMainMenuScene) return;
            // dont save if we're exiting scene from manual loading another scene
            if (SaveLoad.isLoadingSaveSlot) return;

            // save the objects in this scene if we're unloading it,
            // e.g we're out of range of an open world "cell"
            SaveObjectsInScene ( unloadedScene, false );
        }

        static string SceneKey (string scene, string suffix) {
            return scene + "." + suffix;
        }

        void OnSceneLoaded (Scene scene, LoadSceneMode mode)
        {

            if (GameManager.isInMainMenuScene)
                return;

            string sceneName = scene.name;
            
            string sceneKey = SceneKey(sceneName, objectKey);

            bool sceneHasInfoForSaveObjects = SaveLoad.gameSaveState.SaveStateContainsKey(sceneKey);
            
            // get all the active objects that are default in the current scene
            List<C> activeObjects = SaveObject.FilterSaveObjectsForScene(sceneName, GetAllActiveDefaultObjectsInSceneLoad(sceneName));
            
            for (int i = 0; i < activeObjects.Count; i++) {
                activeObjects[i].AddInstanceToPool(sceneHasInfoForSaveObjects && activeObjects[i].IsAvailable());
            }

            // if this scene has saved info for the objects, then load the objects
            if (sceneHasInfoForSaveObjects) {
                // laod the scene objects states that were saved for this scene
                List<S> savedObjects = (List<S>)SaveLoad.gameSaveState.LoadSaveStateObject(sceneKey);
        
                for (int i = 0; i < savedObjects.Count; i++) {
                    C loadedObject = PrefabPool().GetAvailable(PrefabReferences.GetPrefabReference<C>(savedObjects[i].prefabObjectName, savedObjects[i].prefabName), null);

                    SaveObject.OnObjectLoad(loadedObject, loadedObject as ISaveObject<S>, savedObjects[i]);
                }
            }
        }
    }
}