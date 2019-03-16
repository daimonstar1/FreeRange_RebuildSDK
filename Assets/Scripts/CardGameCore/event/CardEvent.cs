namespace FRG.Taco
{
    using System;

    public class CardEvent : EventArgs
    {
        public Card Card { get; set; }
        public Deck Deck { get; set; }

        public static CardEvent From(Card card, Deck deck)
        {
            return new CardEvent() { Card = card, Deck = deck };
        }
    }
}
