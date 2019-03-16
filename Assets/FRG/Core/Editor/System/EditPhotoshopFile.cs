#define EDIT_PHOTOSHOP_FILE_TEST_OUTPUT

using FRG.Core;
using FRG.SharedCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class EditPhotoshopFile
{
    private const string PngExtension = ".png";
    private const string PsdExtension = ".psd";
    private const string BackupExtension = "~";
    private const string AssetsPrefix = "Assets/";

#if EDIT_PHOTOSHOP_FILE_TEST_OUTPUT
    private static Color32[] originalPixels;
#endif

    private static string WorkRoot { get { return StandardEditorPaths.Work; } }

    #region Commands
    [UnityEditor.Callbacks.OnOpenAsset(300)]
    private static bool EditPhotoshopFileAction(int instanceId, int line)
    {
        // This check should be fast
        string assetPath = AssetDatabase.GetAssetPath(instanceId);
        if (!IsSyncedPngPath(assetPath)) return false;

        try
        {
            Process.Start(StandardEditorPaths.GetAbsolutePath(GetWorkPath(assetPath)));
        }
        catch (Exception e)
        {
            DisplayError(
                "Edit Photoshop File",
                "There was an error attempting to edit the PSD at \"" + assetPath + "\".",
                e);
        }
        return true;
    }

    [MenuItem("Assets/Extract PNG and Move PSD to Work Folder", priority = 201)]
    private static void MovePsdToWorkFolder()
    {
        RunForSelection("Extract PNG and Move PSD to Work Folder", Statics.IsPsdAsset, GetAssetPath, MoveSinglePsdToWorkFolder);
    }

    [MenuItem("Assets/Replace PNG with PSD from Work Folder", priority = 202)]
    private static void ReplaceWithPsdFromWorkFolder()
    {
        RunForSelection("Replace PNG with PSD from Work Folder", Statics.IsSyncedPngAsset, GetWorkPath, ReplaceSingleWithPsdFromWorkFolder);
    }

    [MenuItem("Assets/Sync PNG with PSD from Work Folder", priority = 203)]
    private static void SyncPngToPsdFromWorkFolder()
    {
        RunForSelection("Sync PNG with PSD from Work Folder", Statics.IsSyncedPngAsset, GetWorkPath, SyncSinglePngToPsdFromWorkFolder);
    }

    [MenuItem("Assets/Show Work Folder PSD in Explorer", priority = 204)]
    private static void OpenPsdFolder()
    {
        foreach (UnityEngine.Object asset in Selection.objects)
        {
            if (Statics.IsWorkPsdAvailable(asset))
            {
                EditorUtility.RevealInFinder(GetWorkPath(asset));
            }
        }
    }
    #endregion

    #region Validation
    [MenuItem("Assets/Extract PNG and Move PSD to Work Folder", true)]
    private static bool IsPsdAssetSelected()
    {
        return ValidateSelection(Statics.IsPsdAsset);
    }

    [MenuItem("Assets/Replace PNG with PSD from Work Folder", true)]
    [MenuItem("Assets/Sync PNG with PSD from Work Folder", true)]
    private static bool IsSyncedPngAssetSelected()
    {
        return ValidateSelection(Statics.IsSyncedPngAsset);
    }

    [MenuItem("Assets/Show Work Folder PSD in Explorer", true)]
    private static bool IsWorkPsdAvailableForSelected()
    {
        return ValidateSelection(Statics.IsWorkPsdAvailable);
    }
    #endregion

    #region Single Asset Implementations
    private static UnityEngine.Object MoveSinglePsdToWorkFolder(UnityEngine.Object asset)
    {
        string psdAssetPath = GetAssetPath(asset);
        string psdWorkPath = GetWorkPath(psdAssetPath);
        string pngAssetPath = Path.ChangeExtension(psdAssetPath, PngExtension);

        string pngMetaPath = PathUtil.MetaFilePath(pngAssetPath);
        string psdMetaPath = PathUtil.MetaFilePath(psdAssetPath);

        if (File.Exists(psdWorkPath))
        {
            DisplayError(
                "Cannot Move PSD",
                "A file already exists at \"" + psdWorkPath + "\"; you must remove it in order for this command to work.");
            return null;
        }

        // extract png from psd
        byte[] pngBytes;
        byte[] metaContents = File.ReadAllBytes(psdMetaPath);
        try
        {
            pngBytes = MutateAndReadPngBytesFromTextureAtPath(psdAssetPath);
            if (pngBytes == null)
            {
                DisplayError(
                    "Cannot Read PSD",
                    "There was an error reading \"" + psdAssetPath + "\" as a Texture2D.");
                return null;
            }
        }
        finally
        {
            PathUtil.WriteBinaryFile(psdMetaPath, metaContents);
            AssetDatabase.ImportAsset(psdAssetPath, ImportAssetOptions.ForceSynchronousImport);
        }

        bool useSvn = SvnUtil.IsSameWorkingCopy(psdAssetPath, psdWorkPath);

        string workDirectory = Path.GetDirectoryName(psdWorkPath);
        PathUtil.CreateDirectoryRecursively(workDirectory);
        SvnUtil.ForceAddWithoutReplace(workDirectory, useSvn);

        // Move psd to work folder
        SvnUtil.Move(psdAssetPath, psdWorkPath, useSvn);

        // Write PNG bytes
        PathUtil.WriteBinaryFile(psdAssetPath, pngBytes);

        // Reimport
        AssetDatabase.ImportAsset(psdAssetPath, ImportAssetOptions.ForceSynchronousImport);

        // rename psd meta file so guid stays the same and all references are preserved to png
        string moveError = AssetDatabase.MoveAsset(psdAssetPath, pngAssetPath);
        if (!string.IsNullOrEmpty(moveError))
        {
            DisplayError(
                "Cannot Rename PSD",
                "There was an error moving \"" + psdAssetPath + "\" to \"" + pngAssetPath + "\"\n\n" + moveError);
            return null;
        }

        // Repair SVN move operations
        SvnUtil.RepairMove(psdMetaPath, pngMetaPath, useSvn);
        SvnUtil.ForceAddWithoutReplace(pngAssetPath, useSvn);

        Texture2D updatedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(pngAssetPath);
        if (updatedTexture == null)
        {
            DisplayError(
                "Cannot Load New PNG Image",
                "There was an error loading the resulting PNG image, \"" + pngAssetPath + "\".");
            return null;
        }

#if EDIT_PHOTOSHOP_FILE_TEST_OUTPUT
        TestOutput(pngAssetPath, pngMetaPath, updatedTexture);
#endif

        return updatedTexture;
    }

    private static UnityEngine.Object ReplaceSingleWithPsdFromWorkFolder(UnityEngine.Object asset)
    {
        string pngAssetPath = GetAssetPath(asset);
        string psdAssetPath = Path.ChangeExtension(pngAssetPath, PsdExtension);
        string psdWorkPath = GetWorkPath(psdAssetPath);

        string pngMetaPath = PathUtil.MetaFilePath(pngAssetPath);
        string psdMetaPath = PathUtil.MetaFilePath(psdAssetPath);

        // Rename png to psd
        string moveError = AssetDatabase.MoveAsset(pngAssetPath, psdAssetPath);
        if (!string.IsNullOrEmpty(moveError))
        {
            DisplayError(
                "Cannot Rename PSD",
                "There was an error moving \"" + pngAssetPath + "\" to \"" + pngAssetPath + "\"\n\n" + moveError);
            return null;
        }

        // Delete the file and leave only meta file
        File.Delete(psdAssetPath);

        bool useSvn = SvnUtil.IsSameWorkingCopy(psdAssetPath, psdWorkPath);

        SvnUtil.Move(psdWorkPath, psdAssetPath, useSvn);
        SvnUtil.RepairMove(pngMetaPath, psdMetaPath, useSvn);
        SvnUtil.ForceDelete(pngAssetPath, useSvn);

        AssetDatabase.ImportAsset(psdAssetPath, ImportAssetOptions.ForceSynchronousImport);

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(psdAssetPath);
        if (texture == null)
        {
            DisplayError(
                "Cannot Load New PSD Image",
                "There was an error loading the resulting PSD image, \"" + psdAssetPath + "\".");
            return null;
        }
        return texture;
    }

    private static UnityEngine.Object SyncSinglePngToPsdFromWorkFolder(UnityEngine.Object asset)
    {
        string pngAssetPath = GetAssetPath(asset);
        string psdWorkPath = GetWorkPath(pngAssetPath);

        string pngMetaPath = PathUtil.MetaFilePath(pngAssetPath);

        byte[] originalPngBytes = null;
        byte[] pngMetaBytes = null;
        byte[] updatedPngBytes;
        // extract PNG
        try
        {
            originalPngBytes = File.ReadAllBytes(pngAssetPath);
            pngMetaBytes = File.ReadAllBytes(pngMetaPath);

            File.Copy(psdWorkPath, pngAssetPath, true);
            AssetDatabase.ImportAsset(pngAssetPath, ImportAssetOptions.ForceSynchronousImport);

            updatedPngBytes = MutateAndReadPngBytesFromTextureAtPath(pngAssetPath);
            if (updatedPngBytes == null)
            {
                DisplayError(
                    "Cannot Read PSD",
                    "There was an error reading \"" + pngAssetPath + "\" as a Texture2D.");
                return null;
            }
        }
        finally
        {
            if (originalPngBytes != null)
            {
                File.WriteAllBytes(pngAssetPath, originalPngBytes);
            }
            if (pngMetaBytes != null)
            {
                File.WriteAllBytes(pngMetaPath, pngMetaBytes);
            }
        }

        PathUtil.WriteBinaryFile(pngAssetPath, updatedPngBytes);
        AssetDatabase.ImportAsset(pngAssetPath, ImportAssetOptions.ForceSynchronousImport);

        Texture2D updatedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(pngAssetPath);
        if (updatedTexture == null)
        {
            DisplayError(
                "Cannot Load Updated PNG Image",
                "There was an error loading the updated PNG image, \"" + pngAssetPath + "\".");
            return null;
        }


#if EDIT_PHOTOSHOP_FILE_TEST_OUTPUT
        TestOutput(pngAssetPath, pngMetaPath, updatedTexture);
#endif

        return updatedTexture;
    }
    #endregion

    /// <summary>
    /// Back up the file to .backup folder.
    /// Don't append date because that will fill up the hard drive.
    /// </summary>
    private static void RunForSelection(string title, Func<UnityEngine.Object, bool> validationFunc, Func<UnityEngine.Object, string> sourceFunc, Func<UnityEngine.Object, UnityEngine.Object> action)
    {
        if (validationFunc == null) throw new ArgumentNullException("validationFunc");
        if (sourceFunc == null) throw new ArgumentNullException("sourceFunc");
        if (action == null) throw new ArgumentNullException("action");

#if EDIT_PHOTOSHOP_FILE_TEST_OUTPUT
        originalPixels = null;
#endif

        UnityEngine.Object[] selection = Selection.objects;
        Selection.objects = ArrayUtil.Empty<UnityEngine.Object>();

        List<UnityEngine.Object> newSelection = new List<UnityEngine.Object>();
        foreach (UnityEngine.Object asset in selection)
        {
            if (!validationFunc(asset))
            {
                newSelection.Add(asset);
                continue;
            }

            string assetPath = sourceFunc(asset);

            string backupPath = assetPath + BackupExtension;
            try
            {
                if (File.Exists(backupPath))
                {
                    if (!EditorUtility.DisplayDialog(title, "There is a backup file at \"" + backupPath + "\" from a previous failed PSD operation. This operation will overwrite it. Do you want to continue?", "Continue", "Stop"))
                    {
                        return;
                    }
                }

                File.Copy(assetPath, backupPath, true);
            }
            catch (FileNotFoundException)
            {
                DisplayError(title, "The file at \"" + assetPath + "\" could not be found.");
                return;
            }
            catch (Exception e)
            {
                if (!ReflectionUtil.IsIOException(e)) throw;

                DisplayError(title, "There was an error backing up \"" + assetPath + "\".", e);
                return;
            }

            try
            {
                bool successful = false;
                try
                {
                    UnityEngine.Object result = action(asset);
                    successful = (result != null);
                    if (successful)
                    {
                        newSelection.Add(result);
                    }
                    else
                    {
                        return;
                    }
                }
                finally
                {
                    bool delete = true;
                    if (!successful)
                    {
                        delete = false;
                        try
                        {
                            File.Copy(backupPath, assetPath, true);
                            delete = true;
                        }
                        catch (FileNotFoundException)
                        {
                            DisplayError(
                                title,
                                "The backup expected to be at \"" + backupPath + "\" could not be found, so the file was not restored. " +
                                "You may have to fix this error manually.");
                        }
                        catch (Exception e)
                        {
                            if (!ReflectionUtil.IsIOException(e)) throw;

                            DisplayError(
                                title,
                                "There was an error attempting to restore the backup at \"" + backupPath + "\"." +
                                "\n\nYou may need to fix this error manually.",
                                e);
                        }
                    }
                    if (delete)
                    {
                        try
                        {
                            File.Delete(backupPath);
                        }
                        catch (Exception e)
                        {
                            if (!ReflectionUtil.IsIOException(e)) throw;

                            DisplayError(
                                title,
                                "There was an error attempting to delete the backup at \"" + backupPath + "\"." +
                                "\n\nYou may need to delete it manually.",
                                e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ReflectionUtil.CheckDangerousException(e);

                DisplayError(
                    title,
                    "There was an error attempting to " + title.ToLowerInvariant() + " at \"" + assetPath + "\"." +
                    "\n\nAutomatically restored the old file.",
                    e);
                return;
            }
        }
        Selection.objects = newSelection.ToArray();
    }

    #region Helpers
    /// <summary>
    /// Reads PNG bytes from texture asset in unity.
    /// </summary>
    private static byte[] MutateAndReadPngBytesFromTextureAtPath(string textureRelativePath)
    {
        Texture2D originalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureRelativePath);
        if (originalTexture == null)
        {
            return null;
        }

        var importer = AssetImporter.GetAtPath(textureRelativePath) as TextureImporter;
        if (importer == null)
        {
            return null;
        }

#if EDIT_PHOTOSHOP_FILE_TEST_OUTPUT
        if (!importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();

            originalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureRelativePath);
        }

        originalPixels = originalTexture.GetPixels32();
#endif

        importer.isReadable = true;
        importer.textureType = TextureImporterType.Default;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.convertToNormalmap = false;
        importer.textureShape = TextureImporterShape.Texture2D;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.fadeout = false;

        if (Statics.GetPlatformName != null)
        {
            string platformName = (string)Statics.GetPlatformName(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            importer.ClearPlatformTextureSettings(platformName);
        }
        importer.SaveAndReimport();

        Texture2D updatedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureRelativePath);
        if (updatedTexture != null)
        {
            return updatedTexture.EncodeToPNG();
        }
        else
        {
            return null;
        }
    }

    private static bool ValidateSelection(Func<UnityEngine.Object, bool> validationFunc)
    {
        // This happens a lot so optimize slightly
        if (!(Selection.activeObject is Texture2D)) return false;

        bool isAny = false;
        foreach (UnityEngine.Object asset in Selection.objects)
        {
            // Require only textures
            if (!(asset is Texture2D)) return false;

            // Require at least one that this applies to
            if (!validationFunc(asset)) continue;

            isAny = true;
        }

        return isAny;
    }

    private static bool IsValidAssetPath(string path, string extension)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        if (!path.StartsWith(AssetsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (!path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }

    private static bool IsSyncedPngPath(string path)
    {
        if (!IsValidAssetPath(path, PngExtension))
        {
            return false;
        }

        if (!File.Exists(GetWorkPath(path)))
        {
            return false;
        }

        return true;
    }

    private static string GetAssetPath(UnityEngine.Object asset)
    {
        return AssetDatabase.GetAssetPath(asset);
    }

    private static string GetWorkPath(UnityEngine.Object asset)
    {
        return GetWorkPath(GetAssetPath(asset));
    }

    private static string GetWorkPath(string assetPath)
    {
        string path = assetPath;

        path = path.Substring(AssetsPrefix.Length);
        if (!path.EndsWith(PsdExtension, StringComparison.OrdinalIgnoreCase))
        {
            path = Path.ChangeExtension(path, PsdExtension);
        }
        return WorkRoot + path;
    }

    private static void DisplayError(string title, string message, Exception e = null)
    {
        UnityEngine.Debug.LogError(title + ": " + message);
        UnityEngine.Debug.LogException(e);

        if (e != null) message += "";
        EditorUtility.DisplayDialog(title, message, "OK");
    }

#if EDIT_PHOTOSHOP_FILE_TEST_OUTPUT
    private static void TestOutput(string texturePath, string metaPath, Texture2D updatedTexture)
    {
        var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null)
        {
            UnityEngine.Debug.LogWarning("Can't test texture; no importer at path \"" + texturePath + "\".");
            return;
        }

        Color32[] updatedPixels;
        if (!importer.isReadable)
        {
            byte[] metaContents = File.ReadAllBytes(metaPath);
            try
            {
                importer.isReadable = true;
                importer.SaveAndReimport();

                updatedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

                updatedPixels = updatedTexture.GetPixels32();
            }
            finally
            {
                PathUtil.WriteBinaryFile(metaPath, metaContents);
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
            }
        }
        else
        {
            updatedPixels = updatedTexture.GetPixels32();
        }

        if (!ArrayUtil.CompareLists(originalPixels, updatedPixels))
        {
            UnityEngine.Debug.LogWarning("PNG texture differs from original PSD at path \"" + texturePath + "\".");
        }
    }
#endif
    #endregion

    private static class Statics
    {
        public static readonly Func<BuildTargetGroup, string> GetPlatformName;

        public static readonly Func<UnityEngine.Object, bool> IsPsdAsset = asset => IsValidAssetPath(GetAssetPath(asset), PsdExtension);
        public static readonly Func<UnityEngine.Object, bool> IsSyncedPngAsset = asset => IsSyncedPngPath(GetAssetPath(asset));
        public static readonly Func<UnityEngine.Object, bool> IsWorkPsdAvailable = asset => File.Exists(GetWorkPath(asset));

        static Statics()
        {
            MethodInfo method = typeof(PlayerSettings).GetMethod("GetPlatformName", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                GetPlatformName = (Func<BuildTargetGroup, string>)Delegate.CreateDelegate(typeof(Func<BuildTargetGroup, string>), method);
            }
        }
    }
}
