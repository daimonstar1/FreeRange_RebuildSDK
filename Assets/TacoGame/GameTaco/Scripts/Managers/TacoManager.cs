using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;


namespace GameTaco {

	public class TacoManager : MonoBehaviour {
        public static bool Initialized { get; private set; } = false;
        // Temporary solution for debugging canvases with ease
        // We shouldn't have to disable all canvases that are blocking the canvas we want to work on
        // then have to reenable them all again before playing
        [Header("Canvases")]
		public GameObject tacoOurGamesCanvas;
		private static GameObject myLeaderboardPanel = null;
		// canvas
		public static GameObject TacoBlockingCanvas = null;
		public static GameObject TacoCommonCanvas = null;
		public static GameObject TacoAuthCanvas = null;
		public static GameObject TacoTournamentsCanvas = null;
		public static GameObject TacoTournamentsFakeCanvas = null;
		public static GameObject TacoOurGamesCanvas = null;
		public static GameObject ManageTournamentPanel = null;
		public static GameObject FilterListPanel;
		public static GameObject SortListPanel;
		public static GameObject MyProfilePanel;
		public static GameObject BalancesPanel;
		public static GameObject MyTransactionPanel;

		// panels
		public static GameObject FundsRunningLowPanel = null;

		public static GameObject DepositDisablePanel = null;
		public static GameObject MyTournamentsPanel = null;
		public static GameObject MyTournamentsFakePanel = null;
		public static GameObject JoinPublicPanel = null;
		public static GameObject CreatePublicPanel = null;

		// panels common
		public static GameObject ModalPanel = null;
		public static GameObject MessagePanel = null;
		public static GameObject TacoFoldoutPanel = null;
		public static GameObject FeaturedGamesPanel = null;

		// panels auth
		public static GameObject LoginPanelObject;
		public static GameObject RegisterPanelObject;

		public static string GameName;
		public static int GameId;
		public static Image MyAvatarImage;
		public static TacoUser User;
		public static Tournament Target;

		// if not null - a game is being played
		public static string GameToken = null;

		//button and text
		private static GameObject tacoCloseButton;
		private static GameObject tacoOurGame;
		private static GameObject TacoHeaderImage;
		private static GameObject logoutButton;
		private static GameObject AuthMainMenu;
		private static GameObject MoneyMainMenu;
		private static GameObject tacoNotification;

		private static Text usernameText;

		private static GameObject tacoHeaderFooter;

		private static int currentUserId = 0;
		public static int columnSortType = 0; //0 vs 1
		private static int openAdminCount;
		public static string columnSortName = "-";
		public static bool isOpenPopup;

		private static GameObject HeaderFoldoutAfterSignIn;
		private static GameObject HeaderFoldoutBeforeSignIn;

		public static Dictionary<string, GameObject> PanelList = new Dictionary<string, GameObject>();
		public static List<Image> AvatarInPanelList = new List<Image>();
		public static Dictionary<string, Sprite> cacheAvatars = new Dictionary<string, Sprite>();

		private void CreatePanelList() {
			GameObject panelList = GameObject.Find("PanelList");
			CreateAvatarInModalList(panelList);
			foreach (Transform child in panelList.transform) {
				TacoManager.PanelList[child.name] = child.gameObject;
				child.gameObject.SetActive(false);
			}
		}

		private static void HidePanelList() {
			GameObject panelList = GameObject.Find("PanelList");
			foreach (Transform child in panelList.transform) {
				Debug.Log("name: " + child.name);
				child.gameObject.SetActive(false);
			}
		}

		private static void CreateAvatarInModalList(GameObject panelList) {
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("AvatarInPanel");
			for (int i = 0; i < gameObjects.Length; i++) {
				AvatarInPanelList.Add(gameObjects[i].GetComponent<Image>());
			}
		}

		public static void TurnTacoHeaderFooter(bool status) {
			if (tacoHeaderFooter != null) tacoHeaderFooter.SetActive(status);
		}

		public static void TurnPanelOn(string panelName) {
			isOpenPopup = true;
			TacoManager.PanelList[panelName].SetActive(true);
		}

		void Start() {
			TacoBlockingCanvas = GameObject.Find(CanvasNames.TacoBlockingCanvas);
			TacoCommonCanvas = GameObject.Find(CanvasNames.TacoCommonCanvas);
			TacoAuthCanvas = GameObject.Find(CanvasNames.TacoAuthCanvas);
			TacoTournamentsCanvas = GameObject.Find(CanvasNames.TacoTournamentsCanvas);
			TacoTournamentsFakeCanvas = GameObject.Find(CanvasNames.TacoTournamentsFakeCanvas);
			TacoOurGamesCanvas = GameObject.Find(CanvasNames.TacoOurGamesCanvas);

			// set canvas elements to on for finding by names to work
			TacoBlockingCanvas.SetActive(true);
			TacoCommonCanvas.SetActive(true);
			if(TacoTournamentsFakeCanvas != null )
				TacoTournamentsFakeCanvas.SetActive(true);
			TacoAuthCanvas.SetActive(true);
			TacoTournamentsCanvas.SetActive(true);

			// panels
			DepositDisablePanel = GameObject.Find(PanelNames.DepositDisablePanel);
			FundsRunningLowPanel = GameObject.Find(PanelNames.FundsRunningLowPanel);
			MyTournamentsFakePanel = GameObject.Find(PanelNames.MyTournamentsFakePanel);
			MyTournamentsPanel = GameObject.Find(PanelNames.MyTournamentsPanel);
			CreatePublicPanel = GameObject.Find(PanelNames.CreatePublicPanel);
			JoinPublicPanel = GameObject.Find(PanelNames.JoinPublicPanel);
			myLeaderboardPanel = GameObject.Find(PanelNames.MyLeaderboardPanel);
			ManageTournamentPanel = GameObject.Find(PanelNames.ManageTournamentPanel);
			FilterListPanel = GameObject.Find(PanelNames.FilterListPanel);
			SortListPanel = GameObject.Find(PanelNames.SortListPanel);
			MyProfilePanel = GameObject.Find(PanelNames.ProfilePanel);
			BalancesPanel = GameObject.Find(PanelNames.BalancePanel);
			MyTransactionPanel = GameObject.Find(PanelNames.MyTransactionPanel);

			// common
			ModalPanel = GameObject.Find(PanelNames.Modal);
			MessagePanel = GameObject.Find(PanelNames.Message);
			TacoFoldoutPanel = GameObject.Find(PanelNames.Foldout);
			FeaturedGamesPanel = GameObject.Find(PanelNames.FeaturedGamesPanel);

			RegisterPanelObject = GameObject.Find(PanelNames.RegisterPanel);
			LoginPanelObject = GameObject.Find(PanelNames.LoginPanel);

			usernameText = GameObject.Find("UsernameText").GetComponent<Text>();

			// buttons
			tacoCloseButton = GameObject.Find("TacoCloseButton");
			TacoHeaderImage = GameObject.Find("TacoHeaderImage");

			HeaderFoldoutAfterSignIn = GameObject.Find("HeaderFoldoutAfterSignIn");
			HeaderFoldoutBeforeSignIn = GameObject.Find("HeaderFoldoutBeforeSignIn");

			AuthMainMenu = GameObject.Find("AuthMainMenu");
			MoneyMainMenu = GameObject.Find("MoneyMainMenu");
			logoutButton = GameObject.Find("LogoutButton");
			tacoOurGame = GameObject.Find("OurGame");
			tacoNotification = GameObject.Find("Notification");
			//resendInvitationButton = GameObject.Find ("resendInvitationButton").GetComponent<Button> ();

			tacoCloseButton.SetActive(false);

			// misc
			tacoHeaderFooter = GameObject.Find("TacoHeaderFooter");
			MyAvatarImage = GameObject.Find("MyAvatarImage").GetComponent<Image>();
			ToggleButtonWhenLogin(false);
			CreatePanelList();
			Init();
		}

		public static void Init() {
			SetAllPanels(false);
			SetAllCanvas(false);
            Initialized = true;
        }

		public static void SetToMainMenu() {
			TacoSetup.Instance.ToMainMenu();
		}

		public static void OpenTaco() {
			Setup();
		}
		// called when game wants to open taco for tournament play
		public static void OpenTacoFromGame() {
			Setup();
		}

		public static void Setup() {

			if (UserLoggedIn()) {
				ShowPanel(PanelNames.MyTournamentsPanel);
			}
			else {
				//ShowPanel(PanelNames.LoginPanel);
				ShowPanel(PanelNames.MyTournamentsFakePanel);
			}

		}

		public static bool UserLoggedIn() {
			return currentUserId != 0;
		}


		public static void UpdateAvatar(string username, double funds, string gToken, string ticket, string avatarOnline) {

			usernameText.text = "Hello " + username + "!";

			if (!string.IsNullOrEmpty(avatarOnline)) {
				if (avatarOnline.StartsWith("https") || avatarOnline.StartsWith("http")) {
					TacoSetup.Instance.StartCoroutine(WWWAvatarSocial(avatarOnline));
				}
				else {
					avatarOnline = avatarOnline.Split(';')[1];
					avatarOnline = avatarOnline.Split(',')[1];
					byte[] decodedBytes = Convert.FromBase64String(avatarOnline);

					Texture2D avatarImage = MyAvatarImage.sprite.texture;
					avatarImage.LoadImage(decodedBytes);
					Sprite sprite = Sprite.Create(avatarImage, new Rect(0, 0, avatarImage.width, avatarImage.height), new Vector2(.5f, .5f));
					MyAvatarImage.sprite = sprite;
				}
			}

			UpdateFundsWithToken(funds, gToken, ticket);

		}



		private static void UpdateAvatarInModal(Sprite sprite) {
			for (int i = 0; i < AvatarInPanelList.Count; i++) {
				AvatarInPanelList[i].sprite = sprite;
			}
		}

		public static IEnumerator WWWAvatarSocial(string url) {
			url = url.Replace("https", "http");
			WWW www = new WWW(url);
			yield return www;
			if (www.texture != null) {
				Sprite sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
				MyAvatarImage.sprite = sprite;
				UpdateAvatarInModal(sprite);
			}
			www.Dispose();
			www = null;
		}

		public static void UpdateAvatarOnline(string username, double funds, string avatarBase) {
			usernameText.text = "Hello " + username + "!";

			int avatar = UnityEngine.Random.Range(0, 7);
			MyAvatarImage.sprite = TacoConfig.Instance.GetAvatarSprite(avatar);

			if (!string.IsNullOrEmpty(avatarBase)) {
				if (avatarBase.StartsWith("http")) {
					TacoSetup.Instance.StartCoroutine(ApiManager.Instance.WWWAvatarSocial(avatarBase, MyAvatarImage));
				}
				else {
					avatarBase = avatarBase.Split(';')[1];
					avatarBase = avatarBase.Split(',')[1];
					byte[] decodedBytes = Convert.FromBase64String(avatarBase);

					Texture2D avatarImage = MyAvatarImage.sprite.texture;
					avatarImage.LoadImage(decodedBytes);
					Sprite sprite = Sprite.Create(avatarImage, new Rect(0, 0, avatarImage.width, avatarImage.height), new Vector2(.5f, .5f));
					MyAvatarImage.sprite = sprite;
				}
			}

			UpdateFundsOnly(funds);
		}

		public static void CloseTaco() {
			tacoCloseButton.SetActive(false);
			SetAllCanvas(false);
			TacoCommonCanvas.SetActive(true);
		}

		public static void SetAllCanvas(bool active) {
			// Never turn off >>>> TacoCommonCanvas.SetActive(active);
			TacoBlockingCanvas.SetActive(active);
			TacoAuthCanvas.SetActive(active);
			TacoTournamentsCanvas.SetActive(active);
			if(TacoTournamentsFakeCanvas != null)TacoTournamentsFakeCanvas.SetActive(active);
			TacoOurGamesCanvas.SetActive(active);
		}

		public static void UpdateFundsWithToken(double funds, string gToken, string ticket) {
			User.funds = funds;
			User.gToken = gToken;
			User.ticket = ticket;
		}

		public static void UpdateFundsWithToken(double funds, string gToken, string ticket, double bonus) {
			User.bonus = bonus;
			User.funds = funds;
			User.gToken = gToken;
			User.ticket = ticket;
		}

		public static void UpdateTokenOnly(string gToken) {
			User.gToken = gToken;
		}

		public static void UpdateTicketOnly(string ticket) {
			User.ticket = ticket;
		}

		public static void UpdateTicketAndTokenOnly(string token, string ticket) {
			User.ticket = ticket;
			User.gToken = token;
		}

		public static void UpdateFundsOnly(double funds) {
			User.funds = funds;
		}

		public static void UpdateFundsOnly(double funds, double bonus) {
			User.funds = funds;
			User.bonus = bonus;
		}

		public static bool ValidateEmail(string email) {
			return Regex.IsMatch(email, @"^[a-zA-Z0-9]+((_|\-|\.|\+){0,1}[a-zA-Z0-9]+)*@[a-zA-Z0-9]+((_|\-){0,1}[a-zA-Z0-9]+)*(\.[a-zA-Z0-9]+)+$", RegexOptions.IgnoreCase);
		}

		public static bool ValidateUsername(string username) {
			return Regex.IsMatch(username, @"^[a-zA-Z0-9@\._]+$", RegexOptions.IgnoreCase);
		}

		public static void OpenHowToPlayPanel() {
			if (PlayerPrefs.GetString("firstTime", "no") != "yes") {
				TurnPanelOn("SuccessLoginBrief");
				PlayerPrefs.SetString("firstTime", "yes");
			}
		}

		public static string FormatCash(double funds) {
			return "$ " + funds.ToString("N2");
		}

		public static string FormatGTokens(double funds) {
			return string.Format("{0:n0}", funds);
		}

		public static string FormatRP(string funds) {
			return string.Format("{0:n0}", double.Parse(funds));
		}

		public static string FormatRP(int funds) {
			return string.Format("{0:n0}", (double)funds);
		}

		public static string FormatGTokens2(double funds) {
			return string.Format("{0:n0}", funds);
		}

		public static string FormatDate(string time, char seperator = '/') {
			return time.Substring(5, 2) + seperator + time.Substring(8, 2) + seperator + time.Substring(0, 4);
		}

		public static string FormatMoney(double funds, int currencyType) {
			string formatted = string.Empty;
			if (currencyType == 0) {
				formatted = FormatCash(funds);
			}
			else if (currencyType == 1) {
				formatted = FormatGTokens(funds);
			}
			return formatted;
		}


		public static void UpdateUser(SessionResult result, string token) {
			TacoUser user = new TacoUser {
				email = result.user.email,
				userId = result.user.id,
				funds = result.user.funds,
				bonus = result.user.bonus_cash,
				token = token,
				webToken = result.signon_token,
				ticket = result.user.ticket,
				gToken = result.gToken,
				highScoreUser = result.highScoreUser,
				avatarOnline = result.avatar,
				name = string.IsNullOrEmpty(result.user.userName) ? result.user.name : result.user.userName,
				displayName = string.IsNullOrEmpty(result.user.displayName) ? result.user.name : result.user.displayName,
				referedCode = result.user.referCode,
				contactProfile = result.contactProfile
			};

			User = user;
			UpdateAvatar(user.name, result.user.funds, result.gToken, result.user.ticket, result.user.avatar);
			GameId = result.game.id;
			GameName = result.game.name;
			currentUserId = result.user.id;

			Setup();
			ToggleButtonWhenLogin(true);
			CloseMessage();
		}

		//TODO make sure result returns autoLogin - don't pass it in
		public static void CreateUser(LoginResult result) {
			TacoUser user = new TacoUser {
				email = result.mail,
				userId = result.userId,
				funds = 0.0f,
				bonus = result.bonus_cash,
				webToken = result.signon_token,
				token = result.token,
				ticket = result.ticket,
				gToken = result.gToken,
				highScoreUser = result.highScoreUser,
				avatarOnline = result.avatar,
				name = string.IsNullOrEmpty(result.userName) ? result.name : result.userName,
				displayName = string.IsNullOrEmpty(result.displayName) ? result.name : result.displayName,
				referedCode = result.referCode,
				contactProfile = result.contactProfile
			};
			User = user;
			UpdateAvatar(user.name, result.funds, result.gToken, result.ticket, result.avatar);

			GameId = result.gameId;
			GameName = result.gameName;
			currentUserId = result.userId;

			ShowPanel(PanelNames.MyTournamentsPanel);
			ToggleButtonWhenLogin(true);
			CloseMessage();
		}

		public static void ToggleButtonWhenLogin(bool display) {
			HeaderFoldoutAfterSignIn.SetActive(display);
			HeaderFoldoutBeforeSignIn.SetActive(!display);

			tacoOurGame.SetActive(true);
			logoutButton.SetActive(display);
			tacoNotification.SetActive(display);
			TacoHeaderImage.SetActive(true);
			if(MoneyMainMenu != null) MoneyMainMenu.SetActive(display);
			if(AuthMainMenu != null) AuthMainMenu.SetActive(!display);
			TacoSetup.Instance.ToggleMenuButton(display);
		}

		public static string NameToShowFoldout(string name, string email, string displayName) {
			string textWillShow = string.Empty;
			if (!string.IsNullOrEmpty(displayName)) {
				textWillShow = displayName;
			}
			else if (!string.IsNullOrEmpty(name)) {
				textWillShow = name;
			}
			else {
				textWillShow = email;
			}
			if (textWillShow.Length > 13) {
				textWillShow = textWillShow.Substring(0, 13);
				textWillShow = textWillShow + "...";
			}
			return textWillShow;
		}

		public static void AskToLogoutUser() {
			TacoManager.CloseAllModals();
			OpenModalLogoutConfirmPanel();
			//OpenModal (  TacoConfig.TacoSureLogoutModalHeader , TacoConfig.TacoSureLogoutModalBody, null, ModalFunctions.LogoutUser, TacoConfig.CloseSprite, null);
		}

		public static void LogoutUser() {
			TournamentManager.Instance.CurrentSubPanel = string.Empty;
			User = null;
			MyAvatarImage.sprite = null;
			PrivateTournamentsList.Instance.Destroy();
			PublicTournamentsList.Instance.Destroy();

			SetToMainMenu();

			GameId = 0;
			currentUserId = 0;

			// clear the user preferences that were for this user
			SetPreferenceString(UserPreferences.userToken, null);
			SetPreference(UserPreferences.autoLogin, 0);

			TacoAuthCanvas.SetActive(true);

			LoginPanelObject.GetComponent<LoginPanel>().Init();
			RegisterPanelObject.GetComponent<RegisterPanel>().Init();
			ToggleButtonWhenLogin(false);
			HidePanelList();
			CloseTaco();
			TournamentManager.Instance.SetDefaultMoneytext();
		}

		public static int GetDropDownActiveName(List<Dropdown.OptionData> toggleGroup, int index) {
			string activeToggle = toggleGroup[index].text;
			if (activeToggle != null) {
				int activeInt = Int32.Parse(activeToggle);
				return activeInt;
			}
			return 0;
		}

		public static int GetTogglActiveName(ToggleGroup toggleGroup) {
			Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault();
			if (activeToggle) {
				int activeInt = Int32.Parse(activeToggle.name);
				return activeInt;
			}
			return 0;
		}

		public static void ShowPanel(string panelName) {
			// don't hold any panel state, you can show any panel - any time
			SetAllCanvas(false);
			SetAllPanels(false);

			// opening a Taco panel : so show the close button
			tacoCloseButton.SetActive(true);

			// open the one you want and dependent stuff
			switch (panelName) {
				case PanelNames.MyLeaderboardPanelFromEndGame:
					tacoCloseButton.SetActive(true);
					TacoTournamentsCanvas.SetActive(true);
					TacoBlockingCanvas.SetActive(true);
					TacoCommonCanvas.SetActive(true);
					myLeaderboardPanel.SetActive(true);
					SetToMainMenu();
					LeaderboardList.Instance.LoadLeaderboard(TacoManager.Target);
					break;

				case PanelNames.RegisterPanel:
					TacoSetup.Instance.LogEvent("signup_page");
					SetAllPanels(false);
					TacoBlockingCanvas.SetActive(true);
					TacoAuthCanvas.SetActive(true);
					RegisterPanelObject.SetActive(true);
					break;

				case PanelNames.LoginPanel:
					SetAllPanels(false);
					if (PlayerPrefs.GetInt(UserPreferences.autoLogin) == 1) {
						string userToken = PlayerPrefs.GetString(UserPreferences.userToken);
						ApiManager.Instance.GetSession(userToken);
					}
					TacoBlockingCanvas.SetActive(true);
					TacoAuthCanvas.SetActive(true);
					LoginPanelObject.SetActive(true);
					break;

				case PanelNames.FeaturedGamesPanel:
					SetAllPanels(false);
					TacoBlockingCanvas.SetActive(true);
					TacoOurGamesCanvas.SetActive(true);
					FeaturedGamesPanel.SetActive(true);
					FeaturedGamesPanel featuredGamesPanel = TacoOurGamesCanvas.GetComponentInChildren<FeaturedGamesPanel>();
					featuredGamesPanel.GetOurGames();
					break;

				case PanelNames.MyTournamentsPanel:
					SetAllPanels(false);
					TacoBlockingCanvas.SetActive(true);
					TacoTournamentsCanvas.SetActive(true);
					MyTournamentsPanel.SetActive(true);
					FundsRunningLowPanel.SetActive(false);
					DepositDisablePanel.SetActive(false);
					TournamentManager.Instance.Open();
					break;


				case PanelNames.JoinPublicPanel:
					SetAllPanels(false);
					TacoBlockingCanvas.SetActive(true);
					TacoTournamentsCanvas.SetActive(true);
					MyTournamentsPanel.SetActive(true);
					JoinPublicPanel.SetActive(true);
					break;

				case PanelNames.CreatePublicPanel:
					SetAllPanels(false);
					TacoBlockingCanvas.SetActive(true);
					TacoTournamentsCanvas.SetActive(true);
					CreatePublicPanel.SetActive(true);
					break;

				case PanelNames.ManageTournamentPanel:
					MyTournamentsPanel.SetActive(true);//active tournament so that 'tournament lsit' button in leaderboard function properly
					TacoTournamentsCanvas.SetActive(true);
					TacoBlockingCanvas.SetActive(true);
					ManageTournamentPanel.SetActive(true);
					ManageTournament.Instance.LoadInformation(TacoManager.Target);
					break;

				case PanelNames.FilterListPanel:
				case PanelNames.FilterActiveListPanel:
					FilterTournament.Instance.Init(panelName == PanelNames.FilterActiveListPanel);
					SetAllPanels(false);
					TacoTournamentsCanvas.SetActive(true);
					TacoBlockingCanvas.SetActive(true);
					FilterListPanel.SetActive(true);
					break;

				case PanelNames.SortListPanel:
				case PanelNames.SortActiveListPanel:
					SortTournament.Instance.Init(panelName == PanelNames.SortActiveListPanel);
					SetAllPanels(false);
					TacoTournamentsCanvas.SetActive(true);
					TacoBlockingCanvas.SetActive(true);
					SortListPanel.SetActive(true);
					break;

				case PanelNames.ProfilePanel:
          TacoSetup.Instance.ToggleTacoHeaderFooter(true);
					SetAllPanels(false);
					myLeaderboardPanel.SetActive(false);
					TacoTournamentsCanvas.SetActive(true);
					TacoBlockingCanvas.SetActive(true);
					MyProfilePanel.SetActive(true);
					ProfileManager.Instance.Init();
					break;

				case PanelNames.MyTransactionPanel:
					SetAllPanels(false);
					myLeaderboardPanel.SetActive(false);
					TacoTournamentsCanvas.SetActive(true);
					TacoBlockingCanvas.SetActive(true);
					MyTransactionPanel.SetActive(true);
					TransactionList.Instance.GetTransactions();
					break;

				case PanelNames.MyTournamentsFakePanel:
					SetAllPanels(false);
					TacoBlockingCanvas.SetActive(true);
					TacoTournamentsFakeCanvas.SetActive(true);
					MyTournamentsFakePanel.SetActive(true);
					TournamentManagerFake.Instance.Open();
					break;

			}
		}

		public static void ReturnToTournamentBoard() {
			SetAllPanels(false);
			SetAllCanvas(false);
			TacoBlockingCanvas.SetActive(true);
			TacoTournamentsCanvas.SetActive(true);
			MyTournamentsPanel.SetActive(true);
			TournamentManager.Instance.ShowTournamentPanel(TournamentManager.Instance.CurrentSubPanel);
		}

		public void GoToTournamentPagePress() {
			if (TacoManager.User != null) {
				TacoManager.ReturnToTournamentBoard();
			}
			else {
				TacoManager.OpenTacoFromGame();
			}
		}

		public static void SetPanelActiveByName(string panelName, bool active) {
			switch (panelName) {
				case PanelNames.RegisterPanel:
					RegisterPanelObject.SetActive(active);
					break;

				case PanelNames.LoginPanel:
					LoginPanelObject.SetActive(active);
					break;

				case PanelNames.FeaturedGamesPanel:
					FeaturedGamesPanel.SetActive(active);
					break;

				case PanelNames.MyTournamentsPanel:
					MyTournamentsPanel.SetActive(active);
					break;

				case PanelNames.JoinPublicPanel:
					JoinPublicPanel.SetActive(active);
					break;

				case PanelNames.CreatePublicPanel:
					CreatePublicPanel.SetActive(active);
					break;
			}
		}

		public static void OpenMessage(String title) {
			TacoCommonCanvas.SetActive(true);
			MessagePanel.GetComponent<TacoMessagePanel>().Open(TacoConfig.Processing);
			MessagePanel.SetActive(true);
		}

		public static void OpenFoldout() {
			TacoCommonCanvas.SetActive(true);
			TacoFoldoutPanel.SetActive(true);
		}

		public void HideFoldout() {
			TacoFoldoutPanel.SetActive(false);
		}

		public static void CloseFoldout() {
			TacoFoldoutPanel.SetActive(false);
		}

		public static void CloseMessage() {
			MessagePanel.SetActive(false);
		}

		public static void CloseModal() {
			ModalPanel.SetActive(false);
		}

		public static GameObject GetPanel(string panelName) {
			return TacoManager.PanelList[panelName];
		}

		public static void OpenModalDepositDisablePanel() {
			GameObject panel = GetPanel("DepositDisablePanel");
			isOpenPopup = true;
			panel.SetActive(true);
			Debug.Log("OpenModalDepositDisablePanel");
			GameObject panel2 = GetPanel("FundsRunningLowPanel");
			panel2.SetActive(false);
		}
		public static void OpenModalFundsRunningLowPanel() {
			GameObject panel = GetPanel("FundsRunningLowPanel");
			isOpenPopup = true;
			panel.SetActive(true);
			//TournamentManager.Instance.TournamentCallback = ModalFunctions.TournamentSubmit;
		}

		public static void OpenModalCreateTournamentPanel() {
			GameObject panel = GetPanel("CreateTournament");
			isOpenPopup = true;
			panel.SetActive(true);
			TournamentManager.Instance.TournamentCallback = ModalFunctions.TournamentSubmit;
		}

		public static void OpenModalEnterTournamentPanel(string notice, string tournamentFee, int typeCurrency) {
			GameObject panel = GetPanel("EnterTournament");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
			panel.transform.Find("Main/EntryFee/Money").GetComponent<Image>().sprite = TacoConfig.currencySprites[typeCurrency];
			panel.transform.Find("Main/EntryFee/Fee").GetComponent<Text>().text = tournamentFee;
			TournamentManager.Instance.TournamentCallback = ModalFunctions.JoinTournament;
		}

		public static void OpenModalEnterTournamentPlayAgainPanel(string notice, string tournamentFee, int typeCurrency) {
			GameObject panel = GetPanel("EnterTournamentPlayAgain");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
			panel.transform.Find("Main/EntryFee/Money").GetComponent<Image>().sprite = TacoConfig.currencySprites[typeCurrency];
			panel.transform.Find("Main/EntryFee/Fee").GetComponent<Text>().text = tournamentFee;
			TournamentManager.Instance.TournamentCallback = ModalFunctions.JoinPlayAgainTournament;
		}

		public static void OpenModalReEnterPanel(string notice, string tournamentFee) {
			GameObject panel = GetPanel("ReEnter");
			panel.SetActive(true);
			isOpenPopup = true;
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
			//panel.transform.Find ("Main/EntryFee/Money").GetComponent<Image> ().sprite = TacoConfig.currencySprites[typeCurrency];
			panel.transform.Find("Main/EntryFee/Fee").GetComponent<Text>().text = tournamentFee;
			TournamentManager.Instance.TournamentCallback = ModalFunctions.ReEnterTournament;
		}

		public static void OpenModalJoinSuccessPanel(Tournament t, string notice) {
			GameObject panel = GetPanel("JoinSuccess");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Type").GetComponent<Text>().text = TacoConfig.TournamentTypeNotice.
        Replace("&gameName", TacoSetup.Instance.gameName).
				Replace("&type", t.Type);
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
			InitCoundDown(panel, t);
			TournamentManager.Instance.TournamentCallback = ModalFunctions.ConfirmPlay;
		}

		public static void OpenModalPlayTournamentPanel(Tournament tournament = null) {
			GameObject panel = GetPanel("ConfirmPlay");
			isOpenPopup = true;
			panel.SetActive(true);
			if (tournament == null) tournament = TacoManager.Target;
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = TacoConfig.TournamentTypeNotice.
       	Replace("&gameName", TacoSetup.Instance.gameName).
				Replace("&type", tournament.accessType == "public" ? "Public" : "Private");
			TournamentManager.Instance.TournamentCallback = ModalFunctions.WaitingPlay;
		}

		public static void OpenModalDailyTokenPanel(string tokenValue) {
			GameObject panel = GetPanel("DailyToken");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/UserWelcome").GetComponent<Text>().text = "Wellcome back " + TacoManager.User.name + "!";
			panel.transform.Find("Main/TokenText").GetComponent<Text>().text = "+" + tokenValue;
			panel.transform.Find("Main/TokenNotice").GetComponent<Text>().text = "You've received (" + tokenValue + ") Daily Tokens";
		}

		public static void OpenClaimTokenPanel(string header, string tokenValue, string notice) {
			GameObject panel = GetPanel("ClaimToken");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/TokenTitle").GetComponent<Text>().text = header;
			panel.transform.Find("Main/TokenText").GetComponent<Text>().text = tokenValue;
			panel.transform.Find("Main/TokenNotice").GetComponent<Text>().text = notice;
		}

		public static void OpenModalWarningTimePanel(Tournament t) {
			GameObject panel = GetPanel("WarningTime");
			isOpenPopup = true;
			panel.SetActive(true);
			InitCoundDown(panel, t);
		}

		public static void OpenModalCreateSuccessTimePanel(Tournament t) {
			GameObject panel = GetPanel("CreateSuccess");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/TournamentType").GetComponent<Text>().text = "\'" + TacoSetup.Instance.gameName + " " + (t.accessType == "public" ? "Public" : "Private") + " Tournament\'";
			InitCoundDown(panel, t);
			TournamentManager.Instance.TournamentCallback = ModalFunctions.ConfirmPlay;
		}

		public static void OpenModalManageTournamentTimePanel(Tournament t) {
			GameObject panel = GetPanel("ManageTournament");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Body/ManageTournamentDetails").GetComponent<Text>().text = "Lasting " + TacoConfig.Pluralize(t.TimeRemaining, "Day") + " | " + t.size + " Players | " + TacoConfig.Pluralize(t.prize_structure, "Winner");
			Transform emailsPanel = panel.transform.Find("Main/ScrollView/Viewport/Content/EmailPanel");
			RectTransform contentPanel = emailsPanel.parent.GetComponent<RectTransform>();
			float contentWidth = contentPanel.rect.width;
			foreach (Transform child in emailsPanel) {
				Destroy(child.gameObject);
			}
			float spacing = 15;
			float width = (contentPanel.rect.width - spacing) / 2;
			float height = 90;

			List<TournamentInvite> inviteEmailTexts = new List<TournamentInvite>();
			t.invitedEmails = new String[] { };

			for (int i = 0; i < t.size; i++) {
				GameObject ob = Instantiate(TacoConfig.InviteInputPrefab, Vector3.zero, Quaternion.identity, emailsPanel) as GameObject;
				RectTransform r = ob.GetComponent<RectTransform>();
				r.localPosition = new Vector3(width * (i % 2 - 0.5f), height * (-0.5f - (int)(i / 2)) - spacing);
				r.sizeDelta = new Vector2(width - spacing, height - spacing);

				TournamentInvite invite = ob.GetComponent<TournamentInvite>();

				if (i > t.invitedEmails.Length) {
					invite.SetInteractable(false);
				}
				if (i > 0) {
					inviteEmailTexts[i - 1].next = invite;
					invite.prev = inviteEmailTexts[i - 1];
				}

				if (i < t.invitedEmails.Length) {
					invite.FillWithValidateData(t.invitedEmails[i], true);
				}

				if (i == 0 && t.invitedEmails.Length == 0) {
					invite.SetFocus();
				}
				else if (i > 0 && string.IsNullOrEmpty(invite.EmailInput.text) && !string.IsNullOrEmpty(inviteEmailTexts[i - 1].EmailInput.text)) {
					invite.SetFocus();
				}

				inviteEmailTexts.Add(invite);
			}
			contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, Mathf.Max(3 * height, (int)(t.size / 2) * height + spacing));
			TacoManager.SetTarget(t);

			InitCoundDown(panel, t);
			TournamentManager.Instance.TournamentCallback = ModalFunctions.InviteFriends;
		}

		private static void InitCoundDown(GameObject panel, Tournament t) {
			TacoModalManager.Instance.SetupCountdown(panel.transform.Find("Main/Box/CountdownTimer").GetComponent<Text>(), (Single)t.RemainingTimeSpan.TotalSeconds);
		}

		public static void OpenModalRemovePlayerConfirmPanel(string email, Action removeAction) {
			GameObject panel = GetPanel("RemovePlayerConfirm");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/EmailBox/EmailText").GetComponent<Text>().text = email;
			Button removePlayerButton = panel.transform.Find("Main/Buttons/RemovePlayer").GetComponent<Button>();
			removePlayerButton.onClick.RemoveAllListeners();
			Action callback = () => {
				panel.SetActive(false);
				removeAction();
			};
			removePlayerButton.onClick.AddListener(() => {
				TournamentManager.Instance.RemoveFriend(email, callback);

			});
		}

		public static void OpenModalRemovePlayerNoticePanel() {
			GetPanel("RemovePlayerNotice").SetActive(true);
			isOpenPopup = true;
		}

		private static void OpenModalLogoutConfirmPanel() {
			GetPanel("LogoutConfirm").SetActive(true);
			isOpenPopup = true;
			TournamentManager.Instance.TournamentCallback = ModalFunctions.LogoutUser;
		}

		public static void OpenModalIncorrectVersionPanel(string notice) {
			GameObject panel = GetPanel("IncorrectVersion");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
			TournamentManager.Instance.TournamentCallback = ModalFunctions.ExitApp;
		}

		public static void OpenModalLoginFailedPanel(string errorMsg) {
			GameObject panel = GetPanel("LoginFailed");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = errorMsg;
		}

		public static void OpenModalConnectionErrorPanel(string errorMsg) {
			GameObject panel = GetPanel("ConnectionError");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = errorMsg;
		}

		public static void OpenModalResetPasswordPanel() {
			GameObject panel = GetPanel("ResetPassword");
			isOpenPopup = true;
			panel.SetActive(true);
			TournamentManager.Instance.TournamentCallback = ModalFunctions.ResetPassword;
		}

		public static void OpenModalGeneralResultPanel(bool success, string header, string msg) {
			GameObject panel = GetPanel("ResetResult");
			isOpenPopup = true;
			panel.transform.Find("Main/Header").transform.GetChild(1).gameObject.SetActive(success);
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = msg;
			panel.transform.Find("Main/Header/Text").GetComponent<Text>().text = header;
			panel.SetActive(true);
		}

		public static void OpenModalTournamentExistedPanel(string notice, Tournament t) {
			GameObject panel = GetPanel("TournamentExisted");
			isOpenPopup = true;
			Text btnText = panel.transform.Find("Main/Buttons/Action/Text").GetComponent<Text>();
			panel.SetActive(true);
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
			if (Array.IndexOf(t.entryIds, TacoManager.User.userId) > -1) {
				btnText.text = "Result";
				TournamentManager.Instance.TournamentCallback = ModalFunctions.SeeResultExistTournament;
			}
			else if (Array.IndexOf(t.memberIds, TacoManager.User.userId) > -1) {
				btnText.text = "Play";
				TournamentManager.Instance.TournamentCallback = ModalFunctions.ConfirmPlay;
			}
			else {
				btnText.text = "Join";
				TournamentManager.Instance.TournamentCallback = ModalFunctions.JoinExistTournament;
			}
		}

		public static void OpenModalAlreadyJoinedPanel(string header, string notice) {
			GameObject panel = GetPanel("AlreadyJoined");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Header/Text").GetComponent<Text>().text = header;
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
			TournamentManager.Instance.TournamentCallback = string.Empty;
		}

		public static void OpenModalFundErrorJoinPanel(string header, Tournament t) {
			string notice = TacoConfig.TacoJoinPublicErrorNoFund;
			notice = notice.Replace("&fee", "<b>" + TacoManager.FormatMoney(t.entryFee, t.typeCurrency) + "</b>");
			GameObject panel = GetPanel("FundErrorJoin");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Header/Icon").GetComponent<Image>().sprite = TacoConfig.currencyBrokenSprites[t.typeCurrency];
			panel.transform.Find("Main/Header/Text").GetComponent<Text>().text = header;
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
			TournamentManager.Instance.TournamentCallback = ModalFunctions.FundsScreen;
		}

		public static void OpenModalTacoErrorJoinPanel(string header, Tournament t) {
			string notice = TacoConfig.TacoJoinPublicErrorNoFund;
			notice = notice.Replace("funds", "tokens");
			notice = notice.Replace("&fee", "<b>" + TacoManager.FormatMoney(t.entryFee, 1) + "</b>");
			GameObject panel = GetPanel("TacoErrorJoin");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Header/Icon").GetComponent<Image>().sprite = TacoConfig.currencyBrokenSprites[1];
			panel.transform.Find("Main/Header/Text").GetComponent<Text>().text = header;
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
			TournamentManager.Instance.TournamentCallback = ModalFunctions.TacoScreen;
		}

		public static void OpenModalAccountCreationErrorPanel(string header, string notice) {
			RegisterPanel.Instance.ShowErrorText(notice);
			/*GameObject panel = GetPanel ("AccountCreationError");
			panel.SetActive (true);
			panel.transform.Find ("Main/Header/Text").GetComponent<Text> ().text = header;
			panel.transform.Find ("Main/Notice").GetComponent<Text> ().text = notice;*/
		}

		public static void OpenModalTournamentCreationErrorPanel(string notice) {
			GameObject panel = GetPanel("TournamentCreationError");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
		}

		public static void OpenModalPlayTournamentErrorPanel(string notice) {
			GameObject panel = GetPanel("PlayTournamentError");
			panel.SetActive(true);
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
		}

		public static void OpenModalTournamentCreationForbiddenPanel(string notice) {
			GameObject panel = GetPanel("TournamentCreationForbidden");
			panel.SetActive(true);
			//panel.transform.Find ("Main/Notice").GetComponent<Text> ().text = notice;
			TournamentManager.Instance.TournamentCallback = ModalFunctions.ReadMoreForbidden;
		}

		public static void OpenModalTournamentJoinErrorPanel(string header, string notice, string callback = null) {
			GameObject panel = GetPanel("TournamentJoinError");
			panel.SetActive(true);
			isOpenPopup = true;
			panel.transform.Find("Main/Header/Text").GetComponent<Text>().text = header;
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
			TournamentManager.Instance.TournamentCallback = callback;
		}

		public static void OpenModalRegisterPanel(string username) {
			GameObject panel = GetPanel("SuccessRegister");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Header/usernameHeader").GetComponent<Text>().text = "Hello " + username + ",";
		}

		public static void OpenModalHighScorePanel(string header, string notice) {
			GameObject panel = GetPanel("HighScoreNotice");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Header/Text").GetComponent<Text>().text = header;
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
		}

		public static void OpenModalWellPlayPanel(string header, string notice) {
			GameObject panel = GetPanel("WellPlayNotice");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Header/Text").GetComponent<Text>().text = header;
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
		}

		public static void OpenModalEndPlayingPanel(string header, string notice, Action callback) {
			GameObject panel = GetPanel("EndPlayingNotice");
			isOpenPopup = true;
			panel.SetActive(true);
			panel.transform.Find("Main/Header/Text").GetComponent<Text>().text = header;
			panel.transform.Find("Main/Notice").GetComponent<Text>().text = notice;
			if (callback != null) {
				Button btn = panel.transform.Find("Main/Buttons/Ok").GetComponent<Button>();
				btn.onClick.RemoveAllListeners();
				btn.onClick.AddListener(() => { callback(); });
			}
			TournamentManager.Instance.TournamentCallback = ModalFunctions.TacoEndTournament;
		}

    public static void ShowServerPasswordPopUp() {
      openAdminCount++;
			if (openAdminCount < Constants.numberOfPress) return;
      openAdminCount = 0;
      GameObject panel = GetPanel("AdminPassword");
      panel.SetActive(true);
      isOpenPopup = true;
      Button btn = panel.transform.Find("Main/Buttons/Submit").GetComponent<Button>();
      btn.onClick.RemoveAllListeners();
      btn.onClick.AddListener(() => {
        TacoInputField input = panel.transform.Find("Main/Account").GetComponent<TacoInputField>();
        string data = input.text;
        if (data.Equals(Constants.adminCode)) {
          input.text = string.Empty;
          ShowServerDropdownPopUp();
          isOpenPopup = false;
          panel.SetActive(false);
        }
      });

    }

    public static void ShowServerDropdownPopUp() {
      GameObject panel = GetPanel("AdminServer");
      panel.SetActive(true);
      isOpenPopup = true;
      Button btn = panel.transform.Find("Main/Buttons/Submit").GetComponent<Button>();
      btn.onClick.RemoveAllListeners();
      btn.onClick.AddListener(() => {
        int serverOption = panel.transform.Find("Main/ServerTypeDropdown").GetComponent<Dropdown>().value;
        Constants.BaseUrl = Constants.serverUrls[serverOption];
        isOpenPopup = false;
        panel.SetActive(false);
      });
    }

		public static void CloseAllModals() {
			CloseMessage();
			CloseFoldout();
		}

		public static bool CheckModalsOpen() {
			bool open = false;

			if (MessagePanel.activeInHierarchy) {
				open = true;
			}

			if (ModalPanel.activeInHierarchy) {
				open = true;
			}

			if (TacoFoldoutPanel.activeInHierarchy) {
				open = true;
			}

			return open;
		}

		private static void SetAllPanels(bool state) {
			LoginPanelObject.SetActive(state);
			RegisterPanelObject.SetActive(state);
			ModalPanel.SetActive(state);
			MessagePanel.SetActive(state);
			TacoFoldoutPanel.SetActive(state);
			MyTournamentsPanel.SetActive(state);
			CreatePublicPanel.SetActive(state);
			JoinPublicPanel.SetActive(state);
			ManageTournamentPanel.SetActive(state);
			FilterListPanel.SetActive(state);
			SortListPanel.SetActive(state);
			MyProfilePanel.SetActive(state);
			MyTransactionPanel.SetActive(state);
			BalancesPanel.SetActive(state);
			FundsRunningLowPanel.SetActive(state);
			DepositDisablePanel.SetActive(state);
		}

		public static int GetPreference(string key) {
			int preference = PlayerPrefs.GetInt(key);
			return preference;
		}

		public static void SetPreference(string key, int value) {
			PlayerPrefs.SetInt(key, value);
		}

		public static string GetPreferenceString(string key) {
			string preference = PlayerPrefs.GetString(key);

			return preference;
		}

		public static void SetPreferenceString(string key, string value) {
			PlayerPrefs.SetString(key, value);
		}

		public static void SetTarget(Tournament target) {
			Target = target;
		}

		public static string EncryptStringToBytes(string toEncrypt) {
			RijndaelManaged aes = new RijndaelManaged();
			aes.BlockSize = 128;
			aes.KeySize = 256;
			aes.IV = Encoding.UTF8.GetBytes(Constants.AesIV256);
			aes.Key = Encoding.UTF8.GetBytes(Constants.AesKey256);
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;

			byte[] src = Encoding.UTF8.GetBytes(toEncrypt);

			using (ICryptoTransform encrypt = aes.CreateEncryptor()) {
				byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

				return Convert.ToBase64String(dest);
			}
		}
	}
}