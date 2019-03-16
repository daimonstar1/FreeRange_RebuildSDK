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
	public class PublicTournamentsList : BaseListBehavior {
		
		public static PublicTournamentsList Instance;

		public Tournament SelectedTournament = null;
		private GameObject ItemPlayPrefab;
		private GameObject ItemExpiredPrefab;
		private GameObject ItemShowLeaderboardPrefab;
		private GameObject ItemJoinPrefab;
		private GameObject SelectedPreFabToDestroy;
		private List<Tournament> Items;

		void Awake() {
			Instance = this;
			FindNotice ();
			GetPrefab ();
		}

		private void GetPrefab(){
			ItemPlayPrefab = Resources.Load ("TacoPlayButton", typeof(GameObject)) as GameObject;
			ItemExpiredPrefab = Resources.Load ("TacoExpiredButton", typeof(GameObject)) as GameObject;
			ItemShowLeaderboardPrefab = Resources.Load ("TacoSeeLeaderboardButton", typeof(GameObject)) as GameObject;
			ItemJoinPrefab = Resources.Load ("TacoJoinButton", typeof(GameObject)) as GameObject;
		}

    	// Use this for initialization
		protected override void Start() {
			
			ColumnNames = new List<string> () {
				TacoConfig.TournamentPrizeColumn,
				TacoConfig.TournamentWinnersColumn,
				TacoConfig.TournamentPlayersColumn,
				TacoConfig.TournamentTimeLeftColumn,
				TacoConfig.TournamentEntryFeeColumn,
				TacoConfig.TournamentActionColumn
			};

			// TODO : turn this into percentages
			// seems to work when you -20 for the scrollbar and have them add up to 100
		
			float adjustedWidth = GetWidth () - 20;
			var column1 = ( adjustedWidth * 0.16f);
			var column2 = ( adjustedWidth * 0.16f);
			var column3 = ( adjustedWidth * 0.16f);
			var column4 = ( adjustedWidth * 0.16f);
			var column5 = ( adjustedWidth * 0.16f);
			var column6 = ( adjustedWidth * 0.20f);
			ColumnWidths = new int[] { (int)column1 , (int)column2, (int)column3,(int)column4, (int)column5,(int)column6};
			
			ListView.SelectedIndexChanged += ListView_SelectedIndexChanged;

			ListView.DefaultItemButtonHeight = TacoConfig.ListViewTournamentsButtonHeight;
			ListView.DefaultColumnHeaderHeight = TacoConfig.ListViewHeaderHeight;

			ListView.DefaultItemFontSize = TacoConfig.ListViewItemFontSize;
			ListView.DefaultHeadingFontSize = TacoConfig.ListViewHeaderFontSize;

			// colors can't be CONST
			//ListView.DefaultHeadingBackgroundColor = TacoConfig.Instance.TacoListViewHeaderColor;
			ListView.DefaultSelectedItemColor = TacoConfig.Instance.ListViewHighlightColor;
			ListView.GetComponent<Image>().color = new Color(1,1,1,1f);

			UpdateIconForEachCell ();

			ListView.ItemBecameVisible += this.OnItemBecameVisible;
			ListView.ItemBecameInvisible += this.OnItemBecameInvisible;
			ListView.ColumnClick += base.OnColumnClick;

			
			base.Start();
		}


		private void OnItemBecameVisible(ListViewItem item) {
			DisplaySubObject (item, TacoConfig.TournamentActionColumn, true);
			DisplaySubObject (item, TacoConfig.TournamentPrizeColumn, true);
			DisplaySubObject (item, TacoConfig.TournamentEntryFeeColumn, true);
		}

		private void OnItemBecameInvisible(ListViewItem item) {
			DisplaySubObject (item, TacoConfig.TournamentActionColumn, false);
			DisplaySubObject (item, TacoConfig.TournamentPrizeColumn, false);
			DisplaySubObject (item, TacoConfig.TournamentEntryFeeColumn, false);
		}

		private void ListView_SelectedIndexChanged(object sender, System.EventArgs e) {
			if (this.ListView.SelectedIndices.Count > 0) {
				int index = this.ListView.SelectedIndices[0];
				if (Items.Count > index) {
					//ListViewItem.ListViewSubItem selectedSubItem = this.ListView.Items [index].SubItems[4];
					var t = Items [index];
					if(t.status == "cancelled"){
						
					}else if((t.played && !t.playable) || System.Array.IndexOf(t.memberIds,TacoManager.User.userId) > -1 ){
						TournamentManager.Instance.TappedGameFromList(t);
					}else{
						TournamentManager.Instance.TappedJoinFromList (t);
					}
					
					this.ListView.SelectedIndices.Clear ();
				} else {
					this.ListView.SelectedIndices.Clear();
				}
			}
		}
			
		protected override void CreateSubObjects(ListViewItem item, string columnName) {
			Tournament t = item.Tag as Tournament;
			if (t == null) return;
			ListViewItem.ListViewSubItem selectedSubItem = item.SubItems [ColumnNames.IndexOf (columnName)];
			if (columnName == TacoConfig.TournamentActionColumn) {
				if (t.status == "cancelled") {
					SelectedPreFabToDestroy = GameObject.Instantiate (this.ItemExpiredPrefab) as GameObject;

				} else if (t.played && !t.playable) {
					//result
					SelectedPreFabToDestroy = GameObject.Instantiate (this.ItemShowLeaderboardPrefab) as GameObject;
					SelectedPreFabToDestroy.transform.Find ("Background").GetComponent<Button> ().onClick.AddListener (() => {
						TournamentManager.Instance.TappedGameFromList (t);
					});
				} else if (System.Array.IndexOf (t.memberIds, TacoManager.User.userId) > -1) {
					// and play now
					SelectedPreFabToDestroy = GameObject.Instantiate (this.ItemPlayPrefab) as GameObject;
					SelectedPreFabToDestroy.transform.Find ("Background").GetComponent<Button> ().onClick.AddListener (() => {
						TournamentManager.Instance.TappedGameFromList (t);
						//Hide screen 1626
						TacoModalManager.Instance.TournamentConfirmClick();
						GameObject panel = GameObject.Find("TacoGame/TacoCommonCanvas/PanelList/ConfirmPlay");
						panel.SetActive(false);
					});
				} else {
					//join
					SelectedPreFabToDestroy = GameObject.Instantiate (this.ItemJoinPrefab)as GameObject;
					SelectedPreFabToDestroy.transform.Find ("Background").GetComponent<Button> ().onClick.AddListener (() => {
						TournamentManager.Instance.TappedJoinFromList (t);
					});
				}

				selectedSubItem.CustomControl = SelectedPreFabToDestroy.transform as RectTransform;
			} else if (columnName == TacoConfig.TournamentPrizeColumn) {
				SelectedPreFabToDestroy = GameObject.Instantiate (TacoConfig.DetailCellPrefab) as GameObject;
				SelectedPreFabToDestroy.transform.Find ("Background/ColumnName").GetComponent<Text> ().text = "Prize";
				SelectedPreFabToDestroy.transform.Find ("Background/Money").GetComponent<Text> ().text = TacoManager.FormatMoney (t.prize, t.typeCurrency);
				SelectedPreFabToDestroy.transform.Find ("Background/Icon").GetComponent<Image> ().sprite = TacoConfig.currencySprites [t.typeCurrency];
				selectedSubItem.CustomControl = SelectedPreFabToDestroy.transform as RectTransform;
			} else if (columnName == TacoConfig.TournamentEntryFeeColumn) {
				SelectedPreFabToDestroy = GameObject.Instantiate (TacoConfig.DetailCellPrefab) as GameObject;
				SelectedPreFabToDestroy.transform.Find ("Background/ColumnName").GetComponent<Text> ().text = "Entry Fee";
				SelectedPreFabToDestroy.transform.Find ("Background/Money").GetComponent<Text> ().text = TacoManager.FormatMoney (t.entryFee, t.typeCurrency);
				SelectedPreFabToDestroy.transform.Find ("Background/Icon").GetComponent<Image> ().sprite = TacoConfig.currencySprites [t.typeCurrency];
				selectedSubItem.CustomControl = SelectedPreFabToDestroy.transform as RectTransform;
			}
		}

		
		public void Reload(List<Tournament> tournaments) {
			
			this.ListView.SuspendLayout();
			this.ListView.Items.Clear();

			Items = tournaments;


			for (int i = 0; i < Items.Count(); i++)
			{
				var t = Items[i];
				string fee = string.Empty;
				string prize = string.Empty;
				string players = t.memberIds.Length + "/" + t.size;
				if( t.typeCurrency == 0 ) {
					fee = TacoManager.FormatCash(t.entryFee);
					prize = TacoManager.FormatCash(t.prize);
				}
				else {
					fee = TacoManager.FormatGTokens(t.entryFee);
					prize = TacoManager.FormatGTokens(t.prize);
				}
				AddListViewItem(fee, prize, t.prize_structure.ToString(), players, t, i % 2 == 0);
			}
				
			// if there are any results - make a row to tell that

			if (tournaments.Count == 0) {
				SetNotice (TacoConfig.NoResults);
			} else {
				SetNotice ();
			}
				
			// used to set selected to first row
			/*if( this.ListView.Items.Count > 0 ){

				this.ListView.SelectedIndices.Add (0);
			}
			*/

			this.ListView.ResumeLayout();
		}

		public string GetDisplayedStatus(Tournament t){
			string status = string.Empty;
			if(t.status == "cancelled"){
				status = "4-expired";
			}else if(t.played && !t.playable){
				status = "3-leaderboard";
			}else if(System.Array.IndexOf(t.memberIds,TacoManager.User.userId) > -1){
				// and play now
				status = "2-play";
			}
			else{
				//join
				status = "1-join";
			}
			return status;
		}

		protected void AddListViewItem(string fee, string prize, string prize_structure, string players, Tournament tag, bool isEven) {
			string[] subItemTexts = new string[]
			{
				prize,
				prize_structure,
				players,
				tag.RemainingTimeString(),
				fee,
				GetDisplayedStatus(tag)
			};

			ListViewItem listViewItem = new ListViewItem(subItemTexts);

			listViewItem.Tag = tag;
			listViewItem.UseItemStyleForSubItems = false;

			var backgroundColor = TacoConfig.ListViewOddRow;
			if(isEven) {
				backgroundColor = TacoConfig.ListViewEvenRow;
			}

			for (int i = 0; i < listViewItem.SubItems.Count; i++) {
				UpdateSmallImage (listViewItem.SubItems [i], listViewItem, tag);
			}

			listViewItem.UpdateBackgroundColor (backgroundColor);

			this.ListView.Items.Add(listViewItem);
		}

		public void OnVerticalScrollValueChanged(float value)
		{
			if (value == 0) {
				Debug.Log ("Add more");
			}
		}
	}
}
