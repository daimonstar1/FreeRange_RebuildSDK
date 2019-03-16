using FRG.SharedCore;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FRG.Core
{
    public class CheckProjectTextureSettings
    {
        [MenuItem("Assets/FRG/Check Texture Import Settings")]
        private static void CheckTextureSetting()
        {
            var texPaths = new List<string>();
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                UnityEngine.Object current = Selection.objects[i];
                if (current is Texture)
                {
                    var texPath = AssetDatabase.GetAssetPath(current);
                    if (!string.IsNullOrEmpty(texPath))
                    {
                        texPaths.Add(texPath);
                    }
                }
            }

            ProcessTexturePaths(texPaths);
        }

        [MenuItem("Assets/FRG/Check Texture Import Settings", true)]
        private static bool IsTexture()
        {
            return Selection.activeObject is Texture;
        }

        [MenuItem("FRG/Dependency Tools/Check Project Texture Settings", priority = 6)]
        public static void CheckTextureSettings()
        {
            Debug.Log("Checking all textures in the project for bad import settings...");

            EditorUtility.UnloadUnusedAssetsImmediate();
            GC.Collect();

            var allTextureGuids = AssetDatabase.FindAssets("t:Texture");
            ProcessTextureGuids(allTextureGuids);
        }

        public static void ProcessTextureGuids(string[] allTextureGuids)
        {
            List<string> allTexturePaths = new List<string>();
            foreach (var texGuid in allTextureGuids)
            {
                string texPath = AssetDatabase.GUIDToAssetPath(texGuid);
                allTexturePaths.Add(texPath);
            }

            ProcessTexturePaths(allTexturePaths);
        }

        public static void ProcessTexturePaths(List<string> allTexturePaths)
        {
            var importersToReimport = new List<TextureImporter>();

            bool success = EditorProgress.RunWithCancel("Fixing Texture settings", () =>
            {
                for (int i = 0; i < allTexturePaths.Count; i++)
                {
                    if (i % 50 == 0)
                    {
                        using (EditorProgress.PushIteration("Cleaning up memory...", i, allTexturePaths.Count))
                        {
                            GC.Collect();
                            EditorUtility.UnloadUnusedAssetsImmediate();
                        }
                    }

                    string texPath = allTexturePaths[i];
                    // This will crash unity with allocating memory constantly
                    //Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(texPath);
                    //if (tex == null) continue;
                    var texImporter = TextureImporter.GetAtPath(texPath) as TextureImporter;
                    if (texImporter == null) continue;

                    using (EditorProgress.PushIteration(texPath, i, allTexturePaths.Count))
                    {
                        bool changed = FixImporterSettings(texImporter);
                        if (changed)
                        {
                            importersToReimport.Add(texImporter);
                        }
                    }
                }
            });

            if (!success)
            {
                Debug.Log("Texture check aborted but let's try to give the results we got.");
            }

            if (importersToReimport.Count == 0)
            {
                Debug.Log("No bad textures found in project. Textures checked: " + allTexturePaths.Count);
            }
            else
            {
                StringBuilder badTexturesLog = new StringBuilder();
                badTexturesLog.AppendLine("List of modified textures in project that had one of the following:");
                badTexturesLog.AppendLine("    Uncompressed - used RGBA32 or RGB32 or any other uncompressed format (4x more memory)");
                badTexturesLog.AppendLine("    Wrong Format - changed platform formats to defaults DXT1/DXT5 on PC and ASTC6x6 on iOS/Android");
                badTexturesLog.AppendLine("    Readable - was marked as readable (2x more memory)");
                badTexturesLog.AppendLine("    RGB Atlased - we use RGBA in atlases so we end up with one atlas");
                badTexturesLog.AppendLine("Textures changed:");
                foreach (var tex in importersToReimport)
                {
                    badTexturesLog.AppendLine(tex.assetPath);
                }
                Debug.Log(badTexturesLog.ToString());

                var proceed = EditorUtility.DisplayDialog(
                    "Reimoprt textures", 
                    "Found " + importersToReimport.Count + " textures with incorrect info and in need to reimport.\nYou can see the list in the console if you cancel.", 
                    "Reimport", 
                    "Cancel");
                if (!proceed)
                {
                    return;
                }

                EditorProgress.RunWithCancel("Reimporting textures", () =>
                {
                    for (int i = 0; i < importersToReimport.Count; i++)
                    {
                        var importer = importersToReimport[i];
                        using (EditorProgress.PushIteration(importer.assetPath, i, importersToReimport.Count))
                        {
                            importer.SaveAndReimport();
                        }
                    }
                });
            }
        }

        public static bool FixImporterSettings(TextureImporter importer)
        {
            bool changed = false;
            if (FixTextureDefaultCompression(importer))
                changed = true;
            if (FixTexturePlatformFormats(importer))
                changed = true;
            if (FixTextureReadable(importer))
                changed = true;
            if (FixTextureMips(importer))
                changed = true;
            return changed;
        }

        public static bool FixTexturePlatformFormats(TextureImporter importer)
        {
            bool changed = false;
            if (TexturePreProcess.PreProcessTexture_Standalone(importer, true))
                changed = true;
            if (TexturePreProcess.PreProcessTexture_Android(importer, true))
                changed = true;
            if (TexturePreProcess.PreProcessTexture_iOS(importer, true))
                changed = true;
            return changed;
        }

        public static bool FixTextureDefaultCompression(TextureImporter importer)
        {
            bool changed = false;
            if (importer.textureCompression == TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Compressed;
                changed = true;
            }

            return changed;
        }

        public static bool FixTextureReadable(TextureImporter importer)
        {
            bool changed = false;
            if (importer.isReadable)
            {
                importer.isReadable = false;
                changed = true;
            }

            return changed;
        }

        public static bool FixTextureMips(TextureImporter importer)
        {
            bool changed = false;
            if (importer.mipmapEnabled && importer.GetDefaultPlatformTextureSettings().textureCompression == TextureImporterCompression.Uncompressed)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            return changed;
        }
    }
}
