using System;
using System.Collections.Generic;

using UnityEngine;

namespace FRG.Core.UI {
    /// <summary>
    /// Component which updates a UnityEngine.UI.Graphic in some manner relative to another graphic
    /// </summary>
    [ExecuteInEditMode]
    public class ImageUpdater : MonoBehaviour {

        #region SERIALIZED_FIELDS
        /// <summary>
        /// The value to look-up and modify
        /// </summary>
        public enum ValueToChange {
            None                = 0,
            Texture             = 1,
            Material            = 2,
            Color               = 4,

            TextureAndMaterial  = Texture  | Material,
            TextureAndColor     = Texture  | Color,
            MaterialAndColor    = Material | Color,
            All                 = Texture  | Material | Color,
        }

        /// <summary>
        /// When to call the update
        /// </summary>
        public enum UpdateMode {
            Update     = 0,
            LateUpdate = 1,
            Start      = 2,
            Awake      = 3,
            OnEnable   = 4,
            OnDisable  = 5,
        }

        [SerializeField] public UnityEngine.UI.Graphic target = null;
        [SerializeField] public UnityEngine.UI.Graphic source = null;
        [SerializeField] public ValueToChange valueToChange = ValueToChange.All;
        [SerializeField] public UpdateMode updateMode = UpdateMode.Update;
        #endregion SERIALIZED_FIELDS

        #region METHODS
        void Update() {
            if(updateMode == UpdateMode.Update) {
                DoUpdate();
            }
        }

        void LateUpdate() {
            if(updateMode == UpdateMode.LateUpdate) {
                DoUpdate();
            }
        }

        void Awake() {
            if(updateMode == UpdateMode.Awake) {
                DoUpdate();
            }
        }

        void Start() {
            if(updateMode == UpdateMode.Start) {
                DoUpdate();
            }
        }

        void OnEnable() {
            if(updateMode == UpdateMode.OnEnable) {
                DoUpdate();
            }
        }

        void OnDisable() {
            if(updateMode == UpdateMode.OnDisable) {
                DoUpdate();
            }
        }

        public void DoUpdate() {
            if(target != null && source != null) {

                if((valueToChange & ValueToChange.Material) != 0) {
                    target.material = source.material;
                }
                
                if((valueToChange & ValueToChange.Texture) != 0) {
                    if(target is UnityEngine.UI.Image && source is UnityEngine.UI.Image) {
                        ((UnityEngine.UI.Image)target).sprite = ((UnityEngine.UI.Image)source).sprite;

                    }else if(target is UnityEngine.UI.RawImage) {
                        ((UnityEngine.UI.RawImage)target).texture = source.mainTexture;
                    }
                }
                
                if((valueToChange & ValueToChange.Color) != 0) {
                    target.color = source.color;
                }
            }
        }
        #endregion METHODS
    }
}
