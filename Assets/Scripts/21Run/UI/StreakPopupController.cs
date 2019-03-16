using FRG.Taco.Run21;
using UnityEngine;

namespace FRG.Taco
{
    public class StreakPopupController : MonoBehaviour
    {
        private Popup popupGoodRun;
        private Popup popupGreatRun;
        private Popup popupAmazingRun;
        private Popup popupOutstandingRun;
        private Popup popupPerfectRun;
        [SerializeField] private int laneIndex;
        [SerializeField] private Gameplay _gameplay;

        public void ToggleGoodRunPopup()
        {
            popupGoodRun = PopupFactory.instance.Build(PopupFactory.PopupEnum.GoodRunStreak);
            popupGoodRun.transform.parent = transform;
            popupGoodRun.gameObject.SetActive(true);
            popupGoodRun.TogglePopupOn(_gameplay.run21.Score.scoring.StreakBaseBonus);
        }

        public void ToggleGreatRunPopup()
        {
            popupGreatRun = PopupFactory.instance.Build(PopupFactory.PopupEnum.GreatRunStreak);
            popupGreatRun.transform.parent = transform;
            popupGreatRun.gameObject.SetActive(true);
            popupGreatRun.TogglePopupOn(2 * _gameplay.run21.Score.scoring.StreakBaseBonus);
        }

        public void ToggleAmazingRunPopup()
        {
            popupAmazingRun = PopupFactory.instance.Build(PopupFactory.PopupEnum.AmazingRunStreak);
            popupAmazingRun.transform.parent = transform;
            popupAmazingRun.gameObject.SetActive(true);
            popupAmazingRun.TogglePopupOn(3 * _gameplay.run21.Score.scoring.StreakBaseBonus);
        }

        public void ToggleOutstandingRunPopup()
        {
            popupOutstandingRun = PopupFactory.instance.Build(PopupFactory.PopupEnum.OutstandingRunStreak);
            popupOutstandingRun.transform.parent = transform;
            popupOutstandingRun.gameObject.SetActive(true);
            popupOutstandingRun.TogglePopupOn(4 * _gameplay.run21.Score.scoring.StreakBaseBonus);
        }

        public void TogglePerfectRunPopup()
        {
            popupPerfectRun = PopupFactory.instance.Build(PopupFactory.PopupEnum.PerfectRunStreak);
            popupPerfectRun.transform.parent = transform;
            popupPerfectRun.gameObject.SetActive(true);
            popupPerfectRun.TogglePopupOn(5 * _gameplay.run21.Score.scoring.StreakBaseBonus);
        }


        private void Start()
        {
            _gameplay = Gameplay.instance;
        }
    }
}