using UnityEngine;
using Endgame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameTaco {
	public class TransactionList : BaseListBehavior {

		public static TransactionList Instance;

		private GameObject ItemPlayPrefab;

		private ImageList imageList;
		private Sprite notSelected;
		private List<UserTransaction> Items;

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
				TacoConfig.TransactionType,
				TacoConfig.TransactionAmount,
				TacoConfig.TransactionDate,
				TacoConfig.TransactionNumber,
			};

			// TODO : turn this into percentages
			// seems to work when you -20 for the scrollbar and have them add up to 100

			float adjustedWidth = GetWidth() - 30;

			var column1 = (adjustedWidth * 0.3f);
			var column2 = (adjustedWidth * 0.2f);
			var column3 = (adjustedWidth * 0.2f);
			var column4 = (adjustedWidth * 0.3f);
			ColumnWidths = new int[] { (int)column1, (int)column2, (int)column3, (int)column4 };

			ListView.DefaultItemButtonHeight = TacoConfig.ListViewTournamentsButtonHeight;
			ListView.DefaultColumnHeaderHeight = TacoConfig.ListViewHeaderHeight;

			ListView.DefaultItemFontSize = TacoConfig.ListViewItemFontSize;
			ListView.DefaultHeadingFontSize = TacoConfig.ListViewHeaderFontSize;


			ListView.DefaultSelectedItemColor = TacoConfig.Instance.ListViewHighlightColor;

			ListView.ItemBecameVisible += this.OnItemBecameVisible;
			ListView.ItemBecameInvisible += this.OnItemBecameInvisible;
			ListView.ColumnClick += base.OnColumnClick;

			base.Start();
		}

		private void OnItemBecameVisible(ListViewItem item) {
		}

		private void OnItemBecameInvisible(ListViewItem item) {
		}

		public void GetTransactions() {
			TacoManager.OpenMessage(TacoConfig.TacoRefreshing);
			int cate = (int)TacoSetup.Instance.TournamentCategory;
			Action<string> success = (string data) => {
				TransactionResult r = JsonUtility.FromJson<TransactionResult>(data);
				UserTransaction recent = r.transactions[0];
				Reload(r.transactions.ToList());
				TacoManager.CloseMessage();
			};

			Action<string, string> fail = (string data, string error) => {

				Debug.Log("Error getting open tournaments : " + data);
				if (!string.IsNullOrEmpty(error)) {
					Debug.Log("Error : " + error);
				}
				TacoManager.CloseMessage();
			};

			StartCoroutine(ApiManager.Instance.GetTransactions(15, success, fail));
		}

		public void Reload(List<UserTransaction> transactions) {
			this.ListView.SuspendLayout();
			this.ListView.Items.Clear();
			Items = transactions;
			for (int i = 0; i < Items.Count(); i++) {
				var t = Items[i];
				AddListViewItem(t.action, t.FormatCurrency, TacoManager.FormatDate(t.createdAt), t.id, t, i % 2 == 0);
			}

			if (Items.Count == 0) {
				AddListViewItem(TacoConfig.NoResults, string.Empty, string.Empty, string.Empty);
			}

			this.ListView.ResumeLayout();
		}

		protected void AddListViewItem(string action, string amount, string date, int number, UserTransaction tag, bool isEven) {
			string[] subItemTexts = new string[]
			{
				action, amount, date, number.ToString()
			};

			ListViewItem listViewItem = new ListViewItem(subItemTexts);

			listViewItem.Tag = tag;
			listViewItem.UseItemStyleForSubItems = false;


			var backgroundColor = TacoConfig.ListViewOddRow;
			if (isEven) {
				backgroundColor = TacoConfig.ListViewEvenRow;
			}

			listViewItem.UpdateBackgroundColor(backgroundColor);

			this.ListView.Items.Add(listViewItem);
		}
	}
}
