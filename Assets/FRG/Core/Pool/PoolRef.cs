using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FRG.Core
{
    [StructLayout(LayoutKind.Auto)]
    public struct PoolRef
    {
        [NonSerialized]
        readonly PoolObject _poolObject;
        [NonSerialized]
        internal readonly int SpawnHandle;

        internal PoolRef(PoolObject poolObject, int spawnHandle)
        {
            _poolObject = poolObject;
            SpawnHandle = spawnHandle;
        }

        public bool IsSpawned
        {
            get
            {
                return PoolObject.IsRefSpawned(_poolObject, SpawnHandle);
            }
        }

        public GameObject gameObject
        {
            get
            {
                //if (!IsSpawned) throw new InvalidOperationException("Cannot get gameObject of despawned.");
                return IsSpawned ? _poolObject.gameObject : null;
            }
        }

        public Transform transform
        {
            get
            {
                //if (!IsSpawned) throw new InvalidOperationException("Cannot get transform of despawned.");
                return IsSpawned ? _poolObject.transform : null;
            }
        }

        public T GetPoolObject<T>()
            where T : PoolObject
        {
            return IsSpawned ? _poolObject as T : null;
        }

        public void Despawn()
        {
            Despawn(true, true);
        }

        public void Despawn(bool disableGameObject, bool moveGameObject)
        {
            PoolObject.DespawnRef(_poolObject, SpawnHandle, disableGameObject, moveGameObject);
        }
        public override string ToString()
        {
            return "PoolRef(\"" + (IsSpawned ? _poolObject.ToString() : "") + "\")";
        }
    }
}
