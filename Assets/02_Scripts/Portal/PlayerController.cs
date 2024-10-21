﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PortalableObject
{
    private CameraMove cameraMove;

    protected override void Awake()
    {
        base.Awake();

        cameraMove = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMove>();
    }

    public override void Warp()
    {
        base.Warp();
        cameraMove.ResetTargetRotation();
    }
}
