using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace FRG.Core {
    public class ProjectTextureSettings {
        public static TextureChangeOptions[] GetDefaultOptions() {
            return new TextureChangeOptions[]
            {
                // Reflection probes
                new TextureChangeOptions
                {
                    shouldCheckTextureShape = true,
                    matchTextureShape = TextureImporterShape.TextureCube,

                    shouldCheckTextureType = false,

                    shouldCheckTransparency = true,
                    matchHasTransparency = false,

                    shouldCheckSpriteAtlasTag = false,

                    shouldCheckName = true,
                    matchNameRegex = @".*ReflectionProbe.*",

                    applySpriteTag = false,
                    applyMipmapSetting = false,

                    applyCompression = true,
                    textureCompression = TextureImporterCompression.Compressed,
                    compressionQuality = 50,
                    crunchedCompression = true,

                    overrideOptions = new TextureOverrideOptions[]
                    {
                        TextureOverrideOptions.NoOverride(BuildTargetGroup.Standalone),
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.iOS,
                            alwaysOverride = true,
                            allowSmallerMaxTextureSize = true,
                            textureFormat = TextureImporterFormat.ETC_RGB4Crunched,
                            compressionQuality = 50,
                        },
                        TextureOverrideOptions.NoOverride(BuildTargetGroup.Android),
                    },
                },
                // Other cube maps, no alpha
                new TextureChangeOptions
                {
                    shouldCheckTextureShape = true,
                    matchTextureShape = TextureImporterShape.TextureCube,

                    shouldCheckTextureType = false,

                    shouldCheckTransparency = true,
                    matchHasTransparency = false,

                    shouldCheckSpriteAtlasTag = false,
                    shouldCheckName = false,

                    applySpriteTag = false,
                    applyMipmapSetting = false,

                    applyCompression = true,
                    textureCompression = TextureImporterCompression.Compressed,
                    compressionQuality = 50,
                    crunchedCompression = true,

                    overrideOptions = new TextureOverrideOptions[]
                    {
                        TextureOverrideOptions.NoOverride(BuildTargetGroup.Standalone),
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.iOS,
                            alwaysOverride = true,
                            allowSmallerMaxTextureSize = true,
                            textureFormat = TextureImporterFormat.ETC_RGB4Crunched,
                            compressionQuality = 50,
                        },
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.Android,
                            alwaysOverride = true,
                            allowSmallerMaxTextureSize = false,
                            textureFormat = TextureImporterFormat.RGBA32,
                            compressionQuality = 100,
                        }
                    },
                },
                // OnGUI (editor) textures
                new TextureChangeOptions
                {
                    shouldCheckTextureShape = false,
                    shouldCheckTextureType = true,
                    matchTextureType = TextureImporterType.GUI,
                    shouldCheckTransparency = false,
                    shouldCheckSpriteAtlasTag = false,
                    shouldCheckName = false,

                    applySpriteTag = true,
                    spriteTag = "",

                    applyMipmapSetting = false,

                    applyCompression = true,
                    textureCompression = TextureImporterCompression.Uncompressed,
                    compressionQuality = 50,
                    crunchedCompression = false,

                    overrideOptions = new TextureOverrideOptions[]
                    {
                        TextureOverrideOptions.NoOverride(BuildTargetGroup.Standalone),
                        TextureOverrideOptions.NoOverride(BuildTargetGroup.iOS),
                        TextureOverrideOptions.NoOverride(BuildTargetGroup.Android),
                    },
                },
                // Atlased sprites
                new TextureChangeOptions
                {
                    shouldCheckTextureShape = false,
                    shouldCheckTextureType = true,
                    matchTextureType = TextureImporterType.Sprite,
                    shouldCheckTransparency = false,
                    shouldCheckSpriteAtlasTag = true,
                    matchSpriteAtlasTagRegex = "(Menu|Gameplay|Menu_PointFilter)",
                    shouldCheckName = false,

                    applySpriteTag = false,

                    applyMipmapSetting = true,
                    mipmapEnabled = true,

                    applyCompression = true,
                    textureCompression = TextureImporterCompression.Compressed,
                    compressionQuality = 50,
                    crunchedCompression = false,

                    overrideOptions = new TextureOverrideOptions[]
                    {
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.Standalone,
                            allowSmallerMaxTextureSize = false,
                            alwaysOverride = true,
                            textureFormat = TextureImporterFormat.DXT5,
                            compressionQuality = 50,
                        },
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.iOS,
                            allowSmallerMaxTextureSize = false,
                            alwaysOverride = true,
                            textureFormat = TextureImporterFormat.ETC2_RGBA8,
                            compressionQuality = 50,
                        },
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.Android,
                            allowSmallerMaxTextureSize = false,
                            alwaysOverride = true,
                            textureFormat = TextureImporterFormat.ETC2_RGBA8,
                            compressionQuality = 50,
                        },
                    },
                },
                // Atlased sprites
                new TextureChangeOptions
                {
                    shouldCheckTextureShape = false,
                    shouldCheckTextureType = true,
                    matchTextureType = TextureImporterType.Sprite,
                    shouldCheckTransparency = false,
                    shouldCheckSpriteAtlasTag = true,
                    matchSpriteAtlasTagRegex = "NoMip",
                    shouldCheckName = false,

                    applySpriteTag = false,

                    applyMipmapSetting = true,
                    mipmapEnabled = false,

                    applyCompression = true,
                    textureCompression = TextureImporterCompression.Compressed,
                    compressionQuality = 50,
                    crunchedCompression = true,

                    overrideOptions = new TextureOverrideOptions[]
                    {
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.Standalone,
                            allowSmallerMaxTextureSize = false,
                            alwaysOverride = true,
                            textureFormat = TextureImporterFormat.DXT5Crunched,
                            compressionQuality = 50,
                        },
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.iOS,
                            allowSmallerMaxTextureSize = false,
                            alwaysOverride = true,
                            textureFormat = TextureImporterFormat.ETC2_RGBA8Crunched,
                            compressionQuality = 50,
                        },
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.Android,
                            allowSmallerMaxTextureSize = false,
                            alwaysOverride = true,
                            textureFormat = TextureImporterFormat.ETC2_RGBA8Crunched,
                            compressionQuality = 50,
                        },
                    },
                },
                // Atlased sprites
                new TextureChangeOptions
                {
                    shouldCheckTextureShape = false,
                    shouldCheckTextureType = true,
                    matchTextureType = TextureImporterType.Sprite,
                    shouldCheckTransparency = false,
                    shouldCheckSpriteAtlasTag = true,
                    matchSpriteAtlasTagRegex = "NoMipOpaque",
                    shouldCheckName = false,

                    applySpriteTag = false,

                    applyMipmapSetting = true,
                    mipmapEnabled = false,

                    applyCompression = true,
                    textureCompression = TextureImporterCompression.Compressed,
                    compressionQuality = 50,
                    crunchedCompression = true,

                    overrideOptions = new TextureOverrideOptions[]
                    {
                        TextureOverrideOptions.NoOverride(BuildTargetGroup.Standalone),
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.iOS,
                            alwaysOverride = true,
                            allowSmallerMaxTextureSize = true,
                            textureFormat = TextureImporterFormat.ETC_RGB4Crunched,
                            compressionQuality = 50,
                        },
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.Android,
                            alwaysOverride = false,
                            allowSmallerMaxTextureSize = true,
                            textureFormat = TextureImporterFormat.ETC_RGB4Crunched,
                            compressionQuality = 50,
                        }
                    },
                },
                // Other textures with alpha
                new TextureChangeOptions
                {
                    shouldCheckTextureShape = false,
                    shouldCheckTextureType = false,

                    shouldCheckTransparency = true,
                    matchHasTransparency = true,
                    shouldMatchNPOTButDivisBy4 = false,

                    shouldCheckSpriteAtlasTag = false,
                    shouldCheckName = false,

                    applySpriteTag = false,
                    applyMipmapSetting = false,

                    applyCompression = true,
                    textureCompression = TextureImporterCompression.Compressed,
                    compressionQuality = 50,
                    crunchedCompression = true,

                    overrideOptions = new TextureOverrideOptions[]
                    {
                        TextureOverrideOptions.NoOverride(BuildTargetGroup.Standalone),
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.iOS,
                            allowSmallerMaxTextureSize = true,
                            alwaysOverride = true,
                            textureFormat = TextureImporterFormat.ETC2_RGBA8Crunched,
                            compressionQuality = 50,
                        },
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.Android,
                            alwaysOverride = false,
                            allowSmallerMaxTextureSize = true,
                            textureFormat = TextureImporterFormat.ETC2_RGBA8Crunched,
                            compressionQuality = 50,
                        }
                    },
                },
                // Other textures with alpha
                new TextureChangeOptions
                {
                    shouldCheckTextureShape = false,
                    shouldCheckTextureType = false,

                    shouldCheckTransparency = true,
                    matchHasTransparency = false,
                    shouldMatchNPOTButDivisBy4 = true,

                    shouldCheckMipmap = true,
                    matchHasMipmap = false,

                    shouldCheckSpriteAtlasTag = false,
                    shouldCheckName = false,

                    applySpriteTag = false,
                    applyMipmapSetting = false,

                    applyCompression = true,
                    textureCompression = TextureImporterCompression.Compressed,
                    compressionQuality = 50,
                    crunchedCompression = true,

                    overrideOptions = new TextureOverrideOptions[]
                    {
                        TextureOverrideOptions.NoOverride(BuildTargetGroup.Standalone),
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.iOS,
                            allowSmallerMaxTextureSize = true,
                            alwaysOverride = true,
                            textureFormat = TextureImporterFormat.ETC2_RGBA8Crunched,
                            compressionQuality = 50,
                        },
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.Android,
                            alwaysOverride = true,
                            allowSmallerMaxTextureSize = true,
                            textureFormat = TextureImporterFormat.ETC2_RGBA8Crunched,
                            compressionQuality = 50,
                        }
                    },
                },
                // Other textures, no alpha
                new TextureChangeOptions
                {
                    shouldCheckTextureShape = false,
                    shouldCheckTextureType = false,

                    shouldCheckTransparency = true,
                    matchHasTransparency = false,

                    shouldCheckSpriteAtlasTag = false,
                    shouldCheckName = false,

                    applySpriteTag = false,
                    applyMipmapSetting = false,

                    applyCompression = true,
                    textureCompression = TextureImporterCompression.Compressed,
                    compressionQuality = 50,
                    crunchedCompression = true,

                    overrideOptions = new TextureOverrideOptions[]
                    {
                        TextureOverrideOptions.NoOverride(BuildTargetGroup.Standalone),
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.iOS,
                            alwaysOverride = true,
                            allowSmallerMaxTextureSize = true,
                            textureFormat = TextureImporterFormat.ETC_RGB4Crunched,
                            compressionQuality = 50,
                        },
                        new TextureOverrideOptions
                        {
                            matchPlatform = BuildTargetGroup.Android,
                            alwaysOverride = false,
                            allowSmallerMaxTextureSize = true,
                            textureFormat = TextureImporterFormat.ETC_RGB4Crunched,
                            compressionQuality = 50,
                        }
                    },
                },
            };
        }

        static HashSet<string> SkipList = new HashSet<string>(StringComparer.Ordinal)
        {
            "Assets/Environments/Tilesets/Caves/FBX/Textures/Caves_Ground1_DFS.tif",
            "Assets/Environments/Tilesets/Caves/FBX/Textures/Caves_Ground4_DFS.tif",
            "Assets/Environments/Tilesets/Caves/FBX/Textures/Caves_Ground10_DFS.tif",
            "Assets/Environments/Tilesets/Caves/FBX/Textures/Caves_Ground_brick2_DFS.tif",
            "Assets/UI/PlatformAssets/SplashScreen.psd",
            "Assets/UI/PlatformAssets/iOS/Labyrinth_AppIcon.png",
        };

        internal static System.Text.StringBuilder npotLog = new System.Text.StringBuilder();
        internal static System.Text.StringBuilder crunchLog = new System.Text.StringBuilder();
        internal static System.Text.StringBuilder mipmapLog = new System.Text.StringBuilder();
        internal static System.Text.StringBuilder etcLog = new System.Text.StringBuilder();
        internal static Dictionary<string, int> spriteCounts = new Dictionary<string, int>();

        public static bool IsTextureSettingsCorrect(string texPath, TextureChangeOptions[] changeOptions) {
            return !SetTextureSettings(texPath, changeOptions, false);
        }

        public static bool SetTextureSettings(string texPath, TextureChangeOptions[] changeOptions) {
            return SetTextureSettings(texPath, changeOptions, true);
        }

        private static bool SetTextureSettings(string texPath, TextureChangeOptions[] changeOptions, bool applyChanges) {
            if (SkipList.Contains(texPath))
            {
                return false;
            }

            var texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (texImporter == null) return false;
            //if (texImporter.textureShape != TextureImporterShape.Texture2D)
            //{
            //    return false;
            //}

            if (!string.IsNullOrEmpty(texImporter.spritePackingTag))
            {
                int value;
                spriteCounts.TryGetValue(texImporter.spritePackingTag, out value);
                spriteCounts[texImporter.spritePackingTag] = value + 1;
            }

            foreach (var option in changeOptions)
            {
                if (option.IsMatch(texImporter))
                {
                    return option.Run(texImporter, applyChanges);
                }
            }

            Debug.LogWarning("No match for texture " + texPath);

            return false;
        }

        private static bool IsCrunchedFormat(TextureImporterFormat format) {
            switch (format)
            {
                case TextureImporterFormat.DXT1Crunched:
                case TextureImporterFormat.DXT5Crunched:
                case TextureImporterFormat.ETC_RGB4Crunched:
                case TextureImporterFormat.ETC2_RGBA8Crunched:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsSourceTextureBiggerThanMaxSize(TextureImporter textureImporter, int maxSize) {
            int width;
            int height;
            GetWidthHeightAdjusted(textureImporter, out width, out height);

            return (width > maxSize || height > maxSize);
        }

        private static bool IsPowerOfTwo(TextureImporter textureImporter) {


            int width;
            int height;
            GetWidthHeightAdjusted(textureImporter, out width, out height);

            return (width & (width - 1)) == 0 && (height & (height - 1)) == 0;
        }

        public static bool IsMultipleOfFour(TextureImporter textureImporter) {
            int width;
            int height;
            GetWidthHeightAdjusted(textureImporter, out width, out height);

            return width % 4 == 0 && height % 4 == 0;
        }

        private static void GetWidthHeightAdjusted(TextureImporter textureImporter, out int width, out int height) {
            Statics.GetWidthHeight(textureImporter, out width, out height);

            int scale = Mathf.Max(width, height);
            float multiplier = Mathf.Clamp01((float)textureImporter.maxTextureSize / scale);
            width = Mathf.RoundToInt(multiplier * width);
            height = Mathf.RoundToInt(multiplier * height);
        }

        //public TextureImporterFormat GetDefaultTextureFormat(TextureImporter importer, TextureImporterPlatformSettings platformSettings, bool doesTextureContainAlpha, bool sourceWasHDR, BuildTarget destinationPlatform) {

        //    var settings = new TextureImporterSettings();
        //    Statics.ReadTextureSettings(importer, settings);

        //}

        static class Statics {
            static Statics() {
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
                var widthHeightMethod = typeof(TextureImporter).GetMethod("GetWidthAndHeight", flags);
                GetWidthHeight = (GetWidthHeightDelegate)Delegate.CreateDelegate(typeof(GetWidthHeightDelegate), widthHeightMethod);

                var formatMethod = typeof(TextureImporter).GetMethod("FormatFromTextureParameters", flags);
                FormatFromTextureParameters = (FormatFromTextureParametersDelegate)Delegate.CreateDelegate(typeof(FormatFromTextureParametersDelegate), formatMethod);

                var settingsMethod = typeof(TextureImporter).GetMethod("ReadTextureSettings", flags);
                ReadTextureSettings = (ReadTextureSettingsDelegate)Delegate.CreateDelegate(typeof(ReadTextureSettingsDelegate), settingsMethod);
            }

            public delegate void GetWidthHeightDelegate(TextureImporter textureImporter, out int width, out int height);
            public static GetWidthHeightDelegate GetWidthHeight;

            public delegate TextureImporterFormat FormatFromTextureParametersDelegate(TextureImporterSettings settings, TextureImporterPlatformSettings platformSettings, bool doesTextureContainAlpha, bool sourceWasHDR, BuildTarget destinationPlatform);
            public static FormatFromTextureParametersDelegate FormatFromTextureParameters;

            public delegate void ReadTextureSettingsDelegate(TextureImporter importer, TextureImporterSettings settings);
            public static ReadTextureSettingsDelegate ReadTextureSettings;
        }

        [Serializable]
        public class TextureChangeOptions {

            public bool shouldCheckTextureShape = false;
            public TextureImporterShape matchTextureShape = TextureImporterShape.Texture2D;

            public bool shouldCheckTextureType = false;
            public TextureImporterType matchTextureType = TextureImporterType.Default;

            public bool shouldCheckTransparency = false;
            public bool matchHasTransparency = false;
            public bool shouldMatchNPOTButDivisBy4 = false;

            public bool shouldCheckMipmap;
            public bool matchHasMipmap;

            public bool shouldCheckSpriteAtlasTag = false;
            public string matchSpriteAtlasTagRegex = "";

            public bool shouldCheckName = false;
            public string matchNameRegex = "";


            public bool applySpriteTag = false;
            public string spriteTag = "";

            public bool applyMipmapSetting = false;
            public bool mipmapEnabled = true;

            public bool applyCompression = false;
            public TextureImporterCompression textureCompression = TextureImporterCompression.Compressed;
            public int compressionQuality = 50;
            public bool crunchedCompression = true;

            public bool allowReadable = false;
            public TextureOverrideOptions[] overrideOptions = Array.Empty<TextureOverrideOptions>();

            Regex _spriteAtlasTagRegex;
            Regex _nameRegex;

            private Regex GetRegex(ref Regex field, string pattern) {
                field = new Regex("^" + pattern + "$", RegexOptions.Compiled);
                return field;
            }

            public bool IsMatch(TextureImporter importer) {

                if (shouldCheckTextureShape)
                {
                    if (importer.textureShape != matchTextureShape)
                    {
                        return false;
                    }
                }

                if (shouldCheckTextureType)
                {
                    if (matchTextureType != importer.textureType)
                    {
                        return false;
                    }
                }

                if (shouldCheckTransparency)
                {
                    if (matchHasTransparency != importer.DoesSourceTextureHaveAlpha())
                    {
                        return false;
                    }

                    if (shouldMatchNPOTButDivisBy4)
                    {
                        if (importer.npotScale != TextureImporterNPOTScale.None || IsPowerOfTwo(importer) || !IsMultipleOfFour(importer))
                        {
                            return false;
                        }
                    }
                }

                if (shouldCheckMipmap)
                {
                    if (importer.mipmapEnabled != matchHasMipmap)
                    {
                        return false;
                    }
                }

                if (shouldCheckSpriteAtlasTag)
                {
                    if (!GetRegex(ref _spriteAtlasTagRegex, matchSpriteAtlasTagRegex).IsMatch(importer.spritePackingTag))
                    {
                        return false;
                    }
                }

                if (shouldCheckName)
                {
                    if (!GetRegex(ref _nameRegex, matchNameRegex).IsMatch(importer.assetPath))
                    {
                        return false;
                    }
                }

                return true;
            }

            public bool Run(TextureImporter importer, bool apply) {

                if (importer.npotScale != TextureImporterNPOTScale.None)
                {
                    if (!IsPowerOfTwo(importer))
                    {
                        npotLog.AppendLine(importer.assetPath);
                    }
                }
                else if (string.IsNullOrEmpty(importer.spritePackingTag))
                {
                    if (importer.mipmapEnabled && !IsPowerOfTwo(importer))
                    {
                        mipmapLog.AppendLine(importer.assetPath);
                    }
                    else if (crunchedCompression && !IsMultipleOfFour(importer))
                    {
                        crunchLog.AppendLine(importer.assetPath);
                    }
                    else if (crunchedCompression && !importer.DoesSourceTextureHaveAlpha() && !IsPowerOfTwo(importer))
                    {
                        etcLog.AppendLine(importer.assetPath);
                    }
                }

                bool changed = false;

                // TEMP
                var standaloneSettings = importer.GetPlatformTextureSettings("Standalone");
                if (standaloneSettings.maxTextureSize < importer.maxTextureSize)
                {
                    changed = true;
                    if (!apply) { return changed; }
                    importer.maxTextureSize = standaloneSettings.maxTextureSize;
                }

                if (applySpriteTag)
                {
                    if (importer.spritePackingTag != spriteTag)
                    {
                        if (apply) { importer.spritePackingTag = spriteTag; }
                        changed = true;
                    }
                }

                if (applyMipmapSetting)
                {
                    if (importer.mipmapEnabled != mipmapEnabled)
                    {
                        if (apply) { importer.mipmapEnabled = mipmapEnabled; }
                        changed = true;
                    }
                }

                if (applyCompression)
                {
                    if (importer.textureCompression != textureCompression)
                    {
                        if (apply) { importer.textureCompression = textureCompression; }
                        changed = true;
                    }
                    if (importer.compressionQuality != compressionQuality)
                    {
                        if (apply) { importer.compressionQuality = compressionQuality; }
                        changed = true;
                    }
                    if (importer.crunchedCompression != crunchedCompression)
                    {
                        if (apply) { importer.crunchedCompression = crunchedCompression; }
                        changed = true;
                    }
                }

                if (!allowReadable)
                {
                    if (importer.isReadable)
                    {
                        if (apply) { importer.isReadable = false; }
                        changed = true;
                    }
                }

                foreach (var option in overrideOptions)
                {
                    var platformSettings = importer.GetPlatformTextureSettings(option.PlatformString);
                    bool subChanged = option.Run(this, importer, platformSettings, apply);
                    if (subChanged && apply)
                    {
                        if (!platformSettings.overridden) { importer.ClearPlatformTextureSettings(option.PlatformString); }
                        else { importer.SetPlatformTextureSettings(platformSettings); }
                    }

                    changed |= subChanged;
                }

                if (changed && apply) { importer.SaveAndReimport(); }

                return changed;
            }
        }

        [Serializable]
        public class TextureOverrideOptions {
            [Tooltip("Which platform to override.")]
            public BuildTargetGroup matchPlatform = BuildTargetGroup.Standalone;

            public bool alwaysOverride = false;
            public bool allowSmallerMaxTextureSize = false;

            public TextureImporterFormat textureFormat = TextureImporterFormat.RGBA32;
            public int compressionQuality = 50;

            public static TextureOverrideOptions NoOverride(BuildTargetGroup matchPlatform) {
                return new TextureOverrideOptions
                {
                    matchPlatform = matchPlatform,
                    allowSmallerMaxTextureSize = false,
                    alwaysOverride = false,
                };
            }

            public string PlatformString {
                get {
                    if (matchPlatform == BuildTargetGroup.iOS) { return "iPhone"; }
                    return matchPlatform.ToString();
                }
            }

            BuildTarget DefaultTarget {
                get {
                    switch (matchPlatform)
                    {
                        case BuildTargetGroup.Android:
                            return BuildTarget.Android;
                        case BuildTargetGroup.Standalone:
                            return BuildTarget.StandaloneWindows64;
                        case BuildTargetGroup.iOS:
                            return BuildTarget.iOS;
                        default:
                            throw new InvalidOperationException("Build group " + matchPlatform + " needs to be implemented.");
                    }
                }
            }

            public bool Run(TextureChangeOptions options, TextureImporter importer, TextureImporterPlatformSettings platformSettings, bool apply) {

                bool changed = false;

                bool useOverride = alwaysOverride;

                if (platformSettings.maxTextureSize != importer.maxTextureSize)
                {
                    if (!platformSettings.overridden || platformSettings.maxTextureSize > importer.maxTextureSize || !allowSmallerMaxTextureSize || !IsSourceTextureBiggerThanMaxSize(importer, Math.Min(platformSettings.maxTextureSize, importer.maxTextureSize)))
                    {
                        if (apply) { platformSettings.maxTextureSize = importer.maxTextureSize; }
                        // Doesn't matter if texture is bigger
                        changed = platformSettings.overridden;
                    }
                    else
                    {
                        // To support allowSmallerMaxTextureSize
                        useOverride = true;
                    }
                }

                bool useCrunched = IsCrunchedFormat(textureFormat);

                if (useOverride != platformSettings.overridden)
                {
                    if (apply) { platformSettings.overridden = useOverride; }
                    changed = true;
                }

                if (useOverride)
                {
                    if (platformSettings.format != textureFormat)
                    {
                        if (apply) { platformSettings.format = textureFormat; }
                        changed = true;
                    }
                    if (platformSettings.compressionQuality != compressionQuality)
                    {
                        if (apply) { platformSettings.compressionQuality = compressionQuality; }
                        changed = true;
                    }
                    if (platformSettings.crunchedCompression != useCrunched)
                    {
                        if (apply) { platformSettings.crunchedCompression = useCrunched; }
                        changed = true;
                    }
                }

                return changed;
            }
        }
    }
}