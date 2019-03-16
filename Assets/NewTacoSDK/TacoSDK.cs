using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameTacoSDK
{
    public sealed class TacoSDK : Singleton<TacoSDK>, ITacoSDK
    {
        /// <summary>
        /// This dictionary store the reuse GameObject
        /// </summary>
        private static Dictionary<eNumComponentType, GameObject> pool_objects;
        /// <summary>
        /// Call this function to init the SDK
        /// </summary>
        public void init()
        {
            if (pool_objects == null)
                pool_objects = new Dictionary<eNumComponentType, GameObject>();
            else
                return;
            ((ITacoSDK)this).spawnCanvas();
            ((ITacoSDK)this).spawnLoading();
            ((ITacoSDK)this).spawnTopbar();
        }
        /// <summary>
        /// Add the reuse GameObject to dictionary
        /// </summary>
        /// <param name="type"></param>
        void ITacoSDK.addPoolObject(GameObject obj, eNumComponentType type)
        {
            if (pool_objects == null)
            {
                Debug.LogError("Please call Init() first!");
                return;
            }
            if (!pool_objects.ContainsKey(type))
                pool_objects.Add(type, obj);
        }
        /// <summary>
        /// Take the reuse GameObject from dictionary
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public GameObject getPoolObject(eNumComponentType type)
        {
            if (pool_objects == null)
            {
                Debug.LogError("Please call Init() first!");
                return null;
            }
            else if (pool_objects.ContainsKey(type))
                return pool_objects[type];
            else
                Debug.LogError("The GameObject not found in dictionary.Type is "+ type.ToString());
            return null;
        }
        /// <summary>
        /// Spawn the UI canvas at the first initiation
        /// </summary>
        void ITacoSDK.spawnCanvas()
        {
            GameObject _canvas = Resources.Load<GameObject>("Prefabs/tacosdk");
            if (_canvas == null)
            {
                Debug.LogError("Cannot find tacosdk prefabs!. Did you change prefab name?. If you want to know how to customize UI please visit this link " + TacoConstant.REPERENCE_DOCUMENT_LINK);
                return;
            }
            GameObject canvas = UnityEngine.Object.Instantiate(_canvas);
            canvas.transform.position = Vector3.zero;
            canvas.AddComponent<TacoUICanvas>();
            canvas.name = "TacoSDK";
            ((ITacoSDK)this).addPoolObject(canvas,eNumComponentType.OBJECT_CANVAS);
        }
        void ITacoSDK.spawnLoading()
        {
            GameObject _loading = Resources.Load<GameObject>("Prefabs/loading");
            if (_loading == null)
            {
                Debug.LogError("Cannot find loading prefabs!. Did you change prefab name?. If you want to know how to customize UI please visit this link " + TacoConstant.REPERENCE_DOCUMENT_LINK);
                return;
            }
            GameObject loading = UnityEngine.Object.Instantiate(_loading);
            loading.name = "loading";
            loading.transform.parent = getPoolObject(eNumComponentType.OBJECT_CANVAS).transform;
            ((ITacoSDK)this).addPoolObject(loading, eNumComponentType.OBJECT_LOADING);
            loading.transform.position = Vector3.zero;
            loading.transform.localPosition = Vector3.zero;
            loading.transform.localScale = Vector3.one;
            View.SetAnchor(loading.GetComponent<RectTransform>(), AnchorPresets.StretchAll);
            View.SetPivot(loading.GetComponent<RectTransform>(), PivotPresets.MiddleCenter);
            UnityEngine.Object.DontDestroyOnLoad(loading);
        }

        void ITacoSDK.spawnTopbar()
        {
            GameObject _prefabs = Resources.Load<GameObject>("Prefabs/topbar");
            if (_prefabs == null)
            {
                Debug.LogError("Cannot find topbar prefabs!. Did you change prefab name?. If you want to know how to customize UI please visit this link " + TacoConstant.REPERENCE_DOCUMENT_LINK);
                return;
            }
            GameObject clone = UnityEngine.Object.Instantiate(_prefabs);
            clone.name = "topbar";
            clone.transform.parent = getPoolObject(eNumComponentType.OBJECT_CANVAS).transform;
            ((ITacoSDK)this).addPoolObject(clone, eNumComponentType.OBJECT_TOPBAR);
            ((ITacoSDK)this).addChild(clone.GetComponent<RectTransform>(), AnchorPresets.HorStretchTop, PivotPresets.TopLeft);
            clone.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 150);
            clone.AddComponent<TacoTopbarView>();
            UnityEngine.Object.DontDestroyOnLoad(clone);
        }
        /// <summary>
        /// Add a new child to Canvas
        /// </summary>
        void ITacoSDK.addChild(RectTransform child, AnchorPresets anchor, PivotPresets pivot)
        {
            child.transform.position = Vector3.zero;
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;
            View.SetAnchor(child, anchor);
            View.SetPivot(child, pivot);
            getPoolObject(eNumComponentType.OBJECT_LOADING).transform.SetAsLastSibling();
        }
    }
}
