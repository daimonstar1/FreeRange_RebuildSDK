using UnityEngine;
using System.Collections;

namespace FRG.Core {

    /// <summary>
    /// wraps various kinds of transition in and out tweens/animations
    /// </summary>
    public class PoolTransition : PoolBehaviour {

        [SerializeField] Fader fader = null;
        [SerializeField] CanvasGroup interactivityGroup = null;
        [SerializeField] RectTransformEdgeTweener[] edgeTweeners = null;
        [SerializeField] Animator[] animators = null;
        [SerializeField] string onAnimBoolName = "On";
        [SerializeField] float transitionInDuration = 0.1f;
        [SerializeField] float transitionOutDuration = 0.1f;
        [SerializeField] string soundOn = null;
        [SerializeField] string soundOff = null;
        //add wipe options?
        [SerializeField] bool noOffTransition = false;

        bool on = false;
        bool pristine = true;

        Coroutine InteractivityCoroutine = null;
        Coroutine ClearTransitioningCoroutine = null;

        public bool IsOn { get { return on; } }

        public float TransitionInTime { get { return transitionInDuration; } }
        public float TransitionOutTime { get { return transitionOutDuration; } }

        public bool TransitioningIn { get;  private set; }
        public bool TransitioningOut { get;  private set; }

        public override void OnSpawn() {
            base.OnSpawn();

            On();
        }

        public override void OnDespawn() {

            if(InteractivityCoroutine != null) {
                StopCoroutine(InteractivityCoroutine);
                InteractivityCoroutine = null;
            }

            if(ClearTransitioningCoroutine != null) {
                StopCoroutine(ClearTransitioningCoroutine);
                ClearTransitioningCoroutine = null;
            }

            TransitioningIn = false;
            TransitioningOut = false;

            base.OnDespawn();
        }

        public override void DespawnAfterDelay(float delay) {

            if(InteractivityCoroutine != null) {
                StopCoroutine(InteractivityCoroutine);
                InteractivityCoroutine = null;
            }

            float transitionTime = Mathf.Min(transitionOutDuration, delay);
            Off(transitionTime);

            base.DespawnAfterDelay(delay);
        }

        public void On(float dur=-1f) {
            if(!pristine && on) return;

            //Debug.Log("frame("+Time.frameCount+") ScreenActivator On " + Util.GetObjectPath(this) );

            if(dur < 0f) dur = transitionInDuration;

            if(fader != null) {
                fader.FadeIn(dur);
            }

            if(interactivityGroup != null) {

                if(dur <= 0) {
                    //enable right away if instant On
                    interactivityGroup.interactable = true;
                    interactivityGroup.blocksRaycasts = true;
                }
                else {
                    interactivityGroup.interactable = false;
                    interactivityGroup.blocksRaycasts = false;
                    InteractivityCoroutine = StartCoroutine(EnableInteractivityAfterTransitionIn());
                }
            }

            if(edgeTweeners != null) {
                for(int i=0;i<edgeTweeners.Length;i++) {
                    if(edgeTweeners[i] == null) continue;
                    edgeTweeners[i].On(dur);
                }
            }

            if(animators != null) {
                for(int i=0;i<animators.Length;i++) {
                    if(animators[i] == null) continue;
                    animators[i].SetBool(onAnimBoolName, true);
                }
            }

            on = true;
            pristine = false;

            if(dur > 0f && !string.IsNullOrEmpty(soundOn)) {
                //Debug.Log("frame("+Time.frameCount+") ScreenActivator("+gameObject.name+") soundOn MasterAudio.PlaySound("+soundOn+");");
                //MasterAudio.PlaySound(soundOn);
            }

            if(ClearTransitioningCoroutine != null) {
                StopCoroutine(ClearTransitioningCoroutine);
                ClearTransitioningCoroutine = null;
            }

            TransitioningIn = false;
            TransitioningOut = false;

            if(dur > 0f) {
                TransitioningIn = true;
                ClearTransitioningCoroutine = StartCoroutine(ClearTransitioningBoolsAfterDelay(dur));
            }
        }

        public void Off(float dur=-1f) {
            if(!pristine && !on) return;

            if(noOffTransition) return;

            //Debug.Log("frame("+Time.frameCount+") ScreenActivator Off", this );

            if (dur < 0f) dur = transitionOutDuration;

            if(fader != null) {
                //Debug.Log("frame("+Time.frameCount+") ScreenActivator Off fader fader.FadeOut("+dur+");", this );
                fader.FadeOut(dur);
            }

            if(interactivityGroup != null) {
                //Debug.Log("frame("+Time.frameCount+") ScreenActivator Off interactivityGroup.interactable = false;", this );
                interactivityGroup.interactable = false;
                interactivityGroup.blocksRaycasts = false;
            }

            if(edgeTweeners != null) {
                //Debug.Log("frame("+Time.frameCount+") ScreenActivator Off edgeTweeners", this );
                for(int i=0;i<edgeTweeners.Length;i++) {
                    if(edgeTweeners[i] == null) continue;
                    edgeTweeners[i].Off(dur);
                }
            }

            if(animators != null) {
                //Debug.Log("frame("+Time.frameCount+") ScreenActivator Off animators", this );
                for(int i=0;i<animators.Length;i++) {
                    if(animators[i] == null) continue;
                    animators[i].SetBool(onAnimBoolName, false);
                }
            }

            on = false;
            pristine = false;

            if(dur > 0f && !string.IsNullOrEmpty(soundOff)) {
                //Debug.Log("frame("+Time.frameCount+") ScreenActivator Off soundOff MasterAudio.PlaySound("+soundOff+");", this);
                //MasterAudio.PlaySound(soundOff);
            }

            if(ClearTransitioningCoroutine != null) {
                StopCoroutine(ClearTransitioningCoroutine);
                ClearTransitioningCoroutine = null;
            }

            TransitioningIn = false;
            TransitioningOut = false;

            if(dur > 0f) {
                TransitioningOut = true;
                ClearTransitioningCoroutine = StartCoroutine(ClearTransitioningBoolsAfterDelay(dur));
            }
        }

        IEnumerator EnableInteractivityAfterTransitionIn() {
            yield  return new WaitForSeconds(transitionInDuration);

            if(interactivityGroup != null) {
                //enable right away if instant On
                interactivityGroup.interactable = true;
                interactivityGroup.blocksRaycasts = true;
            }
        }

        IEnumerator ClearTransitioningBoolsAfterDelay(float delay) {
            yield  return new WaitForSeconds(delay);

            TransitioningIn = false;
            TransitioningOut = false;
        }

    }

}