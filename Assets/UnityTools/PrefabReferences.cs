using UnityEngine;

using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;
namespace UnityTools {
    /*
        used for referencing prefabs by name, for serialization purposes
    */
    [CreateAssetMenu(menuName="Unity Tools/Prefab References Object")]
    public class PrefabReferences : GameSettingsObject
    {
        [NeatArray] public NeatGameObjectArray prefabs;
        
        public T GetPrefabReference<T> (string name) where T : Component {
            GameObject g = GetPrefabReference(name);
            if (g != null) return g.GetComponent<T>();
            return null;
        }

        public GameObject GetPrefabReference (string name) {
            for (int i = 0; i < prefabs.Length; i++) {
                if (prefabs[i].name == name) {
                    return prefabs[i];
                }
            }
            Debug.LogError("Cant find prefab name: " + name);
            return null;
        }        

        public static T GetPrefabReference<T> (string referencesObjectName, string prefabName) where T : Component {
            PrefabReferences referencesObject = GameSettings.GetSettings<PrefabReferences>(referencesObjectName);
            if (referencesObject != null) return referencesObject.GetPrefabReference<T>(prefabName);
            return null;
        }
        public static GameObject GetPrefabReference (string referencesObjectName, string prefabName) {
            PrefabReferences referencesObject = GameSettings.GetSettings<PrefabReferences>(referencesObjectName);
            if (referencesObject != null) return referencesObject.GetPrefabReference(prefabName);
            return null;
        }
    }
}
