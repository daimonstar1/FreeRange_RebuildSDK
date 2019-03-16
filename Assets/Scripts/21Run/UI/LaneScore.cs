using System;
using FRG.Taco.Run21;
using UnityEngine;
using UnityEngine.UI;

namespace FRG.Taco
{
    [RequireComponent(typeof(Text))]
    public class LaneScore : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private LegacyAnimationClipPlayer _animationClipPlayer;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Run21Data _run21Data;
        private float animDuration;
        private AnimationClip scaleScoreAnimation;

        private void Start()
        {
            animDuration = Gameplay.instance.durations.scoreTextAnimationDuration;
            scaleScoreAnimation = new AnimationClip();
            scaleScoreAnimation.legacy = true;

            Keyframe[] keys = new Keyframe[3];
            keys[0] = new Keyframe(0.0f, 1f);
            keys[1] = new Keyframe(animDuration / 2, _run21Data.laneScoreScale);
            keys[2] = new Keyframe(2 * animDuration, 1f);

            scaleScoreAnimation.SetCurve("", typeof(RectTransform), "localScale.x", new AnimationCurve(keys));
            scaleScoreAnimation.SetCurve("", typeof(RectTransform), "localScale.y", new AnimationCurve(keys));
        }

        /// <summary>
        /// Set score value for <see cref="LaneScore"/>
        /// </summary>
        /// <param name="score"></param>
        public void SetScore(string incomingScore)
        {
            if (_text.text.Equals(incomingScore))
            {
                return;
            }

            _text.text = incomingScore;


            if (_text.text.Equals("0"))
            {
                return;
            }

            _animationClipPlayer.PlayClip(scaleScoreAnimation, animDuration, () => { ClearScore(); });
        }

        private void ClearScore()
        {
            _rectTransform.localScale = Vector2.one;
        }
    }
}