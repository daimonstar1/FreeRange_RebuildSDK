using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameTaco {
	public class TournamentManagerFake : MonoBehaviour {
		public static TournamentManagerFake Instance;

		public Tournament setupTournament;

		public GameObject TournamentDetailsPrefab;
		public GameObject TournamentInvitePrefab;
		public GameObject TournamentCreateInvitePrefab;

		// panels
		private GameObject myPrivatePanel;
		private GameObject myCompletedPanel;
		private GameObject myPublicPanel;
		private GameObject myPublicFakePanel;
		private GameObject myLeaderboardPanel;
		private GameObject MainMenuCanvas;

		// misc
		private Dropdown tournamentTypeDropDown;
		private Toggle cashToggle;
		private Toggle tacoToggle;

		private List<Text> CreateTournamentTacoTexts = new List<Text>();
		private List<Text> CreateTournamentCashTexts = new List<Text>();
		private List<Text> CreateTournamentRPTexts = new List<Text>();

		public string TournamentCallback;
		public String CurrentSubPanel;


		void Awake() {
			Instance = this;
			GetMoneyTextAndSetButton();
			SetDefaultMoneytext();
		}

		void Start() {
			myPrivatePanel = GameObject.Find(PanelNames.MyPrivatePanel);
			myCompletedPanel = GameObject.Find(PanelNames.MyCompletedPanel);
			myPublicPanel = GameObject.Find(PanelNames.MyPublicPanel);
			myPublicFakePanel = GameObject.Find(PanelNames.MyPublicFakePanel);
			myLeaderboardPanel = GameObject.Find(PanelNames.MyLeaderboardPanel);
			MainMenuCanvas = GameObject.Find("MainMenuCanvas");
			tournamentTypeDropDown = GameObject.Find("TournamentTypeDropDownFake").GetComponent<Dropdown>();

			cashToggle = GameObject.Find("CashTournamentsButtonFake").GetComponent<Toggle>();
			tacoToggle = GameObject.Find("TacoTournamentsButtonFake").GetComponent<Toggle>();

			cashToggle.onValueChanged.AddListener((bool isOn) => {
				if (isOn) ChangeTournamentTab(0);
			});

			tacoToggle.onValueChanged.AddListener((bool isOn) => {
				if (isOn) ChangeTournamentTab(1);
			});
		}

		private void ChangeTournamentTab(int type) {
			bool changeTab = (int)TacoSetup.Instance.TournamentCategory != type;
			string panelName = CurrentSubPanel;
			if (string.IsNullOrEmpty(panelName)) panelName = PanelNames.MyPublicPanel;

			TacoSetup.Instance.TournamentCategory = (TournamentCate)type;
			PanelToggle(panelName, changeTab);
		}

		// called to open tournaments to default state
		public void Open() {
			CurrentSubPanel = string.Empty;
			tournamentTypeDropDown.value = 0;
			if (TacoSetup.Instance.TournamentCategory == TournamentCate.Cash) {
				if (!cashToggle.isOn) cashToggle.isOn = true;
				else cashToggle.onValueChanged.Invoke(true);
			}
			else {
				if (!tacoToggle.isOn) tacoToggle.isOn = true;
				else tacoToggle.onValueChanged.Invoke(true);
			}
			SetMoneytext();
		}

		// Unity Toggles only have onValueChanged exposed, so when tapped - you get events for the one turning on and also the one turning off
		// don't want multiple events, so detect only the events that are different
		public void PanelToggle(string panelName = null, bool isChangeTournamentType = false) {
			if (panelName == PanelNames.MyPublicPanel) {
				if (TacoManager.User == null) {
					panelName = PanelNames.MyPublicFakePanel;
				}
			}

			if (panelName != CurrentSubPanel || isChangeTournamentType) {
				
				ShowTournamentPanel(panelName);
			}
		}

		public string ReturnSubPanel(string panelName) {
			if (panelName == null) {
				// do we already have a current panel?
				return CurrentSubPanel;
			}
			else {
				// if not use the one passed in
				return panelName;
			}
		}

		public void SetDefaultMoneytext() {
			for (int i = 0; i < CreateTournamentTacoTexts.Count; i++) {
				CreateTournamentTacoTexts[i].text = TacoManager.FormatGTokens(0);
			}
			for (int i = 0; i < CreateTournamentCashTexts.Count; i++) {
				CreateTournamentCashTexts[i].text = TacoManager.FormatCash(0);
			}
			for (int i = 0; i < CreateTournamentRPTexts.Count; i++) {
				CreateTournamentRPTexts[i].text = TacoManager.FormatRP(0);
			}
		}

		public void SetMoneytext() {
			if (TacoManager.User != null) {

				for (int i = 0; i < CreateTournamentTacoTexts.Count; i++) {
					CreateTournamentTacoTexts[i].text = TacoManager.FormatGTokens(double.Parse(TacoManager.User.gToken));
				}
				for (int i = 0; i < CreateTournamentCashTexts.Count; i++) {
					CreateTournamentCashTexts[i].text = TacoManager.FormatCash(TacoManager.User.TotalCash);
				}
				for (int i = 0; i < CreateTournamentRPTexts.Count; i++) {
					CreateTournamentRPTexts[i].text = TacoManager.FormatRP(TacoManager.User.ticket);
				}
				TacoSetup.Instance.SetMoneyValueForButtons();
			}
		}

		private void GetMoneyTextAndSetButton() {
			GameObject[] list = GameObject.FindGameObjectsWithTag("TacoValueText");
			for (int i = 0; i < list.Length; i++) {
				CreateTournamentTacoTexts.Add(list[i].GetComponent<Text>());
				Button btn = list[i].transform.parent.parent.GetComponent<Button>();
				if (btn != null) {
					btn.onClick.AddListener(delegate {
						BalanceManager.Instance.Init(1);
					});
				}
			}
			list = GameObject.FindGameObjectsWithTag("CashValueText");
			for (int i = 0; i < list.Length; i++) {
				CreateTournamentCashTexts.Add(list[i].GetComponent<Text>());
				Button btn = list[i].transform.parent.parent.GetComponent<Button>();
				if (btn != null) {
					btn.onClick.AddListener(delegate {
						BalanceManager.Instance.Init(0);
					});
				}
			}
			list = GameObject.FindGameObjectsWithTag("RPValueText");
			for (int i = 0; i < list.Length; i++) {
				CreateTournamentRPTexts.Add(list[i].GetComponent<Text>());
				Button btn = list[i].transform.parent.parent.GetComponent<Button>();
				if (btn != null) {
					btn.onClick.AddListener(delegate {
						BalanceManager.Instance.Init(2);
					});
				}
			}
		}

		public void ShowTournamentPanel(string panelName = null, Func<Tournament, bool> filterCondition = null) {
			TacoSetup.Instance.ToggleTacoHeaderFooter(true);
			TacoSetup.Instance.LogEvent("tournament_page");
			if (MainMenuCanvas != null) MainMenuCanvas.SetActive(true);
			// if passed in with null don't reset everything - show the subpanel
			string switchPanelName = ReturnSubPanel(panelName);
			SetAllTournamentPanels(false);
			SetMoneytext();
			TacoManager.columnSortName = "-";
			// open the one you want and dependent stuff
			switch (switchPanelName) {
			case PanelNames.MyCompletedPanel:
				TacoManager.MyTournamentsPanel.SetActive(true);
				myCompletedPanel.SetActive(true);
				getCompletedTournaments(filterCondition);
				CurrentSubPanel = switchPanelName;
				break;

			case PanelNames.MyPrivatePanel:
				TacoManager.MyTournamentsPanel.SetActive(true);
				myPrivatePanel.SetActive(true);
				getPrivateUserTournaments(filterCondition);
				CurrentSubPanel = switchPanelName;
				break;

			case PanelNames.MyPublicPanel:
				TacoManager.MyTournamentsPanel.SetActive(true);
				myPublicPanel.SetActive(true);
				getPublicTournaments(filterCondition);
				CurrentSubPanel = switchPanelName;
				break;

			case PanelNames.MyPublicFakePanel:
				TacoManager.MyTournamentsFakePanel.SetActive (true);
				myPublicFakePanel.SetActive (true);
				getPublicTournaments (filterCondition);
				CurrentSubPanel = switchPanelName;
				break;

			case PanelNames.MyLeaderboardPanel:
				myLeaderboardPanel.SetActive(true);
				LeaderboardList.Instance.LoadLeaderboard(TacoManager.Target);
				break;
			}
		}

		public void SetAllTournamentPanels(bool active) {
			myPublicPanel.SetActive(active);
			myPrivatePanel.SetActive(active);
			myCompletedPanel.SetActive(active);
			myLeaderboardPanel.SetActive(active);
		}

		public void RemoveFriend(string email, Action callback) {
			TacoManager.OpenMessage(TacoConfig.TacoSending);

			Action<string> success = (string data) => {
				TacoManager.CloseMessage();
				TacoManager.OpenModalRemovePlayerNoticePanel();
				callback();
			};

			Action<string, string> fail = (string data, string error) => {
				Debug.Log("Error getting open tournaments : " + data);
				if (!string.IsNullOrEmpty(error)) {
					Debug.Log("Error : " + error);
				}
			};

			Tournament t = ManageTournament.Instance.tournament;
			int friendId = 0;
			for (int i = 0; i < t.friendEmails.Length; i++) {
				if (t.friendEmails[i] == email) {
					friendId = t.friendIds[i];
					break;
				}
			}

			int tournamentId = t.id;
			StartCoroutine(ApiManager.Instance.RemoveFriend(email, friendId, tournamentId, success, fail));
		}

		public void InviteFriend(string email, Action callback = null) {
			TacoManager.OpenMessage(TacoConfig.TacoSending);
			Action<string> success = (string data) => {
				if (callback != null) callback();
				TacoManager.CloseMessage();
			};

			Action<string, string> fail = (string data, string error) => {
				Debug.Log("Error getting open tournaments : " + data);
				if (!string.IsNullOrEmpty(error)) {
					Debug.Log("Error : " + error);
				}
			};

			string fromEmail = TacoManager.User.email;
			string baseUrl = "baysidegames.com";

			int tournamentId = TacoManager.Target.id;
			StartCoroutine(ApiManager.Instance.InviteFriend(fromEmail, baseUrl, email, tournamentId, success, fail));
		}

		public void InviteFriends() {
			//not be used --> remove in new version
			TacoManager.OpenMessage(TacoConfig.TacoSending);
			Action<string> success = (string data) => {
				TacoManager.CloseMessage();
			};

			Action<string, string> fail = (string data, string error) => {
				Debug.Log("Error getting open tournaments : " + data);
				if (!string.IsNullOrEmpty(error)) {
					Debug.Log("Error : " + error);
				}
			};

			StartCoroutine(ApiManager.Instance.InviteFriends(success, fail));
		}

		public void TappedInviteFromList(Tournament t, string actionPrefabName, PrivateTournamentsList myPrivatePanel) {
			// TODO : move tournament Target to this class
			TacoManager.SetTarget(t);
			if (actionPrefabName == myPrivatePanel.ItemShowLeaderboardPrefab.name) {
				ShowTournamentPanel(PanelNames.MyLeaderboardPanel);
			}
			else if (actionPrefabName == myPrivatePanel.ItemPlayPrefab.name) {
				TacoManager.OpenModalPlayTournamentPanel(t);
			}
			else if (actionPrefabName == myPrivatePanel.ItemInvitePrefab.name) {
				TacoManager.Target = t;
				TacoManager.ShowPanel(PanelNames.ManageTournamentPanel);
			}
		}

		public void TappedJoinFromList(Tournament t) {
			TacoManager.SetTarget(t);
			double prize = t.prize;
			double entryFee = t.entryFee;
			int typeCurrency = t.typeCurrency;
			string players = t.size.ToString() + " " + TacoConfig.Players;
			string modalHeader = string.Empty;
			string replacedString = string.Empty;

			if (t.IsWarningTime()) {
				TacoManager.OpenModalWarningTimePanel(t);
			}
			else {
				string formatFee = string.Empty;
				replacedString = "To enter this <b>'" + TacoSetup.Instance.gameName + " &tournamentType'</b> the &tournamentFee will be debited from your account.";
				if (typeCurrency == 0) {
					replacedString = replacedString.Replace("&tournamentFee", "Cash Entry Fee");
					formatFee = TacoManager.FormatCash(entryFee);
				}
				else {
					replacedString = replacedString.Replace("&tournamentFee", "Taco Token Entry Fee");
					formatFee = TacoManager.FormatGTokens(entryFee);
				}

				if (CurrentSubPanel == PanelNames.MyPublicPanel) {
					replacedString = replacedString.Replace("&tournamentType", "Public Tournament");
				}
				else {
					replacedString = replacedString.Replace("&tournamentType", "Private Tournament");
				}

				TacoManager.OpenModalEnterTournamentPanel(replacedString, formatFee, typeCurrency);
			}
		}

		public void SeeResult(Tournament t) {
			TacoManager.SetTarget(t);
			ShowTournamentPanel(PanelNames.MyLeaderboardPanel);
		}

		public void TappedGameFromList(Tournament t) {
			TacoManager.SetTarget(t);

			if (t.played) {
				ShowTournamentPanel(PanelNames.MyLeaderboardPanel);
			}
			else {
				double prize = t.prize;
				int typeCurrency = t.typeCurrency;
				string replacedString = string.Empty;

				if (typeCurrency == 0) {
					replacedString = TacoConfig.TacoSurePlayModalBody.Replace("&prize", TacoManager.FormatCash(prize));
				}
				else {
					replacedString = TacoConfig.TacoSurePlayModalBody.Replace("&prize", TacoManager.FormatGTokens(prize));
				}
				TacoManager.OpenModalPlayTournamentPanel(t);
			}
		}

		#region Get tournaments

		public void getPublicTournaments(Func<Tournament, bool> filterCondition = null) {

			TacoManager.OpenMessage(TacoConfig.TacoRefreshing);
			var cate = (int)TacoSetup.Instance.TournamentCategory;
			Action<string> success = (string data) => {
				List<Tournament> publicList = new List<Tournament>();
				PublicTournamentsResult r = JsonUtility.FromJson<PublicTournamentsResult>(data);
				Debug.Log("Count: " + r.tournaments.Count());
				foreach (Tournament item in r.tournaments) {
					if (item.typeCurrency == cate) {
						publicList.Add(item);
					}
					else if (item.typeCurrency != cate || item.played
						|| ((item.status == "ended" || item.status == "filled"))) {

					}
					else {
						publicList.Add(item);
					}
				}
				if (filterCondition != null) {
					publicList = publicList.Where(filterCondition).ToList();
				}
				publicList = publicList.OrderBy(x => PublicTournamentsListFake.Instance.GetDisplayedStatus(x)).ToList();
				PublicTournamentsListFake.Instance.Reload(publicList);
				TacoManager.CloseMessage();

			};

			Action<string, string> fail = (string data, string error) => {

				Debug.Log("Error getting open tournaments : " + data);
				if (!string.IsNullOrEmpty(error)) {
					Debug.Log("Error : " + error);
				}
			};
			StartCoroutine(ApiManager.Instance.GetWithToken("api/tournament/publicFake/" + TacoSetup.Instance.SiteId, success, fail));
		}

		public void getCompletedTournaments(Func<Tournament, bool> filterCondition = null) {
			TacoManager.OpenMessage(TacoConfig.TacoRefreshing);
			int cate = (int)TacoSetup.Instance.TournamentCategory;
			Action<string> success = (string data) => {
				List<Tournament> publicList = new List<Tournament>();
				CompletedTournamentsResult r = JsonUtility.FromJson<CompletedTournamentsResult>(data);

				foreach (var item in r.tournaments) {
					if (item.typeCurrency == cate) {
						publicList.Add(item);
					}
				}

				if (filterCondition != null) {
					publicList = publicList.Where(filterCondition).ToList();
				}
				CompletedTournamentsList.Instance.Reload(publicList);

				TacoManager.CloseMessage();

			};

			Action<string, string> fail = (string data, string error) => {

				if (!string.IsNullOrEmpty(error)) {
					Debug.Log("Error : " + error);
				}
			};
			StartCoroutine(ApiManager.Instance.GetWithToken("api/tournament/history/" + TacoSetup.Instance.SiteId + "/" + TacoManager.User.userId, success, fail));

		}

		public void getPrivateUserTournaments(Func<Tournament, bool> filterCondition = null) {
			TacoManager.OpenMessage(TacoConfig.TacoRefreshing);
			int cate = (int)TacoSetup.Instance.TournamentCategory;
			Action<string> success = (string data) => {
				List<Tournament> publicList = new List<Tournament>();

				PrivateTournamentsResult r = JsonUtility.FromJson<PrivateTournamentsResult>(data);
				foreach (Tournament item in r.tournaments) {
					if (item.typeCurrency == cate && (!item.played || item.playable) && item.status != "ended") {
						publicList.Add(item);
					}
				}
				if (filterCondition != null) {
					publicList = publicList.Where(filterCondition).ToList();
				}
				publicList = publicList.OrderBy(x => PrivateTournamentsList.Instance.GetDisplayedStatus(x)).ToList();
				PrivateTournamentsList.Instance.Reload(publicList);

				TacoManager.CloseMessage();

			};

			Action<string, string> fail = (string data, string error) => {

				Debug.Log("Error getting open tournaments : " + data);
				if (!string.IsNullOrEmpty(error)) {
					Debug.Log("Error : " + error);
				}
			};

			StartCoroutine(ApiManager.Instance.GetWithToken("api/tournament/private/" + TacoSetup.Instance.SiteId, success, fail));
		}

		public void getActiveTournaments(Action<bool> callback) {
			TacoManager.OpenMessage(TacoConfig.TacoRefreshing);
			int cate = (int)TacoSetup.Instance.TournamentCategory;
			Action<string> success = (string data) => {
				List<Tournament> publicList = new List<Tournament>();
				PrivateTournamentsResult r = JsonUtility.FromJson<PrivateTournamentsResult>(data);
				foreach (Tournament item in r.tournaments) {
					publicList.Add(item);
				}
				ActiveTournamentList.Instance.Reload(publicList);
				callback(publicList.Count > 0);
			};

			Action<string, string> fail = (string data, string error) => {

				Debug.Log("Error getting open tournaments : " + data);
				if (!string.IsNullOrEmpty(error)) {
					Debug.Log("Error : " + error);
				}
				callback(false);
			};

			StartCoroutine(ApiManager.Instance.GetWithToken("api/tournament/active/" + TacoSetup.Instance.SiteId, success, fail));
		}

		#endregion
		#region Create tournament
		public void CreateTournament(Tournament tourament) {

			// disable the button during the creation process : it holds the keyboard focus
			// TODO : find a way to release the focus
			//Button createButton = CreateTournamentButton.GetComponent<Button>();
			//createButton.interactable = false;
			int typeCurrency = tourament.typeCurrency;
			string feeAmount = tourament.entryFee.ToString();
			string currencyIcon = tourament.typeCurrency == 0 ? "$" : "T";

			int playersOption = tourament.size;
			int prizeStructure = tourament.prize_structure;


			string name = string.Format(TacoManager.GameName + " - " + currencyIcon + "{0} Entry - {1} Players", feeAmount, tourament.size);

			string accessType = string.Empty;
			string invitedFriendsString = string.Empty;

			if (tourament.accessType == "public") {
				accessType = "public";
			}
			else {
				//for( int i = 0; i < invitedFriends.Count; i++ ) {
				for (int i = 0; i < tourament.invitedEmails.Length; i++) {
					invitedFriendsString = invitedFriendsString + "," + tourament.invitedEmails[i];
				}
				accessType = "private";
			}


			int timeRemaining = tourament.TimeRemaining;

			Action<string> success = (string data) => {
				var r = JsonUtility.FromJson<CreateTournamentResult>(data);

				if (r.tournament != null) {
					if (r.best) {
						TacoManager.SetTarget(r.tournament);
						TournamentDuplicateResult(TacoConfig.TacoTournamentDuplicated, r.tournament);
					}
					else {
						ShowTournamentPanel(CurrentSubPanel);
						TacoManager.CreatePublicPanel.SetActive(false);
						TournamentSubmitResult(r.tournament);

						double val = 0;
						if (r.typeCurrency == "real") {
							if (double.TryParse(r.userFunds, out val)) {
								TacoManager.UpdateFundsOnly(val, r.userBonus);
							}
						}
						else {
							TacoManager.UpdateTokenOnly(r.userFunds);
						}

						TacoManager.SetTarget(r.tournament);
						SetMoneytext();
					}
				}


			};

			Action<string, string> fail = (string data, string error) => {
				var msg = data + (string.IsNullOrEmpty(error) ? string.Empty : " : " + error);
				Debug.Log("Error create tournamet : " + msg);

				if (!string.IsNullOrEmpty(data)) {
					var r = JsonUtility.FromJson<CreateTournamentResult>(data);
					if (!string.IsNullOrEmpty(r.message)) {
						if (r.forbidden) {
							TournamentSubmitForbidden(r.message);
						}
						else {
							TournamentSubmitError(r.message);
						}
					}
				}
			};

			StartCoroutine(ApiManager.Instance.CreateTournament(timeRemaining, typeCurrency, prizeStructure, invitedFriendsString, name, feeAmount, playersOption, accessType, TacoManager.GameId, TacoManager.User.token, success, fail));

		}

		public void StartCreate() {
			TacoManager.OpenModalCreateTournamentPanel();
		}

		public void TournamentDuplicateResult(string results, Tournament t) {
			TacoManager.CloseMessage();
			TacoManager.OpenModalTournamentExistedPanel(results, t);
		}

		public void TournamentSubmitResult(Tournament t) {
			TacoManager.CloseMessage();
			TacoManager.OpenModalCreateSuccessTimePanel(t);
		}

		private void TournamentSubmitForbidden(string results) {
			TacoManager.CloseMessage();
			TacoManager.OpenModalTournamentCreationForbiddenPanel(results);
		}

		public void TournamentSubmitError(string results) {
			Debug.Log("TournamentSubmitError");
			TacoManager.CloseMessage();
			TacoManager.OpenModalTournamentCreationErrorPanel(results);
		}

		public void TournamentSubmit(Tournament t) {
			// pop the message panel while submitting
			TacoManager.OpenMessage(TacoConfig.TacoTournamentSubmittingMessage);
			CreateTournament(t);

		}

		#endregion

		public void ReEnter() {
			TacoManager.OpenMessage(TacoConfig.TacoPublicJoining);
			var t = TacoManager.Target;

			Action<string> success = (string data) => {
				Debug.Log("reenter data: " + data);
				var r = JsonUtility.FromJson<JoinTournamentResult>(data);
				if (r.success) {
					TacoManager.SetTarget(t);
					var prize = decimal.Parse(r.tournament.prize.ToString("F2"));
					var entryFee = decimal.Parse(r.tournament.entryFee.ToString("F2"));

					string replacedString = string.Empty;
					if (r.tournament.typeCurrency == 0) {
						replacedString = TacoConfig.TacoJoinPublicSuccessBody.Replace("&userFunds", r.userFunds);
						replacedString = replacedString.Replace("&entryFee", entryFee.ToString());
						replacedString = replacedString.Replace("&prize", prize.ToString());
					}
					else {
						replacedString = TacoConfig.TacoJoinPublicSuccessBody.Replace("&userFunds", " T " + r.userFunds);
						replacedString = replacedString.Replace("&entryFee", " T " + entryFee.ToString());
						replacedString = replacedString.Replace("&prize", " T " + prize.ToString());
					}
					TacoManager.CloseMessage();
					TacoManager.OpenModalJoinSuccessPanel(r.tournament, TacoConfig.ReenterSuccessNotice);
					TacoManager.UpdateFundsWithToken(r.cash, r.token.ToString(), r.ticket.ToString());
					ShowTournamentPanel(CurrentSubPanel);

				}
				else {
					TacoManager.CloseMessage();
					if (r.error == TacoConfig.NotEnoughFundError) {
						TacoManager.OpenModalFundErrorJoinPanel(TacoConfig.TacoJoinPublicNoFundHead, t);
					}
					else if (r.error == TacoConfig.NotEnoughTokenError) {
						TacoManager.OpenModalTacoErrorJoinPanel(TacoConfig.TacoJoinPublicNoTacoHead, t);
					}
					else if (r.error == TacoConfig.DuplidateError) {
						TacoManager.OpenModalAlreadyJoinedPanel(TacoConfig.TacoJoinAlreadyHead, r.message);
					}
					else {
						TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.Error, r.message);
					}
				}
			};

			Action<string, string> fail = (string data, string error) => {
				var msg = data + (string.IsNullOrEmpty(error) ? string.Empty : " : " + error);

				if (!string.IsNullOrEmpty(data)) {
					var r = JsonUtility.FromJson<CreateTournamentResult>(data);
					TacoManager.CloseMessage();
					if (!string.IsNullOrEmpty(r.message)) {
						if (r.forbidden) {
							TournamentSubmitForbidden(r.message);
						}
						else {
							TacoManager.OpenModalTournamentJoinErrorPanel(TacoConfig.TacoJoinPublicErrorHead, r.message, ModalFunctions.FundsScreen);
						}
					}
				}
			};

			if (t != null) {
				StartCoroutine(ApiManager.Instance.ReEnterTournament(t.id, success, fail));
			}
		}

		#region Join Tournament
		public void Join() {
			TacoManager.OpenMessage(TacoConfig.TacoPublicJoining);
			var t = TacoManager.Target;

			Action<string> success = (string data) => {
				var r = JsonUtility.FromJson<JoinTournamentResult>(data);
				if (r.success) {
					TacoManager.SetTarget(t);

					Decimal prize = decimal.Parse(r.tournament.prize.ToString("F2"));
					Decimal entryFee = decimal.Parse(r.tournament.entryFee.ToString("F2"));

					string replacedString = string.Empty;
					if (r.tournament.typeCurrency == 0) {
						replacedString = TacoConfig.TacoJoinPublicSuccessBody.Replace("&userFunds", r.userFunds);
						replacedString = replacedString.Replace("&entryFee", entryFee.ToString());
						replacedString = replacedString.Replace("&prize", prize.ToString());
					}
					else {
						replacedString = TacoConfig.TacoJoinPublicSuccessBody.Replace("&userFunds", " T " + r.userFunds);
						replacedString = replacedString.Replace("&entryFee", " T " + entryFee.ToString());
						replacedString = replacedString.Replace("&prize", " T " + prize.ToString());
					}
					TacoManager.CloseMessage();

					TacoManager.OpenModalJoinSuccessPanel(r.tournament, TacoConfig.JoinSuccessNotice);
					double val = 0;
					if (r.tournament.typeCurrency == 0) {
						//real money
						if (double.TryParse(r.userFunds, out val)) {
							TacoManager.UpdateFundsWithToken(val, r.currencyValue, r.ticket, r.userBonus);
						}
					}
					else {
						//userFunds is 'gtoken' now
						if (double.TryParse(r.currencyValue, out val)) {
							TacoManager.UpdateFundsWithToken(val, r.userFunds, r.ticket);
						}
					}

					ShowTournamentPanel(CurrentSubPanel);
				}
				else {
					TacoManager.CloseMessage();
					if (r.addFund) {
						if (t.typeCurrency == 0) {
							TacoManager.OpenModalFundErrorJoinPanel(TacoConfig.TacoJoinPublicNoFundHead, t);
						}
						else {
							TacoManager.OpenModalTacoErrorJoinPanel(TacoConfig.TacoJoinPublicNoTacoHead, t);
						}
					}
					else {
						TacoManager.OpenModalAlreadyJoinedPanel(TacoConfig.TacoJoinAlreadyHead, r.message);
					}
				}
			};

			Action<string, string> fail = (string data, string error) => {
				var msg = data + (string.IsNullOrEmpty(error) ? string.Empty : " : " + error);
				Debug.Log("Error adding funds  : " + msg);

				if (!string.IsNullOrEmpty(data)) {
					var r = JsonUtility.FromJson<CreateTournamentResult>(data);
					TacoManager.CloseMessage();
					if (!string.IsNullOrEmpty(r.message)) {
						if (r.forbidden) {
							TournamentSubmitForbidden(r.message);
						}
						else {
							TacoManager.OpenModalTournamentJoinErrorPanel(TacoConfig.TacoJoinPublicErrorHead, r.message, ModalFunctions.FundsScreen);
						}
					}
				}
			};

			if (t != null) {
				StartCoroutine(ApiManager.Instance.JoinTournament(t.typeCurrency, t.id, TacoSetup.Instance.SiteId, TacoManager.GameId, TacoManager.User.token, success, fail));
			}
		}
		#endregion
	}
}
