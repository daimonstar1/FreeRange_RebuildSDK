using FRG.SharedCore;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FRG.Core.UI
{
    [Serializable]
    [ExecuteInEditMode]
    public class UIInvisibleGraphic : Graphic
    {
        [Serializable]
        public enum Interactibility {
            Inherit,
            Override,
        }

        [SerializeField] public Interactibility interactibility = Interactibility.Inherit;
        [SerializeField, InspectorHide("_OverridesInteractibility")] public bool interactableOverride = true;

        private bool _OverridesInteractibility() {
            return interactibility == Interactibility.Override;
        }

        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            bool? interactable = null;

            //if we're overriding the interactibility, use the override-value
            if(interactibility == Interactibility.Override) {
                interactable = interactableOverride;

            //otherwise, determine the interactibility via checking CanvasGroup/Selectable parent components
            } else {
                //if this is a child of a canvas group, use its interactibility value
                var canvGroup = GetComponentInParent<CanvasGroup>();
                if(canvGroup != null) {
                    interactable = canvGroup.interactable;
                }else {
                    //otherwise, if this is a child of a selectable element, use that interactable value
                    var selectable = GetComponentInParent<Selectable>();
                    if(selectable != null) {
                        interactable = selectable.IsInteractable();
                    }
                }
            }
            // rect bounds are already checked
            return gameObject.activeInHierarchy && isActiveAndEnabled && (!interactable.HasValue || interactable.Value);
        }
#if UNITY_STANDALONE
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            // do nothing to make it invisible without rendering to screen
        }
#endif
        public override void Rebuild(CanvasUpdate update)
        {
            // do nothing to make it invisible without rendering to screen
        }
    }
}