using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace GameTaco {

	// Const values that probably won't change per game or would be customized or localized

	#region Helper Models

	public static class Constants {
		public const string adminCode = "HelloTaco";
		public static string[] serverUrls = { "https://staging.gametaco.com/", "https://demo.gametaco.com/", "https://www.gametaco.com/", "http://localhost:3000/" };
		public static int numberOfPress = 9;
		#if UNITY_EDITOR
			public static string BaseUrl = "https://staging.gametaco.com/";
			//"http://localhost:3000/";
		#else
			public static string BaseUrl = "https://staging.gametaco.com/";//"http://localhost:3000/";
		#endif
		public const string StripeTokenUrl = "https://api.stripe.com/v1/tokens";
		public const string StripePublicKey = "pk_test_mIhrqMhTL13eRBG4CTOBLj9A";
		public const string localUrl = "http://localhost:3000/";
		public const string AesIV256 = @"G8G22#I2197QX^AV";
		public const string AesKey256 = @"5DON70%B*6A71EG&3JD^%1F*E#7JSW@^";

	}

	public class TacoFont {

		public int Size;
		public FontStyle FontStyle;
		public Color32 FontColor;

		public TacoFont(int size) {

			Size = size;
		}

	}

	public class TacoUser {
		public int userId;
		public string email;
		public double funds;
		public double bonus;
		public string token;
		public int avatar;
		public string webToken;
		public string avatarOnline;
		public string ticket;

		public string gToken;
		public int autoLogin;
		public int highScoreUser;
		public string name;
		public string displayName;
		public string referedCode;
		public ContactProfile contactProfile;

		public double TotalCash { get { return funds + bonus; } }
	}


	public static class CanvasNames {

		public const string TacoBlockingCanvas = "TacoBlockingCanvas";
		public const string TacoCommonCanvas = "TacoCommonCanvas";
		public const string TacoAuthCanvas = "TacoAuthCanvas";
		public const string TacoTournamentsCanvas = "TacoTournamentsCanvas";
		public const string TacoTournamentsFakeCanvas = "TacoTournamentsFakeCanvas";
		public const string TacoOurGamesCanvas = "TacoOurGamesCanvas";
	}

	public static class PanelNames {

		public const string Modal = "TacoModalPanel";
		public const string Message = "TacoMessagePanel";
		public const string Foldout = "TacoFoldout";

		public const string MyTournamentsPanel = "MyTournamentsPanel";
		public const string MyTournamentsFakePanel = "MyTournamentsFakePanel";
		public const string MyPrivatePanel = "MyPrivatePanel";
		public const string MyCompletedPanel = "MyCompletedPanel";
		public const string MyLeaderboardPanel = "MyLeaderboardPanel";
		public const string FilterListPanel = "FilterListPanel";
		public const string SortListPanel = "SortListPanel";
		public const string FilterActiveListPanel = "FilterActiveListPanel";
		public const string SortActiveListPanel = "SortActiveListPanel";
		public const string ProfilePanel = "MyProfilePanel";
		public const string BalancePanel = "BalancePanel/Container";
		public const string MyTransactionPanel = "MyTransactionPanel";
		public const string MyLeaderboardPanelFromEndGame = "MyLeaderboardPanelFromEndGame";
		public const string MyPublicPanel = "MyPublicPanel";
		public const string FundsRunningLowPanel = "FundsRunningLowPanel";
		public const string DepositDisablePanel = "DepositDisablePanel";
		public const string MyPublicFakePanel = "MyPublicFakePanel";
		public const string CreatePublicPanel = "CreateTournamentPanel";
		public const string ManageTournamentPanel = "ManageTournamentPanel";
		public const string JoinPublicPanel = "JoinPublicPanel";
		public const string RegisterPanel = "RegisterPanel";
		public const string LoginPanel = "LoginPanel";
		public const string FeaturedGamesPanel = "FeaturedGamesPanel";

	}

	public static class ModalFunctions {

		public const string TournamentSubmit = "TournamentSubmit";
		public const string TournamentViewLeaderboard = "TournamentViewLeaderboard";
		public const string TournamentSubmitResult = "TournamentSubmitResult";
		public const string ForfeitTournamentGame = "ForfeitTournamentGame";

		public const string SawIntro = "SawIntro";
		public const string ConfirmPlay = "ConfirmPlay";
		public const string WaitingPlay = "WaitingPlay";
		public const string StartPlay = "StartPlay";
		public const string JoinExistTournament = "JoinExistTournament";
		public const string SeeResultExistTournament = "SeeResultExistTournament";
		public const string ReadMoreForbidden = "ReadMoreForbidden";
		public const string JoinPublicSuccess = "TacoJoinPublicSuccess";
		public const string ReEnterTournament = "ReEnterTournament";
		public const string JoinTournament = "JoinTournament";
		public const string JoinPlayAgainTournament = "JoinPlayAgainTournament";
		public const string InviteFriends = "InviteFriends";
		public const string FundsScreen = "FundsScreen";
		public const string TacoScreen = "TacoScreen";
		public const string ExitApp = "ExitApp";


		public const string LogoutUser = "LogoutUser";
		public const string TournamentSubmitComplete = "TournamentSubmitComplete";
		public const string RegisterResult = "RegisterResult";
		public const string TournamentGamePosted = "TournamentGamePosted";
		public const string ReturnToTournaments = "ReturnToTournaments";

		public const string ReturnToGame = "ReturnToGame";
		public const string ReturnToMenu = "ReturnToMenu";

		public const string TacoEndTournament = "TacoEndTournament";
		public const string ResetPassword = "ResetPassword";
		public const string TacoFreePlayGiftToken = "TacoFreePlayGiftToken";
		public const string TacoFreePlayGiftTokenMulti = "TacoFreePlayGiftTokenMulti";

	}

	public static class UserPreferences {

		public const string sawIntro = "sawIntro";
		public const string autoLogin = "autoLogin";
		public const string userToken = "userToken";
		public const string userCurrencyType = "currencyType";
		public const string userTourType = "tourType";

	}
	#endregion

	#region Result Models

	[System.Serializable]
	public class CreateTournamentResult : object {

		public bool success;
		public Tournament tournament;
		public bool best;
		public bool forbidden;
		public string message;
		public string userFunds;
		public double userBonus;
		public string ticket;

		public string typeCurrency;
	}

	[System.Serializable]
	public class LeaderboardWinner : object {

		public int rank;
		public int score;
	}

	[System.Serializable]
	public class LeaderboardResult : object {

		public bool success;
		public Tournament tournament;
		public LeaderboardWinner[] winner;
		public string message;
		public string status;
		public List<LeaderboardRow> leaderboard;
	}

	public class ManageTournamentResult : object {
		public bool success;
		public Tournament tournament;
	}

	[System.Serializable]
	public class LeaderboardRow {
		public string rank;
		public int userId;
		public int score;
		public bool playable;
		public string email;
		public string picture;
	}

	public class PrivateRow {

		public bool success;
		public List<Tournament> started;
		public List<Tournament> ended;
		public int userId;
		public int gameId;
		public string gameName;
		public string email;
		public double funds;
		public string token;
		public string message;
		public int avatar;
	}

	[System.Serializable]
	public class CompletedTournamentsResult : object {
		public List<Tournament> tournaments;
		public string message;
	}

	[System.Serializable]
	public class PublicTournamentsResult : object {
		public List<Tournament> tournaments;
		public string message;
	}

	[System.Serializable]
	public class PrivateTournamentsResult : object {
		public List<Tournament> tournaments;
		public string message;
	}

	[System.Serializable]
	public class JoinTournamentResult : object {
		public bool success;
		public bool addFund;
		public Tournament tournament;
		public string message;
		public string userFunds;
		public string ticket;
		public float token;
		public double cash;
		public string error;

		public double userBonus;
		public string currencyValue;
	}

	[System.Serializable]
	public class StartGameResult : object {
		public bool success;
		public bool forbidden;
		public int tournamentId;
		public string token;
		public int highScore;
		public string message;
	}

	[System.Serializable]
	public class ScoreResult : object {

		public bool success;
		public int score;
		public string message;
		public Tournament tournament;
		public double funds;
		public double ticket;
		public bool winner;
		public double gTokens;
		public bool updated;
	}

	[System.Serializable]
	public class TournamentList : object {
		public List<Tournament> tournaments;
	}

	public static class TournamentStatus {
		public const string Started = "started";
		public const string Ended = "ended";
		public const string Processed = "processed";
	}

	[System.Serializable]
	public class Tournament : object {
		public int[] memberIds;
		public int[] entryIds;
		public string[] invitedEmails;
		public string[] inviteEmails;
		public string name;
		public double prize;
		public double entryFee;
		public string accessType;
		public int id;
		public int creatorId;
		public int gameId;
		public string status;
		public bool played;
		public int size;
		public int prize_structure;
		public string endDate;
		public int memberlength;
		public int typeCurrency;
		public int TimeRemaining;
		public string createdAt;
		public string entryCreatedAt;
		public int rank;
		public string[] friendEmails;
		public string[] friendStatuses;
		public string[] inviteStatuses;
		public int[] friendIds;
		public bool playable;

		public string RemainingTimeString(bool useWord = true) {
			System.TimeSpan remaining = RemainingTimeSpan;
			string display = string.Empty;

			if (played && !playable && useWord) {
				display = TacoConfig.FinishStatus;
			}
			else if (remaining > new System.TimeSpan(0, 24, 0, 0)) {
				display = TacoConfig.Pluralize(remaining.Days, "day");
			}
			else if (remaining > new System.TimeSpan(0, 1, 0, 0)) {
				display = TacoConfig.Pluralize(remaining.Hours, "hr");
			}
			else if (remaining > new System.TimeSpan(0, 0, 1, 0)) {
				display = TacoConfig.Pluralize(remaining.Minutes, "min");
			}
			else if (remaining > new System.TimeSpan(0, 0, 0, 0)) {
				display = TacoConfig.Pluralize(remaining.Seconds, "second");
			}
			else {
				display = TacoConfig.EndStatus;
			}
			return display;
		}

		public System.TimeSpan RemainingTimeSpan {
			get {
				System.DateTime createdTime = TacoConfig.ConvertToDateTime(createdAt);
				return createdTime.Add(new System.TimeSpan(TimeRemaining, 0, 0, 0)).Subtract(System.DateTime.UtcNow);
			}
		}

		public double PrizePool {
			get {
				return size * entryFee * (typeCurrency == 0 ? TacoConfig.cashReduce : 1);
			}
		}

		public string Type {
			get {
				return IsPublic ? "Public" : "Private";
			}
		}

		public bool IsPublic {
			get {
				return accessType == "public";
			}
		}

		public bool IsWarningTime() {
			return RemainingTimeSpan < new System.TimeSpan(0, 0, 10, 0);
		}


		public string PlayedDayFormat {
			get {
				var createdTime = TacoConfig.ConvertToDateTime(entryCreatedAt).ToLocalTime();
				return createdTime.Month + "-" + createdTime.Day + "-" + createdTime.Year;
				//return TacoConfig.DateFromString(entryCreatedAt);
			}
		}

		public System.TimeSpan PlayedAtToDateTimeSpan {
			get {
				System.DateTime entryPlayedTime = TacoConfig.ConvertToDateTime(entryCreatedAt);
				return System.DateTime.UtcNow.Subtract(entryPlayedTime);
			}
		}

	}

	[System.Serializable]
	public class TournamentResult : object {
		public bool success;
		public List<Tournament> tournaments;
	}

	[System.Serializable]
	public class SystemError : object {
		public bool verErr;
		public bool success;
		public string message;
	}

	[System.Serializable]
	public class LoginResult : object {

		public bool success;
		public List<Tournament> started;
		public List<Tournament> ended;
		public int userId;
		public int gameId;
		public string gameName;
		public string mail;
		public double funds;
		public double bonus_cash;
		public string signon_token;
		public string token;
		public string message;
		public string avatar;
		public string userName;
		public string value;
		public string msg;
		public string ticket;
		public string displayName;
		public string name;

		public string gToken;
		public int highScoreUser;
		public string free;
		public int remainingClaim;
		public int login_count;
		public string referCode;

		public string score_tokens;
		public ContactProfile contactProfile;
	}

	[System.Serializable]
	public class ContactProfile {
		public string address;
		public string address2;
		public string state;
		public string city;
		public string country;
		public string zipcode;
		public string FullAddress() {
			string[] adds = { address, address2, city, state, zipcode };
			return string.Join(", ", adds.Where(x => !string.IsNullOrEmpty(x)).ToArray());
		}
		public void UpdateData(string _address, string _address2, string _city, string _state, string _zipcode) {
			address = _address;
			address2 = _address2;
			city = _city;
			state = _state;
			zipcode = _zipcode;
		}
	}

	[System.Serializable]
	public class SessionResult : object {

		public bool success;

		public string message;
		public int remainingClaim;
		public int login_count;
		public UserDetail user;
		public GameDetail game;
		public string signon_token;
		public string avatar;
		public string value;
		public string msg;
		public string gToken;

		public int highScoreUser;
		public ContactProfile contactProfile;
	}


	[System.Serializable]
	public class GameDetail : object {
		public int id;
		public string name;
		public string category;
	}

	[System.Serializable]
	public class UserDetailResult : object {
		public bool success;
		public UserDetail user;
	}

	[System.Serializable]
	public class UserDetail : object {

		public int id;
		public string userName;
		public string email;
		public double funds;
		public double bonus_cash;
		public string ticket;
		public string gToken;
		public string name;
		public string referCode;
		public string displayName;
		public string avatar;
	}

	[System.Serializable]
	public class GameFeaturedResult : object {

		public bool success;
		public List<Game> games;
		public string downloadLink;
	}

	[System.Serializable]
	public class Game : object {
		public string name;
		public string type;
		public string imageUrl;
		public string downloadLink;
		public int activeTournaments;
	}

	[System.Serializable]
	public class AddGTokesResult : object {
		public bool success;
		public string message;
		public double funds;
		public string gTokens;

	}

	[System.Serializable]
	public class ErrorResult : object {
		public bool success;
		public string message;
		public bool verErr;
	}

	public class UpdateHighScoreAndTokensResult : object {
		public bool success;
		public string message;
		public string ticket;
		public string gtokens;
	}


	#endregion
	public class AccessTokenGoogleInfo {
		public string access_token;
		public string refresh_token;
		public double id_token;

	}

	public class UserGoogleInfo {
		public string sub;
		public string name;
		public string picture;
		public string email;
		public string gender;
		public string locale;
	}

	public class GoogleDeviceInfo {
		public string device_code;
		public string user_code;
		public string verification_url;
		public string expires_in;
		public float interval;
	}

	public class AccessData {
		public string access_token;
		public string refresh_token;
		public int expires_in;
		public string token_type;
		public string id_token;
		public string error;
	}

	public class ClaimInfo {
		public bool success;
		public int addedTokenAmount;
		public int newTacoToken;
		public int nextToken;
		public int remainingClaim;
	}

	public class GeneralResult {
		public bool success;
		public string msg;
		public string err;
		public int ticket;
		public double cash;
		public double bonus_cash;

		public double token;
	}

	[System.Serializable]
	public class UserTransaction {
		public double amount;
		public string action;
		public string createdAt;
		public int typeCurrency;
		public string TypeCurrency {
			get {
				if (typeCurrency == 1) return "TT";
				else if (typeCurrency == 2) return "RP";
				else if (typeCurrency == 3) return "BM";
				return "$";
			}
		}
		public int id;

		public string FormatCurrency {
			get {
				double amountValue = amount;// double.Parse(amount);
				if (amountValue >= 0) {
					return " " + TypeCurrency + amountValue;
				}
				else {
					return "-" + TypeCurrency + (amountValue * -1).ToString();
				}
			}
		}
	}

	[System.Serializable]
	public class Prize {
		public int id;
		public string name;
		public string images;
		public string description;
		public int ticket;
	}

	public class TransactionResult {
		public bool success;
		public UserTransaction[] transactions;
	}

	public class PrizesResult {
		public bool success;
		public Prize[] prizes;
	}

}
