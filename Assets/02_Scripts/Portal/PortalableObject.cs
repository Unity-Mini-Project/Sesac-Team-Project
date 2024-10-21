using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PortalableObject : MonoBehaviour
{
    private Portal inPortal;
    private Portal outPortal;

    private Vector3 transformOffset;

    private new Rigidbody rigidbody;
    protected new Collider collider;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;
    [SerializeField] private Transform cameraOffsetTr;

    private static readonly Quaternion halfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);

    protected virtual void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    private void LateUpdate()
    {
        if (inPortal == null || outPortal == null)
        {
            return;
        }

        if (transform.up != Vector3.up)
        {
            // XR Origin의 Up 방향을 세계의 Up 방향으로 맞춤
            GetComponent<XROrigin>().MatchOriginUp(Vector3.up);

            if (transform.forward == Vector3.up)
            {
                // 카메라의 Forward를 특정 방향으로 맞춤 (예: 포탈 방향)
                GetComponent<XROrigin>().MatchOriginUpCameraForward(Vector3.up, -Vector3.forward);
            }
            else
            {
                Vector3 oppositeDirection = -GetComponent<XROrigin>().Camera.transform.forward;
                // 카메라의 Forward를 특정 방향으로 맞춤 (예: 포탈 방향)
                GetComponent<XROrigin>().MatchOriginUpCameraForward(Vector3.up, -oppositeDirection);
            }
        }
    }

    public void SetIsInPortal(Portal inPortal, Portal outPortal, Collider wallCollider)
    {
        this.inPortal = inPortal;
        this.outPortal = outPortal;

        if (inPortal.IsPlaced && outPortal.IsPlaced)
        {
            Physics.IgnoreCollision(collider, wallCollider);
        }
    }

    public void ExitPortal(Collider wallCollider)
    {
        if (inPortal.IsPlaced && outPortal.IsPlaced)
        {
            Physics.IgnoreCollision(collider, wallCollider, false);
        }
    }

    public virtual void Warp()
    {
        var inTransform = inPortal.transform;
        var outTransform = outPortal.transform;

        if (inPortal.IsPlaced && outPortal.IsPlaced)
        {
            // Update position of object.
            Vector3 relativePos = inTransform.InverseTransformPoint(transform.position);
            relativePos = halfTurn * relativePos;

            if (outTransform.transform.up == Vector3.up || -outTransform.transform.forward == Vector3.down)
            {
                transform.position = outTransform.TransformPoint(relativePos);
            }
            else
            {
                transform.position = outTransform.TransformPoint(relativePos) + new Vector3(0.0f, 2.0f, -2.0f);
            }

            // Update rotation of object.
            Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * transform.rotation;
            relativeRot = halfTurn * relativeRot;
            transform.rotation = outTransform.rotation * relativeRot;

            // Update velocity of rigidbody.
            Vector3 relativeVel = inTransform.InverseTransformDirection(rigidbody.velocity);
            relativeVel = halfTurn * relativeVel;
            rigidbody.velocity = outTransform.TransformDirection(relativeVel);

            // Swap portal references.
            var tmp = inPortal;
            inPortal = outPortal;
            outPortal = tmp;
        }
    }
}
