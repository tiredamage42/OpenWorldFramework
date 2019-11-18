// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace UnityTools.EditorTools.Internal.Modules {
// namespace CustomEditorTools.Internal.Modules.Editor {

    public class ImportModules
    {
        string[] unityPackageNames;
        string[] unityPackagePaths;
        int chosenPackage;
        string importWarnings;
        List<Module> modulesInProject;

        int excludeIndex;
        public void InitializeModuleImporting (string excludeModuleName) {
            excludeIndex = -1;
            modulesInProject = AssetTools.FindAssetsByType<Module>(false);
            
            unityPackagePaths = ProjectTools.GetFilesInDirectory(ModuleWorkFlow.defaultTargetExportDirectory, ProjectTools.packageExtension, out unityPackageNames);

                
            int l = unityPackagePaths.Length;
            
            importWarnings = "";
                
            for (int i = 0; i < l; i++) {

                string unVersionedName;
                int unimportedVersion = ModuleWorkFlow.VersionFromFilePath(unityPackageNames[i], out unVersionedName);
                
                if (unVersionedName != null) {
                    unityPackageNames[i] = unVersionedName;
                }

                if (unityPackageNames[i] == excludeModuleName){
                    excludeIndex = i;
                }
                
                
                // check for modules alraedy imported...
                for (int j = 0; j < modulesInProject.Count; j++) {
                    Module importedModule = modulesInProject[j];
                    
                    // if package in project already
                    if (unityPackageNames[i] == importedModule.moduleName) {
                        // check if our package version is higher than our already imported version
                        if (unimportedVersion > importedModule.currentVersion) {
                            importWarnings += "\n" + unityPackageNames[i] + " Upgrade available! Version: " + unimportedVersion + ", Current: "  + importedModule.currentVersion;
                            
                            // mark as already imported but updateable
                            unityPackageNames[i] = "[...] " + unityPackageNames[i];
                        }
                        else {
                            // mark as already imported
                            unityPackageNames[i] = "[ X ] " + unityPackageNames[i];
                        }
                        break;
                    }
                }
                // add version to name as readable
                unityPackageNames[i] = unityPackageNames[i] + " [v" + unimportedVersion + "]";

            }
        }

        public bool OnGUI (Module projectModule) {
            bool reInitialize = false;
            EditorGUILayout.LabelField("Choose Module", GUITools.boldLabel);
            
            chosenPackage = EditorGUILayout.Popup(chosenPackage, unityPackageNames);

            // show import warnings
            if (!string.IsNullOrEmpty(importWarnings)) {
                GUITools.Space();
                EditorGUILayout.HelpBox(importWarnings, MessageType.Info);
            }

            GUITools.Space();
            
            
            // dont allow import of project specifier module
            GUI.enabled = chosenPackage != excludeIndex;
            // import the chosen module package
            if (GUILayout.Button("Import")) {
                Debug.Log("Importing package " + unityPackageNames[chosenPackage] + " from path: " + unityPackagePaths[chosenPackage]);
                AssetDatabase.ImportPackage(unityPackagePaths[chosenPackage], true);
                reInitialize = true;
            }
            GUI.enabled = true;

            GUITools.Space(2);
            
            // show all the currently imported modules
            DoubleLabel("Modules In Project:", "Version:", GUITools.boldLabel, GUITools.white);
            for (int i = 0; i < modulesInProject.Count; i++) {
                DoubleLabel(modulesInProject[i].moduleName, "v" + modulesInProject[i].currentVersion, GUITools.label, modulesInProject[i] == projectModule ? GUITools.blue : GUITools.white);
            }   

            return reInitialize;
        }

        void DoubleLabel (string lbl0, string lbl1, GUIStyle s, Color32 color) {
            GUI.color = color;
            EditorGUILayout.BeginHorizontal(GUITools.toolbarButton);
            GUI.color = Color.white;
            EditorGUILayout.LabelField(lbl0, s);
            EditorGUILayout.LabelField(lbl1, s);
            EditorGUILayout.EndHorizontal();
        }

    }
}
