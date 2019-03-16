using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FRG.Taco
{
    public class PausePanel : MonoBehaviour, IPointerClickHandler
    {
        public static event Action<PausePanel> OnClick;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (OnClick != null)
            {
                OnClick(this);
            }
        }
    }
}