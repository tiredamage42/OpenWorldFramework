using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools {
   
    public abstract class SimpleUnityInputsInterface : ActionsInterfaceController
    {
        protected override Vector2 GetMousePos (int controller) {
            return Input.mousePosition;
        }
        protected override Vector2 GetMouseAxis (int controller) {
            Vector2 mPos = Input.mousePosition;
            return new Vector2((mPos.x / Screen.width) * 2 - 1, (mPos.y / Screen.height) * 2 - 1); 
        }
        
        protected override Vector2 GetMouseScrollDelta (int controller) {
            return Input.mouseScrollDelta;
        }
        protected override bool GetMouseButtonDown (int button, int controller) {
            return Input.GetMouseButtonDown(button);
        }
        protected override bool GetMouseButton (int button, int controller) {
            return Input.GetMouseButton(button);
        }
        protected override bool GetMouseButtonUp (int button, int controller) {
            return Input.GetMouseButtonUp(button);
        }
        protected override int MaxControllers () {
            return 1;
        }
    }
}
