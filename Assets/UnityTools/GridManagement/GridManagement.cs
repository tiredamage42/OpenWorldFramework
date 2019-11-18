using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools {

    public class GridManagement
    {

        static int Abs (int x) { return x < 0 ? -x : x; }
        static int Max (int x, int y) { return x > y ? x : y; }
        public static int GridDistance (Vector2Int a, Vector2Int b) {
            return Max(Abs(a.x - b.x), Abs(a.y - b.y));
        }

        public static Vector2Int CalculateGrid (Vector3 pos, float gridSize) {
            return new Vector2Int(
                (int)(pos.x / gridSize) - (pos.x < 0 ? 1 : 0),
                (int)(pos.z / gridSize) - (pos.z < 0 ? 1 : 0)
            );
        }




    }
}
