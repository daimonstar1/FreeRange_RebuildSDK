using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace FRG.Taco
{
    [RequireComponent(typeof(Animation))]
    public class LegacyAnimationClipPlayer : MonoBehaviour
    {
        [SerializeField] private Animation _animation;

        private AnimationClip _animationClip;

        private void Start()
        {
            if (_animation == null)
            {
                throw new ArgumentException($"Animation object not provided for : {gameObject.name}");
            }
        }

        /// <summary>
        /// Plays an <see cref="AnimationClip"/> for a duration of <paramref name="playDuration"/> seconds.
        /// <para />Provides an optional <see cref="Action"/>  <paramref name="playbackEndCallback"/> to execute logic
        /// when animation clip finished playing.
        /// </summary>
        /// <param name="pAnimationClip"> clip to play</param>
        /// <param name="playDuration"> play duration in seconds</param>
        /// <param name="playbackEndCallback"> post playback logic to execute (optional)</param>
        /// <exception cref="ArgumentException">When provided animation clip is not valid</exception>
        public void PlayClip(AnimationClip pAnimationClip, float playDuration, Action playbackEndCallback = null)
        {
            if (pAnimationClip == null)
            {
                throw new ArgumentException($"Clip to play not provided. Clip: {pAnimationClip}");
            }

            if (!_animation.enabled) 
            {
                _animation.enabled = true;
            }

            _animationClip = pAnimationClip;

//            AnimationEvent animEndEvent = new AnimationEvent();
//            animEndEvent.time = pAnimationClip.length;
//            animEndEvent.functionName = "AnimationCallback"; 
            
            if (_animation.GetClip(pAnimationClip.name) == null)
            {
                _animation.AddClip(pAnimationClip, pAnimationClip.name);
                _animation.clip = pAnimationClip;
            }
            

            // TODO should we set duration, when clip is created already with desired length? AnimationClip.length is read only.
            _animation.AddClip(_animationClip, _animationClip.name);
            _animation.Play(_animationClip.name);

            InvokePlaybackEndCallback(playbackEndCallback);
        }

        /// <summary>
        /// Checks if currently played <see cref="Playable"/> is playing.
        /// </summary>
        /// <returns>true when animation is being played</returns>
        public bool IsAnimationPlaying()
        {
            return _animation.isPlaying;
        }

        /// <summary>
        /// There is no built in event system in Timeline/Playbles API (TODO migrate when 2019.1 Unity)
        /// Fired once when animation clip started and used to execute post playback logic specified by
        /// <paramref name="playbackEndCallback"/> 
        /// </summary>
        /// <returns></returns>
        private IEnumerator PlaybackEndCoroutine(Action playbackEndCallback)
        {
            // TODO investigate AnimationEvents, they are supported by legacy Animation system, better than coroutine if applicable
            yield return new WaitUntil(() => IsAnimationPlaying() == false);

            _animation.enabled = false;

            DestroyIfPossible(_animationClip.name);

            if (playbackEndCallback != null)
            {
                playbackEndCallback.Invoke();
            }
        }

        private void InvokePlaybackEndCallback(Action playbackEndCallback)
        {
            StartCoroutine(PlaybackEndCoroutine(playbackEndCallback));
        }

        private void DestroyIfPossible(String pAnimationClipName)
        {
            if (pAnimationClipName != null)
            {
                _animation.RemoveClip(pAnimationClipName); 
            }
        }

        private void AnimationCallback()
        {
            print($"ANIMATION END CALLBACK CALLED AT: {Time.time}");
        }
    }
}