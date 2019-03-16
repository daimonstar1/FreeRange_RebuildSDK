using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

namespace GameTaco {
	public class SortTournament : MonoBehaviour {
		public static SortTournament Instance;
		public Dropdown Column;
		public Dropdown SortOrder;

		private string selectedColumn;
		private int selectedOrder;
		private bool isProfileActive;

		private string[][] columnOption = new string[][] {
			new string[] {
				TacoConfig.TournamentPrizeColumn,
				TacoConfig.TournamentWinnersColumn,
				TacoConfig.TournamentPlayersColumn,
				TacoConfig.TournamentTimeLeftColumn,
				TacoConfig.TournamentEntryFeeColumn
			},
			new string[] {
				TacoConfig.CompletedTournamentDateColumn,
				TacoConfig.CompletedTournamentWinLossColumn,
				TacoConfig.CompletedTournamentPlacementColumn,
				TacoConfig.CompletedTournamentPrizePoolColumn,
				TacoConfig.CompletedTournamentPrizeWonColumn
			},
			new string[] {
				TacoConfig.CompletedTournamentDateColumn,
				TacoConfig.TournamentPrizeColumn,
				TacoConfig.TournamentWinnersColumn,
				TacoConfig.TournamentPlayersColumn,
				TacoConfig.TournamentTimeLeftColumn,
				TacoConfig.TournamentEntryFeeColumn,
				TacoConfig.TournamentActionColumn
			}
		};
		private string[][] orderOptions = new string[][] {
			new string[]{ "Ascending", "Descending"},
			new string[]{ "0", "1" }
		};

		private void Awake() {
			Instance = this;
			AssignButtons();
			SetDropdownHeight();
			AssignDropDown();
		}

		private void SetDropdownHeight() {
			List<Dropdown> dropdownList = new List<Dropdown>() {
				Column, SortOrder
			};
			// Set dropdown height
			for (int i = 0; i < dropdownList.Count; i++) {
				GameObject dropdownObject = dropdownList[i].gameObject;
				Vector2 dropdownPosition = dropdownObject.GetComponent<RectTransform>().sizeDelta;

				RectTransform templateRect = dropdownObject.transform.GetChild(2).GetComponent<RectTransform>();
				templateRect.sizeDelta = new Vector2(dropdownPosition.x, dropdownPosition.y * 5);
				templateRect.localPosition = new Vector3(0, -dropdownPosition.y / 2);

				RectTransform viewportRect = templateRect.transform.GetChild(0).GetComponent<RectTransform>();
				viewportRect.sizeDelta = new Vector2(dropdownPosition.x, dropdownPosition.y * 5);
				viewportRect.localPosition = new Vector3(0, -dropdownPosition.y);

				RectTransform contentRect = viewportRect.transform.GetChild(0).GetComponent<RectTransform>();
				contentRect.sizeDelta = dropdownPosition;
				contentRect.localPosition = new Vector3(dropdownPosition.x / 2, 0);
			}
		}

		private void AssignDropDown() {
			Column.onValueChanged.AddListener(delegate {
				selectedColumn = columnOption[SortColumnPanelValue()][Column.value];
			});
			SortOrder.onValueChanged.AddListener(delegate {
				selectedOrder = int.Parse(orderOptions[orderOptions.Length - 1][SortOrder.value]);
			});
		}

		private void SetSelectedValue() {
			selectedColumn = columnOption[SortColumnPanelValue()][Column.value];
			selectedOrder = int.Parse(orderOptions[orderOptions.Length - 1][SortOrder.value]);
		}

		private bool IsSortForCompletedPanel() {
			return TournamentManager.Instance.CurrentSubPanel == PanelNames.MyCompletedPanel;
		}

		private int SortColumnPanelValue() {
			if (isProfileActive) return 2;
			return IsSortForCompletedPanel() ? 1 : 0;
		}

		private void AssignButtons() {
			transform.Find("ResultHeader/CloseButton").GetComponent<Button>().onClick.AddListener(delegate {
				ClosePanel();
			});
			transform.Find("Buttons/Cancel").GetComponent<Button>().onClick.AddListener(delegate {
				ClosePanel();
			});
			transform.Find("Buttons/Apply").GetComponent<Button>().onClick.AddListener(delegate {
				ApplySort();
			});
		}

		private void ClosePanel() {
			gameObject.SetActive(false);
			if (isProfileActive) {
				TacoManager.MyProfilePanel.SetActive(true);
				//TacoManager.ShowPanel(PanelNames.ProfilePanel);
			}
			else {
				TacoManager.MyTournamentsPanel.SetActive(true);
			}
		}

		private void SetUpOptions() {
			Column.ClearOptions();
			SortOrder.ClearOptions();
			Column.AddOptions(columnOption[SortColumnPanelValue()].ToList());
			SortOrder.AddOptions(orderOptions[0].ToList());
			Column.value = 0;
			SortOrder.value = 0;
			SetSelectedValue();
		}

		public void Init(bool isProfileActive) {
			this.isProfileActive = isProfileActive;
			SetUpOptions();
		}

		private void ApplySort() {
			gameObject.SetActive(false);
			TacoManager.MyTournamentsPanel.SetActive(true);
			//TacoManager.TacoBlockingCanvas.SetActive (true);

			Endgame.ListView.ColumnHeaderCollection listviewColumn = null;
			if (TournamentManager.Instance.CurrentSubPanel == PanelNames.MyPublicPanel) {
				listviewColumn = PublicTournamentsList.Instance.ListView.Columns;
			}
			else if (TournamentManager.Instance.CurrentSubPanel == PanelNames.MyPrivatePanel) {
				listviewColumn = PrivateTournamentsList.Instance.ListView.Columns;
			}
			else if (TournamentManager.Instance.CurrentSubPanel == PanelNames.MyCompletedPanel) {
				listviewColumn = CompletedTournamentsList.Instance.ListView.Columns;
			}

			Endgame.ColumnHeader columnHeader = null;
			for (int i = 0; i < listviewColumn.Count; i++) {
				if (listviewColumn[i].Text == selectedColumn) {
					columnHeader = listviewColumn[i];
					break;
				}
			}

			Endgame.ColumnPanel columnPanel = columnHeader.ColumnPanelInHierarchy;
			if (selectedOrder == 0) { //ascending
				if (TacoManager.columnSortName == selectedColumn) TacoManager.columnSortName = "-";
			}
			// desceding
			else {
				TacoManager.columnSortType = 0;
				TacoManager.columnSortName = selectedColumn;
			}
			columnPanel.Button.onClick.Invoke();
		}
	}
}