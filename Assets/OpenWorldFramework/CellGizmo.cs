
using UnityEngine;

using UnityTools.GameSettingsSystem;

namespace OpenWorldFramework {
    public class CellGizmo : MonoBehaviour { 

        #if UNITY_EDITOR
        OpenWorldSettings _settings;
        OpenWorldSettings settings {
            get {
                if (_settings == null) _settings = GameSettings.GetSettings<OpenWorldSettings>();
                return _settings;
            }
        }
        bool initialized;
        string gridString;
        Vector3 middlePos;
        GUIStyle _lbl;
        GUIStyle lbl {
            get {
                if (_lbl == null) {
                    _lbl = new GUIStyle(GUI.skin.label);
                    _lbl.alignment = TextAnchor.MiddleCenter;
                    _lbl.fontStyle = FontStyle.Bold;
                }
                return _lbl;
            }
        }
        Vector3[] cubePositions, cubeSizes;
        int myLOD;
        void OnDrawGizmos () {

            if (settings == null) 
                return;

            if (!initialized || !Application.isPlaying) {
                string sceneName = gameObject.scene.name;

                myLOD = sceneName.Contains(OpenWorld.lod0Check) ? 0 : 1;

                Vector2Int grid = settings.Scene2Grid(sceneName, myLOD);
                float cellSize = settings.cellSize;

                Vector3 myPos = new Vector3(grid.x * cellSize, 0, grid.y * cellSize);
                float hSize = cellSize * .5f;

                middlePos = myPos + new Vector3(hSize, 0, hSize);

                gridString = grid.x.ToString() + "," + grid.y.ToString();
                
                float hHeight = settings.gizmoCubeHeight * .5f;
                if (myLOD == 0) {
                    cubePositions = new Vector3[] { myPos + new Vector3(hSize, hHeight, hSize) };
                    cubeSizes = new Vector3[] { new Vector3(hSize, settings.gizmoCubeHeight, hSize) };
                }
                else {
                    cubePositions = new Vector3[] {
                        myPos + new Vector3(hSize, hHeight, 0),         
                        myPos + new Vector3(hSize, hHeight, cellSize),  
                        myPos + new Vector3(0, hHeight, hSize),         
                        myPos + new Vector3(cellSize, hHeight, hSize),  
                    };

                    Vector3 size0 = new Vector3(cellSize, settings.gizmoCubeHeight, settings.gizmoCubeDepth);
                    Vector3 size1 = new Vector3(settings.gizmoCubeDepth, settings.gizmoCubeHeight, cellSize);
                    cubeSizes = new Vector3[] { size0, size0, size1, size1 };

                }
                initialized = true;
            }
            
            if (myLOD == 1) 
                UnityEditor.Handles.Label(middlePos, gridString, lbl);            
            
            Gizmos.color = myLOD == 0 ? settings.gizmoCubeColor0 : settings.gizmoCubeColor1;
            
            for (int i = 0; i < cubePositions.Length; i++) 
                Gizmos.DrawCube(cubePositions[i], cubeSizes[i]);
            
            Gizmos.color = Color.white;
        }

        #endif
    }
}
