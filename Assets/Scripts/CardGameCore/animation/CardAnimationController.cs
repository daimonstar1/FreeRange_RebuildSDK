using System;
using System.Collections;
using UnityEngine;

namespace FRG.Taco
{
    /// <summary>
    /// Each <see cref="DisplayCard"/> has an <see cref="CardAnimationController"/> attached used to access the <b>legacy</b> <see cref="Animation"/> API.
    ///
    /// <para /> List of supported animations:
    /// <ul>
    /// <li> Card face up animation - <see cref="PlayAnimationFaceUp"/>  </li>
    /// <li> Card face down animation - <see cref="PlayAnimationFaceDown"/>  </li>
    /// <li> Moving cards between decks - <see cref="MoveTowardsAnimated(System.Nullable{UnityEngine.Vector3},System.Nullable{UnityEngine.Vector3},System.Nullable{UnityEngine.Quaternion},System.Nullable{UnityEngine.Quaternion},float,System.Action)"/>  </li>
    /// </ul>
    /// 
    /// <para /> The Playables API allows you to easily play a single animation without the overhead involved in creating and managing an AnimatorController asset.
    /// </summary>
    ///
    [RequireComponent(typeof(LegacyAnimationClipPlayer))]
    [RequireComponent(typeof(Animatable))]
    public class CardAnimationController : MonoBehaviour
    {
        [SerializeField] private Animatable _animatable;
        [SerializeField] private LegacyAnimationClipPlayer _animationClipPlayer;

        private AnimationClip CardFaceUpAnimation;
        private AnimationClip CardFaceDownAnimation;

        private float FlipAnimationDuration = 1;

        void Awake()
        {
            _animationClipPlayer = GetComponent<LegacyAnimationClipPlayer>();
            CardFaceUpAnimation = _animatable.CreateAnimationClip(null, null, Quaternion.Euler(0, 180, 0), Quaternion.Euler(0, 0, 0), FlipAnimationDuration);
            CardFaceDownAnimation = _animatable.CreateAnimationClip(null, null, Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, -180, 0), FlipAnimationDuration);
        }

        /// <summary>
        /// Plays the <see cref="CardFaceUpAnimation"/> animation. 
        /// </summary>
        public void PlayAnimationFaceUp(float? animDuration = null)
        {
            _animationClipPlayer.PlayClip(CardFaceUpAnimation, animDuration ?? FlipAnimationDuration);
        }

        /// <summary>
        /// Plays the <see cref="CardFaceDownAnimation"/> animation.
        /// </summary>
        public void PlayAnimationFaceDown(float? animDuration = null)
        {
            _animationClipPlayer.PlayClip(CardFaceDownAnimation, animDuration ?? FlipAnimationDuration);
        }

        /// <summary>
        /// Plays a dynamically generated animation clip based on input paramaters.
        /// Atleast one translation or rotation target must be specifed.
        /// </summary>
        /// <param name="translationEnd">end of translation (optional)</param>
        /// <param name="rotationEnd">end of rotation (optional)</param>
        /// <param name="playbackEndCallback"></param>
        /// <exception cref="ArgumentException">Thrown when none target provided</exception>
        public void MoveTowardsAnimated(Vector3? translationEnd, Quaternion? rotationEnd, float durationSeconds,
            Action playbackEndCallback = null)
        {
            _animationClipPlayer.PlayClip(_animatable.CreateAnimationClip(null, translationEnd, null, rotationEnd, durationSeconds), durationSeconds,
                playbackEndCallback);
        }

        /// <summary>
        /// Plays a dynamically generated animation clip based on input paramaters.
        /// <ul>
        /// <li> Card will be translated from <paramref name="translationStart"/>  to <paramref name="translationEnd" />    </li>
        /// <li> Card will be rotated from <paramref name="rotationStart"/>  to <paramref name="rotationEnd"/> </li>
        /// </ul>
        ///  
        /// </summary>
        /// <param name="translationStart">start of translation (optional)</param>
        /// <param name="translationEnd">end of translation (optional)</param>
        /// <param name="rotationStart">start of rotation (optional)</param>
        /// <param name="rotationEnd">end of rotation (optional)</param>
        /// <param name="durationSeconds"> play duration in seconds</param>
        /// <param name="playbackEndCallback">logic to execute when clip finished</param>
        /// <exception cref="ArgumentException">Thrown when both <paramref name="translationEnd"/>  and <paramref name="rotationEnd"/> omitted</exception>
        public void MoveTowardsAnimated(Vector3? translationStart, Vector3? translationEnd, Quaternion? rotationStart,
            Quaternion? rotationEnd, float durationSeconds, Action playbackEndCallback = null)
        {
            _animationClipPlayer.PlayClip(
                _animatable.CreateAnimationClip(translationStart, translationEnd, rotationStart, rotationEnd, durationSeconds),
                durationSeconds,
                playbackEndCallback);
        }

        /// <summary>
        /// Check if card animation is currently playing.
        /// </summary>
        /// <returns></returns>
        public bool IsAnimationPlaying()
        {
            return _animationClipPlayer.IsAnimationPlaying();
        }

        /// Generates an <see cref="AnimationClip"/> instance based on input paramters.
        /// </summary>
        /// <param name="pTranslationStart">start of translation (optional)</param> // TODO world space commentary
        /// <param name="pTranslationEnd">end of translation (optional)</param>
        /// <param name="pRotationStart">start of rotation (optional)</param>
        /// <param name="pRotationEnd">end of rotation (optional)</param>
        /// <param name="duration"> play duration in seconds</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when both <paramref name="translationEnd"/>  and <paramref name="rotationEnd"/> omitted</exception>
        public AnimationClip CreateAnimationClip(Vector3? pTranslationStart, Vector3? pTranslationEnd,
            Quaternion? pRotationStart, Quaternion? pRotationEnd, float duration)
        {
            AnimationClip clip = new AnimationClip();
            clip.legacy = true;

            if (!pTranslationEnd.HasValue && !pRotationEnd.HasValue)
            {
                throw new ArgumentException("At least one target must be set!");
            }

            if (pTranslationEnd.HasValue)
            {
                var translateStart = pTranslationStart ?? transform.localPosition;
                var translateEnd = transform.parent != null ? transform.parent.InverseTransformPoint(pTranslationEnd.Value) : pTranslationEnd.Value;
                clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, translateStart.x, duration, translateEnd.x));
                clip.SetCurve("", typeof(Transform), "localPosition.y", AnimationCurve.Linear(0, translateStart.y, duration, translateEnd.y));
                clip.SetCurve("", typeof(Transform), "localPosition.z", AnimationCurve.Linear(0, translateStart.z, duration, translateEnd.z));
            }

            if (pRotationEnd.HasValue)
            {
                var rotationStart = pRotationStart.HasValue ? pRotationStart.Value : transform.localRotation; // TODO parent rotation check
                clip.SetCurve("", typeof(Transform), "localRotation.x", AnimationCurve.Linear(0, rotationStart.x, duration, pRotationEnd.Value.x));
                clip.SetCurve("", typeof(Transform), "localRotation.y", AnimationCurve.Linear(0, rotationStart.y, duration, pRotationEnd.Value.y));
                clip.SetCurve("", typeof(Transform), "localRotation.z", AnimationCurve.Linear(0, rotationStart.z, duration, pRotationEnd.Value.z));
                clip.SetCurve("", typeof(Transform), "localRotation.w", AnimationCurve.Linear(0, rotationStart.w, duration, pRotationEnd.Value.w));
                clip.EnsureQuaternionContinuity();
            }

            return clip;
        }

        public void PlayBust(float delaySeconds, float duration, Action callback = null)
        {
            if (delaySeconds > 0)
            {
                StartCoroutine(ExecuteActionDelayed(() => { _animationClipPlayer.PlayClip(_animatable.DefaultBustAnimation, duration, callback); }, delaySeconds));
                return;
            }

            _animationClipPlayer.PlayClip(_animatable.DefaultBustAnimation, duration, callback);
        }

        public void PlayCleared(float delaySeconds, float duration, Action callback = null)
        {
            if (delaySeconds > 0)
            {
                StartCoroutine(ExecuteActionDelayed(() => { _animationClipPlayer.PlayClip(_animatable.DefaultClearedAnimation, duration, callback); }, delaySeconds));
                return;
            }

            _animationClipPlayer.PlayClip(_animatable.DefaultBustAnimation, duration, callback);
        }

        private IEnumerator ExecuteActionDelayed(Action action, float delaySeconds)
        {
            if (delaySeconds > 0)
            {
                yield return new WaitForSeconds(delaySeconds);
            }

            action.Invoke();
        }
    }
}