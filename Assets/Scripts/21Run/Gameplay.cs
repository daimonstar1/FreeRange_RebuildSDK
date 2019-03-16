using System;
using System.Collections;
using DarkTonic.MasterAudio;
using FRG.Core;
using GameTaco;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace FRG.Taco.Run21
{
    public class Gameplay : MonoMachine<Gameplay.FSMState>
    {
        [Serializable]
        public class Durations
        {
            public float summaryAppearAfterSeconds = 2f;
            public float summaryLingerForSeconds = 11f;
            public float addPoints = 50f;
            public float scoreTextAnimationDuration = 0.2f;
            public float summaryTimeBonusWaitBetweenAdding = 0.2f;
            public int lastTimeWarning = 5;
            public int firstTimeWarning = 60;
        }

        public static Gameplay instance { get; private set; }

        public Durations durations;

        public enum FSMState
        {
            Init, // on entering Gameplay scene, reset gameplay and wait for everything that we need to be ready (audio etc)
            InitialAnimation, // game is dealing cards to draw deck. happens on init and restart
            Drawing, // after playing a move, card is dealt to empty active deck
            Dragging,  // user has clicked on the ActiveDeck and started dragging the active card
            ReleasedACard,   // user has been dragging a card and dropped it
            AnimatingACard, // card is visually being moved to another position and under a different parent
            PlayingACard, // card is being played in logic
            Idle, // player is thinking
            WaitingForFirstInput, // at start, after dealing to active Deck, we wait 2 seconds here
            HelperHand, // and then display a helpful hand with tips on how to play
            Paused, // player pressed the pause key
            Scoring, // the lane has any of the existing scores (21, 5 cards, blackjack . . .)
            GameOver, // game finished, no Win/Lose states. Either all cards played, or three busts or time out.
            SubmitGameOverScore, // send our results online and go back
            UndoingLastMove, // undoing last move
            PopupAnimating, // popup (score/streak) is being displayed/animated
            Tutorial, // swipeable screens accessible through gameplay 
            PreGameOver,    // After the game finishes we have a few seconds before transitioning into the SummaryScreen
        }

        private void Start()
        {
            buttonSwapToggle.isOn = PlayerPrefsManager.GetIsButtonsSwapped();

            laneOutlines[0] = laneOutlineSpawners[0].Spawn(false).GetComponent<LaneOutline>();
            laneOutlines[1] = laneOutlineSpawners[1].Spawn(false).GetComponent<LaneOutline>();
            laneOutlines[2] = laneOutlineSpawners[2].Spawn(false).GetComponent<LaneOutline>();
            laneOutlines[3] = laneOutlineSpawners[3].Spawn(false).GetComponent<LaneOutline>();

            screenOutline = screenOutlineSpawner.Spawn(false).GetComponent<LaneOutline>();

            ClearLaneOutlines();
        }

        #region SERIALIZED FIELDS

        [SerializeField] public LaneScore score1;
        [SerializeField] public LaneScore score2;
        [SerializeField] public LaneScore score3;
        [SerializeField] public LaneScore score4;
        [SerializeField] public Text mainScore;
        [SerializeField] public DisplayDeck drawDeck;
        [SerializeField] public DisplayDeck lane1;
        [SerializeField] public DisplayDeck lane2;
        [SerializeField] public DisplayDeck lane3;
        [SerializeField] public DisplayDeck lane4;
        [SerializeField] public DisplayDeck activeDeck;
        [SerializeField] GameObject pausePanel;
        [SerializeField] GameObject gameOverPanel;
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] GameObject tutorialScreens;
        [SerializeField] GameObject[] busts;
        [SerializeField] Text timeText;
        [SerializeField] TextMeshProUGUI cardsRemainingValue;
        [SerializeField] Button musicButtonOn;
        [SerializeField] Button musicButtonOff;
        [SerializeField] Button SFXButtonOn;
        [SerializeField] Button SFXButtonOff;
        [SerializeField] Button pauseButton;
        [SerializeField] Button tutorialExitButton;
        [SerializeField] Button tutorialButton;
        [SerializeField] Button resumeButton;
        [SerializeField] Button endGameButton;
        [SerializeField] Button undoButton;
        [SerializeField] GameObject helperHand;
        [SerializeField] public PopupManager popupManager;
        [SerializeField] GameObject activeDeckWildCard;
        [SerializeField] GameObject activeDeckLastCard;
        [SerializeField] DisplayDeck animationDeck;
        [SerializeField] public DisplayDeck undoLastMoveAnimationDeckParent;
        [SerializeField] TextMeshProUGUI text_tournamentPot;
        [SerializeField] Image image_currencyIcon_taco;
        [SerializeField] Image image_currencyIcon_cash;
        [SerializeField] Image image_currencyIcon_practice;
        [SerializeField] Toggle buttonSwapToggle;
        [SerializeField] PoolObjectSpawner screenOutlineSpawner = new PoolObjectSpawner();
        [SerializeField] public PoolObjectSpawner[] laneOutlineSpawners = new PoolObjectSpawner[4];
        private LaneOutline screenOutline = new LaneOutline();
        private LaneOutline[] laneOutlines = new LaneOutline[4];


        FSMState previousState = FSMState.Idle;

        #endregion

        #region PRIVATE FIELDS

        public Run21 run21;

        private bool musicPanelShowing = false;
        private bool requestMusic = false;
        private bool requestGameOver;
        private bool requestDragging;
        private bool requestPlayACard;
        private bool requestTutorial;
        private bool requestPause;
        private bool requestSubmitScore;
        private bool requestUndoLastMove;
        private bool requestPopupAnimating;
        public int laneToDealTo;
        private bool isAnimating = false;
        private DisplayDeck dealDestination;
        private float timeInPreGameOver = 0f;
        private float timeWhenPopupsFinished = -1f;
        private bool willShowHelperHand = true;
        private bool releasedOnLane = false;
        private float firstInputMaxWait = 3f;
        private float timeInWaiting = 0f;
        private float timePlaying = 0f;
        private TutorialPanels tutorialPanels;
        /// <summary>
        /// Indicated that undo last move was performed.
        /// </summary>
        private bool skipCardDrawBecauseOfUndoLast = false;

        // keeping track of currents, to play sound when they change (e.g. new bust, streak . . .)
        private int currentStreak = 0;
        private int previousStreak = 0;
        private int currentBusts = 0;
        private int currentColumnsCleared = 0;
        private bool hasPlayedFinalCountdown = false;
        private bool timerWarnedSoundPlayed = false;

        //time stuff
        private bool hasBeenCountingDown = false; // saving state of countdown BEFORE entering tutorials / pause, returning to it after exiting
        private bool isCountingDown = false;
        private float startTime;
        private float remainingTime;

        private float _gameOverDuration = 0f;

        public float GameOverDuration
        {
            get { return _gameOverDuration; }
        }

        private bool initialShufleAnimationDone;

        private int _summaryScreenDuration = 11;

        public int SummaryScreenDuration
        {
            get { return _summaryScreenDuration; }
            set { _summaryScreenDuration = value; }
        }

        public bool SkipCardDrawBecauseOfUndoLast
        {
            get { return skipCardDrawBecauseOfUndoLast; }
            set { skipCardDrawBecauseOfUndoLast = value; }
        }

        #endregion

        protected override void Ready()
        {
            instance = this;
            durations = Run21Data.Instance.durations;
            base.Ready();
        }

        #region PUBLIC METHODS

        public DisplayDeck DrawDeck
        {
            get { return drawDeck; }
            private set { drawDeck = value; }
        }

        #endregion

        #region PRIVATE METHODS

        private void GoToPlayACard()
        {
            requestPlayACard = true;
        }

        public void TogglePopupAnimatingOn()
        {
            requestPopupAnimating = true;
        }

        public void TogglePopupAnimatingOff()
        {
            requestPopupAnimating = false;
        }

        void PlayCard(DisplayDeck clickedDeck)
        {

            if (isAnimating)
            {
                return;
            }

            requestPlayACard = true;
            if (clickedDeck == lane1)
            {
                laneToDealTo = 0;
                dealDestination = clickedDeck;
            }
            else if (clickedDeck == lane2)
            {
                laneToDealTo = 1;
                dealDestination = clickedDeck;
            }
            else if (clickedDeck == lane3)
            {
                laneToDealTo = 2;
                dealDestination = clickedDeck;
            }
            else if (clickedDeck == lane4)
            {
                laneToDealTo = 3;
                dealDestination = clickedDeck;
            }
            else
            {
                requestPlayACard = false;
            }
        }

        public void TryToUndoLastMove()
        {
            if (run21.IsUndoLastMoveAvailable())
            {
                requestUndoLastMove = true;
            }
        }

        public void ToggleOffUndoLastMove()
        {
            requestUndoLastMove = false;
        }

        /// <summary>
        /// Reset all the objects to starting state. Don't recerate instances since they might be hooked up
        /// to something already.
        /// </summary>
        private void ResetGameplay()
        {
            timePlaying = 0f;
            remainingTime = Run21.PlayTimeMax;
            _gameOverDuration = 0f;
            isCountingDown = false;

            if (TacoSetup.Instance != null && TacoSetup.Instance.IsTournamentPlayed())
            {
                int tournamentId = TacoManager.Target.id;
                //int tournamentGameId = TacoManager.Target.gameId; // Kova TODO find out what is game id
                Debug.Log("Reseting gameplay with draw deck seed: " + tournamentId);
                run21.Reset(tournamentId);
                Debug.Log("Tournament currency type: " + TacoManager.Target.typeCurrency);
                if (TacoManager.Target.typeCurrency == 0)
                {
                    // cash currency?
                    text_tournamentPot.text = TacoManager.FormatCash(TacoManager.Target.prize);
                    image_currencyIcon_cash.gameObject.SetActive(true);
                    image_currencyIcon_taco.gameObject.SetActive(false);
                    image_currencyIcon_practice.gameObject.SetActive(false);
                }
                else if (TacoManager.Target.typeCurrency == 1)
                {
                    // taco currency
                    text_tournamentPot.text = TacoManager.FormatGTokens(TacoManager.Target.prize);
                    image_currencyIcon_cash.gameObject.SetActive(true);
                    image_currencyIcon_taco.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("Unknown taco currency! Copy and report this to a coder. Currency id: " + TacoManager.Target.typeCurrency);
                    // taco currency
                    text_tournamentPot.text = "";
                    image_currencyIcon_cash.gameObject.SetActive(false);
                    image_currencyIcon_taco.gameObject.SetActive(false);
                    image_currencyIcon_practice.gameObject.SetActive(false);
                }
            }
            else
            {
                // offline single player play
                run21.Reset();
                text_tournamentPot.text = "Practice";
                image_currencyIcon_cash.gameObject.SetActive(false);
                image_currencyIcon_taco.gameObject.SetActive(false);
                image_currencyIcon_practice.gameObject.SetActive(true);
            }

            mainScore.text = run21.Score.GameScore.ToString();
            // TODO Kova: use ints
            score1.SetScore("0");
            score2.SetScore("0");
            score3.SetScore("0");
            score4.SetScore("0");
            initialShufleAnimationDone = false;

            ResetDisplayedBustScore(0);
        }


        private void ResetDisplayedBustScore(int bustScore)
        {
            for (int i = 0; i < 3; i++)
            {
                busts[i].SetActive(false);
            }

            if (bustScore < 0 || bustScore > 3)
            {
                return;
            }

            for (int i = 0; i < bustScore; i++)
            {
                busts[i].SetActive(true);
            }
        }

        #endregion

        #region MonoState

        public override void Enter()
        {
            startTime = Run21.PlayTimeMax;
            remainingTime = startTime;

            DisplayDeck.OnClicked += PlayCard;
            DisplayDeck.OnReleased += ReleasedDrag;
            DisplayDeck.OnDown += StartingDrag;
            base.Enter();
        }

        public override void Refresh(float delta)
        {
            base.Refresh(delta);
            if (State != FSMState.SubmitGameOverScore)
            {
                HandleSoundEffects();
            }

            if (isCountingDown)
            {
                timePlaying += delta;
                remainingTime -= delta;
                run21.SetPlayTime(timePlaying);
                run21.Score.PlayTime = Run21.PlayTimeMax - remainingTime;
                FormatTime(remainingTime);
            }
        }

        public override void Exit()
        {
            DisplayDeck.OnClicked -= PlayCard;
            DisplayDeck.OnReleased -= ReleasedDrag;
            DisplayDeck.OnDown -= StartingDrag;
            base.Exit();
        }

        #endregion

        #region MonoMachine
        protected override bool StateTransition()
        {
            // these are outside of the switch since the change may happen from any state
            if (requestGameOver && State != FSMState.GameOver)
            {
                return ChangeState(FSMState.GameOver);
            }

            if (requestTutorial && State != FSMState.Tutorial && State != FSMState.InitialAnimation && State != FSMState.Drawing)
            {
                return ChangeState(FSMState.Tutorial);
            }

            if (requestPause && State != FSMState.Paused && State != FSMState.PreGameOver && State != FSMState.InitialAnimation && State != FSMState.Drawing)
            {
                previousState = State;
                return ChangeState(FSMState.Paused);
            }

            if (run21.IsGameOver && State != FSMState.PreGameOver && State != FSMState.GameOver && State != FSMState.Scoring && State != FSMState.SubmitGameOverScore)
            {
                return ChangeState(FSMState.Scoring);   // routing to scoring before ending game so it displays last score
            }

            if (requestDragging && (State == FSMState.Idle || State == FSMState.WaitingForFirstInput || State == FSMState.HelperHand))
            {
                return ChangeState(FSMState.Dragging);
            }

            switch (State)
            {
                case FSMState.Init:
                    if (IsSystemInitialized())
                    {
                        return ChangeState(FSMState.InitialAnimation);
                    }
                    break;

                case FSMState.InitialAnimation:
                    if (initialShufleAnimationDone)
                    {
                        return ChangeState(FSMState.Drawing);
                    }

                    break;

                case FSMState.Drawing:
                    if (!isAnimating)
                    {
                        if (willShowHelperHand)
                        {
                            return ChangeState(FSMState.WaitingForFirstInput);
                        }
                        else
                        {
                            return ChangeState(FSMState.Idle);
                        }
                    }
                    break;

                case FSMState.Dragging:
                    if (!requestDragging)
                    {
                        return ChangeState(FSMState.ReleasedACard);
                    }

                    break;

                case FSMState.ReleasedACard:
                    if (!isAnimating)
                    {
                        return ChangeState(releasedOnLane ? FSMState.PlayingACard : FSMState.Idle);
                    }
                    break;

                case FSMState.WaitingForFirstInput:
                    if (requestPlayACard)
                    {
                        return ChangeState(FSMState.AnimatingACard);
                    }
                    else if (requestDragging)
                    {
                        return ChangeState(FSMState.Dragging);
                    }
                    else if (timeInWaiting >= firstInputMaxWait)
                    {
                        return ChangeState(FSMState.HelperHand);
                    }

                    break;

                case FSMState.HelperHand:
                    if (requestPlayACard)
                    {
                        return ChangeState(FSMState.AnimatingACard);
                    }
                    else if (requestDragging)
                    {
                        return ChangeState(FSMState.Dragging);
                    }

                    break;

                case FSMState.PreGameOver:
                    if (timeWhenPopupsFinished < 0 && popupManager.AreAllPopupsProcessed())
                    {
                        timeWhenPopupsFinished = timeInPreGameOver;
                    }

                    if (timeWhenPopupsFinished > 0 && popupManager.AreAllPopupsProcessed() && timeInPreGameOver - timeWhenPopupsFinished > durations.summaryAppearAfterSeconds)
                    {

                        return ChangeState(FSMState.GameOver);
                    }

                    break;

                case FSMState.GameOver:
                    if (requestSubmitScore)
                    {
                        return ChangeState(FSMState.SubmitGameOverScore);
                    }

                    break;

                case FSMState.SubmitGameOverScore:
                    break;

                case FSMState.Idle:
                    if (requestPlayACard)
                    {
                        return ChangeState(FSMState.AnimatingACard);
                    }
                    else if (requestDragging)
                    {
                        return ChangeState(FSMState.Dragging);
                    }

                    if (requestUndoLastMove)
                    {
                        if (run21.IsUndoLastMoveAvailable())
                        {
                            return ChangeState(FSMState.UndoingLastMove);
                        }
                    }

                    break;

                case FSMState.AnimatingACard:
                    if (!isAnimating)
                    {
                        return ChangeState(FSMState.PlayingACard);
                    }

                    break;

                case FSMState.PlayingACard:
                    return ChangeState(FSMState.Scoring);

                case FSMState.Paused:
                    if (!requestPause)
                    {
                        return ChangeState(previousState);
                    }

                    break;

                case FSMState.Scoring:
                    if (run21.IsGameOver)
                    {
                        return ChangeState(FSMState.PreGameOver);
                    }
                    else if (requestPopupAnimating)
                    {
                        return ChangeState(FSMState.PopupAnimating);
                    }
                    else
                    {
                        return ChangeState(FSMState.Drawing);
                    }

                case FSMState.UndoingLastMove:
                    if (requestUndoLastMove == false)
                    {
                        //                        Debug.Log("UNDOING LAST MOVE COMPLETED");
                        return ChangeState(FSMState.Scoring);
                    }

                    break;

                case FSMState.PopupAnimating:
                    return ChangeState(FSMState.Scoring);

                case FSMState.Tutorial:
                    if (!requestTutorial)
                    {
                        return ChangeState(previousState);
                    }

                    break;
            }

            return base.StateTransition();
        }

        private bool IsSystemInitialized()
        {
            // wait for master audio init
            return MasterAudio.SafeInstance != null;
        }

        protected override void ClearFramewiseInputs()
        {
            requestPlayACard = false;
            requestSubmitScore = false;
        }

        private void Enter_WaitingForFirstInput()
        {
        }

        private void Refresh_WaitingForFirstInput(float delta)
        {
            timeInWaiting += delta;
        }

        private void Exit_WaitingForFirstInput()
        {
        }

        private void Enter_HelperHand()
        {
            helperHand.SetActive(true);
            willShowHelperHand = true;
        }

        private void Exit_HelperHand()
        {
            willShowHelperHand = false;
            helperHand.SetActive(false);
        }

        private void Enter_Init()
        {
            Initialize();
            ResetGameplay();
        }

        private void Exit_Init()
        {
        }

        private void Enter_Drawing()
        {
            if (skipCardDrawBecauseOfUndoLast)
            {
                return;
            }

            DealSingleCard(DrawDeck, activeDeck);
            AudioManager.instance.PlaySound(AudioManager.Sound.DrawCard);
        }

        private void Exit_Drawing()
        {
            if (skipCardDrawBecauseOfUndoLast)
            {
                skipCardDrawBecauseOfUndoLast = false;
                return;
            }

            run21.DrawCard(); // logic draw
            cardsRemainingValue.text = (run21.RemainingCards - activeDeck.Deck.CardCount).ToString();
            endDeck.PutTopCard(activeCard);

            DisplayTextAboveActiveLane();

            DisplayaneOutlinesBasedOnCard(activeDeck.TopCard);
        }

        private void Enter_Dragging()
        {

        }

        private void Refresh_Dragging(float delta)
        {
            var mousePos = Input.mousePosition;
            mousePos.z = 10;
            if(activeCard!=null)
            activeCard.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        }

        private void Exit_Dragging()
        {

        }

        void StartingDrag(DisplayDeck displayDeck)
        {
            if (displayDeck == activeDeck)
            {
                requestDragging = true;
            }

        }

        void ReleasedDrag(DisplayDeck displayDeck)
        {
            requestDragging = false;
            if (displayDeck == lane1 || displayDeck == lane2 || displayDeck == lane3 || displayDeck == lane4)
            {
                releasedOnLane = true;
            }
            else
            {
                releasedOnLane = false;
            }


            dealDestination = displayDeck;

        }

        private void Enter_ReleasedACard()
        {
            if (releasedOnLane)
            {
                willShowHelperHand = false;
                isCountingDown = true;
                isAnimating = true;
                AudioManager.instance.PlaySound(AudioManager.Sound.CardPlaced);
                DealSingleCard(activeDeck, dealDestination);
                for (int i = 0; i < 4; i++)
                {
                    if (run21.LaneDecks[i] == dealDestination.Deck) { laneToDealTo = i; }
                }

            }
            else
            {
                AudioManager.instance.PlaySound(AudioManager.Sound.CardPlaced);
                DealSingleCard(activeDeck, activeDeck);
            }

        }

        private void Exit_ReleasedACard()
        {
            releasedOnLane = false;
        }

        // visual animation of the card being played
        private void Enter_AnimatingACard()
        {
            willShowHelperHand = false;
            isCountingDown = true;
            isAnimating = true;
            AudioManager.instance.PlaySound(AudioManager.Sound.CardPlaced);

            DealSingleCard(activeDeck, dealDestination);
        }

        private void Exit_AnimatingACard()
        {
            isAnimating = false;
        }

        // playing a card in logic, and adding it's display to corresponding DisplayDeck
        private void Enter_PlayingACard()
        {
            run21.PlayCard(laneToDealTo);
            endDeck.PutTopCard(activeCard);
        }

        private void Exit_PlayingACard()
        {
            ClearLaneOutlines();
        }

        private void Enter_Idle()
        {
        }

        private void Exit_Idle()
        {
        }

        private void Enter_Paused()
        {
            pausePanel.SetActive(true);

            musicButtonOn.gameObject.SetActive(PlayerPrefsManager.GetIsMusicPlaying());
            musicButtonOff.gameObject.SetActive(!PlayerPrefsManager.GetIsMusicPlaying());
            musicSlider.value = PlayerPrefsManager.GetIsMusicPlaying() ? PlayerPrefsManager.GetMusicVolume() : 0f;

            SFXButtonOn.gameObject.SetActive(PlayerPrefsManager.GetIsSFXPlaying());
            SFXButtonOff.gameObject.SetActive(!PlayerPrefsManager.GetIsSFXPlaying());
            sfxSlider.value = PlayerPrefsManager.GetIsSFXPlaying() ? PlayerPrefsManager.GetSFXVolume() : 0f;


            PausePanel.OnClick += Unpause;
            if (isCountingDown)
            {
                hasBeenCountingDown = true;
                isCountingDown = false;
            }
        }

        void Unpause(PausePanel pausePanel)
        {
            requestPause = false;
        }

        private void Exit_Paused()
        {
            PausePanel.OnClick -= Unpause;
            pausePanel.SetActive(false);
            if (hasBeenCountingDown)
            {
                isCountingDown = true;
            }

            requestPause = false;
        }

        private void Enter_Tutorial()
        {
            pausePanel.SetActive(false);
            tutorialPanels.OnClickedExit += OnExitTutorial;
            tutorialScreens.GetComponent<TutorialPanels>().OnClickedExit += OnExitTutorial;
            tutorialScreens.SetActive(true);
            if (isCountingDown)
            {
                hasBeenCountingDown = true;
                isCountingDown = false;
            }
        }

        void OnExitTutorial()
        {
            requestTutorial = false;
        }

        private void Exit_Tutorial()
        {
            tutorialPanels.OnClickedExit -= OnExitTutorial;
            tutorialScreens.SetActive(false);

            if (hasBeenCountingDown)
            {
                isCountingDown = true;
            }

            requestTutorial = false;
        }

        void HandleSoundEffects()
        {
            previousStreak = currentStreak;
            currentStreak = run21.ScoredStreak;
            if (currentStreak != previousStreak && currentStreak > 1)
            {
                previousStreak = currentStreak;
            }

            if (currentBusts != run21.Score.Busts)
            {
                currentBusts = run21.Score.Busts;
            }

            if (currentColumnsCleared != run21.ColumnsCleared)
            {
                currentColumnsCleared = run21.ColumnsCleared;
            }

            if (remainingTime < durations.lastTimeWarning && !hasPlayedFinalCountdown)
            {
                AudioManager.instance.PlaySound(AudioManager.Sound.LastXSeconds);

                screenOutlineSpawner.gameObject.SetActive(true);
                screenOutline.DisplayBustOutline();
                hasPlayedFinalCountdown = true;
            }

            if (remainingTime < durations.firstTimeWarning && remainingTime > durations.lastTimeWarning && !timerWarnedSoundPlayed)
            {
                screenOutlineSpawner.gameObject.SetActive(true);
                screenOutline.DisplayBustOutline();
                AudioManager.instance.PlaySound(AudioManager.Sound.TimerWarned);
                timerWarnedSoundPlayed = true;
            }
            else if (remainingTime < (durations.firstTimeWarning - 1f) && remainingTime > durations.lastTimeWarning)
            {
                screenOutlineSpawner.gameObject.SetActive(false);
            }
        }

        private void Enter_Scoring()
        {
            MainGameLogic.instance.UpdateGameScore(run21.Score.GameScore);

            // lane 1
            if (run21.CalculateDeckValue(lane1.Deck).low != run21.CalculateDeckValue(lane1.Deck).high && run21.CalculateDeckValue(lane1.Deck).high < 21) // if high and low differ
            {
                score1.SetScore($"{run21.CalculateDeckValue(lane1.Deck).low}/{run21.CalculateDeckValue(lane1.Deck).high}");
            }
            else
            {
                score1.SetScore($"{run21.CalculateDeckValue(lane1.Deck).low}"); // since high and low are the same, it doesn't matter if I use .low or .high
            }

            if (run21.CalculateDeckValue(lane1.Deck).low == 0)
            {
                lane1.RemoveAllCards();
            }

            // lane 2
            if (run21.CalculateDeckValue(lane2.Deck).low != run21.CalculateDeckValue(lane2.Deck).high && run21.CalculateDeckValue(lane2.Deck).high < 21)
            {
                score2.SetScore($"{run21.CalculateDeckValue(lane2.Deck).low}/{run21.CalculateDeckValue(lane2.Deck).high}");
            }
            else
            {
                score2.SetScore(run21.CalculateDeckValue(lane2.Deck).low.ToString());
            }

            if (run21.CalculateDeckValue(lane2.Deck).low == 0)
            {
                lane2.RemoveAllCards();
            }

            // lane 3
            if (run21.CalculateDeckValue(lane3.Deck).low != run21.CalculateDeckValue(lane3.Deck).high && run21.CalculateDeckValue(lane3.Deck).high < 21)
            {
                score3.SetScore(run21.CalculateDeckValue(lane3.Deck).low + "/" + run21.CalculateDeckValue(lane3.Deck).high);
            }
            else
            {
                score3.SetScore(run21.CalculateDeckValue(lane3.Deck).low.ToString());
            }

            if (run21.CalculateDeckValue(lane3.Deck).low == 0)
            {
                lane3.RemoveAllCards();
            }

            // lane 4
            if (run21.CalculateDeckValue(lane4.Deck).low != run21.CalculateDeckValue(lane4.Deck).high && run21.CalculateDeckValue(lane4.Deck).high < 21)
            {
                score4.SetScore(run21.CalculateDeckValue(lane4.Deck).low + "/" + run21.CalculateDeckValue(lane4.Deck).high);
            }
            else
            {
                score4.SetScore(run21.CalculateDeckValue(lane4.Deck).low.ToString());
            }

            if (run21.CalculateDeckValue(lane4.Deck).low == 0)
            {
                lane4.RemoveAllCards();
            }


            for (int i = 0; i < run21.Score.Busts; i++)
            {
                busts[i].SetActive(true);
            }
        }

        private void Exit_Scoring()
        {
        }

        private void Enter_PreGameOver()
        {
            activeDeckLastCard.SetActive(false);
            screenOutlineSpawner.gameObject.SetActive(false);
            AudioManager.instance.StopSound(AudioManager.Sound.LastXSeconds);
            timeInPreGameOver = 0f;
            isCountingDown = false;
        }

        private void Refresh_PreGameOver(float delta)
        {
            timeInPreGameOver += delta;
        }

        private void Exit_PreGameOver()
        {
        }

        private void Enter_GameOver()
        {
            isCountingDown = false;
            run21.EndGame();
            gameOverPanel.SetActive(true);
            AudioManager.instance.PlaySound(AudioManager.Sound.GameFinished);


            if (run21.Score.PerfectGameScore != 0)
            {
                AudioManager.instance.PlaySound(AudioManager.Sound.GFPerfectGame);

            }
            else if (run21.Score.Busts == 0)
            {
                AudioManager.instance.PlaySound(AudioManager.Sound.GFNoBusts);
            }
        }

        private void Refresh_GameOver(float delta)
        {
            _gameOverDuration += delta;
        }

        private void Exit_GameOver()
        {
            gameOverPanel.SetActive(false);
            requestGameOver = false;
        }

        private void Enter_SubmitGameOverScore()
        {
            MainGameLogic.instance.RequestExitGameplay(run21.Score.FinalScore);
        }

        private void Exit_SubmitGameOverScore()
        {
        }

        private void Enter_UndoingLastMove()
        {
            //            Debug.Log("ENTERING UNDO LAST MOVE");
            run21.Score.DisplayedGameScore = run21.Score.GameScore;
            run21.UndoLastMove();
        }

        private void Exit_UndoingLastMove()
        {
            SyncGameplayDecksAndScoreWithGame();
            run21.Score.DisplayedGameScore = run21.Score.GameScore;
            mainScore.text = run21.Score.GameScore.ToString();
            DisplayTextAboveActiveLane();
        }

        private void Enter_InitialAnimation()
        {
            // draw deck now matches full shuffled deck in logic, but for animation let's clear it and animate clone deck to it
            animationDeck.Deck = run21.DrawDeck.Clone();
            // reverse to match proper ordering of final draw deck since it's dealt from top to top
            animationDeck.Deck.ReverseCards();
            animationDeck.ShowTopNCards(0);
            animationDeck.RefreshDisplay();

            // deal to empty deck, later we need to make it point to logic deck again
            DrawDeck.Deck = new Deck();
            DrawDeck.RefreshDisplay();
            if (initialShufleAnimationDone == false)
            {
                AudioManager.instance.PlaySound(AudioManager.Sound.Shuffle);
            }

            animationDeck.DealTowardsDeckAnimated(
                Run21Data.Instance.animationConfig.DealSingleCardInitialShuffle,
                Run21Data.Instance.animationConfig.TimeToDealNextCardInitialShuffle,
                DrawDeck,
                () => { initialShufleAnimationDone = true; });

            initialShufleAnimationDone = false;
        }

        private void Exit_InitialAnimation()
        {
            // reset after animation to match actual logic
            DrawDeck.Deck = run21.DrawDeck;
            //            run21.DrawDeck = TestData.acesAndBlackjacks; // TODO remove, for testing purpose    
            //            DrawDeck.Deck = TestData.acesAndBlackjacks; // TODO remove, for testing purpose
            DrawDeck.ShowTopNCards(0);
            DrawDeck.RefreshDisplay();
            run21.TakeSnapshot();
        }

        private void Enter_PopupAnimating()
        {
        }

        private void Exit_PopupAnimating()
        {
        }

        public void ResumeButtonClicked()
        {
            requestPause = false;
        }

        public void RequestGameOver()
        {
            // for tournament, show confirmation popup from game taco, otherwise just end the game
            if (MainGameLogic.instance.Mode == MainGameLogic.GameMode.GameTacoTournament)
            {
                TacoSetup.Instance.callback=(string data) => {
                    TacoSetup.Instance.callback = null;
                    RequestSubmitScore();
                };
                TacoSetup.Instance.TacoOpenEndPlayGame(() => {
                    requestGameOver = true;
                });
                return;
            }

            requestGameOver = true;
        }

        public void RequestSubmitScore()
        {
            requestSubmitScore = true;
        }
        public bool finish_load_finalscore = false;
        public void ConfirmSubmitScore()
        {
            //if (!finish_load_finalscore)
            //   return;
            if(MainMenu.game_offline)
            {
                MainGameLogic.instance.RequestExitGameplay(run21.Score.FinalScore);
                return;
            }

            TacoSetup.Instance.callback= (string data) => {
                TacoSetup.Instance.callback = null;
                if (data.Equals("exit"))
                    MainGameLogic.instance.RequestExitGameplay(run21.Score.FinalScore);
                else
                    RequestSubmitScore();
            };
            MainGameLogic.instance.UpdateGameScore(run21.Score.FinalScore);
            TacoSetup.Instance.TacoEndTournament();
        }

        #endregion

        private DisplayCard activeCard;
        private DisplayDeck startDeck;
        private DisplayDeck endDeck;

        public void DealSingleCard(DisplayDeck pStartDeck, DisplayDeck pEndDeck, bool isFaceUp = true)
        {
            startDeck = pStartDeck;
            endDeck = pEndDeck;
            activeCard = pStartDeck.TopCard;
            isAnimating = true;
            activeCard.Card.FaceUp = isFaceUp;

            Vector3 worldPosInNewDeck = endDeck.GetCardPosition_World(endDeck.Cards.Count);
            activeCard.MoveTowardsAnimated(
                worldPosInNewDeck,
                activeCard.Card.FaceUp ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, 180f, 0f),
                Run21Data.Instance.animationConfig.playSingleCardDuration,
                () =>
                {
                    if (startDeck != endDeck)
                    {
                        activeCard.transform.SetParent(endDeck.parentOfCards);
                        startDeck.TakeTopCard();
                    }
                    isAnimating = false;
                });
        }

        public void SyncGameplayDecksAndScoreWithGame()
        {
            // sync decks
            DrawDeck.Deck = run21.DrawDeck;
            activeDeck.Deck = run21.ActiveCardDeck;
            lane1.Deck = run21.FirstLane;
            lane2.Deck = run21.SecondLane;
            lane3.Deck = run21.ThirdLane;
            lane4.Deck = run21.FourthLane;

            DrawDeck.RecreateDisplay();
            activeDeck.RecreateDisplay();
            lane1.RecreateDisplay();
            lane2.RecreateDisplay();
            lane3.RecreateDisplay();
            lane4.RecreateDisplay();

            ResetDisplayedBustScore(run21.Score.Busts);
            
            run21.Score.DisplayedGameScore = run21.Score.GameScore;
            mainScore.text = run21.Score.GameScore.ToString();
        }

        /// <summary>
        /// Initialize all the objects we need and hook them up
        /// </summary>
        private void Initialize()
        {
            run21 = new Run21(Run21Data.Instance.scoringData);
            hasPlayedFinalCountdown = false;
            timerWarnedSoundPlayed = false;
            musicButtonOn.gameObject.SetActive(AudioManager.instance.MusicOn);
            musicButtonOff.gameObject.SetActive(!AudioManager.instance.MusicOn);
            FormatTime(Run21.PlayTimeMax);
            tutorialPanels = tutorialScreens.GetComponent<TutorialPanels>();
            lane1.Deck = run21.FirstLane;
            lane2.Deck = run21.SecondLane;
            lane3.Deck = run21.ThirdLane;
            lane4.Deck = run21.FourthLane;
            DrawDeck.Deck = run21.DrawDeck;
            activeDeck.Deck = run21.ActiveCardDeck;
            DrawDeck.ShowTopNCards(0);
            DrawDeck.RecreateDisplay();
            tutorialExitButton.onClick.AddListener(OnExitTutorial);
            musicButtonOn.onClick.AddListener(MusicButtonClicked);
            musicButtonOff.onClick.AddListener(MusicButtonClicked);
            SFXButtonOn.onClick.AddListener(SFXButtonClicked);
            SFXButtonOff.onClick.AddListener(SFXButtonClicked);
            musicSlider.onValueChanged.AddListener(delegate { MusicSliderChanged(); } );
            sfxSlider.onValueChanged.AddListener(delegate { SFXSliderChanged(); });
            buttonSwapToggle.onValueChanged.AddListener(delegate { SwapButtons(); });
            pauseButton.onClick.AddListener(PauseButtonClicked);
            tutorialButton.onClick.AddListener(TutorialButtonClicked);
            resumeButton.onClick.AddListener(ResumeButtonClicked);
            endGameButton.onClick.AddListener(RequestGameOver);
            undoButton.onClick.AddListener(TryToUndoLastMove);

            run21.ScoreEvent += run21.Score.OnScoreEvent; // run21Score handles score updated which dont require animations
            run21.ScoreEvent += popupManager.OnScoreEvent;
        }

        private void SwapButtons()
        {
            Vector3 newPosition = undoButton.transform.position;
            undoButton.transform.position = pauseButton.transform.position;
            pauseButton.transform.position = newPosition;

            PlayerPrefsManager.SetIsButtonsSwapped(buttonSwapToggle.isOn);
        }

        private void MusicSliderChanged()
        {
            PlayerPrefsManager.SetMusicVolume(musicSlider.value);
            PlayerPrefsManager.SetIsMusicPlaying(musicSlider.value > musicSlider.minValue);

            AudioManager.instance.MusicVolume = PlayerPrefsManager.GetMusicVolume();
            AudioManager.instance.MusicOn = PlayerPrefsManager.GetIsMusicPlaying();

            musicButtonOn.gameObject.SetActive(PlayerPrefsManager.GetIsMusicPlaying());
            musicButtonOff.gameObject.SetActive( ! PlayerPrefsManager.GetIsMusicPlaying());
        }

        private void SFXSliderChanged()
        {
            PlayerPrefsManager.SetSFXVolume(sfxSlider.value);
            PlayerPrefsManager.SetIsSFXPlaying(sfxSlider.value > sfxSlider.minValue);

            AudioManager.instance.SFXVolume = PlayerPrefsManager.GetSFXVolume();
            AudioManager.instance.SFXOn = PlayerPrefsManager.GetIsSFXPlaying();

            SFXButtonOn.gameObject.SetActive(PlayerPrefsManager.GetIsSFXPlaying());
            SFXButtonOff.gameObject.SetActive( ! PlayerPrefsManager.GetIsSFXPlaying());
        }

        private void AudioButtonClicked()
        {
            requestMusic = ! requestMusic;
        }

        private void SFXButtonClicked()
        {
            PlayerPrefsManager.SetIsSFXPlaying( ! PlayerPrefsManager.GetIsSFXPlaying());
            AudioManager.instance.SFXOn = PlayerPrefsManager.GetIsSFXPlaying();

            if (AudioManager.instance.SFXOn)
            {
                sfxSlider.value = PlayerPrefsManager.previousSFXVolume;
            }
            else
            {
                sfxSlider.value = 0f;
            }
            
            SFXButtonOn.gameObject.SetActive(PlayerPrefsManager.GetIsSFXPlaying());
            SFXButtonOff.gameObject.SetActive( ! PlayerPrefsManager.GetIsSFXPlaying());
        }

        private void MusicButtonClicked()
        {
                PlayerPrefsManager.SetIsMusicPlaying( ! PlayerPrefsManager.GetIsMusicPlaying());
                AudioManager.instance.MusicOn = PlayerPrefsManager.GetIsMusicPlaying();

                if (AudioManager.instance.MusicOn)
                {
                    musicSlider.value = PlayerPrefsManager.previousMusicVolume;
                }
                else
                {
                    musicSlider.value = 0f;
                }

                musicButtonOn.gameObject.SetActive(PlayerPrefsManager.GetIsMusicPlaying());
                musicButtonOff.gameObject.SetActive( ! PlayerPrefsManager.GetIsMusicPlaying());
        }

        private void PauseButtonClicked()
        {
            if (State == FSMState.Idle || State == FSMState.HelperHand || State == FSMState.WaitingForFirstInput)
            {
                requestPause = true;
            }
        }

        private void TutorialButtonClicked()
        {
            requestPause = false;
            requestTutorial = true;
        }

        /// <summary>
        /// Use this method to increment main game score over time.
        /// </summary>
        /// <param name="incrementDuration"></param>
        /// <param name="scorePopup"></param>
        public void IncrementOverTimeTurnOffScorePopup(float incrementDuration, Popup scorePopup)
        {
            StartCoroutine(IncrementOverTime(incrementDuration, scorePopup));
        }

        private IEnumerator IncrementOverTime(float incrementDuration, Popup scorePopup)
        {

            if (scorePopup.Score <= 0)
            {
                throw new ArgumentException("Incoming score has no popup!");
            }

            var times = scorePopup.Score / durations.addPoints;
            var pause = incrementDuration / times;

            for (int i = 0; i < times; i++)
            {
                run21.Score.DisplayedGameScore += (int) durations.addPoints;
                scorePopup.Score -= (int) durations.addPoints;
                mainScore.text = run21.Score.DisplayedGameScore.ToString();
                AudioManager.instance.PlaySound(AudioManager.Sound.ScoreTally);
                yield return new WaitForSeconds(pause);
            }

            scorePopup.TogglePopupOff();

        }

        void FormatTime(float remainingTime)
        {
            int remainingMinutes;
            int remainingSeconds;
            remainingMinutes = (int) remainingTime / 60;
            remainingSeconds = (int) remainingTime % 60;
            if (remainingSeconds < 10) // adding a zero before seconds, so 4 : 5 doesn't happen
            {
                timeText.text = remainingMinutes.ToString("0") + " : 0" + remainingSeconds.ToString("0");
            }
            else
            {
                timeText.text = remainingMinutes.ToString("0") + " : " + remainingSeconds.ToString("0");
            }
        }

        public DisplayDeck GetLaneDeckByIndex(int laneIndex)
        {
            if (laneIndex == 0)
            {
                return lane1;
            }

            if (laneIndex == 1)
            {
                return lane2;
            }

            if (laneIndex == 2)
            {
                return lane3;
            }

            if (laneIndex == 3)
            {
                return lane4;
            }

            throw new ArgumentException($"Cannot resolve lane deck for index :{laneIndex}");
        }

        public void DisplayaneOutlinesBasedOnCard(DisplayCard displayCard)
        {
            if (displayCard == null)
            {
                return;
            }

            if (displayCard.Card.IsBlackJack)
            {
                laneOutlineSpawners[0].gameObject.SetActive(true);
                laneOutlineSpawners[1].gameObject.SetActive(true);
                laneOutlineSpawners[2].gameObject.SetActive(true);
                laneOutlineSpawners[3].gameObject.SetActive(true);
                laneOutlines[0].DisplayWildcardOutline();
                laneOutlines[1].DisplayWildcardOutline();
                laneOutlines[2].DisplayWildcardOutline();
                laneOutlines[3].DisplayWildcardOutline();
                return;
            }


            for (int i = 0; i < laneOutlines.Length; i++)
            {
                if (!run21.IsCardCausingDeckBust(displayCard.Card, GetLaneDeckByIndex(i).Deck))
                {
                    return;
                }
            }

            laneOutlineSpawners[0].gameObject.SetActive(true);
            laneOutlineSpawners[1].gameObject.SetActive(true);
            laneOutlineSpawners[2].gameObject.SetActive(true);
            laneOutlineSpawners[3].gameObject.SetActive(true);
            laneOutlines[0].DisplayBustOutline();
            laneOutlines[1].DisplayBustOutline();
            laneOutlines[2].DisplayBustOutline();
            laneOutlines[3].DisplayBustOutline();
        }

        public void ClearLaneOutlines()
        {
            for (int index = 0; index < laneOutlineSpawners.Length; index++)
            {
                laneOutlineSpawners[index]?.gameObject?.SetActive(false);
            }
        }
        
        private void DisplayTextAboveActiveLane()
        {
            if (endDeck.TopCard.Card.IsBlackJack && run21.RemainingCards > 1)
            {
                activeDeckWildCard.SetActive(true);
            }
            else
            {
                activeDeckWildCard.SetActive(false);
            }

            if (run21.RemainingCards == 1)
            {
                activeDeckLastCard.SetActive(true);
            }
            else
            {
                activeDeckLastCard.SetActive(false);
            }
        }
    }
}