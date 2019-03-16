using System;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace FRG.Taco
{
    [CustomEditor(typeof(DisplayDeck))]
    public class DisplayDeckEditor : Editor
    {
        public CardGameData cardGameData;
        public DisplayDeck.DisplayOptions displayOptions;

        public override void OnInspectorGUI()
        {
            var displayDeck = target as DisplayDeck;

            if (GUILayout.Button("Reposition the cards", EditorStyles.miniButton))
            {
                displayDeck.DeckDisplayOptions = DisplayDeck.DisplayOptions.Down;
                displayDeck.RefreshDisplay();
            }

            if (GUILayout.Button("Destroy top card", EditorStyles.miniButton))
            {
                var cardDisplay = displayDeck.TakeTopCard(true);
                displayDeck.RecreateDisplay();
                DestroyImmediate(cardDisplay.gameObject);
            }

            if (GUILayout.Button("Destroy all cards", EditorStyles.miniButton))
            {
                displayDeck.RemoveAllCards();
                displayDeck.RecreateDisplay();
            }

            if (GUILayout.Button("Add random top card", EditorStyles.miniButton))
            {
                displayDeck.Deck.PutTopCard(new Card(GetRandomCardRank(), GetRandomCardSuit()));
                displayDeck.RecreateDisplay();
            }

            if (GUILayout.Button("Play bust animation", EditorStyles.miniButton))
            {
                displayDeck.PlayBustDeck(2.5f);
            }

            if (GUILayout.Button("Card count", EditorStyles.miniButton))
            {
                Debug.Log($"card cound: {displayDeck.Cards.Count}");
                string c = "";

                
                displayDeck.Cards.ForEach(
                    card => { c += ", " + card.gameObject.name+"_display"; }
                );
                
                displayDeck.Deck.Cards.ForEach(
                    card => { c += card.ToString()+"_logic"; }
                );
                Debug.Log($"{c}");
            }


            DrawDefaultInspector();
        }

        private CardRank GetRandomCardRank()
        {
            Array ranks = Enum.GetValues(typeof(CardRank));
            Random random = new Random();
            return (CardRank) ranks.GetValue(random.Next(ranks.Length));
        }

        private CardSuit GetRandomCardSuit()
        {
            Array ranks = Enum.GetValues(typeof(CardSuit));
            Random random = new Random();
            return (CardSuit) ranks.GetValue(random.Next(ranks.Length));
        }
    }
}