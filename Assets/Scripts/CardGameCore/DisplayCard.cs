using System;
using UnityEngine;

namespace FRG.Taco
{
    [ExecuteInEditMode]
    public class DisplayCard : MonoBehaviour
    {
        [SerializeField] private Sprite _frontSprite;
        [SerializeField] private SpriteRenderer frontSpriteRenderer;
        [SerializeField] private SpriteRenderer backSpriteRenderer;

        public CardAnimationController cardAnimationController;

        private Vector3 backSpriteInitialScale;

        private Card _card;

        public Card Card
        {
            get { return _card; }
            set { _card = value; }
        }

        public Sprite FrontSprite
        {
            get { return _frontSprite; }
            set
            {
                _frontSprite = value;
                frontSpriteRenderer.sprite = _frontSprite;
            }
        }

        private void Awake()
        {
            if (_frontSprite != null)
            {
                frontSpriteRenderer.sprite = _frontSprite;
            }

            backSpriteInitialScale = backSpriteRenderer.transform.localScale;
            cardAnimationController = GetComponent<CardAnimationController>();
        }

        public void Flip(bool? isFaceup = null, float? flipDuration = null)
        {
            if (cardAnimationController.IsAnimationPlaying())
            {
                Debug.Log("Cannot flip card because card is being animated.");
                return;
            }

            if (isFaceup.HasValue)
            {
                if (Card.FaceUp == isFaceup.Value)
                {
                    return;
                }

                Card.FaceUp = isFaceup.Value;
            }
            else
            {
                Card.Flip();
            }

            if (flipDuration.HasValue && flipDuration.Value == 0)
            {
                transform.localRotation = Card.FaceUp ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
                return;
            }

            if (Card.FaceUp)
            {
                cardAnimationController.PlayAnimationFaceUp(flipDuration);
                return;
            }

            cardAnimationController.PlayAnimationFaceDown(flipDuration);
        }

        public void FlipFaceUp()
        {
            Flip(true);
        }

        public void FlipFaceDown()
        {
            Flip(false);
        }

        public void MoveTowardsDeck(DisplayDeck deck, float durationSeconds = 1, Action playbackEndCallback = null)
        {
            Vector3 worldPosInNewDeck = deck.GetCardPosition_World(deck.Cards.Count);
            cardAnimationController.MoveTowardsAnimated(worldPosInNewDeck, null, durationSeconds, playbackEndCallback);
        }

        public void MoveTowardsAnimated(Vector3? translationEnd, Quaternion? rotationEnd, float durationSeconds = 1, Action playbackEndCallback = null)
        {
            GoToDrawingHeight();
            cardAnimationController.MoveTowardsAnimated(translationEnd, rotationEnd, durationSeconds, playbackEndCallback);
        }

        public void MoveTowardsAnimated(Vector3? translationStart, Vector3? translationEnd, Quaternion? rotationStart, Quaternion? rotationEnd, float durationSeconds = 1, Action playbackEndCallback = null)
        {
            GoToDrawingHeight();
            cardAnimationController.MoveTowardsAnimated(translationStart, translationEnd, rotationStart, rotationEnd, durationSeconds, playbackEndCallback);
        }

        public void PlayBust(float delaySeconds, float duration, Action callback = null)
        {
            cardAnimationController.PlayBust(delaySeconds, duration, callback);
        }

        public void PlayCleared(float delaySeconds, float duration, Action callback = null)
        {
            cardAnimationController.PlayCleared(delaySeconds, duration, callback);
        }

        /// <summary>
        /// Set the card position so it's at draw height, <see cref="CardGameData.CardDrawHeight"/>. Use before animating card draw/move to another deck.
        /// Needed to solve being occluded from other cards in same deck if going down, or occluded in destination deck if coming from down.
        /// </summary>
        public void GoToDrawingHeight()
        {
            var pos = transform.localPosition;
            pos.z = CardGameData.Instance.CardDrawHeight;
            transform.localPosition = pos;
        }

        /// <summary>
        /// Adjust the scale of card sprites based on width and height of RectTransform values given here, both front and back.
        /// </summary>
        public void AdjustScaleForSize(float width, float height)
        {
            float scaleW = width / FrontSprite.texture.width * FrontSprite.pixelsPerUnit;
            float scaleH = height / FrontSprite.texture.height * FrontSprite.pixelsPerUnit;
            frontSpriteRenderer.transform.localScale = new Vector3(scaleW, scaleH, 1);
            backSpriteRenderer.transform.localScale = new Vector3(scaleW * backSpriteInitialScale.x, scaleH * backSpriteInitialScale.y, backSpriteInitialScale.z);
        }

        /// <summary>
        /// Destroy card.
        /// </summary>
        public void DestroyCard()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}