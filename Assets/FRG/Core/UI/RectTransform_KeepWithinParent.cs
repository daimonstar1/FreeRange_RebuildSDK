using System;
using System.Collections.Generic;

using UnityEngine;

namespace FRG.Core.UI {
    [ExecuteInEditMode]
    public class RectTransform_KeepWithinParent : MonoBehaviour {
        [SerializeField] public bool lateUpdateEnabled = true;
        [SerializeField] public Vector2 buffer = default(Vector2); //additional buffer-size

        public void Update() {
            var parent = transform.parent as RectTransform;
            var trans = transform as RectTransform;

            //if there is no transform or parent, return
            if(trans == null || parent == null) return;

            //if the width is bigger than the parent's width, the only way to fit is to cap the width and center it
            if(((trans.rect.size.x + (2f * buffer.x)) * trans.localScale.x) > parent.rect.size.x) {
                trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parent.rect.size.x / trans.localScale.x);
                trans.anchoredPosition = new Vector2(buffer.x, trans.anchoredPosition.y);
            }

            //if the height it bigger than the parent's height, the only way to fit is to cap the height and center it
            if(((trans.rect.size.y + (2f * buffer.y)) * trans.localScale.x) > parent.rect.size.y) {
                trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parent.rect.size.y / trans.localScale.y);
                trans.anchoredPosition = new Vector2(trans.anchoredPosition.x, buffer.y);
            }
            
            //if the element is smaller than the parent, find if it is within or not
            Vector2 maxPos = (parent.rect.size / 2f) - Vector2.Scale(Vector2.one - trans.pivot, Vector2.Scale(trans.localScale, trans.rect.size)) - buffer;
            Vector2 minPos = Vector2.Scale(trans.pivot, Vector2.Scale(trans.localScale, trans.rect.size)) - (parent.rect.size / 2f) + buffer;

            //if it is too far to the right
            if(trans.anchoredPosition.x > maxPos.x) {
                trans.anchoredPosition = new Vector2(maxPos.x, trans.anchoredPosition.y);

            //if it is too far to the left
            }else if(trans.anchoredPosition.x < minPos.x) {
                trans.anchoredPosition = new Vector2(minPos.x, trans.anchoredPosition.y);
            }

            //if it is too far up
            if(trans.anchoredPosition.y > maxPos.y) {
                trans.anchoredPosition = new Vector2(trans.anchoredPosition.x, maxPos.y);

            //if it is too far down
            }else if(trans.anchoredPosition.y < minPos.y) {
                trans.anchoredPosition = new Vector2(trans.anchoredPosition.x, minPos.y);
            }
        }

        public void LateUpdate() {
            if(lateUpdateEnabled)
                Update();
        }
    }
}
