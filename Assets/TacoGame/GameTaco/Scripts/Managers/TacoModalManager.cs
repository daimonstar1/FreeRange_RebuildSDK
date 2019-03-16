using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameTaco {

	public class TacoModalManager : MonoBehaviour {
		public static TacoModalManager Instance;

		//create tournament modal
		private Dropdown tournamentBoardTypeDropDown;//public or private
		private Button createNewTournamentButton;
		private Button enterCreateTournamentButton;
		private Button enterCreateTournamentPopup;
		private Text createTournamentEntryFeeLabel;
		private Image entryFeeStatusIcon;
		private GameObject tokenWarningIcon;
		private GameObject cashWarningIcon;
		private GameObject createTournamentDropDownList;
		private GameObject createTournamentInviteFriend;
		private GameObject panelDetail;
		private GameObject panelBlank;
		private GameObject TermOfServiceGameObject;
		private GameObject PolicyGameObject;
		private List<TournamentInvite> inviteEmailTexts = new List<TournamentInvite>();
		private List<bool> cachedEmailValidate = new List<bool>();
		private List<string> cachedEmailErrorMsg = new List<string>();
		private List<string> cachedPrevValidEmail = new List<string>();

		private Coroutine coundownRoutine;
		private Tournament creatingTournament;

		private Text timerText;
		private double countdownTime = -1;

		public Dropdown  currencyTypeDropdown;
		public Dropdown  tournamentTypeDropdown;

		private void Awake() {
			Instance = this;
			createNewTournamentButton = GameObject.Find("CreateNewTournamentButton").GetComponent<Button>();
			enterCreateTournamentButton = GameObject.Find("EnterCreateTournamentButton").GetComponent<Button>();
			enterCreateTournamentPopup = GameObject.Find("EnterCreateTournamentPopUp").GetComponent<Button>();
			createTournamentEntryFeeLabel = GameObject.Find("EntryFeeDropdown").transform.GetChild(0).gameObject.GetComponent<Text>();
			entryFeeStatusIcon = GameObject.Find("EntryFeeStatusIcon").GetComponent<Image>();
			tokenWarningIcon = GameObject.Find("TokenWarningIcon");
			cashWarningIcon = GameObject.Find("CashWarningIcon");

			InitSetupTournament();
			CreateTournamentTypeDropDown();
			SetupCreateTournamentPosition();
			StartCoroutine(SetGameIcon());
			StartCoroutine(HandleTOSAndPP());
		}

		private IEnumerator HandleTOSAndPP() {
			//fix TermOfService auto adjusGameObject.Find("TermOfService PosX
			TermOfServiceGameObject = GameObject.Find("TermOfService");
			PolicyGameObject = GameObject.Find("PrivacyPolicy");
			RectTransform termOfServiceContentRect = GameObject.Find("TermOfServiceContent").GetComponent<RectTransform>();
			RectTransform policyContentRect = GameObject.Find("PrivacyPolicyContent").GetComponent<RectTransform>();
			yield return new WaitForSeconds(3);//wait for 3s to Unity calculate the content size

			RectTransform rect = termOfServiceContentRect.GetChild(0).GetComponent<RectTransform>();
			rect.localPosition = new Vector3(rect.localPosition.x, -100);
			float currentHeight = rect.sizeDelta.y + 200;
			for (int i = 1; i < termOfServiceContentRect.childCount; i++) {
				RectTransform lastRect = termOfServiceContentRect.GetChild(i - 1).GetComponent<RectTransform>();
				rect = termOfServiceContentRect.GetChild(i).GetComponent<RectTransform>();
				rect.localPosition = new Vector3(rect.localPosition.x, -currentHeight);
				currentHeight += (rect.sizeDelta.y + 100);
			}
			termOfServiceContentRect.sizeDelta = new Vector2(termOfServiceContentRect.sizeDelta.x, currentHeight);
			TermOfServiceGameObject.GetComponent<RectTransform>().localPosition = new Vector3();
			TermOfServiceGameObject.SetActive(false);

			rect = policyContentRect.GetChild(0).GetComponent<RectTransform>();
			rect.localPosition = new Vector3(rect.localPosition.x, -100);
			currentHeight = rect.sizeDelta.y + 200;
			policyContentRect.sizeDelta = new Vector2(policyContentRect.sizeDelta.x, currentHeight);
			PolicyGameObject.GetComponent<RectTransform>().localPosition = new Vector3();
			PolicyGameObject.SetActive(false);
		}

		private void Start() {

			TacoManager.SetPreferenceString(UserPreferences.userTourType, "2");

			createNewTournamentButton.onClick.AddListener(TournamentManager.Instance.StartCreate);

			enterCreateTournamentButton.onClick.AddListener(enterCreateTournament);

			enterCreateTournamentPopup.onClick.AddListener(delegate {
				ClosePanel();
				if (TacoManager.User != null) {
					enterCreateTournament();
				}
				else {
					Debug.Log(TacoManager.User);
					TacoManager.ShowPanel(PanelNames.LoginPanel);
				}
			});
		}

		private void Update() {
			if (countdownTime >= 0 && timerText.gameObject != null && timerText.gameObject.activeSelf) {
				Countdown();
			}
		}

		private IEnumerator SetGameIcon() {
			GameObject[] gameIcons = GameObject.FindGameObjectsWithTag("GameIcon");
			Text createTournamentGameName = GameObject.Find("CreateTournamentGameName").GetComponent<Text>();
			yield return new WaitForSeconds(0.1f);
			for (int i = 0; i < gameIcons.Length; i++) {
				gameIcons[i].GetComponent<Image>().sprite = TacoSetup.Instance.GameIcon;
			}
			createTournamentGameName.text = TacoSetup.Instance.gameName;
		}

		private void Countdown() {
			countdownTime -= Time.deltaTime;
			timerText.text = TacoConfig.timerCountdown(countdownTime);
		}

		public void SetupCountdown(Text displayText, Single timeValue) {
			countdownTime = (double)Mathf.Max(timeValue, 0);
			timerText = displayText;
		}

		#region Create Tournament Page

		private void enterCreateTournament() {
			//TODO: Check Cash < 1 and Taco Token < 20
			double token = double.Parse(TacoManager.User.gToken);
			double cash = (TacoManager.User.TotalCash);
			Debug.Log("TacoManager.GetPreferenceString (UserPreferences.userCurrencyType): " + TacoManager.GetPreferenceString (UserPreferences.userCurrencyType));
			if ((TacoManager.GetPreferenceString (UserPreferences.userCurrencyType) == "Cash") && token < 20)
			{
				//Token
				TournamentManager.Instance.SetRunningLowFundText("Would you Like to Add Taco Tokens To Play in Taco Tournaments?", "Token");
				TournamentManager.Instance.OpenModalFundsRunningLowPanel();
			}
			else if((TacoManager.GetPreferenceString (UserPreferences.userCurrencyType) == "Taco" || TacoManager.GetPreferenceString (UserPreferences.userCurrencyType) == "") && cash < 1)
			{
				TournamentManager.Instance.OpenModalFundsRunningLowPanel();
				TournamentManager.Instance.SetRunningLowFundText("Would you Like to Add Funds To Play in Cash Tournaments?", "Cash");
			}
			else
			{
				VerticalLayoutGroup dropdownListRect = createTournamentDropDownList.GetComponent<VerticalLayoutGroup>();
				if (dropdownListRect.childControlHeight) StartCoroutine(changeControlsize(dropdownListRect));
				UpdateEntryFeeText(createTournamentEntryFeeLabel);

				if ((TacoManager.GetPreferenceString (UserPreferences.userCurrencyType) != null)) {
					if (TacoManager.GetPreferenceString (UserPreferences.userCurrencyType) == "Taco")
					{
						//GameObject.Find ("TacoGame/TacoTournamentsCanvas/CreateTournamentPanel/TacoCreatePanel/PanelScroll/Scroll View/Viewport/Content/PanelWrapper/CreateTournamentDropDownList/CurrencyType/CurrencyTypeDropdown").GetComponent<Dropdown> ().value = 0;
						currencyTypeDropdown.value = 0;
					}
					else if (TacoManager.GetPreferenceString (UserPreferences.userCurrencyType) == "Cash")
					{
						//GameObject.Find ("TacoGame/TacoTournamentsCanvas/CreateTournamentPanel/TacoCreatePanel/PanelScroll/Scroll View/Viewport/Content/PanelWrapper/CreateTournamentDropDownList/CurrencyType/CurrencyTypeDropdown").GetComponent<Dropdown>().value = 1;
						currencyTypeDropdown.value = 1;
					}					
				}

				if ((TacoManager.GetPreferenceString (UserPreferences.userTourType) != null)) {
					if ((TacoManager.GetPreferenceString (UserPreferences.userTourType) != "2") && TacoManager.GetPreferenceString (UserPreferences.userTourType) != "") {
						
						//Not Completed Tour
						//GameObject.Find ("TacoGame/TacoTournamentsCanvas/CreateTournamentPanel/TacoCreatePanel/PanelScroll/Scroll View/Viewport/Content/PanelWrapper/CreateTournamentDropDownList/TournamentType/TournamentTypeDropdown").GetComponent<Dropdown> ().value = Int32.Parse(TacoManager.GetPreferenceString (UserPreferences.userTourType));
						tournamentTypeDropdown.value = Int32.Parse(TacoManager.GetPreferenceString (UserPreferences.userTourType));
					}
				}

				TacoManager.ShowPanel(PanelNames.CreatePublicPanel);
			}

			/*
			  */
		}

		private IEnumerator changeControlsize(VerticalLayoutGroup dropdownListRect) {
			yield return new WaitForSeconds(0.1f);
			dropdownListRect.childControlHeight = false;
		}

		private void InitSetupTournament() {
			creatingTournament = new Tournament();
			creatingTournament.size = 2;
			creatingTournament.entryFee = 1;
			creatingTournament.prize_structure = 1;
			creatingTournament.invitedEmails = new String[] { };
			creatingTournament.typeCurrency = 0;
			creatingTournament.accessType = "public";
			creatingTournament.TimeRemaining = 1;
		}

		private void ShowInviteFriendPanelForCreateTournament() {
			bool status = !creatingTournament.IsPublic;

			if (inviteEmailTexts.Count > 0) {
				List<TournamentInvite> temp = inviteEmailTexts.Where(x => !string.IsNullOrEmpty(x.EmailInput.text)).ToList();
				creatingTournament.invitedEmails = temp.Select(x => x.EmailInput.text).ToArray();
				cachedEmailValidate = temp.Select(x => x.IsValid()).ToList();
				cachedEmailErrorMsg = temp.Select(x => x.errorMessage.text).ToList();
				cachedPrevValidEmail = temp.Select(x => x.previousValidEmailValue).ToList();
			}
			for (int i = 1; i < createTournamentInviteFriend.transform.childCount; i++) {
				Destroy(createTournamentInviteFriend.transform.GetChild(i).gameObject);
			}

			createTournamentInviteFriend.SetActive(status);

			// switch to top stretch to handle scroll when expanding the content
			// (default value is stretch stretch so unity will auto-scale for different resolutions
			RectTransform panelWrapperRect = createTournamentDropDownList.transform.parent.GetComponent<RectTransform>();
			RectTransform scrollContentRect = createTournamentDropDownList.transform.parent.parent.GetComponent<RectTransform>();

			float wrapperHeight = panelWrapperRect.rect.height;
			panelWrapperRect.anchorMin = new Vector2(0, 1);
			panelWrapperRect.sizeDelta = new Vector2(panelWrapperRect.sizeDelta.x, wrapperHeight);

			scrollContentRect.anchorMin = new Vector2(0, 1);

			if (status) {
				RectTransform contentPanel = createTournamentInviteFriend.GetComponent<RectTransform>();
				float spacing = 35;
				float width = (contentPanel.rect.width - spacing);
				float pWidth = contentPanel.parent.GetComponent<RectTransform>().rect.width - spacing;
				if (contentPanel.rect.width < pWidth - spacing) {// check contentPanel has correct width
					width = pWidth;
				}
				float height = 150;
				float requirementHeight = 200;
				int field = creatingTournament.size - 1;
				inviteEmailTexts = new List<TournamentInvite>();

				for (int i = 0; i < field; i++) {
					GameObject ob = Instantiate(TacoConfig.InviteInputPrefab, Vector3.zero, Quaternion.identity, createTournamentInviteFriend.transform) as GameObject;
					RectTransform r = ob.GetComponent<RectTransform>();
					if (contentPanel.rect.width < pWidth - spacing) {
						r.localPosition = new Vector3(spacing, height * (-0.5f - i) - requirementHeight);
					}
					else {
						r.localPosition = new Vector3(spacing, height * (-0.5f - i) - requirementHeight);
					}

					r.sizeDelta = new Vector2(width - spacing * 3, height - spacing);
					TournamentInvite invite = ob.GetComponent<TournamentInvite>();

					if (i > creatingTournament.invitedEmails.Length) {
						invite.SetInteractable(false);
					}

					if (i > 0) {
						inviteEmailTexts[i - 1].next = invite;
						invite.prev = inviteEmailTexts[i - 1];
					}

					if (i < creatingTournament.invitedEmails.Length && !string.IsNullOrEmpty(creatingTournament.invitedEmails[i])) {
						invite.FillWithValidateData(creatingTournament.invitedEmails[i], cachedEmailValidate[i], cachedEmailErrorMsg[i], cachedPrevValidEmail[i]);
					}

					if (i == 0 && creatingTournament.invitedEmails.Length == 0) {
						invite.SetFocus();
					}
					else if (i > 0 && string.IsNullOrEmpty(invite.EmailInput.text) && !string.IsNullOrEmpty(inviteEmailTexts[i - 1].EmailInput.text)) {
						invite.SetFocus();
					}

					inviteEmailTexts.Add(invite);
				}
				float extendedHeight = field * height + spacing + requirementHeight;
				contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, extendedHeight);
				scrollContentRect.sizeDelta = new Vector2(scrollContentRect.sizeDelta.x, wrapperHeight + extendedHeight + spacing * 2);
			}
			else {
				scrollContentRect.sizeDelta = new Vector2(scrollContentRect.sizeDelta.x, wrapperHeight);
			}
		}

		private void SetupCreateTournamentPosition() {
			List<GameObject> dropdownList = new List<GameObject>();

			createTournamentDropDownList = GameObject.Find("CreateTournamentDropDownList");
			createTournamentInviteFriend = GameObject.Find("CreateTournamentInviteFriend");
			panelDetail = GameObject.Find("PanelDetail");
			panelBlank = GameObject.Find("PanelBlank");
			createTournamentInviteFriend.SetActive(false);

			foreach (Transform child in createTournamentDropDownList.transform) {
				if (child.gameObject != createTournamentInviteFriend && child.gameObject != panelDetail && child.gameObject != panelBlank) {
					
					dropdownList.Add (child.GetChild (2).gameObject);
				}
			}
			SeptupCreateTournamentDropDownAction(dropdownList);

			// Set dropdown height
			for (int i = 0; i < dropdownList.Count; i++) {
				GameObject dropdownObject = dropdownList[i];
				if (dropdownObject.transform.childCount >= 2) {
					//Except Panel Detail and Panel Blank
					Vector2 dropdownPosition = dropdownObject.GetComponent<RectTransform>().sizeDelta;
					RectTransform label = dropdownObject.transform.GetChild(0).GetComponent<RectTransform>();
					label.sizeDelta = dropdownPosition;
					label.localPosition = new Vector2(30 - dropdownPosition.x / 2, 0);
					RectTransform templateRect = dropdownObject.transform.GetChild(2).GetComponent<RectTransform>();
					templateRect.sizeDelta = new Vector2(dropdownPosition.x, dropdownPosition.y * 5);
					templateRect.localPosition = new Vector3(-dropdownPosition.x / 2, -dropdownPosition.y / 2);

					RectTransform viewportRect = templateRect.transform.GetChild(0).GetComponent<RectTransform>();
					viewportRect.sizeDelta = new Vector2(dropdownPosition.x, dropdownPosition.y * 5);
					viewportRect.localPosition = new Vector3(-dropdownPosition.x / 2, -dropdownPosition.y);

					RectTransform contentRect = viewportRect.transform.GetChild(0).GetComponent<RectTransform>();
					contentRect.sizeDelta = dropdownPosition;
					contentRect.localPosition = new Vector3(dropdownPosition.x / 2, 0);
				}
				
			}
		}

		private void SeptupCreateTournamentDropDownAction(List<GameObject> dropdownList) {
			string[] currencyStrValues = { "USD", "TT" };
			string[][] feeValues = {
			new string[]{ "$1.00", "$2.00", "$5.00", "$10.00", "$100.00" },
			new string[]{ "20", "40", "100", "200", "2,000" },
			new string[]{ "1", "2", "5", "10", "100" },
			new string[]{ "20", "40", "100", "200", "2000" }
		};
			string[] winnerValues = { "1 Winner", "2 Winners", "3 Winners", "5 Winners", "10 Winners", "20 Winners" };
			string[] winnerOptions = { "1 - Winner Takes All", "2 - Winners", "3 - Winners", "5 - Winners", "10 - Winners", "20 - Winners" };
			string[][] allSizeValues = {
			new string[]{ "2 Players", "3 Players", "5 Players", "10 Players", "100 Players" },
			new string[]{ "2 Players", "3 Players", "5 Players", "10 Players", "20 Players" }
		};

			Text feeText = GameObject.Find("CreateTournamentFeeText").GetComponent<Text>();
			Text totalPrize = GameObject.Find("CreateTournamentTotalPrize").GetComponent<Text>();
			Text dayText = GameObject.Find("CreateTournamentDayText").GetComponent<Text>();
			Text winnerText = GameObject.Find("CreateTournamentWinnerText").GetComponent<Text>();
			Text sizeText = GameObject.Find("CreateTournamentSizeText").GetComponent<Text>();

			Text CreateTournamentPanelDetailText = GameObject.Find("CreateTournamentPanelDetailText").GetComponent<Text>();
			Text CreateTournamentPanelTypeText = GameObject.Find("CreateTournamentPanelTypeText").GetComponent<Text>();
			Text CreateTournamentPanelFeeText = GameObject.Find("CreateTournamentPanelFeeText").GetComponent<Text>();
			Text CreateTournamentPanelNoticeText = GameObject.Find("CreateTournamentPanelNoticeText").GetComponent<Text>();

			Dropdown winnerNbDropdown = GameObject.Find("NumberWinnersDropdown").GetComponent<Dropdown>();
			Dropdown entryValueDropdown = GameObject.Find("EntryFeeDropdown").GetComponent<Dropdown>();
			Dropdown sizeDropdown = GameObject.Find("TournamentSizeDropdown").GetComponent<Dropdown>();

			UpdateEntryFeeDropdownOptions(entryValueDropdown, feeValues[creatingTournament.typeCurrency]);
			UpdateCreateTournamentTotalPrize(totalPrize, currencyStrValues);
			StartCoroutine(UpdateTournamentPanelTypeText(CreateTournamentPanelTypeText));

			for (int j = 0; j < dropdownList.Count; j++) {
				GameObject dropdownObject = dropdownList[j];
				Dropdown dropdown = dropdownObject.GetComponent<Dropdown>();
				switch (dropdownObject.name) {

					case "CurrencyTypeDropdown":
						GameObject[] objectList = GameObject.FindGameObjectsWithTag("CreateTournamentMoneySign");
						UpdateCreateTournamentFeeText(CreateTournamentPanelFeeText);
						UpdateCreateTournamentPanelNoticeText(CreateTournamentPanelNoticeText);

						dropdown.onValueChanged.AddListener(delegate {
							creatingTournament.typeCurrency = dropdown.value;
							creatingTournament.entryFee = int.Parse(feeValues[creatingTournament.typeCurrency + 2][entryValueDropdown.value]);
							for (int i = 0; i < objectList.Length; i++) {
								objectList[i].GetComponent<Image>().sprite = TacoConfig.currencySprites[dropdown.value];
							}
							UpdateCreateTournamentTotalPrize(totalPrize, currencyStrValues);
							UpdateCreateTournamentFeeText(CreateTournamentPanelFeeText);
							UpdateCreateTournamentPanelNoticeText(CreateTournamentPanelNoticeText);
							UpdateEntryFeeDropdownOptions(entryValueDropdown, feeValues[creatingTournament.typeCurrency]);
							UpdateEntryFeeText(createTournamentEntryFeeLabel);
						});
						break;

					case "TournamentTypeDropdown":
						string[] tournamentTypes = { "Public Tournament", "Private Tournament" };
						Text typeText = GameObject.Find("CreateTournamentTypeText").GetComponent<Text>();
						dropdown.onValueChanged.AddListener(delegate {
							typeText.text = tournamentTypes[dropdown.value];
							creatingTournament.accessType = dropdown.value == 0 ? "public" : "challenge"; //need update
							string[] sizeValues = allSizeValues[dropdown.value];
							UpdateSizeDropdownOptions(sizeDropdown, sizeValues);

							int hightestValue = int.Parse(sizeValues.Last().Replace(" Players", string.Empty).Trim());
							if (creatingTournament.size > hightestValue) {
								sizeDropdown.value = sizeValues.Length - 1;
								creatingTournament.size = hightestValue;
								sizeText.text = sizeValues[sizeDropdown.value];
							}
							else if (!sizeValues.Contains(creatingTournament.size.ToString() + " Players")) {
								creatingTournament.size = int.Parse(sizeText.text.Replace(" Players", string.Empty).Trim());
							}
							sizeDropdown.onValueChanged.Invoke(sizeDropdown.value);
							ShowInviteFriendPanelForCreateTournament();
							UpdateCreateTournamentTotalPrize(totalPrize, currencyStrValues);
							UpdateCreateTournamentPanelDetail(CreateTournamentPanelDetailText, dayText, sizeText, winnerText);
							CreateTournamentPanelTypeText.text = TacoSetup.Instance.gameName + " " + tournamentTypes[dropdown.value];
						});
						break;

					case "TournamentDurationDropdown":
						string[] dayValues = { "1 Day", "2 Days", "3 Days", "4 Days", "5 Days", "14 Days" };
						dropdown.onValueChanged.AddListener(delegate {
							dayText.text = "Lasting " + dayValues[dropdown.value];
							UpdateCreateTournamentPanelDetail(CreateTournamentPanelDetailText, dayText, sizeText, winnerText);
							creatingTournament.TimeRemaining = int.Parse(dayValues[dropdown.value].Substring(0, dayValues[dropdown.value].Length - "Days".Length).Trim());
						});
						break;

					case "NumberWinnersDropdown":
						dropdown.onValueChanged.AddListener(delegate {
							winnerText.text = winnerValues[dropdown.value];
							creatingTournament.prize_structure = int.Parse(winnerText.text.Substring(0, winnerText.text.Length - "winners".Length).Trim());
							UpdateCreateTournamentTotalPrize(totalPrize, currencyStrValues);
							UpdateCreateTournamentPanelDetail(CreateTournamentPanelDetailText, dayText, sizeText, winnerText);
						});
						break;

					case "TournamentSizeDropdown":
						int winnerOption = 0;

						UpdateWinnersDropdownOptions(winnerNbDropdown, winnerOptions, winnerValues, ref winnerOption);

						dropdown.onValueChanged.AddListener(delegate {
							string[] sizeValues = allSizeValues[creatingTournament.accessType == "public" ? 0 : 1];
							sizeText.text = sizeValues[dropdown.value];
							creatingTournament.size = int.Parse(sizeValues[dropdown.value].Replace(" Players", string.Empty));
							UpdateWinnersDropdownOptions(winnerNbDropdown, winnerOptions, winnerValues, ref winnerOption);
							ShowInviteFriendPanelForCreateTournament();

							if (creatingTournament.size <= creatingTournament.prize_structure) {
								winnerNbDropdown.value = winnerOption;
							}

							UpdateCreateTournamentTotalPrize(totalPrize, currencyStrValues);
							UpdateCreateTournamentPanelDetail(CreateTournamentPanelDetailText, dayText, sizeText, winnerText);
						});
						break;

					case "EntryFeeDropdown":
						UpdateCreateTournamentFeeText(feeText, "With Entry Fee ");

						dropdown.onValueChanged.AddListener(delegate {
							creatingTournament.entryFee = int.Parse(feeValues[creatingTournament.typeCurrency + 2][dropdown.value]);
							UpdateCreateTournamentFeeText(feeText, "With Entry Fee ");
							UpdateCreateTournamentFeeText(CreateTournamentPanelFeeText);
							UpdateCreateTournamentPanelNoticeText(CreateTournamentPanelNoticeText);
							UpdateCreateTournamentTotalPrize(totalPrize, currencyStrValues);
							UpdateEntryFeeText(createTournamentEntryFeeLabel);
						});
						break;
				}
			}
		}

		private IEnumerator UpdateTournamentPanelTypeText(Text text) {
			// fix tacolanguge is not instantiated when this text need it
			yield return new WaitForSeconds(0.1f);
			text.text = TacoSetup.Instance.gameName + " Public Tournament";
		}

		private void UpdateEntryFeeDropdownOptions(Dropdown dropdown, string[] entryFeeOptions) {
			dropdown.ClearOptions();
			dropdown.AddOptions(entryFeeOptions.ToList());
		}

		private void UpdateSizeDropdownOptions(Dropdown dropdown, string[] sizesOptions) {
			dropdown.ClearOptions();
			dropdown.AddOptions(sizesOptions.ToList());
		}

		private void UpdateWinnersDropdownOptions(Dropdown dropdown, string[] winnerOptions, string[] winnerValues, ref int winnerOption) {
			dropdown.ClearOptions();
			List<string> options = new List<string>();
			int lastValue = 0;
			for (int i = 0; i < winnerValues.Length; i++) {
				int winnerNb = int.Parse(winnerValues[i].Substring(0, winnerValues[i].Length - "winners".Length).Trim());
				if (winnerNb >= creatingTournament.size) {
					winnerOption = lastValue;//cannout use dropdown.value in here
					break;
				}
				else {
					lastValue = i;
				}
				options.Add(winnerOptions[i]);
			}
			dropdown.AddOptions(options);
		}

		private void UpdateEntryFeeText(Text feeLabel) {
			cashWarningIcon.SetActive(false);
			tokenWarningIcon.SetActive(false);

			if ((creatingTournament.typeCurrency == 1 && creatingTournament.entryFee <= double.Parse(TacoManager.User.gToken)) || (creatingTournament.typeCurrency == 0 && creatingTournament.entryFee <= TacoManager.User.TotalCash)) {
				if (creatingTournament.typeCurrency == 0) {
					feeLabel.text = TacoManager.FormatCash(creatingTournament.entryFee);
				}
				else {
					feeLabel.text = TacoManager.FormatGTokens2(creatingTournament.entryFee);
				}
				feeLabel.text += " <color=#9E9E9EFF>(available)</color>";
				entryFeeStatusIcon.sprite = TacoConfig.IconCheck;
				if (!createNewTournamentButton.IsInteractable()) createNewTournamentButton.interactable = true;
			}
			else {
				if (creatingTournament.typeCurrency == 0) {
					cashWarningIcon.SetActive(true);
					feeLabel.text = TacoManager.FormatCash(creatingTournament.entryFee);
				}
				else {
					tokenWarningIcon.SetActive(true);
					feeLabel.text = TacoManager.FormatGTokens2(creatingTournament.entryFee);
				}
				feeLabel.text += " <color=#9E9E9EFF>(unavailable)</color>";
				entryFeeStatusIcon.sprite = TacoConfig.IconWarning;
				if (createNewTournamentButton.IsInteractable()) createNewTournamentButton.interactable = false;
			}
		}

		private void UpdateCreateTournamentPanelNoticeText(Text notice) {
			string moneyType = "Cash";
			if (creatingTournament.typeCurrency == 1) moneyType = "Taco Token";
			notice.text = moneyType + " Entry Fee will be debited from your account";
		}

		private void UpdateCreateTournamentFeeText(Text feeText, string prefix = "") {
			string entryFeeStr = string.Empty;
			if (creatingTournament.typeCurrency == 0)
				entryFeeStr = TacoManager.FormatCash(creatingTournament.entryFee);
			else
				entryFeeStr = TacoManager.FormatGTokens2(creatingTournament.entryFee);

			feeText.text = prefix + entryFeeStr;
		}

		private void UpdateCreateTournamentTotalPrize(Text totalPrize, string[] currencyStrValues) {
			double prize = (creatingTournament.size * creatingTournament.entryFee * (1 - 0.1f * (1 - creatingTournament.typeCurrency)) / creatingTournament.prize_structure);
			string prizeStr = string.Empty;
			if (creatingTournament.typeCurrency == 0)
				prizeStr = TacoManager.FormatCash(prize);
			else
				prizeStr = TacoManager.FormatGTokens2(Mathf.Ceil((float)prize));
			totalPrize.text = prizeStr;
		}

		private void UpdateCreateTournamentPanelDetail(Text detailText, Text dayText, Text playerText, Text winnerText) {
			detailText.text = dayText.text + " | " + playerText.text + " | " + winnerText.text;
		}

		#endregion

		private void CreateTournamentTypeDropDown() {
			tournamentBoardTypeDropDown = GameObject.Find("TournamentTypeDropDown").GetComponent<Dropdown>();
			tournamentBoardTypeDropDown.onValueChanged.AddListener(delegate {
				TournamentTypeSelect();
			});
		}

		public void TournamentTypeSelect() {
			switch (tournamentBoardTypeDropDown.value) {
				case 0:
					TournamentManager.Instance.PanelToggle(PanelNames.MyPublicPanel);
					break;
				case 1:
					TournamentManager.Instance.PanelToggle(PanelNames.MyPrivatePanel);
					break;
				case 2:
					TournamentManager.Instance.PanelToggle(PanelNames.MyCompletedPanel);
					break;
			}
		}

		public void TournamentConfirmClick() {

			ClosePanel();

			if (!string.IsNullOrEmpty(TournamentManager.Instance.TournamentCallback)) {
				switch (TournamentManager.Instance.TournamentCallback) {

					case ModalFunctions.LogoutUser:
						TacoManager.LogoutUser();
						break;
					case ModalFunctions.JoinTournament:
						TournamentManager.Instance.Join();
						break;
					case ModalFunctions.JoinPlayAgainTournament:
						//Call API to Get Tour
						var t = TacoManager.Target;
						Debug.Log(t);

						Action<string> success = (string data) => {
							Debug.Log("reenter data: " + data);
							var r = JsonUtility.FromJson<JoinTournamentResult>(data);
							if (r.success) {
								TacoManager.SetTarget(r.tournament);
								Debug.Log("Join Play Again Tour ID: " + r.tournament.id);
								TournamentManager.Instance.JoinPlayAgain(r.tournament, r);

							}
							else {
								TacoManager.CloseMessage();
								Debug.Log("Error Leo: " + r.error);
								Debug.Log("Message Leo: " + r.message);
								if (r.error == TacoConfig.NotEnoughFundError) {
									TacoManager.OpenModalFundErrorJoinPanel(TacoConfig.TacoJoinPublicNoFundHead, t);
								}
								else if (r.error == TacoConfig.NotEnoughTokenError) {
									TacoManager.OpenModalTacoErrorJoinPanel(TacoConfig.TacoJoinPublicNoTacoHead, t);
								}
								else if (r.error == TacoConfig.DuplidateError) {
									TacoManager.OpenModalAlreadyJoinedPanel(TacoConfig.TacoJoinAlreadyHead, r.message);
								}
								else if (r.error == String.Empty && r.message == "We are sorry, you do not have sufficient funds in your account to join this tournament. Please deposit more funds and try again.") {
									TacoManager.OpenModalFundErrorJoinPanel(TacoConfig.TacoJoinPublicNoFundHead, t);
								}
								else { 
									TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.Error, r.message);
								}
							}
						};

						Action<string, string> fail = (string data, string error) => {
							Debug.Log("Error ReEnter data: " + data);
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
							Debug.Log("t != null");
							StartCoroutine(ApiManager.Instance.ReEnterTournament(t.id, success, fail));
						}

						break;

					case ModalFunctions.WaitingPlay:
						OpenModalCountDownPanel();
						break;

					case ModalFunctions.StartPlay:
						DisableModalCountDownPanel();
						break;

					case ModalFunctions.ConfirmPlay:
						//TacoManager.OpenModalPlayTournamentPanel();
						GameManager.Instance.StartPlay(TacoManager.Target);
						break;

					case ModalFunctions.TournamentSubmit:
						if (!creatingTournament.IsPublic && inviteEmailTexts.Count > 0) {
							creatingTournament.invitedEmails = inviteEmailTexts.Where(x => x.IsValid()).Select(x => x.EmailInput.text).ToArray();
						}
						TournamentManager.Instance.TournamentSubmit(creatingTournament);
						break;

					case ModalFunctions.InviteFriends:
						TournamentManager.Instance.InviteFriends();
						break;

					case ModalFunctions.ExitApp:
						Debug.Log("Exit App");
						Application.OpenURL(Constants.BaseUrl + "games");
						Application.Quit();
						break;

					case ModalFunctions.JoinExistTournament:
						TournamentManager.Instance.Join();
						break;

					case ModalFunctions.SeeResultExistTournament:
						TournamentManager.Instance.ShowTournamentPanel(PanelNames.MyLeaderboardPanel);
						break;

					case ModalFunctions.TacoScreen:
						BalanceManager.Instance.Init(1);
						break;

					case ModalFunctions.FundsScreen:
						Application.OpenURL(Constants.BaseUrl + "deposit/" + TacoManager.User.webToken);// + "addfunds"
						break;

					case ModalFunctions.ReadMoreForbidden:
						Application.OpenURL(Constants.BaseUrl + "games");
						break;

					case ModalFunctions.TacoEndTournament:
						TacoSetup.Instance.TacoEndTournament();
						break;

					case ModalFunctions.ReEnterTournament:
						TournamentManager.Instance.ReEnter();
						break;

					case ModalFunctions.ResetPassword:
						LoginPanel.Instance.Reset();
						break;
				}
			}
		}
		private void TournamentSubmitForbidden(string results) {
			TacoManager.CloseMessage();
			TacoManager.OpenModalTournamentCreationForbiddenPanel(results);
		}

		public void OpenModalCountDownPanel() {
			GameObject panel = TacoManager.GetPanel("WaitingPlay");
			panel.SetActive(false);
			GameObject cancel = panel.transform.Find("Main/Buttons/Cancel").gameObject;
			cancel.SetActive(false);
			//coundownRoutine = StartCoroutine(StartCountDown(panel, cancel));
			GameManager.Instance.StartPlay(TacoManager.Target);
			TournamentManager.Instance.TournamentCallback = ModalFunctions.StartPlay;
		}

		public void DisableModalCountDownPanel() {
			if(coundownRoutine != null)
				StopCoroutine(coundownRoutine);
		}

		private IEnumerator StartCountDown(GameObject panel, GameObject cancel) {
			Text countdownText = panel.transform.Find("Main/CountDownImage/Number").GetComponent<Text>();
			int time = TacoConfig.waitingPlayCountdownTime;
			while (time >= -1) {
				countdownText.text = time.ToString();
				yield return new WaitForSeconds(1.0f);
				time--;
				if (time < 0) {
					panel.SetActive(false);
					GameManager.Instance.StartPlay(TacoManager.Target);
					break;
				}
				else if (time == 0) {
					cancel.SetActive(false);
				}
			}
		}

		public void ClosePanelAndRemoveCallback() {
			ClosePanel();
			TournamentManager.Instance.TournamentCallback = null;
		}

		public void ClosePanel() {
			TacoManager.isOpenPopup = false;
			EventSystem.current.currentSelectedGameObject.transform.parent.parent.parent.gameObject.SetActive(false);
		}

		public void SuccessLoginBriefOk() {
			ClosePanel();
			TacoSetup.Instance.LogEvent("how_it_work");
			TurnPanelOn("HowToPlay0");
		}
		public void ToHowToPlay0(bool close = true) {
			if (close) ClosePanel();
			TurnPanelOn("HowToPlay0");
		}

		public void ToHowToPlay1() {
			ClosePanel();
			TurnPanelOn("HowToPlay1");
		}

		public void ToHowToPlay2() {
			ClosePanel();
			TurnPanelOn("HowToPlay2");
		}

		public void ToHowToPlay3() {
			ClosePanel();
			TurnPanelOn("HowToPlay3");
		}

		public void ToHowToPlay4() {
			ClosePanel();
			TurnPanelOn("HowToPlay4");
		}

		public void WhyJoin() {
			TurnPanelOn("WhyJoinPanel");
		}

		public void SignIn() {
			ClosePanel();
			TacoManager.ShowPanel(PanelNames.LoginPanel);
		}

		public void SignUp() {
			ClosePanel();
			TacoManager.ShowPanel(PanelNames.RegisterPanel);
		}

		public void TermOfService() {
			TermOfServiceGameObject.SetActive(true);
			//Application.OpenURL(Constants.BaseUrl +  "signup");
		}

		public void PrivacyPolicy() {
			PolicyGameObject.SetActive(true);
			//Application.OpenURL(Constants.BaseUrl +  "signup");
		}

		private void TurnPanelOn(string panelName) {
			TacoManager.TurnPanelOn(panelName);
		}
	}
}