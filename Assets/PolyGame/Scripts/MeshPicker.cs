using UnityEngine;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MeshPicker : MonoBehaviour
{
    [NonSerialized]
    public Material selectedMat;
    [NonSerialized]
    public List<MeshRenderer> renderers = new List<MeshRenderer>();

    Material sharedMat;

    void Awake()
    {
        sharedMat = GetComponentInChildren<MeshRenderer>().sharedMaterial;
    }

    void OnDestroy()
    {
        foreach (var r in renderers)
            r.sharedMaterial = sharedMat;
    }

    public void Toggle(MeshRenderer renderer)
    {
        int i = renderers.IndexOf(renderer);
        if (i >= 0)
        {
            renderers[i].sharedMaterial = sharedMat;
            renderers.RemoveAt(i);
        }
        else
        {
            renderers.Add(renderer);
            renderer.sharedMaterial = selectedMat;
        }
    }
}
