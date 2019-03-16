using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FRG.Core
{
    [DisallowMultipleComponent]
    public class PoolObject : MonoBehaviour
    {
        [Flags]
        public enum SpawnCleanupType
        {
            None = 0,
            ResetAll = ResetParticles | ResetTransforms,

            ResetParticles = 2,
            ResetTransforms = 4,
        }

        internal static PoolRef Spawn(object pre) {
            throw new NotImplementedException();
        }

        [Header("Pool Object")]
        [SerializeField]
        private SpawnCleanupType spawnCleanupType = SpawnCleanupType.ResetAll;
        [SerializeField]
        bool oneShot = false;
        [SerializeField]
        public bool dontReparent = false;
        [SerializeField]
        public bool componentWiseDisable = false;
        [InspectorReadOnly("oneShot"), SerializeField]
        bool inferOneShotTimeFromParticles = false;
        [InspectorReadOnly("CanEditOneShot"), SerializeField]
        float oneShotTime = 0.0f;
        
        public bool hatesScale = true;

        [NonSerialized]
        bool _isPooled = false;
        [NonSerialized]
        int _prefabInstanceId = 0;
        [NonSerialized]
        int _spawnHandle = 0;
        [NonSerialized]
        bool _isOnlyPreseeding = false;

        [NonSerialized]
        bool _hasExpiration;
        [NonSerialized]
        bool _isExpirationTicking;
        [NonSerialized]
        float _expirationStartTime;
        [NonSerialized]
        float _expirationDuration;

        [NonSerialized]
        IPoolBehaviour[] poolBehaviors = ArrayUtil.Empty<IPoolBehaviour>();
        [NonSerialized]
        ISetData[] setDatas = ArrayUtil.Empty<ISetData>();

        [NonSerialized]
        ParticleSystem[] _particleSystems = ArrayUtil.Empty<ParticleSystem>();
        [NonSerialized]
        Default[] _defaults = ArrayUtil.Empty<Default>();

        [NonSerialized]
        protected GameObject prefabGameObject = null;

        [NonSerialized] private int _despawnFrame = -1;

        public event Action<PoolObject> DespawnedAction = null;

        /// <summary>
        /// true if spawned from the prefab pool.
        /// </summary>
        public bool IsPooled { get { return _isPooled; } }

        /// <summary>
        /// Set true before <see cref="OnSpawn"/> and false before <see cref="OnDespawn"/>.  
        /// </summary>
        public bool IsSpawned { get { return (_spawnHandle % 2 == 1); } }

        /// <summary>
        /// Whether the object is spawned and not preseeding and thus should run update monobehaviours.
        /// </summary>
        protected bool ShouldUpdate { get { return IsSpawned && !_isOnlyPreseeding; } }

        /// <summary>
        /// Whether this object is specified to be a one-shot in the inspector.
        /// </summary>
        public bool IsPrefabOneShot { get { return oneShot; } }

        /// <summary>
        /// Whether we're scheduled to expire, either as a one-shot or manually told to expire.
        /// </summary>
        public bool HasExpiration { get { return _hasExpiration; } }
        
        public bool WasDespawnedThisFrame { get { return _despawnFrame == Time.frameCount; } }

        /// <summary>
        /// Whether a new object should suppress pool logic in its awake.
        /// </summary>
        static int _isSpawningCounter = 0;

#region Spawn

        public static PoolRef Spawn(GameObject prefab, Transform parent = null, object data=null)
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity, parent, data);
        }

        public static PoolRef Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, object data=null)
        {
            return SpawnInternal(false, prefab, position, rotation, parent, data);
        }

        public static PoolRef Spawn(AssetManagerRef assetReference, object data=null)
        {
            return Spawn(assetReference, Vector3.zero, Quaternion.identity, null, data);
        }

        public static PoolRef Spawn(AssetManagerRef assetReference, Transform parent, object data=null)
        {
            return Spawn(assetReference, Vector3.zero, Quaternion.identity, parent, data);
        }

        public static PoolRef Spawn(AssetManagerRef assetReference, Vector3 position, Quaternion rotation, Transform parent = null, object data=null)
        {
            GameObject prefab = AssetManager.Get<GameObject>(assetReference);
            return SpawnInternal(false, prefab, position, rotation, parent, data);
        }

        public static PoolRef Preseed(AssetManagerRef assetReference, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = AssetManager.TryGet<GameObject>(assetReference);
            if (prefab == null)
            {
                Debug.LogWarning("Can't preseed " + assetReference);
                return default(PoolRef);
            }
            return SpawnInternal(true, prefab, position, rotation, null);
        }


        private static PoolRef SpawnInternal(bool preseedOnly, GameObject prefabGameObject, Vector3 position, Quaternion rotation, Transform parent, object data=null)
        {
            Debug.Assert(!ReferenceEquals(prefabGameObject, null), "prefab is null", prefabGameObject);
            Debug.Assert(prefabGameObject != null, "prefab has already been destroyed", prefabGameObject);
            Debug.Assert(Application.isPlaying, "can't spawn prefabs unless the game is running", prefabGameObject);

#if UNITY_EDITOR
            Debug.Assert(UnityEditor.PrefabUtility.GetPrefabType(prefabGameObject) == UnityEditor.PrefabType.Prefab, "PoolObject must be spawned from a prefab.");
#endif

            using (ProfileUtil.PushSample("PoolObject.SpawnInternal<T>", prefabGameObject))
            {
                // Fix zero rotation
                if (rotation == default(Quaternion)) { rotation = Quaternion.identity; }

                Pool.PrefabPool pool = Pool.GetOrCreatePool(prefabGameObject);

                PoolObject poolObject;
                try
                {
                    _isSpawningCounter += 1;
                    poolObject = pool.Spawn(prefabGameObject, position, rotation, parent);
                }
                finally
                {
                    _isSpawningCounter -= 1;
                }

                Debug.Assert(poolObject != null, "Failed to spawn PoolObject", prefabGameObject);

                poolObject._isPooled = true;
                poolObject._isOnlyPreseeding = preseedOnly;

                // Preseed while we're guaranteed to be active and in the pool hierarchy
                poolObject.PreseedInstance(prefabGameObject);

                poolObject.SpawnInstance(data);

                PoolRef poolRef = new PoolRef(poolObject, poolObject._spawnHandle);
                Debug.Assert(poolRef.IsSpawned, "New PoolRef should be spawned", prefabGameObject);
                return poolRef;
            }
        }

        private void PreseedInstance(GameObject prefabGameObject)
        {
            int prefabInstanceId = prefabGameObject.GetInstanceID();

            if (prefabInstanceId == 0) { throw new ArgumentException("prefabInstanceId can't be 0.", "prefabInstanceId"); }

            if (this._prefabInstanceId == 0) {
                OnPreseed();

                if ((spawnCleanupType & SpawnCleanupType.ResetTransforms) != 0) {
                    PopulateTransformDefaults();
                }
                if ((spawnCleanupType & SpawnCleanupType.ResetParticles) != 0) {
                    _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
                }

                poolBehaviors = GetComponents<IPoolBehaviour>();

                setDatas = GetComponents<ISetData>();

                _prefabInstanceId = prefabInstanceId;
                this.prefabGameObject = prefabGameObject;
            }
        }

        public bool logSpawning = false;
        private void SpawnInstance(object data=null)
        {

            if(logSpawning) Debug.Log("frame("+Time.frameCount+") SpawnInstance("+gameObject.name+") _spawnHandle("+_spawnHandle+") IsSpawned("+IsSpawned+") instanceId(" + GetInstanceID() + ")");

            if(IsSpawned) {
                Debug.LogError("PoolObject is already spawned. instanceId(" + GetInstanceID() + ")", this);
            }
            
            //call normal OnSpawn
            _spawnHandle += 1;

            if (!_isOnlyPreseeding) {
                ResetInstance();

                if (oneShot) {
                    DespawnAfterDelay(oneShotTime);
                }

                SetSpawnData(data);
                OnSpawn();
            }
        }

        private void PopulateTransformDefaults()
        {
            Transform[] transforms = transform.GetComponentsInChildren<Transform>(true);
            _defaults = new Default[Math.Max(0, transforms.Length - 1)];

            const int Offset = 1;
            for (int i = Offset; i < transforms.Length; ++i)
            {
                Transform trans = transforms[i];

                Default def = new Default();
                def.transform = trans;
                def.activeSelf = trans.gameObject.activeSelf;
                def.position = GetLocalOrAnchoredPosition(trans);
                def.localRotation = trans.localRotation;
                def.localScale = trans.localScale;

                _defaults[i - Offset] = def;
            }
        }

        private void ResetInstance()
        {
            //if we allow resetting particles on spawn
            if ((spawnCleanupType & SpawnCleanupType.ResetParticles) != 0)
            {
                using (ProfileUtil.PushSample("PoolObject.OnPoolSpawn (ResetParticles)", this))
                {
                    foreach (ParticleSystem p in _particleSystems)
                    {
                        if (p == null)
                        {
                            Debug.LogError("null particle system in " + this.name, this);
                        }
                        else
                        {
                            p.Simulate(0.0f, true, true);
                            p.Play();
                        }
                    }
                }
            }

            //if we allow resetting transforms on spawn
            if ((spawnCleanupType & SpawnCleanupType.ResetTransforms) != 0)
            {
                using (ProfileUtil.PushSample("PoolObject.OnPoolSpawn (ResetTransforms)", this))
                {
                    foreach (Default def in _defaults)
                    {
                        Transform trans = def.transform;
                        if (trans != null)
                        {
                            SetLocalOrAnchoredPosition(trans, def.position);
                            trans.gameObject.SetActive(def.activeSelf);
                            trans.localRotation = def.localRotation;
                            trans.localScale = def.localScale;
                        }
                    }
                }
            }
        }

        internal static void UpdateOneShotExpiration(PoolObject poolObject)
        {
            if (!ReferenceEquals(poolObject, null) && poolObject.IsSpawned && poolObject._isExpirationTicking)
            {
                float elapsed = Time.time - poolObject._expirationStartTime;
                if (elapsed >= poolObject._expirationDuration)
                {
                    poolObject.Despawn();
                }
            }
        }

        // Animation events call this... apparently anim events can't handle default params
        // so this must be a separate function.
        public void Despawn()
        {
            Despawn(true, true);
        }

        /// <summary>
        /// Despawn the pool object
        /// </summary>
        /// <param name="disableGameObject">Whether to disable the object upon moving it back to the pool</param>
        /// <param name="moveGameObject">Whether to reset the object's transform parent to the pool's transform</param>
        public void Despawn(bool disableGameObject, bool moveGameObject)
        {
            bool wasPreseeding = _isOnlyPreseeding;

            if (IsSpawned)
            {
                _spawnHandle += 1;
                _isOnlyPreseeding = false;
                _hasExpiration = oneShot;
                _isExpirationTicking = false;
                _expirationStartTime = 0;
                _expirationDuration = 0;
                _despawnFrame = Time.frameCount;

                Pool.DeregisterOneShot(this);

                if (!wasPreseeding)
                {
                    // NOTE: Should we call this if we're getting destroyed?
                    OnDespawn();
                }

                if(logSpawning) {
                    Debug.Log("frame("+Time.frameCount+") PoolObject("+gameObject.name+") Despawn");
                }

                Pool.PrefabPool pool = null;
                if (IsPooled && _prefabInstanceId != 0)
                {
                    if (Pool.TryGetPool(_prefabInstanceId, out pool))
                    {
                        if(DespawnedAction != null) DespawnedAction(this);

                        pool.Despawn(this);
                        
                        // If not destroyed, deactivate and move it
                        if (this != null && (!Application.isPlaying || !FocusHandler.IsShuttingDown))
                        {

                            if (moveGameObject && !dontReparent) {
                                if (disableGameObject) {
                                    if (componentWiseDisable)
                                    {
                                        transform.SetParent(pool.ComponentWiseInactiveRoot, false);
                                        ToggleComponents(false);
                                    }
                                    else
                                        transform.SetParent(pool.InactiveRoot, false);
                                }
                                else {
                                    transform.SetParent(pool.ActiveRoot, false);
                                }
                            }
                            else if (disableGameObject) {
                                if (componentWiseDisable)
                                    ToggleComponents(false);
                                else
                                    gameObject.SetActive(false);
                            }
                        }
                    }
                }

                // If this was a scene game object that isn't destroyed, despawn should destroy
                if (pool == null && this != null && (!Application.isPlaying || !FocusHandler.IsShuttingDown))
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Toggles the enable state of all mono behaviours, renderers and rigidbodies on this object and any children.
        /// </summary>
        public void ToggleComponents(bool enable)
        {
            MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>();
            foreach (var entry in behaviours)
                entry.enabled = enable;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var entry in renderers)
                entry.enabled = enable;

            Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
            foreach (var entry in rigidbodies)
                entry.detectCollisions = enable;
        }

#endregion

#region Timed Despawn (OneShot)

        /// <summary>
        /// Resume the one-shot timer, starting where last paused.
        /// No-op if despawned or already active.
        /// </summary>
        public void ResumeOneShotTimer()
        {
            if (IsSpawned && !_isExpirationTicking)
            {
                DespawnAfterDelay(_expirationDuration);
            }
        }

        /// <summary>
        /// Restarts the one-shot timer to the full amount specified by the prefab.
        /// No-op if despawned.
        /// </summary>
        public void RestartOneShotTimer()
        {
            DespawnAfterDelay(oneShotTime);
        }

        /// <summary>
        /// Mark this object as a one-shot and despawn after the specified delay.
        /// No-op if despawned. Will be properly reset on despawn.
        /// </summary>
        public void DespawnAfterDelay(float delay)
        {
            if (IsSpawned)
            {
                _hasExpiration = true;
                _isExpirationTicking = true;
                _expirationStartTime = Time.time;
                _expirationDuration = delay;
                Pool.RegisterOneShot(this);

                foreach (var behaviour in poolBehaviors) {
                    if (behaviour == null) continue;
                    behaviour.DespawnAfterDelay(delay);
                }
            }
        }

        /// <summary>
        /// Stop the one-shot timer, maintaining the countdown.
        /// No-op if despawned or already paused.
        /// </summary>
        public void PauseOneShotTimer()
        {
            if (IsSpawned && _isExpirationTicking)
            {
                _isExpirationTicking = false;

                float elapsed = Time.time - _expirationStartTime;
                _expirationDuration = _expirationDuration - elapsed;
                _expirationStartTime = 0;
            }
        }

        internal static bool IsRefSpawned(PoolObject poolObject, int spawnHandle)
        {
            return (poolObject != null && spawnHandle == poolObject._spawnHandle);
        }

        internal static void DespawnRef(PoolObject poolObject, int spawnHandle, bool disableGameObject, bool moveGameObject)
        {
            if (poolObject != null && spawnHandle == poolObject._spawnHandle)
            {
                poolObject.Despawn(disableGameObject, moveGameObject);
            }
        }

#endregion

#region MonoBehaviours/Callbacks

#if UNITY_EDITOR
        [Obsolete("Use OnPreseed instead.", true)]
        protected virtual void Start() { }
#endif

        /// <summary>
        /// Do not override. Use OnPreseed instead.
        /// </summary>
        protected void Awake()
        {
            if (_isSpawningCounter == 0 && _spawnHandle == 0)
            {
                _isPooled = false;
                PreseedInstance(gameObject);
                SpawnInstance();
            }
        }

        /// <summary>
        /// Do not override. Use OnDespawn instead.
        /// </summary>
        protected void OnDestroy()
        {
            //Debug.LogWarning("PoolObjects should not be destroyed. Object name: " + name);
            Despawn(false, false);
        }

        protected virtual void OnValidate()
        {
            if (oneShot && inferOneShotTimeFromParticles)
            {
                float computedTime = 0.0f;
                foreach (ParticleSystem p in GetComponentsInChildren<ParticleSystem>(true)) {
                    float startDelay = ComputeMinMaxCurveUpperBound(p.main.startDelay);
                    float startLifetime = ComputeMinMaxCurveUpperBound(p.main.startLifetime);

                    float time = startDelay + p.main.duration + startLifetime;

                    computedTime = Mathf.Max(computedTime, time);
                }
                oneShotTime = computedTime;
            }

            //foreach (EventSounds sounds in transform.GetComponentsInChildren<EventSounds>(true))
            //{
            //    if (!sounds.IsPooled)
            //    {
            //        sounds.IsPooled = true;
            //    }
            //}
        }

        private static float ComputeMinMaxCurveUpperBound(ParticleSystem.MinMaxCurve curve)
        {
            float upperBound;
            switch (curve.mode) {
                case ParticleSystemCurveMode.Constant:
                case ParticleSystemCurveMode.Curve:
                    upperBound = curve.Evaluate(1);
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                case ParticleSystemCurveMode.TwoCurves:
                    upperBound = curve.Evaluate(1, 1);
                    break;

                default:
                    Debug.Assert(false, "Unknown curve mode.");
                    upperBound = 0;
                    break;
            }

            return upperBound;
        }

        /// <summary>
        /// Called only once in a <see cref="PoolObject"/> lifetime.
        /// Similar to <see cref="Awake"/>. 
        /// </summary>
        protected virtual void OnPreseed()
        {
        }

        /// <summary>
        /// Called every time the <see cref="PoolObject"/> spawns. 
        /// </summary>
        protected virtual void OnSpawn()
        {


            ///------------------------------------------------------------------------------------------------------------------------
            ///HACK
            ///[bh 03-06-2017]: As of Unity 5.6.0, animators are not having their state machine data reset upon OnEnable() as they usually do,
            ///   so they will stay in the state/time which they were when they were disabled. This will break everything that relies on Entry.
            ///   Re-setting the runtime animator controller seems to fix this for now.
            foreach(var animator in GetComponentsInChildren<Animator>(true)) {
                if(animator != null) {
                    var contr = animator.runtimeAnimatorController;
                    //animator.runtimeAnimatorController = null;
                    //animator.runtimeAnimatorController = contr;
                }
            }
            ///------------------------------------------------------------------------------------------------------------------------


            foreach (var behaviour in poolBehaviors) {
                if (behaviour == null) continue;
                behaviour.OnSpawn();
            }

        }

        /// <summary>
        /// Called every time the <see cref="PoolObject"/> despawns. 
        /// </summary>
        protected virtual void OnDespawn()
        {
            foreach (var behaviour in poolBehaviors) {
                if (behaviour == null) continue;
                behaviour.OnDespawn();
            }
        }

        private bool CanEditOneShot()
        {
            return oneShot && !inferOneShotTimeFromParticles;
        }

#endregion

#region UI Helpers
        
        private static Vector3 GetLocalOrAnchoredPosition(Transform transform)
        {
            if (transform == null) { throw new InvalidOperationException("Transform is destroyed."); }

            RectTransform rt = transform as RectTransform;
            if (!ReferenceEquals(rt, null))
            {

                if(float.IsNaN(rt.anchoredPosition3D.x)) {
                    Debug.LogError("PoolObject("+rt.gameObject.name+") GetLocalOrAnchoredPosition rt.anchoredPosition3D.x is NaN!");
                }

                if(float.IsNaN(rt.anchoredPosition3D.y)) {
                    Debug.LogError("PoolObject("+rt.gameObject.name+") GetLocalOrAnchoredPosition rt.anchoredPosition3D.y is NaN!");
                }

                if(float.IsNaN(rt.anchoredPosition3D.z)) {
                    Debug.LogError("PoolObject("+rt.gameObject.name+") GetLocalOrAnchoredPosition rt.anchoredPosition3D.z is NaN!");
                }

                return rt.anchoredPosition3D;
            }
            else
            {
                return transform.localPosition;
            }
        }

        private static void SetLocalOrAnchoredPosition(Transform transform, Vector3 value)
        {
            if (transform == null) { throw new InvalidOperationException("Transform is destroyed."); }

            RectTransform rt = transform as RectTransform;
            if (!ReferenceEquals(rt, null))
            {
                rt.anchoredPosition3D = value;
            }
            else
            {
                transform.localPosition = value;
            }
        }

        /// <summary>
        /// Set this object's data of unknown type. May use reflection to find a SetData() method accepting data_'s real type.
        /// </summary>
        public void SetSpawnData(object data)
        {
            if(setDatas == null) return;

            for(int i=0;i<setDatas.Length;i++) {
                setDatas[i].SetData(data);
            }
        }

#endregion

#region Implementation Types

        [StructLayout(LayoutKind.Auto)]
        private struct Default
        {
            public Transform transform;
            public bool activeSelf;
            public Vector3 position;
            public Quaternion localRotation;
            public Vector3 localScale;
        }

        private static class DataStatics
        {
            public static readonly Dictionary<ReflectionUtil.TypePair, Action<object,object>> SetDataMemoization = new Dictionary<ReflectionUtil.TypePair, Action<object,object>>();
            public static readonly object[] SingleParameterArray = new object[1];
        }

#endregion

#if UNITY_EDITOR

        static List<PoolObject> _poolObjectList;

        public static void WarnIfInvalid(GameObject root)
        {
            if (_poolObjectList == null) { _poolObjectList = new List<PoolObject>(); }
            _poolObjectList.Clear();

            root.GetComponentsInChildren(true, _poolObjectList);
            if (_poolObjectList.Count != 0)
            {
                if (_poolObjectList.Count != 1)
                {
                    Debug.LogError("There should be only one PoolObject on a Prefab.\n\t" + ArrayUtil.Join("\n\t", _poolObjectList, po => Util.GetObjectPath(po)) + "\nYou can remove any unneeded components to fix this error.", root);
                }
                else if (root.scene.IsValid())
                {
                    if (_poolObjectList[0].enabled)
                    {
                        // Kova TODO new way of keeping children of spawners during save spams this
                        //Debug.LogError("PoolObjects will not work correctly in a scene and should be removed or disabled.\n\t" + ArrayUtil.Join("\n\t", _poolObjectList, po => Util.GetObjectPath(po)) + "\nYou can disable or remove the pool object to fix this error.", root);
                    }
                }
                else if (_poolObjectList[0].gameObject != root)
                {
                    Debug.LogError("The PoolObject script should only be on the root of a Prefab.\n\t" + ArrayUtil.Join("\n\t", _poolObjectList, po => Util.GetObjectPath(po)) + "\nYou can remove the unneeded component to fix this error.", root);
                }
                else if (_poolObjectList[0].oneShot && _poolObjectList[0].oneShotTime < .01f)
                {
                    Debug.LogError("A one-shot PoolObject has a very small expiration time " + _poolObjectList[0].oneShotTime.ToString("0.000") + ":\n\t" + ArrayUtil.Join("\n\t", _poolObjectList, po => Util.GetObjectPath(po)) + "\nYou can turn off one-shot or correct the time to fix this error.", root);
                }
                else if (_poolObjectList[0].oneShot && _poolObjectList[0].oneShotTime >= 60 * 60)
                {
                    Debug.LogError("A one-shot PoolObject has an infinite or very large expiration time" + _poolObjectList[0].oneShotTime.ToString("0.000") + "\n\t" + ArrayUtil.Join("\n\t", _poolObjectList, po => Util.GetObjectPath(po)) + "\nYou can turn off one-shot or correct the time to fix this error.", root);
                }
            }
        }

#endif
    }
}
