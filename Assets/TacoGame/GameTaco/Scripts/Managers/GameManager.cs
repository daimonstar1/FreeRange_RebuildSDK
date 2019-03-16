using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace GameTaco {

	public class GameManager : MonoBehaviour {
        public Action<string> callback;
        #region Singleton 
        private static GameManager mInstance;
		public static GameManager Instance {
			get {
				if (mInstance == null) {
					mInstance = new GameObject().AddComponent<GameManager>();
				}
				return mInstance;
			}
		}
		#endregion

		// Use this for initialization
		void Start() {

		}

		// Update is called once per frame
		void Update() {

		}

		#region Play Game
		public void StartPlay(Tournament target) {
			if (target != null && target.id > 0) {
				TacoManager.OpenMessage(TacoConfig.TacoPlayStarting);
				Action<string> success = (string data) => {
					var r = JsonUtility.FromJson<StartGameResult>(data);
					if (r.success) {
						TacoManager.GameToken = r.token;
						TacoManager.CloseMessage();
						// delegate to your game 
						TacoSetup.Instance.StartTournamentGame();
						TacoManager.CloseTaco();
					}
					else {
						TacoManager.CloseAllModals();
						TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.TacoPlayError, r.message);
					}
				};


				Action<string, string> fail = (string data, string error) => {
					var msg = data + ((string.IsNullOrEmpty(error)) ? string.Empty : " : " + error);
					Debug.Log("Error starting game - " + msg);

					var r = JsonUtility.FromJson<StartGameResult>(data);

					TacoManager.CloseAllModals();
					TacoManager.CloseAllModals();
					if (r.forbidden) {
						TacoManager.OpenModalTournamentCreationForbiddenPanel(r.message);
					}
					else {
						TacoManager.OpenModalPlayTournamentErrorPanel(r.message);
					}
					//TacoManager.OpenModal(TacoConfig.TacoPlayError, r.message);
				};

				StartCoroutine(ApiManager.Instance.StartGame(target.typeCurrency, target.id, TacoManager.User.token, success, fail));

			}
		}

		#endregion


		public void PostScore(int score, Tournament target) {
			if (target != null && TacoManager.UserLoggedIn()) {
				Action<string> success = (string data) => {
					ScoreResult r = JsonUtility.FromJson<ScoreResult>(data);
					string modalBody = TacoConfig.TacoPlayEndedModalBody;
					string modal = TacoConfig.TacoPlayEndedWinnerModalBody;
					modalBody = modalBody.Replace("&gameEndScore", r.score.ToString());
					if (r.tournament.typeCurrency == 0) {
						modal = modal.Replace("&prize", " $ " + Math.Round((Decimal)r.tournament.prize, 2, MidpointRounding.AwayFromZero));
						modal = modal.Replace("&point", "P " + (int)Math.Round(r.tournament.prize * 50, 0, MidpointRounding.AwayFromZero) + " and ");
					}
					else {
						modal = modal.Replace("&point", string.Empty);
						modal = modal.Replace("&prize", " T " + r.tournament.prize);
					}

					if (r.winner) {
						TacoManager.CloseAllModals();
						TacoManager.SetToMainMenu();
						TacoManager.ShowPanel(PanelNames.MyLeaderboardPanelFromEndGame);
					}
					else {
						TacoManager.CloseAllModals();
						TacoManager.SetToMainMenu();
						TacoManager.ShowPanel(PanelNames.MyLeaderboardPanelFromEndGame);
					}
					TacoManager.GameToken = null;
					if (r.updated) {
						TacoManager.UpdateFundsWithToken(r.funds, r.gTokens.ToString(), r.ticket.ToString());
					}
				};

				Action<string, string> fail = (string data, string error) => {
					if (Application.internetReachability == NetworkReachability.NotReachable) {
						TacoManager.CloseMessage();
						TacoManager.OpenModalConnectionErrorPanel(TacoConfig.ErrorStatusNoInternet);
					}
					else {
						var r = JsonUtility.FromJson<StartGameResult>(data);
						TacoManager.CloseAllModals();
						TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.TacoPlayError, r.message);
					}
					TacoManager.GameToken = null;

				};

				StartCoroutine(ApiManager.Instance.EndGame(score, target.id, target.gameId, TacoManager.GameToken, TacoManager.User.token, success, fail));
			}
		}

		public void PostScoreImmediately(int score, Tournament target) {
			if (target != null && TacoManager.UserLoggedIn()) {
				Action<string> success = (string data) => {
					ScoreResult r = JsonUtility.FromJson<ScoreResult>(data);
					string modalBody = TacoConfig.TacoPlayEndedModalBody;
					string modal = TacoConfig.TacoPlayEndedWinnerModalBody;
					modalBody = modalBody.Replace("&gameEndScore", r.score.ToString());
					if (r.tournament.typeCurrency == 0) {
						modal = modal.Replace("&prize", " $ " + Math.Round((Decimal)r.tournament.prize, 2, MidpointRounding.AwayFromZero));
						modal = modal.Replace("&point", "P " + r.ticket + "and");
					}
					else {
						modal = modal.Replace("&point", string.Empty);
						modal = modal.Replace("&prize", " T " + r.tournament.prize);
					}
                    Debug.LogError("PostScoreImmediately--success=" + data);
                    callback?.Invoke(data);
                    if (r.winner) {
						TacoManager.CloseAllModals();
						TacoManager.SetToMainMenu();
						TacoManager.ShowPanel(PanelNames.MyLeaderboardPanelFromEndGame);
					}
					else {
						TacoManager.CloseAllModals();
						TacoManager.SetToMainMenu();
						TacoManager.ShowPanel(PanelNames.MyLeaderboardPanelFromEndGame);
					}
					TacoManager.GameToken = null;

					if (r.updated) {
						TacoManager.UpdateFundsWithToken(r.funds, r.gTokens.ToString(), r.ticket.ToString());
					}
				};

				Action<string, string> fail = (string data, string error) => {
                    Debug.LogError("PostScoreImmediately--fail=" + error);
                    var r = JsonUtility.FromJson<StartGameResult>(data);
					TacoManager.CloseAllModals();
					TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.TacoPlayError, r.message);
				};

				StartCoroutine(ApiManager.Instance.EndGame(score, target.id, target.gameId, TacoManager.GameToken, TacoManager.User.token, success, fail));
			}
		}
	}
}