using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace GameTaco {
	public class TacoLabel : MonoBehaviour {

		public bool SetLocalizedKey = true;
		public Text LabelText = null;

		// TODO : turn these into pointers to CSS styles
		// TODO : load a CSS file and parse that 
		public enum CSS { DefaultFont, HeaderFont, BodyFont, InputFont, MessageFont };
		public CSS css;

		void Start() {

			// if the user pointed to the text, get the localized string  use the name of the button as the key
			if (LabelText && SetLocalizedKey) {
				SetKey(gameObject.name);
			}

			if (LabelText && css != CSS.DefaultFont) {
				SetTextStyle(css);
			}
		}

		public void SetTextStyle(CSS css) {

			switch (css) {

				case CSS.BodyFont:
					LabelText.fontSize = TacoConfig.Instance.BodyFont.Size;
					break;

				case CSS.HeaderFont:
					LabelText.fontSize = TacoConfig.Instance.HeaderFont.Size;
					break;

				case CSS.InputFont:
					LabelText.fontSize = TacoConfig.Instance.InputFont.Size;
					break;

				case CSS.MessageFont:
					LabelText.fontSize = TacoConfig.Instance.MessageFont.Size;
					break;

			}
		}


		public void SetKey(string key) {
			string labelText = TacoConfig.GetValue(key);
			SetText(labelText);
		}

		public void SetText(string text) {
			LabelText.text = text;
		}
	}
}
