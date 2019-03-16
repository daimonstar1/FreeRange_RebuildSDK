
using UnityEngine;

namespace FRG.Core.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class TextScaler : MonoBehaviour {
        private float lastX = 0f;
        private float lastY = 0f;
        private RectTransform _rect = null;

        public void Awake() {
            if(target == null && gameObject != null && gameObject.GetComponent<UnityEngine.UI.Text>() != null) {
                this.target = gameObject.GetComponent<UnityEngine.UI.Text>();
            }
        }

        public RectTransform rect {
            get {
                if(_rect == null) _rect = gameObject.GetComponent<RectTransform>();
                return _rect;
            }
            private set { _rect = value; }
        }

        [SerializeField] public UnityEngine.UI.Text target = null;
        [SerializeField] public TextScalerType scalerType = default(TextScalerType);
        [SerializeField] public int referenceFontSize = 12;
        [SerializeField] public float referenceRectSize = 100f;
        public enum TextScalerType {
            FromWidth,
            FromHeight,
        }

        public void Update() {
#if UNITY_EDITOR
            if(!Application.isPlaying) {
                RefreshScale();
                return;
            }
#endif

            switch(scalerType) {
                case TextScalerType.FromWidth:
                    float newX = rect.rect.width;
                    if(lastX != newX) RefreshScale();
                    lastX = newX;
                    break;
                case TextScalerType.FromHeight:
                    float newY = rect.rect.height;
                    if(lastY != newY) RefreshScale();
                    lastY = newY;
                    break;
            }
        }

        public void LateUpdate() {
            Update();
        }

        public void RefreshScale() {
            if(target != null) {
                target.fontSize = GetAdjustedTextSize();
            }
        }

        public int GetAdjustedTextSize() {
            switch(scalerType) {
                case TextScalerType.FromWidth:
                    return (int)(((float)referenceFontSize) * (rect.rect.width / referenceRectSize));
                case TextScalerType.FromHeight:
                    return (int)(((float)referenceFontSize) * (rect.rect.height / referenceRectSize));
                default:
                    return referenceFontSize;
            }
        }
    }
}
