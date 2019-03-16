using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

namespace GameTaco {
	public class TournamentDetails : MonoBehaviour {

		public Text DescriptionText;
		public Text PlayersText;
		public Text PrizeText;

		// Use this for initialization
		public void UpdateDetails(string descriptionText, string playersText, string prizeText) {
			DescriptionText.text = descriptionText;
			PlayersText.text = playersText;
			PrizeText.text = prizeText;
		}
	}
}
