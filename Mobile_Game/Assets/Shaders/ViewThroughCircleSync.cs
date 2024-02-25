using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewThroughCircleSync : MonoBehaviour
{
    public static int PosID = Shader.PropertyToID("_Position");
    public static int SizeID = Shader.PropertyToID("_CircleSize");

    public Material WallMaterial;
    public Camera Camera;
    public LayerMask Mask;

    // Update is called once per frame
    void Update()
    {
        var dir = Camera.transform.position - transform.position;
        var ray = new Ray(transform.position, dir.normalized);

        if (Physics.Raycast(ray, 30000, Mask))
            WallMaterial.SetFloat(SizeID, 1.5f);
        else
            WallMaterial.SetFloat(SizeID, 0);

        var view = Camera.WorldToViewportPoint(transform.position);
        WallMaterial.SetVector(PosID, view);
    }
}
