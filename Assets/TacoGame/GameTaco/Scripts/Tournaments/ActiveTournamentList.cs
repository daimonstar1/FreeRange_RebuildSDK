using UnityEngine;
using UnityEngine.UI;
using Endgame;
using System.Collections.Generic;
using System.Linq;

namespace GameTaco {
	public class ActiveTournamentList : BaseListBehavior {
		public static ActiveTournamentList Instance;
		private GameObject ItemPlayPrefab;

		private ImageList imageList;
		private Sprite notSelected;
		private List<Tournament> Items;
		private GameObject SelectedPreFabToDestroy;

		void Awake() {
			Instance = this;
			GetPrefab();
		}

		private void GetPrefab() {
			ItemPlayPrefab = Resources.Load("TacoPlayButton", typeof(GameObject)) as GameObject;
		}

		// Use this for initialization
		protected override void Start() {
			notSelected = TacoConfig.NotSelected;

			ColumnNames = new List<string>() {
				TacoConfig.ActiveTournamentDateCreated,
				TacoConfig.TournamentPrizeColumn,
				TacoConfig.TournamentWinnersColumn,
				TacoConfig.TournamentPlayersColumn,
				TacoConfig.TournamentTimeLeftColumn,
				TacoConfig.TournamentEntryFeeColumn,
				TacoConfig.TournamentActionColumn
			};

			// TODO : turn this into percentages
			// seems to work when you -20 for the scrollbar and have them add up to 100

			float adjustedWidth = GetWidth() - 20;

			var column1 = (adjustedWidth * 0.2f);
			var column2 = (adjustedWidth * 0.2f);
			var column3 = (adjustedWidth * 0.2f);
			var column4 = (adjustedWidth * 0.2f);
			var column5 = (adjustedWidth * 0.2f);
			var column6 = (adjustedWidth * 0.2f);
			var column7 = (adjustedWidth * 0.2f);
			ColumnWidths = new int[] { (int)column1, (int)column2, (int)column3, (int)column4, (int)column5, (int)column6, (int)column7 };

			ListView.DefaultItemButtonHeight = TacoConfig.ListViewTournamentsButtonHeight;
			ListView.DefaultColumnHeaderHeight = TacoConfig.ListViewHeaderHeight;

			ListView.DefaultItemFontSize = TacoConfig.ListViewItemFontSize;
			ListView.DefaultHeadingFontSize = TacoConfig.ListViewHeaderFontSize;


			ListView.DefaultSelectedItemColor = TacoConfig.Instance.ListViewHighlightColor;

			UpdateIconForEachCell();

			ListView.ItemBecameVisible += this.OnItemBecameVisible;

			ListView.ItemBecameInvisible += this.OnItemBecameInvisible;

			ListView.ColumnClick += base.OnColumnClick;


			base.Start();
		}

		private void OnItemBecameVisible(ListViewItem item) {
			DisplaySubObject(item, TacoConfig.TournamentActionColumn, true);
		}

		private void OnItemBecameInvisible(ListViewItem item) {
			DisplaySubObject(item, TacoConfig.TournamentActionColumn, false);
		}

		protected override void CreateSubObjects(ListViewItem item, string columnName) {
			Tournament t = item.Tag as Tournament;
			ListViewItem.ListViewSubItem selectedSubItem = item.SubItems[ColumnNames.IndexOf(columnName)];

			if (t != null) {
				SelectedPreFabToDestroy = Instantiate(this.ItemPlayPrefab) as GameObject;
				SelectedPreFabToDestroy.transform.Find("Background").GetComponent<Button>().onClick.AddListener(() => {
					TacoManager.Target = t;
					TacoManager.OpenModalPlayTournamentPanel(t);
					//Hide screen 1626
					TacoModalManager.Instance.TournamentConfirmClick();
					GameObject panel = GameObject.Find("TacoGame/TacoCommonCanvas/PanelList/ConfirmPlay");
					panel.SetActive(false);
				});
				selectedSubItem.CustomControl = SelectedPreFabToDestroy.transform as RectTransform;
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
				string players = t.memberIds.Length + "/" + t.size;
				if (t.typeCurrency == 0) {
					fee = TacoManager.FormatCash(t.entryFee);
					prize = TacoManager.FormatCash(t.prize);
				}
				else {
					fee = TacoManager.FormatGTokens(t.entryFee);
					prize = TacoManager.FormatGTokens(t.prize);
				}
				AddListViewItem(t.name, fee, prize, t.prize_structure.ToString(), players, t, i % 2 == 0);
			}

			if (tournaments.Count == 0) {
				AddListViewItem (TacoConfig.NoResults, string.Empty, string.Empty, string.Empty);
				if(GameObject.Find("HideActiveTournament"))
				{
					GameObject.Find ("HideActiveTournament").SetActive (true);
				}
			} else {
				if(GameObject.Find("HideActiveTournament"))
				{
					GameObject.Find("HideActiveTournament").SetActive(false);
				}
			}

			this.ListView.ResumeLayout();
			// fix columns panel does not show vertical handle
			transform.Find("ColumnsPanel").GetComponent<RectTransform>().offsetMax = new Vector2(-20, 0);
		}

		protected void AddListViewItem(string name, string fee, string prize, string prize_structure, string players, Tournament tag, bool isEven) {
			string[] subItemTexts = new string[]
			{
				TacoConfig.DateFromString(tag.createdAt),
				prize,
				prize_structure,
				players,
				tag.RemainingTimeString(),
				fee,
				string.Empty
			};

			ListViewItem listViewItem = new ListViewItem(subItemTexts);

			listViewItem.Tag = tag;
			listViewItem.UseItemStyleForSubItems = false;


			var backgroundColor = TacoConfig.ListViewOddRow;
			if (isEven) {
				backgroundColor = TacoConfig.ListViewEvenRow;
			}

			for (int i = 0; i < listViewItem.SubItems.Count; i++) {
				UpdateSmallImage(listViewItem.SubItems[i], listViewItem, tag);
			}

			listViewItem.UpdateBackgroundColor(backgroundColor);

			this.ListView.Items.Add(listViewItem);
		}
	}
}
