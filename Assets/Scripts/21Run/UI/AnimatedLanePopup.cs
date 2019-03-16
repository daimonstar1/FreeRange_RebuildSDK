using System;
using FRG.Taco.Run21;
using TMPro;
using UnityEngine;

namespace FRG.Taco
{
    public class AnimatedLanePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI bustedText;
        private DisplayDeck _animatedDisplayDeck;

        public DisplayDeck AnimatedDisplayDeck
        {
            get { return _animatedDisplayDeck; }
            set { _animatedDisplayDeck = value; }
        }

        public void ToggleBustedLanePopupOn()
        {
            AnimatedDisplayDeck.DeckDisplayOptions = DisplayDeck.DisplayOptions.Down;
            AnimatedDisplayDeck.RefreshDisplay();

            // trigger card bust animations
            AnimatedDisplayDeck.PlayBustDeck(Run21Data.Instance.animationConfig.bustedDeckReadOnly, () =>
            {
                if (OnToggledOff != null)
                {
                    OnToggledOff(this);
                }
                Destroy(transform.gameObject); // kill busted popup
            });
        }

        public void ToggleScoredLanePopupOn()
        {
            AnimatedDisplayDeck.DeckDisplayOptions = DisplayDeck.DisplayOptions.Down;
            AnimatedDisplayDeck.RefreshDisplay();

            // trigger cleared card animations
            AnimatedDisplayDeck.PlayClearedDeck(Run21Data.Instance.animationConfig.scoredDeckReadOnly, () =>
            {
                if (OnToggledOff != null)
                {
                    OnToggledOff(this);
                }
                Destroy(transform.gameObject); // kill busted popup
            });
        }
        
        public event Action<AnimatedLanePopup> OnToggledOff;
    }
}