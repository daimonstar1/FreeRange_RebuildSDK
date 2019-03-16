using FRG.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour {
    private bool HasRenderer() { return GetComponent<Renderer>() != null; }
    [InspectorHide("HasRenderer")]
    [SerializeField] string colorProperty = "_Color";
    [SerializeField] internal bool debug = false;
    [SerializeField] float onEnableFadeInTime = -1f;
    [SerializeField] float onEnableFadeOutDelay = -1f;
    [SerializeField] float onEnableFadeOutTime = -1f;
    [SerializeField] bool startEnabled = true;
    [SerializeField] internal bool notManagedByParent = false;
    [SerializeField] bool skipMyRenderers = false;
    [SerializeField] bool controlInteractability = true;

    public bool Fading { get { return isActiveAndEnabled && fadeTimer > 0f; } }
    public bool FadingOut { get { return Fading && end.a == 0f; } }
    public bool FadingIn { get { return Fading && end.a != 0f; } }
    public bool FadedOut { get { return !Fading && current.a == 0f; } }
    public bool FadedIn { get { return !Fading && current.a != 0f; } }
    public bool Faded {
        get {
            return current.a == 0f;
        }
    }

    Renderer rend = null;
    Graphic graphic = null;
    CanvasGroup canvasGroup = null;
    ParticleSystem pSystem = null;

    bool interactable = false;
    bool blocksRaycasts = false;

    Color saved = Color.white;
    Color start = Color.white;
    Color end = Color.clear;
    Color current = Color.white;

    float fadeTimer = -1f;
    float duration = 1f;

    MaterialPropertyBlock block;

    List<Fader> childrenFaders = new List<Fader>();
    ParticleSystem.Particle[] m_Particles;

    bool initialized = false;
    bool enabledAfterFade = true;
    bool activateAfterFade = true;

    bool isEnabled = false;

    void Awake() {
        InitIfNeeded();
    }

    void OnEnable() {
        SetEnabled(startEnabled);

        if(onEnableFadeInTime > 0f) {
            FadeIn(onEnableFadeInTime);
        }
        else if(onEnableFadeOutTime > 0f) {
            if(onEnableFadeOutDelay > 0f) {
                FadeOutInSeconds(onEnableFadeOutTime, onEnableFadeOutDelay);
            }
            else {
                FadeOut(onEnableFadeOutTime);
            }
        }
    }

    void LateUpdate() {
        if(fadeTimer <= 0f) return;
        if(duration <= 0f) return;

        fadeTimer = Mathf.Max(0f, fadeTimer - Time.deltaTime);

        RefreshFade();
    }

    void RefreshFade() {
        if(duration <= 0f) return;

        float scalar = 1f - Mathf.Clamp01(fadeTimer / duration);
        Color color = Color.Lerp(start, end, scalar);

        ////Debug.Log(FRG.Core.Util.GetGameObjectPath(gameObject) + " LateUpdate currentAlpha("+currentAlpha+") newAlpha("+newAlpha+")" );

        //if(end > start ^ newAlpha < currentAlpha) {
            SetColor(color);
        //}

        if(fadeTimer <= 0f) {
            //Debug.Log( "fade complete " + name);
            SetEnabled(enabledAfterFade);
            gameObject.SetActive(activateAfterFade);
        }
    }

    void InitIfNeeded() {
        if(initialized) return;

        if(!skipMyRenderers) {
            pSystem = GetComponent<ParticleSystem>();
            //only use renderer if a particle system is not already managing it
            if(pSystem == null) rend = GetComponent<Renderer>();
            graphic = GetComponent<Graphic>();
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if(canvasGroup != null) {
            interactable = canvasGroup.interactable;
            blocksRaycasts = canvasGroup.blocksRaycasts;
            if(controlInteractability && canvasGroup.alpha <= 0f) {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        childrenFaders.Clear();
        for(int i=0;i<transform.childCount;i++) {
            AddChildrenRecursively(transform.GetChild(i));
        }

        block = new MaterialPropertyBlock();

        saved = GetInitialColor();
        current = saved;

        initialized = true;
    }

    //recursing so we can let faders manage themselves and their children without being superceded from above
    void AddChildrenRecursively(Transform t) {
        if(t == null) return;

        Fader fader = t.gameObject.GetComponent<Fader>();
        if(fader != null) {
            if(fader.notManagedByParent) return;
            childrenFaders.Add(fader);
        }

        //recurse to children if apt
        for(int i=0;i<t.childCount;i++) {
            AddChildrenRecursively(t.GetChild(i));
        }
    }

    void SetColor(Color color) {
        current = color;

        if(rend != null) {
            rend.GetPropertyBlock(block);
            block.SetColor(colorProperty, color);
            rend.SetPropertyBlock(block);
        }

        if(graphic != null) {
            graphic.color = color;
        }

        if(canvasGroup != null) {
            // crashed on set_alpha .. I'm hoping it is because of some crazy float value outside standard range so I clamp it here
            canvasGroup.alpha = Mathf.Clamp01(color.a);

            if(controlInteractability) {
                if(canvasGroup.alpha <= 0f) {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
                else {
                    canvasGroup.interactable = interactable;
                    canvasGroup.blocksRaycasts = blocksRaycasts;
                }
            }
        }

        if(pSystem != null) {
            //Debug.Log(FRG.Core.Util.GetGameObjectPath(gameObject) + " SetColor pSystem.startColor = " + color);
            var main = pSystem.main;
            main.startColor = color;

            if (m_Particles == null || m_Particles.Length < main.maxParticles) m_Particles = new ParticleSystem.Particle[main.maxParticles]; 

            int numParticlesAlive = pSystem.GetParticles(m_Particles);

            for(int i=0;i<m_Particles.Length;i++) {
                m_Particles[i].startColor = color;
            }

            pSystem.SetParticles(m_Particles, numParticlesAlive);
        }
           
    }

    Color GetInitialColor() {

        if(!skipMyRenderers) {
            if(rend != null)    return rend.material.GetColor(colorProperty);
            if(graphic != null) return graphic.color;
            if(pSystem != null) return pSystem.main.startColor.color;
        }

        if(canvasGroup != null) return new Color(1f, 1f, 1f, canvasGroup.alpha);

        return Color.white;
    }

    void SetEnabled(bool on) {

        if(rend != null) {
            ////Debug.Log(FRG.Core.Util.GetGameObjectPath(gameObject) + " rend.enabled = "+on );
            rend.enabled = on;
        }

        if(graphic != null)       graphic.enabled = on;

        //if(canvasGroup != null) canvasGroup.enabled = on;

        if(pSystem != null) {
            if(on && !pSystem.isPlaying) {
                //if(pSystem.loop) pSystem.Simulate(pSystem.duration);
                pSystem.Play();
            }
            else if(!on && pSystem.isPlaying) {
                pSystem.Stop();
            }
        }

        isEnabled = on;
    }

    public void Hide() { FadeOut(0f); }
    public void Show() { FadeIn(0f); }

    public void CrossFade(Color start, Color end, float dur, bool fadeChildren=true) {
        InitIfNeeded();

        enabledAfterFade = true;
        activateAfterFade = true;

        this.start = start;
        this.end = end;

        if(debug) Debug.Log(FRG.Core.Util.GetObjectPath(gameObject) + " CrossFade dur("+dur+") start("+start+") end("+end+")" );
        duration = dur;
        fadeTimer = duration;

        if(dur <= 0f) {
            if(debug) Debug.Log(FRG.Core.Util.GetObjectPath(gameObject) + " dur("+dur+") SetColor end("+end+")" );
            SetColor(end);
        }

        if(fadeChildren) {
            if(childrenFaders != null) {
                for(int i=0;i<childrenFaders.Count;i++) {
                    childrenFaders[i].CrossFade(start, end, dur, false);
                }
            }
        }

        SetEnabled(true);
    }

    public void FadeIn(Color color, float dur) {
        InitIfNeeded();

        saved = color;

        FadeIn(dur);
    }

    public void FadeInFromCurrent(float dur) {
        InitIfNeeded();

        if(current != saved) {
            CrossFade(current, saved, dur);
        }
        gameObject.SetActive(true);
    }

    public void FadeIn(float dur) {

        InitIfNeeded();

        Color faded = new Color(saved.r, saved.g, saved.b, 0f);

        CrossFade(faded, saved, dur);
        gameObject.SetActive(true);
    }

    public void FadeOut(Color color, float dur, bool activateAfter=true) {

        InitIfNeeded();

        saved = color;

        FadeOut(dur, activateAfter);
    }

    public void FadeOutFromCurrent(float dur, bool activateAfter=true) {

        InitIfNeeded();

        if(Faded) return;

        saved = current;

        FadeOut(dur, activateAfter);
    }

    public void FadeOut(float dur, bool activateAfter=true) {
        InitIfNeeded();

        if(!isEnabled) dur = 0f; //make it instant if not active

        Color faded = new Color(saved.r, saved.g, saved.b, 0f);
        if(debug) Debug.Log(FRG.Core.Util.GetObjectPath(gameObject) + " FadeOut dur("+dur+") saved("+saved+") faded("+faded+")" );

        CrossFade(saved, faded, dur);

        enabledAfterFade = false;
        activateAfterFade = activateAfter;
        if(dur == 0f) SetEnabled(activateAfter);
    }

    public void FadeOutInSeconds( float dur, float wait ) {
        StartCoroutine( FadeCoro( dur, wait ) );
    }
    IEnumerator FadeCoro( float dur, float wait ) {
        yield return new WaitForSeconds( wait );

        FadeOut( dur );
    }
    public void SaveColor(Color color) {
        InitIfNeeded();

        saved = color;
    }


}
