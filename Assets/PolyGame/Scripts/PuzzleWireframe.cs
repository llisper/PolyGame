using UnityEngine;
using System.Collections.Generic;

public class PuzzleWireframe : MonoBehaviour
{
    Mesh mesh;
    Color[] colors;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        colors = mesh.colors;
    }

    void OnDestroy()
    {
        for (int i = 0; i < colors.Length; ++i)
            colors[i] = Config.wireframeColor;
        mesh.colors = colors;   
    }

    public void SetColor(Color color, List<Edge> borderEdges)
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
                    Debug.LogError("Invalid color index: " + index);
            }
        }
        mesh.colors = colors;
    }
}