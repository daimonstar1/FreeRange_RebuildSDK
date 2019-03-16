using System;
using UnityEngine;

public class ProxyCollisionStay : MonoBehaviour {

    public event Action<Collision> OnCollisionStayEvent = null;

    void OnCollisionStay(Collision collision) {
        if(OnCollisionStayEvent != null) OnCollisionStayEvent(collision);
    }

}
