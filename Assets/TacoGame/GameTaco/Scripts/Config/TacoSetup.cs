#define FIREBASE
using UnityEngine;
using System;
/*#if (FIREBASE)
using Firebase;
using Firebase.Analytics;
#endif*/
namespace GameTaco {
	public enum TournamentCate {
		Cash = 0,
		Token
	}

	public class TacoSetup : MonoBehaviour {
		public static TacoSetup Instance;

		public string gameScene = "game";// Set your next scene here
		public string idGameName = "BUBBLE_SHOOTER";
		public string gameName = "Bubble Shooter";
		public Sprite GameIcon; //game-icon
		public string SiteId = "bubbles";
		public string versionOfGame = "1.4.3";

		public delegate void StartTournament();
		public delegate void UserLoggin(bool display);
		public delegate void SetMoneyValue();
		public delegate void SetMainMenu();
		public event StartTournament TournamentStarted;
		public event UserLoggin ToggleButtonWhenLogin;
		public event SetMoneyValue UpdateMoneyValue;
		public event SetMainMenu BackToMainMenu;
		private int scoreNow;
		private TournamentCate tournamentCate = TournamentCate.Cash;

		public TournamentCate TournamentCategory {
			get { return tournamentCate; }
			set { tournamentCate = value; }
		}

		void Awake() {
			Instance = this;
			LogEvent("start_game");
		}

		void Start() {
			//ValidateConfig();
			DontDestroyOnLoad(transform.gameObject);
			//SwitchScene();
		}

		public void LogEvent(string _event) {
			//Debug.Log(_event);
			//FirebaseAnalytics.LogEvent(_event);
		}

		private void ValidateConfig() {
			if (string.IsNullOrEmpty(gameScene) || string.IsNullOrEmpty(SiteId) || string.IsNullOrEmpty(idGameName) || string.IsNullOrEmpty(gameName) || GameIcon == null) {
				Debug.LogError("Some config is missing. Please check --Game Taco prefab");
			}
		}

		private void SwitchScene() {
			UnityEngine.SceneManagement.SceneManager.LoadScene(gameScene);
		}

		public string GetVersion() {
			return versionOfGame;
		}

		public string GetGameName() {
			return idGameName;
		}

		private void OpenTacoFromGame() {
			TacoManager.OpenTacoFromGame();
		}
        public Action<string> callback;
        private void TacoPostScoreImmediately(int score) {
            Debug.LogError("score="+score);
			if (TacoManager.Target != null && TacoManager.Target.id > 0) {
                if (callback != null)
                    GameManager.Instance.callback = this.callback;
                GameManager.Instance.PostScoreImmediately(score, TacoManager.Target);
			}
            else
            {
                callback?.Invoke("exit");
            }
        }

		public void ToggleMenuButton(bool display) {
			if (ToggleButtonWhenLogin != null) ToggleButtonWhenLogin(display);
		}

		public void SetMoneyValueForButtons() {
			if (UpdateMoneyValue != null) UpdateMoneyValue();
		}

		public void ToMainMenu() {
			if (BackToMainMenu != null) BackToMainMenu();
		}

		#region API
		/// <summary>
		/// Starts the tournament game.
		/// </summary>
		public void StartTournamentGame() {
			if (TournamentStarted != null) {
				scoreNow = 0;
				TournamentStarted();
			}
		}

		/// <summary>
		/// Opens the cash tournament list.
		/// </summary>
		public void OpenCashTournament() {
			TournamentCategory = TournamentCate.Cash;
			OpenTacoFromGame();
		}

		/// <summary>
		/// Opens the token tournament list.
		/// </summary>
		public void OpenTacoTournament() {
			TournamentCategory = TournamentCate.Token;
			OpenTacoFromGame();
		}

		/// <summary>
		/// Call to make sure current game will not be a tournament
		/// </summary>
		public void StartNormalGame() {
			TacoManager.Target = null;
		}

		/// <summary>
		/// Post score when game over
		/// </summary>
		/// <param name="score">Score.</param>
		public void TacoPostScore(int score) {
			if (TacoManager.Target != null && TacoManager.Target.id > 0) {
				GameManager.Instance.PostScore(score, TacoManager.Target);
			}
		}

		/// <summary>
		/// Check if current game is in tournament mode
		/// </summary>

		public bool IsTournamentPlayed() {
			return TacoManager.Target != null;
		}

		/// <summary>
		/// Call to open confirm panel when the user want to exit current tournament
		/// Please set current score using <code>ScoreNow</code> when the user choose to exit the tournament
		/// </summary>
		public void TacoOpenEndPlayGame(System.Action callback=null) {
			TacoManager.CloseAllModals();
			TacoManager.OpenModalEndPlayingPanel(TacoConfig.EndTournamentHeader, TacoConfig.EndTournamentConfirm, callback);
		}

		/// <summary>
		/// Sets the current score.
		/// </summary>
		/// <value>The score now.</value>
		public int ScoreNow {
			set { scoreNow = value; }
		}

		/// <summary>
		/// Call to post ScoreNow to the server
		/// </summary>
		public void TacoEndTournament() {
			TacoPostScoreImmediately(scoreNow);
		}

		/// <summary>
		/// Check If user is logged in or not
		/// </summary>
		public bool IsLoggedIn() {
			return TacoManager.UserLoggedIn();
		}

		/// <summary>
		/// Shows the login panel.
		/// </summary>
		public void OpenLoginPanel() {
			TacoManager.ShowPanel(PanelNames.LoginPanel);
		}

		/// <summary>
		/// Shows the register panel.
		/// </summary>
		public void OpenRegisterPanel() {
			TacoManager.ShowPanel(PanelNames.RegisterPanel);
		}

		/// <summary>
		/// Shows the How to Play panel.
		/// </summary>
		public void OpenHowToPlayPanel() {
      LogEvent("how_it_work");
			TacoManager.TurnPanelOn("HowToPlay0");
		}

		/// <summary>
		/// Show/hide taco header and navigation
		/// Use this if you want to hide them when the user plays the game
		/// </summary>
		/// <param name="status">If set to <c>true</c> status.</param>
		public void ToggleTacoHeaderFooter(bool status) {
			TacoManager.TurnTacoHeaderFooter(status);
		}
	}
	#endregion
}