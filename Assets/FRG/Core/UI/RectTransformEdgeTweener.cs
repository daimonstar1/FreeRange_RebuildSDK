using FRG.Core;
using System.Collections;
using UnityEngine;

public class RectTransformEdgeTweener : MonoBehaviour {
    //dmd todo: add more subtle easing styles to RectTransformSettings' interpolation repertoire

    [SerializeField] float defaultTransitionTime = 0f;
    [SerializeField] public bool startOffEdge = false;
    [SerializeField] public bool ignoreX = false;
    [SerializeField] public bool ignoreY = false;
    [SerializeField] public bool ignoreScale = false;
    [SerializeField] public float offScaleFactor = 1f;
    [SerializeField] bool squareEasing = false;
    [SerializeField] public bool debug = false;
    [SerializeField] public Vector2 offPadding = new Vector2(100f, 100f);
    //[SerializeField] string soundOn = null;
    //[SerializeField] string soundOff = null;
        
    RectTransformSettings insideEdgeSettings = new RectTransformSettings();
    RectTransformSettings outsideEdgeSettings = new RectTransformSettings();

    float timer = -1f;
    float duration = 1f;
    RectTransformSettings start = new RectTransformSettings();
    RectTransformSettings end = new RectTransformSettings();

	RectTransform _rTrans;
	RectTransform rTrans {
		get {
			if(_rTrans == null) {
				_rTrans = transform as RectTransform;
            }
			return _rTrans;
		}
	}

    bool ready = false;

    Coroutine TweeningCoroutine = null;

    public bool IsOn { get; private set; }

	void Awake() {
        Ready();
    }

    void Ready() {
        if(ready) return;

        insideEdgeSettings = new RectTransformSettings(rTrans);
        outsideEdgeSettings = new RectTransformSettings(rTrans);

        float xOffset = 0f;
        if(rTrans.anchorMax.x == rTrans.anchorMin.x) {
            if(rTrans.anchorMax.x > 0.6f) {
                xOffset = offPadding.x;
            }
            else if(rTrans.anchorMax.x < 0.4f) {
                xOffset = -offPadding.x;
            }
        } 

        float yOffset = 0f;
        if(rTrans.anchorMax.y == rTrans.anchorMin.y) {
            if(rTrans.anchorMax.y > 0.6f) {
                yOffset = offPadding.y;
            }
            else if(rTrans.anchorMax.y < 0.4f) {
                yOffset = -offPadding.y;
            }
        } 

        outsideEdgeSettings.anchoredPosition = -insideEdgeSettings.anchoredPosition + new Vector2(xOffset, yOffset);
        outsideEdgeSettings.pivot = new Vector2(1f - insideEdgeSettings.pivot.x, 1f - insideEdgeSettings.pivot.y);
        if(!ignoreScale) outsideEdgeSettings.localScale = outsideEdgeSettings.localScale * offScaleFactor;

        if(debug) {
            Debug.Log("saved on.pivot("+insideEdgeSettings.pivot+") off.pivot("+outsideEdgeSettings.pivot+") on.anchoredPosition("+insideEdgeSettings.anchoredPosition+")->off.anchoredPosition("+outsideEdgeSettings.anchoredPosition+") xOffset("+xOffset+") yOffset("+yOffset+") startOffEdge("+startOffEdge+")");
        }

        ready = true;

        if(startOffEdge) {
            Off(0f, false);
        }
        else {
            IsOn = true;
        }

    }

    void OnDisable() {
        //snap to end of tween if disabled while tweening
        if(TweeningCoroutine != null) {
            StopCoroutine(TweeningCoroutine);
            TweeningCoroutine = null;
    	    RectTransformSettings.LoadSettings(rTrans, end, ignoreX, ignoreY, ignoreScale);
        }    
    }

    void TweenTo(RectTransformSettings settings, float time=0f) {
        timer = time;
        duration = time;
        //stamp = Time.timeSinceLevelLoad;

        if(TweeningCoroutine != null) {
            StopCoroutine(TweeningCoroutine);
        }

        start = new RectTransformSettings(rTrans);
        end = settings;
        if(debug) Debug.Log("TweenTo startAnchoredPos("+start.anchoredPosition+") endAnchoredPos("+end.anchoredPosition+") time("+time+")" + FRG.Core.Util.GetObjectPath(this));

        //snap if instant or currently not active in the hierarchy
        if(time > 0f && gameObject.activeInHierarchy) {
            TweeningCoroutine = StartCoroutine(TweenOverTime());
        }
        else {
	        RectTransformSettings.LoadSettings(rTrans, end, ignoreX, ignoreY, ignoreScale);
        }
	}

	public void On() {
        On(defaultTransitionTime);
	}

	public void On(float dur, bool playEffect=true) {
        if(debug) {
            Debug.Log("frame(" + Time.frameCount + ") On " + FRG.Core.Util.GetObjectPath(this));
        }
        Ready();
        IsOn = true;

        //if(dur > 0f && playEffect /*&& !BattleInput.MuteEffects*/ && !string.IsNullOrEmpty(soundOn)) {
        //    MasterAudio.PlaySound(soundOn);
        //}

        TweenTo(insideEdgeSettings, dur);
	}

	public void Off() {
        Off(defaultTransitionTime);
	}

	public void Off(float dur, bool playEffect=true) {
        if(debug) {
            Debug.Log("frame(" + Time.frameCount + ") Off " + FRG.Core.Util.GetObjectPath(this));
        }
        Ready();
        IsOn = false;

        //if(dur > 0f && playEffect /*&& !BattleInput.MuteEffects*/ && !string.IsNullOrEmpty(soundOff) ) { // && Time.time > 0.1fdon't cause error before MasterAudio loaded
        //    if ( MasterAudio.SafeInstance != null ) {
        //        MasterAudio.PlaySound(soundOff);
        //    }
        //}

        TweenTo(outsideEdgeSettings, dur);
	}

    IEnumerator TweenOverTime() {

        while(timer > 0f && duration > 0f) {
            timer -= Time.deltaTime;
            if(timer <= 0f) {
                break;
            }

            float t = 1f - Mathf.Clamp01(timer / duration);
            if(squareEasing) {
                if(IsOn) {
                    t = 1f - t;
                    t *= t;
                    t = 1f - t;
                }
                else {
                    t *= t;
                }
            }

            RectTransformSettings interpolatedSettings = RectTransformSettings.Lerp(start, end, t, ignoreX, ignoreY, ignoreScale);
            RectTransformSettings.LoadSettings(rTrans, interpolatedSettings, ignoreX, ignoreY, ignoreScale);
            if(debug) Debug.Log("lerped anchoredPosition("+rTrans.anchoredPosition+") timer("+timer+")" + FRG.Core.Util.GetObjectPath(this));

            yield  return new WaitForEndOfFrame();
        }

	    RectTransformSettings.LoadSettings(rTrans, end, ignoreX, ignoreY, ignoreScale);
        if(debug) Debug.Log("final anchoredPosition("+rTrans.anchoredPosition+") timer("+timer+")" + FRG.Core.Util.GetObjectPath(this));

        TweeningCoroutine = null;
    }

}