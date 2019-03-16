
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FRG.Core
{
    /// <summary>
    /// An interface for loading and accessing game data.
    /// </summary>
    public static class AssetManager
    {
        private static readonly Dictionary<AssetManagerRef, WeakReference> cachedResources = new Dictionary<AssetManagerRef, WeakReference>();

        /// <summary>
        /// Gets the specified reference of the given type. The reference is required by default.
        /// </summary>
        public static T Get<T>(AssetManagerRef amr)
            where T : UnityEngine.Object
        {
            return (T)Get(amr, typeof(T));
        }

        /// <summary>
        /// Gets the specified reference of the given type.
        /// </summary>
        public static T TryGet<T>(AssetManagerRef amr)
            where T : UnityEngine.Object
        {
            return (T)TryGet(amr, typeof(T));
        }

        /// <summary>
        /// Gets the specified reference of the given type. The reference is required by default.
        /// </summary>
        public static UnityEngine.Object Get(AssetManagerRef amr, Type assetType)
        {
            return GetInternal(amr, true, assetType);
        }

        /// <summary>
        /// Gets the specified reference of the given type.
        /// </summary>
        public static UnityEngine.Object TryGet(AssetManagerRef amr, Type assetType)
        {
            return GetInternal(amr, false, assetType);
        }


        /// <summary>
        /// Gets the specified reference of the given type.
        /// </summary>
        private static UnityEngine.Object GetInternal(AssetManagerRef amr, bool require, Type type)
        {
            if (!type.IsSubclassOf(typeof(UnityEngine.Object)) && type != typeof(UnityEngine.Object)) {
                Debug.LogError("AssetManager: " + amr.ToString() + " is requesting a type " + ReflectionUtil.CSharpFullName(type) + " that is not a subclass of Object.");
                if (require) {
                    throw new AssetNotFoundException("A required asset of type " + ReflectionUtil.CSharpFullName(type) + " was requested, but that is not a valid asset type.");
                }
                return null;
            }

            if (!amr.IsValid) {
                if (require) {
                    throw new AssetNotFoundException("A required asset of type " + ReflectionUtil.CSharpFullName(type) + " was requested, but was passed a null AssetManagerRef.");
                }
                return null;
            }

            Type requestedType = type;
            if (type == typeof(Component) || type.IsSubclassOf(typeof(Component))) {
                type = typeof(GameObject);
            }

            UnityEngine.Object asset = LoadResource(amr, type, require);
            if (asset == null && require) {
                throw new AssetNotFoundException("A required asset " + amr + " of type " + ReflectionUtil.CSharpFullName(requestedType) + " was requested, but it could not be found.");
            }

            if (requestedType != type && asset != null) {
                GameObject go = asset as GameObject;
                Component component = go.GetComponent(requestedType);
                if (component == null) {
                    var comps = go.GetComponents<Component>();
                    if (comps != null) {
                        for (int c = 0; c < comps.Length; ++c) {
                            if (requestedType.IsAssignableFrom(comps[c].GetType())) {
                                component = comps[c];
                                break;
                            }
                        }
                    }

                    if (component == null && require)
                        throw new AssetNotFoundException("A required component " + amr.ToString() + " of type " + ReflectionUtil.CSharpFullName(requestedType) + " was requested, but while the GameObject was found, the component was not on the GameObject.");
                }
                asset = component;
            }

            return asset;
        }

        private static UnityEngine.Object LoadResource(AssetManagerRef reference, Type type, bool warn)
        {
            using (ProfileUtil.PushSample("AssetManager.LoadResource")) {
                AssetManagerResource resource = null;

                WeakReference weak;
                if (!cachedResources.TryGetValue(reference, out weak)) {
                    weak = new WeakReference(null, false);
                }

                resource = weak.Target as AssetManagerResource;

                if (resource == null) {
                    resource = Resources.Load<AssetManagerResource>(reference.UniqueId);
                }

                if (resource != null) {
                    if (resource.asset == null) {
                        if (warn) {
                            Debug.LogError("AssetManagerResource called " + reference.ToString() + " has a null object reference! (Looking for " + ReflectionUtil.CSharpFullName(type) + ".) Make sure the AssetManagerResource references you use are valid.", resource);
                        }
                        return null;
                    }
                    else if (!type.IsInstanceOfType(resource.asset)) {
                        if (warn) {
                            Debug.LogError("AssetManagerResource " + reference.ToString() + " has a reference of type " + ReflectionUtil.CSharpFullName(resource.asset.GetType()) + ", not " + ReflectionUtil.CSharpFullName(type), resource);
                        }
                        return null;
                    }
                    else {
                        weak.Target = resource;
                        return resource.asset;
                    }
                }
                else {
                    if (warn) {
                        UnityEngine.Object context = null;
#if UNITY_EDITOR
                        context = AssetManagerEditor.ContextualLoad(reference, type);
#endif
                        Debug.LogWarning("AssetManagerResource " + reference.ToString() + " could not be found! Add an AssetManagerResource for the object referred to.", context);
                    }
                    return null;
                }
            }
        }
    }
}

