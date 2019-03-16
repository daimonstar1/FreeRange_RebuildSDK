using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FRG.Core
{
    /// <summary>
    /// Specifies that a service could not be located because it was invalid to even look for the service.
    /// </summary>
    public class ServiceNotFoundException : InvalidOperationException
    {
        public ServiceNotFoundException() { }
        public ServiceNotFoundException(string message) : base(message) { }
        public ServiceNotFoundException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// An object that resolves references to other classes and manages their lifetimes.
    /// </summary>
    public static class ServiceLocator
    {
        private class ServiceCache<T>
            where T : class
        {
            public static T CachedValue = null;
            public static bool IsCreating = false;
        }

        public const string RuntimeRootGroupName = "Singletons";

        private static GameObject runtimeRootGroup;
        private static Dictionary<string, GameObject> runtimeGroups;

        /// <summary>
        /// Resolves a runtime <see cref="MonoBehaviour"/> while the game is running.
        /// These behaviours are created at runtime and cannot have serialized data.
        /// If you need serialized data, have it resolve an asset.
        /// </summary>
        /// <typeparam name="T">The type of singleton service to locate.</typeparam>
        /// <returns>A service. Never returns null.</returns>
        /// <exception cref="ServiceNotFoundException">The <see cref="MonoBehaviour"/> could not be created.</exception>
        /// <exception cref="ServiceNotFoundException">The game is not running.</exception>
        /// <exception cref="ServiceNotFoundException">The class is marked with <see cref="ServiceOptionsAttribute.IsEditorOnly"/> and this is a build.</exception>
        /// <remarks>Some features are controlled by <see cref="ServiceOptionsAttribute"/>.</remarks>
        public static T ResolveRuntime<T>()
            where T : MonoBehaviour
        {
            T runtimeService = ServiceCache<T>.CachedValue;
            if (runtimeService == null) {
                if (ServiceCache<T>.IsCreating) {
                    throw new ServiceNotFoundException("ServiceLocator is already in the process of creating a new " + ReflectionUtil.CSharpFullName(typeof(T)) + " service. It should not be recusively defined.");
                }

                ServiceCache<T>.IsCreating = true;
                try {
                    if (!Application.isPlaying) {
                        throw new ServiceNotFoundException("ServiceLocator can only resolve runtime objects when the game is running.");
                    }

                    if (typeof(T) != typeof(FocusHandler)) {
                        FocusHandler.Prime();
                    }

                    if (FocusHandler.IsShuttingDown) {
                        if (!ReferenceEquals(runtimeService, null)) {
                            return runtimeService;
                        }

                        T[] objects = Resources.FindObjectsOfTypeAll<T>();
                        foreach (T obj in objects) {
#if UNITY_EDITOR
                            if (UnityEditor.EditorUtility.IsPersistent(obj)) continue;
#endif

                            return obj;
                        }

                        throw new ServiceNotFoundException("ServiceLocator could not create a new " + ReflectionUtil.CSharpFullName(typeof(T)) + " service because the game is shutting down.");
                    }

                    runtimeService = CreateRuntimeService<T>();
                    if (runtimeService == null) {
                        throw new ServiceNotFoundException("The " + ReflectionUtil.CSharpFullName(typeof(T)) + " service could not be created for some unknown reason.");
                    }
                }
                finally {
                    ServiceCache<T>.IsCreating = false;
                }
            }
            return runtimeService;
        }

        private static T CreateRuntimeService<T>() where T : MonoBehaviour
        {
            ServiceOptionsAttribute attrib = (ServiceOptionsAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ServiceOptionsAttribute));
            if (attrib != null && attrib.IsEditorOnly) {
                ThrowIfEditorRestricted();
            }

            string groupName = (attrib != null && !string.IsNullOrEmpty(attrib.GroupName)) ? attrib.GroupName : RuntimeRootGroupName;

            if (runtimeGroups == null) {
                runtimeGroups = new Dictionary<string, GameObject>();
            }

            GameObject group;
            if (!runtimeGroups.TryGetValue(groupName, out group) || !ValidateRuntimeObject(group)) {
                if (!ValidateRuntimeObject(runtimeRootGroup)) {
                    GameObject rootGroup = new GameObject(RuntimeRootGroupName);
                    GameObject.DontDestroyOnLoad(rootGroup);
                    //rootGroup.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

                    runtimeRootGroup = rootGroup;
                    runtimeGroups[RuntimeRootGroupName] = rootGroup;
                }

                // Never create GameObjects if shutting down
                if (groupName != RuntimeRootGroupName && !FocusHandler.IsShuttingDown) {
                    group = new GameObject(groupName);
                    //group.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                    group.transform.SetParent(runtimeRootGroup.transform, false);
                }
                else {
                    group = runtimeRootGroup;
                }

                runtimeGroups[groupName] = group;
            }

            T runtimeService = group.AddComponent<T>();
            ServiceCache<T>.CachedValue = runtimeService;
            return runtimeService;
        }

        private static bool ValidateRuntimeObject(UnityEngine.Object obj)
        {
            return (obj != null || !ReferenceEquals(obj, null) && FocusHandler.IsShuttingDown);
        }

        /// <summary>
        /// Resolves a <see cref="ScriptableObject"/> asset.
        /// </summary>
        /// <typeparam name="T">The type of singleton service to locate.</typeparam>
        /// <returns>The asset. Never returns null.</returns>
        /// <exception cref="ServiceNotFoundException">The class is marked with <see cref="ServiceOptionsAttribute.IsEditorOnly"/> and this is a build.</exception>
        /// <remarks>Some features are controlled by <see cref="ServiceOptionsAttribute"/>.</remarks>
        public static T ResolveAsset<T>()
            where T : ScriptableObject
        {
            T asset = ServiceCache<T>.CachedValue;
            if (asset == null) {
                bool isEditorOnly = false;
                string resourcePath = typeof(T).Name;
                ServiceOptionsAttribute attrib = (ServiceOptionsAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ServiceOptionsAttribute));
                if (attrib != null) {
                    if (!string.IsNullOrEmpty(attrib.GroupName)) {
                        resourcePath = attrib.GroupName + "/" + resourcePath;
                    }
                    isEditorOnly = attrib.IsEditorOnly;
                }

                if (!isEditorOnly) {
//#warning TODO FIX RESOURCES.LOAD
                    asset = Resources.Load<T>(resourcePath);
                }
                else {
                    ThrowIfEditorRestricted();
                }

#if UNITY_EDITOR
                if (asset == null) {
                    string editorRootPath = (isEditorOnly ? StandardEditorPaths.CoreDataEditor : StandardEditorPaths.CoreDataResources);
                    asset = (T)ResolveEditorAssetInternal(typeof(T), editorRootPath, resourcePath, true);
                }
#endif

                if (asset == null) {
                    Debug.LogWarning("No ServiceLocator asset found at Resources.Load(\"" + resourcePath + "\"). Creating a default instance.");
                    asset = ScriptableObject.CreateInstance<T>();
                }

                ServiceCache<T>.CachedValue = asset;
            }

            return asset;
        }

#if UNITY_EDITOR
        public static T ResolveEditorAsset<T>(string editorRootPath, string resourcePath)
            where T : ScriptableObject
        {
            return (T)ResolveEditorAssetInternal(typeof(T), editorRootPath, resourcePath, false);
        }

        private static ScriptableObject ResolveEditorAssetInternal(Type assetType, string editorRootPath, string resourcePath, bool uniqueForType)
        {
            ScriptableObject asset = null;

            string fullPath = editorRootPath + resourcePath + ".asset";
            asset = (ScriptableObject)UnityEditor.AssetDatabase.LoadAssetAtPath(fullPath, assetType);

            if (asset == null) {
                UnityEngine.Object existing = UnityEditor.AssetDatabase.LoadAssetAtPath(fullPath, typeof(UnityEngine.Object));
                if (existing != null) {
                    Debug.LogError("Unable to create default asset at path \"" + fullPath + "\" because a different asset already exists there.", existing);

                    asset = ScriptableObject.CreateInstance(assetType);
                }
                else {
                    if (uniqueForType) {
                        OrderedHashSet<ScriptableObject> allAssets = PathUtil.FindAssets<ScriptableObject>("", assetType);
                        if (allAssets.Count == 1) {
                            asset = allAssets[0];

                            string oldPath = UnityEditor.AssetDatabase.GetAssetPath(asset);

                            PathUtil.CreateDirectoryRecursively(Path.GetDirectoryName(fullPath));
                            UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport | UnityEditor.ImportAssetOptions.ImportRecursive);
                            string errMessage = UnityEditor.AssetDatabase.MoveAsset(oldPath, fullPath);

                            if (string.IsNullOrEmpty(errMessage)) {
                                asset = (ScriptableObject)UnityEditor.AssetDatabase.LoadAssetAtPath(fullPath, assetType);

                                if (asset != null) {
                                    Debug.Log("No ServiceLocator asset found at \"" + fullPath + "\". Moved sole existing asset from \"" + oldPath + "\".", asset);
                                }
                                else {
                                    asset = ScriptableObject.CreateInstance(assetType);

                                    Debug.LogError("No ServiceLocator asset found at \"" + fullPath + "\". Failed to move sole existing asset from \"" + oldPath + "\". " +
                                            "Creating a temporary fresh instance for this run.", asset);
                                }
                            }
                            else {
                                asset = ScriptableObject.CreateInstance(assetType);

                                Debug.LogError("No ServiceLocator asset found at \"" + fullPath + "\". Error moving sole existing asset from \"" + oldPath + "\": " + errMessage + " " +
                                        "Creating a temporary fresh instance for this run.", asset);
                            }
                        }
                        else if (allAssets.Count != 0) {
                            asset = ScriptableObject.CreateInstance(assetType);

                            Debug.LogError("No ServiceLocator asset found at \"" + fullPath + "\". There are multiple assets of that type in the project. " +
                                "Creating a temporary fresh instance for this run.", asset);
                        }
                    }

                    if (asset == null) {
                        PathUtil.CreateDirectoryRecursively(Path.GetDirectoryName(fullPath));
                        UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport | UnityEditor.ImportAssetOptions.ImportRecursive);

#if UNITY_5_3_3 && UNITY_EDITOR
                        FixLoadedAssembliesBug();
#endif

                        asset = ScriptableObject.CreateInstance(assetType);
                        UnityEditor.AssetDatabase.CreateAsset(asset, fullPath);

                        Debug.Log("No asset found at \"" + fullPath + "\". Creating a default " + assetType.CSharpFullName() + " and saving it to disk.", asset);
                    }
                }
            }
            
            return asset;
        }
#endif

        private static void ThrowIfEditorRestricted()
        {
            if (!Application.isEditor) {
                throw new ServiceNotFoundException("Editor-only ServiceLocator classes cannot be used in builds.");
            }
        }

#if UNITY_5_3_3 && UNITY_EDITOR
        private static System.Reflection.PropertyInfo loadedAssemblies = null;

        private static void FixLoadedAssembliesBug()
        {
            if (loadedAssemblies == null)
            {
                foreach (Type type in typeof(UnityEditor.EditorApplication).Assembly.GetTypes())
                {
                    if (type.Name == "EditorAssemblies")
                    {
                        loadedAssemblies = type.GetProperty("loadedAssemblies", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance);
                    }
                }
            }

            if (loadedAssemblies != null)
            {
                object obj = loadedAssemblies.GetValue(null, null);
                if (obj == null)
                {
                    loadedAssemblies.SetValue(null, ArrayUtil.Empty<System.Reflection.Assembly>(), null);
                }
            }
        }
#endif
    }
}