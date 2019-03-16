using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if UNITY_IOS || UNITY_ANDROID
//using Facebook.Unity;
#endif
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

namespace GameTaco {
	public class LoginPanel : MonoBehaviour {
		public static LoginPanel Instance;

		public GameObject EmailInput = null;
		public GameObject PasswordInput = null;
		//public GameObject TacoStatusText = null;

		public GameObject AutoLoginToggle = null;
		public GameObject FacebookBtn = null;
		public GameObject GoogleBtn = null;
		public InputField resetAccount;
		public InputField ReferenceCodeInput;
		public Button submitResetBtn;
		public GameObject CodeDisplayPanel;
		public Button copyCode;
		public Button goToGooglePage;
		private GoogleDeviceInfo deviceInfo;

		// client configuration
		const string clientID = "701274653877-p7gh14u6k37hqrob520oh172fq4856m9.apps.googleusercontent.com";
		const string clientSecret = "fsE-_7GJN-XmdLkszyikuWMO";
		//const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
		const string authorizationDeviceEndpoint = "https://accounts.google.com/o/oauth2/device/code";
		const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
		const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
		string state = randomDataBase64url(32);

		void Awake() {
			Instance = this;
#if UNITY_IOS_ || UNITY_ANDROID_
			if (!FB.IsInitialized)
			{
				// Initialize the Facebook SDK
				//FB.Init(InitCallback, OnHideUnity);
			}
			else
			{
				// Already initialized, signal an app activation App Event
				//FB.ActivateApp();
			}

#endif
			Init();

			copyCode.onClick.AddListener(() => {
				CodeDisplayPanel.transform.Find("Main/Code").GetComponent<InputField>().text = deviceInfo.user_code;
			});

			goToGooglePage.onClick.AddListener(() => {
				TacoManager.OpenMessage(TacoConfig.Processing);
				CodeDisplayPanel.SetActive(false);
				Application.OpenURL(deviceInfo.verification_url);
				StartCoroutine(PollRequestToGoogle(deviceInfo));
			});

			submitResetBtn.interactable = false;
			resetAccount.onValueChanged.AddListener((string value) => {
				submitResetBtn.interactable = !string.IsNullOrEmpty(value);
			});

		}

		// Use this for initialization
		public void Init() {
#if UNITY_IOS || UNITY_ANDROID
			//FacebookBtn.SetActive(false);
			//GoogleBtn.SetActive(false);
#else
			//FacebookBtn.SetActive (false);
			//var googleBtnRect = GoogleBtn.GetComponent (typeof(RectTransform)) as RectTransform;
			//var facebookBtnRect = FacebookBtn.GetComponent (typeof(RectTransform)) as RectTransform;
			//googleBtnRect.sizeDelta = new Vector2 (772, 93);

			//googleBtnRect.anchoredPosition3D = new Vector3 (400f, -660f, 0);
#endif


			//TacoStatusText.GetComponent<Text> ().text = TacoConfig.TacoLoginMessage;

			PasswordInput.SetActive(true);
			EmailInput.SetActive(true);

			PasswordInput.GetComponent<InputField>().placeholder.GetComponent<Text>().text = TacoConfig.TacoLoginPassword;
			EmailInput.GetComponent<InputField>().placeholder.GetComponent<Text>().text = TacoConfig.TacoLoginEmail;

			PasswordInput.GetComponent<InputField>().text = string.Empty;
			EmailInput.GetComponent<InputField>().text = string.Empty;
		}
#if UNITY_IOS || UNITY_ANDROID
		/*private void InitCallback() {
			if (FB.IsInitialized) {
				// Signal an app activation App Event
				FB.ActivateApp();
				// Continue with Facebook SDK
				// ...
			}
			else {
				Debug.Log("Failed to Initialize the Facebook SDK");
			}
		}*/

		private void OnHideUnity(bool isGameShown) {
			if (!isGameShown) {
				// Pause the game - we will need to hide
				Time.timeScale = 0;
			}
			else {
				// Resume the game - we're getting focus again
				Time.timeScale = 1;
			}
		}
#endif
		// Update is called once per frame
		void Update() {
			if (this.isActiveAndEnabled & !TacoManager.CheckModalsOpen()) {
				if (Input.GetKeyDown(KeyCode.Tab)) {
					if (EmailInput.GetComponent<InputField>().isFocused) {
						PasswordInput.GetComponent<InputField>().ActivateInputField();
					}

					if (PasswordInput.GetComponent<InputField>().isFocused) {
						EmailInput.GetComponent<InputField>().ActivateInputField();
					}
					else if (!EmailInput.GetComponent<InputField>().isFocused && !PasswordInput.GetComponent<InputField>().isFocused) {
						EmailInput.GetComponent<InputField>().ActivateInputField();
					}
				}
				else if (Input.GetKeyDown(KeyCode.Return)) {
					Login();
				}
			}
		}

		public void SetCredential(string email, string password) {
			EmailInput.GetComponent<InputField>().text = email;
			PasswordInput.GetComponent<InputField>().text = password;
		}

		public void Reset() {
			TacoManager.OpenMessage(TacoConfig.Processing);
			Action<string> success = (string data) => {
				TacoManager.CloseMessage();
				GeneralResult r = JsonUtility.FromJson<GeneralResult>(data);
				string notice = r.success ? r.msg : r.err;
				string header = r.success ? TacoConfig.EmailSentHeader : TacoConfig.ErrorHeader;
				TacoManager.OpenModalGeneralResultPanel(r.success, header, notice);
			};
			Action<string, string> fail = (string data, string error) => {
				TacoManager.CloseMessage();
				GeneralResult r = JsonUtility.FromJson<GeneralResult>(data);
				TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.ErrorHeader, r.err);
			};
			StartCoroutine(ApiManager.Instance.ResetPassword(resetAccount.text, success, fail));
			resetAccount.text = string.Empty;
		}

		public void Login() {
			var email = EmailInput.GetComponent<InputField>().text;
			var password = PasswordInput.GetComponent<InputField>().text;
			// toggle returns a bool
			bool autoLoginBool = AutoLoginToggle.GetComponent<Toggle>().isOn;

			// Unity doesn't allow Bool as a preference type, using int
			int autoLoginInt = 0;

			if (autoLoginBool) {
				autoLoginInt = 1;
			}

			TacoManager.OpenMessage(TacoConfig.TacoLoginStatusMessage00);

			//TODO: Verify valid email syntax
			if (email == string.Empty || password == string.Empty) {
				TacoManager.CloseMessage();
				TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginErrorMessage00);
			}
			else {
				Action<string> success = (string data) => {
					LoginResult r = JsonUtility.FromJson<LoginResult>(data);
					if (r.success) {
						Debug.Log("login login_count " + r.login_count);
						BalanceManager.Instance.SetRemainingValue(r.remainingClaim, r.login_count);
						TacoManager.SetPreference(UserPreferences.autoLogin, autoLoginInt);
						TacoManager.SetPreferenceString(UserPreferences.userToken, r.token);

						TacoManager.OpenMessage(TacoConfig.TacoLoginStatusMessage01);
						TacoManager.CreateUser(r);

						if (r.msg == "in") {
							if (r.free == "free") {
								//Debug.LogError("No longer implemented");
								//TacoManager.OpenModal("Login success", "Welcome back to game taco! You get " + r.score_tokens + " taco token from free play!");
							}
							else {
								if (r.free == "enough") {
									Debug.LogError("No longer implemented");
									//TacoManager.OpenModal("Login success", "Welcome back to game taco! You've got too many taco tokens today! Please try again tomorrow.");
								}
								else {
									TacoManager.OpenHowToPlayPanel();
								}
							}
						}
						else {
							if (r.free == "free") {
								Debug.LogError("No longer implemented");
								//TacoManager.OpenModal("Login success", "You get " + r.value + " taco token for login today! And " + r.score_tokens + " taco token from free play!");
							}
							else {
								if (!TacoManager.GetPanel("SuccessRegister").activeSelf) TacoManager.OpenModalDailyTokenPanel(r.value);
								else TacoManager.OpenModalRegisterPanel(TacoManager.User.name);
							}

						}
						//ApiManager.Instance.OpenConnection();
						//ApiManager.Instance.SendData(r.userName + string.Empty);
						Init();

					}
					else {
						TacoManager.CloseMessage();
						TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginErrorEmailPassword);
					}
				};

				Action<string, string> fail = (string data, string error) => {
					if (!string.IsNullOrEmpty(data)) {
						SystemError r = JsonUtility.FromJson<SystemError>(data);
						if (r.verErr) {
							//version error
							TacoManager.CloseMessage();
							TacoManager.OpenModalIncorrectVersionPanel(r.message);
						}
						else {
							TacoManager.CloseMessage();
							string msg = r.message;
							if (string.IsNullOrEmpty(msg)) {
								msg = TacoConfig.TacoLoginErrorEmailPassword;
							}
							TacoManager.OpenModalLoginFailedPanel(msg);
						}
					}
					else {
						TacoManager.CloseMessage();
						TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginErrorEmailPassword);
					}

					if (!string.IsNullOrEmpty(error)) {
						Debug.Log("Error : " + error);
					}
				};

				StartCoroutine(ApiManager.Instance.Login(email, password, success, fail));
			}
		}

		public void LoginWithFacebook() {
			TacoManager.OpenMessage(TacoConfig.TacoLoginStatusMessage00);
			if (Application.internetReachability == NetworkReachability.NotReachable) {
				TacoManager.CloseMessage();
				TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginStatusNoInternet);
				//TacoManager.OpenModal (TacoConfig.TacoLoginErrorHeader, "Please connect internet before login.");
				return;
			}
#if UNITY_IOS || UNITY_ANDROID
			//FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, AuthCallback);
#endif
		}
#if UNITY_IOS || UNITY_ANDROID
		/*private void AuthCallback(ILoginResult result) {
			if (FB.IsLoggedIn) {
				FB.API("/me?fields=name,email,gender", HttpMethod.GET, FetchProfileCallback, new Dictionary<string, string>() { });
			}
			else {
				TacoManager.CloseMessage();
				Debug.Log("User cancelled login");
			}
		}

		private void FetchProfileCallback(IGraphResult result) {

			Debug.Log(result.RawResult);

			var FBUserDetails = (Dictionary<string, object>)result.ResultDictionary;

			string email = FBUserDetails.ContainsKey("email") ? FBUserDetails["email"].ToString() : string.Empty;
			string nameUser = FBUserDetails.ContainsKey("name") ? FBUserDetails["name"].ToString() : string.Empty;
			string gender = FBUserDetails.ContainsKey("gender") ? FBUserDetails["gender"].ToString() : string.Empty;

			var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;

			bool autoLoginBool = AutoLoginToggle.GetComponent<Toggle>().isOn;

			// API wants a string for the form post
			string autoLoginString = autoLoginBool.ToString();

			// Unity doesn't allow Bool as a preference type, using int
			int autoLoginInt = 0;

			if (autoLoginBool == true) {
				autoLoginInt = 1;
			}

			Action<string> success = (string data) => {
				LoginResult r = JsonUtility.FromJson<LoginResult>(data);
				if (r.success) {
					TacoManager.SetPreference(UserPreferences.autoLogin, autoLoginInt);
					TacoManager.SetPreferenceString(UserPreferences.userToken, r.token);

					TacoManager.OpenMessage(TacoConfig.TacoLoginStatusMessage01);
					TacoManager.CreateUser(r);

					if (r.msg == "in") {
						if (r.free == "free") {
							//TacoManager.OpenModal("Login success", "Welcome back to game taco! You get " + r.score_tokens + " taco token from free play!");
						}
						else {
							if (r.free == "enough") {
								//TacoManager.OpenModal("Login success", "Welcome back to game taco! You've got too many taco tokens today! Please try again tomorrow.");
							}
							else {
								//TacoManager.OpenModal("Login success", "Welcome back to game taco! ");
							}

						}
					}

					else {
						if (r.msg == "anotherSocial") {
							if (r.free == "free") {
								//TacoManager.OpenModal("Login success", "You get " + "6" + " taco token for login today! And " + r.score_tokens + " taco token from free play!");
							}
							else {
								//TacoManager.OpenModal("Login success", "You get " + "6" + " taco token for SignUp and Login today! ");
							}

						}
						else {
							if (r.free == "free") {
								//TacoManager.OpenModal("Login success", "You get " + r.value + " taco token for login today! And " + r.score_tokens + " taco token from free play!");
							}
							else {
								//TacoManager.OpenModal("Login success", "You get " + r.value + " taco token for login today! ");
							}
						}
					}
					// clean up the login panel
					Init();

				}
				else {
					TacoManager.CloseMessage();
					TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginErrorEmailPassword);
				}
			};

			Action<string, string> fail = (string data, string error) => {
				if (!string.IsNullOrEmpty(data)) {
					SystemError r = JsonUtility.FromJson<SystemError>(data);
					if (r.verErr == true) {
						//version error
						TacoManager.CloseMessage();
						TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.TacoVersionErrorHeader, r.message);
					}
					else {
						TacoManager.CloseMessage();
						TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginErrorEmailPassword);
					}
				}
				else {

					TacoManager.CloseMessage();
					TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginErrorEmailPassword);
				}
			};

			StartCoroutine(ApiManager.Instance.LoginFacebook(aToken.TokenString, aToken.UserId, email, gender, nameUser, success, fail));
		}*/

#endif
		public static int GetRandomUnusedPort() {
			int _port = 9000;
			while (true) {
				try {
					var listener = new TcpListener(IPAddress.Loopback, _port);
					listener.Start();
					listener.Stop();
					return _port;
				}
				catch (SocketException) {
					_port++;
				}
			}
			return 0;
			//below code will not work with experimental net 4.6 and il2cpp
			/*var listener = new TcpListener (IPAddress.Loopback, 0);
			listener.Start ();
			var port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop ();
			return port;*/
		}

		public void LoginWithGoogle() {
			TacoManager.LoginPanelObject.SetActive(true);//if from register panel
			TacoManager.OpenMessage(TacoConfig.TacoLoginStatusMessage00);
			Debug.Log("internet: " + Application.internetReachability);
			if (Application.internetReachability == NetworkReachability.NotReachable) {
				TacoManager.CloseMessage();
				TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginStatusNoInternet);
				return;
			}

			Action<string> onSuccess = (string data) => {
				TacoManager.CloseMessage();
				GoogleDeviceInfo r = JsonUtility.FromJson<GoogleDeviceInfo>(data);
				CodeDisplayPanel.SetActive(true);
				CodeDisplayPanel.transform.Find("Main/Code").GetComponent<InputField>().text = r.user_code;
				deviceInfo = r;
				//testing
				TextEditor te = new TextEditor();
				te.text = deviceInfo.user_code;
				te.SelectAll();
				te.Copy();
			};

			Action<string, string> onFail = (string data, string error) => {
				TacoManager.CloseMessage();
				TacoManager.OpenModalLoginFailedPanel(error);
			};
			StartCoroutine(ApiManager.Instance.RequestDeviceAdnUserCode(authorizationDeviceEndpoint, clientID, onSuccess, onFail));
		}

		private IEnumerator PollRequestToGoogle(GoogleDeviceInfo deviceData) {
			bool polling = true;
			int nbOfPoll = 9;
			while (polling) {
				Action<string> onSuccess = (string accessData) => {
					Debug.Log("access data: " + accessData);
					AccessData result = JsonUtility.FromJson<AccessData>(accessData);
					if (!string.IsNullOrEmpty(result.access_token)) {
						polling = false;

						Debug.Log("login google");
						Action<string> successUserInfo = (string userInfo) => {
							TacoManager.CloseMessage();

							Debug.Log(userInfo);
							LoginResult r = JsonUtility.FromJson<LoginResult>(userInfo);
							if (r.success) {
								TacoManager.OpenMessage(TacoConfig.TacoLoginStatusMessage01);
								TacoManager.CreateUser(r);
								BalanceManager.Instance.SetRemainingValue(r.remainingClaim, r.login_count);
								if (r.msg == "in") {
									if (r.free == "free") {
										//TacoManager.OpenModal("Login success", "Welcome back to game taco! You get " + r.score_tokens + " taco token from free play!");
									}
									else {
										if (r.free == "enough") {
											//TacoManager.OpenModal("Login success", "Welcome back to game taco! You've got too many taco tokens today! Please try again tomorrow.");
										}
										else {
											TacoManager.OpenHowToPlayPanel();
										}
									}
								}
								else {
									if (r.msg == "anotherSocial") {
										if (r.free == "free") {
											//TacoManager.OpenModal("Login success", "You get " + "6" + " taco token for login today! And " + r.score_tokens + " taco token from free play!");
										}
										else {
											TacoManager.OpenModalRegisterPanel(TacoManager.User.name);
										}
									}
									else {
										if (r.free == "free") {
											//TacoManager.OpenModal("Login success", "You get " + r.value + " taco token for login today! And " + r.score_tokens + " taco token from free play!");
										}
										else {
											if (!TacoManager.GetPanel("SuccessRegister").activeSelf) TacoManager.OpenModalDailyTokenPanel(r.value);
										}
									}
								}

								// clean up the login panel
								Init();

							}
							else {
								TacoManager.CloseMessage();
								TacoManager.OpenModalLoginFailedPanel(TacoConfig.Error);
							}

						};
						Action<string, string> failUserInfo = (string userInfo, string error) => {
							//TacoManager.CloseMessage();
							//TacoManager.OpenModalLoginFailedPanel(error);
							Debug.Log(userInfo);
							if (!string.IsNullOrEmpty(userInfo)) {
								SystemError r = JsonUtility.FromJson<SystemError>(userInfo);
								if (r.verErr) {
									//version error
									TacoManager.CloseMessage();
									TacoManager.OpenModalIncorrectVersionPanel(r.message);
								}
								else {
									TacoManager.CloseMessage();
									string msg = r.message;
									if (string.IsNullOrEmpty(msg)) {
										msg = TacoConfig.TacoLoginErrorEmailPassword;
									}
									TacoManager.OpenModalLoginFailedPanel(msg);
								}
							}
							else {
								TacoManager.CloseMessage();
								TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginErrorEmailPassword);
							}
						};
						StartCoroutine(ApiManager.Instance.LoginGoogle(result.access_token, result.id_token, ReferenceCodeInput.text, successUserInfo, failUserInfo));
					}
					else if (nbOfPoll < 0) {
						polling = false;
						TacoManager.CloseMessage();
						TacoManager.OpenModalLoginFailedPanel(TacoConfig.Error);
					}
					nbOfPoll--;
				};
				Action<string, string> onFail = (string data, string error) => {
					nbOfPoll--;
          if (nbOfPoll < 0) {
            polling = false;
            TacoManager.CloseMessage();
            TacoManager.OpenModalLoginFailedPanel(error);
          }
				};
				StartCoroutine(ApiManager.Instance.PollGoogleAuthorization("https://www.googleapis.com/oauth2/v4/token", clientID, clientSecret, deviceData.device_code, "http://oauth.net/grant_type/device/1.0", deviceData.interval, onSuccess, onFail));
				yield return new WaitForSeconds(deviceData.interval);
			}
		}

		public void ListenerCallback(IAsyncResult result) {
			HttpListener listener = (HttpListener)result.AsyncState;
			// Call EndGetContext to complete the asynchronous operation.
			HttpListenerContext context = listener.EndGetContext(result);
			// Obtain a response object.
			HttpListenerResponse response = context.Response;
			// Construct a response.
			string responseString = "<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body style='font-size:50px;text-align: center'>Please return to the app.</body></html>";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
			// Get a response stream and write the response to it.
			response.ContentLength64 = buffer.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(buffer, 0, buffer.Length);
			// You must close the output stream.
			output.Close();

		}


		public void ListenerResponse(IAsyncResult result) {

		}

		/// <summary>
		/// Returns URI-safe data with a given input length.
		/// </summary>
		/// <param name="length">Input length (nb. output will be longer)</param>
		/// <returns></returns>
		public static string randomDataBase64url(uint length) {
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			byte[] bytes = new byte[length];
			rng.GetBytes(bytes);
			return base64urlencodeNoPadding(bytes);
		}

		/// <summary>
		/// Returns the SHA256 hash of the input string.
		/// </summary>
		/// <param name="inputStirng"></param>
		/// <returns></returns>
		public static byte[] sha256(string inputStirng) {
			byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
			SHA256Managed sha256 = new SHA256Managed();
			return sha256.ComputeHash(bytes);
		}

		/// <summary>
		/// Base64url no-padding encodes the given input buffer.
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public static string base64urlencodeNoPadding(byte[] buffer) {
			string base64 = Convert.ToBase64String(buffer);

			// Converts base64 to base64url.
			base64 = base64.Replace("+", "-");
			base64 = base64.Replace("/", "_");
			// Strips padding.
			base64 = base64.Replace("=", string.Empty);

			return base64;
		}
	}
}
