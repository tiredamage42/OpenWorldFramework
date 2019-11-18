
using UnityEngine;
using UnityEditor;

namespace UnityTools.EditorTools.Internal.Modules {
// namespace CustomEditorTools.Internal.Modules.Editor {

    public class ModulesWindow : EditorWindow
    {

        		
        [MenuItem(ProjectTools.defaultMenuItemSection + "Unity Tools/Editor Tools/Modules", false, ProjectTools.defaultMenuItemPriority)]
		static void OpenWindow () {
            EditorWindowTools.OpenWindowNextToInspector<ModulesWindow>("Modules");
		}

        public ImportModules importModules = new ImportModules();
        public ExportModules exportModules = new ExportModules();
        int chosenTab;
        
        void OnEnable () {
            Initialization();
        }
        void Initialization () {
            exportModules.InitializeModuleExporting();
            importModules.InitializeModuleImporting(exportModules.projectSpecifierModuleName);
        }

        GUIContent refreshWindowGUI;
        void OnGUI () {
            GUITools.Space(3);
            
            if (refreshWindowGUI == null || string.IsNullOrEmpty(refreshWindowGUI.text))
                refreshWindowGUI = new GUIContent("Refresh Modules Window");

            if (GUITools.Button(refreshWindowGUI, GUITools.green, GUITools.button, GUITools.black)) Initialization();

            GUITools.Space(2);
            
            chosenTab = GUILayout.Toolbar(chosenTab, new string[] { "Import", "Export" });
            
            GUITools.Space(2);

            if (chosenTab == 0) {
                if (importModules.OnGUI(exportModules.projectSpecifierModule)) {
                    Initialization();
                }
            }
            else {
                if (exportModules.OnGUI()) {
                    Initialization();
                }
            }
        }
    }    
}