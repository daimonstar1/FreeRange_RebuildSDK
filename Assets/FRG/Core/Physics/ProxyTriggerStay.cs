using System;
using UnityEngine;

public class ProxyTriggerStay : MonoBehaviour {

    public event Action<Collider> OnTriggerStayEvent = null;

    void OnTriggerStay(Collider other) {
        if(OnTriggerStayEvent != null) OnTriggerStayEvent(other);
    }
}
