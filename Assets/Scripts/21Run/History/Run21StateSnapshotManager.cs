using System;
using System.Collections;
using FRG.Taco.Run21;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Manager for recording and switching between current and previous score/deck layouts for a given <see cref="Run21"/> game.
/// Each state is saved as a <see cref="Run21StateSnapshot"/> instance.
/// Each <see cref="Run21StateSnapshot"/> instance stores the given <see cref="Run21Score"/> and all deck layouts.
/// Use <see cref="UndoLastMove"/> to reset current state to previous state.
/// </summary>
namespace FRG.Taco
{
    public class Run21StateSnapshotManager
    {
        public Gameplay gameplay;

        /// <summary>
        /// Used to perform undo animation for last lane played (which will be undone)
        /// </summary>
        private DisplayDeck lastLanePlayedAnimDeck;

        public Run21StateSnapshotManager(Gameplay gameplay)
        {
            this.gameplay = gameplay;
        }

        /// <summary>
        /// Current game state is stored here.
        /// </summary>
        private Run21StateSnapshot _currentSnapshot;

        /// <summary>
        /// Previous game state is stored here. 
        /// </summary>
        private Run21StateSnapshot _previousSnapshot;

        [CanBeNull]
        public Run21StateSnapshot PreviousSnapshot
        {
            get { return _previousSnapshot; }
            set { _previousSnapshot = value; }
        }

        public Run21StateSnapshot CurrentSnapshot
        {
            get { return _currentSnapshot; }
            set { _currentSnapshot = value; }
        }

        /// <summary>
        ///  Indicates that animated undo of cleared lane is complete
        /// </summary>
        bool isClearedLaneUndoAnimationDone;

        /// <summary>
        ///  Indicates that the last played card has been animated back to the active deck during undo.
        /// </summary>
        bool isLastPlayedCardUndoAnimationDone;

        /// <summary>
        ///  Indicates that the current active card has been animated back to the draw deck during undo.
        /// </summary>
        bool isActiveCardToDrawDeckAnimationDone;

        private bool _isUndoLastMoveInProgress;

        public bool IsUndoLastMoveInProgress
        {
            get { return _isUndoLastMoveInProgress; }
            set { _isUndoLastMoveInProgress = value; }
        }

        /// <summary>
        /// Used to undo game state by replacing currentSnapshot with previousSnapshot
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public bool UndoLastMoveAnimated(Run21.Run21 game)
        {
            IsUndoLastMoveInProgress = true;


            if (!IsUndoLastMoveAvailable())
            {
                IsUndoLastMoveInProgress = false;
                return false;
            }

            gameplay.SkipCardDrawBecauseOfUndoLast = true;

            ClearPopupsAndOutlines();

            RestoreNonAnimatedDecks(game);

            AudioManager.instance.PlaySound(AudioManager.Sound.Undo);

            RestoreAnimatedDecksThenCall(game, () =>
            {
                UndoScores(game);
                ClearAndSwapSnapshots();
                IsUndoLastMoveInProgress = false;

//                Debug.Log($@" DRAW DECK AFTER UNDO METHOD FINISHED:
//                    game draw (logic deck) size  => {game.DrawDeck.Cards.Count}
//                    game draw (top card)         => {game.DrawDeck.TopCard}
//                    gameplay draw deck size      => {gameplay.DrawDeck.Cards.Count}
//                    gameplay draw (logic deck)   => {gameplay.DrawDeck.Deck.TopCard}
//                    gameplay draw (display deck) => {gameplay.DrawDeck.TopCard}");
//                
//                Debug.Log(
//                    $@" ACTIVE DECK AFTER UNDO METHOD FINISHED:
//                            game active deck size          => {game.ActiveCardDeck.Cards.Count}
//                            game active card (logic)       => {game.ActiveCardDeck.TopCard}
//                            gameplay active deck size      => {gameplay.activeDeck.Cards.Count}
//                            gameplay active (logic deck)   => {gameplay.activeDeck.Deck.TopCard}
//                            gameplay active (display deck) => {gameplay.activeDeck.TopCard}");
//                
            });

            return true;
        }

        public bool UndoLastMoveNonAnimated(Run21.Run21 game)
        {
            IsUndoLastMoveInProgress = true;
            if (!IsUndoLastMoveAvailable())
            {
                IsUndoLastMoveInProgress = false;
                return false;
            }

            ClearPopupsAndOutlines();

            RestoreNonAnimatedDecks(game); // restore decks which dont require animation

            game.LaneDecks[_currentSnapshot.PlayedLaneIndex] = PreviousSnapshot.GetLaneDeckByIndex(_currentSnapshot.PlayedLaneIndex).Clone();

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySound(AudioManager.Sound.Undo);
            }

            UndoScores(game);
            ClearAndSwapSnapshots();
            IsUndoLastMoveInProgress = false;

            return true;
        }

        private void RestoreNonAnimatedDecks(Run21.Run21 game)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i != _currentSnapshot.PlayedLaneIndex)
                {
                    game.LaneDecks[i] = PreviousSnapshot.GetLaneDeckByIndex(i).Clone(); // restore deck
                }
            }
        }

        private void ClearPopupsAndOutlines()
        {
            if (gameplay != null)
            {
                gameplay.StopAllCoroutines();
                gameplay.popupManager.ToggleOffAllPopups();
                gameplay.ClearLaneOutlines();
            }
        }

        public void UndoScores(Run21.Run21 game)
        {
            game.RemainingCards = PreviousSnapshot.RemainingCards;
            game.BustedCardCount = PreviousSnapshot.BustedCardCount;

            // restore score
            game.Score = PreviousSnapshot.Score;
            game.ScoredStreak = PreviousSnapshot.ScoredStreak;
        }

        private void ClearAndSwapSnapshots()
        {
            // cleanup animation state
            if (lastLanePlayedAnimDeck != null)
            {
                lastLanePlayedAnimDeck.DestroyDeck();
            }


            isClearedLaneUndoAnimationDone = false;
            isActiveCardToDrawDeckAnimationDone = false;
            isLastPlayedCardUndoAnimationDone = false;

            // swap snapshots
            _currentSnapshot = (Run21StateSnapshot) _previousSnapshot.Clone();
            _previousSnapshot = null;

            gameplay.ToggleOffUndoLastMove();
        }

        /// <summary>
        /// Takes snapshot of current score and deck layout.
        /// </summary>
        /// <param name="game"></param>
        public void TakeSnapshot(Run21.Run21 game)
        {
            if (_currentSnapshot == null)
            {
                _currentSnapshot = Run21StateSnapshot.From(game);
            }
            else
            {
                _previousSnapshot = (Run21StateSnapshot) _currentSnapshot.Clone();
                _currentSnapshot = Run21StateSnapshot.From(game);
            }
        }

        /// <summary>
        /// Check if undoing last move is possible.
        /// </summary>
        /// <returns></returns>
        public bool IsUndoLastMoveAvailable()
        {
            if (_currentSnapshot == null || _previousSnapshot == null)
            {
                return false;
            }

            return true;
        }

        public IEnumerator ExecuteActionWhenConditionIsTrueCoroutine(Func<bool> waitUntilConditionTrueAction, Action callback)
        {
            yield return new WaitUntil(() => waitUntilConditionTrueAction.Invoke());
            callback.Invoke();
        }

        private void RestoreAnimatedDecksThenCall(Run21.Run21 game, Action postAnimationLogicToExecute)
        {
            // LAST LANE PLAYED
            int laneIndexBeingUndone = _currentSnapshot.PlayedLaneIndex;

            // THE DECK BEING UNDONE
            DisplayDeck displayDeckBeingUndone = gameplay.GetLaneDeckByIndex(laneIndexBeingUndone);
            Deck logicDeckBeingUndone = game.LaneDecks[laneIndexBeingUndone];

            // IF LAST LANE PLAYED WAS CLEARED, THEN RECREATE IT ANIMATED
            bool wasLaneCleared = displayDeckBeingUndone.Deck.IsEmpty;

            if (wasLaneCleared)
            {
                DisplayCard lastPlayedCard = DisplayCardFactory.instance.Build(_previousSnapshot.DrawDeck.TopCard);
                lastPlayedCard.Flip(true, 0);

                Deck clearedDeck = _previousSnapshot.GetLaneDeckByIndex(laneIndexBeingUndone).Clone();
                clearedDeck.PutTopCard(lastPlayedCard.Card);

                // now we have the deck that was scored/busted
                lastLanePlayedAnimDeck = DisplayDeckFactory.instance.Build(clearedDeck.ReverseCards(), "tempUndoAnimationDeck");
                gameplay.undoLastMoveAnimationDeckParent.gameObject.SetActive(true);
                lastLanePlayedAnimDeck.transform.SetParent(gameplay.undoLastMoveAnimationDeckParent.transform, false);

                // clear the deck we are animating onto
                logicDeckBeingUndone.Clear();
                displayDeckBeingUndone.RemoveAllCards();
            }

            // CURRENT ACTIVE CARD
            DisplayCard activeCard = gameplay.activeDeck.TakeTopCard(true);
            game.ActiveCardDeck.Clear();
            gameplay.activeDeck.RemoveAllCards();


            // EXECUTE ALL ANIMATIONS IN PREDEFINED ORDER
            if (wasLaneCleared)
            {
                //1. UNDO LANE DECK
                lastLanePlayedAnimDeck.DealTowardsDeckAnimated(
                    Run21Data.Instance.animationConfig.SingleCardMovingDurationFromAnimationDeckToClearedLane,
                    Run21Data.Instance.animationConfig.PauseBetweenDealingCardsFromAnimationDeckToClearedLane,
                    displayDeckBeingUndone,
                    () =>
                    {
                        game.LaneDecks[laneIndexBeingUndone] = PreviousSnapshot.GetLaneDeckByIndex(laneIndexBeingUndone).Clone();
                        gameplay.undoLastMoveAnimationDeckParent.gameObject.SetActive(false);
                        isClearedLaneUndoAnimationDone = true;
                        MoveActiveToDrawDeckAndLastPlayedToActiveDeck(game, activeCard, displayDeckBeingUndone);
                    });
            }
            else
            {
                MoveActiveToDrawDeckAndLastPlayedToActiveDeck(game, activeCard, displayDeckBeingUndone);
                isClearedLaneUndoAnimationDone = true; // no clared lane was undone, but mark it as done
            }


            // invoke post animation callback when all animations are done
            gameplay.StartCoroutine(ExecuteActionWhenConditionIsTrueCoroutine(
                () =>
                {
                    if (isClearedLaneUndoAnimationDone && isActiveCardToDrawDeckAnimationDone && isLastPlayedCardUndoAnimationDone)
                    {
                        return true;
                    }

                    return false;
                },
                () => { postAnimationLogicToExecute.Invoke(); }));
        }

        private void MoveActiveToDrawDeckAndLastPlayedToActiveDeck(Run21.Run21 game, DisplayCard activeCard, DisplayDeck displayDeckBeingUndone)
        {
            if (activeCard != null)
            {
                //2. MOVE ACTIVE CARD TO DRAW DECK 
                activeCard.MoveTowardsAnimated(
                    gameplay.DrawDeck.GetCardPosition_World(gameplay.DrawDeck.Cards.Count),
                    Quaternion.Euler(0f, 180f, 0f),
                    Run21Data.Instance.animationConfig.ActiveCardToDrawDeckAnimationDuration,
                    () =>
                    {
                        // restore draw deck from previous snapshot
                        Deck previousDrawDeck = PreviousSnapshot.DrawDeck.Clone();
                        previousDrawDeck.TakeTopCard();

                        game.DrawDeck = previousDrawDeck;
                        gameplay.DrawDeck.Deck = previousDrawDeck.Clone();
                        activeCard.DestroyCard();

                        isActiveCardToDrawDeckAnimationDone = true;
                    });
            }
            else
            {
                isActiveCardToDrawDeckAnimationDone = true;
            }

            //3. MOVE LAST PLAYED CARD TO ACTIVE DECK
            DisplayCard lastPlayedCard = displayDeckBeingUndone.TakeTopCard(true);
            lastPlayedCard.MoveTowardsAnimated(
                gameplay.activeDeck.GetCardPosition_World(1),
                null,
                Run21Data.Instance.animationConfig.LastPlayedCardToActiveDeckAnimationDuration,
                () =>
                {
                    game.ActiveCardDeck.PutTopCard(lastPlayedCard.Card.Clone());
                    gameplay.activeDeck.PutTopCard(lastPlayedCard); // cannot restore active card from snapshot, use this
                    gameplay.activeDeck.RecreateDisplay();

                    isLastPlayedCardUndoAnimationDone = true;
                });
        }
    }
}