using System.IO;
using UnityEngine;

namespace FRG.Core
{
#if UNITY_EDITOR
    /// <summary>
    /// Common project-relative paths. Any constants that start with "Assets/" should be in here. All paths have a trailing slash.
    /// </summary>
    public static class StandardEditorPaths
    {
        private static string cachedProjectPath = null;
        private static string CachedProjectPath
        {
            get
            {
                if (cachedProjectPath == null)
                {
                    //GetDirectoryName will strip the last slash whether it be file or directory
                    cachedProjectPath = Path.GetDirectoryName(Application.dataPath) + "/";
                }
                return cachedProjectPath;
            }
        }

        /// <summary>
        /// The path relative to the project folder. (Parent of the Assets/ folder)
        /// </summary>
        public static string RootPath
        {
            get
            {
                return CachedProjectPath;
            }
        }

        /// <summary>
        /// Turns an absolute path into a project-relative path.
        /// Relative paths are unchanged.
        /// </summary>
        public static string GetRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            if (Path.IsPathRooted(path))
            {
                return PathUtil.GetRelativePath(path, CachedProjectPath, true);
            }
            else if (path.Contains("\\"))
            {
                return path.Replace('\\', '/');
            }
            else {
                return path;
            }
        }

        /// <summary>
        /// Turns a project-relative path into an absolute path.
        /// Absolute paths are unchanged.
        /// </summary>
        public static string GetAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return CachedProjectPath;
            }

            if (Path.IsPathRooted(path))
            {
                return path;
            }
            else
            {
                return Path.Combine(CachedProjectPath, path);
            }
        }

        /// <summary>
        /// The path to the assets folder off of the project folder.
        /// </summary>
        public const string Assets = "Assets/";

        /// <summary>
        /// The path to the Temp folder Unity uses, off of the project folder.
        /// </summary>
        public const string ProjectTemp = "Temp/";

        /// <summary>
        /// The temporary folder inside Assets/ used when building.
        /// </summary>
        public const string AssetsTemp = Assets + "Temp/";

        /// <summary>
        /// The folder for non-project assets.
        /// </summary>
        public const string Work = "../Work/";

        /// <summary>
        /// Location of project-specific data related to core classes.
        /// </summary>
        public const string CoreData = Assets + "CoreData/";

        /// <summary>
        /// Location of built-in configuration files.
        /// </summary>
        public const string CoreDataConfiguration = CoreData + "Configuration/";

        /// <summary>
        /// Location of built-in editor-only resources.
        /// </summary>
        public const string CoreDataEditor = CoreData + "Editor/";

        /// <summary>
        /// Location of built-in resources.
        /// </summary>
        public const string CoreDataResources = CoreData + "Resources/";

        /// <summary>
        /// Default location of managed entries.
        /// </summary>
        public const string ManagedData = CoreData + "ManagedData/";

        /// <summary>
        /// Location of AssetManagerResource files.
        /// </summary>
        public const string AssetManagerResource = CoreData + "AssetManagerResource/Resources/";

        /// <summary>
        /// Location of all generated files. Everything in here should be able to come back if deleted.
        /// </summary>
        public const string Generated = Assets + "_Generated/";

        /// <summary>
        /// Location of all generated shared files. Everything in here should be able to come back if deleted.
        /// </summary>
        public const string GeneratedShared = Generated + "Shared/";

        /// <summary>
        /// Location of all generated core networking files. Everything in here should be able to come back if deleted.
        /// </summary>
        public const string GeneratedNetworking = Generated + "Networking/";

        /// <summary>
        /// Location of all generated core networking files. Everything in here should be able to come back if deleted.
        /// </summary>
        public const string Gizmos = Assets + "Gizmos/";

        public const string Migration = Assets + "Migration/";

        /// <summary>
        /// Location of all icons tied to <see cref="ScriptableObject"/>s or <see cref="MonoBehaviour"/>s.
        /// Need to have file name "[type] Icon" (and any <see cref="Texture2D"/> extension).
        /// </summary>
        public const string AssetIconFormatString = Gizmos + "{0} Icon.png";

        public const string GeneratedSingleUnityWrapper = Generated + "UnityWrapper/";
        public const string GeneratedCollectionUnityWrapper = GeneratedSingleUnityWrapper;

        private const string Frg = Assets + "FRG/";
        private const string FrgCore = Frg + "Core/";
        private const string FrgNetworking = Frg + "Networking/";

        private const string SharedAssetBaseIcons = FrgCore + "ManagedEntry/Editor/Icons/";
        public const string SharedAssetDefaultIcon = SharedAssetBaseIcons + "SharedSingletonIcon.png";
        public const string ManagedEntryDefaultIcon = SharedAssetBaseIcons + "ManagedEntryIcon.png";

        private const string EnhancedInspectorBaseResources = FrgCore + "EnhancedInspector/Editor/";
        public const string EnhancedInspectorLightResources = EnhancedInspectorBaseResources + "LightTheme/";
        public const string EnhancedInspectorDarkResources = EnhancedInspectorBaseResources + "DarkTheme/";
    }
#endif
}

