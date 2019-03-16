using UnityEngine;
using UnityEngine.UI;

namespace GameTaco {

	//TODO put all the strings from the case statements in TacoModels

	public class TacoButton : MonoBehaviour {

		public string Name = "";
		public Text ButtonText = null;
		private Tournament Target;
		public int callBackInt = -1;

		private void Start() {
			// if the user pointed to the text, get the localized string use the name of the button as the key
			if (ButtonText) {
				SetKey(gameObject.name);
			}
		}

		public void SetEnabled(bool enabled) {

			Button thisButton = this.GetComponent<Button>();
			if (thisButton) {
				thisButton.enabled = enabled;
			}
		}

		public void SetKey(string key) {
			string buttonText = TacoConfig.GetValue(key);
			SetText(buttonText);
		}

		public void SetText(string text) {
			ButtonText.text = text;
		}

		public void SetCallBackDataInt(int data) {
			this.callBackInt = data;
		}

		public void Awake() {
			SetEnabled(true);
		}

		public void Button_Click() {

			TacoManager.Target = Target;
			SetEnabled(false);

		}

		public void ButtonSound(string Soundname) {
			if (Soundname != null) {
				//TacoSoundBase.Instance.GetComponent<AudioSource>().PlayOneShot(TacoSoundBase.Instance.click);
			}
		}

		public void OnOpenPress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				TacoManager.OpenTacoFromGame();
			}
		}

		public void OnSkipPress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");

				switch (name) {

					case "LoginSkipButton":

						TacoManager.CloseTaco();
						break;
#if UNITY_IPHONE || UNITY_ANDROID
					case "LoginWithFacebook":
						LoginPanel.Instance.LoginWithFacebook();
						break;
#endif
					case "LoginWithGoogle":
						LoginPanel.Instance.LoginWithGoogle();
						break;
				}
			}
		}

		public void OnJoinPress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				TournamentManager.Instance.Join();
			}
		}

		public void OnLeaderboardPress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				// considering sub-panel of tournaments - clicked from a list view
				TournamentManager.Instance.ShowTournamentPanel(PanelNames.MyLeaderboardPanel);
			}
		}

		public void OnCreateTournamentPress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				TacoManager.ShowPanel(PanelNames.CreatePublicPanel);
			}
		}

		public void OnCreatePress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				TournamentManager.Instance.StartCreate();
			}
		}

		public void OnLoginPress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				LoginPanel.Instance.Login();
			}
		}

		public void OnResetPress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				TacoManager.OpenModalResetPasswordPanel();
			}
		}

		public void OnEnterPress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				//TODO: Logic to enter a tournament
				//if (TacoManager.Instance.Target != null) {
				//    mainscript.Instance.arcadeMode = true;
				//    GamePlay.Instance.GameStatus = GameState.Playing;
				//    TacoManager.Instance.ShowTournaments(false);
				//}
			}
		}

		public void OnRegisterPress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				RegisterPanel.Instance.Register();
			}
		}

		public void OnFeaturedGamePress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				// The Name member variable was set to the URl for these buttons
				if (this.Name != "") {
					Application.OpenURL(Name);
				}
			}
		}


		public void SetTournament(Tournament tournament) {
			Target = tournament;
		}

		public void OnClosePress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				switch (name) {

					case "RegisterCloseButton":
						RegisterPanel.Instance.Init();
						TacoManager.CloseTaco();
						break;

					case "TacoCloseButton":
						TacoManager.CloseTaco();
						break;

					case "TacoModalCloseButton":
						TacoManager.CloseModal();
						break;

					case "TournamentCloseButton":
						TacoManager.CloseTaco();
						break;

					case "AddFundsCloseButton":
						TacoManager.ShowPanel(PanelNames.MyTournamentsPanel);
						break;

					case "AddGTokensCloseButton":
						TacoManager.ShowPanel(PanelNames.MyTournamentsPanel);
						break;

					case "WithdrawFundsCloseButton":
						TacoManager.ShowPanel(PanelNames.MyTournamentsPanel);
						break;

					case "LeaderboardCloseButton": {
							LeaderboardList.Instance.Destroy();
							TournamentManager.Instance.ShowTournamentPanel();
							TacoManager.Target = null;
							break;
						}

					case "LeaderboardReturnButtonPrivate":
					case "LeaderboardReturnButton" : {
							LeaderboardList.Instance.Destroy();
							TacoManager.MyTournamentsPanel.SetActive(true);
							string panelName = TacoManager.Target.IsPublic ? PanelNames.MyPublicPanel : PanelNames.MyPrivatePanel;
							GameObject.Find("MyTournamentsPanel/TournamentType/TournamentTypeDropDown").GetComponent<Dropdown>().value = TacoManager.Target.IsPublic ? 0 : 1;
							TacoManager.ManageTournamentPanel.SetActive(false);
							TournamentManager.Instance.ShowTournamentPanel(panelName);
							TacoManager.Target = null;
							break;
						}

					case "TransactionCloseButton": {
							TransactionList.Instance.transform.parent.gameObject.SetActive(false);
							TacoManager.ShowPanel(PanelNames.ProfilePanel);
							break;
						}

					case "ManageTournamentCloseButton": {
							TacoManager.ManageTournamentPanel.SetActive(false);
							TacoManager.TacoBlockingCanvas.SetActive(true);
							TournamentManager.Instance.ShowTournamentPanel();
							TacoManager.Target = null;
							break;
						}

					case "CreateTournamentCloseButton":
						TacoManager.ReturnToTournamentBoard();
						break;

					case "JoinTournamentCloseButton":
						TacoManager.ShowPanel(PanelNames.MyTournamentsPanel);
						break;

					case "OurGamesCloseButton":
						TacoManager.ShowPanel(PanelNames.MyTournamentsPanel);
						break;

					default:
						break;
				}
			}
		}

    public void OnHidenPress() {
      switch (name) {
        case "Switch":
					if (TacoManager.User == null){
          	TacoManager.ShowServerPasswordPopUp();
					} else {
						BalanceManager.Instance.LoadCurrentBalance();
					}
          break;
      }
    }

		public void OnFoldoutPress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");
				// all buttons close the panel - even the close one ;)
				TacoManager.CloseFoldout();
				// do the other stuff they do
				switch (name) {

					case "SignIn":
						TacoManager.ShowPanel(PanelNames.LoginPanel);
						break;

					case "SignUp":
						TacoManager.ShowPanel(PanelNames.RegisterPanel);
						break;

					case "StartMenu":
					case "GameStart":
            TacoSetup.Instance.ToMainMenu();
						TacoManager.CloseTaco();
						break;

					case "AllOurGames":
						//TacoManager.ShowPanel(PanelNames.FeaturedGamesPanel);
						Application.OpenURL(Constants.BaseUrl + "games");
						break;
					
					case "QuitApp":
						Application.Quit();
						break;

					case "HowGameWorks":
						TacoSetup.Instance.LogEvent("how_it_work");
						TacoModalManager.Instance.ToHowToPlay0(false);
						break;

					case "ViewProfile":
						if (TacoManager.User != null)
							TacoManager.ShowPanel(PanelNames.ProfilePanel);
						break;

					case "ContactUs":
						Application.OpenURL(Constants.BaseUrl + "contact");
						break;

					case "LogoutButton":
						TacoManager.AskToLogoutUser();
						break;

					case "TournamentsButton":
						TacoManager.ShowPanel(PanelNames.MyTournamentsPanel);
						break;

					case "CreateTournamentButton":
						TacoManager.ShowPanel(PanelNames.CreatePublicPanel);
						break;

					case "UserFundFoldout":
						Application.OpenURL(Constants.BaseUrl);
						break;

					case "UserTokenFoldout":
						Application.OpenURL(Constants.BaseUrl + "addtokens");
						break;

					case "UserPointFoldout":
						Application.OpenURL(Constants.BaseUrl + "prizes");
						break;

					case "ProfileButton":
						Application.OpenURL(Constants.BaseUrl + "account");
						break;
				}
			}
		}

		public void OnTogglePress()
		{
			string currencyType = "";
			if (gameObject.name == "TacoTournamentsButton")
				currencyType = "Cash";
			else
				currencyType = "Taco";
				
			TacoManager.SetPreferenceString(UserPreferences.userCurrencyType, currencyType);
		}

		public void OnTourTypeChange()
		{
			int userTourType = gameObject.GetComponent<Dropdown> ().value;
			TacoManager.SetPreferenceString(UserPreferences.userTourType, userTourType.ToString());
		}

		public void OnNavPress(bool isDown) {
			if (!isDown) {
				ButtonSound("Default");

				switch (name) {
					case "Feedback":
						Application.OpenURL("mailto:" + "baysidegamesdev@gmail.com");
						break;

					case "FilterListButton":
						TacoManager.ShowPanel(TacoManager.MyProfilePanel.activeSelf ? PanelNames.FilterActiveListPanel : PanelNames.FilterListPanel);
						break;

					case "SortListButton":
						TacoManager.ShowPanel(TacoManager.MyProfilePanel.activeSelf ? PanelNames.SortActiveListPanel : PanelNames.SortListPanel);
						break;

					case "MyLeaderboardTournamentsButton":
						TournamentManager.Instance.ShowTournamentPanel(PanelNames.MyLeaderboardPanel);
						break;

					case "TacoOpenButton":
						TacoManager.OpenFoldout();
						break;

					case "LoginRegisterButton":
						TacoManager.ShowPanel(PanelNames.RegisterPanel);
						break;

					case "NavLogoutButton":
						TacoManager.AskToLogoutUser();
						break;

					case "SignInButton":
						TacoManager.ShowPanel(PanelNames.LoginPanel);
						break;

					case "ViewAllTransactions":
						TacoManager.ShowPanel(PanelNames.MyTransactionPanel);
						break;

					default:
						break;
				}
			}
		}
	}
}
