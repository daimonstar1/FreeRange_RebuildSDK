using System;
using UnityEngine;

namespace FRG.Taco.Run21
{
    /// <summary>
    /// Implements 21 Run logic and rules. You can play the whole game here.
    /// </summary>
    public class Run21
    {
        public event Action<ScoreEvent> ScoreEvent;
        public event Action<GameOverEvent> GameOverEvent;

        public const int DeckSize = 52;
        public const int BustsMax = 3;
        public const int LaneOneIndex = 0;
        public const int LaneTwoIndex = 1;
        public const int LaneThreeIndex = 2;
        public const int LaneFourIndex = 3;

        /// <summary>
        /// Represents 2 scores for a dec/card for in lane. Aces have 2 values, 1/11, so we must use 2 scores.
        /// </summary>
        public struct HiLowValue
        {
            public int high;
            public int low;
        }

        /// <summary>
        /// May playtime in seconds. If you go over game ends. Kova TODO move to data asset
        /// </summary>
        public const int PlayTimeMax = 300;

        /// <summary>
        /// Deck that represents starting cards you draw from top to get a card you'll play <see cref="ActiveCard"/>
        /// </summary>
        public Deck DrawDeck = new Deck();

        /// <summary>
        /// Deck that represents a single card player is about to play.
        /// </summary>
        public Deck ActiveCardDeck = new Deck();

        /// <summary>
        /// Decks that repreesent 4 lanes to play in, from left to right.
        /// </summary>
        public Deck[] LaneDecks = new Deck[4];

        /// <summary>
        /// Single card drawn from drawing deck that player is about to play
        /// </summary>
        public Card ActiveCard
        {
            get { return ActiveCardDeck.TopCard; }
        }

        /// <summary>
        /// Game score holder.
        /// </summary>
        public Run21Score Score { get; set; }

        /// <summary>
        /// Busted 3 times or time has run out. Can't play anymore.
        /// </summary>
        public bool IsGameOver { get; private set; }

        /// <summary>
        /// The amount of seconds game is being played. Must be set explicitly as the rules (this) don't have their own timer.
        /// </summary>
        private float _playTime;

        public float PlayTime
        {
            get { return _playTime; }
            private set
            {
                _playTime = value;
                if (_playTime >= PlayTimeMax)
                    _playTime = PlayTimeMax;
            }
        }

        /// <summary>
        /// How many cards has the user played during the game
        /// </summary>
        private int _cardsPlayed = 0;

        public int CardsPlayed
        {
            get { return _cardsPlayed; }
        }

        private int _remainingCards = DeckSize;

        /// <summary>
        /// How many cards remaining in draw deck.
        /// </summary>
        public int RemainingCards
        {
            get { return _remainingCards; }
            set { _remainingCards = value; }
        }

        private int _bustedCardCount;

        /// <summary>
        /// How many cards were busted so far.
        /// </summary>
        public int BustedCardCount
        {
            get { return _bustedCardCount; }
            set { _bustedCardCount = value; }
        }

        /// <summary>
        /// The highest number of clearing decks in a row.
        /// </summary>
        private int _bestStreak = 0;

        public int BestStreak
        {
            get { return _bestStreak; }
        }

        /// <summary>
        /// How many columns have been cleared either by 21, 5 cards or Blackjack (not counting 'clearing' with busts)
        /// </summary>
        private int _columnsCleared = 0;

        public int ColumnsCleared
        {
            get { return _columnsCleared; }
        }

        /// <summary>
        /// How many times have you scored 21 continuously with each card one after another.
        /// Streak breaks when you play a card and you bust or don't clear a lane.
        /// </summary>
        private int _scoredStreak;

        public int ScoredStreak
        {
            get { return _scoredStreak; }
            set { _scoredStreak = value; }
        }

        /// <summary>
        /// Get first lane Deck.
        /// </summary>
        public Deck FirstLane
        {
            get { return LaneDecks[LaneOneIndex]; }
            set { LaneDecks[LaneOneIndex] = value; }
        }

        /// <summary>
        /// Get second lane Deck.
        /// </summary>
        public Deck SecondLane
        {
            get { return LaneDecks[LaneTwoIndex]; }
            set { LaneDecks[LaneTwoIndex] = value; }
        }

        /// <summary>
        /// Get third lane Deck.
        /// </summary>
        public Deck ThirdLane
        {
            get { return LaneDecks[LaneThreeIndex]; }
            set { LaneDecks[LaneThreeIndex] = value; }
        }

        /// <summary>
        /// Get fourth lane Deck.
        /// </summary>
        public Deck FourthLane
        {
            get { return LaneDecks[LaneFourIndex]; }
            set { LaneDecks[LaneFourIndex] = value; }
        }

        private Run21StateSnapshotManager _stateSnapshotManager = new Run21StateSnapshotManager(Gameplay.instance);

        public Run21StateSnapshotManager StateSnapshotManager
        {
            get { return _stateSnapshotManager; }
        }

        /// <summary>
        /// Returns total cards unused for scoring, includes busted cards.
        /// </summary>
        public int UnusedCardCount
        {
            get
            {
                return DrawDeck.CardCount 
                    + ActiveCardDeck.CardCount 
                    + FirstLane.CardCount 
                    + SecondLane.CardCount 
                    + ThirdLane.CardCount 
                    + FourthLane.CardCount 
                    + BustedCardCount;
            }
        }

        public Run21(Run21Score.Scoring scoringData)
        {
            Score = new Run21Score(scoringData);
            for (int i = 0; i < LaneDecks.Length; i++)
            {
                LaneDecks[i] = new Deck();
            }
        }

        public static bool IsCardBlackjack(Card card)
        {
            return card.Rank == CardRank.Jack && (card.Suit == CardSuit.Clubs || card.Suit == CardSuit.Spades);
        }

        /// <summary>
        /// Reset the game to a starting state, with drawing deck full and 1d and every other deck empty.
        /// Doesn't draw the first ActiveCard. Doesn't recreate instances in case they are hooked up to something.
        /// </summary>
        /// <param name="tournamentId">If provided, it will be used as seed to ganerate a deck. Used in PvP so everyone gets the same cards.</param>
        public void Reset(int? tournamentId = null)
        {
            Score.Reset();
            _cardsPlayed = 0;
            _remainingCards = DeckSize;
            IsGameOver = false;
            for (int i = 0; i < LaneDecks.Length; i++)
            {
                LaneDecks[i].Clear();
            }

            DrawDeck.Clear();
            if (tournamentId.HasValue)
            {
                // tournament game, everybody in tournament must have same deck
                Deck tournamentDeck = Deck.FromSeed(tournamentId.Value);
                DrawDeck.Cards.AddRange(tournamentDeck.Cards);
            }
            else
            {
                // offline game, just random cards
                Deck.FillWithAllCards(DrawDeck);
                DrawDeck.Shuffle();
            }

            ActiveCardDeck.Clear();
        }

        /// <summary>
        /// Draws from draw deck and puts it into active card deck.
        /// </summary>
        public void DrawCard()
        {
            if (IsGameOver)
            {
                return;
            }

            DrawDeck.MoveTopCardToDeck(ActiveCardDeck);
            ActiveCardDeck.TopCard.FaceUp = true;
        }

        /// <summary>
        /// Play the active card into one of 4 lane decks.
        /// </summary>
        /// <param name="i">Index of the lane deck, 0-3</param>
        public void PlayCard(int i)
        {
            if (i < 0 || i >= LaneDecks.Length || IsGameOver)
            {
                return;
            }

            ActiveCardDeck.MoveTopCardToDeck(LaneDecks[i]);
            CheckLaneDeck(LaneDecks[i], i);
            _cardsPlayed++;
            _remainingCards--;
            StateSnapshotManager.TakeSnapshot(this);
            CheckGameOver();
        }

        public void SetPlayTime(float playTimeInSeconds, bool isDelta = false)
        {
            if (IsGameOver)
                return;

            if (isDelta)
            {
                Score.PlayTime += playTimeInSeconds;
            }
            else
            {
                Score.PlayTime = playTimeInSeconds;
            }

            CheckGameOver();
        }

        /// <summary>
        /// Called after playing a card, check if we scored 21 or bust.
        /// </summary>
        private void CheckLaneDeck(Deck laneDeck, int laneIndex)
        {
            if (IsGameOver)
            {
                return;
            }

            HiLowValue score = CalculateDeckValue(laneDeck);
            bool isBlackJack = laneDeck.TopCard.IsBlackJack;
            bool isValue21 = score.high == 21 || score.low == 21;
            bool isFiveCardsScore = laneDeck.CardCount == 5 && score.low <= 21;
            bool isBust = !isBlackJack && score.low > 21;
            int scoreEarned = 0;
            Deck deck = null;

            if (isBlackJack || isValue21 || isFiveCardsScore)
            {
                // SCORE!
                scoreEarned = Score.ScoreLaneDeck(isValue21,isBlackJack, isFiveCardsScore, _scoredStreak); // run21 score incremented here
                deck = laneDeck.Clone();
                laneDeck.Clear();
                _scoredStreak++;

                _bestStreak = _scoredStreak > _bestStreak ? _scoredStreak : _bestStreak;
                _columnsCleared++;
            }
            else if (isBust)
            {
                // BUST
                Score.Busts++;
                _bustedCardCount += laneDeck.CardCount;
                deck = laneDeck.Clone();
                laneDeck.Clear();
                _scoredStreak = 0;
            }
            else
            {
                // just played a card
                _scoredStreak = 0;
            }

            RaiseScoreEvent(laneIndex, scoreEarned, isBlackJack, isValue21, isFiveCardsScore, isBust, _scoredStreak > 0, deck);
        }

        public void EndGame()
        {
            IsGameOver = true;
            Score.CalculateFinalScore(this);
        }

        private void CheckGameOver()
        {
            if (StateSnapshotManager.IsUndoLastMoveInProgress)
            {
                return;
            }
            
            if (IsGameOver)
            {
                return;
            }

            bool isMaxBustsReached = Score.Busts >= BustsMax;
            bool isAllCardsPlayed = ActiveCardDeck.IsEmpty && DrawDeck.IsEmpty && _cardsPlayed > 0;
            bool isTimeExpired = Score.PlayTime >= PlayTimeMax;


            if (isMaxBustsReached || isAllCardsPlayed || isTimeExpired)
            {
                IsGameOver = true;
                Score.CalculateFinalScore(this);
                RaiseGameOverEvent();
            }
            
        }

        public HiLowValue CalculateDeckValue(Deck deck)
        {
            var result = new HiLowValue();
            foreach (var card in deck.Cards)
            {
                HiLowValue cardVal = GetCardValue(card);
                // if you would go over 21 with hig val, use low val
                result.high += (result.high + cardVal.high > 21) ? cardVal.low : cardVal.high;
                result.low += cardVal.low;
            }

            return result;
        }

        public HiLowValue GetCardValue(Card card)
        {
            switch (card.Rank)
            {
                case CardRank.Two:
                case CardRank.Three:
                case CardRank.Four:
                case CardRank.Five:
                case CardRank.Six:
                case CardRank.Seven:
                case CardRank.Eight:
                case CardRank.Nine:
                    int val = (int) card.Rank;
                    return new HiLowValue() {high = val, low = val};
                case CardRank.Ten:
                case CardRank.Jack:
                case CardRank.Queen:
                case CardRank.King:
                    return new HiLowValue() {high = 10, low = 10};
                case CardRank.Ace:
                    return new HiLowValue() {high = 11, low = 1};
                default:
                    throw new Exception("Not handled CardValue for switch case: " + card.Rank.ToString());
            }
        }

        public void TakeSnapshot()
        {
            _stateSnapshotManager.TakeSnapshot(this);
        }

        public bool UndoLastMove()
        {
            return _stateSnapshotManager.UndoLastMoveAnimated(this);
        }

        public bool IsUndoLastMoveAvailable()
        {
            return _stateSnapshotManager.IsUndoLastMoveAvailable();
        }

        private void RaiseScoreEvent(int laneIndex, int score, bool isBlackJack, bool isValue21, bool isFiveCardsScore, bool isBust, bool isStreak, Deck deck = null)
        {
            if (ScoreEvent != null)
            {
                ScoreEvent(new ScoreEvent(laneIndex, score, isBlackJack, isValue21, isFiveCardsScore, isBust, isStreak, deck));
            }
        }

        private void RaiseGameOverEvent()
        {
            if (GameOverEvent != null)
            {
                GameOverEvent(Taco.Run21.GameOverEvent.From(this));
            }
        }
        
        public bool IsCardCausingDeckBust(Card card, Deck laneDeck)
        {
            Deck deckToCheck = laneDeck.Clone();
            deckToCheck.PutTopCard(card.Clone());
            HiLowValue scoreCausedByCard = CalculateDeckValue(deckToCheck);
            return scoreCausedByCard.high > 21 && scoreCausedByCard.low > 21;
        }
    }
}