using System;
using UnityEditor;

/// <summary>
/// Auto set default texture format for Android and iOS to be ASTC_RBG_6x6 or ASTC_RGBA_6x6.
/// Only changes if imported with default settings, where texture format is Automatic.
/// </summary>
class TexturePreProcess// : AssetPostprocessor
{
    public const string PlatformStandalone = "Standalone";
    public const string PlatformAndroid = "Android";
    public const string PlatformiPhone = "iPhone";

    //void OnPreprocessTexture()
    //{
    //    var textureImporter = (TextureImporter)assetImporter;
    //    PreProcessTexture_Standalone(textureImporter);
    //    PreProcessTexture_Android(textureImporter);
    //    PreProcessTexture_iOS(textureImporter);

    //    // set default choice to be compressed if not compressed at all
    //    if (textureImporter.textureCompression == TextureImporterCompression.Uncompressed)
    //    {
    //        textureImporter.textureCompression = TextureImporterCompression.Compressed;
    //    }
    //}

    /// <summary>
    /// Both Win and Mac have DXT as default format
    /// </summary>
    public static bool PreProcessTexture_Standalone(TextureImporter importer, bool ignoreOverridenSettings = false)
    {
        var platformSettings = importer.GetPlatformTextureSettings(PlatformStandalone);

        // if already overriden, someone might manually change this, ignore, pickup only default settings and change it
        if (!ignoreOverridenSettings && platformSettings.overridden)
            return false;

        // textures with alpha to RGBA, others RGB
        // sprites in atlases forced to RGBA so they all fit in same one and not be split into 2 atlases
        bool changed = false;
        bool hasAtlas = !string.IsNullOrEmpty(importer.spritePackingTag);
        bool isSprite = importer.textureType == TextureImporterType.Sprite;
        if (importer.DoesSourceTextureHaveAlpha() || (isSprite && hasAtlas))
        {
            if (platformSettings.format != TextureImporterFormat.DXT5 && platformSettings.format != TextureImporterFormat.DXT5Crunched)
            {
                platformSettings.format = TextureImporterFormat.DXT5;
                changed = true;
            }
        }
        else if (platformSettings.format != TextureImporterFormat.DXT1 && platformSettings.format != TextureImporterFormat.DXT1Crunched)
        {
            platformSettings.format = TextureImporterFormat.DXT1;
            changed = true;
        }

        if (changed)
        {
            platformSettings.overridden = true;
            importer.SetPlatformTextureSettings(platformSettings);
        }

        return changed;
    }

    public static bool PreProcessTexture_Android(TextureImporter importer, bool ignoreOverridenSettings = false)
    {
        var platformSettings = importer.GetPlatformTextureSettings(PlatformAndroid);

        // if already overriden, someone might manually change this, ignore, pickup only default settings and change it
        if (!ignoreOverridenSettings && platformSettings.overridden)
            return false;

        bool changed = ProcessAstcTexture(importer, platformSettings);
        return changed;
    }

    public static bool PreProcessTexture_iOS(TextureImporter importer, bool changePlatformOverridenSettings = false)
    {
        var platformSettings = importer.GetPlatformTextureSettings(PlatformiPhone);

        // if already overriden, someone might manually change this, ignore, pickup only default settings and change it
        if (!changePlatformOverridenSettings && platformSettings.overridden)
            return false;

        bool changed = ProcessAstcTexture(importer, platformSettings);
        return changed;
    }

    private static bool ProcessAstcTexture(TextureImporter importer, TextureImporterPlatformSettings platformSettings)
    {
        // textures with alpha to RGBA, others RGB
        // sprites in atlases forced to RGBA so they all fit in same one and not be split into 2 atlases
        bool changed = false;
        bool hasAtlas = !string.IsNullOrEmpty(importer.spritePackingTag);
        bool isSprite = importer.textureType == TextureImporterType.Sprite;
        if (importer.DoesSourceTextureHaveAlpha() || (isSprite && hasAtlas))
        {
            if (!Is_ASTC_RGBA(platformSettings.format))
            {
                if (Is_ASTC_RGB(platformSettings.format))
                    // ASTC to same quality conversion
                    platformSettings.format = ASTC_RGB_to_RGBA(platformSettings.format);
                else
                    // non ASTC to ASTC 6x6 as default
                    platformSettings.format = TextureImporterFormat.ASTC_RGBA_6x6;
                changed = true;
            }
        }
        else if (!Is_ASTC_RGB(platformSettings.format))
        {
            if (Is_ASTC_RGBA(platformSettings.format))
                // ASTC to same quality conversion
                platformSettings.format = ASTC_RGBA_to_RGB(platformSettings.format);
            else
                // non ASTC to ASTC 6x6 as default
                platformSettings.format = TextureImporterFormat.ASTC_RGB_6x6;
            changed = true;
        }

        if (changed)
        {
            platformSettings.overridden = true;
            importer.SetPlatformTextureSettings(platformSettings);
        }

        return changed;
    }

    private static bool Is_ASTC_RGB(TextureImporterFormat format)
    {
        switch (format)
        {
            case TextureImporterFormat.ASTC_RGB_4x4:
            case TextureImporterFormat.ASTC_RGB_5x5:
            case TextureImporterFormat.ASTC_RGB_6x6:
            case TextureImporterFormat.ASTC_RGB_8x8:
            case TextureImporterFormat.ASTC_RGB_10x10:
            case TextureImporterFormat.ASTC_RGB_12x12:
                return true;
            default:
                return false;
        }
    }

    private static bool Is_ASTC_RGBA(TextureImporterFormat format)
    {
        switch (format)
        {
            case TextureImporterFormat.ASTC_RGBA_4x4:
            case TextureImporterFormat.ASTC_RGBA_5x5:
            case TextureImporterFormat.ASTC_RGBA_6x6:
            case TextureImporterFormat.ASTC_RGBA_8x8:
            case TextureImporterFormat.ASTC_RGBA_10x10:
            case TextureImporterFormat.ASTC_RGBA_12x12:
                return true;
            default:
                return false;
        }
    }

    private static TextureImporterFormat ASTC_RGB_to_RGBA(TextureImporterFormat format)
    {
        switch (format)
        {
            case TextureImporterFormat.ASTC_RGB_4x4: return TextureImporterFormat.ASTC_RGBA_4x4;
            case TextureImporterFormat.ASTC_RGB_5x5: return TextureImporterFormat.ASTC_RGBA_5x5;
            case TextureImporterFormat.ASTC_RGB_6x6: return TextureImporterFormat.ASTC_RGBA_6x6;
            case TextureImporterFormat.ASTC_RGB_8x8: return TextureImporterFormat.ASTC_RGBA_8x8;
            case TextureImporterFormat.ASTC_RGB_10x10: return TextureImporterFormat.ASTC_RGBA_10x10;
            case TextureImporterFormat.ASTC_RGB_12x12: return TextureImporterFormat.ASTC_RGBA_12x12;
            default:
                return format;
        }
    }

    private static TextureImporterFormat ASTC_RGBA_to_RGB(TextureImporterFormat format)
    {
        switch (format)
        {
            case TextureImporterFormat.ASTC_RGBA_4x4: return TextureImporterFormat.ASTC_RGB_4x4;
            case TextureImporterFormat.ASTC_RGBA_5x5: return TextureImporterFormat.ASTC_RGB_5x5;
            case TextureImporterFormat.ASTC_RGBA_6x6: return TextureImporterFormat.ASTC_RGB_6x6;
            case TextureImporterFormat.ASTC_RGBA_8x8: return TextureImporterFormat.ASTC_RGB_8x8;
            case TextureImporterFormat.ASTC_RGBA_10x10: return TextureImporterFormat.ASTC_RGB_10x10;
            case TextureImporterFormat.ASTC_RGBA_12x12: return TextureImporterFormat.ASTC_RGB_12x12;
            default:
                return format;
        }
    }
}