using System;
using UnityEngine;

namespace FRG.Taco.Run21
{
    [CreateAssetMenu(menuName = "AnimationConfig21Run")]
    [Serializable]
    /// <summary>
    /// Class responsible for storing and tweeking of various ingame animation parameters
    /// </summary>
    public class AnimationConfig21Run : ScriptableObject
    {
        
        [Header("Initial shuffle")] [Tooltip("Between cards being started animate towards their destination.")] [SerializeField]
        public float TimeToDealNextCardInitialShuffle = 2f / 52;

        [Tooltip("Time for single card to animate.")] [SerializeField]
        public float DealSingleCardInitialShuffle = 0.2f;

        [Header("Play card")] 
        [Tooltip("Time for single card to move from active deck to lane deck.")] [SerializeField]
        public float playSingleCardDuration = 0.3f;
        
        [Header("Lanes")]
        [Tooltip("Delay between displayed popups")] [SerializeField]
        public float timeTillNextPopup = 1.7f;

        [Header("Fixed length animations")]
        [Tooltip("Scored deck. Read only.")] [SerializeField]
        public float scoredDeckReadOnly = 1f;
        
        [Tooltip("Busted deck. Read only.")] [SerializeField]
        public float bustedDeckReadOnly = 1f;

        [Header("Lane popups (score, streak)")]
        [Tooltip("Score/streak popup upward motion duration")] [SerializeField]
        public float lanePopupUpwardMotionDuration = 1f;
        
        [Header("Non lane popups (perfect streak, no bust..)")]
        [Tooltip("Combo/nobust popup upward motion duration")] [SerializeField]
        public float nonLanePopupUpwardMotionDuration = 1.5f;
        
        [Header("Popups general")]
        [Tooltip("Popup score move towards displayed score")] [SerializeField]
        public float moveTowardsMainScoreDuration = 0.5f;
        
        [Tooltip("Shrink popup image")] [SerializeField]
        public float shrinkPopupImage = 0.5f;
        
        [Tooltip("Popup score increment")] [SerializeField]
        public float scoreIncrementingDuration = 1f;

        [Header("Combo popup")]
        [Tooltip("Upward motion duration")]     [SerializeField]    public float UpwardMotionDurationCombos = 1f;
        [Tooltip("Image starting position")]    [SerializeField]    public Vector3 imageStartPositionCombo;
        [Tooltip("Image ending position")]      [SerializeField]    public Vector3 imageEndPositionCombo;
        [Tooltip("Score starting position")]    [SerializeField]    public Vector3 scoreStartPositionCombo;
        [Tooltip("Score ending position")]      [SerializeField]    public Vector3 scoreEndPositionCombo;

        [Header("21 Run popup")]
        [Tooltip("Upward motion duration")]                   [SerializeField] public float upwardMotionDurationRun21 = 1f;
        [Tooltip("Image starting position")]  [SerializeField] public Vector3 imageStartPosition21Run;
        [Tooltip("Image ending position")]    [SerializeField] public Vector3 imageEndPosition21Run;
        [Tooltip("Score starting position")]  [SerializeField] public Vector3 scoreStartPosition21Run;
        [Tooltip("Score ending position")]    [SerializeField] public Vector3 scoreEndPosition21Run;
        
        [Header("Busted Lane popup")]
        [Tooltip("Duration of movement to BustScore ")]         [SerializeField] public float moveToBustedScoreDuration = 1f;
        [Tooltip("Upward motion duration")]         [SerializeField] public float upwardMotionDurationBustedLane = 1f;
        [Tooltip("Image starting position")]  [SerializeField] public Vector3 imageStartPositionBustedLane;
        [Tooltip("Image ending position")]    [SerializeField] public Vector3 imageEndPositionBustedLane;
        [Tooltip("Score starting position")]  [SerializeField] public Vector3 scoreStartPositionBustedLane;
        [Tooltip("Score ending position")]    [SerializeField] public Vector3 scoreEndPositionBustedLane;
        
        [Header("5 Cards popup")]
        [Tooltip("5 Cards popup")] [SerializeField]
        public float upwardMotionDuration5Card = 1f;
        [Tooltip("Image start")]   [SerializeField] public Vector3 imageStartPosition5Card;
        [Tooltip("Image end")]     [SerializeField]   public Vector3 imageEndPosition5Card;
        [Tooltip("Score start")]   [SerializeField] public Vector3 scoreStartPosition5Card;
        [Tooltip("Score end")]     [SerializeField]   public Vector3 scoreEndPosition5Card;

        [Header("Wildcard popup")]
        [Tooltip("Upward motion duration")] [SerializeField] public float upwardMotionDurationWildcard = 1f;
        [Tooltip("Image start")]    [SerializeField] public Vector3 imageStartPositionWildcard;
        [Tooltip("Image end")]      [SerializeField]   public Vector3 imageEndPositionWildcard;
        [Tooltip("Score start")]    [SerializeField] public Vector3 scoreStartPositionWildcard;
        [Tooltip("Score end")]      [SerializeField]   public Vector3 scoreEndPositionWildcard;

        [Header("Streak popup")]
        [Tooltip("Upward motion duration")]                  [SerializeField] public float upwardMotionDurationStreak = 1f;
        [Tooltip("Image start")]  [SerializeField] public Vector3 imageStartPositionStreak;
        [Tooltip("Image end")]    [SerializeField]   public Vector3 imageEndPositionStreak;
        [Tooltip("Score start")]  [SerializeField] public Vector3 scoreStartPositionStreak;
        [Tooltip("Score end")]    [SerializeField]   public Vector3 scoreEndPositionStreak;

        [Header("Perfect Streak popup (NON LANE)")]
        [Tooltip("Upward motion duration")] [SerializeField]     public float upwardMotionDurationPerfectStreak = 1.5f;  
        [Tooltip("Image start")]  [SerializeField]   public Vector3 imageStartPositionPerfectStreak;
        [Tooltip("Image end")]    [SerializeField]   public Vector3 imageEndPositionPerfectStreak;
        [Tooltip("Score start")]  [SerializeField]   public Vector3 scoreStartPositionPerfectStreak;
        [Tooltip("Score end")]    [SerializeField]   public Vector3 scoreEndPositionPerfectStreak;

        
        [Header("Empty lane bonus popup")]
        [Tooltip("Upward motion duration")] [SerializeField]
        public float upwardMotionDurationEmptyLaneBonus = 1f;
        [Tooltip("Image start")]   [SerializeField] public Vector3 imageStartPositionEmptyLane;
        [Tooltip("Image end")]     [SerializeField]   public Vector3 imageEndPositionEmptyLane;
        [Tooltip("Score start")]   [SerializeField] public Vector3 scoreStartPositionEmptyLane;
        [Tooltip("Score end")]     [SerializeField]   public Vector3 scoreEndPositionEmptyLane;

        [Header("No bust bonus popup (NON LANE)")]
        [Tooltip("Upward motion duration")] [SerializeField]
        public float upwardMotionDurationNoBustBonus = 1.5f;
        [Tooltip("Image start")]   [SerializeField] public Vector3 imageStartPositionNoBust;
        [Tooltip("Image end")]     [SerializeField]   public Vector3 imageEndPositionNoBust;
        [Tooltip("Score start")]   [SerializeField] public Vector3 scoreStartPositionNoBust;
        [Tooltip("Score end")]     [SerializeField]   public Vector3 scoreEndPositionNoBust;

        [Header("Perfect score popup (NON LANE)")]
        [Tooltip("Perfect score popup")] [SerializeField]
        public float upwardMotionDurationPerfectScore = 1.5f;
        [Tooltip("Image start")]   [SerializeField] public Vector3 imageStartPositionPerfectScore;
        [Tooltip("Image end")]     [SerializeField]   public Vector3 imageEndPositionPerfectScore;
        [Tooltip("Score start")]   [SerializeField] public Vector3 scoreStartPositionPerfectScore;
        [Tooltip("Score end")]     [SerializeField]   public Vector3 scoreEndPositionPerfectScore;
        
        [Header("Out of time popup (NON LANE)")]
        [Tooltip("Out of time popup")] [SerializeField]
        public float upwardMotionDurationOutOfTime = 1f;
        [Tooltip("Image start")]    [SerializeField] public Vector3 imageStartPositionOutOfTime;
        [Tooltip("Image end")]      [SerializeField]   public Vector3 imageEndPositionOutOfTime;
        [Tooltip("Score start")]    [SerializeField] public Vector3 scoreStartPositionOutOfTime;
        [Tooltip("Score end")]      [SerializeField]   public Vector3 scoreEndPositionOutOfTime;
        
        
        [Header("Undo last move deck animation")] 
        [Tooltip("Part 1. When animating cleared lane. Time to deal single card from the animation deck to the cleared lane.")] 
        [SerializeField] public float SingleCardMovingDurationFromAnimationDeckToClearedLane = 0.3f;
        
        [Tooltip("Part 1. When animating cleared lane. Time between individual card dealings from the animation deck to the cleared lane.")]
        [SerializeField] public float PauseBetweenDealingCardsFromAnimationDeckToClearedLane = 0.2f;
        
        [Tooltip(" Part 2. How long does it take to move the last played card to the active card spot.")] 
        [SerializeField] public float LastPlayedCardToActiveDeckAnimationDuration = 0.4f;
        
        [Tooltip("Part 3. How long does it take to move the current active card back to the drawing deck.")] 
        [SerializeField] public float ActiveCardToDrawDeckAnimationDuration = 0.3f;

    }
}