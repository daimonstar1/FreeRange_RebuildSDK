using System;
using UnityEngine;

namespace FRG.Core
{
    public class GameViewResolution : ScriptableObject
    {
        [Serializable]
        public struct ResolutionEntry
        {
            public enum Layout { LandscapeAndPortrait, Landscape, Portrait, AspectRatio }

            public string name;
            public int width;
            public int height;
            public Layout layout;
        }

        [SerializeField]
        public ResolutionEntry[] resolutons;
    }
}