using UnityEngine;

namespace UnityTools {

    [System.Serializable] public struct sVector2 {
        public float x, y;
        public sVector2 (Vector2 v) { this.x = v.x; this.y = v.y; }
        public static implicit operator Vector2 (sVector2 v) => new Vector2(v.x, v.y);
        public static implicit operator sVector2 (Vector2 v) => new sVector2(v);
    }
    
    [System.Serializable] public struct sVector3 {
        public float x, y, z;
        public sVector3 (Vector3 v) { this.x = v.x; this.y = v.y; this.z = v.z; }
        public static implicit operator Vector3 (sVector3 v) => new Vector3(v.x, v.y, v.z);
        public static implicit operator sVector3 (Vector3 v) => new sVector3(v);
    }

    [System.Serializable] public struct sQuaternion {
        public float x, y, z, w;
        public sQuaternion (Quaternion v) { this.x = v.x; this.y = v.y; this.z = v.z; this.w = v.w; }
        public static implicit operator Quaternion (sQuaternion v) => new Quaternion(v.x, v.y, v.z, v.w);
        public static implicit operator sQuaternion (Quaternion v) => new sQuaternion(v);
    }
}
