using UnityEngine;
using UnityEngine.UI;

namespace GameTaco {

	public class LeaderboardItemButton : MonoBehaviour {
		public Text playerName;
		public Image avatarSprite;

		public void Setup(string rank, string url, string name, int fontSize) {
			playerName.text = name;
			playerName.fontSize = fontSize;

			if (url != null) {
				if (!TacoManager.cacheAvatars.ContainsKey(url)) {
					TacoSetup.Instance.StartCoroutine(ApiManager.Instance.WWWAvatarLeaderboard(url, avatarSprite));
				}
				else {
					avatarSprite.sprite = TacoManager.cacheAvatars[url];
				}
			}
		}
	}

}