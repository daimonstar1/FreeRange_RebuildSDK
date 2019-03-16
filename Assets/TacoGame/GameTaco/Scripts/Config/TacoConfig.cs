// Const values that will probably change per game implementation or might be customized or localized 

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace GameTaco
{
	public class TacoConfig : MonoBehaviour
	{
		public static TacoConfig Instance;
		public static int waitingPlayCountdownTime = 5;

		public static GameObject InviteInputPrefab;
		public static GameObject NotJoinedPlayerPrefab;
		public static GameObject DetailCellPrefab;

		// static sprites that will be loaded from resources folder
		public static Sprite TacoIntroGraphic;
		public static Sprite ModalSprite;
		public static Sprite LoginSprite;
		public static Sprite SignUpSprite;
		public static Sprite OptionalModalSprite;
		public static Sprite CloseSprite;
		public static Sprite PlaySprite;
		public static Sprite JoinSprite;
		public static Sprite InviteSprite;
		public static Sprite FirstPlaceSprite;
		public static Sprite SecondPlaceSprite;
		public static Sprite ThirdPlaceSprite;
		public static Sprite OtherPlaceSprite;
		public static Sprite NAPlaceSprite;
		public static Sprite NotSelected;
		public static Sprite WinSprite;
		public static Sprite LossSprite;
		public static Sprite HandRightIcon;
		public static Sprite UserDefault;
		public static Sprite Avatar00;
		public static Sprite Avatar01;
		public static Sprite Avatar02;
		public static Sprite Avatar03;
		public static Sprite Avatar04;
		public static Sprite Avatar05;
		public static Sprite Avatar06;
		public static Sprite Avatar07;

		public static Sprite IconCheck;
		public static Sprite IconWarning;
		public static Sprite IconCash;
		public static Sprite IconGTaco;
		public static Sprite IconRP;
		public static Sprite IconTournamentTime;
		public static Sprite IconTournamentUser;
		public static Sprite IconTournamentWinner;
		public static Sprite EmailIconInactive;
		public static Sprite EmailIconActive;
		public static Sprite EmailIconFocus;
		public static Sprite EmailIconRemove;

		public static Sprite[] currencySprites = new Sprite[]{};
		public static Sprite[] sortSprites = new Sprite[]{};
		public static Sprite[] emailStatusSprites = new Sprite[]{};
		public static Sprite[] currencyBrokenSprites = new Sprite[]{};
		public static Dictionary<string, Sprite> ourGameSprites = new Dictionary<string, Sprite>();
		// to hold strings
		public static Hashtable TacoStrings = new Hashtable ();

		// allows developers to squelch our debug spam
		public static bool showDebugLog = true;


		public const string HidedNumber = "-";
		public const string FinishStatus = "Finish!";
		public const string EndStatus = "Finish!";
		// text format strings

		public const string TacoRegisteredErrorMessage08 = "Please enter a confirm password.";
		public const int gTokenExchange = 1;
		public const float cashReduce = 0.9f;

		public const string Players = "players";
		public const string ActiveTournament = "Active Tournament";
		public const string YourFundsHeader = "Your Funds";
		public const string YourTokensHeader = "Your Tokens";
		public const string PrizesHeader = "Prizes";

		public static string CreateTournamentNotice = "To enter this <b>'&gameName Public Tournament'</b> the Taco Entry Fee will be removed to your account.";
		public static string TournamentTypeNotice = "'&gameName &type Tournament'";
		public const string NoResults = "No Tournaments";
		public const string GamePlayingMessage = "Playing Tournament Game!";

		public const string TacoIntroHeader = "Start challenging your friends for real money.";
		public const string TacoIntroBody = "Login or register to challenge today";

		public const string TacoSending = "Sending...";
		public const string TacoRefreshing = "Refreshing...";
		public const string Error = "Sorry, Something Went Wrong";

		public const string TacoQuitGameHeader = "Quit Tournament game?";
		public const string TacoQuitGameBody = "Are you sure you want to quit this tournament game and forfeit your round?";

		// help
		public const string TacoHelpLoginTitle = "Sign-Up and Join Today";
		public const string TacoHelpLoginBody = "GameTaco let's you bring your skills to battle! To battle your friends for real money!\n\nAll it takes is a quick Sign-Up and withdraw some cash to get started. Withdrawls are done with Stripe.";

		// headers
		public const string TacoFundsHeader = "Add or Withdraw Funds";
		public const string TacoOpenButtonMessage = "Play Tournaments Today!";
		public const string ClaimSuccessHeader = "Claim token success";
		public const string ClaimFailHeader = "Claim token fail";
		public const string ClaimSuccessNotice = "Your taco token has been updated";
		public const string NoTokenNotice = "No more tokens to claim";
		public const string ClaimErrorNotice = "Claim token error";

		public const string TacoOurGamesHeader = "Game Taco Games!";
		public const string TacoOurGamesMessage = "Take a look at all of our games.";
		public const string TacoRewardHeaderError = "Not Enough Reward Point";
		public const string TacoRewardErrorMsg = "You have not enough Reward Point.";
		public const string NoDefaultAddress = "We see you don't have an address on file,\nLets add one to send your funds to";

		// login
		public const string TacoRegisterEmail = "EMAIL";
		public const string TacoRegisterPassword = "PASSWORD";
		public const string TacoLoginMessage = "Practice, Compete - Win $$";
		public const string TacoLoginEmail = "ENTER EMAIL";
		public const string TacoLoginPassword = "ENTER PASSWORD";
		public const string TacoLoginConfirm = "CONFIRM PASSWORD";
		public const string TacoLoginUser = "USER NAME";
		public const string ReferenceCode = "REFERENCE CODE";

		//public const string TacoLoginErrorHeader = "Login Failed";
		public const string TacoVersionErrorHeader = "New version available";
		public const string TacoLoginErrorMessage00 = "An email and password is required";
		public const string TacoLoginErrorEmailPassword = "Email/Password incorrect";
		public const string TacoLoginErrorMessage02 = "Please confirm your password";
		public const string TacoLoginErrorMessage03 = "Password and Confirm Password must match";
		public const string TacoLoginErrorMessage04 = "Please confirm age to continue";
		public const string TacoLoginErrorMessage05 = "An unexpected error occurred";
		public const string TacoLoginErrorSomethingWrong = "Something went wrong. Please try again!";
		public const string TacoLoginErrorTimeout = "Please try again! The session is timeout";
		public const string TacoLoginStatusMessage00 = "Logging in...";
		public const string TacoLoginStatusNoInternet = "Please connect internet before login.";
		public const string TacoLoginStatusMessage01 = "Login Complete";
		public const string ErrorStatusNoInternet = "Please connect internet";

		public const string TacoRegisteredStatusMessage00 = "Registering...";

		public const string TacoRegisteredModalTitle = "Account Created";

		public const string TacoRegisteredModalBody = "Your account has been created.";
		public const string TacoRegisteredErrorHeader = "Account Creation Error";
		public const string TacoRegisteredErrorMessage06 = "Enter your email.";
		public const string TacoRegisteredErrorMessage05 = "An unexpected error occurred";
		public const string TacoRegisteredErrorMessage00 = "Password and Confirm Password must match.";
		public const string TacoRegisteredErrorMessage01 = "Not a valid email.";
		public const string TacoRegisteredErrorMessage02 = "Please accept Terms of Service and Privacy Policy";
		public const string TacoRegisteredErrorMessage03 = "Please enter a username. This field is used throughout our system for display purposes.";
		public const string TacoRegisteredErrorMessage04 = "Please enter a password and confirm that password.";
		public const string TacoPlayEndedWinnerModalBody = "Congratulation! You has won &point &prize.";
		public const string TacoPlayEndedWinnerModalHeader = "Win Win Win!";
		public const string TacoRegisteredErrorMessage07 = "Password must be at least 6 characters long.";
		public const string TacoRegisteredErrorUsernameLength = "Username must be at least 3 characters long.";
		public const string TacoRegisteredErrorUsernameFormat = "Username only accepts characters, digit, underscore and @.";
		public const string TacoRegisteredAllOkay = "Looks good, tap or click to register.";

		public const string EnterInvalidEmailError = "Please Enter a valid Email Address";
		public const string EnterExistingEmailError = "You have entered an email address that already exists";
		public const string NotInSystemEmailError = "This email address does not exist in system";
		public const string EndTournamentHeader = "End Tournament";
		public const string EndTournamentConfirm = "Are you sure you want to quit tournament and submit the score currently?";
		public const string TacoOurGamesLoadingMessage = "Loading Games...";
		public const string TacoHighScoresPerhapsText = "highscore";
		public const string TacoHighScoresType = "highscoretype";
		public const string Processing = "Processing";
		//tournaments

		// leaderboard
		public const string ReEnterNotice = "To Re-Enter this <b>'&gameName &type Tournament'</b> the Entry Fee will be debited from your account.";
		public const string LeaderboardTournamentType = "&gameName &type Tournament";
		public const string LeaderboardResultDetails = "   <color=#413F44FF>|</color> &player Players  <color=#413F44FF>|</color>  &winner <color=#413F44FF>|</color> ";
		public const string LeaderboardResultEndOn = "Ended on: &day | ";
		public const string LeaderboardResultTournamentID = "Tournament ID #: &id";
		public const string ManageTournamentCreatedOn = "Created on: &day | ";
		public const string CurrentLeader = "Current Leader";

		//Collumn name must be uniq
		public static readonly string ActiveTournamentDateCreated = "DATE CREATED";
		public static readonly string CompletedTournamentDateColumn = "DATE PLAYED";
		public static readonly string CompletedTournamentWinLossColumn = "WIN/LOSS";
		public static readonly string CompletedTournamentPlacementColumn = "PLACEMENT";
		public static readonly string CompletedTournamentPrizePoolColumn = "PRIZE POOL";
		public static readonly string CompletedTournamentPrizeWonColumn = "PRIZE WON";
		public static readonly string CompletedTournamentActionColumn = "";

		public static readonly string TournamentNameColumn = "NAME";
		public static readonly string TournamentPrizeColumn = "PRIZE";
		public static readonly string TournamentWinnersColumn = "WINNERS";
		public static readonly string TournamentPlayersColumn = "PLAYERS";
		public static readonly string TournamentTimeLeftColumn = "TIME LEFT";
		public static readonly string TournamentEntryFeeColumn = "ENTRY FEE";
		public static readonly string TournamentActionColumn = "";

		public static readonly string LeaderboardTournamentCurrentColumn = " ";
		public static readonly string LeaderboardTournamentRankColumn = "RANK";
		public static readonly string LeaderboardTournamentPlayerColumn = "PLAYER";
		public static readonly string LeaderboardTournamentScoreColumn = "SCORE";

		public static readonly string TransactionType = "TYPE";
		public static readonly string TransactionAmount = "AMOUNT";
		public static readonly string TransactionDate = "DATE";
		public static readonly string TransactionNumber = "TRANSACTION NUMBER";

		public const string TacoTournamentError = "Error getting Your Tournaments ";

		public const string TacoTournamentJoinOrCreate = "Join an existing tournament or create a new one below";
		public const string TacoTournamentCreated = "Tournament created and added to your current Tournaments.";
		public const string TacoTournamentDuplicated = "This tournament already exists. Please join it below or create another one with different criteria.";
		public const string TacoTournamentSubmittingMessage = "Creating Your Tournament";
		public const string TacoTournamentOpponentsPublic = "Public - Anyone can join";
		public const string TacoTournamentOpponentsPrivate = "Private - Only friends can join";
		public const string TacoTournamentOpponentsChallenge = "Challenge - Directly challenge other players";

		public const string TacoTournamentActive = "Tournaments You've Joined. Tournaments End When All Members Have Played.";
		public const string TacoTournamentPublic = "Public Tournaments You Can Join.";
		public const string TacoTournamentPrivate = "Your Private Tournaments.";


		public const string TacoPublicJoining = "Joining...";
		public const string TacoSurePlayModalBody = "Are you sure you are ready to play this game with a prize of &prize?";
		public const string TacoWarningTimeModalBody = "This tournament is going to end within an hour. Are you sure you want to join this public tournament with the fee of &entryFee?";

		public const string TacoPlayStarting = "Starting Tournament Game, good luck!";
		public const string TacoPlayError = "Taco Play Tournament Error.";

		public const string TacoPlayEndedMessage = "Your Round Finished : Posting results!";
		public const string TacoPlayAgainEndedMessage = "Your Round Finished!";
		public const string TacoPlayEndedModalHeader = "Tournament Round Finished!";
		public const string TacoPlayEndedModalBody = "Your game ended, we have posted your score of &gameEndScore to the tournament.\n\n Good Luck!  ";

		public const string TacoPlayEndedAgainModalBody = "Your game ended. Good Luck!  ";
		// join public

		public const string TacoRemoveMoneyModalHeader = "Remove Funds";
		public const string TacoRemoveTacoModalHeader = "Remove Taco Tokens";
		public const string TacoSureJoinModalHeader = "Join This Tournament?";
		public const string TacoSureJoinModalBody = "Are you sure you would like to join this Public Tournament with a prize of &prize? \nIf so a fee of &entryFee will be debited from your account?";

		public const string TacoCreateSuccessHead = "Tournament created";
		public const string TacoJoinWhenCreateSuccessHead = "Joined successfully";
		public const string TacoJoinPublicSuccessHead = "Tournament Joined!";
		public const string TacoJoinPublicSuccessBody = "You can Play your round now. \n\n You joined a tournament with a prize of &prize and a entry fee of &entryFee . \n You now have : &userFunds available. ";

		public const string TacoJoinPublicErrorHead = "Join tournament error";
		public const string TacoJoinAlreadyHead = "Already joined";
		public const string TacoJoinPublicNoFundHead = "Not Enough Funds";
		public const string TacoJoinPublicNoTacoHead = "Not Enough Taco Tokens";
		public const string TacoJoinPublicErrorNoFund = "We're Sorry but you do not have the available funds to pay the <b>Entry Fee</b> &fee.";
		public const string JoinSuccessNotice = "Joined Successfully!";
		public const string ReenterSuccessNotice = "You've Successfully Re-Entered the Tournament";
		public const string NotEnoughFundError = "Not enough funds. You can always add more";
		public const string NotEnoughTokenError = "Not enough tokens. You can always add more";
		public const string DuplidateError = "Duplicated";

		// funds

		// invite friends

		public const string TacoInviteFriendsModalHeader = "Invite Your Friends";

		// withdraw
		public const string WithdrawFundsError00 = "An amount is required";
		public const string WithdrawFundsError01 = "Amount must be a number";
		public const string WithdrawFundsError02 = "You don't have that many funds available. Nice try!";
		public const string WithdrawFundsError03 = "A name is required";
		public const string WithdrawFundsError04 = "An address is required";
		public const string WithdrawFundsError05 = "A city is required";
		public const string WithdrawFundsError06 = "A zip code is required";
		public const string WithdrawFundsSuccessMessage = "Your request has been submitted. A check should be sent within 2 weeks.";

		// add
		public const string AddFundsError00 = "A credit card number is required";
		public const string AddFundsError01 = "An expiration month is required";
		public const string AddFundsError02 = "An expiration year is required";
		public const string AddFundsError03 = "A CVC field is required";
		public const string AddFundsError04 = "An amount must be selected.";
		public const string AddFundsSuccessMessage = "Charge complete, you now have ";
		public const string AddFundsTitleSuccessMessage = "Funds Added";

		public const string AddGTokensTitleSuccessMessage = "Taco Tokens Added";
		public const string AddFundsAdding = "Adding specified funds....";
		public const string AddGTokensAdding = "Adding specified Taco Tokens....";

		public const string RedeemErrorNoFill = "Please fill your information!";
		public const string RedeemErrorIncorrectEmail = "Please check your email again!";
		public const string RedeemErrorIncorrectBirthDate = "Please check your date of birth again!";
		public const string RedeemErrorInvalidAge = "You must over 18 !";

		//
		public const string SuccessHeader = "Success";
		public const string ErrorHeader = "Error";
		public const string EmailSentHeader = "Email Has Been Sent!";
		//tabs
		public const string TacoTournamentCurrentTab = "Current Tournaments";

		public const string TournamentListViewView = "Results >";
		public const string TournamentListViewPlay = "Play Now >";

		public const string TournamentListViewPlayed = "Played";
		public const string TournamentListViewReady = "Ready";

		public const int ListViewButtonHeight = 120;
		public const int ListViewTournamentsButtonHeight = 200;

		public Color32 ListViewHighlightColor = hexToColor ("f8c477ff");
		public Color32 ListViewHeaderColor = hexToColor ("950606ff");
		public Color32 ListViewTextBrightColor = hexToColor ("f8c477ff");
		public Color32 ListViewTextBrightColor2 = hexToColor ("950606ff");

		public static Color32 ListViewEvenRow = hexToColor ("373135ff");
		public static Color32 ListViewOddRow = hexToColor ("413f44ff");
		// fonts

		public const int ListViewHeaderHeight = 140;
		public const int ListViewItemFontSize = 35;
		public const int ListViewHeaderFontSize = 28;
		public const float accodionSpeed = 0.002f;
		public static Vector3 accodionDistance = new Vector3(0, 30);

		public TacoFont HeaderFont = new TacoFont (52);
		public TacoFont BodyFont = new TacoFont (42);
		public TacoFont InputFont = new TacoFont (42);
		public TacoFont MessageFont = new TacoFont (50);
		public const int PasswordMinLength = 6;
		public const int UsernameMinLength = 3;
		public const int TicketExchangeRate = 10;
		public const int CashExchangeRate = 1350;
		public const int legalAge = 18;

		public static string DecorateFund(string unit, int size = 0) {
			return DecorateText (unit, "85cf37", size);
		}
		public static string DecorateTaco(string unit, int size = 0) {
			return DecorateText (unit, "fcc241", size);
		}
		public static string DecorateRP(string unit, int size = 0) {
			return DecorateText (unit, "38cae1", size);
		}

		public static string DecorateText(string text, string hexColor, int size = 0) {
			if (size > 0)
				return "<size=" + size + "><color=#" + hexColor + ">" + text + "</color></size>";
			else
				return "<color=#" + hexColor + ">" + text + "</color>";
		}

		public static string DateFromString(string dateStr){
			string year = dateStr.Substring (0, 4);
			string month = dateStr.Substring (5, 2);
			string day = dateStr.Substring (8, 2);
			return month + "-" + day + "-" + year;
		}

		public static string Pluralize (int number, string word, string space = " ")
		{
			if (number == 1)
				return number.ToString () + space + word;
			else
				return number.ToString () + space + word + "s";
		}

		public static string ToShortOrdinal(int value)
		{
			// Start with the most common extension.
			string extension = "th";

			// Examine the last 2 digits.
			int last_digits = value % 100;

			// If the last digits are 11, 12, or 13, use th. Otherwise:
			if (last_digits < 11 || last_digits > 13)
			{
				// Check the last digit.
				switch (last_digits % 10)
				{
				case 1:
					extension = "st";
					break;
				case 2:
					extension = "nd";
					break;
				case 3:
					extension = "rd";
					break;
				}
			}

			return value.ToString() + extension;
		}

		public static System.DateTime ConvertToDateTime (string value)
		{
			//convertedDate = System.Convert.ToDateTime(value);
			int year = int.Parse (value.Substring (0, 4));
			int month = int.Parse (value.Substring (5, 2));
			int day = int.Parse (value.Substring (8, 2));
			int hour = int.Parse (value.Substring (11, 2));
			int minute = int.Parse (value.Substring (14, 2));
			int second = int.Parse (value.Substring (17, 2));
			return new System.DateTime (year, month, day, hour, minute, second);
		}

		public static string timerCountdown(double timer){
			int hour = (int)(timer / 3600);
			int minute = (int)(timer / 60 - hour * 60);
			int second = ((int)timer) % 60;
			return hour.ToString("D2") + ":" + minute.ToString("D2") + ":" + second.ToString("D2") + (hour > 1 ? " hours" : "");
		}

		void Awake ()
		{
			Instance = this;

			InviteInputPrefab = Resources.Load ("TournamentInvite") as GameObject;
			NotJoinedPlayerPrefab = Resources.Load ("TournamentManageNotJoined") as GameObject;
			DetailCellPrefab = Resources.Load ("TacoDetailCell") as GameObject;
			// remember these have to live under the Resources folder
			TacoIntroGraphic = Resources.Load ("TacoIntroGraphic", typeof(Sprite)) as Sprite;
			ModalSprite = Resources.Load ("TacoOkay", typeof(Sprite)) as Sprite;
			LoginSprite = Resources.Load ("TacoLogin", typeof(Sprite)) as Sprite;
			SignUpSprite = Resources.Load ("TacoSignUp", typeof(Sprite)) as Sprite;
			OptionalModalSprite = Resources.Load ("TacoBack", typeof(Sprite)) as Sprite;
			CloseSprite = Resources.Load ("TacoCloseCircle", typeof(Sprite)) as Sprite;
			PlaySprite = Resources.Load ("TacoPlay", typeof(Sprite)) as Sprite;
			JoinSprite = Resources.Load ("TacoJoin", typeof(Sprite)) as Sprite;
			InviteSprite = Resources.Load ("TacoInvite", typeof(Sprite)) as Sprite;
			NotSelected = Resources.Load ("TacoNotSelected", typeof(Sprite)) as Sprite;
			FirstPlaceSprite = Resources.Load ("TacoFirstPlace", typeof(Sprite)) as Sprite;
			SecondPlaceSprite = Resources.Load ("TacoSecondPlace", typeof(Sprite)) as Sprite;
			ThirdPlaceSprite = Resources.Load ("TacoThirdPlace", typeof(Sprite)) as Sprite;
			OtherPlaceSprite = Resources.Load ("TacoOtherPlace", typeof(Sprite)) as Sprite;
			NAPlaceSprite = Resources.Load ("TacoNAPlace", typeof(Sprite)) as Sprite;
			WinSprite = Resources.Load ("iconWin", typeof(Sprite)) as Sprite;
			LossSprite = Resources.Load ("iconLoss", typeof(Sprite)) as Sprite;
			HandRightIcon = Resources.Load ("icon-hand-right", typeof(Sprite)) as Sprite;
			UserDefault = Resources.Load ("userDefault", typeof(Sprite)) as Sprite;


			Avatar00 = Resources.Load ("TacoAvatar_00", typeof(Sprite)) as Sprite;
			Avatar01 = Resources.Load ("TacoAvatar_01", typeof(Sprite)) as Sprite;
			Avatar02 = Resources.Load ("TacoAvatar_02", typeof(Sprite)) as Sprite;
			Avatar03 = Resources.Load ("TacoAvatar_03", typeof(Sprite)) as Sprite;
			Avatar04 = Resources.Load ("TacoAvatar_04", typeof(Sprite)) as Sprite;
			Avatar05 = Resources.Load ("TacoAvatar_05", typeof(Sprite)) as Sprite;
			Avatar06 = Resources.Load ("TacoAvatar_06", typeof(Sprite)) as Sprite;
			Avatar07 = Resources.Load ("TacoAvatar_07", typeof(Sprite)) as Sprite;

			EmailIconInactive = Resources.Load ("icon-left-inactive", typeof(Sprite)) as Sprite;
			EmailIconActive = Resources.Load ("icon-left-active", typeof(Sprite)) as Sprite;
			EmailIconFocus = Resources.Load ("icon-left-focus", typeof(Sprite)) as Sprite;
			EmailIconRemove = Resources.Load ("manage-remove", typeof(Sprite)) as Sprite;
			IconCash = Resources.Load ("icon-cash", typeof(Sprite)) as Sprite;
			IconGTaco = Resources.Load ("icon-gtaco", typeof(Sprite)) as Sprite;
			IconRP = Resources.Load ("icon-reward", typeof(Sprite)) as Sprite;
			IconTournamentTime = Resources.Load ("icon-tournament-time-orange", typeof(Sprite)) as Sprite;
			IconTournamentUser = Resources.Load ("icon-tournament-user", typeof(Sprite)) as Sprite;
			IconTournamentWinner = Resources.Load ("icon-winner", typeof(Sprite)) as Sprite;

			currencySprites = new Sprite[]{ TacoConfig.IconCash, TacoConfig.IconGTaco, TacoConfig.IconRP};
			sortSprites = new Sprite[] {
				(Sprite)Resources.Load ("SortUpSprite", typeof(Sprite)),
				(Sprite)Resources.Load ("SortDownSprite", typeof(Sprite))
			};
			currencyBrokenSprites = new Sprite[] {
				(Sprite)Resources.Load ("icon-not-enough-fund", typeof(Sprite)),
				(Sprite)Resources.Load ("icon-not-enough-token", typeof(Sprite))
			};
			emailStatusSprites = new Sprite[] {
				(Sprite)Resources.Load ("icon-check-on-field", typeof(Sprite)),
				(Sprite)Resources.Load ("icon-close-on-field", typeof(Sprite))
			};
			ourGameSprites = new Dictionary<string, Sprite> () {
				{"2048 Timed",Resources.Load ("img_icon_timed", typeof(Sprite)) as Sprite},
				{"2048 Untimed",Resources.Load ("img_icon_untimed", typeof(Sprite)) as Sprite},
				{"Taco Blocks",Resources.Load ("img_icon_block", typeof(Sprite)) as Sprite},
				{"Color Flex",Resources.Load ("img_icon_color", typeof(Sprite)) as Sprite},
				{"Flappy Taco",Resources.Load ("img_icon_flappy", typeof(Sprite)) as Sprite},
				{"Moving Or Pointing",Resources.Load ("img_icon_moving", typeof(Sprite)) as Sprite},
				{"Bubble Shooter",Resources.Load ("img_icon_bubble", typeof(Sprite)) as Sprite}
			};

			IconCheck = (Sprite)Resources.Load ("icon-check-green", typeof(Sprite));
			IconWarning =	(Sprite)Resources.Load ("icon-warning-red", typeof(Sprite));
			// TODO : Move all of these to const like the other below - don't want two lists of strings. Did it this way so I can match the key using the associative array

			// device specific 

			#if UNITY_EDITOR
			TacoStrings ["DeviceName"] = "Computer";
			#endif

			#if UNITY_IOS
			TacoStrings ["DeviceName"] = "device";
			#endif

			#if UNITY_ANDROID
			TacoStrings ["DeviceName"] = "device";
			#endif

			#if UNITY_STANDALONE_OSX
			TacoStrings ["DeviceName"] = "Mac";
			#endif

			#if UNITY_STANDALONE_WIN
			TacoStrings ["DeviceName"] = "Computer";
			#endif

			
			// styles 

			// buttons
			TacoStrings ["ViewProfile"] = "";

			TacoStrings ["StartMenu"] = "Home";
			TacoStrings ["GameStart"] = "Home";
			TacoStrings ["AllOurGames"] = "All Our Games";
			TacoStrings ["HowGameWorks"] = "How It Works";
			//TacoStrings ["ContactUs"] = "Feedback";
			TacoStrings ["Feedback"] = "Feedback";
			TacoStrings ["LogoutButton"] = "Logout";

			TacoStrings ["CreateTournamentButton"] = "Create";
			TacoStrings ["TournamentsButton"] = "Tournaments";
			TacoStrings ["AddFundsButton"] = "Add Funds";
			TacoStrings ["AddGTokensButton"] = "Add Taco Tokens";
			TacoStrings ["AddGTokenButton"] = "Add Taco Token";
			TacoStrings ["OtherGamesButton"] = "Our Games";
			TacoStrings ["ProfileButton"] = "Profile";
			TacoStrings ["WithdrawFundsButton"] = "Withdraw Funds";

			TacoStrings ["TacoStageButton"] = "Play Tournaments Today!";

			TacoStrings ["TacoModalButton"] = "Close";

			TacoStrings ["WithdrawFundsButton"] = "Withdraw Funds";

			TacoStrings ["CurrentLeaderboardButton"] = "Leaderboard";
			TacoStrings ["CurrentPlayButton"] = "Play";

			TacoStrings ["NavCreateTournamentButton"] = "Create New Tournament!";
			TacoStrings ["MyActiveTournamentsButton"] = "My Tournaments";
			TacoStrings ["MyLeaderboardTournamentsButton"] = "Leaderboards";
			TacoStrings ["MyPublicTournamentsButton"] = "Public";
			TacoStrings ["MyPrivateTournamentsButton"] = "Private";

			TacoStrings ["LoginRegisterButton"] = "Skip For Now";
			TacoStrings ["LoginButton"] = "Sign In";
			TacoStrings ["LoginSkipButton"] = "Skip For Now";
			TacoStrings ["LoginWithFacebook"] = "Facebook";
			TacoStrings ["LoginWithGoogle"] = "Google";
			TacoStrings ["LoginRegisterButton"] = "Create";

			//TacoStrings ["RegisterAgeToggle"] = "By checking this box, I confirm that I am at least 18 years of age.";
			TacoStrings ["AutoLoginToggle"] = "Remember my login on this " + TacoStrings ["DeviceName"];

			TacoStrings ["TacoFundsSkipButton"] = "Skip For Now";
			TacoStrings ["JoinTournamentButton"] = "Join";


			TacoStrings ["InviteFriendsButton"] = "Invite Friends";

			TacoStrings ["TacoCreateTime"] = "Time Remaining :";
			// labels & text


			TacoStrings ["TacoInvitedText"] = "Friends To Invite:";
			TacoStrings ["TacoOurGamesHeader"] = "Other Taco Games";
			TacoStrings ["TacoOurGamesMessage"] = "New Challenges!";

			TacoStrings ["TacoEmail"] = "Enter your email or username";
			TacoStrings ["TacoPassword"] = "Enter your password";

			TacoStrings ["TacoCreateHeader"] = TacoStrings ["CreateTournament"];
			TacoStrings ["TacoCreateMessage"] = "Configure a new tournament below.";
			TacoStrings ["TacoCurrencyText"] = "Type Currency :";
			TacoStrings ["TacoCreatePlayers"] = "Tournament Size :";
			TacoStrings ["TacoCreateFee"] = "Entry Fee :";
			TacoStrings ["TacoPrizeStructure"] = "Winner :";
			TacoStrings ["TacoCreateGToken"] = "Taco Token :";
			// our games 
			TacoStrings ["TacoGames"] = "Featured Games with Taco Power:";
			TacoStrings ["TacoGamesMessage"] = "Tap a game for installation details.";

			// funds 
			TacoStrings ["AddFundsPanelHeader"] = "Enter your credit card details to add funds from Stripe.";
			TacoStrings ["WithdrawFundsPanelHeader"] = "Enter your details and the amount that you would like to withdraw.";

			//GTokens
			TacoStrings ["AddGTokensPanelHeader"] = "Choice price to convert Taco Tokens";

			TacoStrings ["TogglePublic"] = "Public";
			TacoStrings ["TogglePrivate"] = "Private";
			TacoStrings ["TacoCreateAccess"] = "Opponents :";

			TacoStrings ["ToggleRealCurrency"] = "Real";
			TacoStrings ["ToggleGTokenCurrency"] = "Taco Token";

			TacoStrings ["StatusText"] = TacoLoginMessage;
			TacoStrings ["TacoOpenButtonMessage"] = TacoOpenButtonMessage;

			TacoStrings ["TacoJoinMessage"] = "Would you like to join this Public Tournament?";
			TacoStrings ["TacoJoinHeader"] = "Join Tournament?";

			// modals 

			TacoStrings ["TacoIntroHeader"] = "Start challenging your friends for real money.";
			TacoStrings ["TacoIntroBody"] = "Login or register to challenge today.";

			TacoStrings ["TacoInviteEmail"] = "Enter Your Friend's Email or Username:";
		}


		public static string GetValue (string key)
		{
			string value = (string)TacoStrings [key];
			return value;
		}

		public static Color hexToColor (string hex)
		{
			hex = hex.Replace ("0x", "");//in case the string is formatted 0xFFFFFF
			hex = hex.Replace ("#", "");//in case the string is formatted #FFFFFF
			byte a = 255;//assume fully visible unless specified in hex
			byte r = byte.Parse (hex.Substring (0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse (hex.Substring (2, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse (hex.Substring (4, 2), System.Globalization.NumberStyles.HexNumber);
			//Only use alpha if the string has enough characters
			if (hex.Length == 8) {
				a = byte.Parse (hex.Substring (6, 2), System.Globalization.NumberStyles.HexNumber);
			}
			return new Color32 (r, g, b, a);
		}

		public Sprite GetAvatarSprite (int avatar)
		{
			Sprite avatarSprite = Avatar00;

			switch (avatar) {
			case 0:
				avatarSprite = Avatar00;
				break;
			case 1:
				avatarSprite = Avatar01;
				break;
			case 2:
				avatarSprite = Avatar02;
				break;
			case 3:
				avatarSprite = Avatar03;
				break;
			case 4:
				avatarSprite = Avatar04;
				break;
			case 5:
				avatarSprite = Avatar05;
				break;
			case 6:
				avatarSprite = Avatar06;
				break;
			case 7:
				avatarSprite = Avatar07;
				break;
			}

			return avatarSprite;
		}
	}
}
