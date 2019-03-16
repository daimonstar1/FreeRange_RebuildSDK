using UnityEngine;
using System;

[Serializable]
public class RotateObject : MonoBehaviour {

    public float xRate = 0f;
    public float yRate = 0f;
    public float zRate = 0f;
    public bool useLocalEulerAngles = true;

    void Update() {

        float dt = Time.deltaTime;

        Vector3 eulers = useLocalEulerAngles ? transform.localEulerAngles : transform.eulerAngles;

        eulers.x += xRate * dt;
        eulers.y += yRate * dt;
        eulers.z += zRate * dt;

        if(useLocalEulerAngles) {
            transform.localEulerAngles = eulers;
        }
        else {
            transform.eulerAngles = eulers;
        }

    }

}
