#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


namespace UnityTools.EditorTools.Internal.Modules {

// namespace CustomEditorTools.Internal.Modules {
    [CreateAssetMenu(menuName="Unity Tools/Editor Tools/Module", fileName="New Module")]
    public class Module : ScriptableObject {
        [HideInInspector] public string moduleName;
        [HideInInspector] public int currentVersion;
    }

    [CustomEditor(typeof(Module))]
    class ModuleEditor : UnityEditor.Editor {
        
        Module t;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (t == null)
                t = target as Module;

            EditorGUILayout.HelpBox("Do not move this object!\nKeep it in the module's root directory", MessageType.Info);

            EditorGUILayout.LabelField("Module:", GUITools.boldLabel);
            EditorGUILayout.LabelField("\t" + t.moduleName);

            EditorGUILayout.LabelField("Version:", GUITools.boldLabel);
            EditorGUILayout.LabelField("\t" + t.currentVersion.ToString());
        }
    }
}
#endif