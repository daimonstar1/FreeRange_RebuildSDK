
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FRG.Core
{
    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    /// <summary>
    /// Spawner class to spawn a single PoolObject in game at runtime
    /// </summary>
    [AddComponentMenu("PoolObjectSpawner"), DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class PoolObjectSpawner : MonoBehaviour, IPresaveCleanupHandler
    {
        /// <summary>
        /// (Re)spawn all elements for this spawner. Optionally clearing all existing elements beforehand.
        /// </summary>
        public OBJ Spawn<OBJ>(bool clearExisting_ = true)
            where OBJ : PoolObject
        {
            PoolObject ret = Spawn(clearExisting_);
            if (ret is OBJ)
            {
                return (OBJ)ret;
            }
            else
            {
                Debug.LogError("PoolObjectSpawner trying to return spawned object as a (" + ReflectionUtil.CSharpFullName(typeof(OBJ)) + "), "
                    + "but it is a (" + ReflectionUtil.CSharpFullName(ret.GetType()) + "). Returning NULL");
                return default(OBJ);
            }
        }

        /// <summary>
        /// (Re)spawn all elements for this spawner, using a different object asset than the one specified in the inspector;
        /// Optionally clearing all existing elements beforehand
        /// </summary>
        public OBJ Spawn<OBJ>(GameObject prefab, bool clearExisting_ = true)
            where OBJ : PoolObject
        {
            PoolObject ret = Spawn(prefab, clearExisting_);
            if (ret is OBJ)
            {
                return (OBJ)ret;
            }
            else
            {
                Debug.LogError("PoolObjectSpawner trying to return spawned object as a (" + typeof(OBJ).CSharpFullName() + "), "
                    + "but it is a (" + (ret != null ? ret.GetType().CSharpFullName() : "null") + "). Returning NULL");
                return default(OBJ);
            }
        }

        public List<PoolObject> Children { get { return _children; } }
        public PoolObject Child { get { if (_children != null && _children.Count > 0) return _children[0]; return null; } }
        public CHILD GetChild<CHILD>() where CHILD : PoolObject
        {
            if (Child is CHILD) return (CHILD)Child;
            return default(CHILD);
        }

        public PoolObject Spawn()
        {
            return Spawn(true);
        }

        /// <summary>
        /// (Re)spawn all elements for this spawner. Optionally clearing all existing elements beforehand.
        /// </summary>
        public PoolObject Spawn(bool clearExisting_)
        {
            _Init();
            if (clearExisting_)
            {
                Despawn();
            }

            PoolRef? pRef;
            return _Spawn(ElementPrefab, _spawningSystem, null, out pRef);
        }

        /// <summary>
        /// (Re)spawn all elements for this spawner, using a different object asset than the one specified in the inspector;
        /// Optionally clearing all existing elements beforehand
        /// </summary>
        public PoolObject Spawn(GameObject prefab, bool clearExisting_ = true)
        {
            _Init();
            if (clearExisting_)
            {
                Despawn();
            }
            prefab = prefab ?? ElementPrefab;

            PoolRef? pRef;
            return _Spawn(prefab, _ResolveSpawningSystem(prefab), null, out pRef);
        }

        /// <summary>
        /// (Re)spawn all elements for this spawner. Optionally clearing all existing elements beforehand.
        /// </summary>
        public PoolRef SpawnRef(bool clearExisting_ = true)
        {
            _Init();
            if (clearExisting_)
            {
                Despawn();
            }

            PoolRef? pRef;
            _Spawn(ElementPrefab, _spawningSystem, null, out pRef);

            return pRef.Value;
        }

        /// <summary>
        /// (Re)spawn all elements for this spawner, using a different object asset than the one specified in the inspector;
        /// Optionally clearing all existing elements beforehand
        /// </summary>
        public PoolRef SpawnRef(GameObject prefab, bool clearExisting_ = true)
        {

            _Init();
            if (clearExisting_)
            {
                Despawn();
            }
            prefab = prefab ?? ElementPrefab;

            PoolRef? pRef;
            _Spawn(prefab, _ResolveSpawningSystem(prefab), null, out pRef);

            return pRef.Value;
        }

        ///:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        /// <summary>
        /// Despawn any currently spawned elements. Depending on the object components, will either
        ///   call PoolObject.Despawn(), or UnityEngine.Object.Destroy()
        /// </summary>
        public void Despawn(float delay = 0f)
        {
            foreach (var child in _children)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    if (child != null)
                    {

                        child.DespawnedAction -= PoolObjectDespawnedElsewhere;

                        if (delay <= 0f)
                        {
                            child.Despawn();
                        }
                        else
                        {
                            child.DespawnAfterDelay(delay);
                        }
                    }
                }
                else if (child != null)
                {
                    DestroyImmediate(child.gameObject);
                }
#else
                if(child != null) {
                    child.DespawnedAction -= PoolObjectDespawnedElsewhere;
                    child.Despawn();
                }
#endif
            }
            _children.Clear();

            spawnedPrefab = null;
        }

        //........................................................................................................
        /// <summary>
        /// Either Pool-Spawns, or Unity-Instantiates the elements depending on the type parameter passed
        /// </summary>
        /// <param name="assetRef_">Prefab of the element to spawn.</param>
        /// <param name="spawningSystem_">The spawning system to use; by default, should be called with this._spawningSystem</param>
        private PoolObject _Spawn(GameObject prefab, _SpawningSystem spawningSystem_, object[] args_, out PoolRef? pRef)
        {
            pRef = null;

            if (Application.isPlaying && FocusHandler.IsShuttingDown) return null;

            if (debug) Debug.Log("frame(" + Time.frameCount + ") _Spawn(" + prefab.name + ")");
            _Init();

            spawnedPrefab = prefab;

            switch (spawningSystem_)
            {
                default: return null;
                case _SpawningSystem.Instantiate:
                    {
                        GameObject elementPrefabGameObject = prefab;
                #if UNITY_EDITOR
                        var child = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(elementPrefabGameObject, transform.gameObject.scene);
                        child.transform.SetParent(transform, false);
                        child.hideFlags = HideFlags.DontSaveInEditor;
                #else
                    var child = (GameObject)Instantiate(elementPrefabGameObject, transform, false);
                    child.hideFlags = HideFlags.DontSaveInEditor;
                #endif
                        var poolObject = child.GetComponent<PoolObject>();
                        if (poolObject == null)
                        {
                            Debug.LogError("PoolObject cannot be found on game object being spawned: " + elementPrefabGameObject.name);
                            return null;
                        }
                        _AddToChildren(poolObject);
                        if (onSpawn != null)
                        {
                            onSpawn.InvokeOn(poolObject);
                        }
                        return poolObject;
                    }
                case _SpawningSystem.PoolSpawn:
                    {
                        pRef = PoolObject.Spawn(prefab, transform);
                        var child = pRef.Value.GetPoolObject<PoolObject>();

                        child.DespawnedAction += PoolObjectDespawnedElsewhere;

                        _AddToChildren(child);
                        if (onSpawn != null)
                        {
                            onSpawn.InvokeOn(child);
                        }
                        return child;
                    }
            }
        }

        //buttons for editor
        [InspectorButton("ButtonSpawnPreview", "Spawn")]
        [InspectorButton("ButtonSpawnPreviewRecursive", "Spawn ALL")]
        [InspectorButton("ButtonSaveAndDespawnPreview", "Save + Despawn")]
        //[InspectorButton("ButtonSaveAndDespawnPreviewRecursive", "Save ALL + Despawn")]
        [InspectorButton("ButtonDespawnPreviewWithoutSaving", "Despawn")]
        [SerializeField] private GameObject _elementPrefab;
        [SerializeField] private GameObject _elementPrefab_Override_VR;

#if UNITY_EDITOR
        [NonSerialized] private GameObject lastElementPrefab;
#endif

        public GameObject ElementPrefab
        {
            get
            {
                //check for prefab override by mode
                //switch(SharedVersion.CurrentAssetSku) {
                //    case AssetSku.RiftVR:
                //    case AssetSku.GearVR:
                //    case AssetSku.SteamVR:
                //        if(_elementPrefab_Override_VR.IsValid) return _elementPrefab_Override_VR;
                //        break;
                //}

                return _elementPrefab;
            }
            set
            {

                //check for prefab override by mode, assign new value to override if it was a valid prefab
                //switch(SharedVersion.CurrentAssetSku) {
                //    case AssetSku.RiftVR:
                //    case AssetSku.GearVR:
                //    case AssetSku.SteamVR:
                //        if(_elementPrefab_Override_VR.IsValid) {
                //            _elementPrefab_Override_VR = value;
                //        }
                //        break;
                //}

                _elementPrefab = value;
            }
        }
        [SerializeField] protected Vector3 _elementScale = Vector3.one;
        public Vector3 ElementScale { get { return _elementScale; } set { _elementScale = value; } }

        [SerializeField] private bool _autoSpawnInGame = false;
        [InspectorHide]
        [SerializeField]
        private bool _autoSpawnInEditor = false;
        public bool AutoSpawnInEditor { get { return _autoSpawnInEditor; } set { _autoSpawnInEditor = value; } }

        [SerializeField]
        private bool _autoSaveWhenSavingScene = true;

        public bool AutoSaveOnSceneSave { get { return _autoSaveWhenSavingScene; } set { _autoSaveWhenSavingScene = value; } }

        [SerializeField] public UnityEvent onSpawn = null;

        [SerializeField] private bool doNotResetTransformOnSpawn = false;

        [SerializeField] private bool debug = false;

        [SerializeField] private float spawnEarlyTime = 0f;
        public float SpawnEarlyTime { get { return spawnEarlyTime; } }

        public bool IsSpawned
        {
            get
            {
                return (_children != null) && (_children.Count > 0);
            }
        }

        public int SpawnCount { get { return _children != null ? _children.Count : 0; } }

        [NonSerialized] GameObject spawnedPrefab = null;
        public GameObject SpawnedPrefab { get { return spawnedPrefab; } }

        private enum _SpawningSystem { Instantiate, PoolSpawn }

        [NonSerialized]
        private _SpawningSystem _spawningSystem = _SpawningSystem.Instantiate;

        protected List<PoolObject> _children = new List<PoolObject>();

        [NonSerialized]
        private bool _hasInit = false;

#if UNITY_EDITOR
        [NonSerialized]
        bool spawnedEditorPrefab = false;

        [NonSerialized]
        private bool _spawnOnNextEditorUpdate = false;
#endif

        //........................................................................................................
        /// <summary>
        /// Add a newly spawned element to children, setting hide-flags if we're in editor
        /// </summary>
        private void _AddToChildren(PoolObject obj)
        {
            _children.Add(obj);

            if (!doNotResetTransformOnSpawn)
            {
                Util.ResetTransform(obj.transform);
                if (obj.transform is RectTransform)
                {
                    RectTransform trans = (RectTransform)obj.transform;
                    //properly scale the bounds of the new element depending on its rect transform
                    float sizeX = trans.sizeDelta.x * (1f - (trans.anchorMax.x - trans.anchorMin.x));
                    float sizeY = trans.sizeDelta.y * (1f - (trans.anchorMax.y - trans.anchorMin.y));
                    trans.sizeDelta = new Vector2(sizeX, sizeY);
                }
                obj.transform.localScale = _elementScale;
            }
        }

        //........................................................................................................
        /// <summary>
        /// On Spawn, if in game, spawn all elements if we've specified to do so
        /// </summary>
        protected void OnEnable()
        {
            _hasInit = false;
            _Init();
#if UNITY_EDITOR
            if (!Application.isPlaying && _autoSpawnInEditor)
            {
                while (_children.Count == 0 && transform.childCount > 0)
                {
                    var trans = transform.GetChild(0);
                    trans.SetParent(null);
                    DestroyImmediate(trans.gameObject);
                }
                Spawn(true);
                spawnedEditorPrefab = true;
            }
            else if (spawnedEditorPrefab)
            {
                Despawn();
                spawnedEditorPrefab = false;
            }

            lastElementPrefab = null;
#endif
        }

        private void _Init()
        {
            if (_hasInit) return;
            _hasInit = true;

            _spawningSystem = _ResolveSpawningSystem(ElementPrefab);
            if (Application.isPlaying && _autoSpawnInGame)
            {
                Spawn(true);
            }
        }

        private _SpawningSystem _ResolveSpawningSystem(GameObject prefabObj)
        {
            if (prefabObj != null && prefabObj.GetComponent<PoolObject>() != null && Application.isPlaying)
                return _SpawningSystem.PoolSpawn;
            return _SpawningSystem.Instantiate;
        }

#if UNITY_EDITOR

        //........................................................................................................
        /// <summary>
        /// (EDITOR ONLY) Called when we click the Spawn button on main inspector
        /// </summary>
        [ContextMenu("Spawn Preview")]
        public void ButtonSpawnPreview()
        {
            _spawnOnNextEditorUpdate = true;
            _PreviewCleanup();
            _Preview_Spawn();
        }

        //........................................................................................................
        /// <summary>
        /// (EDITOR ONLY) Called when we click the Spawn button on main inspector
        /// </summary>
        public void ButtonSpawnPreviewRecursive()
        {
            _spawnOnNextEditorUpdate = true;
            _PreviewCleanup();
            _Preview_Spawn();

            IList<PoolObjectSpawner> childSpawners = GetComponentsInChildren<PoolObjectSpawner>(true);
            for (int i = 0; i < childSpawners.Count; ++i)
            {
                try
                {
                    if (childSpawners[i].gameObject.GetInstanceID() != this.gameObject.GetInstanceID())
                        childSpawners[i].ButtonSpawnPreviewRecursive();
                }
                catch (Exception e)
                {
                    ReflectionUtil.CheckDangerousException(e);

                    //if (logger.IsErrorEnabled)
                    //{
                    //    logger.Error("Error attempting to preview PoolObjectSpawner child " + ToString() + "; ignoring.", e);
                    //}
                    continue;
                }
            }
        }

        //........................................................................................................
        /// <summary>
        /// (EDITOR ONLY) Called when we click the Despawn button on main inspector
        /// </summary>
        [ContextMenu("Despawn Preview")]
        public void ButtonDespawnPreviewWithoutSaving()
        {
            _spawnOnNextEditorUpdate = false;
            _PreviewCleanup();
        }

        //........................................................................................................
        /// <summary>
        /// (EDITOR ONLY) ReSpawn and refresh all children, popup a progress dialog if it starts taking too long
        /// </summary>
        private void _Preview_Spawn()
        {
            if (!Application.isPlaying && !_spawnOnNextEditorUpdate)
                return;
            _PreviewCleanup();
            Spawn(true);
        }

        //........................................................................................................
        /// <summary>
        /// (EDITOR ONLY) Cleanup all children of this spawner
        /// </summary>
        public void _PreviewCleanup()
        {

            if (this != null && (!Application.isPlaying || !FocusHandler.IsShuttingDown))
            {
                List<GameObject> objectsToCleanUp = new List<GameObject>();

                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    if (child != null)
                    {
                        objectsToCleanUp.Add(child.gameObject);
                    }
                }

                for (int i = objectsToCleanUp.Count - 1; i >= 0; i--)
                {
                    DestroyImmediate(objectsToCleanUp[i]);
                }
            }

            //            foreach(var obj in GetComponentsInChildren<Transform>(true)) {
            //                if(obj == null) continue;
            //                if(obj.transform.parent != transform) continue;
            //                if(obj.GetInstanceID() != transform.GetInstanceID() && obj.gameObject != null) {
            //#if UNITY_EDITOR
            //                    if(_children != null && !Application.isPlaying) {
            //                        bool isAChild = false;
            //                        for(int i=0; i<_children.Count; ++i) {
            //                            if(_children[i].gameObject.transform.GetInstanceID() == obj.GetInstanceID()) {
            //                                isAChild = true;
            //                                break;
            //                            }
            //                        }
            //                        if(isAChild) {
            //                            DestroyImmediate(obj.gameObject, true);
            //                        }
            //                    }else{
            //                        DestroyImmediate(obj.gameObject, true);
            //                    }
            //#else
            //                    DestroyImmediate(obj.gameObject, true);
            //#endif
            //                }
            //            }

            _children.Clear();
        }

        //........................................................................................................
        /// <summary>
        /// (EDITOR ONLY) Called when we click the Save all + despawn button on main inspector
        /// </summary>
        public void ButtonSaveAndDespawnPreviewRecursive()
        {
            _spawnOnNextEditorUpdate = false;

            IList<PoolObjectSpawner> childSpawners = GetComponentsInChildren<PoolObjectSpawner>(true);
            for (int i = 0; i < childSpawners.Count; ++i)
            {
                try
                {
                    if (childSpawners[i] != null && childSpawners[i].gameObject.GetInstanceID() != this.gameObject.GetInstanceID())
                        childSpawners[i].ButtonSaveAndDespawnPreviewRecursive();
                }
                catch (Exception excep)
                {
                    ReflectionUtil.CheckDangerousException(excep);

                    //if (logger.IsErrorEnabled)
                    //{
                    //    logger.Error("Exception encounted when saving and despawning prefab for " + ElementPrefab + "; ignoring.", excep);
                    //}
                    continue;
                }
            }

            if (Child != null)
            {
                try
                {
                    var newPrefab = _ReplacePrefab(Child.gameObject, ElementPrefab);
                    if (newPrefab != null)
                    {
                    }
                    else
                    {
                        Debug.LogError("Failed to save prefab for " + ElementPrefab);
                    }
                }
                catch (Exception e)
                {
                    ReflectionUtil.CheckDangerousException(e);

                    //if (logger.IsErrorEnabled)
                    //{
                    //    logger.Error("Exception encounted when saving and despawning prefab for " + ElementPrefab + "; ignoring.", e);
                    //}
                }
            }

            _PreviewCleanup();
        }

        private GameObject _ReplacePrefab(GameObject source, GameObject targetObj)
        {
            // spawned children are not for saving, restore that option
            source.hideFlags = HideFlags.None;

            var ret = UnityEditor.PrefabUtility.ReplacePrefab(source, targetObj, UnityEditor.ReplacePrefabOptions.Default);
            return ret;
        }

        //........................................................................................................
        /// <summary>
        /// (EDITOR ONLY) Detect any field changes on update and call appropriate functions
        /// </summary>
        private void OnValidate()
        {
            if (lastElementPrefab != ElementPrefab)
            {
                if (ElementPrefab != null && onSpawn != null)
                {
                    onSpawn.SetTargetOnAllPersistentCalls(ElementPrefab);
                }
                lastElementPrefab = ElementPrefab;
            }
        }
#endif
        public void OnPresaveCleanup()
        {

#if UNITY_EDITOR

            if (!Application.isPlaying)
            {
                if (_autoSaveWhenSavingScene)
                {
                    ButtonSaveAndDespawnPreviewRecursive();
                }
                else
                {
                    ButtonDespawnPreviewWithoutSaving();
                }
            }
            else
            {
                _PreviewCleanup();
            }

#endif
        }

        private void PoolObjectDespawnedElsewhere(PoolObject poolObject)
        {
            if (_children != null)
            {
                for (int i = 0; i < _children.Count; i++)
                {
                    if (_children[i] == null) continue;
                    if (_children[i] != poolObject) continue;

                    _children.RemoveAt(i);
                    return;
                }
            }
        }

        public bool GetIsSpawned()
        {
            return IsSpawned;
        }
    }
}