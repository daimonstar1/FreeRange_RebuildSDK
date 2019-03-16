using FRG.Core;
using GameTaco;
using UnityEngine;
using UnityEngine.SceneManagement;
using DarkTonic.MasterAudio;
using System;
namespace FRG.Taco
{
    /// <summary>
    /// Main game logic - starting point with main states of play. Stays active throught the whole playtime.
    /// Add child MonoMachines as needed
    /// </summary>
    public class MainGameLogic : MonoMachine<MainGameLogic.FSMState>
    {
        public static MainGameLogic instance { get; private set; }

        public enum FSMState
        {
            Initializing, // init SDK and all that we need for game to run: singletons, pool etc.
            AutoLogin, // detect if we saved credentials and auto login
            ConfirmDailyPrizes, // if there is a popup (daily prize) on login, stay until user closes it
            Menu, // we're in main menu
            Gameplay, // we're in gameplay
            Quit, // cleanup as we're going to quit
        }

        public enum GameMode
        {
            Offline,
            GameTacoTournament
        }

        [SerializeField] private string gameplaySceneName = "Gameplay";
        [SerializeField] private PoolObjectSpawner spawner_Menu; // auto-magically spawned from MonoMachine when that state is entered
        [SerializeField] private PlaylistController playerlistController;
        [Tooltip("Name of the song from MasterAudio playlist to play while in main menu")]
        [SerializeField] private string songNameMainMenu = "MainMenu";
        [Tooltip("Name of the song from MasterAudio playlist to play while in gameplay")]
        [SerializeField] private string songNameGameplay = "Gameplay";

        public GameMode Mode { get; private set; }
        private bool requestGameplay;
        private bool isGameplayOver;
        private int gameplayFinalScore;

        #region PUBLIC METHODS
        public void respawnMenu()
        {
            spawner_Menu.Spawn(true);
        }
        public void GoToGameplay(GameMode mode)
        {
            Mode = mode;
            requestGameplay = true;
        }

        #endregion

        #region MonoState

        public override void Enter()
        {
            //instance = this;
            base.Enter();
        }

        public override void Exit()
        {
            base.Exit();
        }

        #endregion

        #region MonoMachine

        protected override void Ready()
        {
            instance = this;
            base.Ready();
        }

        protected override FSMState GetStartState()
        {
            return FSMState.Initializing;
        }

        protected override bool StateTransition()
        {
            switch (State)
            {
               
                case FSMState.Initializing:
                    // wait for sdk to be instantiated
                    if (TacoManager.Initialized)
                    {
                        // gameplay already loaded! We started that scene straight away, go to gameplay
                        Scene gameplayScene = SceneManager.GetSceneByName(gameplaySceneName);
                        if (gameplayScene.isLoaded)
                            return ChangeState(FSMState.Gameplay);
                        return ChangeState(FSMState.AutoLogin);
                    }
                    break;
                case FSMState.AutoLogin:
                    // login failed and auto login was cleared
                   
                    if (TacoManager.GetPreference(UserPreferences.autoLogin) == 0)
                        return ChangeState(FSMState.Menu);
                    // login success
                    if (TacoSetup.Instance.IsLoggedIn())
                    {
                        return ChangeState(FSMState.ConfirmDailyPrizes);

                    }
                    break;
                case FSMState.ConfirmDailyPrizes:
                    // wait for user to close a popup (the daily prize!)
                    if (!TacoManager.isOpenPopup)
                        return ChangeState(FSMState.Menu);
                    break;
                case FSMState.Menu:
                    if (StateFinished || StateCancelled)
                    {
                        return ChangeState(FSMState.Quit);
                    }
                    if (requestGameplay)
                    {
                        return ChangeState(FSMState.Gameplay);
                    }
                    break;
                case FSMState.Gameplay:
                    if (isGameplayOver)
                    {
                        return ChangeState(FSMState.Menu);
                    }
                    break;
            }

            return base.StateTransition();
        }

        protected override void ClearFramewiseInputs()
        {
            requestGameplay = false;
        }

        private void Enter_Initializing()
        {
            Application.targetFrameRate = 60;
        }

        private void Exit_Initializing()
        {
        }

        private void Enter_AutoLogin()
        {
            if (TacoManager.GetPreference(UserPreferences.autoLogin) == 1)
            {
                TacoSetup.Instance.OpenLoginPanel();
            }
        }

        private void Exit_AutoLogin()
        {
        }

        private void Enter_ConfirmDailyPrizes()
        {
        }

        private void Exit_ConfirmDailyPrizes()
        {
        }
        private bool first_init = false;
        private void Enter_Menu()
        {
            // doing a login shows the game taco home page, close it
            if(!first_init)
            {
                first_init = true;
                TacoManager.CloseTaco();
            }

        }

        private void Exit_Menu()
        {
        }
        int t = 0;
        private void Enter_Gameplay()
        {
            TacoSetup.Instance.ToggleTacoHeaderFooter(false);

            isGameplayOver = false;
            switch (Mode)
            {
                case GameMode.Offline:
                    TacoSetup.Instance.StartNormalGame();
                    break;
                case GameMode.GameTacoTournament:
                    TacoSetup.Instance.StartTournamentGame();
                    break;
                default:
                    break;
            }
            Debug.LogError("Enter game");
            Core.Util.LoadScene(gameplaySceneName, LoadSceneMode.Additive, true);
            playerlistController.TriggerPlaylistClip(songNameGameplay);

        }

        private void Exit_Gameplay()
        {
            if (TacoSetup.Instance != null && TacoSetup.Instance.IsTournamentPlayed())
            {
                Debug.LogError("Exit_Gameplay=="+ gameplayFinalScore);
                TacoSetup.Instance.ScoreNow = gameplayFinalScore;
                TacoSetup.Instance.callback= (string data) => {
                    TacoSetup.Instance.callback = null;
                    Run21.Gameplay.instance.RequestSubmitScore();
                    Debug.LogError("Exit_Gameplay");
                };

            }
            SceneManager.UnloadSceneAsync(gameplaySceneName);

            playerlistController.TriggerPlaylistClip(songNameMainMenu);
        }

        private void Enter_Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void UpdateGameScore(int currentScore)
        {
            if (State != FSMState.Gameplay)
            {
                Debug.LogError("Trying to update score while not in gameplay. Doing nothing. Current state: " + State.ToString());
                return;
            }
            gameplayFinalScore = currentScore;
            TacoSetup.Instance.ScoreNow = gameplayFinalScore;
        }

        public void RequestExitGameplay(int finalScore)
        {
            if (State != FSMState.Gameplay)
            {
                Debug.LogError("Calling exit gameplay while not in gameplay. Doing nothing. Current state: " + State.ToString());
                return;
            }

            gameplayFinalScore = finalScore;
            isGameplayOver = true;
        }
        public void exitGamePlay()
        {
            isGameplayOver = true;
        }


        #endregion
    }
}