using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FRG.Core
{
    [AddComponentMenu(""), DisallowMultipleComponent]
    [ServiceOptions(GroupName = "PrefabPool")]
    public class PreseedManager : MonoBehaviour {

        //public List<PreseedInfo> manualList = new List<PreseedInfo>();
        //public List<Transform> ignoreList = new List<Transform>();

        private static bool isPreseeding = false;
        public static bool IsPreseeding
        {
            get
            {
                return isPreseeding;
            }
            private set
            {
                isPreseeding = value;
                //DarkTonic.MasterAudio.EventSounds.IsPreseeding = value; //butodo
            }
        }
        public static bool IsPreseeded { get; private set; }

        //public int totalPreseedScenes = 0;
        //private int totalScenesPreseeded = 0;
        [SerializeField] PreseedSnapshot snapshot = null;

        private List<PreseedSnapshot.PreseedInfo> preseedInfo = new List<PreseedSnapshot.PreseedInfo>();
        private List<PoolRef> preseedObjects = new List<PoolRef>();

        public static PreseedManager Instance { get; private set; }//{ get { return ServiceLocator.ResolveRuntime<PreseedManager>(); } }

#if UNITY_EDITOR
        private static bool editorPrefsLoaded = false;
        private const string DebugPreseedingPreferenceName = "FRG.Core.PreseedManager.DebugPreseeding";
        private static bool debugPreseeding = false;

        public static bool DebugPreseeding
        {
            get
            {
                LoadPreferences();
                return debugPreseeding;
            }
            set {
                if(DebugPreseeding != value) {
                    UnityEditor.EditorPrefs.SetBool(DebugPreseedingPreferenceName, value);
                    debugPreseeding = value;
                }
            }
        }

        private static void LoadPreferences() {
            if(!editorPrefsLoaded) {
                editorPrefsLoaded = true;
                debugPreseeding = UnityEditor.EditorPrefs.GetBool(DebugPreseedingPreferenceName, false);
            }
        }
#endif

        public void AddRuntimePreseed( AssetManagerRef reference, UnityEngine.Object source, int count = 1 ) {
            if (!reference.IsValid) {
                return;
            }
            for (int i = 0; i < preseedInfo.Count; i++) {
                if (preseedInfo[i].reference == reference) {
                    if (preseedInfo[i].numberOfPreseeds < count) {
                        preseedInfo[i].numberOfPreseeds = count;
                    }
                    return;
                }
            }
            preseedInfo.Add(new PreseedSnapshot.PreseedInfo(reference, count, ""));
        }

        void Awake() {
            if(Instance != null)
                Debug.LogError("Multiple PreseedManagers found in scene!");

            DontDestroyOnLoad(gameObject);

            Instance = this;
            Preseed();
        }

        //void Start()
        //{
        //    snapshot = EntryManager.GetSingleton<PreseedSnapshot>();
        //}

        //void Update()
        //{
        //    if (!Application.isEditor || Application.isPlaying) {
        //        return;
        //    }
        //    for (int i = 0; i < manualList.Count; i++) {
        //        if (manualList[i].prefab != null) {
        //            if (manualList[i].prefab.name != manualList[i].name) {
        //                manualList[i].name = manualList[i].prefab.name;
        //            }
        //        }
        //    }
        //}

        //public void ScenePreseeded()
        //{
        //    totalScenesPreseeded++;

        //    if (totalScenesPreseeded >= totalPreseedScenes) {
        //        isPreseeded = true;
        //    }
        //    else {
        //        Util.LoadLevelAdditiveAsync("Preseed_" + (totalScenesPreseeded + 1));
        //    }
        //}

        void OnDisable() {
            if(Instance == this) Instance = null;
        }

        /// <summary>
        /// Start preseed coroutine.
        /// </summary>
        public void RuntimePreseed() {
            // don't take from snapshot, just kick off preseed
            IsPreseeded = false;
            StartCoroutine( DoPreseed() );
        }

        public void Preseed()
        {
            snapshot = PreseedOptions.instance.snapshot;

            IsPreseeded = false;
            preseedInfo.Clear();

            //if (totalPreseedScenes > 0 && Application.isPlaying) {
            //    Util.LoadLevelAdditiveAsync("Preseed_1");
            //    return;
            //}

            if (snapshot != null) {
                preseedInfo.AddRange(snapshot.preseedInfo);

                //for (int i = 0; i < manualList.Count; i++) {
                //    bool found = false;

                //    for (int j = 0; j < preseedInfo.Count; j++) {
                //        if (preseedInfo[j].name == manualList[i].name) {
                //            if (manualList[i].numberOfPreseeds > preseedInfo[j].numberOfPreseeds) {
                //                preseedInfo[j].numberOfPreseeds = manualList[i].numberOfPreseeds;
                //            }

                //            found = true;
                //            break;
                //        }
                //    }

                //    if (!found) {
                //        preseedInfo.Add(manualList[i]);
                //    }
                //}
            }
            //else {
            //    preseedInfo.AddRange(manualList);
            //}

            StartCoroutine(DoPreseed());
        }

        private IEnumerator DoPreseed()
        {

            IsPreseeding = true;
            Vector3 lastPreseedPosition = new Vector3(-10000, -10000, -10000);
            if (preseedInfo != null) {
                List<Rigidbody> bodies = new List<Rigidbody>();
                for (int i = 0; i < preseedInfo.Count; i++) {
                    //bool ignore = false;

                    //for (int j = 0; j < ignoreList.Count; j++) {
                    //    if (preseedInfo[i].name == ignoreList[j].name) {
                    //        ignore = true;
                    //        break;
                    //    }
                    //}

                    //if (ignore) {
                    //    continue;
                    //}

                    //Debug.Log(preseedInfo[i].reference + ": Preseeding " + preseedInfo[i].numberOfPreseeds + " times.");

                    for (int j = 0; j < preseedInfo[i].numberOfPreseeds; j++) {
#if UNITY_EDITOR
                        if (DebugPreseeding)
                        {
                            Debug.Log("Preseeding: " + preseedInfo[i].reference);
                        }
#endif
                        PoolRef poolRef = default(PoolRef);
                        try
                        {
                            poolRef = PoolObject.Preseed(preseedInfo[i].reference, lastPreseedPosition, Quaternion.identity);
                        }
                        catch(Exception e)
                        {
                            //logger.Error( "i = " + i + ", reference:" + preseedInfo[i].reference, e );
                            Debug.Log( "i = " + i + ", reference: " + preseedInfo[i].reference + " " + e );
                            throw;
                        }

                        lastPreseedPosition.x += 10.0f;
                        lastPreseedPosition.y += 10.0f;
                        lastPreseedPosition.z += 10.0f;

                        if (poolRef.gameObject != null) {
                            Rigidbody[] rigidBodies = poolRef.gameObject.GetComponentsInChildren<Rigidbody>();
                            if (rigidBodies != null) {
                                // turn on isKinematic, add to list
                                foreach (Rigidbody body in rigidBodies) {
                                    if (!body.isKinematic) {
                                        body.isKinematic = true;
                                        bodies.Add(body);
                                    }
                                }
                            }
                            preseedObjects.Add(poolRef);
                        }
                    }
                }

                yield return null;

                for (int i = 0; i < bodies.Count; i++) {
                    bodies[i].isKinematic = false;
                }

                for (int i = 0; i < preseedObjects.Count; i++) {
                    preseedObjects[i].Despawn();
                }

                preseedObjects.Clear();

                IsPreseeded = true;
            }
            IsPreseeding = false;
        }

        internal void DumpRuntimePreseed()
        {
            using (Pooled<StringBuilder> pooled = RecyclingPool.SpawnStringBuilder())
            {
                StringBuilder builder = pooled.Value;

                builder.AppendLine("Runtime Preseed:");
                for (int i = 0; i < preseedInfo.Count; i++)
                {
                    builder.AppendLine(preseedInfo[i].reference + ": " + preseedInfo[i].numberOfPreseeds);
                }
                Debug.Log(builder);
            }
        }
    }
}
