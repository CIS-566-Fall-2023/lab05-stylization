using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DitherByCameraDistance : MonoBehaviour
{
    public float MinDistance;
    public float MaxDistance;
    public float DitherSize;


    // Update is called once per frame
    void Update()
    {

        var distance = (Camera.main.transform.position - transform.position).magnitude;
        var distance01 = MinDistance + (distance) * (MaxDistance - MinDistance) / (1000);
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.material.SetFloat("_DitherThreshold", distance01);
            renderer.material.SetFloat("_DitherTexelSize", DitherSize);
        }
    }
}
