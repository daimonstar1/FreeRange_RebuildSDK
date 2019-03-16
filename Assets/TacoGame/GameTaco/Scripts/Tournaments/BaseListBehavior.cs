using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Endgame;
using System.Collections;

namespace GameTaco
{
	public class BaseListBehavior : MonoBehaviour
	{
		public ListView ListView;
		protected List<string> ColumnNames;
		protected int[] ColumnWidths;
		public GameObject ButtonPrefab;
		private bool clickingAColumnSorts = true;
		private ImageList imageList;
		protected Text noTournamentNotice;

		// Use this for initialization
		protected virtual void Start ()
		{
			if (this.ListView != null) {
				AddColumns ();
				AddItems ();
			}
		}

		// Update is called once per frame
		void Update ()
		{

		}

		public void Destroy ()
		{
			this.ListView.Items.Clear ();
		}

		public float GetWidth ()
		{
			return this.GetComponent<RectTransform> ().rect.width;
		}

		protected void FindNotice(){
			noTournamentNotice = GameObject.Find ("NoTournamentNotice").GetComponent<Text> ();
		}

		protected void SetNotice(string notice=""){
			noTournamentNotice.text = notice;
			noTournamentNotice.gameObject.SetActive (!string.IsNullOrEmpty (notice));
		}

		protected void AddColumns ()
		{
			this.ListView.SuspendLayout ();
			{
				foreach (var name in ColumnNames) {
					AddColumnHeader (name);
				}
				for (int i = 0; i < ColumnWidths.Length; i++) {
					this.ListView.Columns [i].Width = ColumnWidths [i];
				}
			}
			this.ListView.ResumeLayout ();
		}

		private void AddColumnHeader (string title)
		{
			ColumnHeader columnHeader = new ColumnHeader ();
			columnHeader.Text = title;
			this.ListView.Columns.Add (columnHeader);
		}

		protected void AddItems ()
		{
			this.ListView.SuspendLayout ();
			{
				this.ListView.Items.Clear ();
			}
			this.ListView.ResumeLayout ();
		}

		protected virtual void AddListViewItem (string name, string fee, string prize, object tag)
		{
			ListViewItem listViewItem = new ListViewItem (name);

			listViewItem.Tag = tag;
			listViewItem.SubItems.Add (fee);
			listViewItem.UseItemStyleForSubItems = false;


			this.ListView.Items.Add (listViewItem);
		}

		protected void OnColumnClick (object sender, ListView.ColumnClickEventArgs e)
		{
			if (this.clickingAColumnSorts) {
				ListView listView = (ListView)sender;

				if (string.IsNullOrEmpty (listView.Columns [e.Column].Text)) return;

				if (TacoManager.columnSortName != listView.Columns[e.Column].Text) {
					TacoManager.columnSortType = 0;
					TacoManager.columnSortName = listView.Columns[e.Column].Text;
				}
				else {
					TacoManager.columnSortType = 1 - TacoManager.columnSortType;
				}
				
				if (listView.Columns [e.Column].Text.IndexOf ("PRIZE") > -1 || listView.Columns [e.Column].Text == TacoConfig.TournamentEntryFeeColumn) {
					listView.ListViewItemSorter = new ListViewItemComparer (columnIndex: e.Column, isMoney: true);
				} else if (listView.Columns [e.Column].Text == TacoConfig.TournamentWinnersColumn) {
					listView.ListViewItemSorter = new ListViewItemComparer (columnIndex: e.Column, isPrizeStructure: true);
				} else if (listView.Columns [e.Column].Text == TacoConfig.TournamentPlayersColumn) {
					listView.ListViewItemSorter = new ListViewItemComparer (columnIndex: e.Column, isSize: true);
				} else if (listView.Columns [e.Column].Text == TacoConfig.TournamentTimeLeftColumn) {
					listView.ListViewItemSorter = new ListViewItemComparer (columnIndex: e.Column, isTimeRemaining: true);
				} else if (listView.Columns [e.Column].Text == TacoConfig.CompletedTournamentPlacementColumn) {
					listView.ListViewItemSorter = new ListViewItemComparer (columnIndex: e.Column, isPlacement: true);
				} else if (listView.Columns [e.Column].Text == TacoConfig.TransactionAmount) {
					listView.ListViewItemSorter = new ListViewItemComparer(columnIndex: e.Column, isTransactionAmount: true);
				} else if (listView.Columns[e.Column].Text.IndexOf("DATE") > -1) {
					listView.ListViewItemSorter = new ListViewItemComparer(columnIndex: e.Column, isDateString: true);
				} else if (listView.Columns[e.Column].Text == TacoConfig.TransactionNumber) {
					listView.ListViewItemSorter = new ListViewItemComparer(columnIndex: e.Column, isNumber: true);
				} else {
					listView.ListViewItemSorter = new ListViewItemComparer (e.Column);
				}
			}
		}

		private class ListViewItemComparer : IComparer
		{
			private int columnIndex = 0;
			private bool isPrizeStructure;
			private bool isSize;
			private bool isMoney;
			private bool isTimeRemaining;
			private bool isStatus;
			private bool isPlacement;
			private bool isTransactionAmount;
			private bool isNumber;
			private bool isDateString;

			public ListViewItemComparer()
			{
			}

			public ListViewItemComparer(int columnIndex, bool isPrizeStructure = false, bool isSize = false, bool isMoney = false, bool isTimeRemaining = false, bool isStatus = false, bool isPlacement = false,
										 bool isTransactionAmount = false, bool isNumber = false, bool isDateString = false)
			{
				this.columnIndex = columnIndex;
				this.isPrizeStructure = isPrizeStructure;
				this.isSize = isSize;
				this.isMoney = isMoney;
				this.isTimeRemaining = isTimeRemaining;
				this.isStatus = isStatus;
				this.isPlacement = isPlacement;
				this.isTransactionAmount = isTransactionAmount;
				this.isNumber = isNumber;
				this.isDateString = isDateString;
			}

			public int Compare(object object1, object object2)
			{
				ListViewItem listViewItem1 = object1 as ListViewItem;
				ListViewItem listViewItem2 = object2 as ListViewItem;
				string text1 = listViewItem1.SubItems[this.columnIndex].Text;
				string text2 = listViewItem2.SubItems[this.columnIndex].Text;
				int sortResult = 0;
				if (isMoney)
				{
					double number1;
					double number2;

					if (text1.IndexOf(TacoConfig.HidedNumber) > -1)
					{
						number1 = 0;
					}
					else
					{
						if (text1.IndexOf("<") > -1) text1 = text1.Substring(0, text1.IndexOf("<"));
						text1 = text1.Replace("$", string.Empty).Trim();//currently we use color for currency
						number1 = double.Parse(text1, System.Globalization.NumberStyles.Number);
					}

					if (text2.IndexOf(TacoConfig.HidedNumber) > -1)
					{
						number2 = 0;
					}
					else
					{
						if (text2.IndexOf("<") > -1) text2 = text2.Substring(0, text2.IndexOf("<"));
						text2 = text2.Replace("$", string.Empty).Trim();
						number2 = double.Parse(text2, System.Globalization.NumberStyles.Number);
					}

					sortResult = number1.CompareTo(number2);
				}
				else if (isPrizeStructure)
				{
					int number1 = int.Parse(text1);
					int number2 = int.Parse(text2);
					sortResult = number1.CompareTo(number2);
				}
				else if (isSize)
				{
					int number1 = ((Tournament)(listViewItem1.Tag)).size;
					int number2 = ((Tournament)(listViewItem2.Tag)).size;
					sortResult = number1.CompareTo(number2);
				}
				else if (isTimeRemaining)
				{
					double number1 = ((Tournament)(listViewItem1.Tag)).RemainingTimeSpan.TotalHours;
					double number2 = ((Tournament)(listViewItem2.Tag)).RemainingTimeSpan.TotalHours;
					sortResult = number1.CompareTo(number2);

				}
				else if (isPlacement)
				{
					int number1 = 0;
					int number2 = 0;
					Tournament t1 = (Tournament)(listViewItem1.Tag);
					Tournament t2 = (Tournament)(listViewItem2.Tag);

					if (string.IsNullOrEmpty(t1.endDate)) number1 = 999;
					else number1 = t1.rank;

					if (string.IsNullOrEmpty(t2.endDate)) number2 = 999;
					else number2 = t2.rank;

					sortResult = number1.CompareTo(number2);

				}
				else if (isNumber)
				{
					int number1 = int.Parse(text1);
					int number2 = int.Parse(text2);
					sortResult = number1.CompareTo(number2);
				}
				else if (isTransactionAmount)
				{
					float number1 = (float)((UserTransaction)(listViewItem1.Tag)).amount; //float.Parse(((UserTransaction)(listViewItem1.Tag)).amount);
					float number2 = (float)((UserTransaction)(listViewItem2.Tag)).amount;// float.Parse(((UserTransaction)(listViewItem2.Tag)).amount);
					if (number1 == number2)
					{
						sortResult = string.Compare(text1, text2);
					}
					else
					{
						sortResult = number1.CompareTo(number2);
					}
				}
				else if (isDateString)
				{
					string date1 = text1.Substring(5, 4) + text1.Substring(0, 2) + text1.Substring(3, 2);
					string date2 = text2.Substring(5, 4) + text2.Substring(0, 2) + text2.Substring(3, 2);
					sortResult = string.Compare(date1, date2);
				}
				/*else if(isStatus) {
					text1 = listViewItem1.SubItems [this.columnIndex].Text;
					text2 = listViewItem2.SubItems [this.columnIndex].Text;
					sortResult = string.Compare(text1, text2);
				}*/
				else
				{
					sortResult = string.Compare(text1, text2);
				}
				if (TacoManager.columnSortType == 1)
				{
					if (sortResult == 1)
					{
						sortResult = -1;
					}
					else if (sortResult == -1)
					{
						sortResult = 1;
					}
					return sortResult;
				}
				return sortResult;
			}
		}


		protected void UpdateIconForEachCell()
		{
			imageList = new ImageList();

			// Add some images.

			imageList.Images.Add("TacoPRIZE", TacoConfig.IconGTaco);
			imageList.Images.Add("RealPRIZE", TacoConfig.IconCash);
			imageList.Images.Add("WINNERS", TacoConfig.IconTournamentWinner);
			imageList.Images.Add("PLAYERS", TacoConfig.IconTournamentUser);
			imageList.Images.Add("TIMELEFT", TacoConfig.IconTournamentTime);

			// Set the listview's image list.
			this.ListView.SmallImageList = imageList;
		}

		protected void UpdateSmallImage(ListViewItem.ListViewSubItem subitem, ListViewItem item, Tournament t)
		{
			if (subitem == item.SubItems[ColumnNames.IndexOf(TacoConfig.TournamentPrizeColumn)] || subitem == item.SubItems[ColumnNames.IndexOf(TacoConfig.TournamentEntryFeeColumn)])
			{
				if (t.typeCurrency == 0)
				{
					subitem.ImageKey = "RealPRIZE";
				}
				else
				{
					subitem.ImageKey = "TacoPRIZE";
				}
			}
			else if (subitem == item.SubItems[ColumnNames.IndexOf(TacoConfig.TournamentWinnersColumn)])
			{
				subitem.ImageKey = "WINNERS";
			}
			else if (subitem == item.SubItems[ColumnNames.IndexOf(TacoConfig.TournamentPlayersColumn)])
			{
				subitem.ImageKey = "PLAYERS";
			}
			else if (subitem == item.SubItems[ColumnNames.IndexOf(TacoConfig.TournamentTimeLeftColumn)])
			{
				subitem.ImageKey = "TIMELEFT";
			}
		}

		protected void DisplaySubObject(ListViewItem item, string columnName, bool status){
			if (status) {
				CreateSubObjects (item, columnName);
			}
			else{
				var subItem = item.SubItems[ColumnNames.IndexOf(columnName)];
				if (subItem.CustomControl != null) {
					Destroy (subItem.CustomControl.gameObject);
				}
			}
		}

		protected virtual void CreateSubObjects(ListViewItem item, string columnName){
		
		}
	}
}


