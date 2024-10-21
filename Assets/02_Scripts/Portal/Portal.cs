using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    [SerializeField] private LayerMask placementMask;
    [SerializeField] private Transform testTransform;
    [SerializeField] private GameObject innerObject;

    [field: SerializeField] public Portal OtherPortal { get; private set; }
    [field: SerializeField] public Color PortalColour { get; private set; }

    private Wall wall;
    private Animator animator;
    private Collider wallCollider;
    private new BoxCollider collider;
    private List<PortalableObject> portalObjects = new List<PortalableObject>();

    public bool IsPlaced { get; private set; } = false;
    public Renderer Renderer { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<BoxCollider>();
        Renderer = GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        Renderer.enabled = OtherPortal.IsPlaced;

        for (int i = 0; i < portalObjects.Count; ++i)
        {
            Vector3 objPos = transform.InverseTransformPoint(portalObjects[i].transform.position);

            if (objPos.z > 0.0f)
            {
                portalObjects[i].Warp();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var obj = other.GetComponent<PortalableObject>();
        if (obj != null)
        {
            portalObjects.Add(obj);
            obj.SetIsInPortal(this, OtherPortal, wallCollider);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var obj = other.GetComponent<PortalableObject>();

        if (portalObjects.Contains(obj))
        {
            portalObjects.Remove(obj);
            obj.ExitPortal(wallCollider);
        }
    }

    public bool PlacePortal(Collider wallCollider, Vector3 pos, Quaternion rot)
    {
        testTransform.position = pos;
        testTransform.rotation = rot;
        testTransform.position -= testTransform.forward * 0.001f;

        wall = wallCollider.GetComponent<Wall>();

        FixOverhangs(wallCollider);
        FixIntersects();


        if (CheckOverlap())
        {
            gameObject.SetActive(false);
            this.wallCollider = wallCollider;
            transform.position = testTransform.position;
            transform.rotation = testTransform.rotation;

            IsPlaced = true;

            if (OtherPortal.IsPlaced && IsPlaced)
            {
                innerObject.SetActive(false);
                OtherPortal.innerObject.SetActive(false);
            }

            gameObject.SetActive(true);
            return true;
        }

        return false;
    }

    private void FixOverhangs(Collider wallCollider)
    {
        var testPoints = new List<Vector3>
        {
            new Vector3(-1.1f,  0.0f, 0.1f),
            new Vector3( 1.1f,  0.0f, 0.1f),
            new Vector3( 0.0f, -2.0f, 0.1f),
            new Vector3( 0.0f,  2.1f, 0.1f)
        };

        var testDirs = new List<Vector3>
        {
             Vector3.right,
            -Vector3.right,
             Vector3.up,
            -Vector3.up
        };

        for (int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = testTransform.TransformPoint(testPoints[i]);
            Vector3 raycastDir = testTransform.TransformDirection(testDirs[i]);


            if (Physics.CheckSphere(raycastPos, 0.001f, placementMask))
            {
                HashSet<string> collidersNameSet = ProcessingColldierArray(Physics.OverlapSphere(raycastPos, 0.001f, placementMask));


                foreach (var name in collidersNameSet)
                {
                    Debug.Log(wallCollider.gameObject.name);
                    Debug.Log(name);

                    if (name != wallCollider.gameObject.name && Physics.Raycast(raycastPos, raycastDir, out hit, 2.0f, placementMask))//2.1f, placementMask))
                    {
                        var offset = hit.point - raycastPos;

                        if (offset.magnitude < 2.0f)
                        {
                            testTransform.Translate(offset, Space.World);
                        }
                    }
                }
                continue;
            }
            else if (Physics.Raycast(raycastPos, raycastDir, out hit, 2.0f, placementMask))//2.1f, placementMask))
            {
                var offset = hit.point - raycastPos;

                testTransform.Translate(offset, Space.World);
            }
        }
    }

    private void FixIntersects()
    {
        float x = -1.1f, y = 2.1f;
        var testDirs = new List<Vector3>
        {
             Vector3.right,
            -Vector3.right,
             Vector3.up,
            -Vector3.up
        };

        var testDists = new List<float> { x, x, y, y };//{ 0.11f, 0.11f, 0.21f, 0.21f };

        for (int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = transform.TransformPoint(0.0f, 0.0f, -0.1f);
            Vector3 raycastDir = transform.TransformDirection(testDirs[i]);

            if (Physics.Raycast(raycastPos, raycastDir, out hit, testDists[i], placementMask))
            {
                var offset = hit.point - raycastPos;
                var newOffset = -raycastDir * (testDists[i] - offset.magnitude);
                transform.Translate(newOffset, Space.World);
            }
        }
    }

    private bool CheckOverlap()
    {
        float x = 1f, y = 2f;
        var checkExtents = new Vector3(0.9f, 1.9f, 0.05f);
        var checkPositions = new Vector3[]
        {
            testTransform.position + testTransform.TransformVector(new Vector3( 0.0f,  0.0f, -0.1f)),

            testTransform.position + testTransform.TransformVector(new Vector3(-x, -y, -0.1f)),
            testTransform.position + testTransform.TransformVector(new Vector3(-x,  y, -0.1f)),
            testTransform.position + testTransform.TransformVector(new Vector3( x, -y, -0.1f )),
            testTransform.position + testTransform.TransformVector(new Vector3(x, y, -0.1f )),

            testTransform.TransformVector(new Vector3(0.0f, 0.0f, 0.2f))
        };

        var intersections = Physics.OverlapBox(checkPositions[0], checkExtents, testTransform.rotation, placementMask);
        if (intersections.Length > 4)
        {
            return false;
        }
        else if (intersections.Length == 4)
        {
            if (intersections[0] != collider)
            {
                return false;
            }
        }

        bool isOverlapping = true;

        return isOverlapping;
    }

    private HashSet<string> ProcessingColldierArray(Collider[] colliders)
    {
        HashSet<string> tmpHashSet = new HashSet<string>();

        for (int i = 0; i < colliders.Length; i++)
        {
            tmpHashSet.Add(colliders[i].gameObject.name);
        }

        return tmpHashSet;
    }

    public void RemovePortal()
    {
        gameObject.SetActive(false);
        IsPlaced = false;
    }
}