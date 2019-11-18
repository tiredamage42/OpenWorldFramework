
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


namespace UnityTools.EditorTools.Internal.Modules {


    /*
        if the project is for editing and exporting a module,
        create an instance of this object in the project and specify the module

        DO NOT KEEP IT IN THE MODULES DIRECTORY OR IT MIGHT ACCIDENTALLY BE 
        EXPORTED WITH THE MODULE AND MESS UP OTHER PROJECTS FOR MODULES
    */
    [CreateAssetMenu(menuName="Unity Tools/Editor Tools/Module Project Specifier", fileName="ModuleProjectSpecifier")]
    public class ModuleProjectSpecifier : ScriptableObject
    {
        public Module workingModule;
    }

    [CustomEditor(typeof(ModuleProjectSpecifier))]
    class ModuleProjectSpecifierEditor : UnityEditor.Editor {
        
        ModuleProjectSpecifier t;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (t == null)
                t = target as ModuleProjectSpecifier;

            if (t.workingModule != null) {
                string newName = EditorGUILayout.TextField("Module Name", t.workingModule.moduleName);
                if (t.workingModule.moduleName != newName){
                    t.workingModule.moduleName = newName;
                    EditorUtility.SetDirty(t.workingModule);
                }
            }
        }
    }
}
#endif
