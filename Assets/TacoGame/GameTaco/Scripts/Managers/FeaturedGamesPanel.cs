using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace GameTaco
{
	public class FeaturedGamesPanel : MonoBehaviour {
		public GameObject featuredGamePreFab;//need remove?
		public GameObject ourGamePreFab;
		public Transform gamesList;
		private List<Toggle> buttons = new List<Toggle>();
		private List<OurGame> ourGamesList = new List<OurGame> ();
		private RectTransform gameListRect;
		//public ToggleGroup gamesToggleGroup;
		private float width;
		private int gamePerRow;
		private float childWidth;
		private float childHeight;
		private string platform;

		private void Awake()
		{
			gameListRect = gamesList.gameObject.GetComponent<RectTransform>();
			width = 1920; //graphic resolution //gameObject.GetComponent<RectTransform> ().rect.width;
			gamePerRow = 2;
			childWidth = width / gamePerRow;
			childHeight = childWidth * 0.75f;
			GetToggleButtons();
			GetPlatform();
		}

		private void GetPlatform()
		{
			switch (Application.platform)
			{
				case RuntimePlatform.Android:
					platform = "isAndroid";
					break;
				case RuntimePlatform.IPhonePlayer:
					platform = "isMobile";
					break;
				case RuntimePlatform.OSXPlayer:
					platform = "isMac";
					break;
				case RuntimePlatform.WindowsPlayer:
					platform = "isWindows86";//"isWindows64";//"isWindows86"
					break;
				default:
					platform = "isWindows86";
					break;
			}
		}

		private void GetToggleButtons(){
			Transform buttonsTransfrom = transform.Find ("Toggles").transform;
			List<Text> labels = new List<Text> ();
			foreach (Transform child in buttonsTransfrom) {
				labels.Add (child.Find("Label").GetComponent<Text> ());
			}
			for (int i = 0; i < buttonsTransfrom.childCount; i++) {
				Toggle button = buttonsTransfrom.GetChild(i).GetComponent<Toggle> ();
				int pos = i;
				button.onValueChanged.AddListener ((value) => {
					if(!value) return;
					FilterGames (button.name);
					for (int n = 0; n < labels.Count; n++) {
						if(n==pos){
							labels[n].color = new Color32(255,120,0,255);
						}
						else {
							labels[n].color = new Color32(50,50,50,255);
						}
					}
				});
				buttons.Add (button);
			}
		}
		/*void Update() {
			if (this.isActiveAndEnabled & !TacoManager.CheckModalsOpen() ) {
				if (Input.GetKeyDown(KeyCode.Return) ) {
					TacoManager.ShowPanel (PanelNames.MyPublicPanel);
				}
			}
		}*/

		private void FilterGames(string type){
			int i = 0;
			float spacing = 40;
			gameListRect.localPosition = new Vector3 (gameListRect.localPosition.x, 0);
			for (int n = 0; n < ourGamesList.Count; n++) {
				if (ourGamesList [n].type == type || type == "all") {
					ourGamesList [n].gameObject.SetActive (true);
					RectTransform currentGameRect = ourGamesList [n].GetComponent<RectTransform> ();
					currentGameRect.localPosition = new Vector3 (childWidth * (i % gamePerRow) - spacing/ 2, childHeight * (-0.5f - (int)(i / gamePerRow)) - spacing);
					currentGameRect.sizeDelta = new Vector2 (childWidth - spacing, childHeight - spacing);
					i++;
				} else {
					ourGamesList [n].gameObject.SetActive (false);
				}
			}
			gameListRect.sizeDelta = new Vector2 (gameListRect.sizeDelta.x, ((int)(i / gamePerRow) + 1) * childHeight + spacing);
		}

		private void UpdateGames(List<Game> games) {
			float spacing = 40;
			int gameNb = games.Count;
			gameListRect.localPosition = new Vector3 (gameListRect.localPosition.x, 0);
			for (int i = 0; i < gameNb; i++) {
				GameObject currentGame = Instantiate (ourGamePreFab, Vector3.zero, Quaternion.identity, gamesList) as GameObject;
				OurGame ourGame = currentGame.GetComponent<OurGame> ();
				ourGame.type = games [i].type;
				ourGame.UpdateTournamentText (games [i].activeTournaments);
				ourGame.Link = games[i].downloadLink;
				ourGame.UpdateNameTextAndImage (games [i].name);
				RectTransform currentGameRect = currentGame.GetComponent<RectTransform> ();
				currentGameRect.localPosition = new Vector3 (childWidth * (i % gamePerRow) - spacing/2, childHeight * (-0.5f - (int)(i / gamePerRow)) - spacing);
				currentGameRect.sizeDelta = new Vector2 (childWidth - spacing, childHeight - spacing);
				ourGamesList.Add (ourGame);
			}
			gameListRect.sizeDelta = new Vector2 (gameListRect.sizeDelta.x, ((int)(gameNb / gamePerRow) + 1) * childHeight + spacing);
		}

		public void GetOurGames() {
			TacoManager.OpenMessage (TacoConfig.TacoOurGamesLoadingMessage);
			foreach (Transform child in gamesList) {
				Destroy (child.gameObject);
			}
			ourGamesList = new List<OurGame> ();
			buttons [0].isOn = true;//set default to all toggle

			Action<string> success = (string data) => {
				GameFeaturedResult r = JsonUtility.FromJson<GameFeaturedResult> (data);
				if (r.success) {
					TacoManager.CloseMessage();
					UpdateGames (r.games);
				}
			};

			Action<string, string> fail = (string errorData, string error) => {
				Debug.Log ("Error on get : " + errorData);
				if (!string.IsNullOrEmpty (error)) {
					Debug.Log ("Error : " + error);
				}

				TacoManager.CloseMessage();

				TacoManager.OpenModalLoginFailedPanel(TacoConfig.TacoLoginErrorEmailPassword);
			};

			string url = "api/game/featured?userAgent=" + platform;
			StartCoroutine (ApiManager.Instance.GetWithToken (url, success, fail));
		}
	}
}
