using System;
using System.Collections;
using System.Collections.Generic;
using FRG.Taco.Run21;
using UnityEngine;
using Object = System.Object;

namespace FRG.Taco
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private Gameplay _gameplay;

        [SerializeField] private RectTransform _nonLanePopupSpawnPoint;
        [SerializeField] private GameEventQueue lane1Queue;
        [SerializeField] private GameEventQueue lane2Queue;
        [SerializeField] private GameEventQueue lane3Queue;
        [SerializeField] private GameEventQueue lane4Queue;
        [SerializeField] private GameEventQueue gameOverQueue;
        [SerializeField] private GameObject bustScoreImage;

        private List<Popup> _popupList = new List<Popup>();
        private List<AnimatedLanePopup> _animatedLanePopups = new List<AnimatedLanePopup>();
        private int comboPoints;

        void Start()
        {
            _gameplay.run21.GameOverEvent += OnGameOverEvent;
        }


        public void OnScoreEvent(ScoreEvent scoreEvent)
        {
            if (!scoreEvent.IsScoreAnimated())
            {
                return;
            }

            if (scoreEvent.IsValue21 || scoreEvent.IsFiveCardsScore || scoreEvent.ContainsBlackJack || scoreEvent.IsBust)
            {
                ToggleBustedDeckAnimation(scoreEvent.LaneIndex, scoreEvent.deck); // not a popup
                ToggleClearedDeckAnimation(scoreEvent.LaneIndex, scoreEvent.deck); // not a popup
            }

            GetEventQueueForLane(scoreEvent.LaneIndex).EnqueueEvent(scoreEvent);
        }

        public void OnGameOverEvent(GameOverEvent gameOverEvent)
        {
            gameOverQueue.EnqueueEvent(gameOverEvent);
        }

        /// <summary>
        /// Depending on current score, displays corresponding popup above lane <see param="scoreEvent"/> 
        /// </summary>
        /// <param name="scoreEvent"></param>
        /// <returns></returns>
        public IEnumerator GetScorePopupsCoroutine(ScoreEvent scoreEvent, Action callback = null)
        {
            Popup popupLaneBust = null;
            Popup popup21 = null;
            Popup popupFiveCards = null;
            Popup popupBlackJack = null;
            Popup popupForStreaks = null;
            Popup popupCombos = null;

            // create all popups which need to be displayed
            if (scoreEvent.IsValue21)
            {
                popup21 = Get21RunPopup(scoreEvent.LaneIndex);
            }

            if (scoreEvent.IsBust)
            {
                popupLaneBust = GetBustedLanePopup(scoreEvent.LaneIndex);
            }

            if (scoreEvent.IsFiveCardsScore)
            {
                popupFiveCards = Get5CardPopup(scoreEvent.LaneIndex);
            }

            if (scoreEvent.ContainsBlackJack)
            {
                popupBlackJack = GetBlackjackPopup(scoreEvent.LaneIndex);
            }

            if (scoreEvent.IsValue21 || scoreEvent.IsFiveCardsScore || scoreEvent.ContainsBlackJack)
            {
                popupForStreaks = ResolveStreakPopupByCount(_gameplay.run21.ScoredStreak, scoreEvent.LaneIndex);
            }

            // try and display popups

            if (popupCombos != null)
            {
                _popupList.Add(popupCombos);
                popup21.OnToggledOff += OnPopupToggledOff;
                popup21.gameObject.SetActive(true);
                popup21.TogglePopupOn();
                yield return new WaitForSeconds(Run21Data.Instance.animationConfig.UpwardMotionDurationCombos);
            }

            if (popup21 != null)
            {
                _popupList.Add(popup21);
                popup21.OnToggledOff += OnPopupToggledOff;
                AudioManager.instance.PlaySound(AudioManager.Sound.StackComplete);
                popup21.gameObject.SetActive(true);
                popup21.TogglePopupOn();
                yield return new WaitForSeconds(Run21Data.Instance.animationConfig.upwardMotionDurationRun21);
            }

            if (popupLaneBust != null)
            {
                _popupList.Add(popupLaneBust);
                popupLaneBust.OnToggledOff += OnPopupToggledOff;

                popupLaneBust.gameObject.SetActive(true);
                AudioManager.instance.PlaySound(AudioManager.Sound.Bust);
                popupLaneBust.TogglePopupOn();
                yield return new WaitForSeconds(Run21Data.Instance.animationConfig.upwardMotionDurationBustedLane);
            }

            if (popupFiveCards != null)
            {
                _popupList.Add(popupFiveCards);
                popupFiveCards.OnToggledOff += OnPopupToggledOff;
                AudioManager.instance.PlaySound(AudioManager.Sound.FiveCards);
                popupFiveCards.gameObject.SetActive(true);
                popupFiveCards.TogglePopupOn(_gameplay.run21.Score.scoring.FiveCardBonus);
                yield return new WaitForSeconds(Run21Data.Instance.animationConfig.upwardMotionDuration5Card);
            }

            if (popupBlackJack != null)
            {
                _popupList.Add(popupBlackJack);
                popupBlackJack.OnToggledOff += OnPopupToggledOff;
                AudioManager.instance.PlaySound(AudioManager.Sound.BlackJack);
                popupBlackJack.gameObject.SetActive(true);
                popupBlackJack.TogglePopupOn(_gameplay.run21.Score.scoring.BlackJackBonus);
                yield return new WaitForSeconds(Run21Data.Instance.animationConfig.upwardMotionDurationWildcard);
            }

            if (popupForStreaks != null)
            {
                _popupList.Add(popupForStreaks);
                popupForStreaks.OnToggledOff += OnPopupToggledOff;

                int streakSoundIndex;
                if (popupForStreaks.popupType == PopupFactory.PopupEnum.GoodRunStreak)
                {
                    streakSoundIndex = 0;
                }
                else if (popupForStreaks.popupType == PopupFactory.PopupEnum.GreatRunStreak)
                {
                    streakSoundIndex = 1;
                }
                else if (popupForStreaks.popupType == PopupFactory.PopupEnum.AmazingRunStreak)
                {
                    streakSoundIndex = 2;
                }
                else if (popupForStreaks.popupType == PopupFactory.PopupEnum.OutstandingRunStreak)
                {
                    streakSoundIndex = 3;
                }
                else
                {
                    streakSoundIndex = 4;
                }

                AudioManager.instance.PlaySound(AudioManager.Sound.Streak, (AudioManager.Streak) streakSoundIndex);
                popupForStreaks.gameObject.SetActive(true);
                popupForStreaks.TogglePopupOn();
                yield return new WaitForSeconds(Run21Data.Instance.animationConfig.upwardMotionDurationStreak);
            }

            // show combo popup

            if (scoreEvent.ContainsBlackJack && scoreEvent.IsFiveCardsScore && scoreEvent.IsValue21)
            {
                comboPoints = Run21Data.Instance.comboPoints.TwentyoneFiveCardBlackjack;
            }
            else if (scoreEvent.IsFiveCardsScore && scoreEvent.IsValue21)
            {
                comboPoints = Run21Data.Instance.comboPoints.TwentyoneFiveCard;
            }
            else if (scoreEvent.ContainsBlackJack && scoreEvent.IsFiveCardsScore)
            {
                comboPoints = Run21Data.Instance.comboPoints.FiveCardBlackjack;
            }
            else if (scoreEvent.ContainsBlackJack && scoreEvent.IsValue21)
            {
                comboPoints = Run21Data.Instance.comboPoints.TwentyoneBlackjack;
            }
            else
            {
                comboPoints = 0;
            }

            if (comboPoints != 0)
            {
                popupCombos = GetComboPopup(scoreEvent.LaneIndex);
                _popupList.Add(popupCombos);
                popupCombos.OnToggledOff += OnPopupToggledOff;
                AudioManager.instance.PlaySound(AudioManager.Sound.Combo);
                popupCombos.gameObject.SetActive(true);
                popupCombos.TogglePopupOn();
                yield return new WaitForSeconds(Run21Data.Instance.animationConfig.UpwardMotionDurationCombos);
            }
            


            if (callback != null)
            {
                callback.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// Display logic for game over popups.
        /// </summary>
        /// <param name="gameOverEvent"></param>
        /// <returns></returns>
        public IEnumerator GetGameOverPopupsCoroutine(GameOverEvent gameOverEvent, Action callback = null)
        {
            // wait until all ScoreEvents are processed
            yield return new WaitUntil(() => lane1Queue.AreAllEventsProcessed() && lane2Queue.AreAllEventsProcessed() && lane3Queue.AreAllEventsProcessed() && lane4Queue.AreAllEventsProcessed());

            if (gameOverEvent.IsTimeExpired)
            {
                Popup popup = GetOutOfTimePopup();
                _popupList.Add(popup);
                popup.OnToggledOff += OnPopupToggledOff;
                popup.TogglePopupOn();
                AudioManager.instance.PlaySound(AudioManager.Sound.OutOfTime);

                yield return new WaitForSeconds(Run21Data.Instance.animationConfig.timeTillNextPopup);
            }

            for (int laneIndex = 0; laneIndex < gameOverEvent.EmptyLanes.Length; laneIndex++)
            {
                if (gameOverEvent.EmptyLanes[laneIndex])
                {
                    Popup popup = GetEmptyLaneBonus(laneIndex);
                    _popupList.Add(popup);
                    popup.OnToggledOff += OnPopupToggledOff;
                    popup.TogglePopupOn();
                }

                if (laneIndex == 3)
                {
                    yield return new WaitForSeconds(Run21Data.Instance.animationConfig.timeTillNextPopup);
                }
            }

            if (gameOverEvent.BustCount == 0)
            {
                Popup popup = GetNoBustBonusPopup();
                _popupList.Add(popup);
                popup.OnToggledOff += OnPopupToggledOff;
                popup.TogglePopupOn();

                yield return new WaitForSeconds(Run21Data.Instance.animationConfig.timeTillNextPopup);
            }

            if (gameOverEvent.PerfectScore)
            {
                Popup popup = GetPerfectGamePopup();
                _popupList.Add(popup);
                popup.OnToggledOff += OnPopupToggledOff;
                popup.TogglePopupOn();
            }


            if (callback != null)
            {
                callback.Invoke();
            }
        }

        public Popup GetComboPopup(int pLaneIndex)
        {
            Popup _popupCombos = PopupFactory.instance.Build(PopupFactory.PopupEnum.CombosPopup);
            _popupCombos.transform.SetParent(GetLaneScoreByIndex(pLaneIndex).transform.parent, false);
            _popupCombos.Score = comboPoints;
            _popupCombos.gameObject.SetActive(false);
            return _popupCombos;
        }

        public Popup Get21RunPopup(int pLaneIndex)
        {
            Popup _popup21 = PopupFactory.instance.Build(PopupFactory.PopupEnum.Run21Score);
            _popup21.transform.SetParent(GetLaneScoreByIndex(pLaneIndex).transform.parent, false);
            _popup21.Score = _gameplay.run21.Score.scoring.Run21Bonus;
            _popup21.gameObject.SetActive(false);
            return _popup21;
        }

        public Popup GetBustedLanePopup(int pLaneIndex)
        {
            Popup _popupBustedLane = PopupFactory.instance.Build(PopupFactory.PopupEnum.BustedLanePopup);
            _popupBustedLane.bustScoreImage = bustScoreImage;
            _popupBustedLane.transform.SetParent(GetLaneScoreByIndex(pLaneIndex).transform.parent, false);
            _popupBustedLane.PopupHasScore = false;
            _popupBustedLane.gameObject.SetActive(false);
            return _popupBustedLane;
        }

        public Popup Get5CardPopup(int pLaneIndex)
        {
            Popup _popup5Cards = PopupFactory.instance.Build(PopupFactory.PopupEnum.FiveCardScore);
            _popup5Cards.transform.SetParent(GetLaneScoreByIndex(pLaneIndex).transform.parent, false);
            _popup5Cards.Score = _gameplay.run21.Score.scoring.FiveCardBonus;
            _popup5Cards.gameObject.SetActive(false);
            return _popup5Cards;
        }

        public Popup GetBlackjackPopup(int pLaneIndex)
        {
            Popup _popupBlackjack = PopupFactory.instance.Build(PopupFactory.PopupEnum.BlackjackScore);
            _popupBlackjack.transform.SetParent(GetLaneScoreByIndex(pLaneIndex).transform.parent, false);
            _popupBlackjack.Score = _gameplay.run21.Score.scoring.BlackJackBonus;
            _popupBlackjack.gameObject.SetActive(false);
            return _popupBlackjack;
        }

        public Popup GetGoodRunPopup(int pLaneIndex)
        {
            Popup _popupGoodRun = PopupFactory.instance.Build(PopupFactory.PopupEnum.GoodRunStreak);
            _popupGoodRun.transform.SetParent(GetLaneScoreByIndex(pLaneIndex).transform.parent, false);
            _popupGoodRun.Score = _gameplay.run21.Score.scoring.StreakBaseBonus;
            _popupGoodRun.gameObject.SetActive(false);
            return _popupGoodRun;
        }

        public Popup GetGreatRunPopup(int pLaneIndex)
        {
            Popup _popupGreatRun = PopupFactory.instance.Build(PopupFactory.PopupEnum.GreatRunStreak);
            _popupGreatRun.transform.SetParent(GetLaneScoreByIndex(pLaneIndex).transform.parent, false);
            _popupGreatRun.Score = 2 * _gameplay.run21.Score.scoring.StreakBaseBonus;
            _popupGreatRun.gameObject.SetActive(false);
            return _popupGreatRun;
        }

        public Popup GetAmazingRunPopup(int pLaneIndex)
        {
            Popup _popupAmazingRun = PopupFactory.instance.Build(PopupFactory.PopupEnum.AmazingRunStreak);
            _popupAmazingRun.transform.SetParent(GetLaneScoreByIndex(pLaneIndex).transform.parent, false);
            _popupAmazingRun.Score = 3 * _gameplay.run21.Score.scoring.StreakBaseBonus;
            _popupAmazingRun.gameObject.SetActive(false);
            return _popupAmazingRun;
        }

        public Popup GetOutstandingRunPopup(int pLaneIndex)
        {
            Popup _popupOutstandingRun = PopupFactory.instance.Build(PopupFactory.PopupEnum.OutstandingRunStreak);
            _popupOutstandingRun.transform.SetParent(GetLaneScoreByIndex(pLaneIndex).transform.parent, false);
            _popupOutstandingRun.Score = 4 * _gameplay.run21.Score.scoring.StreakBaseBonus;
            _popupOutstandingRun.gameObject.SetActive(false);
            return _popupOutstandingRun;
        }

        public Popup GetPerfectRunPopup()
        {
            Popup _popupPerfectRun = PopupFactory.instance.Build(PopupFactory.PopupEnum.PerfectRunStreak);
            _popupPerfectRun.transform.SetParent(_nonLanePopupSpawnPoint.transform, false);
            _popupPerfectRun.transform.localPosition = new Vector3(0, 0, -10);
            _popupPerfectRun.IsNonLanePopup = true;
            _popupPerfectRun.Score = 5 * _gameplay.run21.Score.scoring.StreakBaseBonus;
            _popupPerfectRun.gameObject.SetActive(false);
            return _popupPerfectRun;
        }

        public Popup GetOutOfTimePopup()
        {
            Popup _outOfTimePopup = PopupFactory.instance.Build(PopupFactory.PopupEnum.OutOfTime);
            _outOfTimePopup.transform.SetParent(_nonLanePopupSpawnPoint.transform, false);
            _outOfTimePopup.transform.localPosition = new Vector3(0, 0, -10);
            _outOfTimePopup.gameObject.SetActive(true);
            _outOfTimePopup.PopupHasScore = false;
            _outOfTimePopup.IsNonLanePopup = true;
            return _outOfTimePopup;
        }

        public Popup GetEmptyLaneBonus(int pLaneIndex)
        {
            AudioManager.instance.PlaySound(AudioManager.Sound.LanePopup);
            Popup _emptyLaneBonusPopup = PopupFactory.instance.Build(PopupFactory.PopupEnum.EmptyLaneBonus);
            _emptyLaneBonusPopup.transform.SetParent(GetLaneScoreByIndex(pLaneIndex).transform.parent, false);
            _emptyLaneBonusPopup.gameObject.SetActive(true);
            _emptyLaneBonusPopup.PopupHasScore = false;
            return _emptyLaneBonusPopup;
        }

        public Popup GetNoBustBonusPopup()
        {
            Popup _noBustBonusPopup = PopupFactory.instance.Build(PopupFactory.PopupEnum.NoBustBonus);
            _noBustBonusPopup.transform.SetParent(_nonLanePopupSpawnPoint.transform, false);
            _noBustBonusPopup.transform.localPosition = new Vector3(0, 0, -10);
            _noBustBonusPopup.gameObject.SetActive(true);
            _noBustBonusPopup.PopupHasScore = false;
            _noBustBonusPopup.IsNonLanePopup = true;
            return _noBustBonusPopup;
        }

        public Popup GetPerfectGamePopup()
        {
            Popup _perfectGamePopup = PopupFactory.instance.Build(PopupFactory.PopupEnum.PerfectGameBonus);
            _perfectGamePopup.transform.SetParent(_nonLanePopupSpawnPoint.transform, false);
            _perfectGamePopup.transform.localPosition = new Vector3(0, 0, -10);
            _perfectGamePopup.gameObject.SetActive(true);
            _perfectGamePopup.PopupHasScore = false;
            _perfectGamePopup.IsNonLanePopup = true;
            return _perfectGamePopup;
        }

        public Popup ResolveStreakPopupByCount(int streakCount, int pLaneIndex)
        {
            Popup streakPopup = null;
            if (streakCount <= 1)
            {
                return streakPopup;
            }

            if (streakCount == 2)
            {
                return GetGoodRunPopup(pLaneIndex);
            }

            if (streakCount == 3)
            {
                return GetGreatRunPopup(pLaneIndex);
            }

            if (streakCount == 4)
            {
                return GetAmazingRunPopup(pLaneIndex);
            }

            if (streakCount == 5)
            {
                return GetOutstandingRunPopup(pLaneIndex);
            }

            if (streakCount == 6)
            {
                return GetPerfectRunPopup();
            }

            return null;
        }

        public void ToggleBustedDeckAnimation(int pLaneIndex, Deck bustedDeckClone)
        {
            var laneDeck = Gameplay.instance.GetLaneDeckByIndex(pLaneIndex);
            AnimatedLanePopup _animatedLanePopup = PopupFactory.instance.BuildAnimatedLanePopup(bustedDeckClone);
            _animatedLanePopup.transform.SetParent(laneDeck.parentOfCards.transform, false);
//            _animatedLanePopups.Add(_animatedLanePopup); 
            _animatedLanePopup.OnToggledOff += OnPopupToggledOff;
            _animatedLanePopup.ToggleBustedLanePopupOn();
        }

        public void ToggleClearedDeckAnimation(int pLaneIndex, Deck bustedDeckClone)
        {
            var laneDeck = Gameplay.instance.GetLaneDeckByIndex(pLaneIndex);
            AnimatedLanePopup _animatedLanePopup = PopupFactory.instance.BuildAnimatedLanePopup(bustedDeckClone);
            _animatedLanePopup.transform.SetParent(laneDeck.parentOfCards.transform, false);
//            _animatedLanePopups.Add(_animatedLanePopup);
            _animatedLanePopup.OnToggledOff += OnPopupToggledOff;
            _animatedLanePopup.ToggleScoredLanePopupOn();
        }

        private LaneScore GetLaneScoreByIndex(int laneIndex)
        {
            if (laneIndex == 0)
            {
                return _gameplay.score1;
            }

            if (laneIndex == 1)
            {
                return _gameplay.score2;
            }

            if (laneIndex == 2)
            {
                return _gameplay.score3;
            }

            if (laneIndex == 3)
            {
                return _gameplay.score4;
            }

            throw new ArgumentException($"Cannot resolve LaneScore for index :{laneIndex}");
        }

        public void ToggleOffAllPopups()
        {
            foreach (var popup in _popupList.ToArray())
            {
                popup?.TogglePopupOff();
            }

            _popupList.Clear();


            foreach (var popup in _animatedLanePopups.ToArray())
            {
                if (popup != null)
                {
                    Destroy(popup.transform.gameObject);
                }
            }

            _animatedLanePopups.Clear();
        }

        public Coroutine TogglePopupsForEvent(GameEvent _event, Action callback = null)
        {
            if (_event is ScoreEvent)
            {
                return StartCoroutine(GetScorePopupsCoroutine(_event as ScoreEvent, callback));
            }


            if (_event is GameOverEvent)
            {
                return StartCoroutine(GetGameOverPopupsCoroutine(_event as GameOverEvent, callback));
            }


            throw new ArgumentException($"Cannot toggle popups for {_event}");
        }


        private GameEventQueue GetEventQueueForLane(int laneIndex)
        {
            if (laneIndex < 0 || laneIndex > 3)
            {
                throw new ArgumentException("Invalid lane index");
            }

            switch (laneIndex)
            {
                case 0:
                    return lane1Queue;
                case 1:
                    return lane2Queue;
                case 2:
                    return lane3Queue;
                default:
                    return lane4Queue;
            }
        }

        private void TriggerGameOverEventForTesting()
        {
            var goe = new GameOverEvent();
            goe.EmptyLanes = new[] {true, true, true, true};
            goe.BustCount = 0;
            goe.PerfectScore = true;
            goe.IsTimeExpired = true;
            gameOverQueue.EnqueueEvent(goe);
        }

        private void OnPopupToggledOff(Object popup)
        {
            if (popup is Popup)
            {
//                Debug.Log($"OnPopupToggledOff BEFORE Remove popup from popuplist: {_popupList.Count}");
                _popupList.Remove(popup as Popup);
//                Debug.Log($"OnPopupToggledOff AFTER Remove popup from popuplist: {_popupList.Count}");
                return;
            }

            if (popup is AnimatedLanePopup)
            {
//                Debug.Log($"OnPopupToggledOff BEFORE Remove animated lane popup from popuplist: {_animatedLanePopups.Count}");
                _animatedLanePopups.Remove(popup as AnimatedLanePopup);
//                Debug.Log($"OnPopupToggledOff AFTER Remove animated lane popup from popuplist: {_animatedLanePopups.Count}");
            }
        }

        public bool AreAllPopupsProcessed()
        {
            return
                lane1Queue.AreAllEventsProcessed() &&
                lane2Queue.AreAllEventsProcessed() &&
                lane3Queue.AreAllEventsProcessed() &&
                lane4Queue.AreAllEventsProcessed() &&
                gameOverQueue.AreAllEventsProcessed() &&
                _popupList.Count == 0;
        }
    }
}