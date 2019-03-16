using FRG.Taco.Run21;
using UnityEngine;

namespace FRG.Taco
{
    [CreateAssetMenu(menuName = "PopupFactory")]
    public class PopupFactory : ScriptableObject
    {
        public static PopupFactory instance
        {
            get { return Run21Data.Instance.popupFactory; }
        }

        [SerializeField] private GameObject _run21ScorePopupPrefab;
        [SerializeField] private GameObject _fiveCardScorePopupPrefab;
        [SerializeField] private GameObject _blackjackScorePopupPrefab;
        [SerializeField] private GameObject _goodRunStreakPopupPrefab;
        [SerializeField] private GameObject _greatRunStreakPopupPrefab;
        [SerializeField] private GameObject _amazingRunStreakPopupPrefab;
        [SerializeField] private GameObject _outstandingRunStreakPopupPrefab;
        [SerializeField] private GameObject _perfectRunStreakPopupPrefab;
        [SerializeField] private GameObject _animatedLanePopupPrefab;
        [SerializeField] private GameObject _outOfTimePopupPrefab;
        [SerializeField] private GameObject _emptyLaneBonusPopupPrefab;
        [SerializeField] private GameObject _perfectGamePopupPrefab;
        [SerializeField] private GameObject _noBustBonusPopupPrefab;
        [SerializeField] private GameObject _bustedLanePopupPrefab;
        [SerializeField] private GameObject _comboBonusPopupPrefab;

        public enum PopupEnum
        {
            Run21Score,
            FiveCardScore,
            BlackjackScore,
            GoodRunStreak,
            GreatRunStreak,
            AmazingRunStreak,
            OutstandingRunStreak,
            PerfectRunStreak,
            OutOfTime,
            NoBustBonus,
            EmptyLaneBonus,
            PerfectGameBonus,
            BustedLanePopup,
            CombosPopup
        }

        public Popup Build(PopupEnum popupType)
        {
            Popup outgoingPopup;
            if (popupType == PopupEnum.CombosPopup)
            {
                outgoingPopup = Instantiate(_comboBonusPopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }
            else if (popupType == PopupEnum.Run21Score)
            {
                outgoingPopup = Instantiate(_run21ScorePopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }
            else if (popupType == PopupEnum.FiveCardScore)
            {
                outgoingPopup = Instantiate(_fiveCardScorePopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }

            else if (popupType == PopupEnum.BlackjackScore)
            {
                outgoingPopup = Instantiate(_blackjackScorePopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }

            else if (popupType == PopupEnum.GoodRunStreak)
            {
                outgoingPopup = Instantiate(_goodRunStreakPopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }

            else if (popupType == PopupEnum.GreatRunStreak)
            {
                outgoingPopup = Instantiate(_greatRunStreakPopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }

            else if (popupType == PopupEnum.AmazingRunStreak)
            {
                outgoingPopup = Instantiate(_amazingRunStreakPopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }

            else if (popupType == PopupEnum.OutstandingRunStreak)
            {
                outgoingPopup = Instantiate(_outstandingRunStreakPopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }
            else if (popupType == PopupEnum.OutOfTime)
            {
                outgoingPopup = Instantiate(_outOfTimePopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }
            else if (popupType == PopupEnum.NoBustBonus)
            {
                outgoingPopup = Instantiate(_noBustBonusPopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }

            else if (popupType == PopupEnum.EmptyLaneBonus)
            {
                outgoingPopup = Instantiate(_emptyLaneBonusPopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }

            else if (popupType == PopupEnum.PerfectGameBonus)
            {
                outgoingPopup = Instantiate(_perfectGamePopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }
            else if(popupType == PopupEnum.BustedLanePopup)
            {
                outgoingPopup = Instantiate(_bustedLanePopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }
            else
            {
                outgoingPopup = Instantiate(_perfectRunStreakPopupPrefab).GetComponent<Popup>();
                outgoingPopup.popupType = popupType;
            }

            outgoingPopup._gameplay = Gameplay.instance;
            return outgoingPopup;
        }

        public AnimatedLanePopup BuildAnimatedLanePopup(Deck pAnimatedDeckClone)
        {
            AnimatedLanePopup animatedLanePopup =
                Instantiate(_animatedLanePopupPrefab).GetComponent<AnimatedLanePopup>();

            var displayDeck = DisplayDeckFactory.instance.Build(pAnimatedDeckClone, "tempAnimatedDeck");
            displayDeck.DeckDisplayOptions = DisplayDeck.DisplayOptions.Down;
            displayDeck.Deck = pAnimatedDeckClone;
            displayDeck.transform.SetParent(animatedLanePopup.transform);
            // turn off collider so animated decks can't be clicked on
            displayDeck.Interactible = false;

            animatedLanePopup.AnimatedDisplayDeck = displayDeck;

            return animatedLanePopup;
        }
    }
}