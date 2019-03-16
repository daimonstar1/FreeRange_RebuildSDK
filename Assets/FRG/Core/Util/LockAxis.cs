using FRG.SharedCore;
using System;

using UnityEngine;

namespace FRG.Core
{

    [Serializable]
    [ExecuteInEditMode]
    public class LockAxis : MonoBehaviour {
        [SerializeField] public Mode mode = Mode.Update;
        [SerializeField] public bool lockX = false;
        [SerializeField] public bool lockY = false;
        [SerializeField] public bool lockZ = false;

        [InspectorHide("_IsNotRectTransform")]
        [SerializeField] public bool local = true;

        [InspectorReadOnly("_LockX")]
        [SerializeField] public float x = 0f;

        [InspectorReadOnly("_LockY")]
        [SerializeField] public float y = 0f;

        [InspectorReadOnly("_LockZ")]
        [SerializeField] public float z = 0f;

        public enum Mode {
            Update,
            LateUpdate,
            Awake,
            Start,
        }


        private bool _LockX() { return lockX; }
        private bool _LockY() { return lockY; }
        private bool _LockZ() { return lockZ; }
        private bool _IsNotRectTransform() { return !(transform is RectTransform); }

        public void Update() {
            if(mode == Mode.Update) {
                DoUpdate();
            }
        }

        public void LateUpdate() {
            if(mode == Mode.LateUpdate) {
                DoUpdate();
            }
        }

        public void Awake() {
            if(mode == Mode.Awake) {
                DoUpdate();
            }
        }

        public void Start() {
            if(mode == Mode.Start) {
                DoUpdate();
            }
        }

        public void DoUpdate() {
            if(transform is RectTransform) {
                (transform as RectTransform).anchoredPosition3D = new Vector3(
                    lockX ? x : transform.localPosition.x,
                    lockY ? y : transform.localPosition.y,
                    lockZ ? z : transform.localPosition.z
                );
            } else {
                transform.position = new Vector3(
                    lockX ? x : (local ? transform.localPosition.x : transform.position.x),
                    lockY ? y : (local ? transform.localPosition.y : transform.position.y),
                    lockZ ? z : (local ? transform.localPosition.z : transform.position.z)
                );
            }
        }
    }
}
