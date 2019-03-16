using UnityEngine;
using UnityEngine.UI;
using Endgame;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameTaco {
	public class CompletedTournamentsList : BaseListBehavior {

		public static CompletedTournamentsList Instance;

		private GameObject ItemShowLeaderboardPrefab;
		private GameObject ItemInvitePrefab;

		private ImageList imageList;
		private Sprite notSelected;
		private List<Tournament> Items;
		private GameObject SelectedPreFabToDestroy;

		void Awake() {
			Instance = this;
			FindNotice();
			GetPrefab();
		}

		private void GetPrefab() {
			ItemShowLeaderboardPrefab = Resources.Load("TacoSeeLeaderboardButton", typeof(GameObject)) as GameObject;
			ItemInvitePrefab = Resources.Load("TacoInviteButton", typeof(GameObject)) as GameObject;
		}

		// Use this for initialization
		protected override void Start() {
			notSelected = TacoConfig.NotSelected;

			ColumnNames = new List<string>() {
				TacoConfig.CompletedTournamentDateColumn,
				TacoConfig.CompletedTournamentWinLossColumn,
				TacoConfig.CompletedTournamentPlacementColumn,
				TacoConfig.CompletedTournamentPrizePoolColumn,
				TacoConfig.CompletedTournamentPrizeWonColumn,
				TacoConfig.CompletedTournamentActionColumn
			};

			// TODO : turn this into percentages
			// seems to work when you -20 for the scrollbar and have them add up to 100

			float adjustedWidth = GetWidth() - 20;

			var column1 = (adjustedWidth * 0.19f);
			var column2 = (adjustedWidth * 0.15f);
			var column3 = (adjustedWidth * 0.15f);
			var column4 = (adjustedWidth * 0.15f);
			var column5 = (adjustedWidth * 0.15f);
			var column6 = (adjustedWidth * 0.21f);
			ColumnWidths = new int[] { (int)column1, (int)column2, (int)column3, (int)column4, (int)column5, (int)column6 };

			UpdateIcon();

			ListView.SelectedIndexChanged += ListView_SelectedIndexChanged;

			ListView.DefaultItemButtonHeight = TacoConfig.ListViewTournamentsButtonHeight;
			ListView.DefaultColumnHeaderHeight = TacoConfig.ListViewHeaderHeight;

			ListView.DefaultItemFontSize = TacoConfig.ListViewItemFontSize;
			ListView.DefaultHeadingFontSize = TacoConfig.ListViewHeaderFontSize;

			// colors can't be CONST
			//ListView.DefaultHeadingBackgroundColor = TacoConfig.Instance.TacoListViewHeaderColor;
			ListView.DefaultSelectedItemColor = TacoConfig.Instance.ListViewHighlightColor;

			ListView.ItemBecameVisible += this.OnItemBecameVisible;
			ListView.ItemBecameInvisible += this.OnItemBecameInvisible;
			ListView.ColumnClick += base.OnColumnClick;
			base.Start();
		}

		private void OnItemBecameVisible(ListViewItem item) {
			DisplaySubObject(item, string.Empty, true);
		}

		private void OnItemBecameInvisible(ListViewItem item) {
			DisplaySubObject(item, string.Empty, false);
		}

		private void ListView_SelectedIndexChanged(object sender, System.EventArgs e) {
			if (this.ListView.SelectedIndices.Count > 0) {
				int index = this.ListView.SelectedIndices[0];

				if (Items.Count > index) {
					//ListViewItem.ListViewSubItem selectedSubItem = this.ListView.Items [index].SubItems[4];
					var t = Items[index];
					//SelectedPreFabToDestroy = (t.played) ? GameObject.Instantiate(this.ItemShowLeaderboardPrefab) as GameObject : GameObject.Instantiate(this.ItemPlayPrefab) as GameObject;
					//selectedSubItem.CustomControl = SelectedPreFabToDestroy.transform as RectTransform;

					TournamentManager.Instance.TappedGameFromList(t);

					this.ListView.SelectedIndices.Clear();

					// int index = this.ListView.SelectedIndices[0];
					// var t = Items[index];
				}
				else {
					this.ListView.SelectedIndices.Clear();
				}
			}
		}

		protected void UpdateIcon() {
			imageList = new ImageList();

			// Add some images.

			imageList.Images.Add("1", TacoConfig.FirstPlaceSprite);
			imageList.Images.Add("2", TacoConfig.SecondPlaceSprite);
			imageList.Images.Add("3", TacoConfig.ThirdPlaceSprite);
			imageList.Images.Add("win", TacoConfig.WinSprite);
			imageList.Images.Add("loss", TacoConfig.LossSprite);
			imageList.Images.Add("remaining", TacoConfig.NAPlaceSprite);

			// Set the listview's image list.
			this.ListView.SmallImageList = imageList;
		}

		protected override void CreateSubObjects(ListViewItem item, string columnName) {
			Tournament t = item.Tag as Tournament;
			ListViewItem.ListViewSubItem selectedSubItem = item.SubItems[ColumnNames.IndexOf(columnName)];

			if (t != null) {
				int nbOfActions = (t.creatorId == TacoManager.User.userId && t.status != "ended" && t.accessType != "public" && t.memberIds.Length < t.size) ? 2 : 1;
				CreateActionButton(item, t, this.ItemShowLeaderboardPrefab, nbOfActions);
				if (nbOfActions > 1) CreateActionButton(item, t, this.ItemInvitePrefab, nbOfActions);
			}
		}

		private void CreateActionButton(ListViewItem item, Tournament t, GameObject prefab, int nbOfActions) {
			ListViewItem.ListViewSubItem selectedSubItem = item.SubItems[ColumnNames.IndexOf(TacoConfig.TournamentActionColumn)];
			SelectedPreFabToDestroy = Instantiate(prefab) as GameObject;
			selectedSubItem.CustomControl = SelectedPreFabToDestroy.transform as RectTransform;
			Transform background = SelectedPreFabToDestroy.transform.Find("Background");

			RectTransform rectPos = background.gameObject.GetComponent<RectTransform>();
			if (nbOfActions > 1) {
				SelectedPreFabToDestroy.transform.Find("Background/Text").gameObject.SetActive(false);
				rectPos.sizeDelta = new Vector2(rectPos.sizeDelta.x / 2, rectPos.sizeDelta.y);
				SelectedPreFabToDestroy.transform.Find("Background/BackgroundImage").gameObject.SetActive(false);
				SelectedPreFabToDestroy.transform.Find("Background/BackgroundImage1").gameObject.SetActive(true);
			}
			background.GetComponent<Button>().onClick.AddListener(() => {
				if (prefab.name == ItemShowLeaderboardPrefab.name) {
					TournamentManager.Instance.SeeResult(t);
				}
				else {
					TacoManager.Target = t;
					TacoManager.ShowPanel(PanelNames.ManageTournamentPanel);
				}
			});

			if (nbOfActions > 1) {
				SelectedPreFabToDestroy.transform.Find("Background/Text").gameObject.SetActive(false);
			}
		}

		public void Reload(List<Tournament> tournaments) {
			this.ListView.SuspendLayout();
			this.ListView.Items.Clear();

			Items = tournaments;

			for (int i = 0; i < Items.Count(); i++) {
				var t = Items[i];
				string fee = string.Empty;
				string prize = string.Empty;

				t.played = true;

				if (t.typeCurrency == 0) {
					fee = TacoManager.FormatCash(t.entryFee);
					prize = TacoManager.FormatCash(t.prize);
				}
				else {
					fee = TacoManager.FormatGTokens(t.entryFee);
					prize = TacoManager.FormatGTokens(t.prize);
				}

				AddListViewItem(fee, prize, t.prize_structure.ToString(), t, i % 2 == 0);
			}


			if (tournaments.Count == 0) {
				SetNotice(TacoConfig.NoResults);
			}
			else {
				SetNotice();
			}

			this.ListView.ResumeLayout();
		}

		private List<Tournament> GetItems() {
			return Items;
		}

		protected void AddListViewItem(string fee, string prize, string prize_structure, Tournament tag, bool isEven) {
			string winLoss;
			string prizeResult = TacoConfig.HidedNumber;
			string placement = string.Empty;
			string prizePool = string.Empty;
			string winLossKey;
			string placementKey = string.Empty;


			if (TacoSetup.Instance.TournamentCategory == TournamentCate.Cash) {
				prizePool = TacoManager.FormatCash(tag.PrizePool);
			}
			else {
				prizePool = TacoManager.FormatGTokens(tag.PrizePool);
			}

			if (string.IsNullOrEmpty(tag.endDate)) {
				winLoss = "Ends in\n" + tag.RemainingTimeString(false);
				winLossKey = "Remaining";
				placement = "N/A";
			}
			else {

				if (tag.rank <= tag.prize_structure) {
					winLoss = "<color=#ccff66>WIN</color>";
					winLossKey = "win";
					if (TacoSetup.Instance.TournamentCategory == TournamentCate.Cash) {
						prizeResult = TacoManager.FormatCash(tag.prize);
					}
					else {
						prizeResult = TacoManager.FormatGTokens(tag.prize);
					}
				}
				else {
					winLoss = "<color=#ff6666>LOSS</color>";
					winLossKey = "loss";
				}

				if (tag.rank < 4) {
					placement = string.Empty;
					placementKey = tag.rank.ToString();
				}
				else
					placement = tag.rank.ToString();
			}

			string[] subItemTexts = new string[] {
				tag.PlayedDayFormat,
				winLoss,
				placement,
				prizePool,
				prizeResult,
				string.Empty
			};

			ListViewItem listViewItem = new ListViewItem(subItemTexts);

			listViewItem.Tag = tag;
			listViewItem.UseItemStyleForSubItems = false;

			listViewItem.SubItems[ColumnNames.IndexOf("WIN/LOSS")].ImageKey = winLossKey;
			listViewItem.SubItems[ColumnNames.IndexOf("PLACEMENT")].ImageKey = placementKey;

			var backgroundColor = TacoConfig.ListViewOddRow;
			if (isEven) {
				backgroundColor = TacoConfig.ListViewEvenRow;
			}

			for (int i = 0; i < listViewItem.SubItems.Count; i++) {
				listViewItem.SubItems[i].BackColor = backgroundColor;
			}

			this.ListView.Items.Add(listViewItem);
		}

	}
}
