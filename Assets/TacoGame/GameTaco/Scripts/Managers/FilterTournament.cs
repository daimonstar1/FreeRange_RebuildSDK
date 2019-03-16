using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace GameTaco {
	public class FilterTournament : MonoBehaviour {
		public static FilterTournament Instance;
		public Dropdown PrizeAmount;
		public Dropdown EntryFee;
		public Dropdown NumberOfPlayers;
		public Dropdown NumberOfWinners;
		public Dropdown TournamentTime;

		public Dropdown PlayedAt;
		public Dropdown WinLose;
		public Dropdown Rank;
		public Dropdown PrizePool;
		public Dropdown PrizeWon;

		private int selectedPrize;
		private int selectedFee;
		private int selectedPlayerNb;
		private int selectedWinnerNb;
		private int selectedTime;

		private int seletedPlayedAt;
		private int selectedRank;
		private int selectedWinLose;
		private int selectedPrizePool;
		private int selectedPrizeWon;
		private bool isProfileActive;

		private string[][][] prizeOptions = new string[][][] {
			new string[][] {
				new string[]{ "$1 +", "$2 +", "$3 +", "$5 +", "$10 +" },
				new string[]{ "1", "2", "3", "5", "10" }
			},
			new string[][] {
				new string[]{ "20 +", "40 +", "80 +", "120 +", "240 +" },
				new string[]{ "20", "40", "80", "120", "240" }
			}

		};
		private string[][][] feeOptions = new string[][][] {
			new string[][]{
				new string[]{ "$1 +", "$2 +", "$3 +", "$5 +", "$10 +" },
				new string[]{ "1", "2", "3", "5", "10" }
			},
			new string[][]{
				new string[]{ "20 +", "40 +", "60 +", "100 +", "200 +" },
				new string[]{ "20", "40", "60", "100", "200" }
			}
		};
		private string[][] playerNbOptions = new string[][] {
			new string[]{ "2 +", "3 +", "5 +", "10 +", "100 +" },
			new string[]{ "2", "3", "5", "10", "100" }
		};
		private string[][] winnerNbOptions = new string[][] {
			new string[]{ "1", "2", "3", "5", "10" }
		};
		private string[][] timeOptions = new string[][] {
			new string[]{ "1 >", "10hrs >", "24 hours >", "48 hours >", "72 hours >", "120 hours >", "240 hours >" },
			new string[]{ "1","10", "24", "48", "72", "120", "240" }
		};

		private string[][] playedAtOptions = new string[][] {
			new string[]{ "All Time", "Within 24 hours", "Within 48 hours", "Within 72 hours", "Within 120 hours", "Within 240 hours" },
			new string[]{ "-9999", "24", "48", "72", "120", "240" }
		};
		private string[][] rankOptions = new string[][] {
			new string[]{ "All", "1", "2 +", "3 +", "5 +", "10 +", "100 +" },
			new string[]{ "-9999", "1", "2", "3", "5", "10", "100" }
		};
		private string[][] winLoseOptions = new string[][] {
			new string[]{ "All", "-", "Win", "Loss"},
			new string[]{ "-9999", "0", "1", "-1"}
		};
		private string[][] prizePoolOptions = new string[][] {
			new string[]{ "All", "2 +", "3 +", "5 +", "10 +", "100 +" },
			new string[]{ "-9999", "2", "3", "5", "10", "100" }
		};
		private string[][] prizeWinOptions = new string[][] {
			new string[]{ "All", "1 +", "2 +", "3 +", "5 +", "10 +", "100 +" },
			new string[]{ "-9999", "1", "2", "3", "5", "10", "100" }
		};

		private void Awake() {
			Instance = this;
			SetDropdownHeight();
			AssignButtons();
			AssignDropDown();
		}

		private void SetDropdownHeight() {
			List<Dropdown> dropdownList = new List<Dropdown>() {
				PrizeAmount, EntryFee, NumberOfPlayers, NumberOfWinners, TournamentTime, PlayedAt, Rank, PrizePool, PrizeWon, WinLose
			};
			// Set dropdown height
			for (int i = 0; i < dropdownList.Count; i++) {
				GameObject dropdownObject = dropdownList[i].gameObject;
				Vector2 dropdownPosition = dropdownObject.GetComponent<RectTransform>().sizeDelta;
				/*RectTransform label = dropdownObject.transform.GetChild (0).GetComponent<RectTransform> ();
				label.sizeDelta = dropdownPosition;
				label.localPosition = new Vector2 ( 30 - dropdownPosition.x / 2, 0);*/

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

		private void AssignButtons() {
			transform.Find("ResultHeader/CloseButton").GetComponent<Button>().onClick.AddListener(delegate {
				ClosePanel();
			});
			transform.Find("Buttons/Cancel").GetComponent<Button>().onClick.AddListener(delegate {
				ClosePanel();
			});
			transform.Find("Buttons/Apply").GetComponent<Button>().onClick.AddListener(delegate {
				ApplyFilter();
			});
		}

		private void ClosePanel() {
			gameObject.SetActive(false);
			ToggleAllOptions(false);
			if (isProfileActive) {
				TacoManager.MyProfilePanel.SetActive(true);
				//TacoManager.ShowPanel(PanelNames.ProfilePanel);
			}
			else {
				TacoManager.MyTournamentsPanel.SetActive(true);
			}
		}

		private void AssignDropDown() {
			PrizeAmount.onValueChanged.AddListener(delegate {
				selectedPrize = int.Parse(prizeOptions[(int)TacoSetup.Instance.TournamentCategory][1][PrizeAmount.value]);
			});
			EntryFee.onValueChanged.AddListener(delegate {
				selectedFee = int.Parse(feeOptions[(int)TacoSetup.Instance.TournamentCategory][1][EntryFee.value]);
			});
			NumberOfPlayers.onValueChanged.AddListener(delegate {
				selectedPlayerNb = int.Parse(playerNbOptions[playerNbOptions.Length - 1][NumberOfPlayers.value]);
			});
			NumberOfWinners.onValueChanged.AddListener(delegate {
				selectedWinnerNb = int.Parse(winnerNbOptions[winnerNbOptions.Length - 1][NumberOfWinners.value]);
			});
			TournamentTime.onValueChanged.AddListener(delegate {
				selectedTime = int.Parse(timeOptions[timeOptions.Length - 1][TournamentTime.value]);
			});


			PlayedAt.onValueChanged.AddListener(delegate {
				seletedPlayedAt = int.Parse(playedAtOptions[playedAtOptions.Length - 1][PlayedAt.value]);
			});
			WinLose.onValueChanged.AddListener(delegate {
				selectedWinLose = int.Parse(winLoseOptions[winLoseOptions.Length - 1][WinLose.value]);
			});
			Rank.onValueChanged.AddListener(delegate {
				selectedRank = int.Parse(rankOptions[rankOptions.Length - 1][Rank.value]);
			});
			PrizePool.onValueChanged.AddListener(delegate {
				selectedPrizePool = int.Parse(prizePoolOptions[prizePoolOptions.Length - 1][PrizePool.value]);
			});
			PrizeWon.onValueChanged.AddListener(delegate {
				selectedPrizeWon = int.Parse(prizeWinOptions[prizeWinOptions.Length - 1][PrizeWon.value]);
			});
		}

		private void SetSelectedValue() {
			SetSelectedValueForCompleted();
			SetSelectedValueForGeneral();
		}

		private void SetSelectedValueForCompleted() {
			seletedPlayedAt = int.Parse(playedAtOptions[playedAtOptions.Length - 1][PlayedAt.value]);
			selectedRank = int.Parse(rankOptions[rankOptions.Length - 1][WinLose.value]);
			selectedWinLose = int.Parse(winLoseOptions[winLoseOptions.Length - 1][Rank.value]);
			selectedPrizePool = int.Parse(prizePoolOptions[prizePoolOptions.Length - 1][PrizePool.value]);
			selectedPrizeWon = int.Parse(prizeWinOptions[prizeWinOptions.Length - 1][PrizeWon.value]);
		}

		private void SetSelectedValueForGeneral() {
			selectedPrize = int.Parse(prizeOptions[(int)TacoSetup.Instance.TournamentCategory][1][PrizeAmount.value]);
			selectedFee = int.Parse(feeOptions[(int)TacoSetup.Instance.TournamentCategory][1][EntryFee.value]);
			selectedPlayerNb = int.Parse(playerNbOptions[playerNbOptions.Length - 1][NumberOfPlayers.value]);
			selectedWinnerNb = int.Parse(winnerNbOptions[winnerNbOptions.Length - 1][NumberOfWinners.value]);
			selectedTime = int.Parse(timeOptions[timeOptions.Length - 1][TournamentTime.value]);
		}

		private void ToggleAllOptions(bool status) {
			ToggleGeneralOptions(status);
			ToggleCompletedOptions(status);
		}

		private void ToggleCompletedOptions(bool status) {
			PlayedAt.transform.parent.gameObject.SetActive(status);
			Rank.transform.parent.gameObject.SetActive(status);
			PrizePool.transform.parent.gameObject.SetActive(status);
			PrizeWon.transform.parent.gameObject.SetActive(status);
			WinLose.transform.parent.gameObject.SetActive(status);
		}

		private void ToggleGeneralOptions(bool status) {
			PrizeAmount.transform.parent.gameObject.SetActive(status);
			EntryFee.transform.parent.gameObject.SetActive(status);
			NumberOfPlayers.transform.parent.gameObject.SetActive(status);
			NumberOfWinners.transform.parent.gameObject.SetActive(status);
			TournamentTime.transform.parent.gameObject.SetActive(status);
		}

		private void SetUpOptionsForGeneral() {
			ToggleGeneralOptions(true);
			PrizeAmount.ClearOptions();
			EntryFee.ClearOptions();
			NumberOfPlayers.ClearOptions();
			NumberOfWinners.ClearOptions();
			TournamentTime.ClearOptions();
			PrizeAmount.AddOptions(prizeOptions[(int)TacoSetup.Instance.TournamentCategory][0].ToList());
			EntryFee.AddOptions(feeOptions[(int)TacoSetup.Instance.TournamentCategory][0].ToList());
			NumberOfPlayers.AddOptions(playerNbOptions[0].ToList());
			NumberOfWinners.AddOptions(winnerNbOptions[0].ToList());
			TournamentTime.AddOptions(timeOptions[0].ToList());
			PrizeAmount.value = 0;
			EntryFee.value = 0;
			NumberOfPlayers.value = 0;
			NumberOfWinners.value = 0;
			TournamentTime.value = 0;
			SetSelectedValueForGeneral();
		}

		private void SetUpOptionsForCompleted() {
			ToggleCompletedOptions(true);
			PlayedAt.ClearOptions();
			Rank.ClearOptions();
			PrizePool.ClearOptions();
			PrizeWon.ClearOptions();
			WinLose.ClearOptions();
			PlayedAt.AddOptions(playedAtOptions[0].ToList());
			Rank.AddOptions(rankOptions[0].ToList());
			PrizePool.AddOptions(prizePoolOptions[0].ToList());
			PrizeWon.AddOptions(prizeWinOptions[0].ToList());
			WinLose.AddOptions(winLoseOptions[0].ToList());
			PlayedAt.value = 0;
			Rank.value = 0;
			PrizePool.value = 0;
			PrizeWon.value = 0;
			WinLose.value = 0;
			SetSelectedValueForCompleted();
		}

		public void Init(bool isProfileActive) {
			this.isProfileActive = isProfileActive;
			if (IsFilterForCompletedPanel()) {
				SetUpOptionsForCompleted();
			}
			else {
				SetUpOptionsForGeneral();
			}
		}

		private bool IsFilterForCompletedPanel() {
			return TournamentManager.Instance.CurrentSubPanel == PanelNames.MyCompletedPanel;
		}

		private void ApplyFilter() {
			ToggleAllOptions(false);
			Func<Tournament, bool> filterCondition = null;
			if (IsFilterForCompletedPanel()) {
				filterCondition = delegate (Tournament t) {
					bool playedDate = seletedPlayedAt < -1000 ? true : (t.PlayedAtToDateTimeSpan <= new TimeSpan(0, seletedPlayedAt, 0, 0));
					bool rank = true;//default all
					bool placement = selectedRank < -1000 ? true : (!string.IsNullOrEmpty(t.endDate) && t.rank <= selectedRank);
					bool prizePool = selectedPrizePool < -1000 ? true : (t.prize * t.prize_structure >= selectedPrizePool);
					bool prizeWon = selectedPrizeWon < -1000 ? true : (!string.IsNullOrEmpty(t.endDate) && t.rank <= t.prize_structure && t.prize >= selectedPrizeWon);

					if (selectedWinLose == -1) {
						rank = !string.IsNullOrEmpty(t.endDate) && t.rank > t.prize_structure;
					}
					else if (selectedWinLose == 1) {
						rank = !string.IsNullOrEmpty(t.endDate) && t.rank <= t.prize_structure;
					}
					else if (selectedWinLose == 0) {
						rank = string.IsNullOrEmpty(t.endDate);
					}
					//Debug.Log("selectedWinLose: " + selectedWinLose + "playedDate: " + playedDate + " - rank: " + rank + " - placement: " + placement + " - prizePool: " + prizePool + " - prizeWon: " + prizeWon);
					return playedDate && rank && placement && prizePool && prizeWon;
				};

			}
			else {
				filterCondition = delegate (Tournament t) {
					return t.entryFee >= (double)selectedFee && t.prize >= (double)selectedPrize && t.size >= selectedPlayerNb
					&& t.prize_structure >= selectedWinnerNb && t.RemainingTimeSpan > new TimeSpan(0, selectedTime, 0, 0);
				};
			}
			TournamentManager.Instance.ShowTournamentPanel(null, filterCondition);
			gameObject.SetActive(false);
			TacoManager.TacoBlockingCanvas.SetActive(true);
		}
	}
}