using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameTaco;

namespace FRG.Taco
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Button button_login;
        [SerializeField] private Button button_register;
        [SerializeField] private Button button_play;
        [SerializeField] private Button button_showTournaments;
        [SerializeField] private GameObject panel_loggedIn;
        [SerializeField] private GameObject panel_notLoggedIn;
        [SerializeField] private Button button_cash;
        [SerializeField] private Button button_tokens;
        [SerializeField] private Button button_tickets;
        [SerializeField] private TMP_Text text_cash;
        [SerializeField] private TMP_Text text_tokens;
        [SerializeField] private TMP_Text text_tickets;
        public static bool first_load = true,game_offline=false;
        private void OnEnable()
        {
            GameTaco.TacoSetup.Instance.ToggleButtonWhenLogin += OnLogin;

            GameTaco.TacoSetup.Instance.ToggleTacoHeaderFooter(true);
            bool showLogin = !GameTaco.TacoSetup.Instance.IsLoggedIn();

            button_login.onClick.AddListener(OpenLoginPanel);
            button_register.onClick.AddListener(OpenRegisterPanel);

            button_play.onClick.AddListener(PlayOfflineGame);
            button_showTournaments.onClick.AddListener(openTacoTournament);
            button_cash.onClick.AddListener(() => showCashLayout());
            button_tokens.onClick.AddListener(() => showTokenLayout());
            button_tickets.onClick.AddListener(() => showTicketLayout());

            TacoSetup.Instance.TournamentStarted += PlayTacoTournament;
            RefreshDisplay();
            if (game_offline)
            {
                showMainMenuUI();
                game_offline = false;
            }
            else if (!first_load)
            {
                hideMainMenuUI();
                TacoSetup.Instance.BackToMainMenu += GoToMainMenu;
            }
            if (first_load)
                first_load = false;
        }
        private void showTicketLayout()
        {
            BalanceManager.Instance.back += showTicketLayout_back;
            BalanceManager.Instance.Init(2);
            hideMainMenuUI();

        }
        private void showTicketLayout_back()
        {
            BalanceManager.Instance.back -= showTicketLayout_back;
            showMainMenuUI();
        }
        private void showTokenLayout()
        {
            BalanceManager.Instance.back += showTokenLayout_back;
            BalanceManager.Instance.Init(1);
            hideMainMenuUI();
        }
        private void showTokenLayout_back()
        {
            BalanceManager.Instance.back -= showTokenLayout_back;
            showMainMenuUI();
        }
        private void showCashLayout()
        {
            BalanceManager.Instance.back += showCashLayout_back;
            BalanceManager.Instance.Init(0);
            hideMainMenuUI();
        }
        private void showCashLayout_back()
        {
            BalanceManager.Instance.back -= showCashLayout_back;
            showMainMenuUI();
        }
        private void OpenLoginPanel()
        {
            TacoSetup.Instance.BackToMainMenu += GoToMainMenu;
            TacoSetup.Instance.OpenLoginPanel();
            hideMainMenuUI();
        }
        private void OpenRegisterPanel()
        {
            TacoSetup.Instance.BackToMainMenu += GoToMainMenu;
            TacoSetup.Instance.OpenRegisterPanel();
            hideMainMenuUI();
        }
        private void openTacoTournament()
        {
            TacoSetup.Instance.BackToMainMenu += GoToMainMenu;
            TacoSetup.Instance.OpenTacoTournament();
            hideMainMenuUI();


        }
        private void hideMainMenuUI()
        {
            transform.Find("Canvas_MainMenu").gameObject.SetActive(false);
            transform.Find("spawner_Logo3d").gameObject.SetActive(false);
        }
        private void showMainMenuUI()
        {
            Debug.LogError("showMainMenuUI!!");
            transform.Find("Canvas_MainMenu").gameObject.SetActive(true);
            transform.Find("spawner_Logo3d").gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            button_login.onClick.RemoveAllListeners();
            button_register.onClick.RemoveAllListeners();
            button_play.onClick.RemoveAllListeners();
            button_cash.onClick.RemoveAllListeners();
            button_tokens.onClick.RemoveAllListeners();
            button_tickets.onClick.RemoveAllListeners();

            TacoSetup.Instance.TournamentStarted -= PlayTacoTournament;
        }
        private void GoToMainMenu()
        {
            Debug.LogError("GoToMainMenu");
            TacoSetup.Instance.BackToMainMenu -= GoToMainMenu;
            showMainMenuUI();
        }
        private void RefreshDisplay()
        {
            bool isLoggedIn = GameTaco.TacoSetup.Instance.IsLoggedIn();
            panel_loggedIn.SetActive(isLoggedIn);
            panel_notLoggedIn.SetActive(!isLoggedIn);

            if (isLoggedIn && TacoManager.User != null)
            {
                string cash = TacoManager.FormatCash(TacoManager.User.TotalCash);
                string tokens = TacoManager.User.gToken;
                string tickets = TacoManager.User.ticket;
                text_cash.text = cash;
                text_tokens.text = tokens;
                text_tickets.text = tickets;
            }
        }

        private void OnLogin(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                Debug.Log("User has logged in!");
            }
            else
            {
                Debug.Log("User has not logged in yet.");
            }

            RefreshDisplay();
        }

        private void PlayTacoTournament()
        {
            MainGameLogic.instance.GoToGameplay(MainGameLogic.GameMode.GameTacoTournament);
        }

        private void PlayOfflineGame()
        {
            game_offline = true;
            MainGameLogic.instance.GoToGameplay(MainGameLogic.GameMode.Offline);
        }
    }
}
