using UnityEngine;
using FRG.SharedCore;
using System;

namespace FRG.Core
{
    [ServiceOptions(IsEditorOnly = true)]
    public class AudioImportSettings : ScriptableObject
    {
        [Serializable]
        public class CilpSettings
        {
            public string myNote = "";

            // clips to apply settings on
            [InspectorComment(CommentText = "Will aplly to clips with length in this time span")]
            public float minLengthInSeconds;
            public float maxLengthInSeconds;

            public bool loadInBackground = false;
            public bool preloadAudioData = true;

            public PlatformSettings Default = new PlatformSettings();
            public PlatformSettings Standalone = new PlatformSettings();
            public PlatformSettings iOS = new PlatformSettings();
            public PlatformSettings Android = new PlatformSettings();
        }

        [Serializable]
        public class PlatformSettings
        {
            // settings to apply
            [SerializeField] public bool replaceOriginalSetting = false;
            [InspectorHide("_IsOverride")]
            [SerializeField] public int maxFrequency = 44100;
            [InspectorHide("_IsOverride")]
            [SerializeField] public AudioClipLoadType loadType = AudioClipLoadType.CompressedInMemory;
            [InspectorHide("_IsOverride")]
            [SerializeField] public AudioCompressionFormat compressionFormat = AudioCompressionFormat.Vorbis;
            [InspectorHide("_IsOverride")]
            [InspectorHide("_IsCompressed")]
            [SerializeField] public float compressionQuality = 0.7f;

            private bool _IsOverride() { return replaceOriginalSetting; }
            private bool _IsCompressed() { return compressionFormat == AudioCompressionFormat.Vorbis; }
        }

        public static AudioImportSettings instance { get { return ServiceLocator.ResolveEditorAsset<AudioImportSettings>(StandardEditorPaths.CoreDataEditor, typeof(AudioImportSettings).Name); } }

        [InspectorComment(CommentText = "After adjusting, please reimport the whole Sound directory to be sure every AudioClip is processed.")]
        public CilpSettings[] clipSettings;
    }
}