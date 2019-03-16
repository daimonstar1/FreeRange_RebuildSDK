using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameTaco {
	public class TacoMessagePanel : MonoBehaviour {
		public static TacoMessagePanel Instance;
		public Text TitleText = null;

		void Start() {
			Instance = this;
		}

		public void Open(String title) {
			TitleText.text = title;
		}
	}
}