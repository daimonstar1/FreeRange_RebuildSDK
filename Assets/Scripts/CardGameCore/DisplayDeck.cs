using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace FRG.Taco
{
    public class DisplayDeck : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IDragHandler, IDropHandler
    {
        public static event Action<DisplayDeck> OnDown;
        public static event Action<DisplayDeck> OnReleased;
        public static event Action<DisplayDeck> OnClicked;

        public RectTransform parentOfCards;

        [SerializeField]
        private DisplayOptions _deckDisplayOptions = new DisplayOptions();

        [SerializeField] private List<DisplayCard> _cards = new List<DisplayCard>();
        private Deck _deck = new Deck();

        [SerializeField]
        private Collider deckCollider;

        /// <summary>
        /// Puts all 52 cards in the DeisplayDeck (doesn't clear it beforehand).
        /// </summary>
        /// <param name="displayDeck">DisplayDeck to fill</param>
        /// <param name="shouldRecreateCards">True to destroy existing DisplayCard game objects and create new ones, false to do nothing.</param>
        public static void FillWithAllCards(DisplayDeck displayDeck, bool shouldRecreateCards = true)
        {
            Deck.FillWithAllCards(displayDeck.Deck);
            if (shouldRecreateCards)
            {
                displayDeck.RecreateDisplay();
            }
        }

        /// <summary>
        /// Is deck interactible? Turns on/off collider
        /// </summary>
        public bool Interactible
        {
            get
            {
                return deckCollider.enabled;
            }
            set
            {
                deckCollider.enabled = value;
            }
        }

        public Deck Deck
        {
            get { return _deck; }
            set
            {
                _deck = value;
                RecreateDisplay();
            }
        }

        public List<DisplayCard> Cards
        {
            get { return _cards; }
            set { _cards = value; }
        }

        public DisplayOptions DeckDisplayOptions
        {
            get { return _deckDisplayOptions; }
            set { _deckDisplayOptions = value; }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (OnClicked != null)
            {
                OnClicked(this);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (OnReleased != null)
            {
                OnReleased(this);
            }
        }

        public void OnDrag(PointerEventData data)
        {
            if (OnDown != null)
            {
                OnDown(this);
            }
        }

        public void OnDrop(PointerEventData data)
        {
            if (OnReleased != null)
            {
                OnReleased(this);
            }
        }


        public void PutTopCard(DisplayCard displayCard, bool putLogicCard = false)
        {
            displayCard.transform.SetParent(parentOfCards, false);
            _cards.Add(displayCard);
            if (putLogicCard)
            {
                Deck.PutTopCard(displayCard.Card);
            }

            RefreshCardPositions();
        }

        public DisplayCard TopCard
        {
            get { return _cards.Count == 0 ? null : _cards[_cards.Count - 1]; }
        }

        public DisplayCard TakeTopCard(bool takeLogicCard = false)
        {
            if (_cards.Count == 0)
                return null;

            if (takeLogicCard)
            {
                Deck.TakeTopCard();
            }

            var topDisplayCard = _cards[_cards.Count - 1];
            _cards.RemoveAt(_cards.Count - 1);
            return topDisplayCard;
        }

        public void RemoveAllCards()
        {
            // destroy all card display and clear the list
            foreach (var card in _cards)
            {
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(card.gameObject);
                else
                    Destroy(card.gameObject);
            }

            _cards.Clear();
            Deck.Clear();
        }

        public void DestroyDeck()
        {
            RemoveAllCards();

            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Destroys the current display and recreates DisplayCard list from Deck
        /// </summary>
        public void RecreateDisplay()
        {
            // destroy all card display and clear the list
            foreach (var card in _cards)
            {
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(card.gameObject);
                else
                    Destroy(card.gameObject);
            }

            _cards.Clear();

            // create displays
            foreach (var card in Deck.Cards)
            {
                CreateDisplayCardAtTop(card);
            }
        }

        /// <summary>
        /// Repositions current DisplayCards to match wanted deck stacking layout and card facing, <see cref="_deckDisplayOptions"/>
        /// </summary>
        public void RefreshDisplay()
        {
            RefreshCardPositions();
        }

        public void CreateDisplayCardAtTop(Card card)
        {
            DisplayCard newDisplayCard = CardGameData.Instance.DisplayCardFactory.Build(card);
            _cards.Add(newDisplayCard);
            RefreshCardPositions();
        }

        public void ShowTopNCards(int n)
        {
            int lastCardIndex = Deck.Cards.Count - 1;
            int firstHiddenCardIndex = lastCardIndex - n;
            for (int i = lastCardIndex; i >= 0; i--)
            {
                Deck.Cards[i].FaceUp = i > firstHiddenCardIndex;
            }

            RefreshCardPositions();
        }

        public void HideTopNCards(int n)
        {
            int lastCardIndex = Deck.Cards.Count - 1;
            int firstShownCardIndex = lastCardIndex - n;
            for (int i = lastCardIndex; i >= 0; i--)
            {
                Deck.Cards[i].FaceUp = i < firstShownCardIndex;
            }

            RefreshCardPositions();
        }

        private void RefreshCardPositions()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                var displayCard = _cards[i];
                displayCard.transform.SetParent(parentOfCards, false);
                displayCard.transform.localPosition = GetCardPosition_Local(i);
                displayCard.transform.localRotation = displayCard.Card.FaceUp ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
                displayCard.AdjustScaleForSize(parentOfCards.rect.width * _deckDisplayOptions.cardScale.x, parentOfCards.rect.height * _deckDisplayOptions.cardScale.y);
            }
        }


        /// <summary>
        /// For given card order retrieves it's intended local position in given deck. 
        /// </summary>
        /// <param name="cardIndex"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private Vector3 GetCardPosition_Local(float cardIndex)
        {
            return _deckDisplayOptions.cardPadding * cardIndex + _deckDisplayOptions.cardOffset;
        }

        /// <summary>
        /// For given card order retrieves it's intended world position in given deck.
        /// World positions are targetable by animation system i.e. <see cref="DisplayCard.MoveTowardsAnimated"/> 
        /// </summary>
        public Vector3 GetCardPosition_World(float cardIndex)
        {
            var localPoint = _deckDisplayOptions.cardPadding * cardIndex + _deckDisplayOptions.cardOffset;

            return parentOfCards.TransformPoint(localPoint);
        }

        /// <summary>
        /// Takes the top card from the deck and put it on top of target deck, animated.
        /// </summary>
        public void DealTowardsDeckAnimated(float timeSingleCardAnimating, float timeBetweenCards, DisplayDeck targetDeck, Action dealingCompletedCallback = null)
        {
            StartCoroutine(DeckDealingCoroutine(timeSingleCardAnimating, timeBetweenCards, targetDeck, dealingCompletedCallback));
        }

        private IEnumerator DeckDealingCoroutine(float timeSingleCardAnimating, float timeBetweenCards, DisplayDeck targetDeck, Action dealingCompletedCallback = null)
        {
            int cards = Deck.CardCount;
            for (int i = 0; i < cards; i++)
            {
                int positionInTargetDeck = i;
                DisplayCard cardToDeal = TakeTopCard(true);
                var dealOverAction = Deck.CardCount == 0 ? dealingCompletedCallback : null;
                DealSingleCardAnimated(timeSingleCardAnimating, cardToDeal, positionInTargetDeck, targetDeck, dealOverAction);

                if (timeBetweenCards > 0)
                {
                    yield return new WaitForSeconds(timeBetweenCards);
                }
            }
        }

        private void DealSingleCardAnimated(float duration, DisplayCard card, int positionInStartingDeck, DisplayDeck targetDeck, Action postCardDealtAction = null, bool putLogicCard = true)
        {
            var target = targetDeck.GetCardPosition_World(positionInStartingDeck);

            card.MoveTowardsAnimated(target, null, duration, () =>
            {
                targetDeck.PutTopCard(card, putLogicCard);
                if (postCardDealtAction != null)
                {
                    postCardDealtAction.Invoke();
                }
            });
        }

        public void PlayBustDeck(float duration, Action callback = null)
        {
            _cards.Reverse();
            for (int i = 0; i < _cards.Count; i++)
            {
                if (i == _cards.Count - 1)
                {
                    _cards[i].PlayBust(i * 0.1f, duration, callback);
                    continue;
                }

                _cards[i].PlayBust(i * 0.1f, duration);
            }
        }
        
        
        public void PlayClearedDeck(float duration, Action callback = null)
        {
            _cards.Reverse();
            for (int i = 0; i < _cards.Count; i++)
            {
                if (i == _cards.Count - 1)
                {
                    _cards[i].PlayBust(i * 0.1f, duration, callback);
                    continue;
                }

                _cards[i].PlayBust(i * 0.1f, duration);
            }
        }


        [Serializable]
        public class DisplayOptions
        {
            /// <summary>
            /// Bottom card in the deck is the starting point, the rest of the deck is filled in a direction
            /// </summary>
            public enum Direction
            {
                None,
                Up,
                Down,
                Left,
                Right
            }

            /// <summary>
            /// Apply padding between cards when positioning them.
            /// </summary>
            public Vector3 cardPadding = new Vector3(0, 0, -1);

            /// <summary>
            /// Apply local offset to cards when positioning them.
            /// TODO same as moving entire deck, not sure if this is needed.
            /// </summary>
            public Vector3 cardOffset = Vector3.zero;

            /// <summary>
            /// Apply this local scale to cards.
            /// </summary>
            public Vector3 cardScale = Vector3.one;

            public static readonly DisplayOptions Default = new DisplayOptions();
            public static readonly DisplayOptions Down = From(Direction.Down, 50);
            public static readonly DisplayOptions Right = From(Direction.Right, 6);

            public static DisplayOptions From(Direction dir, float padding)
            {
                switch (dir)
                {
                    case Direction.None:
                        return Default;
                    case Direction.Up:
                        return new DisplayOptions {cardPadding = new Vector3(0, padding, -1)};
                    case Direction.Down:
                        return new DisplayOptions {cardPadding = new Vector3(0, -padding, -1)};
                    case Direction.Left:
                        return new DisplayOptions {cardPadding = new Vector3(-padding, 0, -1)};
                    case Direction.Right:
                        return new DisplayOptions {cardPadding = new Vector3(padding, 0, -1)};
                    default:
                        Debug.LogError("DisplayDeck.DisplayOptions.From switch case not handled for value: " + dir);
                        return new DisplayOptions();
                }
            }
        }
    }
}