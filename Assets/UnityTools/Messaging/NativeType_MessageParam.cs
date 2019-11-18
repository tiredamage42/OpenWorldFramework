using UnityEngine;

using UnityEditor;
namespace UnityTools.Internal {

    public class NativeType_MessageParam : MessageParameter
    {
        public enum NativeType { Float, Int, Bool, String };
        public float nValue;
        public bool bValue;
        public string sValue;
        public NativeType type;

        public override object GetParamObject() {
            switch (type) {
                case NativeType.Float: return nValue;
                case NativeType.Int: return nValue;
                case NativeType.Bool: return bValue;
                case NativeType.String: return sValue;
            }
            return nValue;
        }


        #if UNITY_EDITOR
        public override void DrawGUIFlat(Rect pos) {
            switch (type) {
                case NativeType.Float:  nValue = EditorGUI.FloatField(pos, nValue);     break;
                case NativeType.Int:    nValue = EditorGUI.IntField(pos, (int)nValue);  break;
                case NativeType.Bool:   bValue = EditorGUI.Toggle(pos, bValue);         break;
                case NativeType.String: sValue = EditorGUI.TextField(pos, sValue);      break;
            }
        }

        public override void DrawGUI(Rect pos) {
            float hWidth = pos.width * .5f;
            pos.width = hWidth;
            type = (NativeType)EditorGUI.EnumPopup(pos, type);
            pos.x += hWidth;
            DrawGUIFlat (pos);                
        }
        #endif
        
    }
}
