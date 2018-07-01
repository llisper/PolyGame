using UnityEngine;
using System;
using System.Collections.Generic;

#region data structures
[Serializable]
public class Edge
{
    public Vector2Int v0;
    public Vector2Int v1;
    public List<int> wireframeTriangles = new List<int>();

    public Edge(Vector2Int v0, Vector2Int v1)
    {
        this.v0 = v0;
        this.v1 = v1;
    }

    public bool EqualTo(Vector2Int v0, Vector2Int v1)
    {
        return (this.v0 == v0 && this.v1 == v1) ||
               (this.v0 == v1 && this.v1 == v0);
    }
}

[Serializable]
public class Triangle
{
    public int region;
    public Vector2Int[] vertices = new Vector2Int[3];
    public long[] hashes = new long[3];
    public List<int> adjacents = new List<int>();
}

[Serializable]
public class Region
{
    public string name;
    public List<int> triangles = new List<int>();
    public List<int> adjacents = new List<int>();
    public List<Edge> borderEdges;
}
#endregion data structures

public class PolyGraph : MonoBehaviour
{
    public Vector2Int size;
    public List<Triangle> triangles = new List<Triangle>();
    public List<Region> regions = new List<Region>();

    public void Build()
    {

    }

    public static long PointHash(Vector2 p, Vector2Int size)
    {
        // NOTE: points are output by ImageTriangulator, and these coordinates are actually integers
        // each coordinate represent a pixel coordinate i think.
        return (long)p.y * size.x + (long)p.x;
    }

    public long PointHash(Vector2 p)
    {
        return PointHash(p, size);
    }

    public static Vector2 GetCentroid(params Vector2[] points)
    {
        Vector2 ret = Vector2.zero;
        for (int i = 0; i < points.Length; ++i)
            ret += points[i];
        return ret / points.Length;
    }
}
