using System.Collections;
using System.Collections.Generic;
using UnityEngine;



using UnityEditor;
using UnityTools.EditorTools;
using System;
using UnityEngine.SceneManagement;
namespace UnityTools {

    [System.Serializable] public class FastTravelComponenet {
        public string scene;
        public string fastTravelTargetName;
        public void DoFastTravel () {
            FastTravel.FastTravelTo(scene, fastTravelTargetName);
        }
    }

    #if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(FastTravelComponenet))] 
    public class FastTravelComponenetDrawer : PropertyDrawer
    {
            
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);
            GUITools.Label(pos, label, GUITools.black, GUITools.boldLabel);
            pos.x += GUITools.iconButtonWidth;
            pos.width -= GUITools.iconButtonWidth;
            pos.y += GUITools.singleLineHeight;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("scene"), true);
            pos.y += GUITools.singleLineHeight;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("fastTravelTargetName"), true);
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight * 3;
        }
    }

    #endif

    public class FastTravel : InitializationSingleTon<FastTravel>
    {
        protected override void Awake() {
            base.Awake();
            if (!thisInstanceErrored) {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        public static void FastTravelTo (string scene, string fastTravelTargetName) {
            if (string.IsNullOrEmpty(scene)) {
                // Debug.LogWarning("No Scene Specified");
                return;
            }

            fastTravelling = true;
            FastTravel.fastTravelTargetName = fastTravelTargetName;

            // if we're not specifying a target, use our default one if 
            // specified

            if (getFastTravelDefaultPosition != null) {
                if (string.IsNullOrEmpty(fastTravelTargetName)) {
                    Vector3 pos;
                    if (getFastTravelDefaultPosition(scene, out pos)) {
                        FastTravel.fastTravelTargetName = useRawPositionKey;
                        fastTravelTargetPosition = pos;
                    }
                }
            } 


            SceneLoading.LoadSceneAsync(scene, null, null, LoadSceneMode.Single, false);
        }
        public static void FastTravelTo (string scene, Vector3 targetPosition) {
            fastTravelTargetPosition = targetPosition;
            FastTravelTo(scene, useRawPositionKey);
        }


        public delegate bool GetFastTravelDefaultPosition (string scene, out Vector3 position);
        static GetFastTravelDefaultPosition getFastTravelDefaultPosition;

        public static void SetGetFastTravelDefaultPosition (GetFastTravelDefaultPosition getter) {
            getFastTravelDefaultPosition = getter;
        }


        static bool fastTravelling;
        static string fastTravelTargetName = null;
        const string useRawPositionKey = "USERAWPOSITION";
        static Vector3 fastTravelTargetPosition;


        static Vector3 GroundPosition (Vector3 pos) {
            RaycastHit hit;
            if (Physics.Raycast(pos + Vector3.up * 100, Vector3.down, out hit, 200, -1, QueryTriggerInteraction.Ignore))
                pos.y = hit.point.y;
            return pos;
        }


        static void OnSceneLoaded (Scene scene, LoadSceneMode mode) {
            if (fastTravelling) {
                if (GameManager.playerExists) {
                    if (fastTravelTargetName == useRawPositionKey) {

                        // Debug.Log("Warped player to " + fastTravelTargetPosition);
                        GameManager.playerActor.transform.WarpTo(GroundPosition(fastTravelTargetPosition), GameManager.playerActor.transform.rotation);
                    }
                    else {
                        FastTravelMarker marker = FastTravelMarker.GetMarker(fastTravelTargetName);
                        if (marker != null) {
                            GameManager.playerActor.transform.WarpTo(GroundPosition(marker.transform.position), marker.transform.rotation);
                        }
                    }
                }
                fastTravelling = false;
            }
        }
    }
}
