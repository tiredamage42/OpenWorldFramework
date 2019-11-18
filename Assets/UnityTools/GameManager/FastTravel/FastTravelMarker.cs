using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools {
    public class FastTravelMarker : MonoBehaviour
    {
        static List<FastTravelMarker> allMarkers = new List<FastTravelMarker>();

        public static FastTravelMarker GetMarker (string name) {
            if (string.IsNullOrEmpty(name)) {
                if (allMarkers.Count > 0) 
                    return allMarkers[0];
            }
            for (int i = 0; i < allMarkers.Count; i++) {
                if (allMarkers[i].name == name) {
                    return allMarkers[i];
                }
            }
            Debug.LogError("No Fast Travel Marker Found With Name: " + name);
            return null;
        }


        void OnEnable () {
            allMarkers.Add(this);
        }
        void OnDisable () {
            allMarkers.Remove(this);
        }
    }
}
