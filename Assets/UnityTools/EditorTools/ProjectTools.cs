#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

namespace UnityTools.EditorTools {


    public class ProjectTools
    {
        public static void CopyStringToClipBoard (string txt) {
            EditorGUIUtility.systemCopyBuffer = txt;
        }
            
				
        // keep our module menu items together
        public const int defaultMenuItemPriority = 300; 
        public const string defaultMenuItemSection = "GameObject/CustomModules/";
        
        public static string projectName { 
            get {
                string[] s = Application.dataPath.Split('/');
                string projectName = s[s.Length - 2];
                return projectName;
            }
        }

        public static void RefreshAndSave () {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        public static string [] GetFilesInDirectory (string directory) {
            string [] files = Directory.GetFiles (directory, "*.*", SearchOption.AllDirectories);
            return files.Where(file => !file.EndsWith(".meta")).ToArray();
        }       


        public const string packageExtension = ".unitypackage";

        /*
            returns the file paths in the directory
            and the file names without extension or base directory
        */
        public static string[] GetFilesInDirectory (string directory, string extension, out string[] fileNames) {
            string [] files = Directory.GetFiles (directory, "*" + extension, SearchOption.AllDirectories);
            int filesCount = files.Length;
            
            int directoryLength = directory.Length;
            int totalRemove = extension.Length + directoryLength;
            fileNames = new string[filesCount];
            
            for (int i = 0; i < filesCount; i++) 
                fileNames[i] = files[i].Substring(directoryLength, files[i].Length - totalRemove);
            
            return files;
        }    
        
        // Create a layer at the next available index. Returns silently if layer already exists.
        public static void CreateLayer(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "New layer name string is either null or empty.");

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layerProps = tagManager.FindProperty("layers");
            var propCount = layerProps.arraySize;

            UnityEditor.SerializedProperty firstEmptyProp = null;

            for (var i = 0; i < propCount; i++)
            {
                var layerProp = layerProps.GetArrayElementAtIndex(i);
                var stringValue = layerProp.stringValue;

                if (stringValue == name) return;

                if (i < 8 || stringValue != string.Empty) 
                    continue;

                if (firstEmptyProp == null) {
                    firstEmptyProp = layerProp;
                    break;
                }
            }

            if (firstEmptyProp == null)
            {
                Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + name + "\" not created.");
                return;
            }
            Debug.Log("Layer \"" + name + "\" created.");
            
            firstEmptyProp.stringValue = name;
            tagManager.ApplyModifiedProperties();
        }
    }
}
#endif