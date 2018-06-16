using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Triangle
{
    public int region;
    public Vector2[] vertices;
    public long[] hashes;
    public List<int> adjacents = new List<int>();

    public Vector2 Centroid { get { return PolyGraph.GetCentroid(vertices); } }
}

[Serializable]
public class Region
{
    public List<int> triangles = new List<int>();
    public List<int> adjacents = new List<int>();
}

public class PolyGraph
{
    public string name;
    public Vector2Int size;
    public List<Vector2> points = new List<Vector2>();
    public List<Vector2[]> triangles = new List<Vector2[]>();

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

    public void AddTriangle(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        triangles.Add(new Vector2[] { p0, p1, p2 });
    }

    public static Vector2 GetCentroid(Vector2[] points)
    {
        Vector2 ret = Vector2.zero;
        for (int i = 0; i < points.Length; ++i)
            ret += points[i];
        return ret / points.Length;
    }
}
