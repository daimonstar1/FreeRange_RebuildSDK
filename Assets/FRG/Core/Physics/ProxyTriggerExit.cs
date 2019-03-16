using System;
using UnityEngine;

public class ProxyTriggerExit : MonoBehaviour {

    public event Action<Collider> OnTriggerExitEvent = null;

    void OnTriggerExit(Collider other) {
        if(OnTriggerExitEvent != null) OnTriggerExitEvent(other);
    }
}
