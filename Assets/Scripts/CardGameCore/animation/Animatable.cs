using System;
using FRG.Taco;
using UnityEngine;

namespace FRG.Taco
{
    public class Animatable : MonoBehaviour
    {
        [SerializeField] private LegacyAnimationClipPlayer _animationClipPlayer;
        [SerializeField] private AnimationClip _defaultBustAnimation;
        [SerializeField] private AnimationClip _defaultClearedAnimation;

        public AnimationClip DefaultBustAnimation
        {
            get { return _defaultBustAnimation; }
        }
        
        public AnimationClip DefaultClearedAnimation
        {
            get { return _defaultClearedAnimation; }
        }

        public void ScalePlay(float targetScale, float duration, Action callback)
        {
            AnimationClip clip = new AnimationClip();
            clip.legacy = true;

            clip.SetCurve("", typeof(RectTransform), "localScale.x", AnimationCurve.Linear(0, 1, duration, targetScale));
            clip.SetCurve("", typeof(RectTransform), "localScale.y", AnimationCurve.Linear(0, 1, duration, targetScale));

            _animationClipPlayer.PlayClip(clip, duration, callback);
        }

        public void TranslateTransformToWorldSpacePlay(Vector3 worldPositionTarget, float duration, Action callback)
        {
            AnimationClip clip = new AnimationClip();
            clip.legacy = true;

            var translateStart = transform.localPosition;
            var translateEnd = transform.parent.InverseTransformPoint(worldPositionTarget);
            clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, translateStart.x, duration, translateEnd.x));
            clip.SetCurve("", typeof(Transform), "localPosition.y", AnimationCurve.Linear(0, translateStart.y, duration, translateEnd.y));
            clip.SetCurve("", typeof(Transform), "localPosition.z", AnimationCurve.Linear(0, translateStart.z, duration, translateEnd.z));

            _animationClipPlayer.PlayClip(clip, duration, callback);
        }

        public void TranslateTransformToLocalSpacePlay(Vector3 localPositionTarget, float duration, Action callback = null)
        {
            AnimationClip clip = new AnimationClip();
            clip.legacy = true;

            var translateStart = transform.localPosition;
            var translateEnd = localPositionTarget;
            clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.Linear(0, translateStart.x, duration, translateEnd.x));
            clip.SetCurve("", typeof(Transform), "localPosition.y", AnimationCurve.Linear(0, translateStart.y, duration, translateEnd.y));
            clip.SetCurve("", typeof(Transform), "localPosition.z", AnimationCurve.Linear(0, translateStart.z, duration, translateEnd.z));

            _animationClipPlayer.PlayClip(clip, duration, callback);
        }

        public void TranslateRectTransformToLocalSpacePlay(Vector3 localPositionTarget, float duration, Action callback)
        {
            AnimationClip clip = new AnimationClip();
            clip.legacy = true;

            var translateStart = transform.localPosition;
            var translateEnd = localPositionTarget;
            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.x", AnimationCurve.Linear(0, translateStart.x, duration, translateEnd.x));
            clip.SetCurve("", typeof(RectTransform), "m_AnchoredPosition.y", AnimationCurve.Linear(0, translateStart.y, duration, translateEnd.y));

            _animationClipPlayer.PlayClip(clip, duration, callback);
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
    }
}