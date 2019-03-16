using System;
using System.Collections.Generic;
using System.Linq;
using FRG.Core;
using UnityEngine;

namespace FRG.Taco
{
    public class Deck
    {
        public event Action<CardEvent> CardAddedEvent;
        public event Action<CardEvent> CardRemovedEvent;
        private List<Card> _cards = new List<Card>();

        public bool IsEmpty
        {
            get { return _cards.Count == 0; }
        }

        public List<Card> Cards
        {
            get { return _cards; }
            set { _cards = value; }
        }

        public int CardCount
        {
            get { return _cards.Count; }
        }

        public Card TopCard
        {
            get { return _cards.Count == 0 ? null : _cards[_cards.Count - 1]; }
        }

        public Deck(params Card[] cards)
        {
            cards.ToList().ForEach(card => PutTopCard(card));
        }

        /// <summary>
        /// Returns a standard deck, filled with cards in order we call standard
        /// <see cref="Card.AllCards"/>
        /// </summary>
        public static Deck FullDeck(bool isFaceUp = true)
        {
            var deck = new Deck();
            FillWithAllCards(deck);
            
            deck.Cards.ForEach(card => card.FaceUp = isFaceUp);  
            return deck;
        }

        /// <summary>
        /// Returns an empty deck and signalizes it was intended.
        /// </summary>
        /// <returns></returns>
        public static Deck EmptyDeck()
        {
            return new Deck();
        }

        /// <summary>
        /// Puts all 52 cards in the deck (doesn't clear it beforehand) in order we call standard.
        /// <see cref="Card.AllCards"/>
        /// </summary>
        public static void FillWithAllCards(Deck drawDeck)
        {
            foreach (var card in Card.AllCards)
            {
                drawDeck.PutTopCard(card.Clone());
            }
        }

        /// <summary>
        /// Returns a random deck defined by the seed. For the same seed it will always
        /// choose the same order of cards.
        /// </summary>
        public static Deck FromSeed(int seed)
        {
            var deck = new Deck();
            FillWithAllCards(deck);
            ArrayUtil.RandomizeListFromSeed(deck.Cards, seed);
            return deck;
        }

        public void PutTopCard(Card card)
        {
            if (card == null)
            {
                throw new ArgumentException();
            }

            _cards.Add(card);

            RaiseCardAddedEvent(card);
        }

        public Card TakeTopCard()
        {
            if (_cards.Count == 0)
            {
                return null;
            }

            Card card = _cards[_cards.Count - 1];
            _cards.RemoveAt(_cards.Count - 1);
            RaiseCardRemovedEvent(card);

            return card;
        }

        public void MoveTopCardToDeck(Deck otherDeck)
        {
            var card = TakeTopCard();
            otherDeck.PutTopCard(card);
        }

        public void Shuffle()
        {
            if (_cards.Count == 0)
                return;

            ArrayUtil.RandomizeList(_cards);
        }

        public void Clear()
        {
            _cards.Clear();
        }

        /// <summary>
        /// Revers card order in the deck, the top is now the bottom and vice versa
        /// </summary>
        public Deck ReverseCards()
        {
            _cards.Reverse();
            return this;
        }

        public bool ContainsCard(Card card)
        {
            return _cards.Contains(card);
        }

        public bool ContainsCard(CardSuit suit, CardRank rank)
        {
            foreach (var card in _cards)
            {
                if (card.Suit == suit && card.Rank == rank)
                    return true;
            }

            return false;
        }

        public bool ContainsCards(Card[] cards)
        {
            foreach (var card in cards)
            {
                if (_cards.Contains(card))
                {
                    return false;
                }
            }

            return true;
        }

        public bool DeckEquals(Deck other)
        {
            if (CardCount != other.CardCount)
                return false;

            for (int i = 0; i < CardCount; i++)
            {
                if (!_cards[i].CardEquals(other.Cards[i]))
                    return false;
            }

            return true;
        }

        private void RaiseCardAddedEvent(Card card)
        {
            if (CardAddedEvent != null) // has subscribers
            {
                CardAddedEvent(CardEvent.From(card, this));
            }
        }

        private void RaiseCardRemovedEvent(Card card)
        {
            if (CardRemovedEvent != null) // has subscribers
            {
                CardRemovedEvent(CardEvent.From(card, this));
            }
        }

        public Deck Clone()
        {
            Deck clone = new Deck();
            _cards.ForEach(card => { clone._cards.Add(card.Clone()); });
            return clone;
        }
    }
}