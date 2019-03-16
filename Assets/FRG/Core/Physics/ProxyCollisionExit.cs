using System;
using UnityEngine;

public class ProxyCollisionExit : MonoBehaviour {

    public event Action<Collision> OnCollisionExitEvent = null;

    void OnCollisionExit(Collision collision) {
        if(OnCollisionExitEvent != null) OnCollisionExitEvent(collision);
    }

}
