using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace GameTaco {
	public class ManageTournament : MonoBehaviour {
		public static ManageTournament Instance;
		public Text remainingTime;
		public Text tournamentDetailsText;
		public Text prizePoolText;
		public Text playeredJoinedText;
		public Text winnerText;
		public Text entryFeeText;
		public Text gameName;
		public RectTransform contentPanel;
		public Transform emailsPanel;
		public RectTransform line;
		public Button seeResultButton;
		public Tournament tournament;
		private GameObject joinedPlayerPrefab;
		private List<Image> MoneyTypeImages = new List<Image>();
		private List<TournamentInvite> inviteEmailTexts = new List<TournamentInvite>();
		public List<string> invitedEmails = new List<string>();
		private double countdownTime;


		private void Awake() {
			Instance = this;
			joinedPlayerPrefab = Resources.Load("TournamentManageJoined") as GameObject;
			GameObject[] moneyImageObjects = GameObject.FindGameObjectsWithTag("ManageTournamentMoneyType");

			for (int i = 0; i < moneyImageObjects.Length; i++) {
				MoneyTypeImages.Add(moneyImageObjects[i].GetComponent<Image>());
			}
		}

		private void Start() {
			gameName.text = TacoSetup.Instance.gameName;
			seeResultButton.onClick.AddListener(() => {
				TacoManager.Target = tournament;
				TournamentManager.Instance.ShowTournamentPanel(PanelNames.MyLeaderboardPanel);
			});
		}

		private void Update() {
			if (countdownTime >= 0) {
				TournamentRemainingTime();
			}

		}

		private void UpdateUIValue(ManageTournamentResult data) {
			tournament = data.tournament;
			List<string> joinedPlayers = new List<string>() { TacoManager.User.email };
			List<string> notJoinedPlayers = new List<string>();
			invitedEmails = new List<string>() { TacoManager.User.email };
			for (int i = 0; i < tournament.friendEmails.Length; i++) {
				if (tournament.friendStatuses[i] == "invited") {
					if (!notJoinedPlayers.Contains(tournament.friendEmails[i])) notJoinedPlayers.Add(tournament.friendEmails[i]);
					if (!invitedEmails.Contains(tournament.friendEmails[i])) invitedEmails.Add(tournament.friendEmails[i]);
				}
				else if (tournament.friendStatuses[i] == "accepted") {
					if (!joinedPlayers.Contains(tournament.friendEmails[i])) joinedPlayers.Add(tournament.friendEmails[i]);
					if (!invitedEmails.Contains(tournament.friendEmails[i])) invitedEmails.Add(tournament.friendEmails[i]);
				}
			}
			Debug.Log("tournament.inviteEmails.Length: " + tournament.inviteEmails.Length);
			for (int i = 0; i < tournament.inviteEmails.Length; i++) {
				if(tournament.inviteEmails[i] != String.Empty)
				{
					Debug.Log("tournament.inviteEmails[i]: " + tournament.inviteEmails[i]);
					if (tournament.inviteStatuses[i] == "invited") {
						if (!notJoinedPlayers.Contains(tournament.inviteEmails[i])) notJoinedPlayers.Add(tournament.inviteEmails[i]);
						if (!invitedEmails.Contains(tournament.inviteEmails[i])) invitedEmails.Add(tournament.inviteEmails[i]);
					}
					else if (tournament.inviteStatuses[i] == "accepted") {
						if (!joinedPlayers.Contains(tournament.inviteEmails[i])) joinedPlayers.Add(tournament.inviteEmails[i]);
						if (!invitedEmails.Contains(tournament.inviteEmails[i])) invitedEmails.Add(tournament.inviteEmails[i]);
					}
				}
			}
			string infos = string.Empty;


			for (int i = 0; i < MoneyTypeImages.Count; i++) {
				MoneyTypeImages[i].sprite = TacoConfig.currencySprites[tournament.typeCurrency];
			}

			string createdTime = TacoManager.FormatDate(tournament.createdAt);
			infos += TacoConfig.ManageTournamentCreatedOn.Replace("&day", createdTime);
			infos += TacoConfig.LeaderboardResultTournamentID.Replace("&id", tournament.id.ToString());

			if (tournament.typeCurrency == 0) {
				prizePoolText.text = TacoManager.FormatCash(tournament.prize);
				entryFeeText.text = TacoManager.FormatCash(tournament.entryFee);
			}
			else {
				prizePoolText.text = TacoManager.FormatGTokens(tournament.prize);
				entryFeeText.text = TacoManager.FormatGTokens(tournament.entryFee);
			}
			winnerText.text = TacoConfig.Pluralize(tournament.prize_structure, "Winner");
			playeredJoinedText.text = tournament.memberIds.Length + "/" + tournament.size + " Players Joined";
			tournamentDetailsText.text = infos;
			countdownTime = (double)Mathf.Max((float)tournament.RemainingTimeSpan.TotalSeconds, -1);
			if (countdownTime < 0) remainingTime.text = "00:00:00";

			seeResultButton.interactable = tournament.entryIds.Contains(TacoManager.User.userId);

			//destroy old email fields
			float contentWidth = contentPanel.rect.width;
			for (int i = 1; i < emailsPanel.childCount; i++) {
				Destroy(emailsPanel.GetChild(i).gameObject);
			}

			//generate new email fields
			float spacing = 35;
			float width = (contentPanel.rect.width - spacing);
			float height = 150;

			inviteEmailTexts = new List<TournamentInvite>();

			for (int i = 0; i < notJoinedPlayers.Count; i++) {
				GameObject ob = Instantiate(TacoConfig.NotJoinedPlayerPrefab, Vector3.zero, Quaternion.identity, emailsPanel) as GameObject;
				Text inviteeEmailText = ob.transform.Find("EmailInput/NameInput").GetComponent<Text>();
				inviteeEmailText.text = notJoinedPlayers[i];
				RectTransform r = ob.GetComponent<RectTransform>();
				r.localPosition = new Vector3(0, height * (-0.5f - i) - spacing);
				r.sizeDelta = new Vector2(width - spacing * 5, height - spacing);

				ob.transform.Find("EmailInput/Send").GetComponent<Button>().onClick.AddListener(() => {
					ob.transform.Find("EmailInput/Send").gameObject.SetActive(false);
					Debug.Log("send reminder: " + inviteeEmailText.text);
					TournamentManager.Instance.InviteFriend(inviteeEmailText.text);
					if (!invitedEmails.Contains(inviteeEmailText.text)) invitedEmails.Add(inviteeEmailText.text);
				});

				AddRemoveEmailEvent(ob, r, inviteeEmailText.text);
			}
			float offset = height * (-notJoinedPlayers.Count);


			int newEmailCount = tournament.size - joinedPlayers.Count - notJoinedPlayers.Count;
			for (int i = 0; i < newEmailCount; i++) {
				GameObject ob = Instantiate(TacoConfig.InviteInputPrefab, Vector3.zero, Quaternion.identity, emailsPanel) as GameObject;
				RectTransform r = ob.GetComponent<RectTransform>();
				r.localPosition = new Vector3(0, height * (-0.5f - i) - spacing + offset);
				r.sizeDelta = new Vector2(width - spacing * 5, height - spacing);

				TournamentInvite invite = ob.GetComponent<TournamentInvite>();
				invite.reservedEmails = invitedEmails;
				invite.isInManage = true;
				if (i > 0) {
					inviteEmailTexts[i - 1].next = invite;
					invite.prev = inviteEmailTexts[i - 1];
					invite.SetInteractable(false);
				}

				if (i == 0) {
					invite.SetFocus();
				}

				inviteEmailTexts.Add(invite);

			}
			//fix if invite more people than tournament size
			offset = height * (-0.5f - Mathf.Max((notJoinedPlayers.Count + newEmailCount), notJoinedPlayers.Count));
			Vector3 linePos = new Vector3(0, offset);

			//joined players
			for (int i = 0; i < joinedPlayers.Count; i++) {
				GameObject ob = Instantiate(joinedPlayerPrefab, Vector3.zero, Quaternion.identity, emailsPanel) as GameObject;
				ob.transform.Find("EmailInput/NameInput").GetComponent<Text>().text = joinedPlayers[i];
				RectTransform r = ob.GetComponent<RectTransform>();
				r.localPosition = new Vector3(0, height * (-1.5f - i) - spacing + offset);
				r.sizeDelta = new Vector2(width - spacing * 5, height - spacing);
			}
			contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, Mathf.Max(5 * height, (int)(Mathf.Max(tournament.size, notJoinedPlayers.Count + joinedPlayers.Count) + 4.5f) * height + spacing));
			contentPanel.localPosition = new Vector3(contentPanel.localPosition.x, 0);
			line.localPosition = linePos;
		}

		public void AddRemoveEmailEvent(GameObject ob, RectTransform r, string email, TournamentInvite replaced = null) {
			if (!invitedEmails.Contains(email)) invitedEmails.Add(email);
			if (replaced != null) {
				Destroy(replaced.gameObject);
				inviteEmailTexts.Remove(replaced);
			}
			ob.transform.Find("EmailInput/Remove").GetComponent<Button>().onClick.AddListener(() => {
				Action removeAction = () => {
					Destroy(ob);
					GameObject newOb = Instantiate(TacoConfig.InviteInputPrefab, Vector3.zero, Quaternion.identity, emailsPanel) as GameObject;
					RectTransform newRect = newOb.GetComponent<RectTransform>();

					newRect.localPosition = r.localPosition;
					newRect.sizeDelta = r.sizeDelta;

					List<TournamentInvite> nearPrevs = inviteEmailTexts.Where(x => x.gameObject.transform.position.y > newOb.transform.position.y).ToList();
					List<TournamentInvite> nearNexts = inviteEmailTexts.Where(x => x.gameObject.transform.position.y < newOb.transform.position.y).ToList();
					TournamentInvite newInvite = newOb.GetComponent<TournamentInvite>();
					invitedEmails.Remove(email);
					newInvite.reservedEmails = invitedEmails;
					newInvite.isInManage = true;
					if (nearPrevs.Count > 0) {
						TournamentInvite newPrev = nearPrevs.OrderBy(x => Mathf.Abs(x.gameObject.transform.position.y - newOb.transform.position.y)).First();
						newInvite.prev = newPrev;
						newPrev.next = newInvite;
					}
					if (nearNexts.Count > 0) {
						TournamentInvite newNext = nearNexts.OrderBy(x => Mathf.Abs(x.gameObject.transform.position.y - newOb.transform.position.y)).First();
						newInvite.next = newNext;
						newNext.prev = newInvite;
					}
					inviteEmailTexts.Add(newInvite);
				};
				TacoManager.OpenModalRemovePlayerConfirmPanel(email, removeAction);
			});
		}

		public void LoadInformation(Tournament t) {
			TacoManager.OpenMessage(TacoConfig.TacoRefreshing);

			Action<string> success = (string data) => {
				ManageTournamentResult r = JsonUtility.FromJson<ManageTournamentResult>(data);
				UpdateUIValue(r);

				TacoManager.CloseMessage();
			};

			Action<string, string> fail = (string data, string error) => {
				TacoManager.OpenModalGeneralResultPanel(false, TacoConfig.Error, TacoConfig.TacoTournamentError);
			};

			var url = "api/tournament/getTourById";
			StartCoroutine(ApiManager.Instance.GetManageTournament(url, t.id, success, fail));
		}

		private void TournamentRemainingTime() {
			countdownTime -= Time.deltaTime;
			remainingTime.text = TacoConfig.timerCountdown(countdownTime);
			if (countdownTime <= 0) {
				Debug.Log("Reload page... " + countdownTime);
			}
		}
	}
}
