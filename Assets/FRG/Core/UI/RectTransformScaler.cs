using FRG.SharedCore;
using System;
using UnityEngine;

namespace FRG.Core.UI
{
    /// <summary>
    /// Properly scales certain components based upon the rect-transforms size. Currently supports transforms, particle systems, ortho cameras, 
    /// </summary>
    [ExecuteInEditMode]
    public class RectTransformScaler : MonoBehaviour {
        public enum ScalerTarget {
            Transform          = 0,
            RectTransform      = 1,
            ParticleSystem     = 2,
            Camera             = 3,
            PreferredTextSize  = 4,
            Canvas             = 5,
        }

        public enum ScalerSource {
            RectTransform     = 0,
            PreferredTextSize = 1,
            PreferredTMPTextSize = 2,
        }

        public enum Mode {
            Update     = 0,
            LateUpdate = 1,
            Awake      = 2,
            OnEnable   = 3,
            Start      = 4,
        }

        public enum AveragingMode {
            Static    = 0,
            SnapX     = 1,
            SnapY     = 2,
            MinXY     = 3,
            MaxXY     = 4,
            AverageXY = 5,
        }

        public enum AxisMask {
            None = 0,
            X    = 1,
            Y    = 2,
            XY   = X | Y,
        }

        //when this script will update
        [SerializeField] public Mode mode = Mode.Update;
        //from where to read the scale data
        [SerializeField] public ScalerSource sourceMode = ScalerSource.RectTransform;
        //on what to apply this scale data to
        [SerializeField] public ScalerTarget targetMode = ScalerTarget.Transform;

        [SerializeField] public AxisMask mask = AxisMask.XY;

        #region SOURCE_DATA_FIELDS
        //the recttransform source to read size data from (Only if sourceMode == ScalerSource.RectTransform)
        [SerializeField] public RectTransform source = null;

        //the text source to read preferred size data from (Only if sourceMode == ScalerSource.PreferredTextSize)
        [InspectorHide("_IsSourceMode_PreferredTextSize")]
        [SerializeField] public UnityEngine.UI.Text _textSource = null;

        //the text source to read preferred size data from (Only if sourceMode == ScalerSource.PreferredTextSize)
        [InspectorHide("_IsSourceMode_PreferredTMPTextSize")]
        [SerializeField]public TMPro.TMP_Text _TMPtextSource = null;

        //how to scale the source size data
        [SerializeField] public Vector2 referenceScaleSize = new Vector2(100f, 100f);

        //how to offset the source size data
        [SerializeField] public Vector2 referenceScaleOffset = new Vector2(0f, 0f);

        //the minimum source size data allowed
        [SerializeField] public Vector2 referenceMinimum = new Vector2(0f, 0f);

        //the maximum source size data allowed
        [SerializeField] public Vector2 referenceMaximum = new Vector2(9999f, 9999f);
        #endregion SOURCE_DATA_FIELDS

        #region TARGET_FIELDS
        //what to scale source size by before applying to target
        [InspectorHide("_IsTransformMode")]
        [SerializeField] public Vector2 scaleFactor = new Vector2(1f, 1f);
        //what to do with the z-axis before applying to target
        [InspectorHide("_IsTransformMode")]
        [SerializeField] public AveragingMode zScaleMode = AveragingMode.MinXY;

        //additional scaler after reading rect-transform size from source
        [InspectorHide("_IsRectTransformMode")]
        [SerializeField] public Vector2 rectTransformScaleFactor = new Vector2(1f, 1f);
        
        //target camera
        [InspectorHide("_IsCameraMode")]
        [SerializeField] private Camera _camera = null;
        //aspect ratio scaler before applying to camera
        [InspectorHide("_ShowCameraFields")]
        [SerializeField] public Vector2 cameraAspectFactor = new Vector2(1f, 1f);
        //scaler applied to source size data if in Camera target mode
        [InspectorHide("_ShowCameraFields")]
        [SerializeField] public float referenceCameraAspect = 1f;
        //how to apply size data to camera
        [InspectorHide("_ShowCameraFields")]
        [SerializeField] public AveragingMode cameraAspectMode = AveragingMode.MinXY;
        //
        [InspectorHide("_ShowCameraFields")]
        [SerializeField] public float referenceCameraOrthoSize = 1f;
        
        [InspectorHide("_IsParticleMode")]
        [SerializeField] private ParticleSystem _particleSystem = null;
        [InspectorHide("_IsParticleMode")]
        [SerializeField] public Vector3 particleScaleFactor = new Vector3(1f, 1f, 1f);



        [InspectorHide("_ShowParticleFields")]
        [SerializeField] public float referenceParticleSize = 1f;
        [InspectorHide("_ShowParticleFields")]
        [SerializeField] public AveragingMode particleScaleMode = AveragingMode.MinXY;
        #endregion TARGET_FIELDS

        [NonSerialized] private Transform _parentCanvas = null;
        [NonSerialized] private Transform localTransform;

        private bool _IsTransformMode() {
            return targetMode == ScalerTarget.Transform;
        }
        
        private bool _IsRectTransformMode() {
            return targetMode == ScalerTarget.RectTransform;
        }

        private bool _IsCameraMode() {
            return targetMode == ScalerTarget.Camera;
        }
        
        private bool _IsParticleMode() {
            return targetMode == ScalerTarget.ParticleSystem;
        }
        
        private bool _ShowCameraFields() {
            return _IsCameraMode() && _camera != null;
        }

        private bool _ShowParticleFields() {
            return _IsParticleMode() && _particleSystem;
        }
        

        private bool _IsSourceMode_RectTransform() {
            return sourceMode == ScalerSource.RectTransform;
        }

        private bool _IsSourceMode_PreferredTextSize() {
            return sourceMode == ScalerSource.PreferredTextSize;
        }

        private bool _IsSourceMode_PreferredTMPTextSize()
        {
            return sourceMode == ScalerSource.PreferredTMPTextSize;
        }

        private bool _ShowSourceFields_RectTransform() {
            return _IsSourceMode_RectTransform() && _textSource != null;
        }

        private bool _ShowSourceFields_Text() {
            return _IsSourceMode_PreferredTextSize() && _textSource != null;
        }

        public void Awake() {
            //guess main source
            if(source == null) {
                if(transform.parent != null && transform.parent is RectTransform) {
                    source = transform.parent as RectTransform;
                }
            }

            //guess target camera
            if(_camera == null) {
                _camera = GetComponent<Camera>();
            }

            //guess target particle system
            if(_particleSystem == null) {
                _particleSystem = GetComponent<ParticleSystem>();
            }

            //guess parent canvas
            if(_parentCanvas == null) {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    _parentCanvas = canvas.transform;
                }
            }

            //guess source text
            if(_textSource == null) {
                _textSource = GetComponent<UnityEngine.UI.Text>();
            }

            //do update if mode is awake
            if(mode == Mode.Awake) {
                _DoUpdate();
            }
        }

        public void Update() {
            if(mode == Mode.Update) {
                _DoUpdate();
            }
        }

        public void LateUpdate() {
            if(mode == Mode.LateUpdate) {
                _DoUpdate();
            }
        }

        public void OnEnable() {
            if(mode == Mode.OnEnable) {
                _DoUpdate();
            }
        }

        public void Start() {
            if(mode == Mode.Start) {
                _DoUpdate();
            }
        }

        protected struct SourceData {
            public float x;
            public float y;
            public Transform transform;

            public SourceData(float x, float y) {
                this.x = x;
                this.y = y;
                this.transform = null;
            }

            public SourceData(float x, float y, Transform transform) {
                this.x = x;
                this.y = y;
                this.transform = transform;
            }
        }

        protected SourceData _GetSourceData() {
            switch(sourceMode) {
                default:  case ScalerSource.RectTransform:
                    if(source == null) return default(SourceData);

                    //return source data with recttransform size
                    return new SourceData(
                        Mathf.Max(referenceMinimum.x, Mathf.Min(referenceMaximum.x, referenceScaleOffset.x + source.rect.width)),
                        Mathf.Max(referenceMinimum.y, Mathf.Min(referenceMaximum.y, referenceScaleOffset.y + source.rect.height)),
                        localTransform);

                case ScalerSource.PreferredTextSize:
                    if(_textSource == null) return default(SourceData);

                    //return source data with text preferred size
                    return new SourceData(
                        Mathf.Max(referenceMinimum.x, Mathf.Min(referenceMaximum.x, referenceScaleOffset.x + _textSource.preferredWidth)),
                        Mathf.Max(referenceMinimum.y, Mathf.Min(referenceMaximum.y, referenceScaleOffset.y + _textSource.preferredHeight)),
                        localTransform);

                case ScalerSource.PreferredTMPTextSize:
                    if (_TMPtextSource == null) return default(SourceData);
                    //return source data with text preferred size
                    return new SourceData(
                        Mathf.Max(referenceMinimum.x, Mathf.Min(referenceMaximum.x, referenceScaleOffset.x + _TMPtextSource.preferredWidth)),
                        Mathf.Max(referenceMinimum.y, Mathf.Min(referenceMaximum.y, referenceScaleOffset.y + _TMPtextSource.preferredHeight)),
                        localTransform);
            }
        }

        protected void _DoUpdate() {
            if (localTransform == null) localTransform = transform;

            if (source != null) {
                SourceData sourceData = _GetSourceData();
                switch(targetMode) {
                    //transform target
                    case ScalerTarget.Transform:
                        float transX = 1f + (((sourceData.x / referenceScaleSize.x) - 1f) * scaleFactor.x);
                        float transY = 1f + (((sourceData.y / referenceScaleSize.y) - 1f) * scaleFactor.y);
                        localTransform.localScale = new Vector3(
                            ((mask & AxisMask.X) != 0) ? transX : localTransform.localScale.x,
                            ((mask & AxisMask.Y) != 0) ? transY : localTransform.localScale.y,
                            _GetAverageValue(transX,transY, localTransform.localScale.z));
                        break;

                    //rect transform target
                    case ScalerTarget.RectTransform:
                        RectTransform rectTransform = localTransform as RectTransform;
                        if(rectTransform != null) {
                            float rectX = 1f + (((sourceData.x / referenceScaleSize.x) - 1f));
                            float rectY = 1f + (((sourceData.y / referenceScaleSize.y) - 1f));
                            if((mask & AxisMask.X) != 0) {
                                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectX);
                            }
                            if((mask & AxisMask.Y) != 0) {
                                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectY);
                            }
                            Vector3 newScale = Vector3.Scale(source.transform.localScale, rectTransformScaleFactor);
                            newScale = new Vector3(((mask & AxisMask.X) != 0) ? newScale.x : localTransform.localScale.x,
                                                   ((mask & AxisMask.Y) != 0) ? newScale.y : localTransform.localScale.y,
                                                   1f);
                            localTransform.localScale = newScale;
                        }
                        break;
                    
                    //camera target
                    case ScalerTarget.Camera:
                        if(_camera != null) {
                            Vector3 canvScale = _parentCanvas != null ? _parentCanvas.localScale : Vector3.one;
                            Vector3 sourceScale = Vector3.Scale(sourceData.transform.lossyScale, new Vector3(1f / canvScale.x, 1f / canvScale.y, 1f / canvScale.z));
                            float camX = 1f + (((sourceData.x * sourceScale.x  / referenceScaleSize.x) - 1f) * cameraAspectFactor.x);
                            float camY = 1f + (((sourceData.y * sourceScale.y / referenceScaleSize.y) - 1f) * cameraAspectFactor.y);
                            _camera.aspect = referenceCameraAspect * camX / camY;
                            if(_camera.orthographic) {
                                _camera.orthographicSize = referenceCameraOrthoSize * _GetAverageValue(camX, camY, _camera.orthographicSize);
                            }
                        }
                        break;

                    //particle system target
                    case ScalerTarget.ParticleSystem:
                        if(_particleSystem != null) {
                            float partX = 1f + (((sourceData.x / referenceScaleSize.x) - 1f) * particleScaleFactor.x);
                            float partY = 1f + (((sourceData.y / referenceScaleSize.y) - 1f) * particleScaleFactor.y);
                            var main = _particleSystem.main;
                            main.startSizeMultiplier = referenceParticleSize * _GetAverageValue(partX, partY, main.startSizeMultiplier);
                        }
                        break;
                }
            }

        }

        protected float _GetAverageValue(float x, float y, float z) {
            switch(zScaleMode) {
                case AveragingMode.AverageXY:  return (x+y)/2f;
                case AveragingMode.MaxXY:      return Mathf.Max(x, y);
                case AveragingMode.MinXY:      return Mathf.Min(x, y);
                case AveragingMode.SnapX:      return x;
                case AveragingMode.SnapY:      return y;
                case AveragingMode.Static:     return z;
                default:                       return z;
            }
        }
    }
}
