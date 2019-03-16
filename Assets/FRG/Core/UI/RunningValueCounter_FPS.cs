using UnityEngine;

namespace FRG.Core {
    public class RunningValueCounter_FPS : RunningValueCounter {

        public static RunningValueCounter_FPS Instance { get; private set; }

        float lastStamp = -1f;
        const float dummyDelta = 0.01f;

        void Awake() {
            Instance = this;
        }

        void OnEnable() {
            lastStamp = -1f;
        }

        protected override float RefreshValue() {
            float newStamp = Time.realtimeSinceStartup;
            float delta = dummyDelta;
            if(lastStamp >= 0f) {
                delta = newStamp - lastStamp;
            }

            lastStamp = newStamp;
            return delta > 0f ? (1f / delta) : 1f;
        }

        void OnDestroy() {
            if(Instance == this) Instance = null;
        }
    }
}

