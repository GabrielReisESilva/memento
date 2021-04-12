using System.Collections;
using System.Collections.Generic;
using Geogram;
using UnityEngine;

public class MementoParent : SceneObject
{
    // Start is called before the first frame update
    protected override void Start()
    {
        radius = Utils.SCENE_CIRCLE_RADIUS;
        base.Start();
    }

    protected override void Update()
    {

    }
}
