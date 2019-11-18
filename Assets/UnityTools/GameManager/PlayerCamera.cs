using UnityEngine;

namespace UnityTools {
    public class PlayerCamera : MonoBehaviour
    {
        static PlayerCamera _i;
        static PlayerCamera instance { get { return Singleton.GetInstance<PlayerCamera>(ref _i, true); } }
        static Camera _camera;
        public static Camera myCamera {
            get {
                if (_camera == null) {
                    if (instance != null) {
                        _camera = instance.GetComponent<Camera>();
                    }
                }
                return _camera;
            }
        }
    }
}
