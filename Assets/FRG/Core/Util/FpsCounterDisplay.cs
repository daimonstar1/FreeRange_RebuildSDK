using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FRG.Core
{
    public class FpsCounterDisplay : MonoBehaviour
    {
        public enum ColorizeText
        {
            UseColor,
            DontUseColor
        }

        [SerializeField]
        Text text_fps; // text to update 2d
        [SerializeField]
        TextMesh textMesh_fps;  // text to update 3d
        [SerializeField]
        float textUpdateFequency = 0.5f;
        [SerializeField]
        string prefix = "";
        [SerializeField]
        string suffix = " fps";

        private float fpsFreqCounter = 0;
        private bool updateTextUI = false;
        private bool updateText3d = false;

        [SerializeField]
        public ColorizeText colorizeText = ColorizeText.DontUseColor;

        void Awake()
        {
            updateTextUI = text_fps != null;
            updateText3d = textMesh_fps != null;
        }

        void Update()
        {
            fpsFreqCounter += Time.deltaTime;
            if (fpsFreqCounter >= textUpdateFequency)
            {
                fpsFreqCounter = 0f;

                float medianFps = FpsCounter.instance.MedianFps;
                int fps = (int)medianFps;
                string formattedString;
                if (!Statics.IntToStringMemorization.TryGetValue(fps, out formattedString))
                {
                    formattedString = prefix + fps.ToString() + suffix;
                    Statics.IntToStringMemorization.Add(fps, formattedString);
                }
                if (updateTextUI)
                {
                    text_fps.text = formattedString;
                    if (colorizeText == ColorizeText.UseColor)
                    {
                        text_fps.color = GetTextColor(medianFps);
                    }
                }
                if (updateText3d)
                {
                    textMesh_fps.text = formattedString;
                    if (colorizeText == ColorizeText.UseColor)
                    {
                        textMesh_fps.color = GetTextColor(medianFps);
                    }
                }
            }
        }

        private Color GetTextColor(float fps)
        {
            if (fps <= 10.0f) return Color.red;
            if (fps <= 30.0f) return Color.yellow;
            return Color.green;
        }

        private class Statics
        {
            public static readonly Dictionary<int, string> IntToStringMemorization = new Dictionary<int, string>();
        }
    }
}
