using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameTaco {
	public class TournamentCreateInvite : MonoBehaviour {
		public GameObject EmailInputGameObject;
		public InputField EmailInput;
		public GameObject EmailOkay = null;
		public GameObject Invited;
		public GameObject InviteInputField;
		private bool emailValidated = false;

		public string GetEmail() {
			return EmailInput.text;
		}

		public void Start() {
			EmailInput.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
		}

		public void AddInvite(string email, int index) {

			GameObject invitedEntry = Instantiate(InviteInputField, Invited.transform) as GameObject;

			InputField invitedInputField = invitedEntry.GetComponent<InputField>();

			invitedInputField.interactable = false;
			invitedInputField.text = email;
			invitedInputField.textComponent.fontSize = 24;

			TacoButton tacoButton = invitedEntry.GetComponentInChildren<TacoButton>();
			tacoButton.SetCallBackDataInt(index);

		}

		public void ValueChangeCheck() {
			ValidateUserOrEmail(GetEmail());
		}

		public void ValidateUserOrEmail(string emailToCheck) {
			EmailOkay.SetActive(false);

			Action<string> success = (string data) => {
				GameFeaturedResult r = JsonUtility.FromJson<GameFeaturedResult>(data);
				EmailOkay.SetActive(true);
				// if (r.success) {
				// 	if (data.Contains("true")) {
				// 		EmailOkay.SetActive(true);
				// 	}
				// };
			};

			Action<string, string> fail = (string errorData, string error) => {
				Debug.Log("Error on get : " + errorData);
				if (!string.IsNullOrEmpty(error)) {
					Debug.Log("Error : " + error);
				}

				TacoManager.CloseMessage();
				TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginErrorEmailPassword);
			};

			string url = "api/user/verify?u=" + emailToCheck;
			StartCoroutine(ApiManager.Instance.GetWithToken(url, success, fail));

		}

		public void Awake() {
			InputField emailInputField = EmailInput.GetComponent<InputField>();
			emailInputField.ActivateInputField();
			EventSystem.current.SetSelectedGameObject(EmailInputGameObject, null);

		}
	}
}
