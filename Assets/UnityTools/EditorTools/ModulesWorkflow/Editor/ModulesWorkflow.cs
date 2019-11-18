#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;


namespace UnityTools.EditorTools.Internal.Modules {
// namespace CustomEditorTools.Internal.Modules {

    /*
        helpers for modules workflow.

        wraps the package importing and creation of modules that will be updated in seperate projects

        makes it easier to keep things up to date
    */      


    /*
        importing:
            nothing special, just hardcoded the eventual export / import directory so
            I dont have to search for it in the finder window everytime.

            
        creating custom module:
            1) create Module in export root directory
                    Right-click -> Create -> Custom Editor Tools -> Module
                
            2) create ModuleProjectSpecifier, outside of that directory
                    Right-click -> Create -> Custom Editor Tools -> Module Project Specifier
            
            3) drag and drop the module into the module project speficier's module slot

            4) refresh the Modules window if open

            5) now you can use the export tab in the Modules window

            Note: if the project is a module project, we cant import that module
                
    */

    public static class ModuleWorkFlow {

        public const string defaultTargetExportDirectory = "/Users/Sydney/Desktop/MyUnityPackages/";
        
        public static void ExportModulePackage (Module module, string targetDirectory, bool isUpgrade) {
            
            // export base directory is the directory the module is in
            string exportDir = Path.GetDirectoryName( AssetDatabase.GetAssetPath(module) );

            // if the directory name for the module isnt a match with the module name
            // raise a warning, it might mean we're exporting the wrong module by accident
            string [] sp = exportDir.Split('/');
            string dirName = sp[sp.Length - 1];
            if (dirName != module.moduleName) {
                string msg = "Trying to export module: " + module.moduleName + ", but root directory for module is named: " + dirName + "\n\nAre you sure everything is set up right?";
                if (!EditorUtility.DisplayDialog("Module Export:\n" + module.moduleName, msg, "Continue", "Abort"))
                    return;
            }
            
            // when upgrading, we increment the version number, so other projects that use this module
            // know they need an update
            if (isUpgrade) {
                if (!EditorUtility.DisplayDialog("Module Upgrade:\n" + module.moduleName, "Are you sure you want to upgrade from v" + module.currentVersion + " to v"+(module.currentVersion+1), "Upgrade", "Abort"))
                    return;

                module.currentVersion++;
                
                // make sure this change gets saved before export 
                // TODO: check if this is actually needed
                EditorUtility.SetDirty(module);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                string oldPath = targetDirectory + module.moduleName + versionPrefix + (module.currentVersion-1) + ProjectTools.packageExtension;

                // Check if file exists with its full path    
                if (File.Exists(oldPath)) {
                    File.Delete(oldPath);    
                    Debug.Log("Deleted: " + oldPath);
                }       
            }

            
            // dont let the ModuleProjectSpecifier in the project be exported with the module
            Func<string, bool> whereNotProjectSpecifierAsset = (filePath) => {
                // if its not an asset
                if (!filePath.EndsWith(".asset"))
                    return true;
                
                bool isProjectSpecifier = AssetDatabase.LoadAssetAtPath<ModuleProjectSpecifier>(filePath) != null;
                if (isProjectSpecifier) {
                    Debug.LogWarning("Found ModuleProjectSpecifier in export files, removing: " + filePath);
                }
                return !isProjectSpecifier;
            };
            
            string [] files = ProjectTools.GetFilesInDirectory(exportDir).Where(whereNotProjectSpecifierAsset).ToArray();

            if (files.Length == 0) {
                Debug.LogWarning("No files to export at directory: " + exportDir);
                return;
            }
            
            // only one file... chances are it's just the module object.
            // in which case, dont export
            if (files.Length == 1) {
                bool isModule = AssetDatabase.LoadAssetAtPath<ModuleProjectSpecifier>(files[0]) != null;
                if (isModule) {
                    Debug.LogWarning("No files except module to export at directory: " + exportDir);
                    return;
                }
            }

            // print out files as debug
            // for (int i = 0; i < files.Length; i++) Debug.Log("Export File: " + files[i]);
            
            string exportPath = targetDirectory + module.moduleName + versionPrefix + module.currentVersion + ProjectTools.packageExtension;
            
            AssetDatabase.ExportPackage (files, exportPath);
            EditorUtility.DisplayDialog("Export Complete", "Module Exported To:\n" + exportPath, "Ok", "Ok");
        }

        const string versionPrefix = "_@";
        
        /*
            get the version number from the filepath for the .unitypackage module
            
            should be in format: 
                ModulePackageName_@2.unitypackage 
            
            where 2 is the version number
        */
        public static int VersionFromFilePath (string modulePackagePath, out string packageName) {
            packageName = null;
            if (!modulePackagePath.Contains(versionPrefix)) {
                Debug.Log("No versioning present for package: " + modulePackagePath);
                return -1;
            }

            string [] split = modulePackagePath.Split(new string[] { versionPrefix }, StringSplitOptions.None);
            
            packageName = split[0];
            
            string versionString = split[1];
            
            // take out the .unitypackage file extension if it's there
            if (versionString.EndsWith(ProjectTools.packageExtension)) {
                versionString = versionString.Substring(0, versionString.Length - ProjectTools.packageExtension.Length);
            }

            int version;
            if (int.TryParse(versionString, out version)) {
                return version;
            }
            Debug.LogError("Error reading module version at :: " + modulePackagePath);
            return -1;
        }
    }
}
#endif