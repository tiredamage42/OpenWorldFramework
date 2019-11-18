using UnityEngine;
using UnityEditor;

namespace UnityTools {
    public class FastTravelObject : MonoBehaviour
    {
        public FastTravelComponenet fastTravelTo;
        public void FastTravel () {
            fastTravelTo.DoFastTravel();
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(FastTravelObject))] 
    public class FastTravelObjectEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Fast Travel") && Application.isPlaying) 
                (target as FastTravelObject).FastTravel();
        }
    }
    #endif
}