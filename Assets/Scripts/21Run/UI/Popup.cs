using System;
using FRG.Taco.Run21;
using UnityEngine;
using UnityEngine.UI;

namespace FRG.Taco
{
    public class Popup : MonoBehaviour
    {
        [SerializeField] public Image popupImage;
        [SerializeField] public Text popupScore;
        [SerializeField] private int _score;

        [SerializeField] private Animatable imageAnimatable;
        [SerializeField] private Animatable scoreAnimatable;

        [SerializeField] public GameObject bustScoreImage;

        public Gameplay _gameplay;
        private Vector3 imageStartPosition;
        private Vector3 imageEndPosition;
        private Vector3 scoreStartPosition;
        private Vector3 scoreEndPosition;

        private Vector3 mainScoreOffset;
        private bool _popupHasScore = true;
        private bool _isNonLanePopup;

        public PopupFactory.PopupEnum popupType;

        public event Action<Popup> OnToggledOff;


        /// <summary>
        /// Toggle popup.
        /// </summary>
        /// <param name="score"></param>
        public void TogglePopupOn(int score = 0)
        {
            ConfigurePopupComponentPositions(popupType);

            if (score > 0)
            {
                Score = score;
            }


            if (!PopupHasScore)
            {
                popupScore.enabled = false;
            }

            popupImage.enabled = true;
            popupScore.rectTransform.anchoredPosition = scoreStartPosition;
            popupScore.transform.localPosition = scoreStartPosition;
            popupImage.rectTransform.anchoredPosition = imageStartPosition;
            popupImage.transform.localPosition = imageStartPosition;

            AnimatePopup();
        }


        /// <summary>
        /// Toggle off popup.
        /// </summary>
        public void TogglePopupOff()
        {
            if (popupScore != null)
            {
                popupScore.text = string.Empty;
                popupScore.transform.localScale = Vector3.one;
                popupScore.rectTransform.anchoredPosition = scoreStartPosition;
                popupScore.transform.localPosition = scoreStartPosition;
            }

            if (popupImage != null)
            {
                popupImage.enabled = false;
                popupImage.transform.localScale = Vector3.one;
                popupImage.rectTransform.anchoredPosition = imageStartPosition;
                popupImage.transform.localPosition = imageStartPosition;
                Destroy(popupImage.transform.parent.gameObject);
            }

            _gameplay.TogglePopupAnimatingOff();


            if (OnToggledOff != null)
            {
                OnToggledOff(this);
            }
        }

        /// <summary>
        /// Animation definition for score popups (21, 5 cards and black jack)
        /// </summary>
        public void AnimatePopup()
        {
            var updwardMotionDuration = _isNonLanePopup ? Run21Data.Instance.animationConfig.nonLanePopupUpwardMotionDuration : Run21Data.Instance.animationConfig.lanePopupUpwardMotionDuration;

            if (_popupHasScore)
            {
                // move score up    
                scoreAnimatable.TranslateRectTransformToLocalSpacePlay(scoreEndPosition, updwardMotionDuration, () =>
                {
                    // score fade to green
                    popupScore.CrossFadeColor(Color.green, 0.5f, true, true);

                    // score moves near displayed main score
                    scoreAnimatable.TranslateTransformToLocalSpacePlay(mainScoreOffset, Run21Data.Instance.animationConfig.moveTowardsMainScoreDuration, () =>
                    {
                        // increment score
                        _gameplay.IncrementOverTimeTurnOffScorePopup(Run21Data.Instance.animationConfig.scoreIncrementingDuration, this);
                        
                        // toggled of by _gameplay
                    });
                });
            }


            if (popupType == PopupFactory.PopupEnum.BustedLanePopup)
            {
                // move image up
                imageAnimatable.TranslateTransformToWorldSpacePlay(bustScoreImage.transform.position, Run21Data.Instance.animationConfig.moveToBustedScoreDuration, () =>
                {
                    // shrink and make image disappear
                    imageAnimatable.ScalePlay(0f, Run21Data.Instance.animationConfig.shrinkPopupImage, () =>
                    {
                        if (!_popupHasScore)
                        {

                            TogglePopupOff();
                        }
                    });
                });

                return;
            }

            // move image up
            imageAnimatable.TranslateRectTransformToLocalSpacePlay(imageEndPosition, updwardMotionDuration, () =>
            {
                // shrink and make image disappear
                imageAnimatable.ScalePlay(0f, Run21Data.Instance.animationConfig.shrinkPopupImage, () =>
                {
                    if (!_popupHasScore)
                    {
                        TogglePopupOff();
                    }
                });
            });
        }

        /// <summary>
        /// If you dont want to display the popup score.
        /// </summary>
        public bool PopupHasScore
        {
            get { return _popupHasScore; }
            set { _popupHasScore = value; }
        }

        /// <summary>
        /// Mark a popup as not being displayed above a lane. (i.e. out of time)
        /// </summary>
        public bool IsNonLanePopup
        {
            get { return _isNonLanePopup; }
            set { _isNonLanePopup = value; }
        }

        public int Score
        {
            get { return _score; }
            set
            {
                _score = value;
                if (popupScore != null)
                {
                    popupScore.text = value > 0 ? "+" + value : "";
                }
            }
        }

        private void Start()
        {
            _gameplay = Gameplay.instance;
            mainScoreOffset = transform.InverseTransformPoint(_gameplay.mainScore.transform.position) + new Vector3(150, -50, 0);
        }

        public void ConfigurePopupComponentPositions(PopupFactory.PopupEnum popupType)
        {
            switch (popupType)
            {
                case PopupFactory.PopupEnum.CombosPopup:
                    imageStartPosition = Run21Data.Instance.animationConfig.imageStartPositionCombo;
                    imageEndPosition = Run21Data.Instance.animationConfig.imageEndPositionCombo;
                    scoreStartPosition = Run21Data.Instance.animationConfig.scoreStartPositionCombo;
                    scoreEndPosition = Run21Data.Instance.animationConfig.scoreEndPositionCombo;
                    break;

                case PopupFactory.PopupEnum.Run21Score:

                    imageStartPosition = Run21Data.Instance.animationConfig.imageStartPosition21Run;
                    imageEndPosition = Run21Data.Instance.animationConfig.imageEndPosition21Run;
                    scoreStartPosition = Run21Data.Instance.animationConfig.scoreStartPosition21Run;
                    scoreEndPosition = Run21Data.Instance.animationConfig.scoreEndPosition21Run;
                    break;
                case PopupFactory.PopupEnum.BustedLanePopup:

                    imageStartPosition = Run21Data.Instance.animationConfig.imageStartPositionBustedLane;
                    imageEndPosition = Run21Data.Instance.animationConfig.imageEndPositionBustedLane;
                    scoreStartPosition = Run21Data.Instance.animationConfig.scoreStartPositionBustedLane;
                    scoreEndPosition = Run21Data.Instance.animationConfig.scoreEndPositionBustedLane;
                    break;
                case PopupFactory.PopupEnum.FiveCardScore:

                    imageStartPosition = Run21Data.Instance.animationConfig.imageStartPosition5Card;
                    imageEndPosition = Run21Data.Instance.animationConfig.imageEndPosition5Card;
                    scoreStartPosition = Run21Data.Instance.animationConfig.scoreStartPosition5Card;
                    scoreEndPosition = Run21Data.Instance.animationConfig.scoreEndPosition5Card;
                    break;

                case
                    PopupFactory.PopupEnum.BlackjackScore:

                    imageStartPosition = Run21Data.Instance.animationConfig.imageStartPositionWildcard;
                    imageEndPosition = Run21Data.Instance.animationConfig.imageEndPositionWildcard;
                    scoreStartPosition = Run21Data.Instance.animationConfig.scoreStartPositionWildcard;
                    scoreEndPosition = Run21Data.Instance.animationConfig.scoreEndPositionWildcard;
                    break;

                case PopupFactory.PopupEnum.GoodRunStreak:
                case PopupFactory.PopupEnum.GreatRunStreak:
                case PopupFactory.PopupEnum.AmazingRunStreak:
                case PopupFactory.PopupEnum.OutstandingRunStreak:

                    imageStartPosition = Run21Data.Instance.animationConfig.imageStartPositionStreak;
                    imageEndPosition = Run21Data.Instance.animationConfig.imageEndPositionStreak;
                    scoreStartPosition = Run21Data.Instance.animationConfig.scoreStartPositionStreak;
                    scoreEndPosition = Run21Data.Instance.animationConfig.scoreEndPositionStreak;
                    break;
                case PopupFactory.PopupEnum.OutOfTime:

                    imageStartPosition = Run21Data.Instance.animationConfig.imageStartPositionOutOfTime;
                    imageEndPosition = Run21Data.Instance.animationConfig.imageEndPositionOutOfTime;
                    scoreStartPosition = Run21Data.Instance.animationConfig.scoreStartPositionOutOfTime;
                    scoreEndPosition = Run21Data.Instance.animationConfig.scoreEndPositionOutOfTime;
                    break;
                case PopupFactory.PopupEnum.NoBustBonus:

                    imageStartPosition = Run21Data.Instance.animationConfig.imageStartPositionNoBust;
                    imageEndPosition = Run21Data.Instance.animationConfig.imageEndPositionNoBust;
                    scoreStartPosition = Run21Data.Instance.animationConfig.scoreStartPositionNoBust;
                    scoreEndPosition = Run21Data.Instance.animationConfig.scoreEndPositionNoBust;
                    break;

                case PopupFactory.PopupEnum.EmptyLaneBonus:

                    imageStartPosition = Run21Data.Instance.animationConfig.imageStartPositionEmptyLane;
                    imageEndPosition = Run21Data.Instance.animationConfig.imageEndPositionEmptyLane;
                    scoreStartPosition = Run21Data.Instance.animationConfig.scoreStartPositionEmptyLane;
                    scoreEndPosition = Run21Data.Instance.animationConfig.scoreEndPositionEmptyLane;
                    break;

                case PopupFactory.PopupEnum.PerfectGameBonus:

                    imageStartPosition = Run21Data.Instance.animationConfig.imageStartPositionPerfectScore;
                    imageEndPosition = Run21Data.Instance.animationConfig.imageEndPositionPerfectScore;
                    scoreStartPosition = Run21Data.Instance.animationConfig.scoreStartPositionPerfectScore;
                    scoreEndPosition = Run21Data.Instance.animationConfig.scoreEndPositionPerfectScore;
                    break;

                default:

                    imageStartPosition = Run21Data.Instance.animationConfig.imageStartPositionPerfectStreak;
                    imageEndPosition = Run21Data.Instance.animationConfig.imageEndPositionPerfectStreak;
                    scoreStartPosition = Run21Data.Instance.animationConfig.scoreStartPositionPerfectStreak;
                    scoreEndPosition = Run21Data.Instance.animationConfig.scoreEndPositionPerfectStreak;
                    break;
            }
        }
    }
}