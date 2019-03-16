using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using Endgame;
using UnityEngine.Networking;


namespace GameTaco {
	public class LeaderboardList : BaseListBehavior {
		public static LeaderboardList Instance;

		public GameObject FinishedButtons;
		public GameObject UnfinishedButtons;
		public GameObject endedLeaderboardFooter;
		public GameObject playingLeaderboardFooter;
		public Text tournamentDetailsText0;
		public Text tournamentDetailsText1;
		public Image prizeIcon;
		public Image rankIcon;
		public Text rankText;
		public Text prizePoolText;
		public Text entryFeeText;
		public Text prizeAmount;
		public Text redeemAmount;
		public Text resultStatus;
		public Text resultScore;
		public Text remainingTime;
		public Text tournamentTypeText;
		public Button playAgain;
		public Button startPlay;
		private ImageList imageList;
		private ImageList imageRankList;
		private double countdownTime;
		public List<LeaderboardRow> Items;

		public GameObject ItemLeaderboardButtonPrefab;
		public GameObject TacoLeaderBoardRankPrefab;
		public List<Image> LeaderBoardMoneyTypeImages = new List<Image>();

		void Awake() {
			//playAgain.gameObject.SetActive(false);//currently do not use play again feature
			startPlay.gameObject.SetActive(false);
			//UnfinishedButtons.SetActive(false);
		}

		// Use this for initialization
		protected override void Start() {

			Instance = this;

			ColumnNames = new List<string>() {
				TacoConfig.LeaderboardTournamentCurrentColumn,
				TacoConfig.LeaderboardTournamentRankColumn,
				TacoConfig.LeaderboardTournamentPlayerColumn,
				TacoConfig.LeaderboardTournamentScoreColumn
			};

			// TODO : turn this into percentages
			float adjustedWidth = GetWidth() - 20;

			var column1 = (adjustedWidth * 0.15f);
			var column2 = (adjustedWidth * 0.15f);
			var column3 = (adjustedWidth * 0.4f);
			var column4 = (adjustedWidth * 0.3f);

			ColumnWidths = new int[] { (int)column1, (int)column2, (int)column3, (int)column4 };

			UpdateIconForRank();

			ListView.DefaultItemButtonHeight = TacoConfig.ListViewTournamentsButtonHeight;
			ListView.DefaultColumnHeaderHeight = TacoConfig.ListViewHeaderHeight;

			ListView.DefaultItemFontSize = TacoConfig.ListViewItemFontSize;
			ListView.DefaultHeadingFontSize = TacoConfig.ListViewHeaderFontSize;

			// colors can't be CONST 
			//ListView.DefaultHeadingBackgroundColor = TacoConfig.Instance.TacoListViewHeaderColor;
			ListView.DefaultSelectedItemColor = new Color32(1, 1, 1, 0);// TacoConfig.Instance.ListViewHighlightColor;

			ListView.ItemBecameVisible += OnItemBecameVisible;
			ListView.ItemBecameInvisible += OnItemBecameInvisible;

			GameObject[] leaderboardMoneyImages = GameObject.FindGameObjectsWithTag("LeaderBoardMoneyType");
			for (int i = 0; i < leaderboardMoneyImages.Length; i++) {
				LeaderBoardMoneyTypeImages.Add(leaderboardMoneyImages[i].GetComponent<Image>());
			}

			base.Start();
		}

		// Update is called once per frame
		void Update() {
			if (countdownTime >= 0) {
				TournamentRemainingTime();
			}

			if (isActiveAndEnabled & !TacoManager.CheckModalsOpen()) {
				if (Input.GetKeyDown(KeyCode.Return)) {
					TournamentManager.Instance.ShowTournamentPanel();
				}

			}
		}

		private void ResetUIValue() {
			TacoManager.CreatePublicPanel.SetActive(false);//from create tournament panel can go to here through existing panel
			tournamentDetailsText0.text = string.Empty;
			tournamentDetailsText1.text = string.Empty;
			endedLeaderboardFooter.SetActive(false);
			playingLeaderboardFooter.SetActive(false);
			//FinishedButtons.SetActive (false);
			//UnfinishedButtons.SetActive (false);
			rankIcon.gameObject.SetActive(false);
			rankText.transform.parent.gameObject.SetActive(false);
			resultStatus.text = string.Empty;
			resultScore.text = string.Empty;
			prizeAmount.text = string.Empty;
			remainingTime.text = string.Empty;
			countdownTime = -1;
		}

		private void UpdateUIValue(LeaderboardResult r) {
			Tournament t = r.tournament;

			string tournamentType = t.Type;
			double prizePool = t.PrizePool;
			double entryFee = double.Parse(t.entryFee.ToString());

			LeaderboardRow currentUserRank = r.leaderboard.First(x => x.userId == TacoManager.User.userId);
			string infos = string.Empty;
			string details;
			double moneyValue = 0;
      		int winnerNb = r.winner.Length;

			if(tournamentType == "Private")
			{
				UnfinishedButtons.SetActive(false);
				FinishedButtons.transform.Find("LeaderboardReturnButton").gameObject.SetActive(false);
				FinishedButtons.transform.Find("LeaderboardReturnButtonPrivate").gameObject.SetActive(true);
			}
			else
			{
				UnfinishedButtons.SetActive(true);
				FinishedButtons.transform.Find("LeaderboardReturnButton").gameObject.SetActive(true);
				FinishedButtons.transform.Find("LeaderboardReturnButtonPrivate").gameObject.SetActive(false);
			}
			prizeIcon.sprite = TacoConfig.currencySprites[t.typeCurrency];
			tournamentTypeText.text = TacoConfig.LeaderboardTournamentType.Replace("&type", tournamentType).Replace("&gameName", TacoSetup.Instance.gameName);
			details = TacoConfig.LeaderboardResultDetails.Replace("&player", t.size.ToString()).Replace("&winner", TacoConfig.Pluralize(t.prize_structure, "Winner"));
			if (!string.IsNullOrEmpty(t.endDate)) {
				FinishedButtons.SetActive(true);
				//Debug.Log("tournamentType: " + tournamentType);
				//UnfinishedButtons.SetActive(true);
				endedLeaderboardFooter.SetActive(true);
				rankIcon.transform.parent.gameObject.SetActive(true);
				int userRank = int.Parse(currentUserRank.rank);
				if (userRank < 4) {
					rankIcon.gameObject.SetActive(true);
					rankIcon.sprite = imageList.Images[currentUserRank.rank];
				}
				else {
					rankText.transform.parent.gameObject.SetActive(true);
					rankText.text = TacoConfig.ToShortOrdinal(userRank);
				}
				string formatedDay = t.endDate.Substring(5, 2) + "-" + t.endDate.Substring(8, 2) + "-" + t.endDate.Substring(0, 4);
				infos += TacoConfig.LeaderboardResultEndOn.Replace("&day", formatedDay);
				if (userRank <= t.prize_structure) {
					resultStatus.text = "You Won!";
					prizeAmount.text = "+ ";
					moneyValue = t.PrizePool / winnerNb;
					int rpModifier = 0;
					if (t.typeCurrency == 0) {
						rpModifier = 50;
					}
					else {
						rpModifier = 5;
					}
					redeemAmount.text = "+ " + TacoManager.FormatRP(Mathf.RoundToInt((float)(moneyValue* rpModifier)));
				}
				else {
					resultStatus.text = "You Lost!";
					prizeAmount.text = "+ ";
					moneyValue = 0;
					redeemAmount.text = "+ " + TacoManager.FormatRP(0);
				}
			}
			else {
				resultStatus.text = "Beautiful Job!";
				playingLeaderboardFooter.SetActive(true);
				/*if (currentUserRank.playable)
				{
					FinishedButtons.SetActive(true);
				}
				else
				{
					UnfinishedButtons.SetActive(true);
				}*/
				rankIcon.transform.parent.gameObject.SetActive(false);
				countdownTime = Mathf.Max((float)t.RemainingTimeSpan.TotalSeconds, -1);
				if (countdownTime < 0) remainingTime.text = "00:00:00";
			}

			if (t.typeCurrency == 0) {
				prizePoolText.text = TacoManager.FormatCash(prizePool);
				entryFeeText.text = TacoManager.FormatCash(entryFee);
				prizeAmount.text += TacoManager.FormatCash(moneyValue);
			}
			else {
				prizePoolText.text = TacoManager.FormatGTokens(prizePool);
				entryFeeText.text = TacoManager.FormatGTokens(entryFee);
				prizeAmount.text += TacoManager.FormatGTokens(moneyValue);
			}

			for (int i = 0; i < LeaderBoardMoneyTypeImages.Count; i++) {
				LeaderBoardMoneyTypeImages[i].sprite = TacoConfig.currencySprites[t.typeCurrency];
			}

			infos += TacoConfig.LeaderboardResultTournamentID.Replace("&id", t.id.ToString());

			resultScore.text = "Score: " + currentUserRank.score.ToString();
			tournamentDetailsText0.text = infos;
			tournamentDetailsText1.text = details;

			playAgain.onClick.RemoveAllListeners();
			playAgain.onClick.AddListener (() => {
				// TacoManager.OpenModalReEnterPanel (
				// 	TacoConfig.ReEnterNotice.Replace ("&type", tournamentType).Replace("&gameName", t.accessType == "public" ? "Public" : "Private"),
				// 	entryFeeText.text);

				//Show Enter Tournament Play Again
				TacoManager.OpenModalEnterTournamentPlayAgainPanel(
				 	TacoConfig.ReEnterNotice.Replace ("&type", tournamentType).Replace("&gameName", TacoSetup.Instance.gameName),
				 	entryFeeText.text,
					t.typeCurrency
					 );
				LeaderboardList.Instance.Destroy();
				TournamentManager.Instance.ShowTournamentPanel();
				Debug.Log("Tour ID: " + TacoManager.Target.id);
				//TacoManager.Target = null;


			});
			Reload(r.leaderboard, !string.IsNullOrEmpty(t.endDate));
		}


		public void LoadLeaderboard(Tournament tournament) {
			TacoManager.OpenMessage(TacoConfig.TacoRefreshing);
			ResetUIValue();

			Action<string> success = (string data) => {
				LeaderboardResult r = JsonUtility.FromJson<LeaderboardResult>(data);
				UpdateUIValue(r);

				TacoManager.CloseMessage();
			};

			Action<string, string> fail = (string data, string error) => {
				TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.Error, TacoConfig.TacoTournamentError);
			};

			var url = "api/tournament/leaderboard/" + TacoSetup.Instance.SiteId + "/" + tournament.id;
			StartCoroutine(ApiManager.Instance.GetWithToken(url, success, fail));
		}

		private void TournamentRemainingTime() {
			countdownTime -= Time.deltaTime;
			remainingTime.text = TacoConfig.timerCountdown(countdownTime);
			if (countdownTime <= 0) {
				Debug.Log("Reload page... " + countdownTime);
			}
		}

		private System.Collections.IEnumerator getLeaderboard(int tournamentId) {
			UnityWebRequest www = UnityWebRequest.Get(Constants.BaseUrl + "api/tournament/leaderboard/" + TacoSetup.Instance.SiteId + "/" + tournamentId);
			www.SetRequestHeader("x-access-token", TacoManager.User.token);

			yield return www.Send();

			if (www.isNetworkError) {
				Debug.Log("Error getting leaderboard : " + www.error);
			}
			else {
				LeaderboardResult r = JsonUtility.FromJson<LeaderboardResult>(www.downloadHandler.text);
				Reload(r.leaderboard, !string.IsNullOrEmpty(r.tournament.endDate));
			}
		}

		private void Reload(List<LeaderboardRow> rows, bool isEndedTournament) {
			ListView.SuspendLayout();
			{
				ListView.Items.Clear();

				Items = rows;
				for (int i = 0; i < rows.Count; i++) {
					AddListViewItem(rows[i], i % 2 == 0, isEndedTournament);
				}
			}
			ListView.ResumeLayout();
		}

		protected void AddListViewItem(LeaderboardRow leaderboardRow, bool isEven, bool isEndedTournament) {
			int rankInt = int.Parse(leaderboardRow.rank);
			string[] subItemTexts = {
				string.Empty,
				isEndedTournament ? (rankInt > 3 ? leaderboardRow.rank : string.Empty) : "N/A",
				leaderboardRow.email,
				leaderboardRow.score.ToString()
			};

			ListViewItem listViewItem = new ListViewItem(subItemTexts);

			if (leaderboardRow.email == TacoManager.User.name) {
				listViewItem.SubItems[0].ImageKey = "currentUser";
			}

			var subItem = listViewItem.SubItems[ColumnNames.IndexOf("RANK")];
			if (!isEndedTournament) {
				//subItem.ImageKey = "na";
			}
			else if (rankInt < 4) {
				subItem.ImageKey = leaderboardRow.rank;
			}
			else {
				//subItem.ImageKey = "0";
			}

			listViewItem.Tag = leaderboardRow;
			listViewItem.UseItemStyleForSubItems = false;

			var backgroundColor = TacoConfig.ListViewOddRow;
			if (isEven) {
				backgroundColor = TacoConfig.ListViewEvenRow;
			}

			for (int i = 0; i < listViewItem.SubItems.Count; i++) {
				listViewItem.SubItems[i].BackColor = backgroundColor;
			}

			ListView.Items.Add(listViewItem);
		}

		protected void UpdateIconForRank() {
			imageList = new ImageList();

			// Add some images.
			imageList.Images.Add("1", TacoConfig.FirstPlaceSprite);
			imageList.Images.Add("2", TacoConfig.SecondPlaceSprite);
			imageList.Images.Add("3", TacoConfig.ThirdPlaceSprite);
			imageList.Images.Add("0", TacoConfig.OtherPlaceSprite);
			imageList.Images.Add("na", TacoConfig.NAPlaceSprite);
			imageList.Images.Add("currentUser", TacoConfig.HandRightIcon);

			// Set the listview's image list.
			ListView.SmallImageList = imageList;
		}

		protected override void CreateSubObjects(ListViewItem item, string columnName) {
			ListViewItem.ListViewSubItem subItem = item.SubItems[ColumnNames.IndexOf(columnName)];
			LeaderboardRow itemData = item.Tag as LeaderboardRow;
			if (columnName == TacoConfig.LeaderboardTournamentPlayerColumn) {
				GameObject button = Instantiate(ItemLeaderboardButtonPrefab);
				LeaderboardItemButton leaderboardItemButton = button.GetComponent<LeaderboardItemButton>();
				leaderboardItemButton.Setup(itemData.rank, itemData.picture, subItem.Text, ListView.DefaultItemFontSize);
				subItem.CustomControl = button.transform as RectTransform;
			}
		}

		private void OnItemBecameVisible(ListViewItem item) {
			DisplaySubObject(item, TacoConfig.LeaderboardTournamentPlayerColumn, true);
			//DisplaySubObject (item, TacoConfig.LeaderboardTournamentRankColumn, true);
		}

		private void OnItemBecameInvisible(ListViewItem item) {
			DisplaySubObject(item, TacoConfig.LeaderboardTournamentPlayerColumn, false);
			//DisplaySubObject (item, TacoConfig.LeaderboardTournamentRankColumn, false);
		}
	}
}
