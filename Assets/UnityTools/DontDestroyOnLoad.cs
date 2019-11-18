// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace UnityTools {
    public class DontDestroyOnLoad : MonoBehaviour
    {
        void Awake () {
            gameObject.DontDestroyOnLoad(true);
        }
    }
}
