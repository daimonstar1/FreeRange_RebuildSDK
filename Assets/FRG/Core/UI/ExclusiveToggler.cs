using System;
using System.Collections.Generic;
using UnityEngine;

namespace FRG.Core.UI {

    [ExecuteInEditMode]
    public class ExclusiveToggler : MonoBehaviour {
        [SerializeField] string exclusiveGroup = "";
        [SerializeField] ToggleMode toggleMode = ToggleMode.Unknown;

        public enum ToggleMode {
            Unknown    = 0,
            GameObject = 1,
            Toggle     = 2,
        }

        //the buffer list used to get neighbor togglers
        [NonSerialized] List<ExclusiveToggler> neighbors = new List<ExclusiveToggler>();

        //flag set on Update() whenever we need to re-disable neighbors
        [NonSerialized] bool needsUpdate = false;

        //whether this ExclusiveToggler was toggled on/off the previous frame
        [NonSerialized] bool previousToggleState = false;

        /// <summary>
        /// If this is the first time setting up the toggle, we may need to infer the default toggle mode from what components this object has
        /// </summary>
        void Awake() {
            _ResolveToggleMode();
        }

        void OnEnable() {
            switch(toggleMode) {
                default: case ToggleMode.Unknown:
                    _ResolveToggleMode();
                    break;
                case ToggleMode.GameObject:
                    needsUpdate = true;
                    break;
                case ToggleMode.Toggle:
                    break;

            }
        }

        void Update() {
            switch(toggleMode) {
                //if we're in unknwon mode, attempt to resolve the mode to something valid
                default: case ToggleMode.Unknown:
                    _ResolveToggleMode();
                    break;

                //if we're in gameObject-mode, just set the previousToggleState flag
                case ToggleMode.GameObject:
                    previousToggleState = true;
                    break;

                //if we're in toggle-mode, check the toggle.isOn field
                case ToggleMode.Toggle:
                    var toggle = GetComponent<UnityEngine.UI.Toggle>();
                    if(toggle != null) {
                        if(toggle.isOn && !previousToggleState) {
                            needsUpdate = true;
                        }
                        previousToggleState = toggle.isOn;
                    }
                    break;
            }

            if(needsUpdate) {
                _DoUpdate();
            }
        }

        void _ResolveToggleMode() {
            if(toggleMode == ToggleMode.Unknown) {
                if(GetComponent<UnityEngine.UI.Toggle>() != null) {
                    toggleMode = ToggleMode.Toggle;
                } else {
                    toggleMode = ToggleMode.GameObject;
                }
            }
        }

        void _DoUpdate() {
            needsUpdate = false;
            if(transform.parent != null) {
                //populate neighbors list with all valid neighbors
                neighbors.Clear();
                transform.parent.GetComponentsInChildren(neighbors, _ValidateNeighborRecursion, _ValidateNeighbor);

                //disable all neighbors
                if(neighbors != null && neighbors.Count > 0) {
                    for(int i=0; i<neighbors.Count; ++i) {
                        _DisableNeighbor(neighbors[i]);
                    }
                }
            }
        }

        void _DisableNeighbor(ExclusiveToggler neighbor) {
            if(neighbor != null) {
                switch(toggleMode) {
                    default: case ToggleMode.Unknown:
                        break;
                    case ToggleMode.GameObject:
                        neighbor.gameObject.SetActive(false);
                        break;
                    case ToggleMode.Toggle:
                        var toggle = neighbor.GetComponent<UnityEngine.UI.Toggle>();
                        if(toggle != null) {
                            toggle.isOn = false;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// While searching for neighbor togglers, we need to validate with this function
        /// </summary>
        bool _ValidateNeighborRecursion(Transform neighbor) {
            if(neighbor != null
            && neighbor.GetInstanceID() != transform.GetInstanceID()) {
                return neighbor.GetComponent<ExclusiveToggler>() == null;
            }

            return false;
        }

        /// <summary>
        /// While searching for neighbor togglers, we need to validate them before adding them to the neighbors list
        /// </summary>
        bool _ValidateNeighbor(ExclusiveToggler neighbor) {
            return neighbor != null
                && neighbor.GetInstanceID() != this.GetInstanceID()
                && string.Equals(neighbor.exclusiveGroup, this.exclusiveGroup, StringComparison.OrdinalIgnoreCase);
        }
    }
}