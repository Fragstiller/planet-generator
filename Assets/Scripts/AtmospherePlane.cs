using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmospherePlane : MonoBehaviour
{
    public Camera followCamera;

    void LateUpdate()
    {
        transform.LookAt(transform.position + followCamera.transform.rotation * Vector3.back, followCamera.transform.rotation * Vector3.up);
    }
}
