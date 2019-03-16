using UnityEngine;
using System;

[Serializable]
public class ScaleObject : MonoBehaviour {

    [SerializeField] Vector3 startScale = Vector3.one;
    [SerializeField] Vector3 endScale = Vector3.one;
    [SerializeField] float timeInSeconds = 1f;
    [SerializeField] bool oscillate = false;
    [SerializeField] bool squaredEasing = false;

    float timer = 0f;
    bool forward = true;

    void OnEnable() {
        transform.localScale = startScale;
        timer = timeInSeconds;
        forward = true;
    }

    void Update() {
        if(timeInSeconds == 0f) return;

        if(!oscillate) forward = true;

        timer -= Time.deltaTime;
        if(timer < 0f) {
            timer = timeInSeconds;
            if(oscillate) {
                forward = !forward;
            }
        }

        float factor = Mathf.Clamp01(timer / timeInSeconds);
        if(squaredEasing) factor *= factor;
        if(forward) factor = 1f - factor;
        transform.localScale = Vector3.Lerp(startScale, endScale, factor);
    }

}
