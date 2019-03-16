using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace GameTacoSDK
{
    interface ITacoSDK
    {
        /// <summary>
        /// Call this function to init the SDK
        /// </summary>
        void init();
        /// <summary>
        /// Spawn the UI canvas at the first initiation
        /// </summary>
        void spawnCanvas();
        /// <summary>
        /// Take the reuse GameObject from dictionary
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        GameObject getPoolObject(eNumComponentType type);
        /// <summary>
        /// Add the reuse GameObject to dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        void addPoolObject(GameObject obj,eNumComponentType type);
        /// <summary>
        /// Spawn the loading UI at the first initiation
        /// </summary>
        void spawnLoading();
        /// <summary>
        /// Spawn the Top bar
        /// </summary>
        void spawnTopbar();
        /// <summary>
        /// Add a new child to Canvas
        /// </summary>
        void addChild(RectTransform child, AnchorPresets anchor, PivotPresets pivot);
    }
}
