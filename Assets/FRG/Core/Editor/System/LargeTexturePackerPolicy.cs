using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Sprites;
using System.Collections.Generic;
using FRG.Core;

/// <summary>
/// Supports 4k atlases.
/// </summary>
public class LargeTexturePackerPolicy : IPackerPolicy {
    protected class Entry {
        public Sprite sprite;
        public AtlasSettings settings;
        public string atlasName;
        public SpritePackingMode packingMode;
        public int anisoLevel;
    }

    private const uint kDefaultPaddingPower = 3; // Good for base and two mip levels.

    public virtual int GetVersion() { return 1; }
    public virtual bool AllowSequentialPacking { get { return false; } }

    protected virtual string TagPrefix { get { return "[TIGHT]"; } }
    protected virtual bool AllowTightWhenTagged { get { return true; } }
    protected virtual bool AllowRotationFlipping { get { return false; } }

    public void OnGroupAtlases(BuildTarget target, PackerJob job, int[] textureImporterInstanceIDs) {
        List<Entry> entries = new List<Entry>();

        string targetName = "";
        if (target != BuildTarget.NoTarget)
        {
            targetName = GetBuildTargetName(target);
        }

        foreach (int instanceID in textureImporterInstanceIDs)
        {
            TextureImporter ti = EditorUtility.InstanceIDToObject(instanceID) as TextureImporter;

            TextureFormat desiredFormat;
            ColorSpace colorSpace;
            int compressionQuality;
            ti.ReadTextureImportInstructions(target, out desiredFormat, out colorSpace, out compressionQuality);

            TextureImporterSettings tis = new TextureImporterSettings();
            ti.ReadTextureSettings(tis);

            bool hasAlphaSplittingForCompression = (targetName != "" && HasPlatformEnabledAlphaSplittingForCompression(targetName, ti));

            Sprite[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(ti.assetPath).Select(x => x as Sprite).Where(x => x != null).ToArray();
            foreach (Sprite sprite in sprites)
            {
                Entry entry = new Entry();
                entry.sprite = sprite;
                entry.settings.format = desiredFormat;
                entry.settings.colorSpace = colorSpace;
                // Use Compression Quality for Grouping later only for Compressed Formats. Otherwise leave it Empty.
                entry.settings.compressionQuality = IsCompressedTextureFormat(desiredFormat) ? compressionQuality : 0;
                entry.settings.filterMode = Enum.IsDefined(typeof(FilterMode), ti.filterMode) ? ti.filterMode : FilterMode.Bilinear;
                // NOTE: This is the major change
                entry.settings.maxWidth = 4096;
                entry.settings.maxHeight = 4096;
                entry.settings.generateMipMaps = ti.mipmapEnabled;
                entry.settings.enableRotation = AllowRotationFlipping;
                entry.settings.allowsAlphaSplitting = IsTextureFormatETC1Compression(desiredFormat) && hasAlphaSplittingForCompression;
                if (ti.mipmapEnabled)
                    entry.settings.paddingPower = kDefaultPaddingPower;
                else
                    entry.settings.paddingPower = (uint)EditorSettings.spritePackerPaddingPower;
                entry.atlasName = ParseAtlasName(ti.spritePackingTag);
                entry.packingMode = GetPackingMode(ti.spritePackingTag, tis.spriteMeshType);
                entry.anisoLevel = ti.anisoLevel;

                //var width = (int)sprite.rect.width;
                //var height = (int)sprite.rect.height;
                //if ((width & (width - 1)) == 0 && ((height & (height - 1)) == 0) && width > 32 && height > 32) {
                //    Debug.Log(width + "x" + height + ": " + sprite.name, sprite);
                //}

                entries.Add(entry);
            }

            Resources.UnloadAsset(ti);
        }

        // First split sprites into groups based on atlas name
        var atlasGroups =
            from e in entries
            group e by e.atlasName;
        foreach (var atlasGroup in atlasGroups)
        {
            int page = 0;
            // Then split those groups into smaller groups based on texture settings
            var settingsGroups =
                from t in atlasGroup
                group t by t.settings;
            foreach (var settingsGroup in settingsGroups)
            {
                string atlasName = atlasGroup.Key;
                if (settingsGroups.Count() > 1)
                    atlasName += string.Format(" (Group {0})", page);

                AtlasSettings settings = settingsGroup.Key;
                settings.anisoLevel = 1;
                // Use the highest aniso level from all entries in this atlas
                if (settings.generateMipMaps)
                    foreach (Entry entry in settingsGroup)
                        if (entry.anisoLevel > settings.anisoLevel)
                            settings.anisoLevel = entry.anisoLevel;

                job.AddAtlas(atlasName, settings);

                BuildAtlasReport(settingsGroup, atlasName);

                foreach (Entry entry in settingsGroup)
                {
                    var startTime = DateTime.UtcNow;
                    job.AssignToAtlas(atlasName, entry.sprite, entry.packingMode, SpritePackingRotation.None);
                    var diff = DateTime.UtcNow - startTime;
                    if (diff.TotalSeconds > 5)
                    {
                        Debug.LogWarning("Packing sprite was slow: " + entry.sprite.name + ", seconds: " + diff.TotalSeconds);
                    }
                }

                ++page;
            }
        }
    }

    protected bool HasPlatformEnabledAlphaSplittingForCompression(string targetName, TextureImporter ti) {
        TextureImporterPlatformSettings platformSettings = ti.GetPlatformTextureSettings(targetName);
        return (platformSettings.overridden && platformSettings.allowsAlphaSplitting);
    }

    private static void BuildAtlasReport(IGrouping<UnityEditor.Sprites.AtlasSettings, Entry> settingsGroup, string atlasName) {
        const int Count = 10;

        var settingsGroupdSorted = settingsGroup.OrderBy((x, y) => (y.sprite.texture.width * y.sprite.texture.height).CompareTo(x.sprite.texture.width * x.sprite.texture.height));
        int counter2 = 0;
        string log5LargestTextures = "Atlas " + atlasName + " first " + Math.Min(settingsGroupdSorted.Count(), Count) + " largest textures:";
        foreach (Entry entry in settingsGroupdSorted)
        {
            if (counter2 == 0)
            {
                log5LargestTextures += "\nsettings: mipmaps=" + entry.settings.generateMipMaps + " padding=" + entry.settings.paddingPower + " format=" + entry.settings.format + " quality=" + entry.settings.compressionQuality + " other=" + entry.settings.allowsAlphaSplitting + entry.settings.enableRotation + entry.settings.filterMode + entry.settings.colorSpace;
            }
            if (counter2++ >= Count) break;
            log5LargestTextures += "\n" + entry.sprite.name + " (" + entry.sprite.texture.width + "x" + entry.sprite.texture.height + ")";

        }


        Debug.Log(log5LargestTextures, settingsGroup.FirstOrDefault()?.sprite);
    }

    protected bool IsTagPrefixed(string packingTag) {
        packingTag = packingTag.Trim();
        if (packingTag.Length < TagPrefix.Length)
            return false;
        return (packingTag.Substring(0, TagPrefix.Length) == TagPrefix);
    }

    private string ParseAtlasName(string packingTag) {
        string name = packingTag.Trim();
        if (IsTagPrefixed(name))
            name = name.Substring(TagPrefix.Length).Trim();
        return (name.Length == 0) ? "(unnamed)" : name;
    }

    private SpritePackingMode GetPackingMode(string packingTag, SpriteMeshType meshType) {
        if (meshType == SpriteMeshType.Tight)
            if (IsTagPrefixed(packingTag) == AllowTightWhenTagged)
                return SpritePackingMode.Tight;
        return SpritePackingMode.Rectangle;
    }


    static string GetBuildTargetName(BuildTarget target) {
        return Statics.GetBuildTargetName(target);
    }
    static bool IsCompressedTextureFormat(TextureFormat format) {
        return Statics.IsCompressedTextureFormat(format);
    }
    static bool IsTextureFormatETC1Compression(TextureFormat format) {
        return Statics.IsTextureFormatETC1Compression(format);
    }

#if UNITY_EDITOR
    static class Statics {
        static Statics() {
            var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

            var buildTargetMethod = typeof(BuildPipeline).GetMethod("GetBuildTargetName", flags);
            GetBuildTargetName = (Func<BuildTarget, string>)Delegate.CreateDelegate(typeof(Func<UnityEditor.BuildTarget, string>), buildTargetMethod);

            Type textureUtil = null;
            foreach (var type in ReflectionUtil.GetEditorTypes())
            {
                if (type.FullName == "UnityEditor.TextureUtil")
                {
                    textureUtil = type;
                    break;
                }
            }

            var compressedMethod = textureUtil.GetMethod("IsCompressedTextureFormat", flags);
            IsCompressedTextureFormat = (Func<TextureFormat, bool>)Delegate.CreateDelegate(typeof(Func<TextureFormat, bool>), compressedMethod);

            var etcMethod = typeof(TextureImporter).GetMethod("IsTextureFormatETC1Compression", flags);
            IsTextureFormatETC1Compression = (Func<TextureFormat, bool>)Delegate.CreateDelegate(typeof(Func<TextureFormat, bool>), etcMethod);
        }

        public static readonly Func<BuildTarget, string> GetBuildTargetName;
        public static readonly Func<TextureFormat, bool> IsCompressedTextureFormat;
        public static readonly Func<TextureFormat, bool> IsTextureFormatETC1Compression;
    }
#endif
}