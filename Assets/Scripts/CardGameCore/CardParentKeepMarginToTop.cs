using UnityEngine;

namespace FRG.Taco
{
    [ExecuteInEditMode]
    public class CardParentKeepMarginToTop : MonoBehaviour
    {
        [SerializeField] float margin;

        void Update()
        {
            var rect = GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -(rect.rect.height / 2) - margin);
        }
    }
}