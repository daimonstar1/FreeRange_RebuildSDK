using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FRG.Core
{
    public class ProjectTextureSettingsEditor : EditorWindow
    {
        // editor options
        [SerializeField] Vector2 scrollPosition = Vector2.zero;
        [SerializeField] Vector2 scrollPositionOptions = Vector2.zero;
        [SerializeField] List<string> badTexturePaths = new List<string>();
        [SerializeField] ProjectTextureSettings.TextureChangeOptions[] changeOptions = ProjectTextureSettings.GetDefaultOptions();
        [SerializeField] GUIStyle style_bigButton;
        [SerializeField] string selectedTexturePath;

        [MenuItem("FRG/Editor/Project Texture Settings")]
        static void Init()
        {
            ProjectTextureSettingsEditor window = EditorWindow.GetWindow<ProjectTextureSettingsEditor>(true, "Project Texture Settings", true);
            window.minSize = new Vector2(844, 500);
        }

        //[SerializeField] string tempText;
        void OnGUI()
        {
            //tempText = EditorGUILayout.TextArea(tempText, GUILayout.Height(300));
            //if (GUILayout.Button("Select Paths"))
            //{
            //    string[] lines = tempText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            //    List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
            //    foreach (var line in lines)
            //    {
            //        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(line);
            //        if (tex != null)
            //        {
            //            assets.Add(tex);
            //        }
            //        else
            //        {
            //            Debug.LogWarning("Skipping: " + line);
            //        }
            //    }
            //    Selection.objects = assets.ToArray();
            //}

            GUILayout.Space(10);

            // using a custom GUIStyle to center just made the text black so use this instead
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("This is a utility to go through all the textures in the project and find the ones that don't match our settings for given platform.");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Then you have the option to inspect them and fix them.");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            scrollPositionOptions = GUILayout.BeginScrollView(scrollPositionOptions, GUILayout.MaxWidth(600));

            var obj = new SerializedObject(this);
            obj.Update();
            var prop = obj.FindProperty(nameof(changeOptions));
            EditorGUILayout.PropertyField(prop);
            obj.ApplyModifiedProperties();

            GUILayout.EndScrollView();


            if (style_bigButton == null)
            {
                style_bigButton = new GUIStyle("button");
                style_bigButton.fontSize = 16;
                style_bigButton.fontStyle = FontStyle.Bold;
                style_bigButton.fixedWidth = 300;
                style_bigButton.fixedHeight = 28;
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset Config", style_bigButton))
            {
                changeOptions = ProjectTextureSettings.GetDefaultOptions();
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button("Check Project Textures", style_bigButton))
            {
                badTexturePaths.Clear();
                badTexturePaths = FindBadTextures(changeOptions);
                GUIUtility.ExitGUI();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            if (badTexturePaths.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Total of " + badTexturePaths.Count + " textures found");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                DrawListOfTextures(badTexturePaths);
                GUILayout.Space(10);
                DrawFixAllButton();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawListOfTextures(List<string> badTexturePaths)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.MaxHeight(1000));

            int count = 0;
            foreach (var texPath in badTexturePaths)
            {
                count += 1;
                if (count > 100) { break; }
                DrawTextureRow(texPath);
            }

            GUILayout.EndScrollView();
        }

        private void DrawTextureRow(string texPath)
        {
            var texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (texImporter == null) return;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(texPath, GUILayout.ExpandWidth(true)))
            {
                var asset = AssetDatabase.LoadAssetAtPath(texPath, typeof(UnityEngine.Object));
                Selection.objects = new UnityEngine.Object[] { asset };
                selectedTexturePath = texPath;
                GUIUtility.ExitGUI();
            }

            //GUILayout.Label(texImporter.assetPath, GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();

            // if selected, show fix button
            if (texPath == selectedTexturePath)
            {
                GUILayout.Space(10);

                if (GUILayout.Button("Fix and reimport", GUILayout.Width(150)))
                {
                    ProjectTextureSettings.SetTextureSettings(texPath, changeOptions);
                    GUIUtility.ExitGUI();
                }

                GUILayout.Space(10);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawFixAllButton()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Fix and reimport ALL", style_bigButton))
            {
                bool confirm = true;
                if (badTexturePaths.Count > 10)
                {
                    confirm = EditorUtility.DisplayDialog("Are you sure?", "Reimporting large number of textures takes a very long time. Are you sure?", "OK", "Cancel");
                }

                if (confirm)
                {
                    AssetDatabase.StartAssetEditing();
                    try
                    {
                        //badTexturePaths.Clear();
                        for (int i = 0; i < badTexturePaths.Count; i++)
                        {
                            string texPath = badTexturePaths[i];
                            string title = "Changing settings for project textures [" + i + "/" + badTexturePaths.Count + "] (will reimport after)";
                            float progress = i / (float)badTexturePaths.Count;
                            bool isCanceled = EditorUtility.DisplayCancelableProgressBar(title, texPath, progress);
                            if (isCanceled)
                            {
                                break;
                            }
                            ProjectTextureSettings.SetTextureSettings(texPath, changeOptions);
                        }
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                        AssetDatabase.StopAssetEditing();
                        AssetDatabase.Refresh();
                        AssetDatabase.SaveAssets();
                    }
                }
                GUIUtility.ExitGUI();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Warning: Reimporting a large number of textues is a very slow process.");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static List<string> FindBadTextures(ProjectTextureSettings.TextureChangeOptions[] changeOptions)
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
            GC.Collect();

            var allTextureGuids = AssetDatabase.FindAssets("t:Texture");
            var allTexturePaths = GetTexturePathsFromGuids(allTextureGuids);
            var badTexturePaths = GetIncorrectTexturePaths(allTexturePaths, changeOptions);
            return badTexturePaths;
        }

        public static List<string> GetTexturePathsFromGuids(string[] allTextureGuids)
        {
            List<string> allTexturePaths = new List<string>();
            foreach (var texGuid in allTextureGuids)
            {
                string texPath = AssetDatabase.GUIDToAssetPath(texGuid);
                allTexturePaths.Add(texPath);
            }
            return allTexturePaths;
        }

        public static List<string> GetIncorrectTexturePaths(List<string> allTexturePaths, ProjectTextureSettings.TextureChangeOptions[] changeOptions) {
            ProjectTextureSettings.npotLog.Length = 0;
            ProjectTextureSettings.crunchLog.Length = 0;
            ProjectTextureSettings.mipmapLog.Length = 0;
            ProjectTextureSettings.etcLog.Length = 0;
            ProjectTextureSettings.spriteCounts.Clear();

            var badTexturePaths = new List<string>();

            try
            {
                for (int i = 0; i < allTexturePaths.Count; i++)
                {
                    if (i % 1000 == 0)
                    {
                        GC.Collect();
                        EditorUtility.UnloadUnusedAssetsImmediate();
                    }

                    string texPath = allTexturePaths[i];
                    float progress = i / (float)allTexturePaths.Count;
                    string title = "Checking project textures [" + i + "/" + allTexturePaths.Count + "]";
                    bool isCanceled = EditorUtility.DisplayCancelableProgressBar(title, texPath, progress);
                    if (isCanceled)
                    {
                        break;
                    }

                    bool isCorrect = ProjectTextureSettings.IsTextureSettingsCorrect(texPath, changeOptions);
                    if (!isCorrect)
                    {
                        badTexturePaths.Add(texPath);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Log("Textures that have NPOT set to something other than None:\n", ProjectTextureSettings.npotLog);
            Log("Textures that cannot be compressed due to mipmap and NPOT:\n", ProjectTextureSettings.mipmapLog);
            Log("Textures that cannot be crunched due to not being a multiple of 4:\n", ProjectTextureSettings.crunchLog);
            Log("Textures that cannot be crunched due to not being NPOT with no alpha:\n", ProjectTextureSettings.etcLog);

            string sprites = "Sprite tags:\n";
            foreach (var entry in ProjectTextureSettings.spriteCounts)
            {
                sprites += entry.Key + ": " + entry.Value + "\n";
            }
            Debug.Log(sprites);

            return badTexturePaths;
        }

        private static void Log(string preamble, System.Text.StringBuilder log) {
            if (log.Length > 0) { Debug.LogWarning(preamble + log.ToString()); }
        }
    }
}