
using System;
using UnityEditor;
using UnityEngine;

namespace FRG.Core
{
    public class AssetManagerUtil
    {
        public static UnityEngine.Object DeduceAsset(AssetManagerRef reference, bool suppressErr, Type requiredType)
        {
            if (!reference.IsValid) {
                return null;
            }

            UnityEngine.Object asset = AssetManager.TryGet<UnityEngine.Object>(reference);
            if (asset != null) {
                return asset;
            }

            asset = AssetManagerEditor.ContextualLoad(reference, requiredType);

            if (asset != null) {
                AssetManagerRef newRef = CreateRawReference(asset);
                if (!string.Equals(reference.UniqueId, newRef.UniqueId)) {
                    Debug.Assert(false, "Deduced different unique id!");
                    return null;
                }

                Debug.Log("No AssetManagerResource for " + asset.name + " as " + reference, asset);
                CreateResource(reference, asset);
            }
            else if (!suppressErr) {
                Debug.LogError("Unable to find asset: " + reference);
            }
            return asset;
        }

        public static AssetManagerRef ReferenceAsset(UnityEngine.Object asset)
        {
            if (asset == null) {
                return new AssetManagerRef();
            }
            else {
                AssetManagerRef reference = CreateRawReference(asset);
                UnityEngine.Object test = AssetManager.TryGet<UnityEngine.Object>(reference);
                if (test == null) {
                    AssetManagerResource resource = CreateResource(reference, asset);
                    Debug.Log("Created an AssetManagerResource for " + asset.name + " named \"" + reference.UniqueId + "\".", resource);

                    // Sanity check that it saved.
                    AssetManager.Get<UnityEngine.Object>(reference);
                }
                return reference;
            }
        }

        internal static AssetManagerRef CreateRawReference(UnityEngine.Object asset)
        {
            if (asset == null) throw new ArgumentNullException("asset");

            if (!EditorUtility.IsPersistent(asset)) throw new InvalidOperationException("Cannot create an AssetManagerRef for an object not in the asset database: " + Util.GetObjectPath(asset, true));

            return new AssetManagerRef(AssetManagerEditor.GetUniqueIdForAsset(asset));
        }

        private static AssetManagerResource CreateResource(AssetManagerRef reference, UnityEngine.Object asset)
        {
            AssetManagerResource resource = (AssetManagerResource)ScriptableObject.CreateInstance(typeof(AssetManagerResource));
            resource.asset = asset;

            PathUtil.CreateDirectoryRecursively(StandardEditorPaths.AssetManagerResource);
            AssetDatabase.CreateAsset(resource, StandardEditorPaths.AssetManagerResource + reference.UniqueId + ".asset");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ImportRecursive);

            return resource;
        }

        [MenuItem("Assets/FRG/Create AssetManagerResource", priority = 20)]
        private static void CreateAssetManagerResourceForAsset()
        {
            foreach (UnityEngine.Object asset in Selection.objects) {
                if (asset != null) {
                    if (!EditorUtility.IsPersistent(asset)) {
                        Debug.Log("Skipping object because it is not a saved asset: " + Util.GetObjectPath(asset));

                        continue;
                    }

                    AssetManagerRef reference = CreateRawReference(asset);
                    UnityEngine.Object otest = AssetManager.TryGet<UnityEngine.Object>(reference);
                    if (otest == null) {
                        AssetManagerResource resource = CreateResource(reference, asset);
                        Debug.Log("Created an AssetManagerResource for " + asset.name + " named \"" + reference.UniqueId + "\".", resource);
                    }
                }
            }
        }

        [MenuItem("Assets/FRG/Create AssetManagerResource", validate = true)]
        private static bool CheckCreateAssetManagerResourceForAsset()
        {
            foreach (UnityEngine.Object obj in Selection.objects) {
                if (!AssetDatabase.Contains(obj)) {
                    return false;
                }
            }
            return true;
        }


        //[UnityEditor.MenuItem("Assets/Copy Serialized Name to Clipboard")]
        private static void CopyIdToClipboard()
        {
            //var amr = UnityEditor.Selection.activeObject as AssetManagerResource;
            //string name = amr.GetSerializedName();
            string copy = "";
            foreach (var obj in UnityEditor.Selection.objects) {
                AssetManagerResource amr = obj as AssetManagerResource;
                if (copy.Length > 0)
                    copy += Environment.NewLine;
                copy += ReferenceAsset(amr.asset).Serialize();
            }
            UnityEditor.EditorGUIUtility.systemCopyBuffer = copy;
            Debug.Log("Copied " + copy);
        }

        //[UnityEditor.MenuItem("Assets/Copy Serialized Name to Clipboard", validate = true)]
        private static bool CopyIdToClipboardValidate()
        {

            foreach (var obj in UnityEditor.Selection.objects) {
                if (!(obj is AssetManagerResource))
                    return false;
            }
            return true;
            /*
            if (UnityEditor.Selection.activeObject is AssetManagerResource)
                return true;
            return false;
            */
        }
    }
}
