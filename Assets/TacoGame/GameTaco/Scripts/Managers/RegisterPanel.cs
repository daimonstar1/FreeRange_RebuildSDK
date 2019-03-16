using System;
using UnityEngine;
using UnityEngine.UI;


namespace GameTaco {

	public class RegisterPanel : MonoBehaviour {

		public static RegisterPanel Instance;

		public Button RegisterButton;

		public InputField UserInput;
		public GameObject UserOkay;
		public InputField EmailInput;
		public GameObject EmailOkay;
		public InputField PasswordInput;
		public GameObject PasswordOkay;
		public InputField ConfirmInput;
		public GameObject ConfirmOkay;
		public InputField ReferenceCodeInput;
		public Toggle PolicyToggle;
		public Text StatusText = null;

		public bool inputValue = false;
		bool emailValidated = false;
		bool userValidated = false;
		bool passwordValidated = false;
		void Awake() {
			Instance = this;
			AddEventListener();
			Init();
		}

		private void AddValidateMessage() {
			//passwordErrorMsg = PasswordNotOkay.transform.Find ("ErrorTooltip/ErrorMsg").GetComponent<Text> ();
			//confirmErrorMsg = ConfirmNotOkay.transform.Find ("ErrorTooltip/ErrorMsg").GetComponent<Text> ();
			//emailErrorMsg = EmailNotOkay.transform.Find ("ErrorTooltip/ErrorMsg").GetComponent<Text> ();
			//userErrorMsg = UserNotOkay.transform.Find ("ErrorTooltip/ErrorMsg").GetComponent<Text> ();
		}

		private void AddEventListener() {
			PasswordInput.onEndEdit.AddListener(delegate { ChangePasswordState(); });
			EmailInput.onEndEdit.AddListener(delegate { ChangeEmailState(); });
			ConfirmInput.onEndEdit.AddListener(delegate { ChangeConfirmPasswordState(); });
			UserInput.onEndEdit.AddListener(delegate { ChangeInputState(); });
			PolicyToggle.onValueChanged.AddListener(delegate { ChangePolicyState(); });
		}

		// Use this for initialization
		public void Init() {

			PasswordInput.gameObject.SetActive(true);
			ConfirmInput.gameObject.SetActive(true);
			EmailInput.gameObject.SetActive(true);
			UserInput.gameObject.SetActive(true);

			PasswordInput.placeholder.GetComponent<Text>().text = TacoConfig.TacoLoginPassword;
			EmailInput.placeholder.GetComponent<Text>().text = TacoConfig.TacoLoginEmail;
			ConfirmInput.placeholder.GetComponent<Text>().text = TacoConfig.TacoLoginConfirm;
			UserInput.placeholder.GetComponent<Text>().text = TacoConfig.TacoLoginUser;
			ReferenceCodeInput.placeholder.GetComponent<Text>().text = TacoConfig.ReferenceCode;

			PasswordInput.text = string.Empty;
			EmailInput.text = string.Empty;
			ConfirmInput.text = string.Empty;
			UserInput.text = string.Empty;
			ReferenceCodeInput.text = string.Empty;

			userValidated = false;
			emailValidated = false;
			passwordValidated = false;
			passwordValidated = false;
			ToggleRegisterButton(false);
			ToggleInputOk(false);
			PolicyToggle.isOn = false;
			inputValue = false;
			ShowErrorText();
		}

		private void ToggleInputOk(bool status) {
			PasswordOkay.SetActive(status);
			ConfirmOkay.SetActive(status);
			EmailOkay.SetActive(status);
			UserOkay.SetActive(status);
		}

		private void ToggleRegisterButton(bool status) {
			RegisterButton.interactable = status;
			if (status) {
				ToggleInputOk(true);
			}
		}

		private bool AllInputIsValid() {
			return userValidated && emailValidated && passwordValidated && passwordValidated;// && PolicyToggle.isOn;
		}

		private void ChangePolicyState() {
			if (StatusText.text == TacoConfig.TacoRegisteredErrorMessage02 && PolicyToggle.isOn) {
				ShowErrorText();
			}
			ToggleRegisterButton(AllInputIsValid());
		}

		public void ChangeInputState() {
			inputValue = true;
			if (string.IsNullOrEmpty(UserInput.text)) {
				userValidated = false;
				ShowErrorText(TacoConfig.TacoRegisteredErrorMessage03);
				UserOkay.SetActive(false);
			}
			else if (UserInput.text.Length < TacoConfig.UsernameMinLength) {
				userValidated = false;
				ShowErrorText(TacoConfig.TacoRegisteredErrorUsernameLength);
				UserOkay.SetActive(false);
			}
			else if (!TacoManager.ValidateUsername(UserInput.text)) {
				userValidated = false;
				ShowErrorText(TacoConfig.TacoRegisteredErrorUsernameFormat);
				UserOkay.SetActive(false);
			}
			else {
				userValidated = true;
				UserOkay.SetActive(true);
				if (StatusText.text == TacoConfig.TacoRegisteredErrorMessage03) {
					ShowErrorText();
				}
			}
			ToggleRegisterButton(AllInputIsValid());
		}

		private void ChangeEmailState() {
			inputValue = true;
			if (string.IsNullOrEmpty(EmailInput.text)) {
				emailValidated = false;
				EmailOkay.SetActive(false);
				ShowErrorText(TacoConfig.TacoRegisteredErrorMessage06);
			}
			else {
				var textEmail = EmailInput.text;
				bool isEmail = TacoManager.ValidateEmail(textEmail);
				if (isEmail == true) {
					emailValidated = true;
					ShowErrorText();
					ChangeInputState();
					EmailOkay.SetActive(true);

				}
				else {
					emailValidated = false;
					ShowErrorText(TacoConfig.TacoRegisteredErrorMessage01);
					EmailOkay.SetActive(false);
				}
			}
			ToggleRegisterButton(AllInputIsValid());
		}
		public void ChangePasswordState() {
			inputValue = true;
			if (string.IsNullOrEmpty(PasswordInput.text)) {
				ShowErrorText(TacoConfig.TacoRegisteredErrorMessage04);
				passwordValidated = false;
				PasswordOkay.SetActive(false);
			}
			else if (PasswordInput.text.Length < TacoConfig.PasswordMinLength) {
				passwordValidated = false;
				ShowErrorText(TacoConfig.TacoRegisteredErrorMessage07);
				PasswordOkay.SetActive(false);
			}
			else {
				if (ConfirmInput.text == PasswordInput.text) {
					passwordValidated = true;
					ShowErrorText();
					ChangeEmailState();
					ChangeInputState();
					PasswordOkay.SetActive(true);
				}
				else if (!string.IsNullOrEmpty(ConfirmInput.text)) {
					passwordValidated = false;
					ShowErrorText(TacoConfig.TacoRegisteredErrorMessage00);
					PasswordOkay.SetActive(false);
				}
				else if (StatusText.text == TacoConfig.TacoRegisteredErrorMessage07) {
					ShowErrorText();
				}
			}
			ToggleRegisterButton(AllInputIsValid());
		}

		public void ShowErrorText(string msg = null) {
			StatusText.text = msg;
			StatusText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(msg));
		}

		public void ChangeConfirmPasswordState() {
			inputValue = true;
			if (string.IsNullOrEmpty(ConfirmInput.text) && !ConfirmInput.isFocused) {
				ShowErrorText(TacoConfig.TacoRegisteredErrorMessage08);
				//confirmErrorMsg.text = TacoConfig.TacoRegisteredErrorMessage08;
				passwordValidated = false;
				ConfirmOkay.SetActive(false);
				//ConfirmNotOkay.SetActive (true);
			}
			else {
				if (ConfirmInput.text == PasswordInput.text) {
					if (ConfirmInput.text.Length > 3) {
						passwordValidated = true;
						ShowErrorText();
						ChangePasswordState();
						ChangeEmailState();
						ChangeInputState();
						ConfirmOkay.SetActive(true);
					}
					else {
						passwordValidated = false;
						ShowErrorText(TacoConfig.TacoRegisteredErrorMessage07);
						ConfirmOkay.SetActive(false);
					}
				}
				else {
					passwordValidated = false;
					//confirmErrorMsg.text = TacoConfig.TacoRegisteredErrorMessage00;
					ShowErrorText(TacoConfig.TacoRegisteredErrorMessage00);
					ConfirmOkay.SetActive(false);
					//ConfirmNotOkay.SetActive (true);
				}
			}
			ToggleRegisterButton(AllInputIsValid());
		}

		void Update() {
			if (Input.GetKeyDown(KeyCode.Tab)) {

				if (UserInput.isFocused) {
					EmailInput.ActivateInputField();
				}
				else if (EmailInput.isFocused) {
					PasswordInput.ActivateInputField();
				}
				else if (PasswordInput.isFocused) {
					ConfirmInput.ActivateInputField();
				}
				else if (ConfirmInput.isFocused) {
					UserInput.ActivateInputField();
				}
				else {
					UserInput.ActivateInputField();
				}

			}
			else if (Input.GetKeyDown(KeyCode.Return)) {
				Register();
			}
		}

		private bool validateInput() {

			var email = EmailInput.text;
			var password = PasswordInput.text;
			var confirm = ConfirmInput.text;
			var age = PolicyToggle.isOn;
			var user = UserInput.text;
			bool validated = true;

			if (string.IsNullOrEmpty(user)) {
				validated = false;
				ShowErrorText(TacoConfig.TacoRegisteredErrorMessage03);
				return validated;
			}

			if (string.IsNullOrEmpty(email)) {
				validated = false;
				ShowErrorText(TacoConfig.TacoRegisteredErrorMessage06);
				return validated;
			}
			else {
				bool isEmail = TacoManager.ValidateEmail(email);

				if (!isEmail) {
					validated = false;
					ShowErrorText(TacoConfig.TacoRegisteredErrorMessage01);
					return validated;
				}
			}

			if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm)) {
				validated = false;
				ShowErrorText(TacoConfig.TacoRegisteredErrorMessage04);
				return validated;
			}
			// if (string.IsNullOrEmpty(user)) {
			// 	validated = false;
			// 	StatusText.text = TacoConfig.TacoRegisteredErrorMessage03;
			// }

			if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(confirm)) {

				ShowErrorText(TacoConfig.TacoRegisteredErrorMessage04);

				if (password == confirm) {

					if (password.Length <= 3) {
						validated = false;
						ShowErrorText(TacoConfig.TacoRegisteredErrorMessage07);
						return validated;
					}
				}
				else {
					validated = false;
					ShowErrorText(TacoConfig.TacoRegisteredErrorMessage00);
					return validated;
				}

			}
			else if (!ConfirmInput.isFocused) {
				validated = false;
				ShowErrorText(TacoConfig.TacoRegisteredErrorMessage04);
				return validated;
			}

			if (!age) {
				validated = false;
				ShowErrorText(TacoConfig.TacoRegisteredErrorMessage02);
				ToggleRegisterButton(false);
				return validated;
			}
			return validated;
		}
		public void Register() {

			Debug.Log("Register Pressed");

			var email = EmailInput.text;
			var password = PasswordInput.text;
			var age = PolicyToggle.isOn;
			var user = UserInput.text;
			var refCode = ReferenceCodeInput.text;

			if (validateInput()) {

				//StartCoroutine(postRegister(email, password, age, avatar, user));

				TacoManager.OpenMessage(TacoConfig.TacoRegisteredStatusMessage00);

				Action<string> success = (string data) => {
					Debug.Log(data);
					LoginResult r = JsonUtility.FromJson<LoginResult>(data);

					if (r.success) {
						TacoManager.CloseMessage();
						// email sent, have to wait for them to verify.
						//TacoManager.OpenModal(TacoConfig.TacoRegisteredModalTitle, r.message, null, "RegisterResult");
						TacoManager.OpenModalRegisterPanel(string.IsNullOrEmpty(r.displayName) ? r.name : r.displayName);
						LoginPanel.Instance.SetCredential(EmailInput.text, PasswordInput.text);
						// clean up the registerpanel
						Init();
						TacoManager.CloseTaco();
						TacoManager.TacoAuthCanvas.SetActive(true);
						LoginPanel.Instance.gameObject.SetActive(true);
						LoginPanel.Instance.Login();
					}
					else {
						TacoSetup.Instance.LogEvent("signup_failed");
						// an error returned
						TacoManager.CloseMessage();
						//TacoManager.OpenModal(TacoConfig.TacoRegisteredErrorHeader, r.message);
						TacoManager.OpenModalAccountCreationErrorPanel(TacoConfig.TacoRegisteredErrorHeader, r.message);
					}
				};

				Action<string, string> fail = (string data, string error) => {
					TacoSetup.Instance.LogEvent("signup_failed");
					ErrorResult errorResult = JsonUtility.FromJson<ErrorResult>(data);
					Debug.Log("Error on register : " + data);
					if (!string.IsNullOrEmpty(error)) {
						Debug.Log("Error : " + error);
					}
					TacoManager.CloseMessage();
					string msg = data + (string.IsNullOrEmpty(error) ? string.Empty : " : " + error);
					if (!string.IsNullOrEmpty(data)) {
						if (!errorResult.success) {
							msg = errorResult.message;
						}
					}

					TacoManager.OpenModalAccountCreationErrorPanel(TacoConfig.TacoRegisteredErrorHeader, msg);
          if (errorResult.verErr) {
            TacoManager.OpenModalIncorrectVersionPanel(errorResult.message);
          }
				};

				StartCoroutine(ApiManager.Instance.Register(user, email, password, age, refCode, success, fail));
			}
		}
	}
}
