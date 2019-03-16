using System.Collections;
using FRG.Core;
using FRG.Taco.Run21;
using UnityEngine;
using UnityEngine.UI;

namespace FRG.Taco
{
    public class LaneOutline : PoolObject
    {
        [SerializeField] Image laneOutlineImage;
        [SerializeField] private Run21Data _run21Data;
        
        public void DisplayBustOutline()
        {
            StartCoroutine(FadeImage(true, _run21Data.bustLaneOutline));
        }

        public void DisplayWildcardOutline()
        {
            StartCoroutine(FadeImage(true, _run21Data.wildcardClearOutline));
        }

        IEnumerator FadeImage(bool isOpaqueColor, Color outlineColor, float fadeInOutDuration = 1f)
        {
            
            Color color = new Color(outlineColor.r, outlineColor.g, outlineColor.b, outlineColor.a);

            if (isOpaqueColor)
            {
                for (float i = _run21Data.outlinePulseDuration; i >= 0; i -= Time.deltaTime) // fade from opaque to transparent

                {
                    color.a = i;
                    laneOutlineImage.color = color;
                    yield return null;
                }

                if (gameObject.activeSelf)
                {
                    StartCoroutine(FadeImage(false, outlineColor, _run21Data.outlinePulseDuration));
                }
            }
            else
            {
                for (float i = 0; i <= _run21Data.outlinePulseDuration; i += Time.deltaTime) // fade from transparent to opaque

                {
                    color.a = i;
                    laneOutlineImage.color = color;
                    yield return null;
                }

                if (gameObject.activeSelf)
                {
                    StartCoroutine(FadeImage(true, outlineColor, _run21Data.outlinePulseDuration));
                }
            }
        }
    }
}