using UnityEngine;
using System;
using System.Collections.Generic;

#region data structures
[Serializable]
public class Edge
{
    public Vector2 v0;
    public Vector2 v1;
    public List<int> wireframeTriangles;

    long v0h;
    long v1h;

    public Edge(Vector2 v0, long v0h, Vector2 v1, long v1h)
    {
        this.v0 = v0;
        this.v0h = v0h;
        this.v1 = v1;
        this.v1h = v1h;
    }

    public bool EqualTo(Edge other)
    {
        return EqualTo(other.v0h, other.v1h);
    }

    public bool EqualTo(long v0h, long v1h)
    {
        return (this.v0h == v0h && this.v1h == v1h) ||
               (this.v0h == v1h && this.v1h == v0h);
    }
}

[Serializable]
public class Triangle
{
    public int region;
    public Vector2[] vertices = new Vector2[3];
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
    public Texture2D background;
    public List<Triangle> triangles = new List<Triangle>();
    public List<Region> regions = new List<Region>();

    const float coordinatePrecision = 10f;

    public static long PointHash(Vector2 p, Vector2Int size)
    {
        // NOTE: coordinate floating point precision up to 0.1
        double c = coordinatePrecision;
        long x = (long)(p.x * c);
        long y = (long)(p.y * c);
        long w = (long)(size.x * c);
        return y * w + x;
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

    public static Vector3 GetCentroid(params Vector3[] points)
    {
        Vector3 ret = Vector2.zero;
        for (int i = 0; i < points.Length; ++i)
            ret += points[i];
        return ret / points.Length;
    }
}
