using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System.Collections;

namespace GameTaco {

	public class TacoInputField : InputField {
		UnityEvent m_MyEvent;

		void ClearPlaceHolder() {
			Text placeholder = this.placeholder.GetComponent<Text>();

			placeholder.enabled = false;

			/*
			if (input.text.Length > 0) 
			{
				Debug.Log("Text has been entered");
			}
			else if (input.text.Length == 0) 
			{
				Debug.Log("Main Input Empty");
			}
			*/
		}

		// TODO Does this cause a performance issue? 
		void Update() {
			if (this.isFocused && m_MyEvent != null) {
				m_MyEvent.Invoke();
			}
		}

		protected override void Start() {
			if (m_MyEvent == null)
				m_MyEvent = new UnityEvent();

			m_MyEvent.AddListener(delegate { ClearPlaceHolder(); });
		}
	}
}
