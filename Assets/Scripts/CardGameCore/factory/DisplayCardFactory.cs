namespace FRG.Taco
{
    using UnityEngine;

    public class DisplayCardFactory : ScriptableObject
    {
        public static DisplayCardFactory instance
        {
            get { return CardGameData.Instance.DisplayCardFactory; }
        }

        [SerializeField] private GameObject cardPrefab;

        [SerializeField] private Sprite[] spritesHearts = new Sprite[13];

        [SerializeField] private Sprite[] spritesClubs = new Sprite[13];

        [SerializeField] private Sprite[] spritesDiamonds = new Sprite[13];

        [SerializeField] private Sprite[] spritesSpades = new Sprite[13];

        public DisplayCard Build(Card card)
        {
            return Build(card, Vector3.zero, Quaternion.identity);
        }

        public DisplayCard Build(Card card, Vector3 position, Quaternion rotation)
        {
            DisplayCard displayCard = Instantiate(cardPrefab).GetComponent<DisplayCard>();
            displayCard.Card = card;
            displayCard.gameObject.name = card.ToString();
            displayCard.gameObject.transform.position = position;
            displayCard.gameObject.transform.rotation = rotation;
            displayCard.FrontSprite = GetSprite(card.Suit, card.Rank);

            return displayCard;
        }

        public Sprite GetSprite(CardSuit suit, CardRank rank)
        {
            Sprite[] suitArray = null;
            switch (suit)
            {
                case CardSuit.Hearts:
                    suitArray = spritesHearts;
                    break;
                case CardSuit.Clubs:
                    suitArray = spritesClubs;
                    break;
                case CardSuit.Diamonds:
                    suitArray = spritesDiamonds;
                    break;
                case CardSuit.Spades:
                    suitArray = spritesSpades;
                    break;
                default:
                    return null;
            }

            return suitArray != null && suitArray.Length == 13 ? suitArray[(int) rank - 2] : null;
        }
    }
}