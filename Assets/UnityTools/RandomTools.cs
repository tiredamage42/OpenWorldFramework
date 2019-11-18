using UnityEngine;

namespace UnityTools {

    public class RandomTools 
    {
        public static float RandomSign (float value, float mask) {
            return (mask == 0 || value == 0 || Random.value < .5f) ? value : -value;
        }
        public static float RandomSign (float value) {
            return RandomSign(value, 1);
        }
        public static Vector3 RandomSign (Vector3 v, Vector3 mask) {
            return new Vector3 (RandomSign(v.x, mask.x), RandomSign(v.y, mask.y), RandomSign(v.z, mask.z));
        }
        public static Vector3 RandomSign (Vector3 v) {
            return RandomSign(v, Vector3.one);
        }   
    }
}
