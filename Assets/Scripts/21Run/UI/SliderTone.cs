using UnityEngine;
using UnityEngine.EventSystems;

namespace FRG.Taco
{
    public class SliderTone : MonoBehaviour, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData eventData)
        {
            AudioManager.instance.PlaySound(AudioManager.Sound.TimerWarned);
        }
    }
}