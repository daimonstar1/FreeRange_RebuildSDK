using UnityEngine;

namespace FRG.Core {

    //[ExecuteInEditMode]
    public class FaceCamera : MonoBehaviour {
        public enum FacingAxis {
            Forward,
            Up,
            Right
        }
        public FacingAxis axis = FacingAxis.Forward;

        public bool reverseAxis = false;

        public bool faceWorld;
        [InspectorHide("FaceWorld")]
        public Vector3 worldDirection = new Vector3(1, 0, 0);
        [InspectorHide("NotFaceWorld")]
        public Camera facingCamera = null;
        [InspectorHide("NotFaceWorld")]
        public bool lockToWorldUp = false;

        private Transform parent = null;

        public void SetParent(Transform t) { parent = t; }

        private bool FaceWorld() { return faceWorld; }
        private bool NotFaceWorld() { return !faceWorld; }

      


        void LateUpdate() {
            if(facingCamera == null) {
                facingCamera = Camera.main;
            }
            if(facingCamera == null) {
                return;
            }

//#if UNITY_EDITOR
//            if(Camera.current != null) {
//                facingCamera = Camera.current;
//            }
//#endif

            if(parent != null) {
                transform.position = parent.position;
            }

            Vector3 facing = worldDirection;
            if(!faceWorld) {
                Vector3 facePos = Vector3.zero;
                bool facingTargetFound = false;

                if(facingCamera != null) {
                    facePos = facingCamera.transform.position;
                    facingTargetFound = true;
                    //Debug.Log("FaceCamera("+gameObject.name+") facingCamera("+facingCamera.name+") ");
                }
                else {
                    //Debug.Log("FaceCamera("+gameObject.name+") no camera");
                }

                if(facingTargetFound) {
                    facing = (facePos - transform.position).normalized;
                }
                else {
                    facing = transform.forward;
                }

                if(lockToWorldUp) {
                    facing = Vector3.ProjectOnPlane(facing, Vector3.up).normalized;
                }
            }
            if(facing == Vector3.zero) facing = Vector3.forward;

            if(reverseAxis) facing = -facing;

            switch(axis) {
                case FacingAxis.Forward:
                    transform.forward = facing;
                    break;
                case FacingAxis.Up:
                    transform.up = facing;
                    break;
                case FacingAxis.Right:
                    transform.right = facing;
                    break;
            }
        }
    }

}