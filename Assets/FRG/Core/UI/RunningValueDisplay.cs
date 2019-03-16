using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FRG.Core {
    public class RunningValueDisplay : MonoBehaviour {

        [Serializable]
        public struct ValueColorThreshold {
            public float threshold;
            public Color color;
        }

        [SerializeField] TMP_Text text_Value; // text to update
        [SerializeField] Text unityText_Value; // unity text to update
        [SerializeField] float textUpdateFequency = 0.5f;
        [SerializeField, Range(0, 10)] int decimalPlaces = 0;
        [SerializeField] string prefix = "";
        [SerializeField] string suffix = " mps";
        [SerializeField] ValueColorThreshold[] colorThresholds = null;
        [SerializeField] bool interpolateColors = true;
        [SerializeField] RunningValueCounter valueCounter = null;

        float freqCounter = 0;
        string formatString = null;
        int lastDecimalPlaces = -1;
        Dictionary<float, string> FloatToStringMemoization = new Dictionary<float, string>();

        void Update() {

            if(string.IsNullOrEmpty(formatString) || lastDecimalPlaces != decimalPlaces) {
                formatString = "N" + decimalPlaces;
                lastDecimalPlaces = decimalPlaces;
                FloatToStringMemoization.Clear();
            }

            freqCounter += Time.deltaTime;
            if(freqCounter >= textUpdateFequency) {
                freqCounter = 0f;

                float value = Round(valueCounter.Value, decimalPlaces);
                string formattedString;
                if(!FloatToStringMemoization.TryGetValue(value, out formattedString)) {
                    formattedString = prefix + value.ToString(formatString) + suffix;
                    FloatToStringMemoization.Add(value, formattedString);
                }

                if(text_Value != null) {
                    text_Value.text = formattedString;
                    text_Value.color = GetTextColor(value);
                }

                if(unityText_Value != null) {
                    unityText_Value.text = formattedString;
                    unityText_Value.color = GetTextColor(value);
                }
            }
        }

        Color GetTextColor(float value) {
            if(colorThresholds != null && colorThresholds.Length > 0f) {
                for(int i = colorThresholds.Length - 1;i >= 0;i--) {
                    if(value < colorThresholds[i].threshold) continue;

                    if(!interpolateColors || i == 0) return colorThresholds[i].color;

                    float spread = colorThresholds[i].threshold - colorThresholds[i - 1].threshold;
                    if(spread == 0f) return colorThresholds[i].color;

                    float factor = (value - colorThresholds[i - 1].threshold) / spread;

                    return Color.Lerp(colorThresholds[i - 1].color, colorThresholds[i].color, factor);
                }
            }
            return Color.white;
        }

        public static float Round(float value, int digits) {
            float mult = Mathf.Pow(10.0f, digits);
            return Mathf.Round(value * mult) / mult;
        }

    }
}
