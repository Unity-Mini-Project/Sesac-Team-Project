using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(VRMovement))]
public class PortalPlacement : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Portal portalA;
    [SerializeField] private Portal portalB;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private InputActionProperty firePortalA;
    [SerializeField] private InputActionProperty firePortalB;

    private XRRayInteractor leftInteractor;
    private XRRayInteractor rightInteractor;

    private void Awake()
    {
        leftInteractor = GameObject.FindGameObjectWithTag("Left Hand").GetComponent<XRRayInteractor>();
        rightInteractor = GameObject.FindGameObjectWithTag("Right Hand").GetComponent<XRRayInteractor>();
    }

    private void OnEnable()
    {
        firePortalA.action.performed += _ =>
        {
            FirePortal(0, leftInteractor.transform.position, leftInteractor.transform.forward, 500.0f);
        };

        firePortalB.action.performed += _ =>
        {
            FirePortal(1, rightInteractor.transform.position, rightInteractor.transform.forward, 500.0f);
        };
    }

    private void FirePortal(int portalID, Vector3 pos, Vector3 dir, float distance)
    {
        RaycastHit hit;

        // if (portalID == 0) leftInteractor.TryGetCurrent3DRaycastHit(out hit);
        // else rightInteractor.TryGetCurrent3DRaycastHit(out hit);

        if (portalID == 0) Physics.Raycast(pos, dir, out hit, distance, layerMask);
        else Physics.Raycast(pos, dir, out hit, distance, layerMask);

        if (hit.collider != null && hit.collider.tag == "Wall")
        {
            var cameraRotation = mainCamera.transform.rotation;
            var portalRight = cameraRotation * Vector3.right;

            if (Mathf.Abs(portalRight.x) >= Mathf.Abs(portalRight.z))
            {
                portalRight = (portalRight.x >= 0) ? Vector3.right : -Vector3.right;
            }
            else
            {
                portalRight = (portalRight.z >= 0) ? Vector3.forward : -Vector3.forward;
            }

            var portalForward = -hit.normal;
            var portalUp = -Vector3.Cross(portalRight, portalForward);

            var portalRotation = Quaternion.LookRotation(portalForward, portalUp);
            if (portalID == 0) portalA.PlacePortal(hit.collider, hit.point, portalRotation);
            else portalB.PlacePortal(hit.collider, hit.point, portalRotation);
        }
    }
}