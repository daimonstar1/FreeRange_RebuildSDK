using UnityEngine.UI;
using UnityEngine;

namespace GameTaco {
	public class OurGame : MonoBehaviour {
		public string type;
		private string link;

		public string Link {
			set { link = value; }
		}

		private void Start() {
			transform.Find ("Background/Image").GetComponent<Button> ().onClick.AddListener (delegate {
				Application.OpenURL(System.Uri.EscapeUriString(link));
			});
		}
		public void UpdateNameTextAndImage(string name){
			transform.Find ("Background/NameText").GetComponent<Text> ().text = name;
			transform.Find ("Background/Image").GetComponent<Image> ().sprite = TacoConfig.ourGameSprites [name];
		}

		public void UpdateTournamentText(int nb){
			transform.Find ("Background/TournamentText").GetComponent<Text> ().text = TacoConfig.Pluralize (nb, TacoConfig.ActiveTournament);
		}
	}
}