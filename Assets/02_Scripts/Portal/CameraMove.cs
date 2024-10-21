using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CameraMove : MonoBehaviour
{
    public Quaternion TargetRotation { private set; get; }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        TargetRotation = transform.rotation;
    }

    public void ResetTargetRotation()
    {
        TargetRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
    }
}
