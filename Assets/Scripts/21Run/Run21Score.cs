using System;
using System.Linq;

namespace FRG.Taco.Run21
{
    /// <summary>
    /// Class responsible for scoring 21 Run during gameplay and calculating final score.
    /// </summary>
    public class Run21Score
    {
        [Serializable]
        public class Scoring
        {
            public int TimeBonusBase = 5000;
            public int Run21Bonus = 500;
            public int BlackJackBonus = 750;
            public int FiveCardBonus = 1000;
            public int StreakBaseBonus = 500;
            public int NoBustsBonus = 150;
            public int SingleNoBustBonus = 25;
            public int EmptyLaneBonus = 25;
            public int PerfectGameBonus = 1000;
            public int GoodStreakBonus = 500;
            public int GreatStreakBonus = 2 * 500;
            public int AmazingStreakBonus = 3 * 500;
            public int OutstandingStreakBonus = 4 * 5000;
            public int PerfectStreakBonus = 5 * 500 + 100;
        }

        [Serializable]
        public class ComboPoints
        {
            public int TwentyoneBlackjack = 100;
            public int FiveCardBlackjack = 200;
            public int TwentyoneFiveCard = 300;
            public int TwentyoneFiveCardBlackjack = 400;
        }

        /// <summary>
        /// Get or set scoring base data where the base numbers scoring formula uses are located.
        /// </summary>
        public Scoring scoring = Run21Data.Instance.scoringData;

        /// <summary>
        /// Current score from playing in lanes, excludes final score. Updated throughout the game.
        /// </summary>
        public int GameScore;

        /// <summary>
        /// Current score for display on UI. Updated throughout score popups animated.
        /// </summary>
        public int DisplayedGameScore;

        /// <summary>
        /// Minimum time a player has to be playing in order to receive time bonus points.
        /// This stops the game from awarding TimeBonusBase if the player just pauses and ends game.
        /// </summary>
        public int MinPlayTimeThreshold = 30;

        private float _playTime;

        /// <summary>
        /// Current playing time in seconds. Updated throughout the game.
        /// </summary>
        public float PlayTime
        {
            get { return _playTime; }
            set
            {
                if (value > Run21.PlayTimeMax)
                    value = Run21.PlayTimeMax;
                _playTime = value;
            }
        }

        private int _busts;

        /// <summary>
        /// How many times you've gone over 21 in a lane. Updated throughout the game.
        /// </summary>
        public int Busts
        {
            get { return _busts; }
            set { _busts = value > Run21.BustsMax ? Run21.BustsMax : value; }
        }

        /// <summary>
        /// 150 for 0 busts, 25 per no bust otherwise.
        /// </summary>
        public int BustScore
        {
            get { return Busts == 0 ? scoring.NoBustsBonus : (Run21.BustsMax - Busts) * scoring.SingleNoBustBonus; }
        }

        /// <summary>
        /// Each empty lane is 25 points
        /// </summary>
        public int LaneScore
        {
            get { return EmptyLanes * scoring.EmptyLaneBonus; }
        }

        /// <summary>
        /// No cards left is 1000 points.
        /// </summary>
        public int PerfectGameScore
        {
            get { return Busts == 0 && EmptyLanes == 4 && DrawDeckEmpty && ActiveDeckEmpty ? scoring.PerfectGameBonus : 0; }
        }

        /// <summary>
        /// 5000 points scaled to remaining time, e.g. finishing in half time gets you 2500 points.
        /// </summary>
        public int TimeScore
        {
            get
            {
                if (PlayTime < MinPlayTimeThreshold)
                {
                    return 0;
                }
                else
                {
                    return (int) (((Run21.PlayTimeMax - PlayTime) / Run21.PlayTimeMax) * scoring.TimeBonusBase);
                }
            }
        }

        public int EmptyLanes { get; private set; }

        public bool DrawDeckEmpty { get; private set; }

        public bool ActiveDeckEmpty { get; private set; }

        public int FinalScore { get; private set; }

        public Run21Score(Scoring scoringData)
        {
            scoring = scoringData;
        }

        /// <summary>
        /// After game ends, calculate final score
        /// </summary>
        /// <param name="game"></param>
        public void CalculateFinalScore(Run21 game)
        {
            EmptyLanes = game.LaneDecks.Count(x => x.IsEmpty);
            DrawDeckEmpty = game.DrawDeck.IsEmpty;
            ActiveDeckEmpty = game.ActiveCardDeck.IsEmpty;

            FinalScore = GameScore + BustScore + LaneScore + PerfectGameScore + TimeScore;
        }

        /// <summary>
        /// Score the deck, we hit 21 on it.
        /// </summary>
        public int ScoreLaneDeck(bool is21Run, bool isBlackJack, bool isFiveCards, int noBustStreakCount)
        {

            int deckScore = 0;

            if (is21Run)
            {
                deckScore += scoring.Run21Bonus;
            }
            
            if (isBlackJack)
            {
                deckScore += scoring.BlackJackBonus;
            }

            if (isFiveCards)
            {
                deckScore += scoring.FiveCardBonus;
            }

            int comboPoints = 0;

            if (isBlackJack && isFiveCards && is21Run)
            {
                comboPoints += Run21Data.Instance.comboPoints.TwentyoneFiveCardBlackjack;
            }
            else if (isFiveCards && is21Run)
            {
                comboPoints += Run21Data.Instance.comboPoints.TwentyoneFiveCard;
            }
            else if (isBlackJack && isFiveCards)
            {
                comboPoints += Run21Data.Instance.comboPoints.FiveCardBlackjack;
            }
            else if (isBlackJack && is21Run)
            {
                comboPoints += Run21Data.Instance.comboPoints.TwentyoneBlackjack;
            }
            else
            {
                comboPoints = 0;
            }

            int bonusScore = ResolveStreakBonus(noBustStreakCount);
            
            int scoreEarned = deckScore + bonusScore + comboPoints;

            GameScore += scoreEarned;
            return scoreEarned;
        }
        
        /// <summary>
        /// After two lanes cleared in a row, streak count becomes 1
        /// After three lanes cleared in a row, streak count becomes 2
        /// After After 6 lanes cleared in a row, streak count becomes 5
        /// </summary>
        /// <param name="streakCount"></param>
        /// <returns></returns>
        private int ResolveStreakBonus(int streakCount)
        {
            switch (streakCount)
            {
                case 1:
                    return scoring.GoodStreakBonus;
                case 2:
                    return scoring.GreatStreakBonus;
                case 3:
                    return scoring.AmazingStreakBonus;
                case 4:
                    return scoring.OutstandingStreakBonus;
                case 5:
                    return scoring.PerfectStreakBonus;
                default:
                    return 0;
            }
        }

        public Run21Score Clone()
        {
            Run21Score cloneScore = new Run21Score(scoring);

            cloneScore.GameScore = GameScore;
            cloneScore.Busts = Busts;
            cloneScore.PlayTime = PlayTime;
            cloneScore.EmptyLanes = EmptyLanes;
            cloneScore.DrawDeckEmpty = DrawDeckEmpty;
            cloneScore.ActiveDeckEmpty = ActiveDeckEmpty;
            cloneScore.FinalScore = FinalScore;

            return cloneScore;
        }

        /// <summary>
        /// Updates the <see cref="DisplayedGameScore"/> 
        /// </summary>
        /// <param name="scoreEvent"></param>
        /// <exception cref="ArgumentException"></exception>
        public void OnScoreEvent(ScoreEvent scoreEvent)
        {
            if (scoreEvent == null)
            {
                throw new ArgumentException("Cannot update score. Received invalid score.");
            }

            if (!scoreEvent.IsScoreAnimated())
            {
                DisplayedGameScore += scoreEvent.Score;
            }
        }

        internal void Reset()
        {
            GameScore = 0;
            DisplayedGameScore = 0;
            PlayTime = 0;
            Busts = 0;
            EmptyLanes = 0;
            DrawDeckEmpty = false;
            ActiveDeckEmpty = false;
            FinalScore = 0;
        }

        public override string ToString()
        {
            return
                $@"GameScore:{GameScore} Busts:{Busts} PlayTime:{PlayTime} EmptyLanes:{EmptyLanes} DrawDeckEmpty:{DrawDeckEmpty} ActiveDeckEmpty:{ActiveDeckEmpty} TimeScore:{TimeScore} PerfectGameScore:{PerfectGameScore} FinalScore:{FinalScore} ";
        }
    }

    public interface GameEvent
    {
        
    }

    public class ScoreEvent : GameEvent
    {
        private int _laneIndex;
        private int _score;
        private bool _containsBlackJack;
        private bool _isValue21;
        private bool _isFiveCardsScore;
        private bool _isStreak;
        private bool _isBust;
        public Deck deck;

        public ScoreEvent(int laneIndex, int score, bool isBlackJack, bool isValue21, bool isFiveCardsScore, bool isBust, bool isStreak, Deck pDeck = null)
        {
            _laneIndex = laneIndex;
            _score = score;
            _containsBlackJack = isBlackJack;
            _isValue21 = isValue21;
            _isFiveCardsScore = isFiveCardsScore;
            _isBust = isBust;
            _isStreak = isStreak;
            deck = pDeck;
        }

        public int LaneIndex
        {
            get { return _laneIndex; }
            set { _laneIndex = value; }
        }

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }

        public bool ContainsBlackJack
        {
            get { return _containsBlackJack; }
            set { _containsBlackJack = value; }
        }

        public bool IsValue21
        {
            get { return _isValue21; }
            set { _isValue21 = value; }
        }

        public bool IsFiveCardsScore
        {
            get { return _isFiveCardsScore; }
            set { _isFiveCardsScore = value; }
        }

        public bool IsBust
        {
            get { return _isBust; }
            set { _isBust = value; }
        }

        public bool IsStreak
        {
            get { return _isStreak; }
            set { _isStreak = value; }
        }

        public bool IsScoreAnimated()
        {
            return IsValue21 || IsFiveCardsScore || ContainsBlackJack || IsBust;
        }
    }

    public class GameOverEvent : GameEvent
    {
        private bool _isTimeExpired;
        private int _bustCount;
        private bool _perfectScore;

        private bool[] _emptyLanes = new bool[4];

        public bool IsTimeExpired
        {
            get { return _isTimeExpired; }
            set { _isTimeExpired = value; }
        }

        public bool[] EmptyLanes
        {
            get { return _emptyLanes; }
            set { _emptyLanes = value; }
        }

        public int BustCount
        {
            get { return _bustCount; }
            set { _bustCount = value; }
        }

        public bool PerfectScore
        {
            get { return _perfectScore; }
            set { _perfectScore = value; }
        }

        public static GameOverEvent From(Run21 game)
        {
            GameOverEvent gameOverEvent = new GameOverEvent();

            gameOverEvent._isTimeExpired = game.Score.PlayTime >= Run21.PlayTimeMax;
            gameOverEvent._bustCount = game.Score.Busts;
            gameOverEvent._perfectScore = game.Score.PerfectGameScore != 0;

            gameOverEvent._emptyLanes[0] = game.FirstLane.Cards.Count == 0;
            gameOverEvent._emptyLanes[1] = game.SecondLane.Cards.Count == 0;
            gameOverEvent._emptyLanes[2] = game.ThirdLane.Cards.Count == 0;
            gameOverEvent._emptyLanes[3] = game.FourthLane.Cards.Count == 0;

            return gameOverEvent;
        }
    }
}