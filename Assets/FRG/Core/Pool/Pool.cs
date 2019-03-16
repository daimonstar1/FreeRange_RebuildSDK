#if UNITY_EDITOR
#define POOL_SORT_BY_GROUP
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FRG.Core {
    [AddComponentMenu(""), DisallowMultipleComponent]
    [ServiceOptions(GroupName = "PrefabPool")]
    public class Pool : MonoBehaviour
    {
        [NonSerialized]
        Transform _activeRoot;
        [NonSerialized]
        Transform _inactiveRoot;
        [NonSerialized]
        Transform _componentWiseInactiveRoot;

        Dictionary<int, PrefabPool> _instanceIdToPoolLookup;

        /// <remarks>
        /// Serialize this field so we know which pool objects to delete.
        /// </remarks>
        List<PrefabPool> _prefabPools = new List<PrefabPool>();
        
        List<PoolObject> _oneShotObjects = new List<PoolObject>();

        private static Pool _savedInstance;
        private static Pool Instance
        {
            get
            {
                if (_savedInstance == null)
                    _savedInstance = ServiceLocator.ResolveRuntime<Pool>();

                return _savedInstance;
            }
        }

        private Dictionary<int, PrefabPool> InstanceIdToPoolLookup {
            get {
                if (_instanceIdToPoolLookup == null) {
                    _instanceIdToPoolLookup = new Dictionary<int, PrefabPool>();
                    for (int i = 0; i < _prefabPools.Count; ++i) {
                        var pool = _prefabPools[i];
                        int instanceId = pool.PrefabInstanceId;
                        _instanceIdToPoolLookup.Add(instanceId, pool);
                    }
                }
                return _instanceIdToPoolLookup;
            }
        }

        public static void DespawnAll()
        {
            if (_savedInstance != null)
            {
                _savedInstance.RunForAll(pool => pool.DespawnAll());
            }
        }

        public static Dictionary<int, int> GetPreseedCounts()
        {
            List<PrefabPool> prefabPools = Instance._prefabPools;

            Dictionary<int, int> counts = new Dictionary<int, int>(prefabPools.Count);
            for (int i = 0; i < prefabPools.Count; ++i)
            {
                PrefabPool pool = prefabPools[i];
                if (pool.PrefabInstanceId == 0)
                {
                    Debug.LogError("Zero instance id in pool " + pool.PrefabName);
                }

                counts.Add(pool.PrefabInstanceId, pool.PreseedCount);
            }
            return counts;
        }

        protected void Update()
        {
            for (int i = _oneShotObjects.Count - 1; i >= 0; i -= 1)
            {
                PoolObject obj = _oneShotObjects[i];
                // May change _oneShotObjects, but double-execution is OK
                PoolObject.UpdateOneShotExpiration(obj);
            }
        }

        private void DestroyPools()
        {
            RunForAll(pool => pool.DestroyPool());
        }

        private void RunForAll(Action<PrefabPool> action)
        {
            if (_prefabPools != null)
            {
                for (int i = _prefabPools.Count - 1; i >= 0; --i)
                {
                    PrefabPool pool = _prefabPools[i];
                    action(pool);
                }

                // Might skip some if despawns spawn, but let's ignore that horrible case
                if (_instanceIdToPoolLookup != null) { _instanceIdToPoolLookup.Clear(); }
                _prefabPools.Clear();
            }
        }

        internal static void RegisterOneShot(PoolObject poolObject)
        {
            if (poolObject == null) { throw new ArgumentNullException("poolObject"); }

            if (_savedInstance != null && _savedInstance._oneShotObjects != null)
            {
                _savedInstance._oneShotObjects.Add(poolObject);
            }
        }

        internal static void DeregisterOneShot(PoolObject poolObject)
        {
            if (_savedInstance != null && _savedInstance._oneShotObjects != null)
            {
                _savedInstance._oneShotObjects.Remove(poolObject);
            }
        }

        /// <summary>
        /// Finds a pool or creates it.
        /// </summary>
        internal static PrefabPool GetOrCreatePool(GameObject prefabGameObject)
        {
            if (prefabGameObject == null) { throw new ArgumentNullException("prefab"); }

            using (ProfileUtil.PushSample("Pool.GetPool"))
            {
                int prefabInstanceId = prefabGameObject.GetInstanceID();

                PrefabPool pool;
                if (!TryGetPool(prefabInstanceId, out pool))
                {
                    Type prefabPoolObjectType;
                    PoolObject prefabPoolObject = prefabGameObject.GetComponent<PoolObject>();
                    if (prefabPoolObject == null) {
                        Debug.LogErrorFormat(prefabGameObject, "PoolObject is not in \"{0}\" prefab GameObject. Adding one.", prefabGameObject.name);
                        prefabPoolObject = prefabGameObject.AddComponent<PoolObject>();
                    }
                    else if (!ReferenceEquals(prefabPoolObject.transform.parent, null)) {
                        Debug.LogErrorFormat(prefabGameObject, "PoolObject is not at \"{0}\" prefab root. Adding a generic one.", prefabGameObject.name);
                        prefabPoolObject = prefabPoolObject.transform.root.gameObject.GetOrAddComponent<PoolObject>();
                    }

                    if (prefabPoolObject == null) {
                        throw new InvalidOperationException("Could not add PoolObject script to PoolObject prefab " + prefabGameObject.name);
                    }

                    prefabPoolObjectType = prefabPoolObject.GetType();

                    Debug.Assert(prefabInstanceId != 0, "Prefab instance ID must be nonzero", prefabGameObject);

                    Pool instance = Instance;
                    if (instance._activeRoot == null) {
                        instance._activeRoot = new GameObject("Active").transform;
                        instance._activeRoot.SetParent(instance.transform, false);
                    }
                    if (instance._inactiveRoot == null) {
                        instance._inactiveRoot = new GameObject("Inactive").transform;
                        instance._inactiveRoot.SetParent(instance.transform, false);
                        instance._inactiveRoot.gameObject.SetActive(false);
                    }
                    if (instance._componentWiseInactiveRoot == null) {
                        instance._componentWiseInactiveRoot = new GameObject("Inactive (Component-wise)").transform;
                        instance._componentWiseInactiveRoot.SetParent(instance.transform, false);
                    }

                    pool = new PrefabPool(prefabInstanceId, prefabGameObject.name, prefabPoolObjectType, instance._activeRoot, instance._inactiveRoot,
                        instance._componentWiseInactiveRoot);

                    instance.InstanceIdToPoolLookup.Add(pool.PrefabInstanceId, pool);
                    instance._prefabPools.Add(pool);

#if UNITY_EDITOR
                    // debug: write pool objects that are not presseded but added to the pool later, creating their own pool here
                    if (PreseedManager.DebugPreseeding && PreseedManager.IsPreseeded)
                    {
                        Debug.LogErrorFormat(instance, "Created new pool for objects after preseed: {0}; Please add them to preseed snapshot.\nYou are seeing this because you have preseed debug enabled (Edit > Preferences > FRG > Debug Prefab Pool Preseeding)", prefabGameObject.name);
                    }
#endif
                }

                return pool;
            }
        }

        /// <summary>
        /// Finds an existing pool or returns null.
        /// </summary>
        internal static bool TryGetPool(int prefabInstanceId, out PrefabPool pool)
        {
            if (prefabInstanceId == 0) { throw new ArgumentNullException("prefabInstanceId"); }

            using (ProfileUtil.PushSample("Pool.TryGetPool"))
            {
                return Instance.InstanceIdToPoolLookup.TryGetValue(prefabInstanceId, out pool);
            }
        }

        [Serializable]
        public class PrefabPool
        {
            [SerializeField] private string prefabName;
            [SerializeField] private int prefabInstanceId;
            [SerializeField] private Transform activeRoot;
            [SerializeField] private Transform inactiveRoot;
            [SerializeField] private Transform componentWiseInactiveRoot;
            /// <summary>
            /// Objects handled by pool.
            /// </summary>
            [SerializeField] private List<PoolObject> objects;
            /// <summary>
            /// Numner of objects in pool that are spawned and active/used in the game
            /// </summary>
            [SerializeField] private int activeCount;
            /// <summary>
            /// Number of total objects handled by pool.
            /// </summary>
            [SerializeField] private int totalCount;

            private static Dictionary<int, string> _intMemoization;

            public string PrefabName { get { return prefabName; } }
            public int PrefabInstanceId { get { return prefabInstanceId; } }
            public int PreseedCount { get { return objects.Count; } }
            public Transform ActiveRoot { get { return activeRoot; } }
            public Transform InactiveRoot { get { return inactiveRoot; } }
            public Transform ComponentWiseInactiveRoot { get { return componentWiseInactiveRoot; } }

            internal PrefabPool(int prefabInstanceId, string prefabName, Type poolObjectType, Transform activeRoot, Transform inactiveRoot, 
                Transform componentWiseInactiveRoot)
            {
                this.prefabInstanceId = prefabInstanceId;
                this.prefabName = prefabName;
                this.activeRoot = activeRoot;
                this.inactiveRoot = inactiveRoot;
                this.componentWiseInactiveRoot = componentWiseInactiveRoot;
                this.objects = new List<PoolObject>(8);
            }

            internal PoolObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
            {
                RemoveExternalyDestroyedObjects();

                PoolObject poolObject = null;

                for (int i = activeCount; i < objects.Count; ++i) {
                    var obj = objects[i];
                    if (!obj.WasDespawnedThisFrame) {
                        objects[i] = objects[activeCount];
                        objects[activeCount] = obj;

                        poolObject = obj;
                        break;
                    }
                }

#if UNITY_EDITOR
                // debug: write pool objects that are not preseeded but added to the pool later
                if (poolObject == null && PreseedManager.DebugPreseeding && PreseedManager.IsPreseeded) {
                    Debug.LogErrorFormat(prefab, "Putting new object into pool after preseed: {0}; Please add it to preseed snapshot.\nYou are seeing this because you have preseed debug enabled (Edit > Preferences > FRG > Debug Prefrab Pool Preseeding)", prefab.name);
                }
#endif

                if (objects.Count == 0 && activeRoot != null && inactiveRoot != null && componentWiseInactiveRoot != null) {
                    using (ProfileUtil.PushSample("Pool.PrefabPool.Despawn (POOL_SORT_BY_GROUP create root)", prefab)) {
                        GameObject activeGroupParent = new GameObject(prefabName);
                        activeGroupParent.transform.SetParent(activeRoot, false);
                        activeRoot = activeGroupParent.transform;

                        GameObject inactiveGroupParent = new GameObject(prefabName);
                        inactiveGroupParent.transform.SetParent(inactiveRoot, false);
                        inactiveRoot = inactiveGroupParent.transform;

                        GameObject componentWiseInactiveGroupParent = new GameObject(prefabName);
                        componentWiseInactiveGroupParent.transform.SetParent(componentWiseInactiveRoot, false);
                        componentWiseInactiveRoot = componentWiseInactiveGroupParent.transform;
                    }
                }

                Debug.Assert(activeRoot.gameObject.activeInHierarchy, "PoolObject active root must be active");
                Debug.Assert(!inactiveRoot.gameObject.activeInHierarchy, "PoolObject inactive root must be inactive");
                Debug.Assert(componentWiseInactiveRoot.gameObject.activeInHierarchy, "PoolObject inactive (component-wise) root must be active");

                bool spawnedWithParent = false;

                if (poolObject == null)
                {
                    using (ProfileUtil.PushSample("Pool.PrefabPool.Spawn (create new)", prefab)) {
                        GameObject gameObject;
                        if (parent != null && parent.gameObject.activeInHierarchy && prefab.GetComponent<PoolObject>().isActiveAndEnabled) {
                            spawnedWithParent = true;

                            RectTransform parentRectTransform = parent.transform as RectTransform;
                            Vector3 worldPosition = (parentRectTransform != null) ? parentRectTransform .anchoredPosition3D + position : parent.position + position;
                            Quaternion worldRotation = parent.rotation * rotation;
                            gameObject = (GameObject)Instantiate(prefab, worldPosition, worldRotation, parent);
                        }
                        else {
                            gameObject = (GameObject)Instantiate(prefab, position, rotation, activeRoot);
                        }

                        poolObject = gameObject.GetComponent<PoolObject>();
                        if (poolObject == null) {
                            Destroy(gameObject);
                            Debug.LogErrorFormat(prefab, "{0} should have a PoolObject. Pool.Spawn was called on it.", prefab.name);
                            throw new InvalidOperationException("Could not spawn PoolObject.");
                        }

                        if ( poolObject.hatesScale ) poolObject.transform.localScale = Vector3.one;

                        gameObject.name += CountToString(totalCount);

                        objects.Insert(activeCount, poolObject);
                    }
                }
                else {
                    Transform transform = poolObject.transform;
                    transform.localPosition = position;
                    transform.localRotation = rotation;
                    if ( poolObject.hatesScale ) transform.localScale = Vector3.one;
                }

                // reparenting from pool to desired parent. This is skipped if object is just instantiated and parent is set during Instantiate()
                if(poolObject.dontReparent) {
                    if(!poolObject.componentWiseDisable)
                        poolObject.gameObject.SetActive(true);
                } else if (!spawnedWithParent) {
                    using (ProfileUtil.PushSample("Pool.PrefabPool.Spawn (set parent)", poolObject)) {
                        // Kova: SetParent() crashes randomly due to unity bugs on it
                        // https://issuetracker.unity3d.com/issues/player-slash-editor-crash-on-transform-setparent-when-playing-scene
                        // https://issuetracker.unity3d.com/issues/editor-crash-on-canvasrenderer-when-game-object-is-given-different-parent
                        // https://issuetracker.unity3d.com/issues/standalone-combination-of-setparent-and-setactive-true-causes-crash-in-standalone-build
                        // etc.. try working around it by setting it active first
                        //poolObject.gameObject.SetActive(true);
                        //poolObject.transform.SetParent(null, false);
                        poolObject.transform.SetParent(parent, false);
                    }
                }

                using (ProfileUtil.PushSample("Pool.PrefabPool.Spawn (activate)", poolObject))
                {
                    if (!poolObject.componentWiseDisable) {
                        if (!poolObject.gameObject.activeSelf)
                            poolObject.gameObject.SetActive(true);
                    }
                    else
                        poolObject.ToggleComponents(true);

                    if (!poolObject.enabled)
                        poolObject.enabled = true;
                }

                activeCount++;

#if POOL_SORT_BY_GROUP
                if (activeRoot != null && inactiveRoot != null && componentWiseInactiveRoot != null) {
                    using (ProfileUtil.PushSample("Pool.PrefabPool.Spawn (POOL_SORT_BY_GROUP set name)", poolObject)) {
                        string name = prefabName + " [" + CountToString(activeCount) + "/" + CountToString(objects.Count) + "]";
                        activeRoot.name = name;
                        inactiveRoot.name = name;
                        componentWiseInactiveRoot.name = name;
                    }
                }
#endif

                return poolObject;
            }

            internal void Despawn(PoolObject poolObject)
            {
                RemoveExternalyDestroyedObjects();

                // trying to despawn already destroyed object
                if (poolObject == null)
                    return;

                int index;
                int count = objects.Count;
                for (index = 0; index < count; ++index)
                {
                    if (ReferenceEquals(objects[index], poolObject))
                    {
                        break;
                    }
                }
                
                if (index == count)
                {
                    Debug.LogWarning("PoolObject not in pool.", poolObject);
                }
                else
                {
                    activeCount--;
                    objects[index] = objects[activeCount];
                    objects[activeCount] = poolObject;

#if POOL_SORT_BY_GROUP
                    if (activeRoot != null && inactiveRoot != null && componentWiseInactiveRoot != null) {
                        using (ProfileUtil.PushSample("Pool.PrefabPool.Despawn (POOL_SORT_BY_GROUP set name)", poolObject)) {
                            string name = prefabName + " [" + CountToString(activeCount) + "/" + CountToString(objects.Count) + "]";
                            activeRoot.name = name;
                            inactiveRoot.name = name;
                            componentWiseInactiveRoot.name = name;
                        }
                    }
#endif
                }
            }

            /// <summary>
            /// Cleans up the pool from invalid state. Objects that pool handles could be destroyed outside of it with Destroy()
            /// and they become invalid after. Destroyed objects return "== null" as true as operator is overriden in unity, 
            /// but reference is not null.
            /// </summary>
            private void RemoveExternalyDestroyedObjects()
            {
                for (int i = objects.Count - 1; i >= 0; --i)
                {
                    if (objects[i] == null)
                    {
                        objects.RemoveAt(i);
                        if (i < activeCount - 1)
                        {
                            activeCount--;
                        }
                    }
                }
            }

            internal void DespawnAll()
            {
                RemoveExternalyDestroyedObjects();
                for (int i = activeCount - 1; i >= 0; i = Mathf.Min(activeCount - 1, i - 1))
                {
                    objects[i].Despawn();
                }
            }

            internal void DestroyPool()
            {
                RemoveExternalyDestroyedObjects();

                for (int i = objects.Count - 1; i >= 0; --i)
                {
                    if (objects[i] != null)
                    {
                        if (i < activeCount)
                        {
                            objects[i].Despawn();
                        }

                        GameObject gameObject = objects[i].gameObject;
                        if (gameObject != null)
                        {
                            if (Application.isPlaying)
                            {
                                Destroy(gameObject);
                            }
                            else
                            {
                                DestroyImmediate(gameObject, false);
                            }
                        }
                    }
                }

                objects.Clear();
                activeCount = 0;

#if UNITY_EDITOR
                if (activeRoot != null && inactiveRoot != null && componentWiseInactiveRoot != null && objects.Count != 0) {
                    GameObject oldActiveRoot = activeRoot.gameObject;
                    activeRoot = activeRoot.parent;
                    if (oldActiveRoot != null) {
                        if (Application.isPlaying) {
                            Destroy(oldActiveRoot);
                        }
                        else {
                            DestroyImmediate(oldActiveRoot, false);
                        }
                    }

                    GameObject oldInactiveRoot = inactiveRoot.gameObject;
                    inactiveRoot = inactiveRoot.parent;
                    if (oldInactiveRoot != null) {
                        if (Application.isPlaying) {
                            Destroy(oldInactiveRoot);
                        }
                        else {
                            DestroyImmediate(oldInactiveRoot, false);
                        }
                    }

                    GameObject oldComponentWiseInactiveRoot = componentWiseInactiveRoot.gameObject;
                    componentWiseInactiveRoot = componentWiseInactiveRoot.parent;
                    if (oldComponentWiseInactiveRoot != null) {
                        if (Application.isPlaying) {
                            Destroy(oldComponentWiseInactiveRoot);
                        }
                        else {
                            DestroyImmediate(oldComponentWiseInactiveRoot, false);
                        }
                    }
                }
#endif
            }
            
            private static string CountToString(int count)
            {
                if (_intMemoization == null)
                {
                    _intMemoization = new Dictionary<int, string>();
                }

                string result;
                if (!_intMemoization.TryGetValue(count, out result))
                {
                    result = count.ToString();
                    _intMemoization.Add(count, result);
                }
                return result;
            }
        }
    }
}
