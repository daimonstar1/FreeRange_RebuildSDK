using UnityEngine;
using UnityEngine.Profiling;

public class PhysicsSynchronizer : MonoBehaviour {

    void Update () {
        Profiler.BeginSample("PhysicsSynchronizer.Update");
        Physics.SyncTransforms();
        Profiler.EndSample();
    }

    void FixedUpdate() {
        Profiler.BeginSample("PhysicsSynchronizer.FixedUpdate");
        Physics.SyncTransforms();
        Profiler.EndSample();
    }
}
