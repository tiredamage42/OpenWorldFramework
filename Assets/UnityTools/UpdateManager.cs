using UnityEngine;
using System;

namespace UnityTools {
    /*
        lets static classes hook into the application's update
        
        and run coroutines
    */
    public class UpdateManager : InitializationSingleTon<UpdateManager>
    {
        public event Action<float> update, fixedUpdate, lateUpdate;
        void Update() { if (update != null) update(Time.deltaTime); }
        void FixedUpdate() { if (fixedUpdate != null) fixedUpdate(Time.fixedDeltaTime); }
        void LateUpdate() { if (lateUpdate != null) lateUpdate(Time.deltaTime); }
    }

    public enum UpdateMode { Update, FixedUpdate, LateUpdate, Custom };


    public abstract class CustomUpdater {
        protected abstract UpdateMode GetUpdateMode ();
        public abstract void UpdateLoop (float deltaTime);

        public bool Update () {
            if (GetUpdateMode() != UpdateMode.Update) return false;
            UpdateLoop(Time.deltaTime);
            return true;
        }
        public bool FixedUpdate () {
            if (GetUpdateMode() != UpdateMode.FixedUpdate) return false;
            UpdateLoop(Time.fixedDeltaTime);
            return true;
        }
        public bool LateUpdate () {
            if (GetUpdateMode() != UpdateMode.LateUpdate) return false;
            UpdateLoop(Time.deltaTime);
            return true;
        }
    }

    public abstract class CustomUpdaterMonobehaviour : MonoBehaviour {

        public UpdateMode updateMode;
        public abstract void UpdateLoop (float deltaTime);

        void Update () {
            if (updateMode != UpdateMode.Update) return;
            UpdateLoop(Time.deltaTime);
        }
        void FixedUpdate () {
            if (updateMode != UpdateMode.FixedUpdate) return;
            UpdateLoop(Time.fixedDeltaTime);
        }
        void LateUpdate () {
            if (updateMode != UpdateMode.LateUpdate) return;
            UpdateLoop(Time.deltaTime);
        }
    }
}
