using UnityEngine;
using System.Collections.Generic;

public class PolyGraph
{
    public string name;
    public Vector2Int size;
    public List<Vector2> points = new List<Vector2>();
    public List<Vector2[]> triangles = new List<Vector2[]>();

    public Vector3[] vertices
    {
        get
        {
            return points.ConvertAll(v => (Vector3)v).ToArray();
        }
    }

    public int[] triangleIndexs
    {
        get
        {
            int[] ret = new int[triangles.Count * 3];
            for (int i = 0; i < triangles.Count; ++i)
            {
                ret[i * 3] = points.IndexOf(triangles[i][0]);
                ret[i * 3 + 1] = points.IndexOf(triangles[i][1]);
                ret[i * 3 + 2] = points.IndexOf(triangles[i][2]);
            }
            return ret;
        }
    }

    public Vector2[] uv
    {
        get
        {
            Vector2[] ret = new Vector2[points.Count];
            for (int i = 0; i < points.Count; ++i)
            {
                ret[i].x = points[i].x / size.x;
                ret[i].y = points[i].y / size.y;
            }
            return ret;
        }
    }

    public long PointHash(Vector2 p)
    {
        // NOTE: points are output by ImageTriangulator, and these coordinates are actually integers
        // each coordinate represent a pixel coordinate i think.
        return (long)p.y * size.x + (long)p.x;
    }

    public void AddTriangle(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        // p0, p1, p2, centroid
        triangles.Add(new Vector2[4] { p0, p1, p2, new Vector2((p0.x + p1.x + p2.x) / 3f, (p0.y + p1.y + p2.y) / 3f) });
    }
}
