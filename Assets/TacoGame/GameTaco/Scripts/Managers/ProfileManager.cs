using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine;

namespace GameTaco {
	public class ProfileManager : MonoBehaviour {
		public static ProfileManager Instance;
		public Text username;
		public Text email;
		public Text address;
		public Text referenceCode;
		public Text recentTransaction;
		public RectTransform mainPageRect;
		public GameObject TransactionPrefab;
		public RectTransform transactionRect;
		private UserTransaction[] transactions;

		public InputField currentPassword;
		public InputField newPassword;
		public InputField confirmPassword;
		public GameObject NewPasswordNotOk;
		public GameObject ConfirmPasswordNotOk;
		public GameObject CurrentPasswordNotOk;
		public Button SubmitPasswordChangeBtn;
		private Text newPasswordErrorMsg;
		private Text confirmPasswordErrorMsg;
		private Text currentPasswordErrorMsg;

		public InputField addressInput;
		public InputField address2Input;
		public InputField cityInput;
		public InputField stateInput;
		public InputField zipcodeInput;
		public Button SubmitAddessesBtn;

		public Transform activeTournamentAccordion;

		private bool isLoadingTransaction;
		private bool isLoadingActiveTours;

		void Awake() {
			RectTransform rect = transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();//make Viewport stretch so that childrent objects get correct demension
			rect.anchorMin = new Vector2(0, 0);
			rect.anchorMax = new Vector2(1, 1);
			Instance = this;
			newPasswordErrorMsg = NewPasswordNotOk.transform.GetChild(0).GetChild(0).GetComponent<Text>();
			confirmPasswordErrorMsg = ConfirmPasswordNotOk.transform.GetChild(0).GetChild(0).GetComponent<Text>();
			currentPasswordErrorMsg = CurrentPasswordNotOk.transform.GetChild(0).GetChild(0).GetComponent<Text>();
			SubmitPasswordChangeBtn.interactable = false;
			AddPasswordChangeEvent();
			AddAddressesEvent();
		}

		void Start() {
		}

		void Update() {
			if (Input.GetKeyDown(KeyCode.Tab)) {
				ApplyTabForPassword();
				ApplyTabForAddress();
			}
		}

		private void ApplyTabForPassword() {
			if (currentPassword.isFocused || newPassword.isFocused || confirmPassword.isFocused) {
				if (currentPassword.isFocused) {
					newPassword.ActivateInputField();
				}
				else if (newPassword.isFocused) {
					confirmPassword.ActivateInputField();
				}
				else if (confirmPassword.isFocused) {
					currentPassword.ActivateInputField();
				}
			}
		}

		private void ApplyTabForAddress() {
			if (addressInput.isFocused || address2Input.isFocused || cityInput.isFocused || stateInput.isFocused || zipcodeInput.isFocused) {
				if (addressInput.isFocused) {
					address2Input.ActivateInputField();
				}
				else if (address2Input.isFocused) {
					cityInput.ActivateInputField();
				}
				else if (cityInput.isFocused) {
					stateInput.ActivateInputField();
				}
				else if (stateInput.isFocused) {
					zipcodeInput.ActivateInputField();
				}
				else if (zipcodeInput.isFocused) {
					addressInput.ActivateInputField();
				}
			}
		}

		private void AddAddressesEvent() {
			SubmitAddessesBtn.onClick.AddListener(() => {
				if (!string.IsNullOrEmpty(addressInput.text + address2Input.text + cityInput.text + stateInput.text + zipcodeInput.text)) {
					TacoManager.OpenMessage(TacoConfig.Processing);
					Action<string> success = (string data) => {
						TacoManager.CloseMessage();
						GeneralResult r = JsonUtility.FromJson<GeneralResult>(data);
						string notice = string.Empty;
						string header = string.Empty;
						if (r.success) {
							notice = r.msg;
							header = TacoConfig.SuccessHeader;
							TacoManager.User.contactProfile.UpdateData(addressInput.text, address2Input.text, cityInput.text, stateInput.text, zipcodeInput.text);
							FillFullAddress();
						}
						else {
							notice = r.err;
							header = TacoConfig.ErrorHeader;
							FillAddressesField();
						}
						TacoManager.OpenModalGeneralResultPanel(r.success, header, notice);
					};
					Action<string, string> fail = (string data, string error) => {
						TacoManager.CloseMessage();
						GeneralResult r = JsonUtility.FromJson<GeneralResult>(data);
						TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.ErrorHeader, r.err);
					};
					StartCoroutine(ApiManager.Instance.ChangeAddress(addressInput.text, address2Input.text, cityInput.text, stateInput.text, zipcodeInput.text, success, fail));
				}
			});
		}

		private void AddPasswordChangeEvent() {
			currentPassword.onEndEdit.AddListener((string value) => {
				if (string.IsNullOrEmpty(value)) return;
				if (value.Length < TacoConfig.PasswordMinLength) {
					CurrentPasswordNotOk.SetActive(true);
					currentPasswordErrorMsg.text = TacoConfig.TacoRegisteredErrorMessage07;
					SubmitPasswordChangeBtn.interactable = false;
				}
				else {
					CurrentPasswordNotOk.SetActive(false);
					SubmitPasswordChangeBtn.interactable = !NewPasswordNotOk.activeSelf && !ConfirmPasswordNotOk.activeSelf && !string.IsNullOrEmpty(confirmPassword.text);
				}
			});
			newPassword.onEndEdit.AddListener((string value) => {
				if (string.IsNullOrEmpty(value)) return;
				if (value.Length < TacoConfig.PasswordMinLength) {
					NewPasswordNotOk.SetActive(true);
					newPasswordErrorMsg.text = TacoConfig.TacoRegisteredErrorMessage07;
					SubmitPasswordChangeBtn.interactable = false;
				}
				else {
					NewPasswordNotOk.SetActive(false);
					if (!string.IsNullOrEmpty(confirmPassword.text)) {
						if (value == confirmPassword.text) {
							ConfirmPasswordNotOk.SetActive(false);
							SubmitPasswordChangeBtn.interactable = currentPassword.text.Length >= TacoConfig.PasswordMinLength;
						}
						else if (!ConfirmPasswordNotOk.activeSelf) {
							ConfirmPasswordNotOk.SetActive(true);
							confirmPasswordErrorMsg.text = TacoConfig.TacoRegisteredErrorMessage00;
							SubmitPasswordChangeBtn.interactable = false;
						}
					}
					else {
						SubmitPasswordChangeBtn.interactable = false;
					}
				}
			});
			confirmPassword.onEndEdit.AddListener((string value) => {
				if (string.IsNullOrEmpty(value)) return;
				if (value != newPassword.text) {
					ConfirmPasswordNotOk.SetActive(true);
					confirmPasswordErrorMsg.text = TacoConfig.TacoRegisteredErrorMessage00;
					SubmitPasswordChangeBtn.interactable = false;
				}
				else if (value.Length >= TacoConfig.PasswordMinLength) {
					ConfirmPasswordNotOk.SetActive(false);
					SubmitPasswordChangeBtn.interactable = currentPassword.text.Length >= TacoConfig.PasswordMinLength;
				}
			});

			SubmitPasswordChangeBtn.onClick.AddListener(() => {
				TacoManager.OpenMessage(TacoConfig.Processing);
				Action<string> success = (string data) => {
					TacoManager.CloseMessage();
					GeneralResult r = JsonUtility.FromJson<GeneralResult>(data);
					string notice = r.success ? r.msg : r.err;
					string header = r.success ? TacoConfig.SuccessHeader : TacoConfig.Error;
					TacoManager.OpenModalGeneralResultPanel(r.success, header, notice);
					confirmPassword.text = string.Empty;
					newPassword.text = string.Empty;
					currentPassword.text = string.Empty;
				};
				Action<string, string> fail = (string data, string error) => {
					TacoManager.CloseMessage();
					GeneralResult r = JsonUtility.FromJson<GeneralResult>(data);
					TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.ErrorHeader, r.err);
				};
				StartCoroutine(ApiManager.Instance.ChangePassword(currentPassword.text, newPassword.text, confirmPassword.text, success, fail));
				SubmitPasswordChangeBtn.interactable = false;
			});
		}

		private void RemovePasswordInputContent() {
			NewPasswordNotOk.SetActive(false);
			CurrentPasswordNotOk.SetActive(false);
			ConfirmPasswordNotOk.SetActive(false);
			confirmPassword.text = string.Empty;
			newPassword.text = string.Empty;
			currentPassword.text = string.Empty;
		}


		public void StretchContent(float height) {
			RemovePasswordInputContent();
			if (height > 0) {
				StartCoroutine(StartStretchAnimation(height));
			}
			else {
				StartCoroutine(StartShrinkAnimation(-height));
			}
		}

		private IEnumerator StartStretchAnimation(float height) {
			float newYSize = mainPageRect.sizeDelta.y + height;
			while (mainPageRect.sizeDelta.y < newYSize) {
				yield return new WaitForSeconds(TacoConfig.accodionSpeed);
				mainPageRect.sizeDelta += new Vector2(0, TacoConfig.accodionDistance.y);
			}
			mainPageRect.sizeDelta = new Vector2(mainPageRect.sizeDelta.x, newYSize);
		}

		private IEnumerator StartShrinkAnimation(float height) {
			mainPageRect.localPosition = new Vector3(mainPageRect.localPosition.x, 0);
			float newYSize = mainPageRect.sizeDelta.y - height;
			while (mainPageRect.sizeDelta.y > newYSize) {
				yield return new WaitForSeconds(TacoConfig.accodionSpeed);
				mainPageRect.sizeDelta -= new Vector2(0, TacoConfig.accodionDistance.y);
			}
			mainPageRect.sizeDelta = new Vector2(mainPageRect.sizeDelta.x, newYSize);
		}

		public void ShrinkImmediately(float height) {
			mainPageRect.localPosition = new Vector3(mainPageRect.localPosition.x, 0);
			mainPageRect.sizeDelta = new Vector2(mainPageRect.sizeDelta.x, mainPageRect.sizeDelta.y - height);
		}

		private void FillAddressesField() {
			addressInput.text = TacoManager.User.contactProfile.address;
			address2Input.text = TacoManager.User.contactProfile.address2;
			cityInput.text = TacoManager.User.contactProfile.city;
			stateInput.text = TacoManager.User.contactProfile.state;
			zipcodeInput.text = TacoManager.User.contactProfile.zipcode;
		}

		private void GetTransaction() {
			isLoadingTransaction = true;
			TacoManager.OpenMessage(TacoConfig.Processing);
			Action<string> success = (string data) => {
				Debug.Log(data);
				isLoadingTransaction = false;
				if (!isLoadingActiveTours) TacoManager.CloseMessage();
				TransactionResult r = JsonUtility.FromJson<TransactionResult>(data);
				if (r.transactions.Length > 0) {
					UserTransaction recent = r.transactions[0];
					recentTransaction.text = string.Format("{0} ({1})", recent.action, recent.FormatCurrency);
				}
				transactions = r.transactions;
				GenerateTransactionTable();
			};
			Action<string, string> fail = (string data, string error) => {
				isLoadingTransaction = false;
				if (!isLoadingActiveTours) TacoManager.CloseMessage();
				TacoManager.OpenModalConnectionErrorPanel(error);
				recentTransaction.text = string.Empty;
			};

			StartCoroutine(ApiManager.Instance.GetTransactions(3, success, fail));
		}

		private void GenerateTransactionTable() {
			for (int i = 1; i < transactionRect.childCount; i++) {
				Destroy(transactionRect.GetChild(i).gameObject);
			}
			float height = 140;
			float width = transactionRect.rect.width;
			float padding = 20;
			for (int i = 0; i < transactions.Length; i++) {
				RectTransform row = Instantiate(TransactionPrefab, transactionRect).GetComponent<RectTransform>();
				row.localPosition = new Vector3(row.localPosition.x, -padding - height * i);
				row.Find("Name").GetComponent<Text>().text = transactions[i].action;
				RectTransform col = row.Find("Name").GetComponent<RectTransform>();
				col.sizeDelta = new Vector2(width * 0.5f, height);
				col.localPosition = new Vector3(30, 0);
				row.Find("Value").GetComponent<Text>().text = transactions[i].FormatCurrency;
				col = row.Find("Value").GetComponent<RectTransform>();
				col.sizeDelta = new Vector2(width * 0.25f, height);
				col.localPosition = new Vector3(width * 0.55f, 0);
				row.Find("Day").GetComponent<Text>().text = TacoManager.FormatDate(transactions[i].createdAt, '-');
				col = row.Find("Day").GetComponent<RectTransform>();
				col.sizeDelta = new Vector2(width * 0.20f, height);
				col.localPosition = new Vector3(width * 0.8f, 0);
			}
			transactionRect.sizeDelta = new Vector2(transactionRect.sizeDelta.x, transactions.Length * (height + padding) + height);
		}

		private void GetActiveTournament() {
			isLoadingActiveTours = true;
			TournamentManager.Instance.getActiveTournaments((bool success) => {
				isLoadingActiveTours = false;
				if (!isLoadingTransaction) TacoManager.CloseMessage();
				toggleActiveTournaments(success);
			});
		}

		private void FillFullAddress() {
			string fullAddress = TacoManager.User.contactProfile.FullAddress();
			Debug.Log("fullAddress length: " + fullAddress.Length);
			if(fullAddress.Length > 60)
			{
				address.gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(address.GetComponent<RectTransform>().offsetMin.x, 0);
				address.gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(address.GetComponent<RectTransform>().offsetMax.x, 0);
			}
			else
			{
				address.gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(address.GetComponent<RectTransform>().offsetMin.x, 30);
				address.gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(address.GetComponent<RectTransform>().offsetMax.x, 0);
			}
			address.text = TacoManager.User.contactProfile.FullAddress();
		}

		private void FillReferenceCode() {
			referenceCode.text = TacoManager.User.referedCode;
		}

		public void Init() {
			RemovePasswordInputContent();
			toggleActiveTournaments(true);
			mainPageRect.localPosition = new Vector3(mainPageRect.localPosition.x, 0);
			username.text = TacoManager.User.name;
			email.text = TacoManager.User.email;
			FillFullAddress();
			FillReferenceCode();
			FillAddressesField();
			GetTransaction();
			Accordion ele1 = mainPageRect.Find("Body").GetChild(0).GetComponent<Accordion>();
			if (ele1.isActive) {
				mainPageRect.Find("Body").GetChild(1).GetComponent<Accordion>().CloseCurrentStretch();
			}
			else {
				ele1.CloseCurrentStretch();//close all element
			}

			GetActiveTournament();
		}

		private void toggleActiveTournaments(bool status) {
			activeTournamentAccordion.GetChild(1).gameObject.SetActive(status);
			activeTournamentAccordion.GetChild(2).gameObject.SetActive(status);
			activeTournamentAccordion.GetChild(3).gameObject.SetActive(status);
			activeTournamentAccordion.GetChild(4).gameObject.SetActive(status);
			activeTournamentAccordion.GetChild(7).gameObject.SetActive(status);
		}
	}
}