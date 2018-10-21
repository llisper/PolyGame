using UnityEngine;
using System.Collections.Generic;

public class PuzzleWireframe : MonoBehaviour
{
    Mesh mesh;
    Color[] colors;
    Color wireframeColor;

    void Awake()
    {
        Renderer = GetComponent<MeshRenderer>();
        mesh = GetComponent<MeshFilter>().sharedMesh;
        colors = mesh.colors;
        wireframeColor = Utils.ColorFromString(Config.Instance.wireframe.color);
        ResetColors();
    }

    void OnDestroy()
    {
        for (int i = 0; i < colors.Length; ++i)
            colors[i] = Color.black;
        mesh.colors = colors;   
    }

    public Renderer Renderer { get; private set; }

    public void SetColor(Color color, List<Edge> borderEdges)
    {
        InternalSetColor(color, borderEdges);
        mesh.colors = colors;
    }

    public void ResetColors()
    {
        for (int i = 0; i < colors.Length; ++i)
            colors[i] = wireframeColor;
        mesh.colors = colors;   
    }

    const int showRegions = 14;

    public void SetColor(Color color, PolyGraph graph, Region region, bool thisRegionOnly = false)
    {
        if (!thisRegionOnly)
        {
            var queue = new Queue<Region>();
            queue.Enqueue(region);
            var visited = new HashSet<Region>();
            visited.Add(region);
            for (int i = 0; i < showRegions && queue.Count > 0; ++i)
            {
                var r = queue.Dequeue();
                InternalSetColor(color, r.borderEdges);
                for (int index = 0; index < r.adjacents.Count; ++index)
                {
                    var adj = graph.regions[r.adjacents[index]];
                    if (!visited.Contains(adj))
                    {
                        queue.Enqueue(adj);
                        visited.Add(adj);
                    }
                }
            }
        }
        else
        {
            InternalSetColor(color, region.borderEdges);
        }
        mesh.colors = colors;
    }

    void InternalSetColor(Color color, List<Edge> borderEdges)
    {
        for (int i = 0; i < borderEdges.Count; ++i)
        {
            var edge = borderEdges[i];
            for (int j = 0; j < edge.wireframeTriangles.Count; ++j)
            {
                int index = edge.wireframeTriangles[j];
                if (index >= 0 && index < colors.Length)
                    colors[index] = color;
                else
                    GameLog.LogError("Invalid color index: " + index);
            }
        }
    }
}