using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTacoDelegate : MonoBehaviour {
	public static GameTacoDelegate Instance;

	private void Awake() {
		DontDestroyOnLoad(this.gameObject);
		Instance = this;
	}

	private void Start() {
		GameTaco.TacoSetup.Instance.ToggleButtonWhenLogin += ToggleButtonWhenLogin;
		GameTaco.TacoSetup.Instance.UpdateMoneyValue += SetMoneytext;
		GameTaco.TacoSetup.Instance.BackToMainMenu += GoToMainMenu;
	}

	private static GameObject AuthMainMenu;
	private static GameObject MoneyMainMenu;

	List<Text> CreateTournamentTacoTexts = new List<Text>();
	List<Text> CreateTournamentCashTexts = new List<Text>();
	List<Text> CreateTournamentRPTexts = new List<Text>();

	public void Init() {
		InitGameTacoObjects();
	}

	private void InitGameTacoObjects() {
		CreateTournamentTacoTexts = new List<Text>();
		CreateTournamentCashTexts = new List<Text>();
		CreateTournamentRPTexts = new List<Text>();
		GetMoneyTextAndSetButton();
		AuthMainMenu = GameObject.Find("AuthMainMenu");
		MoneyMainMenu = GameObject.Find("MoneyMainMenu");
		if (GameTaco.TacoSetup.Instance.IsLoggedIn()) {
			SetMoneytext();
			ToggleButtonWhenLogin(true);
		}
		else {
			ToggleButtonWhenLogin(false);
		}
	}

	public void ToggleButtonWhenLogin(bool display) {
		if (MoneyMainMenu != null) {
			MoneyMainMenu.SetActive(display);
			AuthMainMenu.SetActive(!display);
		}
	}

	private void GetMoneyTextAndSetButton() {
		GameObject[] list = GameObject.FindGameObjectsWithTag("TacoValueText");
		for (int i = 0; i < list.Length; i++) {
			CreateTournamentTacoTexts.Add(list[i].GetComponent<Text>());
			Button btn = list[i].transform.parent.parent.GetComponent<Button>();
			if (btn != null) {
				btn.onClick.AddListener(delegate {
					GameTaco.BalanceManager.Instance.Init(1);
				});
			}
		}
		list = GameObject.FindGameObjectsWithTag("CashValueText");
		for (int i = 0; i < list.Length; i++) {
			CreateTournamentCashTexts.Add(list[i].GetComponent<Text>());
			Button btn = list[i].transform.parent.parent.GetComponent<Button>();
			if (btn != null) {
				btn.onClick.AddListener(delegate {
					GameTaco.BalanceManager.Instance.Init(0);
				});
			}
		}
		list = GameObject.FindGameObjectsWithTag("RPValueText");
		for (int i = 0; i < list.Length; i++) {
			CreateTournamentRPTexts.Add(list[i].GetComponent<Text>());
			Button btn = list[i].transform.parent.parent.GetComponent<Button>();
			if (btn != null) {
				btn.onClick.AddListener(delegate {
					GameTaco.BalanceManager.Instance.Init(2);
				});
			}
		}
	}

	public void SetMoneytext() {
		for (int i = 0; i < CreateTournamentTacoTexts.Count; i++) {
			if (CreateTournamentTacoTexts[i] != null)
				CreateTournamentTacoTexts[i].text = GameTaco.TacoManager.FormatGTokens(double.Parse(GameTaco.TacoManager.User.gToken));
		}
		for (int i = 0; i < CreateTournamentCashTexts.Count; i++) {
			if (CreateTournamentCashTexts[i] != null)
				CreateTournamentCashTexts[i].text = GameTaco.TacoManager.FormatCash(GameTaco.TacoManager.User.funds);
		}
		for (int i = 0; i < CreateTournamentRPTexts.Count; i++) {
			if (CreateTournamentRPTexts[i] != null)
				CreateTournamentRPTexts[i].text = GameTaco.TacoManager.FormatRP(GameTaco.TacoManager.User.ticket);
		}
	}

	private void GoToMainMenu() {
		GameTaco.TacoSetup.Instance.ToggleTacoHeaderFooter(true);
		//UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
	}
}
