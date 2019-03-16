using System;
using UnityEngine;

public class ProxyTriggerEnter : MonoBehaviour {

    public event Action<Collider> OnTriggerEnterEvent = null;
    public Collider myCollider { get; private set; }

    void Awake() {
        myCollider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other) {
        if(OnTriggerEnterEvent != null) OnTriggerEnterEvent(other);
    }

}
