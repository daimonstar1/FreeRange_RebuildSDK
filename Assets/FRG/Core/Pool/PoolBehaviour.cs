using UnityEngine;

namespace FRG.Core
{
    public interface IPoolBehaviour {
        void OnSpawn();
        void OnDespawn();
        void DespawnAfterDelay(float delay);
    }

    public class PoolBehaviour : MonoBehaviour, IPoolBehaviour {
        public virtual void OnSpawn() { }
        public virtual void OnDespawn() { }
        public virtual void DespawnAfterDelay(float delay) { }
    }
}
