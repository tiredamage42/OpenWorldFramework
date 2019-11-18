#if UNITY_EDITOR

using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace UnityTools.EditorTools {

    public static class AssetTools
    {

        public static T CreateScriptableObject <T> (string path, bool refreshAndSave=true) where T : ScriptableObject {
            T r = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(r, path + ".asset");
            
            if (refreshAndSave) 
                ProjectTools.RefreshAndSave();
            
            return r;
        }

        public static List<T> FindAssetsByType<T> (bool logToConsole) where T : Object {
            return FindAssetsByType(typeof(T), logToConsole).Cast<T>().ToList();
        }

        public static List<Object> FindAssetsByType (System.Type type, bool logToConsole)
        {
            if (logToConsole) Debug.Log("Searching project for assets of type: " + type.FullName);
            List<Object> assets = new List<Object>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", type.FullName));
            
            if (guids.Length == 0) {
                if (logToConsole) Debug.Log("None Found, searching project for assets of type: " + type.Name);
                guids = AssetDatabase.FindAssets(string.Format("t:{0}", type.Name));
            }
            
            for( int i = 0; i < guids.Length; i++ )
            {
                Object asset = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath( guids[i] ), type );
                if ( asset != null ) assets.Add(asset);
            }

            Object[] resourcesFound = Resources.FindObjectsOfTypeAll(type);

            for (int i = 0; i < resourcesFound.Length; i++) {
                if (!assets.Contains(resourcesFound[i])) {
                    assets.Add(resourcesFound[i]);

                    // if resources found it, but guids didnt, reimport the asset
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(resourcesFound[i]));
                }
            }

            if (logToConsole) Debug.Log("Found " + assets.Count + " assets in project");
            
            return assets;
        }
    }
}

#endif
