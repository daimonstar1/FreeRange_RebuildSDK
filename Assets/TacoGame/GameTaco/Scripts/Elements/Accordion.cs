using System.Collections;
using UnityEngine.UI;
using UnityEngine;

namespace GameTaco {
	public class Accordion : MonoBehaviour {
		public Accordion prev;
		public Accordion next;
		public RectTransform content;
		private Button button;
		public bool isActive;
		public bool isRunningAnimation;
		public Transform sign;

		void Awake() {
			transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => {
				if (content != null && !IsAnimationStillRunning()) {
					CloseCurrentStretch();
					if (isActive) ShrinkContent();
					else StretchContent();
				}
			});
		}

		private void UpdateParent(float height) {
			ProfileManager.Instance.StretchContent(height);
		}

		public void StretchContent() {
			UpdateParent(content.rect.height);
			isActive = true;
			sign.eulerAngles = new Vector3(0, 0, -90);
			if (next != null && next.gameObject.activeSelf) next.DropPosition(content.rect.height);
		}

		public void ShrinkContent() {
			UpdateParent(-content.rect.height);
			isActive = false;
			sign.eulerAngles = new Vector3(0, 0, 0);
			if (next != null && next.gameObject.activeSelf) next.ShrinkPosition(content.rect.height);
		}

		public void DropPosition(float height) {
			if (next != null && next.gameObject.activeSelf) next.DropPosition(height);
			StartCoroutine(StartDropAnimation(height));
		}

		public void ShrinkPosition(float height) {
			if (next != null && next.gameObject.activeSelf) next.ShrinkPosition(height);
			StartCoroutine(StartShrinkAnimation(height));
		}

		private IEnumerator StartDropAnimation(float height) {
			isRunningAnimation = true;
			RectTransform rect = GetComponent<RectTransform>();
			float newPosY = rect.localPosition.y - height;
			while (rect.localPosition.y > newPosY) {
				yield return new WaitForSeconds(TacoConfig.accodionSpeed);
				rect.localPosition -= TacoConfig.accodionDistance;
			}
			rect.localPosition = new Vector3(rect.localPosition.x, newPosY);
			isRunningAnimation = false;
		}

		private IEnumerator StartShrinkAnimation(float height) {
			isRunningAnimation = true;
			RectTransform rect = GetComponent<RectTransform>();
			float newPosY = rect.localPosition.y + height;
			while (rect.localPosition.y < newPosY) {
				yield return new WaitForSeconds(TacoConfig.accodionSpeed);
				rect.localPosition += TacoConfig.accodionDistance;
			}
			rect.localPosition = new Vector3(rect.localPosition.x, newPosY);
			isRunningAnimation = false;
		}

		public void ShrinkImmediately(float height) {
			RectTransform rect = GetComponent<RectTransform>();
			rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y + height);
		}

		private Accordion currentActive() {
			Accordion current = next;
			while (current != null && current.gameObject.activeSelf) {
				if (current.isActive) {
					return current;
				}
				current = current.next;
			}
			current = prev;
			while (current != null && current.gameObject.activeSelf) {
				if (current.isActive) {
					return current;
				}
				current = current.prev;
			}
			return null;
		}

		public void CloseCurrentStretch() {
			Accordion current = currentActive();
			if (current == null) return;
			current.isActive = false;
			current.sign.eulerAngles = new Vector3(0, 0, 0);
			float height = current.content.rect.height;
			ProfileManager.Instance.ShrinkImmediately(height);
			Accordion nextOne = current.next;
			while (nextOne != null && nextOne.gameObject.activeSelf) {
				nextOne.ShrinkImmediately(height);
				nextOne = nextOne.next;
			}
		}

		private bool IsAnimationStillRunning() {
			if (isRunningAnimation) return true;
			Accordion current = next;
			while (current != null && current.gameObject.activeSelf) {
				if (current.isRunningAnimation) return true;
				current = current.next;
			}
			current = prev;
			while (current != null && current.gameObject.activeSelf) {
				if (current.isRunningAnimation) return true;
				current = current.prev;
			}
			return false;
		}
	}
}