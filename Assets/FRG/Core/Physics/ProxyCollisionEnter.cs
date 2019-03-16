using System;
using UnityEngine;

public class ProxyCollisionEnter : MonoBehaviour {

    public event Action<Collision> OnCollisionEnterEvent = null;

    public Collider myCollider { get; private set; }

    void Awake() {
        myCollider = GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision collision) {
        if(OnCollisionEnterEvent != null) OnCollisionEnterEvent(collision);
    }

}
