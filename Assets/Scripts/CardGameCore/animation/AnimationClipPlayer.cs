using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace FRG.Taco
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Animatable))]
    public class AnimationClipPlayer : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private AnimationClip CardFaceUpAnimation;
        private AnimationClip CardFaceDownAnimation;
        private PlayableGraph playableGraph;
        private AnimationClipPlayable currentPlayable;

        private void Start()
        {
            if (_animator == null)
            {
                throw new ArgumentException($"Animator not provided for : {gameObject.name}");
            }
        }

        /// <summary>
        /// Plays an <see cref="AnimationClip"/> for a duration of <paramref name="playDuration"/> seconds.
        /// <para />Provides an optional <see cref="Action"/>  <paramref name="playbackEndCallback"/> to execute logic
        /// when animation clip finished playing.
        /// </summary>
        /// <param name="animationClip"> clip to play</param>
        /// <param name="playDuration"> play duration in seconds</param>
        /// <param name="playbackEndCallback"> post playback logic to execute (optional)</param>
        /// <exception cref="ArgumentException">When provided animation clip is not valid</exception>
        public void PlayClip(AnimationClip animationClip, float playDuration, Action playbackEndCallback = null)
        {
            if (!_animator.enabled)
            {
                _animator.enabled = true;
            }

            currentPlayable = AnimationPlayableUtilities.PlayClip(_animator, animationClip, out playableGraph);
            currentPlayable.SetDuration(playDuration);

            InvokePlaybackEndCallback(playbackEndCallback);
        }

        /// <summary>
        /// Checks if currently played <see cref="Playable"/> is playing.
        /// </summary>
        /// <returns>true when animation is being played</returns>
        public bool IsAnimationPlaying()
        {
            if (currentPlayable.IsValid())
            {
                return !currentPlayable.IsDone();
            }

            return false;
        }

        /// <summary>
        /// There is no built in event system in Timeline/Playbles API (TODO migrate when 2019.1 Unity)
        /// Fired once when animation clip started and used to execute post playback logic specified by
        /// <paramref name="playbackEndCallback"/> 
        /// </summary>
        /// <returns></returns>
        private IEnumerator PlaybackEndCoroutine(Action playbackEndCallback)
        {
            yield return new WaitUntil(() => IsAnimationPlaying() == false);

            _animator.enabled = false;

            DestroyIfPossible(playableGraph, currentPlayable);

            if (playbackEndCallback != null)
            {
                playbackEndCallback.Invoke();
            }
        }

        private void InvokePlaybackEndCallback(Action playbackEndCallback)
        {
            StartCoroutine(PlaybackEndCoroutine(playbackEndCallback));
        }

        private void DestroyIfPossible(PlayableGraph graph, Playable playable)
        {
            if (playable.IsValid())
            {
                if (playable.CanDestroy())
                {
                    playable.Destroy();
                }
            }

            if (graph.IsValid())
            {
                graph.Stop();
                graph.Destroy();
            }
        }
    }
}