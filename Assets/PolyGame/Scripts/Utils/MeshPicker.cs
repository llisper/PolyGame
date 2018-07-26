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

    PolyGraph graph;
    Material sharedMat;
    Region hoveringRegion;

    void Awake()
    {
        graph = GetComponent<PolyGraph>();
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

    public void Clear()
    {
        foreach (var r in renderers)
            r.sharedMaterial = sharedMat;
        renderers.Clear();
    }

    public void SetHoveringRegion(string name)
    {
        hoveringRegion = null;
        if (null != name && null != graph)
            hoveringRegion = graph.regions.Find(v => v.name == name);
    }

    void OnDrawGizmosSelected()
    {
        if (null != hoveringRegion)
        {
            foreach (int adj in hoveringRegion.adjacents)
                DrawRegionBorder(graph.regions[adj], Color.magenta);
            DrawRegionBorder(hoveringRegion, Color.green);
        }
    }

    void DrawRegionBorder(Region region, Color color)
    {
        Gizmos.color = color;
        foreach (var edge in region.borderEdges)
        {
            Gizmos.DrawLine(
                new Vector3(edge.v0.x, edge.v0.y, 0f),
                new Vector3(edge.v1.x, edge.v1.y, 0f));
        }
    }
}
