using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine.Networking;

namespace GameTaco {
	public class ApiManager : MonoBehaviour {
		#region Singleton
		private static ApiManager mInstance;
		public static ApiManager Instance {
			get {
				if (mInstance == null) {
					mInstance = new GameObject().AddComponent<ApiManager>();
				}
				return mInstance;
			}
		}

		static TcpClient client;
		#endregion

		private void AddValidationHeader(UnityWebRequest www) {
			www.SetRequestHeader("x-version", TacoSetup.Instance.GetVersion());
			www.SetRequestHeader("x-game-name", TacoSetup.Instance.GetGameName());
string platform = string.Empty;
string source = "DESKTOP";
#if UNITY_STANDALONE_OSX
      platform = "MACOS";
#elif UNITY_STANDALONE_WIN
			platform = "WINDOWS";
#elif UNITY_WSA_10_0
			platform = "WINDOWS";
#elif UNITY_STANDALONE_LINUX
			platform = "LINUX";
#elif UNITY_PS4
			platform = "PLAYSTATION";
#elif UNITY_XBOXONE
			platform = "XBOX";
#elif UNITY_IOS
			platform = "IOS";
			source = "MOBILE";
#elif UNITY_ANDROID
			platform = "ANDROID";
			source = "MOBILE";
#else
			platform = "OTHER";
#endif
			www.SetRequestHeader("x-game-source", source);
			www.SetRequestHeader("x-game-platform", platform);
		}
#region Generic Api Calls
		public IEnumerator GetWithToken(string url, Action<string> onSuccess, Action<string, string> onFail = null) {
			UnityWebRequest www = UnityWebRequest.Get(Constants.BaseUrl + url);
			AddValidationHeader(www);
			string currentToken = String.Empty;

			if (TacoManager.User == null) {
				currentToken = TacoManager.GetPreferenceString(UserPreferences.userToken);
			// 	if (currentToken == "")
			// currentToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6MjAzOSwiaWF0IjoxNTQ2MjI2NTg0fQ.xB07RkGRA7b2_ht0axww2kmHIYG_5lGOEOx87J8LX_U";
			// 	Debug.Log ("User Null : " + currentToken);
			}
			else {
				currentToken = TacoManager.User.token;
				Debug.Log ("User : " + currentToken);
			}

			www.SetRequestHeader("x-access-token", currentToken);

			yield return www.Send();

			Debug.Log(Constants.BaseUrl + url);
			// Debug.Log("www = " + www.downloadHandler.text);

			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("www.isError =" + www.error);

				if (onFail != null) {
					onFail(www.downloadHandler.text, www.error);
				}
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public void GetSession(string token) {
			TacoManager.OpenMessage(TacoConfig.TacoLoginStatusMessage00);

			Action<string> success = (string data) => {
				SessionResult r = JsonUtility.FromJson<SessionResult>(data);
				if (r.success) {
					Debug.Log("session: " + data);
					BalanceManager.Instance.SetRemainingValue(r.remainingClaim, r.login_count);
					TacoManager.OpenMessage(TacoConfig.TacoLoginStatusMessage01);
					TacoManager.UpdateUser(r, token);
					if (r.msg == "in") {
						//Debug.LogWarning("No longer implemented");
					}
					else {
						TacoManager.OpenModalDailyTokenPanel(r.value);
					}
				}
				else {
					TacoManager.CloseMessage();
					TacoManager.SetPreferenceString(UserPreferences.userToken, null);
					TacoManager.SetPreference(UserPreferences.autoLogin, 0);
					TacoManager.ShowPanel(PanelNames.LoginPanel);

					if (!string.IsNullOrEmpty(r.message)) {
						TacoManager.OpenModalLoginFailedPanel(r.message);
					}
				}
			};

			Action<string, string> fail = (string data, string error) => {
				if (!string.IsNullOrEmpty(data)) {
					try {
						SystemError r = JsonUtility.FromJson<SystemError>(data);
						if (r.verErr) {
							//version error
							TacoManager.OpenModalIncorrectVersionPanel(r.message);
						}
						else {
							string msg = r.message;
							if (string.IsNullOrEmpty(msg)) {
								msg = TacoConfig.TacoLoginErrorEmailPassword;
							}
							TacoManager.OpenModalLoginFailedPanel(msg);
						}
					}
					catch (Exception) {
						TacoManager.OpenModalLoginFailedPanel(TacoConfig.Error);
					}
				}
				else {
					TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginErrorEmailPassword);
				}
				TacoManager.CloseMessage();
			};
			string url = "api/session/detail/" + TacoSetup.Instance.SiteId;
			StartCoroutine(ApiManager.Instance.GetWithToken(url, success, fail));
		}


#endregion

		public IEnumerator ResetPassword(string account, Action<string> onSuccess, Action<string, string> onFail) {
			WWWForm form = new WWWForm();
			form.AddField("email", account);
			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "forgot", form);
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);

				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator ChangePassword(string currentPassword, string newPassword, string confirmPassword, Action<string> onSuccess, Action<string, string> onFail) {
			WWWForm form = new WWWForm();
			form.AddField("currentPassword", currentPassword);
			form.AddField("password", newPassword);
			form.AddField("confirmPassword", confirmPassword);
			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/password", form);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}
		public IEnumerator ChangeAddress(string address, string address2, string city, string state, string zipcode, Action<string> onSuccess, Action<string, string> onFail) {
			WWWForm form = new WWWForm();
			form.AddField("address", address);
			form.AddField("address2", address2);
			form.AddField("city", city);
			form.AddField("state", state);
			form.AddField("zipcode", zipcode);
			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/updateContactInfo", form);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator SubmitRedeemInfo(Prize purchasingPrize, string email, string dateOfBirth, string street, string building, string country, string state, string postcode, string city, Action<string> onSuccess, Action<string, string> onFail) {
			WWWForm form = new WWWForm();
			form.AddField("email", email);
			form.AddField("date", dateOfBirth);
			form.AddField("street", street);
			form.AddField("state", state);
			form.AddField("details", building);
			form.AddField("country", country);
			form.AddField("postalcode", postcode);
			form.AddField("city", city);
			form.AddField("id", purchasingPrize.id);
			form.AddField("name", purchasingPrize.name);
			form.AddField("ticket", purchasingPrize.ticket);
			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/redeemPrize", form);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator GetTransactions(int limit, Action<string> onSuccess, Action<string, string> onFail) {

			UnityWebRequest www = UnityWebRequest.Get(Constants.BaseUrl + "api/account/transactions/history?limit=" + limit + "&offset=0");
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			//www.SetRequestHeader("x-version", GameTaco.TacoSetup.Instance.GetVersion());
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);

				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator GetUserBalance(Action<string> onSuccess, Action<string, string> onFail) {

			UnityWebRequest www = UnityWebRequest.Get(Constants.BaseUrl + "api/user/getFundsAndGTokens");
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			//www.SetRequestHeader("x-version", GameTaco.TacoSetup.Instance.GetVersion());
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);

				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator GetPrizes(Action<string> onSuccess, Action<string, string> onFail) {

			UnityWebRequest www = UnityWebRequest.Get(Constants.BaseUrl + "prizeList");
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			//www.SetRequestHeader("x-version", GameTaco.TacoSetup.Instance.GetVersion());
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);

				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator BuyToken(string typeCurrency, string value, Action<string> onSuccess, Action<string, string> onFail) {

			WWWForm form = new WWWForm();
			form.AddField("currency", typeCurrency);
			form.AddField("amount", value);
			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/addtokens", form);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator Withdraw(string amount, string address, string address2, string city, string state, string zipcode, bool isSave, Action<string> onSuccess, Action<string, string> onFail) {
			WWWForm form = new WWWForm();
			form.AddField("amount", amount);
			form.AddField("address", address);
			form.AddField("address2", address2);
			form.AddField("city", city);
			form.AddField("state", state);
			form.AddField("zip", zipcode);
			form.AddField("saveNewContactInfo", isSave.ToString());
			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/funds/withdraw", form);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);

				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator WWWPrizeImage(string url, Image imageToReplace) {
			using (WWW www = new WWW(url)) {
				yield return www;

				if (imageToReplace != null && www.texture != null) {
					imageToReplace.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
				}
			}
		}

#region Authentication
		public IEnumerator Login(string email, string password, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();

			form.AddField("email", email);
			form.AddField("password", password);
			form.AddField("siteId", TacoSetup.Instance.SiteId);
			form.AddField("gtoken", string.Empty);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/login", form);
			AddValidationHeader(www);
			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator LoginFacebook(string accesstoken, string facebookId, string email, string gender, string name, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();

			form.AddField("accesstoken", accesstoken);
			form.AddField("facebookId", facebookId);
			form.AddField("email", email);
			form.AddField("gender", gender);
			form.AddField("name", name);
			form.AddField("siteId", TacoSetup.Instance.SiteId);
			form.AddField("gtoken", string.Empty);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/user/loginfb", form);
			AddValidationHeader(www);
			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator LoginGoogle(string accessToken, string idToken, string refCode, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();

			form.AddField("accessToken", accessToken);
			form.AddField("id_token", idToken);
			form.AddField("siteId", TacoSetup.Instance.SiteId);
			form.AddField("referCode", refCode);
			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/user/logingooglemobile", form);
			AddValidationHeader(www);
			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				Debug.Log("login www.responseCode = " + www.responseCode);

				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator RequestDeviceAdnUserCode(string url, string clientId, Action<string> onSuccess, Action<string, string> onFail) {
			WWWForm form = new WWWForm();
			form.AddField("client_id", clientId);
			form.AddField("scope", "profile email");
			UnityWebRequest www = UnityWebRequest.Post(url, form);
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator PollGoogleAuthorization(string url, string clientId, string clientSecret, string deviceCode, string grantType, float inteval, Action<string> onSuccess, Action<string, string> onFail) {
			WWWForm form = new WWWForm();
			form.AddField("client_id", clientId);
			form.AddField("client_secret", clientSecret);
			form.AddField("code", deviceCode);
			form.AddField("grant_type", grantType);
			UnityWebRequest www = UnityWebRequest.Post(url, form);
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator VerifyAuthenCodeGoogle(string url, string code, string redirectUri, string clientID, string codeVerifier, string clientSecret, Action<string> onSuccess) {
			WWWForm form = new WWWForm();

			form.AddField("code", code);
			form.AddField("redirect_uri", redirectUri);
			form.AddField("client_id", clientID);
			form.AddField("code_verifier", codeVerifier);
			form.AddField("client_secret", clientSecret);
			form.AddField("scope", string.Empty);
			form.AddField("grant_type", "authorization_code");

			UnityWebRequest www = UnityWebRequest.Post(url, form);

			yield return www.Send();
			Debug.Log(www.error);
			onSuccess(www.downloadHandler.text);
		}


		public IEnumerator GetInfoUserGoogle(string url, string accesstoken, Action<string> onSuccess) {
			UnityWebRequest www = UnityWebRequest.Get(url);
			www.SetRequestHeader("Authorization", "Bearer " + accesstoken);
			yield return www.Send();

			onSuccess(www.downloadHandler.text);
		}

		public IEnumerator Register(string userName, string email, string password, bool ageCheck, string refCode, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();

			form.AddField("userName", userName);
			form.AddField("email", email.ToLower());
			form.AddField("password", password);
			form.AddField("confirmPassword", password);
			form.AddField("ageCheck", ageCheck.ToString().ToLower());
			form.AddField("siteId", TacoSetup.Instance.SiteId);
			form.AddField("gtoken", string.Empty);
			form.AddField("referCode", refCode);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/register", form);
			AddValidationHeader(www);
			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}
#endregion

#region Funds
		public IEnumerator AddFunds(string number, string month, string year, string cvc, int amount, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();

			form.AddField("number", number);
			form.AddField("month", month);
			form.AddField("year", year);
			form.AddField("cvc", cvc);
			form.AddField("amount", amount);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "/api/funds", form);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);

			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				// Check for HTTP failure
				if (onFail != null) {
					onFail(www.downloadHandler.text, www.error);
				}
				Debug.Log("Add Funds fails - " + www.downloadHandler.text);
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}


		public IEnumerator AddGTokens(int fee, int gTokens, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();

			form.AddField("fee", fee);
			form.AddField("amount", gTokens.ToString());

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/tokens", form);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		//NOTE: Add funds first calls stripe to get the proper token, this calls our backend to actually charge the user
		private IEnumerator chargeUser(string token, int amount, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();
			form.AddField("token", token);
			form.AddField("amount", amount);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/funds/stripe", form);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator WithdrawFunds(string amount, string name, string address1, string address2, string city, string zip, string state, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();
			form.AddField("amount", amount);
			form.AddField("name", name);
			form.AddField("address1", address1);
			form.AddField("address2", address2);
			form.AddField("city", city);
			form.AddField("state", state);
			form.AddField("zip", zip);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/funds/withdraw", form);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);

			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}
#endregion

#region Game
		public IEnumerator StartGame(int typeCurrency, int tournamentId, string userToken, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();
			form.AddField("typeCurrency", typeCurrency);
			form.AddField("siteId", TacoSetup.Instance.SiteId);
			form.AddField("tournamentId", tournamentId);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/game/start", form);
			www.SetRequestHeader("x-access-token", userToken);
			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator EndGame(int score, int tournamentId, int gameId, string gameToken, string userToken, Action<string> onSuccess, Action<string, string> onFail = null) {

			WWWForm form = new WWWForm();

			form.AddField("token", string.IsNullOrEmpty(gameToken) ? string.Empty : gameToken);
			form.AddField("tournamentId", tournamentId);
			form.AddField("gameId", gameId);
			form.AddField("score", score);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/game/end", form);
			www.SetRequestHeader("x-access-token", userToken);

			string message = string.IsNullOrEmpty(gameToken) ? TacoConfig.TacoPlayAgainEndedMessage : TacoConfig.TacoPlayEndedMessage;
			TacoManager.OpenMessage(message);

			yield return www.Send();

			if (www.isNetworkError) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator ClaimToken(Action<string> onSuccess, Action<string, string> onFail) {
			UnityWebRequest www = UnityWebRequest.Get(Constants.BaseUrl + "api/claimToken");
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			//www.SetRequestHeader("x-version", GameTaco.TacoSetup.Instance.GetVersion());
			yield return www.Send();
			Debug.Log(www.responseCode + " : " + www.isNetworkError);
			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

#endregion

#region Tournament

		public IEnumerator GetManageTournament(string url, int tourId, Action<string> onSuccess, Action<string, string> onFail) {
			WWWForm form = new WWWForm();
			form.AddField("tourId", tourId);
			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + url, form);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);
			yield return www.Send();
			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator CreateTournament(int timeRemaining, int typeCurrency, int prizeStruc, string invited, string name, string fee, int players, string opponents, int gameId, string userToken, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();
			form.AddField("prize", prizeStruc);
			form.AddField("name", name);
			form.AddField("fee", fee);
			form.AddField("size", players);
			form.AddField("gameId", gameId);
			form.AddField("siteId", TacoSetup.Instance.SiteId);
			form.AddField("opponents", opponents);
			form.AddField("timeRemaining", timeRemaining);
			form.AddField("email", TacoManager.User.email);
			form.AddField("typeCurrency", typeCurrency == 0 ? "real" : "gToken");
			form.AddField("invites", invited);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/tournament", form);
			www.SetRequestHeader("x-access-token", userToken);

			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator ReEnterTournament(int tournamentId, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();
			form.AddField("tourId", tournamentId);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/tournament/reenter", form);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);

			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator JoinTournament(int type, int tournamentId, string siteId, int gameId, string userToken, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();
			form.AddField("type", type);
			form.AddField("tournamentId", tournamentId);
			form.AddField("siteId", siteId);
			form.AddField("gameId", gameId);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/tournament/join", form);
			www.SetRequestHeader("x-access-token", userToken);

			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator RemoveFriend(string email, int friendId, int tournamentId, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();
			string currentToken;

			if (TacoManager.User == null) {
				currentToken = TacoManager.GetPreferenceString(UserPreferences.userToken);
			}
			else {
				currentToken = TacoManager.User.token;
			}
			
			if(friendId != 0)
			{
				form.AddField("friendId", friendId);
			}
			form.AddField("tourId", tournamentId);
			form.AddField("email", email);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/friends/remove2", form);
			www.SetRequestHeader("x-access-token", currentToken);

			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator InviteFriend(string fromEmail, string baseUrl, string emails, int tournamentId, Action<string> onSuccess, Action<string, string> onFail = null) {
			WWWForm form = new WWWForm();

			string currentToken = null;

			if (TacoManager.User == null) {
				currentToken = TacoManager.GetPreferenceString(UserPreferences.userToken);
			}
			else {
				currentToken = TacoManager.User.token;
			}

			form.AddField("fromUserId", TacoManager.User.userId);
			form.AddField("fromEmail", fromEmail);
			form.AddField("baseUrl", baseUrl);
			form.AddField("emails", emails);
			form.AddField("tournamentId", tournamentId);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/friends/invite/tournament", form);
			www.SetRequestHeader("x-access-token", currentToken);

			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

		public IEnumerator InviteFriends(Action<string> onSuccess, Action<string, string> onFail = null) {
			Tournament t = TacoManager.Target;
			WWWForm form = new WWWForm();

			string currentToken = null;

			if (TacoManager.User == null) {
				currentToken = TacoManager.GetPreferenceString(UserPreferences.userToken);
			}
			else {
				currentToken = TacoManager.User.token;
			}

			form.AddField("fromUserId", TacoManager.User.userId);
			form.AddField("fromEmail", TacoManager.User.email);
			form.AddField("baseUrl", "baysidegames.com");
			form.AddField("emails", t.invitedEmails[0]);
			form.AddField("tournamentId", t.id);

			UnityWebRequest www = UnityWebRequest.Post(Constants.BaseUrl + "api/friends/invite/tournament", form);
			www.SetRequestHeader("x-access-token", currentToken);

			yield return www.Send();

			if (www.isNetworkError || www.responseCode == 500) {
				if (onFail != null) { onFail(www.downloadHandler.text, www.error); }
			}
			else {
				onSuccess(www.downloadHandler.text);
			}
		}

#endregion

#region Load Image From WWW
		public IEnumerator WWWImageLoad(string url, Image imageToReplace) {
			string imageURL = Constants.BaseUrl + url;

			using (WWW www = new WWW(imageURL)) {
				yield return www;
				imageToReplace.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
			}
		}

		public IEnumerator WWWAvatarSocial(string url, Image imageToReplace) {
			using (WWW www = new WWW(url)) {
				yield return www;
				if (imageToReplace != null) {
					imageToReplace.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
					imageToReplace.rectTransform.sizeDelta = new Vector2(120, 125);
					imageToReplace.GetComponent<RectTransform>().localPosition = new Vector3(90, 0, 0);
				}
			}
		}

		public IEnumerator WWWAvatarLeaderboard(string url, Image imageToReplace) {
			using (WWW www = new WWW(url)) {
				yield return www;
				if (imageToReplace != null && www.texture != null) {
					imageToReplace.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
					imageToReplace.rectTransform.sizeDelta = new Vector2(100, 100);
					imageToReplace.GetComponent<RectTransform>().localPosition = new Vector3(50, 0, 0);
					TacoManager.cacheAvatars[url] = imageToReplace.sprite;
				}
			}
		}

#endregion

#region Socket Client
		public void OpenConnection() {
			return;//remove now
			if (client != null) {
				Debug.Log("-Connection is already open-");
			}
			else {
				try {
					client = new TcpClient();
					client.Connect("localhost", 8000);
					Debug.Log("-Connected-");
				}
				catch (Exception) {
					client = null;
					Debug.Log("-Connection Error-");
				}
			}
		}
		public void CloseConnection() {
			if (client == null) {
				Debug.Log("-Connection is not open or already close-");
				return;
			}
			try {
				client.Close();
			}
			catch (Exception ex) {
				throw ex;
			}
			finally {

				client = null;
			}
			Debug.Log("-Connection is close successfully-");
		}
		public void SendData(string hello) {
			if (client == null) {
				Debug.Log("-Connection is not open or already close-");
				return;
			}
			NetworkStream nvStream = client.GetStream();
			byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(hello);
			nvStream.Write(bytesToSend, 0, bytesToSend.Length);
			Debug.Log("Send data " + hello);
		}
#endregion
	}
}
