using System.Collections.Generic;
using UnityEngine;
using UnityEditor;




namespace UnityTools.EditorTools.Internal.Modules {
// namespace CustomEditorTools.Internal.Modules.Editor {


    public class ExportModules {
        string exportWarnings;
        const string defaultSpecifierPath = "Assets/ModuleProjectSpecifier";

        ModuleProjectSpecifier projectSpecifier;
        public Module projectSpecifierModule {
            get {
                if (projectSpecifier != null)
                    return projectSpecifier.workingModule;
                return null;
            }
        }
        public string projectSpecifierModuleName {
            get {
                if (projectSpecifier != null && projectSpecifier.workingModule != null)
                    return projectSpecifier.workingModule.moduleName;
                return null;
            }
        }
        string moduleProjectSpecifierHelp = @"
Keep the ModuleProjectSpecifier out of the module's directory!
Accidentally exporting could lead to errors in other projects that edit other modules
        ";
        string noModuleProjectSpecifierHelp = @"
If the project is for editing and exporting a module,
create an instance of a ModuleProjectSpecifier object in the project 
and specify the module.

In the project view:
    Right-click -> Custom Editor Tools -> Module Project Specifier

Keep the ModuleProjectSpecifier out of the module's directory!
Accidentally exporting could lead to errors in other projects that edit other modules
        ";
        

        /*
            check and see if this project is a module project
        */
        public void InitializeModuleExporting () {
            projectSpecifier = null;
            exportWarnings = "";

            List<ModuleProjectSpecifier> specifiers = AssetTools.FindAssetsByType<ModuleProjectSpecifier>(false);
            if (specifiers.Count == 0)
                return;
            
            projectSpecifier = specifiers[0];

            // show warning if we have more than one in the project...
            if (specifiers.Count > 1) {
                exportWarnings = "Found more than one ModuleProjectSpecifier in the project, using the first one located at path: " + AssetDatabase.GetAssetPath(projectSpecifier);
                Debug.LogWarning(exportWarnings);
            }
        }

        
        bool DoExport (bool upgrade) {
            if (projectSpecifier.workingModule == null) {
                Debug.LogWarning("ModuleProjectSpecifier: working module is null, at path: " + AssetDatabase.GetAssetPath(projectSpecifier));
                return false;
            }

            ModuleWorkFlow.ExportModulePackage(projectSpecifier.workingModule, ModuleWorkFlow.defaultTargetExportDirectory, upgrade);
            return true;
        }
        
        
        public bool OnGUI () {
            bool reInitialize = false;

            EditorGUILayout.HelpBox(projectSpecifier != null ? string.Format(moduleProjectSpecifierHelp, projectSpecifierModuleName) : noModuleProjectSpecifierHelp, MessageType.Info);
            GUITools.Space();
                
            if (projectSpecifier != null) {

                EditorGUILayout.LabelField("Current Module:", GUITools.boldLabel);
                EditorGUILayout.LabelField(projectSpecifierModuleName);
                
                if (!string.IsNullOrEmpty( exportWarnings ) ) {
                    EditorGUILayout.HelpBox(exportWarnings, MessageType.Warning);
                    GUITools.Space();
                }

                GUITools.Space();

                // show export options
                EditorGUILayout.LabelField("Exporting:", GUITools.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Export")) {
                    if (DoExport(false)) {
                        reInitialize = true;
                    }       
                }
                if (GUILayout.Button("Export And Upgrade")) {
                    if (DoExport(true)) {
                        reInitialize = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else {
                // create a new module project specifier
                if (GUILayout.Button("Create Module Project Specifier")) {
                    ModuleProjectSpecifier specifier = AssetTools.CreateScriptableObject<ModuleProjectSpecifier>(defaultSpecifierPath);
                    Selection.activeObject = specifier;
                    EditorGUIUtility.PingObject(specifier);
                    Debug.Log("Created Module Project Specifier at: " + defaultSpecifierPath);
                    reInitialize = true;
                }
            }
            return reInitialize;
        }
    }

}