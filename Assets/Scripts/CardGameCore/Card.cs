namespace FRG.Taco
{
    using System;
    using System.Collections.Generic;

    [System.Serializable]
    public enum CardSuit
    {
        Hearts,
        Clubs,
        Diamonds,
        Spades
    }

    [System.Serializable]
    public enum CardRank
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    public class Card
    {
        public static List<Card> AllCards { get; private set; }

        static Card()
        {
            AllCards = new List<Card>();
            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
                {
                    AllCards.Add(new Card(rank, suit));
                }
            }
        }

        private CardSuit _suit;

        private CardRank _rank;

        private bool _isFaceUp = true;

        public Card(CardRank rank, CardSuit suit)
        {
            _rank = rank;
            _suit = suit;
        }

        public CardSuit Suit
        {
            get { return _suit; }
            set { _suit = value; }
        }

        public CardRank Rank
        {
            get { return _rank; }
            set { _rank = value; }
        }

        public bool IsBlackJack
        {
            get { return _rank == CardRank.Jack && (_suit == CardSuit.Clubs || _suit == CardSuit.Spades); }
        }

        public void Flip()
        {
            _isFaceUp = !_isFaceUp;
        }

        public bool FaceUp
        {
            get { return _isFaceUp; }
            set { _isFaceUp = value; }
        }

        public Card Clone()
        {
            var card = new Card(_rank, _suit);
            card.FaceUp = _isFaceUp;
            return card;
        }

        public bool CardEquals(Card other)
        {
            return Rank == other.Rank && Suit == other.Suit;
        }

        public override string ToString()
        {
            return string.Format("{0}_of_{1}", ((int)Rank) <= 10 ?((int)Rank).ToString() : Rank.ToString(), Suit.ToString());
        }
    }
}
