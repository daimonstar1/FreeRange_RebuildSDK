using UnityEngine.UI;
using UnityEngine;
using System;
using GameTacoSDK;

namespace GameTaco {
	public class BalanceManager : MonoBehaviour {
        public delegate void BalanceEvent();
        public event BalanceEvent back;
        public static BalanceManager Instance;
		public Text moneyValue;
		public Text title;
		public Text timeLeftText;
		public Text timeLeftPopupText;
		public Text tokenClaim;
		public Text tokenPopupClaim;
		public Image moneyIcon;
		public Button claimBtn;
		public Button claimPopupBtn;
		private GameObject Container;
		private GameObject FundPanel;
		private GameObject TokenPanel;
		private GameObject PointPanel;
		private GameObject DepositPanel;
		private GameObject WithdrawPanel;
		public GameObject prizePrefab;
		private double remainingClaim;
		private bool fromMainMenu;
		private bool alreadyLoadPrize;
		public RectTransform prizeListRect;
		public Transform prizeDetailPanel;
		public Transform redeemPrizePanel;
		public Text redeemFormError;
		public InputField emailInput;
		public InputField dateOfBirthInput;
		public InputField streetInput;
		public InputField buildingInput;
		public InputField countryInput;
		public InputField stateInput;
		public InputField postcodeInput;
		private bool isValidAge;
		public InputField cityInput;
		public Button RedeemSubmit;
		private Prize purchasingPrize;


		//fund - withdraw panel
		public Button withdrawBtn;
		public Button depositeBtn;
		public Text availableFund;
		public Text defaultAddress;
		public Button switchAddressBtn;
		public InputField withdrawAmoutInput;
		public Button cancelWithdrawBtn;
		public Button submitWithdrawBtn;
		public Button cancelDepositeBtn;
		public Button submitDepositeBtn;
		public GameObject newAddress;
		public InputField wdAddress1Input;
		public InputField wdAddress2Input;
		public InputField wdCityInput;
		public InputField wdStateInput;
		public InputField wdZipcodeInput;
		public Toggle wdSaveProfile;

		//exchange token
		public Transform confirmPanel;
		public Button purchaseByCash;
		public Button purchaseByRP;
		public Button confirmPurchaseBtn;
		public Image exchangedCurrencyIcon;
		public Text exchangedValue;
		private string exchangedNumber;
		public Text tokenValue;
		public Text changedTacoPoint;
		public Text changedTacoCash;
		public InputField pointInput;
		public InputField cashInput;

		void Awake() {
			Instance = this;
			Container = transform.Find("Container").gameObject;//need balance manager active to coundown
			FundPanel = Container.transform.Find("Funds").gameObject;
			TokenPanel = Container.transform.Find("Tokens").gameObject;
			PointPanel = Container.transform.Find("Points").gameObject;
			DepositPanel = Container.transform.Find("Deposite").gameObject;
			WithdrawPanel = Container.transform.Find("Withdraw").gameObject;
			DepositPanel.SetActive(false);
			WithdrawPanel.SetActive(false);
			newAddress.SetActive(false);
			TokenPanel.SetActive(false);
			PointPanel.SetActive(false);
			FundPanel.SetActive(false);
			ResetTacoInput();
			SetupButton();
			WithdrawFieldValidation();
			AddRedeemFormValidation();
		}
		void Start() {
			changedTacoPoint.text = TacoManager.FormatRP(TacoConfig.TicketExchangeRate);
			changedTacoCash.text = TacoManager.FormatRP(TacoConfig.CashExchangeRate);
			pointInput.onValueChanged.AddListener(delegate {
				AddExchangeEvent(pointInput, changedTacoPoint, TacoConfig.TicketExchangeRate, purchaseByRP);
			});
			cashInput.onValueChanged.AddListener(delegate {
				AddExchangeEvent(cashInput, changedTacoCash, TacoConfig.CashExchangeRate, purchaseByCash);
			});
			prizeDetailPanel.gameObject.SetActive(false);
			redeemPrizePanel.gameObject.SetActive(false);
			confirmPanel.gameObject.SetActive(false);
		}

		private void ResetTacoInput() {
			cashInput.text = "1";
			pointInput.text = "1";
			cashInput.onValueChanged.Invoke(cashInput.text);
			pointInput.onValueChanged.Invoke(pointInput.text);
		}

		private void AddRedeemFormValidation() {
			dateOfBirthInput.onEndEdit.AddListener((string value) => {
				ValidateRedeemAge(value);
			});

			RedeemSubmit.onClick.AddListener(() => {
				if (!TacoManager.ValidateEmail(emailInput.text)) {
					ToggleRedeemErrorMsg(TacoConfig.RedeemErrorIncorrectEmail);
				}
				else if (string.IsNullOrEmpty(dateOfBirthInput.text) || string.IsNullOrEmpty(streetInput.text) || string.IsNullOrEmpty(buildingInput.text) || string.IsNullOrEmpty(countryInput.text)
								 || string.IsNullOrEmpty(stateInput.text) || string.IsNullOrEmpty(postcodeInput.text) || string.IsNullOrEmpty(cityInput.text)) {
					ToggleRedeemErrorMsg(TacoConfig.RedeemErrorNoFill);
				}
				else if (!isValidAge) {
					ValidateRedeemAge(dateOfBirthInput.text);
				}
				else {
					ToggleRedeemErrorMsg();
					SubmitRedeemForm();
				}
			});
		}

		private void ValidateRedeemAge(string value) {
			isValidAge = false;
			if (string.IsNullOrEmpty(value)) return;
			if (value.Length != 10) {//"MM/dd/yyyy".Length
				ToggleRedeemErrorMsg(TacoConfig.RedeemErrorIncorrectBirthDate);
				return;
			}

			DateTime dateOfBirth;

			if (!DateTime.TryParseExact(value, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateOfBirth)) {
				ToggleRedeemErrorMsg(TacoConfig.RedeemErrorIncorrectBirthDate);
				return;
			}

			if (DateTime.UtcNow.Year - dateOfBirth.Year < TacoConfig.legalAge) {
				ToggleRedeemErrorMsg(TacoConfig.RedeemErrorInvalidAge);
				return;
			}
			isValidAge = true;
			ToggleRedeemErrorMsg();
		}

		private void SubmitRedeemForm() {
			TacoManager.OpenMessage(TacoConfig.Processing);
			Action<string> success = (string data) => {
				TacoManager.CloseMessage();
				GeneralResult r = JsonUtility.FromJson<GeneralResult>(data);
				string notice;
				string header;
				if (r.success) {
					notice = r.msg;
					header = TacoConfig.SuccessHeader;
					emailInput.text = string.Empty;
					dateOfBirthInput.text = string.Empty;
					streetInput.text = string.Empty;
					buildingInput.text = string.Empty;
					countryInput.text = string.Empty;
					stateInput.text = string.Empty;
					postcodeInput.text = string.Empty;
					cityInput.text = string.Empty;
					TacoManager.UpdateTicketOnly(r.ticket.ToString());
					TournamentManager.Instance.SetMoneytext();
					moneyValue.text = TacoManager.FormatRP(TacoManager.User.ticket);
				}
				else {
					notice = r.err;
					header = TacoConfig.ErrorHeader;
				}
				TacoManager.OpenModalGeneralResultPanel(r.success, header, notice);
			};
			Action<string, string> fail = (string data, string error) => {
				TacoManager.CloseMessage();
				GeneralResult r = JsonUtility.FromJson<GeneralResult>(data);
				TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.ErrorHeader, r.err);
			};
			StartCoroutine(ApiManager.Instance.SubmitRedeemInfo(purchasingPrize,
			                                                    emailInput.text,
			                                                    dateOfBirthInput.text,
			                                                    streetInput.text,
			                                                    buildingInput.text,
			                                                    countryInput.text,
			                                                    stateInput.text,
			                                                    postcodeInput.text,
			                                                    cityInput.text,
			                                                    success, fail));
		}

		private void ToggleRedeemErrorMsg(string msg = null) {
			redeemFormError.transform.parent.parent.gameObject.SetActive(!string.IsNullOrEmpty(msg));
			redeemFormError.text = msg;
		}

		private void AddExchangeEvent(InputField inputExchange, Text displayText, int rate, Button btn) {
			int point = 0;
			if (int.TryParse(inputExchange.text, out point) && point > 0) {
				btn.interactable = true;
				displayText.text = TacoManager.FormatRP(point * rate);
			}
			else {
				btn.interactable = false;
			}
		}

		public void SetRemainingValue(int remainingTime, int tokenValue) {
			remainingClaim = remainingTime / 1000;
			ToggleClaimButton(false);
			tokenClaim.text = "+" + tokenValue.ToString();
			tokenPopupClaim.text = tokenClaim.text;
		}

		void Update() {
			if (remainingClaim >= 0) {
				TournamentRemainingTime();
			}
			if (Input.GetKeyDown(KeyCode.Tab)) {
				ApplyTabForRedeemInput();
				ApplyTabForWithdrawInput();
			}
		}

		private void ApplyTabForWithdrawInput() {
			if (newAddress.activeSelf && (wdAddress1Input.isFocused || withdrawAmoutInput.isFocused || wdAddress2Input.isFocused || wdCityInput.isFocused || wdStateInput.isFocused || wdZipcodeInput.isFocused)) {
				if (withdrawAmoutInput.isFocused) {
					wdAddress1Input.ActivateInputField();
				}
				else if (wdAddress1Input.isFocused) {
					wdAddress2Input.ActivateInputField();
				}
				else if (wdAddress2Input.isFocused) {
					wdCityInput.ActivateInputField();
				}
				else if (wdCityInput.isFocused) {
					wdStateInput.ActivateInputField();
				}
				else if (wdStateInput.isFocused) {
					wdZipcodeInput.ActivateInputField();
				}
				else if (wdZipcodeInput.isFocused) {
					withdrawAmoutInput.ActivateInputField();
				}
			}
		}

		private void ApplyTabForRedeemInput() {
			if (emailInput.isFocused || dateOfBirthInput.isFocused || streetInput.isFocused || buildingInput.isFocused || countryInput.isFocused || stateInput.isFocused || postcodeInput.isFocused || cityInput.isFocused) {
				if (emailInput.isFocused) {
					dateOfBirthInput.ActivateInputField();
				}
				else if (dateOfBirthInput.isFocused) {
					streetInput.ActivateInputField();
				}
				else if (streetInput.isFocused) {
					buildingInput.ActivateInputField();
				}
				else if (buildingInput.isFocused) {
					countryInput.ActivateInputField();
				}
				else if (countryInput.isFocused) {
					stateInput.ActivateInputField();
				}
				else if (stateInput.isFocused) {
					postcodeInput.ActivateInputField();
				}
				else if (postcodeInput.isFocused) {
					cityInput.ActivateInputField();
				}
				else if (cityInput.isFocused) {
					emailInput.ActivateInputField();
				}
			}
		}

		private void ToggleClaimButton(bool status) {
			timeLeftText.transform.parent.gameObject.SetActive(!status);
			claimBtn.gameObject.SetActive(status);
			timeLeftPopupText.transform.parent.gameObject.SetActive(!status);
			claimPopupBtn.gameObject.SetActive(status);
		}

		private void TournamentRemainingTime() {
			remainingClaim -= Time.deltaTime;
			timeLeftText.text = TacoConfig.timerCountdown(remainingClaim);
			timeLeftPopupText.text = timeLeftText.text;
			if (remainingClaim <= 0) {
				ToggleClaimButton(true);
			}
		}

		private void ShowPurchaseTokenPanel(int typeCurrency) {
			confirmPanel.gameObject.SetActive(true);
			exchangedCurrencyIcon.sprite = TacoConfig.currencySprites[typeCurrency];
			if (typeCurrency == 0) {
				tokenValue.text = changedTacoCash.text;
				exchangedValue.text = TacoManager.FormatRP(cashInput.text);
				exchangedNumber = cashInput.text;
			}
			else {
				tokenValue.text = changedTacoPoint.text;
				exchangedValue.text = TacoManager.FormatRP(pointInput.text);
				exchangedNumber = pointInput.text;
			}
		}

		private void SubmitPurchaseToken() {
			TacoManager.OpenMessage(TacoConfig.Processing);
			Action<string> success = (string data) => {
				TacoManager.CloseMessage();
				GeneralResult r = JsonUtility.FromJson<GeneralResult>(data);
				string notice;
				string header;
				if (r.success) {
					TacoManager.UpdateFundsWithToken(r.cash, r.token.ToString(), r.ticket.ToString());
					moneyValue.text = TacoManager.FormatGTokens(double.Parse(TacoManager.User.gToken));
					TournamentManager.Instance.SetMoneytext();
					header = TacoConfig.SuccessHeader;
					notice = r.msg;
				}
				else {
					header = TacoConfig.ErrorHeader;
					notice = r.err;
				}

				TacoManager.OpenModalGeneralResultPanel(r.success, header, notice);
			};
			Action<string, string> fail = (string data, string error) => {
				TacoManager.CloseMessage();
				TacoManager.OpenModalConnectionErrorPanel(error);
			};
			string currency = exchangedCurrencyIcon.sprite == TacoConfig.currencySprites[0] ? "0" : "2";
			StartCoroutine(ApiManager.Instance.BuyToken(currency, exchangedNumber, success, fail));
		}

		private void SetupButton() {
			purchaseByCash.onClick.AddListener(() => {
				if (string.IsNullOrEmpty(cashInput.text)) return;
				ShowPurchaseTokenPanel(0);
			});
			purchaseByRP.onClick.AddListener(() => {
				if (string.IsNullOrEmpty(pointInput.text)) return;
				ShowPurchaseTokenPanel(2);
			});
			confirmPurchaseBtn.onClick.AddListener(SubmitPurchaseToken);
			Container.transform.Find("Header/CloseButton").GetComponent<Button>().onClick.AddListener(ClosePanel);
			claimBtn.onClick.AddListener(SendClaimToken);
			claimPopupBtn.onClick.AddListener(SendClaimToken);
			prizeDetailPanel.Find("Main/Buttons/Claim").GetComponent<Button>().onClick.AddListener(delegate {
				prizeDetailPanel.gameObject.SetActive(false);
				if (int.Parse(TacoManager.User.ticket) < purchasingPrize.ticket) {
					TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.TacoRewardHeaderError, TacoConfig.TacoRewardErrorMsg);
				}
				else {
					redeemPrizePanel.gameObject.SetActive(true);
				}
			});

			withdrawBtn.onClick.AddListener(() => {

                FundPanel.SetActive(false);
                availableFund.text = TacoManager.FormatCash(TacoManager.User.funds);
                defaultAddress.text = TacoManager.User.contactProfile.FullAddress();
                submitWithdrawBtn.interactable = false;
                if (string.IsNullOrEmpty(defaultAddress.text))
                {
                    switchAddressBtn.interactable = false;
                    newAddress.SetActive(true);
                    defaultAddress.transform.parent.gameObject.SetActive(false);
                    UpdateSwitchBtnText(TacoConfig.NoDefaultAddress);
                }
                else
                {
                    switchAddressBtn.interactable = true;
                    newAddress.SetActive(false);
                    defaultAddress.transform.parent.gameObject.SetActive(true);
                    UpdateSwitchBtnText("Use new contact information");
                }
                ResetWithdrawInputField();
                WithdrawPanel.SetActive(true);

                /*CountryLocationDetector.Instance.startDetectLocation(transform, (string rawdata, bool can_continue) => {
                    if (can_continue)
                    {
                        FundPanel.SetActive(false);
                        availableFund.text = TacoManager.FormatCash(TacoManager.User.funds);
                        defaultAddress.text = TacoManager.User.contactProfile.FullAddress();
                        submitWithdrawBtn.interactable = false;
                        if (string.IsNullOrEmpty(defaultAddress.text))
                        {
                            switchAddressBtn.interactable = false;
                            newAddress.SetActive(true);
                            defaultAddress.transform.parent.gameObject.SetActive(false);
                            UpdateSwitchBtnText(TacoConfig.NoDefaultAddress);
                        }
                        else
                        {
                            switchAddressBtn.interactable = true;
                            newAddress.SetActive(false);
                            defaultAddress.transform.parent.gameObject.SetActive(true);
                            UpdateSwitchBtnText("Use new contact information");
                        }
                        ResetWithdrawInputField();
                        WithdrawPanel.SetActive(true);
                    }
                });*/

                
			});
			depositeBtn.onClick.AddListener(() => {

				PayPalDeposit.instance.Init(gameObject, transform, (bool save_payment_info_status, string save_payment_info, string paymentinfo) => {
                    if (save_payment_info_status)
                        LoadCurrentBalance();
				});
				
				/* Open Disable Panel */
				//ClosePanel();
				//TournamentManager.Instance.OpenModalDepositDisablePanel();
				/* Open Disable Panel */

				// TacoSetup.Instance.LogEvent("open_deposit_page");
				// if(WebViewController.webView == null) {
				// 	WebViewController.CreateWebView();
				// }
				// WebViewController.webView.Load(Constants.BaseUrl + "deposit/" + TacoManager.User.webToken);
				// WebViewController.webView.Show();
			});
			cancelWithdrawBtn.onClick.AddListener(() => {
				FundPanel.SetActive(true);
				WithdrawPanel.SetActive(false);
			});
			submitWithdrawBtn.onClick.AddListener(SendWithdrawRequest);
			cancelDepositeBtn.onClick.AddListener(() => {
				FundPanel.SetActive(true);
				WithdrawPanel.SetActive(false);
			});
			submitDepositeBtn.onClick.AddListener(() => {

			});
			switchAddressBtn.onClick.AddListener(() => {
				bool isShowNewAddr = newAddress.activeSelf;
				newAddress.SetActive(!isShowNewAddr);
				defaultAddress.transform.parent.gameObject.SetActive(isShowNewAddr);
				UpdateSwitchBtnText(isShowNewAddr ? "Use new contact information" : "Use saved contact information");
				ToggleWithdrawBtn();
			});
		}

		private void UpdateSwitchBtnText(string text) {
			switchAddressBtn.transform.GetChild(0).GetComponent<Text>().text = text;
		}


		private void SendWithdrawRequest() {
			TacoManager.OpenMessage(TacoConfig.TacoLoginStatusMessage00);
			Action<string> onSuccess = (string data) => {
				GeneralResult r = JsonUtility.FromJson<GeneralResult>(data);
				if (r.success) {
					TacoManager.CloseMessage();
					TacoManager.OpenModalGeneralResultPanel(r.success, TacoConfig.SuccessHeader, "You will receive a confirmation email within\n1 Business day once the withdraw amount has been approved.");
					TacoManager.UpdateFundsOnly(r.cash);
					if (wdSaveProfile.isOn) {
						TacoManager.User.contactProfile.UpdateData(wdAddress1Input.text, wdAddress2Input.text, wdCityInput.text, wdStateInput.text, wdZipcodeInput.text);
					}
					ResetWithdrawInputField();
					moneyValue.text = TacoManager.FormatCash(TacoManager.User.TotalCash);
					availableFund.text = TacoManager.FormatCash(TacoManager.User.funds);
					TournamentManager.Instance.SetMoneytext();
					defaultAddress.text = TacoManager.User.contactProfile.FullAddress();
					if (switchAddressBtn.transform.GetChild(0).GetComponent<Text>().text == TacoConfig.NoDefaultAddress) {
						withdrawBtn.onClick.Invoke();
					}
				}
				else {
					TacoManager.CloseMessage();
					TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.Error, r.msg);
				}
				TacoManager.CloseMessage();
			};
			Action<string, string> onFail = (string data, string error) => {
				TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.ErrorHeader, error);
				TacoManager.CloseMessage();
			};
			if (newAddress.activeSelf) {
				StartCoroutine(ApiManager.Instance.Withdraw(withdrawAmoutInput.text, wdAddress1Input.text, wdAddress2Input.text, wdCityInput.text, wdStateInput.text, wdZipcodeInput.text, wdSaveProfile.isOn, onSuccess, onFail));
			}
			else {
				ContactProfile addr = TacoManager.User.contactProfile;
				StartCoroutine(ApiManager.Instance.Withdraw(withdrawAmoutInput.text, addr.address, addr.address2, addr.city, addr.state, addr.zipcode, false, onSuccess, onFail));
			}
		}

		private void WithdrawFieldValidation() {
			withdrawAmoutInput.onEndEdit.AddListener((string value) => {
				ToggleWithdrawBtn();
			});
			wdAddress1Input.onEndEdit.AddListener((string value) => {
				ToggleWithdrawBtn();
			});
			wdAddress2Input.onEndEdit.AddListener((string value) => {
				ToggleWithdrawBtn();
			});
			wdCityInput.onEndEdit.AddListener((string value) => {
				ToggleWithdrawBtn();
			});
			wdStateInput.onEndEdit.AddListener((string value) => {
				ToggleWithdrawBtn();
			});
			wdZipcodeInput.onEndEdit.AddListener((string value) => {
				ToggleWithdrawBtn();
			});
		}

		private void ToggleWithdrawBtn() {
			submitWithdrawBtn.interactable = !IsAllWithdrawInputsInvalid();
		}

		private bool IsAllWithdrawInputsInvalid() {
			float amount = 0;
			bool invalidValue = !(float.TryParse(withdrawAmoutInput.text, out amount) && amount > 0);
			bool emptyInput = newAddress.activeSelf && (string.IsNullOrEmpty(wdAddress1Input.text) || string.IsNullOrEmpty(wdCityInput.text) || string.IsNullOrEmpty(wdStateInput.text) || string.IsNullOrEmpty(wdZipcodeInput.text));
			return invalidValue || emptyInput;
		}

		private void ResetWithdrawInputField() {
			withdrawAmoutInput.text = string.Empty;
			wdAddress1Input.text = string.Empty;
			wdAddress2Input.text = string.Empty;
			wdCityInput.text = string.Empty;
			wdStateInput.text = string.Empty;
			wdZipcodeInput.text = string.Empty;
			wdSaveProfile.isOn = false;
		}

		private void SendClaimToken() {
			TacoManager.OpenMessage(TacoConfig.TacoLoginStatusMessage00);
			Action<string> onSuccess = (string data) => {
				ClaimInfo r = JsonUtility.FromJson<ClaimInfo>(data);
				if (r.success) {
					TacoManager.OpenClaimTokenPanel(TacoConfig.ClaimSuccessHeader, tokenClaim.text, TacoConfig.ClaimSuccessNotice);
					TacoManager.User.gToken = r.newTacoToken.ToString();
					UpdateUI(r);
					TournamentManager.Instance.SetMoneytext();
				}
				else {
					TacoManager.OpenClaimTokenPanel(TacoConfig.ClaimFailHeader, "0", TacoConfig.NoTokenNotice);
					UpdateUI(r);
				}
				TacoManager.CloseMessage();
			};
			Action<string, string> onFail = (string data, string error) => {
				TacoManager.OpenClaimTokenPanel(TacoConfig.ClaimFailHeader, "0", TacoConfig.ClaimErrorNotice);
				TacoManager.CloseMessage();
			};
			StartCoroutine(ApiManager.Instance.ClaimToken(onSuccess, onFail));
		}

		private void UpdateUI(ClaimInfo r) {
			moneyValue.text = TacoManager.FormatGTokens(double.Parse(TacoManager.User.gToken));
			tokenClaim.text = "+" + r.nextToken.ToString();
			tokenPopupClaim.text = tokenClaim.text;
			ToggleClaimButton(false);
			SetRemainingValue(r.remainingClaim, r.nextToken);
		}

		// {"success":true,"message":"","funds":"952.6999999999998","bonus_cash":"15","gTokens":14216188,"ticket":8576756}

		public void LoadCurrentBalance () {
			// TacoManager.OpenMessage(TacoConfig.Processing);
			Action<string> success = (string data) => {
				// TacoManager.CloseMessage();
				Debug.Log(data);
				GeneralResult r = JsonUtility.FromJson<GeneralResult>(data);

				if (r.success) {
					TacoManager.UpdateFundsOnly(r.cash, r.bonus_cash);
					// moneyValue.text = TacoManager.FormatCash(TacoManager.User.TotalCash);
					// availableFund.text = TacoManager.FormatCash(TacoManager.User.funds);
					TournamentManager.Instance.SetMoneytext();
				}
			};
			Action<string, string> fail = (string data, string error) => {
				TacoManager.CloseMessage();
				//do nothing for now
			};

			StartCoroutine(ApiManager.Instance.GetUserBalance(success, fail));	
		}
		private void ClosePanel() {
			TokenPanel.SetActive(false);
			PointPanel.SetActive(false);
			FundPanel.SetActive(false);
			Container.gameObject.SetActive(false);
			if (fromMainMenu) TacoManager.TacoTournamentsCanvas.SetActive(false);
			LoadCurrentBalance ();
            back?.Invoke();
        }

		public void Init(int currencyType) {
			fromMainMenu = !TacoManager.TacoTournamentsCanvas.activeSelf;
			if (fromMainMenu) TacoManager.TacoTournamentsCanvas.SetActive(true);
			Container.gameObject.SetActive(true);
			moneyIcon.sprite = TacoConfig.currencySprites[currencyType];
			if (currencyType == 0) {
				title.text = TacoConfig.YourFundsHeader;
				FundPanel.SetActive(true);
				moneyValue.text = TacoManager.FormatMoney(TacoManager.User.TotalCash, currencyType);
			}
			else if (currencyType == 1) {
				title.text = TacoConfig.YourTokensHeader;
				TokenPanel.SetActive(true);
				moneyValue.text = TacoManager.FormatGTokens(double.Parse(TacoManager.User.gToken));
				ResetTacoInput();
			}
			else if (currencyType == 2) {
				title.text = TacoConfig.PrizesHeader;
				PointPanel.SetActive(true);
				moneyValue.text = TacoManager.FormatRP(TacoManager.User.ticket);
				if (!alreadyLoadPrize) {
					GetPrizesList();
				}
			}
		}

		private void GetPrizesList() {
			TacoManager.OpenMessage(TacoConfig.Processing);
			Action<string> success = (string data) => {
				alreadyLoadPrize = true;
				TacoManager.CloseMessage();
				PrizesResult r = JsonUtility.FromJson<PrizesResult>(data);
				LoadPrizes(r.prizes);
			};
			Action<string, string> fail = (string data, string error) => {
				TacoManager.CloseMessage();
				TacoManager.OpenModalConnectionErrorPanel(error);
			};

			StartCoroutine(ApiManager.Instance.GetPrizes(success, fail));
		}

		private void LoadPrizes(Prize[] prizeList) {
			for (int i = 1; i < prizeListRect.childCount; i++) {
				Destroy(prizeListRect.GetChild(i).gameObject);
			}
			float height = 530;
			float width = prizeListRect.rect.width;
			float padding = 30;
			int prizePerRow = 1;
			for (int i = 0; i < prizeList.Length; i++) {
				Prize prize = prizeList[i];
				RectTransform row = Instantiate(prizePrefab, prizeListRect).GetComponent<RectTransform>();
				row.sizeDelta = new Vector2(width / prizePerRow - padding * 2, height - padding);
				row.localPosition = new Vector3(padding + (i % prizePerRow) * (width / prizePerRow), -250 - padding - height * (i / prizePerRow));
				row.Find("PrizeName").GetComponent<Text>().text = prize.name;
				row.Find("Buttons/Token/Text").GetComponent<Text>().text = TacoManager.FormatRP(prize.ticket);
				Image prizeImage = row.Find("Image").GetComponent<Image>();
				StartCoroutine(ApiManager.Instance.WWWPrizeImage(prize.images, prizeImage));
				Button btn = row.Find("Buttons/Claim").GetComponent<Button>();
				btn.onClick.RemoveAllListeners();
				btn.onClick.AddListener(() => {
					prizeDetailPanel.gameObject.SetActive(true);
					prizeDetailPanel.Find("Main/Header").GetComponent<Text>().text = prize.name;
					prizeDetailPanel.Find("Main/Description").GetComponent<Text>().text = prize.description;
					prizeDetailPanel.Find("Main/Image").GetComponent<Image>().sprite = prizeImage.sprite;
					prizeDetailPanel.Find("Main/Buttons/Token/Text").GetComponent<Text>().text = TacoManager.FormatRP(prize.ticket);
					purchasingPrize = prize;
				});
			}
			prizeListRect.sizeDelta = new Vector2(prizeListRect.sizeDelta.x, (prizeList.Length / prizePerRow) * (height + padding));
		}
	}
}