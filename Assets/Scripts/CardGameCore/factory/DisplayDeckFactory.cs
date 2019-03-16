using System;
using UnityEngine;

namespace FRG.Taco
{
    public class DisplayDeckFactory : ScriptableObject
    {
        public static DisplayDeckFactory instance
        {
            get { return CardGameData.Instance.DisplayDeckFactory; }
        }

        [SerializeField] private GameObject deckPrefab;

        public DisplayDeck Build(Deck logicDeck, string deckName = null)
        {
            return Build(deckName, logicDeck, Vector3.zero, Quaternion.identity);
        }

        public DisplayDeck Build(string deckName, Deck logicDeck, Vector3 position, Quaternion rotation)
        {
            DisplayDeck displayDeck = Instantiate(deckPrefab).GetComponent<DisplayDeck>();
            displayDeck.gameObject.name = string.IsNullOrEmpty(deckName) ? "deck_" + Guid.NewGuid() : deckName;

            foreach (var card in logicDeck.Cards)
            {
                displayDeck.PutTopCard(DisplayCardFactory.instance.Build(card), true); 
            }

            displayDeck.gameObject.transform.position = position;
            displayDeck.gameObject.transform.rotation = rotation;

            return displayDeck;
        }
    }
}