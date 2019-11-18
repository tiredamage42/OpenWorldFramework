using System.Collections.Generic;
using UnityEngine;

using System.Reflection;

using UnityEngine.SceneManagement;
namespace UnityTools {

    public static class GameObjects 
    {
        public static void DontDestroyOnLoad (this GameObject g, bool checkIfRootObject) {
            
            if (checkIfRootObject) {
                if (g.transform.parent == null) {
                    dontDestroyOnLoadRootObjects.Add(g);
                }
            }
            MonoBehaviour.DontDestroyOnLoad(g);
        }
        static HashSet<GameObject> dontDestroyOnLoadRootObjects = new HashSet<GameObject>();

        public static T[] FindObjectsOfType<T>(bool checkDisabled) where T : Component
        {
            if (checkDisabled) {

                List<T> results = new List<T>();

                foreach (var go in dontDestroyOnLoadRootObjects) {
                    results.AddRange(go.GetComponentsInChildren<T>(true));
                }


                for(int i = 0; i< SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (s.isLoaded)
                    {
                        var allGameObjects = s.GetRootGameObjects();
                        for (int j = 0; j < allGameObjects.Length; j++)
                        {
                            var go = allGameObjects[j];
                            results.AddRange(go.GetComponentsInChildren<T>(true));
                        }
                    }
                }
                return results.ToArray();
            }
            else {
                return GameObject.FindObjectsOfType<T>();
            }
        }
        public static T FindObjectOfType<T>(bool checkDisabled) where T : Component
        {
            T r = GameObject.FindObjectOfType<T>();

            if (checkDisabled && r == null) {
                foreach (var go in dontDestroyOnLoadRootObjects) {
                    T c = go.GetComponentInChildren<T>(true);
                    if (c != null)
                        return c;        
                }

                for(int i = 0; i< SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (s.isLoaded)
                    {
                        
                        var allGameObjects = s.GetRootGameObjects();
                        for (int j = 0; j < allGameObjects.Length; j++)
                        {
                            var go = allGameObjects[j];
                            T c = go.GetComponentInChildren<T>(true);
                            if (c != null) return c;
                        }
                    }
                }
            }
            return r;
        }

        public static T[] GetComponents <T> (this GameObject g, bool checkDisabled) where T : Component {
            if (checkDisabled) {
                List<T> r = new List<T>();
                T[] found = g.GetComponentsInChildren<T>(true);
                for (int i= 0; i < found.Length; i++) {
                    if (found[i].gameObject == g) {
                        r.Add(found[i]);
                    }
                }
                return r.ToArray();
            }
            return g.GetComponents<T>();
        }

        public static T GetComponent<T> (this GameObject g, bool checkDisabled) where T : Component {
            if (checkDisabled) {
                T[] found = g.GetComponents<T>(true);
                if (found.Length > 0) {
                    return found[0];
                }
            }
            return g.GetComponent<T>();
        }


        public static T GetOrAddComponent<T> (this GameObject g, bool checkDisabled) where T : Component {
            T r = g.GetComponent<T>(checkDisabled);
            if (r == null) r = g.AddComponent<T>();
            return r;
        }
        public static T GetOrAddComponent<T> (this GameObject g, ref T variable, bool checkDisabled) where T : Component {
            if (variable == null) variable = g.GetOrAddComponent<T>(checkDisabled);
            return variable;
        }
        public static T GetComponentIfNull<T> (this GameObject g, ref T variable, bool checkDisabled) where T : Component {
            if (variable == null) variable = g.GetComponent<T>(checkDisabled);
            return variable;
        }
        public static T GetComponentInChildrenIfNull<T> (this GameObject g, ref T variable, bool checkDisabled) where T : Component {
            if (variable == null) variable = g.GetComponentInChildren<T>(checkDisabled);
            return variable;
        }

        public static T[] GetComponentsIfNull<T> (this GameObject g, ref T[] variable, bool checkDisabled) where T : Component {
            if (variable == null || variable.Length == 0) variable = g.GetComponents<T>(checkDisabled);
            return variable;
        }
        public static T[] GetComponentsInChildrenIfNull<T> (this GameObject g, ref T[] variable, bool checkDisabled) where T : Component {
            if (variable == null || variable.Length == 0) variable = g.GetComponentsInChildren<T>(checkDisabled);
            return variable;
        }    






        public static T[] GetComponents <T> (this Component g, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponents<T>(checkDisabled);
        }
        public static T GetComponent<T> (this Component g, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponent<T>(checkDisabled);
        }
        public static T GetOrAddComponent<T> (this Component g, bool checkDisabled) where T : Component {
            return g.gameObject.GetOrAddComponent<T>(checkDisabled);
        }
        public static T GetOrAddComponent<T> (this Component g, ref T variable, bool checkDisabled) where T : Component {
            return g.gameObject.GetOrAddComponent<T>(ref variable, checkDisabled);
        }
        public static T GetComponentIfNull<T> (this Component g, ref T variable, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponentIfNull<T>(ref variable, checkDisabled);
        }
        public static T GetComponentInChildrenIfNull<T> (this Component g, ref T variable, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponentInChildrenIfNull<T>(ref variable, checkDisabled);
        }
        public static T[] GetComponentsIfNull<T> (this Component g, ref T[] variable, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponentsIfNull<T>(ref variable, checkDisabled);
        }
        public static T[] GetComponentsInChildrenIfNull<T> (this Component g, ref T[] variable, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponentsInChildrenIfNull<T>(ref variable, checkDisabled);
        }



        public static T[] GetComponents <T> (this MonoBehaviour g, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponents<T>(checkDisabled);
        }
        public static T GetComponent<T> (this MonoBehaviour g, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponent<T>(checkDisabled);
        }
        public static T GetOrAddComponent<T> (this MonoBehaviour g, bool checkDisabled) where T : Component {
            return g.gameObject.GetOrAddComponent<T>(checkDisabled);
        }
        public static T GetOrAddComponent<T> (this MonoBehaviour g, ref T variable, bool checkDisabled) where T : Component {
            return g.gameObject.GetOrAddComponent<T>(ref variable, checkDisabled);
        }
        public static T GetComponentIfNull<T> (this MonoBehaviour g, ref T variable, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponentIfNull<T>(ref variable, checkDisabled);
        }
        public static T GetComponentInChildrenIfNull<T> (this MonoBehaviour g, ref T variable, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponentInChildrenIfNull<T>(ref variable, checkDisabled);
        }
        public static T[] GetComponentsIfNull<T> (this MonoBehaviour g, ref T[] variable, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponentsIfNull<T>(ref variable, checkDisabled);
        }
        public static T[] GetComponentsInChildrenIfNull<T> (this MonoBehaviour g, ref T[] variable, bool checkDisabled) where T : Component {
            return g.gameObject.GetComponentsInChildrenIfNull<T>(ref variable, checkDisabled);
        }    






 
    }
}
