// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace UnityTools {

    public static class Vectors
    {
        public static float RandomRange (this Vector2 range) {
            if (range.x == range.y) return range.x;
            return Random.Range(range.x, range.y);
        }
    }
}
