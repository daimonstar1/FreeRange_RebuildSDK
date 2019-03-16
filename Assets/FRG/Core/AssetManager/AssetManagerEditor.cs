using System;
using System.Reflection;
using UnityEngine;

namespace FRG.Core
{
#if UNITY_EDITOR
    /// <summary>
    /// An interface for loading and accessing game data in the editor only. Keeps a separate list.
    /// </summary>
    public class AssetManagerEditor
    {
        private static PropertyInfo cachedInspectorModeInfo;
        private static readonly object inspectorModeArgument = UnityEditor.InspectorMode.Debug;

        /// <summary>
        /// Editor-only: Finds an asset that looks like it should be matched up to the given AssetManagerRef and type.
        /// </summary>
        public static UnityEngine.Object ContextualLoad(AssetManagerRef reference, Type contextualType)
        {
            return ContextualLoadByUniqueId(reference.UniqueId, contextualType);
        }

        public static UnityEngine.Object ContextualLoadByUniqueId(string uniqueId, Type contextualType)
        {
            using (ProfileUtil.PushSample("AssetManagerEditor.ContextualLoad")) {
                contextualType = contextualType ?? typeof(UnityEngine.Object);

                string uuid;
                long fileId;
                if (!TryParseUniqueId(uniqueId, out uuid, out fileId)) {
                    return null;
                }

                if (string.IsNullOrEmpty(uuid)) {
                    return null;
                }

                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(uuid);
                if (string.IsNullOrEmpty(assetPath)) {
                    return null;
                }

                if (fileId == 0) {
                    UnityEngine.Object main = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);
                    if (contextualType.IsInstanceOfType(main)) {
                        return main;
                    }

                    Debug.LogWarning("Main asset of " + uniqueId + " is not the correct contextual type (" + contextualType.ToString() + ").", main);
                    return null;
                }
                else {
                    // Optimization: Try the default asset for type first
                    UnityEngine.Object typed = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, contextualType);
                    if (typed != null && contextualType.IsInstanceOfType(typed)) {
                        if (GetFileIdForAsset(typed) == fileId) {
                            return typed;
                        }
                    }

                    // This may be the wrong call; works for sprites, but we might want AllAssets for subprefabs
                    var representations = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
                    foreach (var asset in representations) {
                        if (contextualType.IsInstanceOfType(asset)) {
                            if (GetFileIdForAsset(asset) == fileId) {
                                return asset;
                            }
                        }
                    }

                    foreach (var asset in representations) {
                        if (contextualType.IsInstanceOfType(asset)) {
                            continue;
                        }
                        if (GetFileIdForAsset(asset) == fileId) {
                            Debug.LogWarning("Subasset of " + uniqueId + " is not the correct contextual type (" + contextualType.ToString() + ").", asset);
                            return null;
                        }
                    }

                    Debug.LogWarning("Could not find any asset matching unique ID " + uniqueId + "; expected contextual type (" + contextualType.ToString() + ").");
                    return null;
                }
            }
        }

        public static bool TryParseUniqueId(string uniqueId, out string uuid, out long fileId)
        {
            uniqueId = uniqueId ?? "";

            uuid = "";
            fileId = 0;

            int shortLength = AssetManagerRef.UniquePrefix.Length + AssetManagerRef.UuidLength;
            if (uniqueId.Length < shortLength) {
                return false;
            }
            string uuidValue = uniqueId.Substring(AssetManagerRef.UniquePrefix.Length, AssetManagerRef.UuidLength);


            if (uniqueId.Length == shortLength) {
                uuid = uuidValue;
                fileId = 0;
                return true;
            }

            if (uniqueId[shortLength] != '+') {
                return false;
            }

            string fileIdStr = uniqueId.Substring(shortLength + 1, uniqueId.Length - (shortLength + 1));

            ulong fileIdValue;
            ulong.TryParse(fileIdStr, out fileIdValue);
            if (fileIdValue == 0) {
                return false;
            }

            uuid = uuidValue;
            fileId = (long)fileIdValue;
            return true;
        }

        public static string GetUniqueIdForAsset(UnityEngine.Object asset)
        {
            if (!UnityEditor.AssetDatabase.Contains(asset)) {
                return "";
            }

            if (asset is Component) { asset = ((Component)asset).gameObject; }

            string path = UnityEditor.AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path)) {
                throw new InvalidOperationException("Cannot create a unique ID for an object that has no asset path: " + Util.GetObjectPath(asset, true));
            }
            string uuid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(uuid)) {
                throw new InvalidOperationException("Cannot create a unique ID for an object with no guid: " + Util.GetObjectPath(asset, true));
            }

            if (uuid.Length != AssetManagerRef.UuidLength) {
                throw new InvalidOperationException("Cannot create a unique ID for an object with invalid UUID (" + uuid + "): " + Util.GetObjectPath(asset, true));
            }

            if (UnityEditor.AssetDatabase.IsMainAsset(asset)) {
                return string.Format("{0}{1}", AssetManagerRef.UniquePrefix, uuid);
            }

            long fileId = GetFileIdForAsset(asset);
            if (fileId <= 0) {
                throw new InvalidOperationException("Cannot create a unique ID for an object with nonpositive file ID (" + fileId + "): " + Util.GetObjectPath(asset, true));
            }

            return string.Format("{0}{1}+{2}", AssetManagerRef.UniquePrefix, uuid, fileId);
        }

        private static long GetFileIdForAsset(UnityEngine.Object asset)
        {
            if (asset == null || !UnityEditor.EditorUtility.IsPersistent(asset)) {
                return 0;
            }

            if (cachedInspectorModeInfo == null) {
                cachedInspectorModeInfo = typeof(UnityEditor.SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(asset);
            cachedInspectorModeInfo.SetValue(serializedObject, inspectorModeArgument, null);
            UnityEditor.SerializedProperty serializedProperty = serializedObject.FindProperty("m_LocalIdentfierInFile");
            return serializedProperty.longValue;
        }
    }
#endif
}

