using UnityEngine;
namespace UnityTools {
    public class FreeCamera : CustomUpdaterMonobehaviour
    {

        [Header("Axes")]
        [Action] public Vector2Int look;
        [Action] public Vector3Int move;

        [Header("If Axis, Keep Negative")]
        [Action] public Vector3Int negMove = new Vector3Int(-1, -1, -1);

        [Header("Speeds")]
		public float moveSpeed = 1;
		public float turnSpeed = 5;
        
        float rotX, rotY;
	
        void Start () {
            rotX = transform.eulerAngles.x;
            rotY = transform.eulerAngles.y;


            GetComponent<Actor>().InitializeActor(() => transform.position, () => transform.forward );
        }

        float DecideAxis (int pos, int neg) {
            float axis = 0;
            if (neg >= 0) {
                if (ActionsInterface.GetAction(pos)) axis += 1;
                if (ActionsInterface.GetAction(neg)) axis -= 1;
            }
            else axis = ActionsInterface.GetAxis(pos);
            return axis;
        }

		public override void UpdateLoop (float deltaTime) {
	        Vector3 side = transform.right * DecideAxis(move.x, negMove.x);
            Vector3 upDown = transform.up * DecideAxis(move.y, negMove.y);
            Vector3 fwd = transform.forward * DecideAxis(move.z, negMove.z);

            transform.position += (side + upDown + fwd) * moveSpeed * deltaTime;

            float turnSpeed = this.turnSpeed * deltaTime;
            rotX += ActionsInterface.GetAxis(look.y) * turnSpeed;
            rotY += ActionsInterface.GetAxis(look.x) * turnSpeed;

            transform.rotation = Quaternion.Euler(rotX, rotY, 0);
		}
	}
}