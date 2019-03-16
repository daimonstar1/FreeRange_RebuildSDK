using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GameTaco {
	public class TournamentInvite : MonoBehaviour {
		public InputField EmailInput;
		public Image EmailOkay;
		public Image EmailStatusHead;
		public Text errorMessage;
		public TournamentInvite next;
		public TournamentInvite prev;
		public Button SendEmail;
		private Button EmailRemove;
		public bool isInManage;
		public string previousValidEmailValue;
		public List<string> reservedEmails = new List<string>();

		private void Awake() {
			EmailRemove = EmailStatusHead.GetComponent<Button>();
			Transform errorTooltip = errorMessage.transform.parent;
			errorTooltip.GetComponent<Canvas>().overrideSorting = false;
			errorTooltip.gameObject.SetActive(false);
		}
		private void Start() {
			AddMessageErrorEvent();
			SendEmailEvent();
		}

		private void Update() {
			//switch neighbour field to unfocus status
			if (EmailInput.isFocused && EmailStatusHead.sprite != TacoConfig.EmailIconFocus) {
				EmailStatusHead.sprite = TacoConfig.EmailIconFocus;
				UnFocusEmailList();
			}
			else if (!EmailInput.isFocused && EmailStatusHead.sprite == TacoConfig.EmailIconFocus) {
				//if null move focus icon to the most left or nearest right
				if (!string.IsNullOrEmpty(EmailInput.text)) {
					FocusOnNearestField();
				}
				//show placeholder text
				else if (EmailInput.placeholder.color.a != 1) {
					TogglePlaceholder(true);
				}
			}
		}

		private void SendEmailEvent() {
			SendEmail.onClick.AddListener(() => {
				Action callback = () => {
					GameObject ob = Instantiate(TacoConfig.NotJoinedPlayerPrefab, Vector3.zero, Quaternion.identity, transform.parent) as GameObject;
					ob.transform.Find("EmailInput/Send").gameObject.SetActive(false);
					ob.transform.Find("EmailInput/NoticeReminder").gameObject.SetActive(false);
					RectTransform currentRect = GetComponent<RectTransform>();
					Text inviteeEmailText = ob.transform.Find("EmailInput/NameInput").GetComponent<Text>();
					inviteeEmailText.text = GetEmail();
					RectTransform r = ob.GetComponent<RectTransform>();
					r.localPosition = currentRect.localPosition;
					r.sizeDelta = currentRect.sizeDelta;
					ManageTournament.Instance.AddRemoveEmailEvent(ob, r, inviteeEmailText.text, this);
				};
				TournamentManager.Instance.InviteFriend(GetEmail(), callback);
			});
		}

		private void FocusOnNearestField() {
			if (isInManage) {
				EmailStatusHead.sprite = TacoConfig.EmailIconRemove;
				EmailRemove.onClick.AddListener(() => {
					EmailStatusHead.sprite = TacoConfig.EmailIconInactive;
					EmailInput.text = string.Empty;
					EmailInput.Select();
					EmailRemove.onClick.RemoveAllListeners();
				});
			}
			else {
				EmailStatusHead.sprite = TacoConfig.EmailIconActive;
			}
			TournamentInvite current = prev;
			TournamentInvite target = null;
			while (current != null) {
				if (string.IsNullOrEmpty(current.EmailInput.text)) {
					target = current;
				}
				current = current.prev;
			}

			if (target != null) {
				target.SetFocus();
				return;
			}

			current = next;
			while (current != null) {
				if (string.IsNullOrEmpty(current.EmailInput.text)) {
					current.SetFocus();
					break;
				}
				current = current.next;
			}
		}

		private void UnFocusEmailList() {
			TournamentInvite current = next;
			while (current != null) {
				current.SetUnfocus();
				current = current.next;
			}
			current = prev;
			while (current != null) {
				current.SetUnfocus();
				current = current.prev;
			}
		}

		private void RemoveDuplidateError() {
			TournamentInvite nearestDuplidate = NearestDuplicateError();
			if (nearestDuplidate != null) {
				nearestDuplidate.ToggleEmailOkay(true);
			}
		}

		private TournamentInvite NearestDuplicateError() {
			TournamentInvite current = prev;
			while (current != null) {
				if (current.GetEmail() == previousValidEmailValue) {
					return current;
				}
				current = current.prev;
			}
			current = next;
			while (current != null) {
				if (current.GetEmail() == previousValidEmailValue) {
					return current;
				}
				current = current.next;
			}
			return null;
		}

		private bool ValidateDuplidateEmail(string emailToCheck) {
			TournamentInvite current = next;
			while (current != null) {
				if (current.GetEmail() == emailToCheck) {
					return false;
				}
				current = current.next;
			}
			current = prev;
			while (current != null) {
				if (current.GetEmail() == emailToCheck) {
					return false;
				}
				current = current.prev;
			}
			return true;
		}

		private void AddMessageErrorEvent() {
			//GameObject messagePanel = errorMessage.transform.parent.gameObject;
			EmailInput.onEndEdit.AddListener(delegate { ValueChangeCheck(); });
		}

		public void SetUnfocus() {
			if (EmailStatusHead.sprite == TacoConfig.EmailIconFocus) {
				if (string.IsNullOrEmpty(EmailInput.text)) {
					EmailStatusHead.sprite = TacoConfig.EmailIconInactive;
					TogglePlaceholder(false);
					if (prev != null && string.IsNullOrEmpty(prev.EmailInput.text)) SetInteractable(false);
				}
				else {
					EmailStatusHead.sprite = TacoConfig.EmailIconActive;
				}
			}
		}

		public string GetEmail() {
			return EmailInput.text;
		}

		public void SetActive() {
			EmailStatusHead.sprite = TacoConfig.EmailIconActive;
		}

		public void SetFocus() {
			//EmailInput.Select ();
			EmailStatusHead.sprite = TacoConfig.EmailIconFocus;
			TogglePlaceholder(true);
		}

		public void TogglePlaceholder(bool status) {
			EmailInput.placeholder.color = new Color32(140, 146, 173, status ? (byte)255 : (byte)0);
		}

		//
		public void SetInteractable(bool status) {
			EmailInput.interactable = status;
			EmailStatusHead.sprite = TacoConfig.EmailIconInactive;
		}

		public bool IsInteractable() {
			return EmailInput.interactable;
		}

		public void ValueChangeCheck() {
			errorMessage.transform.parent.gameObject.SetActive(false);
			EmailOkay.color = new Color32(255, 255, 255, 0);
			SendEmail.gameObject.SetActive(false);
			ValidateUserOrEmail(GetEmail());
		}

		public void ToggleEmailOkay(bool status) {
			if (string.IsNullOrEmpty(EmailInput.text)) return;
			EmailOkay.color = new Color32(255, 255, 255, 255);
			EmailOkay.sprite = TacoConfig.emailStatusSprites[status ? 0 : 1];
			EmailInput.textComponent.color = status ? Color.black : Color.red;

			errorMessage.transform.parent.gameObject.SetActive(!status);

			if (isInManage) {
				SendEmail.gameObject.SetActive(status);
				//EmailStatusHead.sprite = status ? TacoConfig.EmailIconRemove : TacoConfig.EmailIconFocus;
			}
		}


		public void FillWithValidateData(string email, bool valid, string errMsg = "", string prevValidEmail = "") {
			EmailInput.text = email;
			errorMessage.text = errMsg;
			previousValidEmailValue = prevValidEmail;
			SetActive();
			ToggleEmailOkay(valid);
		}

		public bool IsInvalid() {
			return EmailOkay.sprite == TacoConfig.emailStatusSprites[1] && EmailOkay.color.a == 1;//show x mark in email okay
		}
		public bool IsValid() {
			return !string.IsNullOrEmpty(EmailInput.text) && EmailInput.textComponent.color == Color.black;
			//return !string.IsNullOrEmpty(EmailInput.text);
		}

		public void ValidateUserOrEmail(string emailToCheck) {
			if (string.IsNullOrEmpty(emailToCheck)) {
				if (next != null && string.IsNullOrEmpty(next.EmailInput.text) && next.IsInteractable()) {
					next.SetInteractable(false);
				}
				return;
			}

			if (next != null && !next.IsInteractable()) {
				next.SetInteractable(true);

			}

			if (!TacoManager.ValidateEmail(emailToCheck)) {
				ToggleEmailOkay(false);
				RemoveDuplidateError();
				errorMessage.text = TacoConfig.EnterInvalidEmailError;
				return;
			}

			if (reservedEmails.Contains(GetEmail())) {

				ToggleEmailOkay(false);
				RemoveDuplidateError();
				errorMessage.text = TacoConfig.EnterExistingEmailError;
				return;
			}

			Action<string> success = (string data) => {

				GameFeaturedResult r = JsonUtility.FromJson<GameFeaturedResult>(data);

				if (!ValidateDuplidateEmail(emailToCheck)) {
					ToggleEmailOkay(false);
					errorMessage.text = TacoConfig.EnterExistingEmailError;
				}
				else {
					ToggleEmailOkay(true);
				}
				RemoveDuplidateError();
				previousValidEmailValue = emailToCheck;
				// if (r.success) {

				// 	if (data.Contains("true")) {
				// 		if (!ValidateDuplidateEmail(emailToCheck)) {
				// 			ToggleEmailOkay(false);
				// 			errorMessage.text = TacoConfig.EnterExistingEmailError;
				// 		}
				// 		else {
				// 			ToggleEmailOkay(true);
				// 		}
				// 		RemoveDuplidateError();
				// 		previousValidEmailValue = emailToCheck;
				// 	}
				// }
				// else {
				// 	ToggleEmailOkay(false);
				// 	RemoveDuplidateError();

				// 	errorMessage.text = TacoConfig.NotInSystemEmailError;
				// }
			};

			Action<string, string> fail = (string errorData, string error) => {
				Debug.Log("Error on get : " + errorData);
				if (!string.IsNullOrEmpty(error)) {
					Debug.Log("Error : " + error);
				}
				TacoManager.CloseMessage();
				//string msg = errorData + (string.IsNullOrEmpty (error) ? "" : " : " + error);
				TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginErrorEmailPassword);
				//TacoManager.OpenModal (TacoConfig.TacoLoginErrorHeader, TacoConfig.TacoLoginErrorMessage01);
			};


			string url = "api/user/verify?u=" + emailToCheck;
			StartCoroutine(ApiManager.Instance.GetWithToken(url, success, fail));

		}

		public void awake() {

			EmailInput.GetComponent<InputField>().ActivateInputField();

		}
	}
}
