using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace FRG.Core
{
    /// <summary>
    /// File and path utilities.
    /// </summary>
    public static class PathUtil
    {
#if UNITY_EDITOR
        /// <summary>
        /// Filters by certain parameters to change the types of results.
        /// </summary>
        public enum AssetLoadFilter
        {
            /// <summary>
            /// (Default) Use <see cref="UnityEditor.AssetDatabase.LoadAssetAtPath(string, Type)"/> to load only one asset at each matching path.
            /// </summary>
            OnlyFirstOfType,

            /// <summary>
            /// Use <see cref="UnityEditor.AssetDatabase.LoadMainAssetAtPath(string)"/> to only return assets if they are the main asset at each matching path.
            /// </summary>
            OnlyMainAsset,

            /// <summary>
            /// Use <see cref="UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(string)"/> to load every asset visible in the hierarchy at each matching path.
            /// </summary>
            AllVisibleAssets,

            /// <summary>
            /// Use <see cref="UnityEditor.AssetDatabase.LoadAllAssetsAtPath(string)"/> to load every single asset, including hidden subobjects, at each matching path.
            /// </summary>
            AllAssets,
        }
#endif

        /// <summary>
        /// Returns true if the given path has a .prefab extension.
        /// </summary>
        public static bool IsPrefabFile(string path)
        {
            return (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns true if the given path has a .unity extension.
        /// </summary>
        public static bool IsSceneFile(string path)
        {
            return (!string.IsNullOrEmpty(path) && path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns true if the given path has a .asset extension.
        /// </summary>
        public static bool IsAssetFile(string path)
        {
            return (!string.IsNullOrEmpty(path) && path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase));
        }

#if !GAME_SERVER
        /// <summary>
        /// Attempts to be equivalent to <see cref="UnityEditor.EditorUtility.RevealInFinder(string)"/>, but works at runtime.
        /// Only works on some platforms.
        /// </summary>
        public static void RevealInExplorer(string targetPath)
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = false;
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                p.StartInfo.FileName = "explorer.exe";
                targetPath = targetPath.Replace( '/', '\\' );
                p.StartInfo.Arguments = "/select,\"" + targetPath + "\"";
            }
            else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                p.StartInfo.FileName = "open";
                p.StartInfo.Arguments = "-a Finder[\"" + targetPath + "\"]";
            }
            else
            {
                // Application.OpenUrl?
                return;
            }
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            p.StartInfo.CreateNoWindow = false;
            p.Start();
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Finds all assets of the given type.
        /// </summary>
        /// <typeparam name="T">The type of results to return.</typeparam>
        /// <param name="filterString">Additional filtering parameters. (String that would go in the project search box.) For instance, "t:prefab" or "myassetname".</param>
        /// <param name="subtype">
        /// An optional type to further filter by. Must be <typeparamref name="T"/> or a subclass. Defaults to <typeparamref name="T"/>.
        /// Note that <see cref="AssetLoadFilter.AllAssets"/> may not return correct results if the <paramref name="subtype"/> is not a visible asset type.
        /// </param>
        /// <param name="folderPaths">The optional path to search.</param>
        /// <returns>An unsorted sequence of assets of the given type.</returns>
        public static OrderedHashSet<T> FindAssets<T>(string filterString = "", Type subtype = null, AssetLoadFilter loadFilter = AssetLoadFilter.OnlyFirstOfType, string[] folderPaths = null)
            where T : UnityEngine.Object
        {
            OrderedHashSet<T> buffer = new OrderedHashSet<T>();
            FindAssets<T>(buffer, filterString, subtype, loadFilter, folderPaths);
            return buffer;
        }

        /// <summary>
        /// Finds all assets of the given type.
        /// </summary>
        /// <typeparam name="T">The type of results to return.</typeparam>
        /// <param name="buffer">The buffer to add to. Collection is not cleared before adding.</param>
        /// <param name="filterString">Additional filtering parameters. (String that would go in the project search box.) For instance, "t:prefab" or "myassetname".</param>
        /// <param name="subtype">
        /// An optional type to further filter by. Must be <typeparamref name="T"/> or a subclass. Defaults to <typeparamref name="T"/>.
        /// Note that <see cref="AssetLoadFilter.AllAssets"/> may not return correct results if the <paramref name="subtype"/> is not a visible asset type.
        /// </param>
        /// <param name="folderPaths">The optional path to search.</param>
        /// <returns>An unsorted sequence of assets of the given type.</returns>
        public static void FindAssets<T>(ICollection<T> buffer, string filterString = "", Type subtype = null, AssetLoadFilter loadFilter = AssetLoadFilter.OnlyFirstOfType, string[] folderPaths = null)
            where T : UnityEngine.Object
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }

            if (subtype != null && !typeof(T).IsAssignableFrom(subtype))
            {
                throw new ArgumentException("Cannot convert " + subtype.CSharpFullName() + " to " + typeof(T).CSharpFullName() + ".", "assetType");
            }

            subtype = subtype ?? typeof(T);

            // NOTE: subtype might interfere with AllAssets
            string[] paths = FindRawAssetPaths(filterString, subtype, folderPaths);
            Array.Sort(paths, StringComparer.Ordinal);
            if (buffer is ICapacity)
            {
                ((ICapacity)buffer).EnsureCapacity(paths.Length);
            }

            Action<ICollection<T>, Type, string> operation = FindStatics<T>.Callbacks[loadFilter];
            int i = 0;

            bool shouldShowEditorLoadingBar = false;
#if UNITY_EDITOR
            shouldShowEditorLoadingBar = subtype == typeof(ManagedEntry) && UnityEditor.EditorPrefs.GetBool("showLoadingBarForLoadingAllManagedEntries", false);
#endif
            foreach (string path in paths)
            {
#if UNITY_EDITOR
                if (shouldShowEditorLoadingBar && Application.isPlaying)
                {
                    float progress = ((float)i) / paths.Length;
                    i++;
                    if(UnityEditor.EditorUtility.DisplayCancelableProgressBar("Loading All Managed Entries", path, progress))
                    {
                        UnityEditor.EditorApplication.isPlaying = false;
                        UnityEditor.EditorUtility.ClearProgressBar();
                        return;
                    }
                }
#endif
                if (!string.IsNullOrEmpty(path))
                {
                    operation(buffer, subtype, path);
                }
            }
#if UNITY_EDITOR
            if(shouldShowEditorLoadingBar)
                UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }

        /// <summary>
        /// Finds all asset paths matching the specified filter.
        /// </summary>
        /// <param name="filterString">A string appropriate for AssetDatabase.FindAssets.</param>
        /// <param name="assetType">An optional assetType to filter by.</param>
        /// <param name="folderPaths">The optional folder to search; defaults to Assets/.</param>
        /// <returns>An array of paths, sorted alphabetically, with no null or empty entries.</returns>
        public static OrderedHashSet<string> FindAssetPaths(string filterString = "", Type assetType = null, string[] folderPaths = null)
        {
            OrderedHashSet<string> paths = new OrderedHashSet<string>(FindRawAssetPaths(filterString, assetType, folderPaths), StringComparer.Ordinal);
            paths.Remove(null);
            paths.Remove("");
            paths.StableSort(NaturalCompareOrdinal);
            return paths;
        }

        /// <summary>
        /// Finds all asset paths matching the specified filter, returning nulls.
        /// </summary>
        /// <param name="filterString">A string appropriate for AssetDatabase.FindAssets.</param>
        /// <param name="assetType">A type to search for. (Just gets added to the filter string.)</param>
        /// <param name="folderPaths">The optional folder to search; defaults to Assets/.</param>
        /// <returns>An array of paths. It will be unsorted and with possibly-null entries.</returns>
        public static string[] FindRawAssetPaths(string filterString = "", Type assetType = null, string[] folderPaths = null)
        {
            string[] found = FindRawAssetUuids(filterString, assetType, folderPaths);
            for (int i = 0; i < found.Length; ++i)
            {
                found[i] = UnityEditor.AssetDatabase.GUIDToAssetPath(found[i]);
            }
            return found;
        }

        /// <summary>
        /// Finds all asset guids matching the specified filter, returning nulls.
        /// </summary>
        /// <param name="filterString">A string appropriate for AssetDatabase.FindAssets.</param>
        /// <param name="assetType">A type to search for. (Just gets added to the filter string.)</param>
        /// <param name="folderPaths">The optional folder to search; defaults to Assets/.</param>
        /// <returns>An array of uuids. It will be unsorted and with possibly-null entries.</returns>
        public static string[] FindRawAssetUuids(string filterString = "", Type assetType = null, string[] folderPaths = null)
        {
            if (string.IsNullOrEmpty(filterString) && assetType == null && (folderPaths == null || folderPaths.Length == 0))
            {
                throw new ArgumentException("Will not return GUIDs of every single asset in the game.");
            }

            filterString = filterString ?? "";
            folderPaths = folderPaths ?? ArrayUtil.Empty<string>();

            if (assetType != null)
            {
                if (string.IsNullOrEmpty(filterString))
                {
                    filterString = FindStatics.GetFilterString(assetType);
                }
                else if (!filterString.Contains("t:"))
                {
                    filterString += FindStatics.GetFilterString(assetType);
                }
            }
            
            filterString = filterString.Trim();

            string[] trimmedFolders = ArrayUtil.Empty<string>();
            if (folderPaths.Length != 0)
            {
                trimmedFolders = new string[folderPaths.Length];
                for (int i = 0; i < folderPaths.Length; ++i)
                {
                    trimmedFolders[i] = StandardEditorPaths.GetRelativePath(folderPaths[i]).TrimEnd('/');
                }
            }

            return UnityEditor.AssetDatabase.FindAssets(filterString, trimmedFolders);
        }
#endif

        /// <summary>
        /// Sort, considering latin numerals. ie "Data 10" after "Data 2".
        /// Use this for filesystem operations and internal strings.
        /// </summary>
        public static int NaturalCompareOrdinal(string a, string b)
        {
            return NaturalCompare(a, b, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sort, considering latin numerals. ie "Data 10" after "Data 2".
        /// Usually when sorting strings you should use this of the normal sort.
        /// </summary>
        public static int NaturalCompare(string a, string b, CultureInfo cultureInfo)
        {
            return NaturalCompare(a, b, cultureInfo, CompareOptions.IgnoreCase);
        }

        /// <summary>
        /// Sort, considering latin numerals. ie "Data 10" after "Data 2".
        /// Usually when sorting user-facing strings you should use this instead of the normal sort.
        /// </summary>
        public static int NaturalCompare(string a, string b, CultureInfo culture, CompareOptions options)
        {
            if (ReferenceEquals(a, null)) return (ReferenceEquals(b, null) ? 0 : -1);
            if (ReferenceEquals(b, null)) return 1;

            CompareInfo compareInfo = culture.CompareInfo;

            int lengthA = a.Length;
            int lengthB = b.Length;

            int indexA = 0;
            int indexB = 0;

            int endA = indexA;
            int endB = indexB;

            while (indexA < lengthA && indexB < lengthB)
            {

                if (BitUtil.IsAsciiDigit(a, endA) && BitUtil.IsAsciiDigit(b, endB))
                {

                    endA += 1;
                    endB += 1;

                    // Often used for filesystem stuff, so don't allow a full stop or others as separator.
                    // TODO: Allow culture-specific number separators when CultureInfo is not invariant?
                    while (endA < lengthA && (BitUtil.IsAsciiDigit(a, endA) || a[endA] == ',')) endA += 1;
                    while (endB < lengthB && (BitUtil.IsAsciiDigit(b, endB) || b[endB] == ',')) endB += 1;
                    // Trailing separators will be caught later

                    int? numberValueA = BitUtil.ParseIntFast(a, indexA, endA - indexA);
                    int? numberValueB = BitUtil.ParseIntFast(b, indexB, endB - indexB);
                    Debug.Assert(numberValueA.HasValue);
                    Debug.Assert(numberValueB.HasValue);
                    int compareNumbers = numberValueA.Value - numberValueB.Value;
                    if (compareNumbers != 0) return compareNumbers;

                    // Need to check case where numbers are same but strings are different, eg "02" vs "2"
                }
                else
                {
                    while (endA < lengthA && !BitUtil.IsAsciiDigit(a, endA)) endA = BitUtil.NextCodePoint(a, endA, lengthA);
                    while (endB < lengthB && !BitUtil.IsAsciiDigit(b, endB)) endB = BitUtil.NextCodePoint(b, endB, lengthB);
                }

                int compareStrings = SafeLocalizedCompare(a, indexA, endA - indexA, b, indexB, endB - indexB, compareInfo, options);
                if (compareStrings != 0) return compareStrings;

                indexA = endA;
                indexB = endB;
            }

            // Lengths might be different; let the system figure that out
            return SafeLocalizedCompare(a, indexA, lengthA - indexA, b, indexB, lengthB - indexB, compareInfo, options);
        }

        /// <summary>
        /// Double checks a localized compare because sometimes they are not antisymmetric.
        /// </summary>
        private static int SafeLocalizedCompare(string a, int indexA, int lengthA, string b, int indexB, int lengthB, CompareInfo compareInfo, CompareOptions options)
        {
            int compare = compareInfo.Compare(a, indexA, lengthA, b, indexB, lengthB, options);
            int reverseCompare = compareInfo.Compare(b, indexB, lengthB, a, indexA, lengthA, options);

            if (compare == -reverseCompare || Math.Sign(compare) == -Math.Sign(reverseCompare))
            {
                return compare;
            }

            // Fall back to equality
            return 0;
        }

#if UNITY_EDITOR
        public static bool IsInAssetFolder(string assetPath, bool allowAssetsFolder = false)
        {
            StringComparison comparison = StringComparison.OrdinalIgnoreCase;
            if (assetPath.StartsWith(StandardEditorPaths.Assets, comparison) && (allowAssetsFolder || !string.Equals(assetPath, "Assets/", comparison)))
            {
                return true;
            }

            string absolute = StandardEditorPaths.GetAbsolutePath(StandardEditorPaths.Assets);
            if (assetPath.StartsWith(absolute, comparison) && (allowAssetsFolder || !string.Equals(assetPath, absolute, comparison)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Works even if the file does not yet exist.
        /// </summary>
        public static string MetaFilePath(string assetPath)
        {
            return assetPath.TrimEnd(Statics.PathEndChars) + ".meta";
        }

        private static bool CreateMetafile(string assetPath, string template, bool isFolder)
        {
            if (!IsInAssetFolder(assetPath)) {
                return false;
            }

            string metaFilePath = MetaFilePath(assetPath);
            if (File.Exists(metaFilePath)) {
                return false;
            }

            string guid = CreateDeterministicUuid(metaFilePath, isFolder);
            string body = string.Format(template, guid);
            WriteTextFile(metaFilePath, body);
            return true;
        }

        private static string CreateDeterministicUuid(string metaFilePath, bool isFolder)
        {
            // Legacy.
            string folderAsset = isFolder ? "yes" : "no";
            string guid = BitUtil.CalculateMD5(metaFilePath + "$" + folderAsset);
            return guid;
        }

        public static bool CreateFolderMetafile(string assetPath)
        {
            const string template = @"fileFormatVersion: 2
guid: {0}
folderAsset: yes
timeCreated: 1428537765
licenseType: Pro
DefaultImporter:
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";

            return CreateMetafile(assetPath, template, true);
        }

        public static bool CreateScriptMetaFile(string assetPath)
        {
            const string template = @"fileFormatVersion: 2
guid: {0}
timeCreated: 1428537765
licenseType: Pro
MonoImporter:
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";

            return CreateMetafile(assetPath, template, false);
        }

        public static bool EnforceDeterministicMetafileUuid(string assetPath, bool isFolder)
        {
            string metaFilePath = MetaFilePath(assetPath);
            if (!File.Exists(metaFilePath)) {
                return false;
            }

            string uuid = CreateDeterministicUuid(metaFilePath, isFolder);

            string contents = "";
            if (File.Exists(metaFilePath)) {
                contents = File.ReadAllText(metaFilePath);
            }
            contents = System.Text.RegularExpressions.Regex.Replace(contents, @"(?<=guid:\s*)[0-9A-Za-z]+", uuid);

            return WriteTextFile(metaFilePath, contents);
        }

        public static bool CreateGizmoTextureMetafile(string assetPath)
        {
            const string template = @"fileFormatVersion: 2
guid: {0}
timeCreated: 1428537765
licenseType: Pro
TextureImporter:
  fileIDToRecycleName: {{}}
  serializedVersion: 2
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    linearTexture: 1
    correctGamma: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
  isReadable: 0
  grayScaleToAlpha: 0
  generateCubemap: 0
  cubemapConvolution: 0
  cubemapConvolutionSteps: 4
  cubemapConvolutionExponent: 1.5
  seamlessCubemap: 0
  textureFormat: -3
  maxTextureSize: 128
  textureSettings:
    filterMode: 1
    aniso: 1
    mipBias: -1
    wrapMode: 1
  nPOTScale: 0
  lightmap: 0
  rGBM: 0
  compressionQuality: 50
  allowsAlphaSplitting: 0
  spriteMode: 0
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spritePixelsToUnits: 100
  alphaIsTransparency: 1
  textureType: 2
  buildTargetSettings: []
  spriteSheet:
    sprites: []
    outline: []
  spritePackingTag: 
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";

            return CreateMetafile(assetPath, template, false);
        }

#endif
        
        /// <summary>
        /// Creates a directory recursively at the given path.
        /// </summary>
        public static void CreateDirectoryRecursively(string path)
        {
            Directory.CreateDirectory(path);
#if UNITY_EDITOR
            while (!string.IsNullOrEmpty(path) && IsInAssetFolder(path))
            {
                CreateFolderMetafile(path);
                string oldPath = path;
                path = Path.GetDirectoryName(path);
                if (oldPath == path) break;
            }
#endif
        }

        public static void DeleteFileOrFolder(string path)
        {
#if UNITY_EDITOR
            if (IsInAssetFolder(path))
            {
                UnityEditor.AssetDatabase.MoveAssetToTrash(StandardEditorPaths.GetRelativePath(path).TrimEnd('/'));

                string metaPath = MetaFilePath(path);
                if (File.Exists(metaPath))
                {
                    File.Delete(metaPath);
                }
            }
#endif
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Creates or clears a folder, as needed, never actually destroying it.
        /// </summary>
        /// <param name="path">The path of the directory, relative or absolute.</param>
        public static void EnsureEmptyFolder(string path)
        {
#if UNITY_EDITOR
            if (IsInAssetFolder(path, true))
            {
                string folderPath = StandardEditorPaths.GetRelativePath(path).TrimEnd('/');
                if (UnityEditor.AssetDatabase.IsValidFolder(folderPath))
                {
                    foreach (string subfolder in UnityEditor.AssetDatabase.GetSubFolders(folderPath))
                    {
                        if (UnityEditor.AssetDatabase.IsValidFolder(subfolder) && !string.Equals(subfolder, folderPath, StringComparison.OrdinalIgnoreCase))
                        {
                            UnityEditor.AssetDatabase.DeleteAsset(subfolder);
                        }
                    }
                    foreach (string assetPath in FindRawAssetPaths("", null, new string[] { folderPath }))
                    {
                        UnityEditor.AssetDatabase.DeleteAsset(assetPath);
                    }
                }
            }
#endif
            if (Directory.Exists(path))
            {
                DirectoryInfo buildFolder = new DirectoryInfo(path);
                foreach (DirectoryInfo dir in buildFolder.GetDirectories())
                {
                    dir.Delete(true);
                }
                foreach (FileInfo file in buildFolder.GetFiles())
                {
                    file.Delete();
                }
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
            CreateDirectoryRecursively(path);
        }

        /// <summary>
        /// Gets a path relative to the given folder.
        /// </summary>
        /// <param name="fullPath">The complete pathname to make relative.</param>
        /// <param name="folder">The folder to make relative to.</param>
        /// <param name="usePortablePath">True to make the directory separators portable (/).</param>
        /// <returns>A path relative to the given folder. It may contain /../ entries.</returns>
        public static string GetRelativePath(string fullPath, string folder, bool usePortablePath)
        {
            if (!Path.IsPathRooted(fullPath))
            {
                throw new ArgumentException("File path should be absolute.", "fullPath");
            }
            if (!Path.IsPathRooted(folder))
            {
                throw new ArgumentException("Folder path should be absolute.", "folder");
            }

            Uri pathUri = new Uri(fullPath);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            string relativePath = Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
            if (usePortablePath)
            {
                relativePath = relativePath.Replace('\\', '/');
            }
            return relativePath;
        }

        /// <summary>
        /// Gets all filenames (skipping directories) below the root path.
        /// </summary>
        /// <param name="fileSearchPattern">A wildcard search path, like *.txt</param>
        /// <param name="rootFolderPath">The folder to search under.</param>
        /// <param name="usePortablePath">True to make the directory separators portable (/).</param>
        /// <param name="ignoreDirs">Optional list or array of directory names to ignore. Wildcards not currently supported.</param>
        /// <returns>
        /// An enumerable collection of file paths. Created as a generator to space out file accesses.
        /// All returned strings are prefixed by <paramref name="rootFolderPath"/>.
        /// </returns>
        public static IEnumerable<string> FindFiles(string fileSearchPattern, string rootFolderPath, bool usePortablePath, IList<string> ignoreDirs = null)
        {
            Queue<string> pending = new Queue<string>();
            pending.Enqueue(rootFolderPath);
            string[] currentFiles;
            while (pending.Count > 0)
            {
                rootFolderPath = pending.Dequeue();
                currentFiles = Directory.GetFiles(rootFolderPath, fileSearchPattern);
                for (int i = 0; i < currentFiles.Length; i++)
                {
                    string current = currentFiles[i];
                    if (usePortablePath)
                    {
                        current = current.Replace('\\', '/');
                    }
                    yield return current;
                }
                currentFiles = Directory.GetDirectories(rootFolderPath);
                for (int i = 0; i < currentFiles.Length; i++)
                {
                    if (ignoreDirs == null || !ignoreDirs.Contains(currentFiles[i]))
                    {
                        pending.Enqueue(currentFiles[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Writes the specified text to the specified file.
        /// </summary>
        /// <param name="path">Path relative to project root.</param>
        /// <param name="outString">The text to write.</param>
        /// <returns>true if written, else false.</returns>
        public static bool WriteTextFile(string path, string outString)
        {
            using (Pooled<MemoryStream> pooledMemoryStream = RecyclingPool.SpawnMemoryStream(Encoding.UTF8.GetMaxByteCount(outString.Length))) {
                var memoryStream = pooledMemoryStream.Value;

                using (Pooled<StreamWriter> pooledWriter = RecyclingPool.SpawnStreamWriter(memoryStream)) {
                    var writer = pooledWriter.Value;

                    int length = outString.Length;
                    for (int i = 0; i < length; ++i) {
                        char c = outString[i];
                        switch (c) {
                            case '\r':
                                break;
                            case '\n':
                                writer.Write("\r\n");
                                break;
                            case '\t':
                                writer.Write("    ");
                                break;
                            default:
                                writer.Write(c);
                                break;
                        }
                    }
                }

                WriteBinaryFile(path, memoryStream.GetBuffer(), (int)memoryStream.Length);
            }

            return true;
        }

        /// <summary>
        /// Writes the specified text to the specified file.
        /// </summary>
        /// <param name="path">Path relative to project root.</param>
        /// <param name="bytes">The bytes to write.</param>
        /// <returns>true if written, else false.</returns>
        public static bool WriteBinaryFile(string path, byte[] bytes)
        {
            return WriteBinaryFile(path, bytes, bytes.Length);
        }

        public static bool WriteBinaryFile(string path, byte[] bytes, int length)
        {
            if (File.Exists(path)) {
                using (FileStream fileStream = File.OpenRead(path)) {
                    if (length == fileStream.Length) {
                        using (Pooled<MemoryStream> pooledMemoryStream = RecyclingPool.SpawnMemoryStream(length)) {
                            var memoryStream = pooledMemoryStream.Value;

                            byte[] buffer = memoryStream.GetBuffer();
                            int index = 0;
                            while (index < length) {
                                int readResult = fileStream.Read(buffer, index, length - index);
                                if (readResult == 0) {
                                    throw new IOException("Unexpected end of stream");
                                }
                                index += readResult;
                            }

                            if (BitUtil.ByteArrayEqual(bytes, buffer, length)) {
                                return false;
                            }
                        }
                    }
                }
            }

            using (Stream stream = File.Create(path)) {
                stream.Write(bytes, 0, length);
            }
            return true;
        }

        private static class Statics
        {
            public static char[] PathEndChars = new char[] {'/', '\\'};
        }

        private static class FindStatics
        {
            public static readonly Dictionary<Type, string> FilterStrings = new Dictionary<Type, string>();

            public static string GetFilterString(Type type)
            {
                string filterString;
                if (!FilterStrings.TryGetValue(type, out filterString))
                {
                    if (ReflectionUtil.IsBuiltInAssembly(type.Assembly))
                    {
                        filterString = " t:" + type.FullName + " t: " + type.Name;
                    }
                    else
                    {
                        filterString = " t:" + type.FullName;
                    }
                    FilterStrings.Add(type, filterString);
                }
                return filterString ?? "";
            }
        }

#if UNITY_EDITOR
        private static class FindStatics<T>
            where T : UnityEngine.Object
        {
            public static readonly Dictionary<AssetLoadFilter, Action<ICollection<T>, Type, string>> Callbacks =
                new Dictionary<AssetLoadFilter, Action<ICollection<T>, Type, string>>
                {
                    { AssetLoadFilter.OnlyFirstOfType, AddOnlyFirstOfType },
                    { AssetLoadFilter.OnlyMainAsset, AddOnlyMainAsset },
                    { AssetLoadFilter.AllVisibleAssets, AddAllVisibleAssets },
                    { AssetLoadFilter.AllAssets, AddAllAssets },
                };

            private static void AddOnlyFirstOfType(ICollection<T> set, Type subtype, string path)
            {
                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(path, subtype);
                AddAssetChecked(set, subtype, obj);
            }

            private static void AddOnlyMainAsset(ICollection<T> set, Type subtype, string path)
            {
                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path);
                AddAssetChecked(set, subtype, obj);
            }

            private static void AddAllVisibleAssets(ICollection<T> set, Type subtype, string path)
            {
                foreach (UnityEngine.Object obj in UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                {
                    AddAssetChecked(set, subtype, obj);
                }
            }

            private static void AddAllAssets(ICollection<T> set, Type subtype, string path)
            {
                foreach (UnityEngine.Object obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    AddAssetChecked(set, subtype, obj);
                }
            }

            private static void AddAssetChecked(ICollection<T> set, Type subtype, UnityEngine.Object obj)
            {
                if (obj != null && subtype.IsInstanceOfType(obj))
                {
                    T castObj = (T)obj;
                    if (!set.Contains(castObj))
                    {
                        set.Add(castObj);
                    }
                }
            }
        }
#endif
    }
}